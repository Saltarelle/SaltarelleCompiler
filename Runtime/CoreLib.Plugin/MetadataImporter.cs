using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Plugin {
	public class MetadataImporter : IMetadataImporter {
		private static readonly ReadOnlySet<string> _unusableStaticFieldNames = new ReadOnlySet<string>(new HashSet<string>(new[] { "__defineGetter__", "__defineSetter__", "apply", "arguments", "bind", "call", "caller", "constructor", "hasOwnProperty", "isPrototypeOf", "length", "name", "propertyIsEnumerable", "prototype", "toLocaleString", "valueOf" }.Concat(Saltarelle.Compiler.JSModel.Utils.AllKeywords)));
		private static readonly ReadOnlySet<string> _unusableInstanceFieldNames = new ReadOnlySet<string>(new HashSet<string>(new[] { "__defineGetter__", "__defineSetter__", "constructor", "hasOwnProperty", "isPrototypeOf", "propertyIsEnumerable", "toLocaleString", "valueOf" }.Concat(Saltarelle.Compiler.JSModel.Utils.AllKeywords)));

		private class TypeSemantics {
			public TypeScriptSemantics Semantics { get; private set; }
			public bool IsSerializable { get; private set; }
			public bool IsNamedValues { get; private set; }
			public bool IsImported { get; private set; }

			public TypeSemantics(TypeScriptSemantics semantics, bool isSerializable, bool isNamedValues, bool isImported) {
				Semantics           = semantics;
				IsSerializable      = isSerializable;
				IsNamedValues       = isNamedValues;
				IsImported          = isImported;
			}
		}

		private readonly Dictionary<ITypeDefinition, TypeSemantics> _typeSemantics;
		private readonly Dictionary<ITypeDefinition, DelegateScriptSemantics> _delegateSemantics;
		private readonly Dictionary<ITypeDefinition, HashSet<string>> _instanceMemberNamesByType;
		private readonly Dictionary<ITypeDefinition, HashSet<string>> _staticMemberNamesByType;
		private readonly Dictionary<IMethod, MethodScriptSemantics> _methodSemantics;
		private readonly Dictionary<IProperty, PropertyScriptSemantics> _propertySemantics;
		private readonly Dictionary<IField, FieldScriptSemantics> _fieldSemantics;
		private readonly Dictionary<IEvent, EventScriptSemantics> _eventSemantics;
		private readonly Dictionary<IMethod, ConstructorScriptSemantics> _constructorSemantics;
		private readonly Dictionary<IProperty, string> _propertyBackingFieldNames;
		private readonly Dictionary<IEvent, string> _eventBackingFieldNames;
		private readonly Dictionary<ITypeDefinition, int> _backingFieldCountPerType;
		private readonly Dictionary<Tuple<IAssembly, string>, int> _internalTypeCountPerAssemblyAndNamespace;
		private readonly HashSet<IMember> _ignoredMembers;
		private readonly IErrorReporter _errorReporter;
		private readonly IType _systemObject;
		private readonly ICompilation _compilation;

		private readonly bool _minimizeNames;

		public MetadataImporter(IErrorReporter errorReporter, ICompilation compilation, CompilerOptions options) {
			_errorReporter = errorReporter;
			_compilation = compilation;
			_minimizeNames = options.MinimizeScript;
			_systemObject = compilation.MainAssembly.Compilation.FindType(KnownTypeCode.Object);
			_typeSemantics = new Dictionary<ITypeDefinition, TypeSemantics>();
			_delegateSemantics = new Dictionary<ITypeDefinition, DelegateScriptSemantics>();
			_instanceMemberNamesByType = new Dictionary<ITypeDefinition, HashSet<string>>();
			_staticMemberNamesByType = new Dictionary<ITypeDefinition, HashSet<string>>();
			_methodSemantics = new Dictionary<IMethod, MethodScriptSemantics>();
			_propertySemantics = new Dictionary<IProperty, PropertyScriptSemantics>();
			_fieldSemantics = new Dictionary<IField, FieldScriptSemantics>();
			_eventSemantics = new Dictionary<IEvent, EventScriptSemantics>();
			_constructorSemantics = new Dictionary<IMethod, ConstructorScriptSemantics>();
			_propertyBackingFieldNames = new Dictionary<IProperty, string>();
			_eventBackingFieldNames = new Dictionary<IEvent, string>();
			_backingFieldCountPerType = new Dictionary<ITypeDefinition, int>();
			_internalTypeCountPerAssemblyAndNamespace = new Dictionary<Tuple<IAssembly, string>, int>();
			_ignoredMembers = new HashSet<IMember>();

			var sna = compilation.MainAssembly.AssemblyAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(ScriptNamespaceAttribute).FullName);
			if (sna != null) {
				var data = AttributeReader.ReadAttribute<ScriptNamespaceAttribute>(sna);
				if (data.Name == null || (data.Name != "" && !data.Name.IsValidNestedJavaScriptIdentifier())) {
					Message(Messages._7002, sna.Region, "assembly");
				}
			}
		}

		private void Message(Tuple<int, MessageSeverity, string> message, DomRegion r, params object[] additionalArgs) {
			_errorReporter.Region = r;
			_errorReporter.Message(message, additionalArgs);
		}

		private void Message(Tuple<int, MessageSeverity, string> message, IEntity e, params object[] additionalArgs) {
			var name = (e is IMethod && ((IMethod)e).IsConstructor ? e.DeclaringType.FullName : e.FullName);
			_errorReporter.Region = e.Region;
			_errorReporter.Message(message, new object[] { name }.Concat(additionalArgs).ToArray());
		}

		private string GetDefaultTypeName(ITypeDefinition def, bool ignoreGenericArguments) {
			if (ignoreGenericArguments) {
				return def.Name;
			}
			else {
				int outerCount = (def.DeclaringTypeDefinition != null ? def.DeclaringTypeDefinition.TypeParameters.Count : 0);
				return def.Name + (def.TypeParameterCount != outerCount ? "$" + (def.TypeParameterCount - outerCount).ToString(CultureInfo.InvariantCulture) : "");
			}
		}

		private bool? IsAutoProperty(IProperty property) {
			if (property.Region == default(DomRegion))
				return null;
			return property.Getter != null && property.Setter != null && property.Getter.BodyRegion == default(DomRegion) && property.Setter.BodyRegion == default(DomRegion);
		}

		private string DetermineNamespace(ITypeDefinition typeDefinition) {
			while (typeDefinition.DeclaringTypeDefinition != null) {
				typeDefinition = typeDefinition.DeclaringTypeDefinition;
			}

			var ina = AttributeReader.ReadAttribute<IgnoreNamespaceAttribute>(typeDefinition);
			var sna = AttributeReader.ReadAttribute<ScriptNamespaceAttribute>(typeDefinition);
			if (ina != null) {
				if (sna != null) {
					Message(Messages._7001, typeDefinition);
					return typeDefinition.FullName;
				}
				else {
					return "";
				}
			}
			else {
				if (sna != null) {
					if (sna.Name == null || (sna.Name != "" && !sna.Name.IsValidNestedJavaScriptIdentifier()))
						Message(Messages._7002, typeDefinition);
					return sna.Name;
				}
				else {
					var asna = AttributeReader.ReadAttribute<ScriptNamespaceAttribute>(typeDefinition.ParentAssembly.AssemblyAttributes);
					if (asna != null) {
						if (asna.Name != null && (asna.Name == "" || asna.Name.IsValidNestedJavaScriptIdentifier()))
							return asna.Name;
					}

					return typeDefinition.Namespace;
				}
			}
		}

		private Tuple<string, string> SplitNamespacedName(string fullName) {
			string nmspace;
			string name;
			int dot = fullName.IndexOf('.');
			if (dot >= 0) {
				nmspace = fullName.Substring(0, dot);
				name = fullName.Substring(dot + 1 );
			}
			else {
				nmspace = "";
				name = fullName;
			}
			return Tuple.Create(nmspace, name);
		}

		private void ProcessDelegate(ITypeDefinition delegateDefinition) {
			bool bindThisToFirstParameter = AttributeReader.HasAttribute<BindThisToFirstParameterAttribute>(delegateDefinition);
			bool expandParams = AttributeReader.HasAttribute<ExpandParamsAttribute>(delegateDefinition);

			if (bindThisToFirstParameter && delegateDefinition.GetDelegateInvokeMethod().Parameters.Count == 0) {
				Message(Messages._7147, delegateDefinition, delegateDefinition.FullName);
				bindThisToFirstParameter = false;
			}
			if (expandParams && !delegateDefinition.GetDelegateInvokeMethod().Parameters.Any(p => p.IsParams)) {
				Message(Messages._7148, delegateDefinition, delegateDefinition.FullName);
				expandParams = false;
			}

			_delegateSemantics[delegateDefinition] = new DelegateScriptSemantics(expandParams: expandParams, bindThisToFirstParameter: bindThisToFirstParameter);
		}

		private void ProcessType(ITypeDefinition typeDefinition) {
			if (typeDefinition.Kind == TypeKind.Delegate) {
				ProcessDelegate(typeDefinition);
				return;
			}

			if (AttributeReader.HasAttribute<NonScriptableAttribute>(typeDefinition) || typeDefinition.DeclaringTypeDefinition != null && GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NotUsableFromScript(), false, false, false);
				return;
			}

			var scriptNameAttr = AttributeReader.ReadAttribute<ScriptNameAttribute>(typeDefinition);
			var importedAttr = AttributeReader.ReadAttribute<ImportedAttribute>(typeDefinition.Attributes);
			bool preserveName = importedAttr != null || AttributeReader.HasAttribute<PreserveNameAttribute>(typeDefinition);

			bool? includeGenericArguments = typeDefinition.TypeParameterCount > 0 ? MetadataUtils.ShouldGenericArgumentsBeIncluded(typeDefinition) : false;
			if (includeGenericArguments == null) {
				_errorReporter.Region = typeDefinition.Region;
				Message(Messages._7026, typeDefinition);
				includeGenericArguments = true;
			}

			if (AttributeReader.HasAttribute<ResourcesAttribute>(typeDefinition)) {
				if (!typeDefinition.IsStatic) {
					Message(Messages._7003, typeDefinition);
				}
				else if (typeDefinition.TypeParameterCount > 0) {
					Message(Messages._7004, typeDefinition);
				}
				else if (typeDefinition.Members.Any(m => !(m is IField && ((IField)m).IsConst))) {
					Message(Messages._7005, typeDefinition);
				}
			}

			string typeName, nmspace;
			if (scriptNameAttr != null && scriptNameAttr.Name != null && scriptNameAttr.Name.IsValidJavaScriptIdentifier()) {
				typeName = scriptNameAttr.Name;
				nmspace = DetermineNamespace(typeDefinition);
			}
			else if (scriptNameAttr != null && string.IsNullOrEmpty(scriptNameAttr.Name) && !string.IsNullOrEmpty(MetadataUtils.GetModuleName(typeDefinition))) {
				typeName = "";
				nmspace = "";
			}
			else {
				if (scriptNameAttr != null) {
					Message(Messages._7006, typeDefinition);
				}

				if (_minimizeNames && MetadataUtils.CanBeMinimized(typeDefinition) && !preserveName) {
					nmspace = DetermineNamespace(typeDefinition);
					var key = Tuple.Create(typeDefinition.ParentAssembly, nmspace);
					int index;
					_internalTypeCountPerAssemblyAndNamespace.TryGetValue(key, out index);
					_internalTypeCountPerAssemblyAndNamespace[key] = index + 1;
					typeName = "$" + index.ToString(CultureInfo.InvariantCulture);
				}
				else {
					typeName = GetDefaultTypeName(typeDefinition, !includeGenericArguments.Value);
					if (typeDefinition.DeclaringTypeDefinition != null) {
						if (AttributeReader.HasAttribute<IgnoreNamespaceAttribute>(typeDefinition) || AttributeReader.HasAttribute<ScriptNamespaceAttribute>(typeDefinition)) {
							Message(Messages._7007, typeDefinition);
						}

						var declaringName = SplitNamespacedName(GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Name);
						nmspace = declaringName.Item1;
						typeName = declaringName.Item2 + "$" + typeName;
					}
					else {
						nmspace = DetermineNamespace(typeDefinition);
					}

					if (MetadataUtils.CanBeMinimized(typeDefinition) && !preserveName && !typeName.StartsWith("$")) {
						typeName = "$" + typeName;
					}
				}
			}

			bool isSerializable = MetadataUtils.IsSerializable(typeDefinition);

			if (isSerializable) {
				var baseClass = typeDefinition.DirectBaseTypes.Single(c => c.Kind == TypeKind.Class).GetDefinition();
				if (!baseClass.Equals(_systemObject) && baseClass.FullName != "System.Record" && !GetTypeSemanticsInternal(baseClass).IsSerializable) {
					Message(Messages._7009, typeDefinition);
				}
				foreach (var i in typeDefinition.DirectBaseTypes.Where(b => b.Kind == TypeKind.Interface && !GetTypeSemanticsInternal(b.GetDefinition()).IsSerializable)) {
					Message(Messages._7010, typeDefinition, i.FullName);
				}
				if (typeDefinition.Events.Any(evt => !evt.IsStatic)) {
					Message(Messages._7011, typeDefinition);
				}
				foreach (var m in typeDefinition.Members.Where(m => m.IsVirtual)) {
					Message(Messages._7023, typeDefinition, m.Name);
				}
				foreach (var m in typeDefinition.Members.Where(m => m.IsOverride)) {
					Message(Messages._7024, typeDefinition, m.Name);
				}

				if (typeDefinition.Kind == TypeKind.Interface && typeDefinition.Methods.Any()) {
					Message(Messages._7155, typeDefinition);
				}
			}
			else {
				var globalMethodsAttr = AttributeReader.ReadAttribute<GlobalMethodsAttribute>(typeDefinition);
				var mixinAttr = AttributeReader.ReadAttribute<MixinAttribute>(typeDefinition);
				if (mixinAttr != null) {
					if (!typeDefinition.IsStatic) {
						Message(Messages._7012, typeDefinition);
					}
					else if (typeDefinition.Members.Any(m => !AttributeReader.HasAttribute<CompilerGeneratedAttribute>(m) && (!(m is IMethod) || ((IMethod)m).IsConstructor))) {
						Message(Messages._7013, typeDefinition);
					}
					else if (typeDefinition.TypeParameterCount > 0) {
						Message(Messages._7014, typeDefinition);
					}
					else if (string.IsNullOrEmpty(mixinAttr.Expression)) {
						Message(Messages._7025, typeDefinition);
					}
					else {
						var split = SplitNamespacedName(mixinAttr.Expression);
						nmspace   = split.Item1;
						typeName  = split.Item2;
					}
				}
				else if (globalMethodsAttr != null) {
					if (!typeDefinition.IsStatic) {
						Message(Messages._7015, typeDefinition);
					}
					else if (typeDefinition.TypeParameterCount > 0) {
						Message(Messages._7017, typeDefinition);
					}
					else {
						nmspace  = "";
						typeName = "";
					}
				}
			}

			if (importedAttr != null) {
				if (!string.IsNullOrEmpty(importedAttr.TypeCheckCode)) {
					if (importedAttr.ObeysTypeSystem) {
						Message(Messages._7158, typeDefinition);
					}
					ValidateInlineCode(MetadataUtils.CreateTypeCheckMethod(typeDefinition, _compilation), typeDefinition, importedAttr.TypeCheckCode, Messages._7157);
				}
				if (!string.IsNullOrEmpty(MetadataUtils.GetSerializableTypeCheckCode(typeDefinition))) {
					Message(Messages._7159, typeDefinition);
				}
			}

			_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NormalType(!string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName, ignoreGenericArguments: !includeGenericArguments.Value, generateCode: importedAttr == null), isSerializable: isSerializable, isNamedValues: MetadataUtils.IsNamedValues(typeDefinition), isImported: importedAttr != null);
		}

		private HashSet<string> GetInstanceMemberNames(ITypeDefinition typeDefinition) {
			HashSet<string> result;
			if (!_instanceMemberNamesByType.TryGetValue(typeDefinition, out result))
				throw new ArgumentException("Error getting instance member names: type " + typeDefinition.FullName + " has not yet been processed.");
			return result;
		}

		private Tuple<string, bool> DeterminePreferredMemberName(IMember member) {
			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(member);
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && !AttributeReader.HasAttribute<AlternateSignatureAttribute>(m) && !AttributeReader.HasAttribute<NonScriptableAttribute>(m) && !AttributeReader.HasAttribute<InlineCodeAttribute>(m)).ToList();
				if (otherMembers.Count != 1) {
					Message(Messages._7100, member);
					return Tuple.Create(member.Name, false);
				}
			}
			return MetadataUtils.DeterminePreferredMemberName(member, _minimizeNames);
		}

		private void ProcessTypeMembers(ITypeDefinition typeDefinition) {
			if (typeDefinition.Kind == TypeKind.Delegate)
				return;

			var baseMembersByType = typeDefinition.GetAllBaseTypeDefinitions().Where(x => x != typeDefinition).Select(t => new { Type = t, MemberNames = GetInstanceMemberNames(t) }).ToList();
			for (int i = 0; i < baseMembersByType.Count; i++) {
				var b = baseMembersByType[i];
				for (int j = i + 1; j < baseMembersByType.Count; j++) {
					var b2 = baseMembersByType[j];
					if (!b.Type.GetAllBaseTypeDefinitions().Contains(b2.Type) && !b2.Type.GetAllBaseTypeDefinitions().Contains(b.Type)) {
						foreach (var dup in b.MemberNames.Where(b2.MemberNames.Contains)) {
							Message(Messages._7018, typeDefinition, b.Type.FullName, b2.Type.FullName, dup);
						}
					}
				}
			}

			var instanceMembers = baseMembersByType.SelectMany(m => m.MemberNames).Distinct().ToDictionary(m => m, m => false);
			if (_instanceMemberNamesByType.ContainsKey(typeDefinition))
				_instanceMemberNamesByType[typeDefinition].ForEach(s => instanceMembers[s] = true);
			_unusableInstanceFieldNames.ForEach(n => instanceMembers[n] = false);

			var staticMembers = _unusableStaticFieldNames.ToDictionary(n => n, n => false);
			if (_staticMemberNamesByType.ContainsKey(typeDefinition))
				_staticMemberNamesByType[typeDefinition].ForEach(s => staticMembers[s] = true);

			var membersByName =   from m in typeDefinition.GetMembers(options: GetMemberOptions.IgnoreInheritedMembers)
			                     where !_ignoredMembers.Contains(m)
			                       let name = DeterminePreferredMemberName(m)
			                     group new { m, name } by name.Item1 into g
			                    select new { Name = g.Key, Members = g.Select(x => new { Member = x.m, NameSpecified = x.name.Item2 }).ToList() };

			bool isSerializable = GetTypeSemanticsInternal(typeDefinition).IsSerializable;
			foreach (var current in membersByName) {
				foreach (var m in current.Members.OrderByDescending(x => x.NameSpecified).ThenBy(x => x.Member, MemberOrderer.Instance)) {
					if (m.Member is IMethod) {
						var method = (IMethod)m.Member;

						if (method.IsConstructor) {
							ProcessConstructor(method, current.Name, m.NameSpecified, staticMembers);
						}
						else {
							ProcessMethod(method, current.Name, m.NameSpecified, m.Member.IsStatic || isSerializable ? staticMembers : instanceMembers);
						}
					}
					else if (m.Member is IProperty) {
						var p = (IProperty)m.Member;
						ProcessProperty(p, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
						var ps = GetPropertySemantics(p);
						if (p.CanGet)
							_methodSemantics[p.Getter] = ps.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods ? ps.GetMethod : MethodScriptSemantics.NotUsableFromScript();
						if (p.CanSet)
							_methodSemantics[p.Setter] = ps.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods ? ps.SetMethod : MethodScriptSemantics.NotUsableFromScript();
					}
					else if (m.Member is IField) {
						ProcessField((IField)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
					}
					else if (m.Member is IEvent) {
						var e = (IEvent)m.Member;
						ProcessEvent((IEvent)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
						var es = GetEventSemantics(e);
						_methodSemantics[e.AddAccessor]    = es.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods ? es.AddMethod    : MethodScriptSemantics.NotUsableFromScript();
						_methodSemantics[e.RemoveAccessor] = es.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods ? es.RemoveMethod : MethodScriptSemantics.NotUsableFromScript();
					}
				}
			}

			_instanceMemberNamesByType[typeDefinition] = new HashSet<string>(instanceMembers.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
			_staticMemberNamesByType[typeDefinition] = new HashSet<string>(staticMembers.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
		}

		private string GetUniqueName(string preferredName, Dictionary<string, bool> usedNames) {
			return MetadataUtils.GetUniqueName(preferredName, n => !usedNames.ContainsKey(n));
		}

		private bool ValidateInlineCode(IMethod method, IEntity errorEntity, string code, Tuple<int, MessageSeverity, string> errorTemplate) {
			var typeErrors = new List<string>();
			IList<string> errors;
			Func<string, JsExpression> resolveType = n => {
				var type = ReflectionHelper.ParseReflectionName(n).Resolve(_compilation);
				if (type.Kind == TypeKind.Unknown) {
					typeErrors.Add("Unknown type '" + n + "' specified in inline implementation");
				}
				return JsExpression.Null;
			};
			
			if (method.ReturnType.Kind == TypeKind.Void && !method.IsConstructor) {
				errors = InlineCodeMethodCompiler.ValidateStatementListLiteralCode(method, code, resolveType, t => JsExpression.Null);
			}
			else {
				errors = InlineCodeMethodCompiler.ValidateExpressionLiteralCode(method, code, resolveType, t => JsExpression.Null);
			}
			if (errors.Count > 0 || typeErrors.Count > 0) {
				Message(errorTemplate, errorEntity, string.Join(", ", errors.Concat(typeErrors)));
				return false;
			}
			return true;
		}

		private void ProcessConstructor(IMethod constructor, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (constructor.Parameters.Count == 1 && constructor.Parameters[0].Type.FullName == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			var source = (IMethod)MetadataUtils.UnwrapValueTypeConstructor(constructor);

			var nsa = AttributeReader.ReadAttribute<NonScriptableAttribute>(source);
			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(source);
			var epa = AttributeReader.ReadAttribute<ExpandParamsAttribute>(source);
			var ola = AttributeReader.ReadAttribute<ObjectLiteralAttribute>(source);

			if (nsa != null || GetTypeSemanticsInternal(source.DeclaringTypeDefinition).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			if (source.IsStatic) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();	// Whatever, it is not really used.
				return;
			}

			if (epa != null && !source.Parameters.Any(p => p.IsParams)) {
				Message(Messages._7102, constructor);
			}

			bool isSerializable    = GetTypeSemanticsInternal(source.DeclaringTypeDefinition).IsSerializable;
			bool isImported        = GetTypeSemanticsInternal(source.DeclaringTypeDefinition).IsImported;
			bool skipInInitializer = AttributeReader.HasAttribute<ScriptSkipAttribute>(constructor);

			var ica = AttributeReader.ReadAttribute<InlineCodeAttribute>(source);
			if (ica != null) {
				if (!ValidateInlineCode(source, source, ica.Code, Messages._7103)) {
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
					return;
				}
				if (ica.NonExpandedFormCode != null && !ValidateInlineCode(source, source, ica.NonExpandedFormCode, Messages._7103)) {
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
					return;
				}
				if (ica.NonExpandedFormCode != null && !constructor.Parameters.Any(p => p.IsParams)) {
					Message(Messages._7029, constructor.Region, "constructor", constructor.DeclaringType.FullName);
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
					return;
				}

				_constructorSemantics[constructor] = ConstructorScriptSemantics.InlineCode(ica.Code, skipInInitializer: skipInInitializer, nonExpandedFormLiteralCode: ica.NonExpandedFormCode);
				return;
			}
			else if (asa != null) {
				_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(generateCode: false, expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Named(preferredName, generateCode: false, expandParams: epa != null, skipInInitializer: skipInInitializer);
				return;
			}
			else if (ola != null || (isSerializable && GetTypeSemanticsInternal(source.DeclaringTypeDefinition).IsImported)) {
				if (isSerializable) {
					bool hasError = false;
					var members = source.DeclaringTypeDefinition.Members.Where(m => m.SymbolKind == SymbolKind.Property || m.SymbolKind == SymbolKind.Field).ToDictionary(m => m.Name.ToLowerInvariant());
					var parameterToMemberMap = new List<IMember>();
					foreach (var p in source.Parameters) {
						IMember member;
						if (p.IsOut || p.IsRef) {
							Message(Messages._7145, p.Region, p.Name);
							hasError = true;
						}
						else if (members.TryGetValue(p.Name.ToLowerInvariant(), out member)) {
							if (p.Type.GetAllBaseTypes().Any(b => b.Equals(member.ReturnType)) || (member.ReturnType.IsKnownType(KnownTypeCode.NullableOfT) && member.ReturnType.TypeArguments[0].Equals(p.Type))) {
								parameterToMemberMap.Add(member);
							}
							else {
								Message(Messages._7144, p.Region, p.Name, p.Type.FullName, member.ReturnType.FullName);
								hasError = true;
							}
						}
						else {
							Message(Messages._7143, p.Region, source.DeclaringTypeDefinition.FullName, p.Name);
							hasError = true;
						}
					}
					_constructorSemantics[constructor] = hasError ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Json(parameterToMemberMap, skipInInitializer: skipInInitializer || constructor.Parameters.Count == 0);
				}
				else {
					Message(Messages._7146, constructor.Region, source.DeclaringTypeDefinition.FullName);
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
				}
				return;
			}
			else if (source.Parameters.Count == 1 && source.Parameters[0].Type is ArrayType && ((ArrayType)source.Parameters[0].Type).ElementType.IsKnownType(KnownTypeCode.Object) && source.Parameters[0].IsParams && isImported) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.InlineCode("ss.mkdict({" + source.Parameters[0].Name + "})", skipInInitializer: skipInInitializer);
				return;
			}
			else if (nameSpecified) {
				if (isSerializable)
					_constructorSemantics[constructor] = ConstructorScriptSemantics.StaticMethod(preferredName, expandParams: epa != null, skipInInitializer: skipInInitializer);
				else
					_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Named(preferredName, expandParams: epa != null, skipInInitializer: skipInInitializer);
				usedNames[preferredName] = true;
				return;
			}
			else {
				if (!usedNames.ContainsKey("$ctor") && !(isSerializable && _minimizeNames && MetadataUtils.CanBeMinimized(source))) {	// The last part ensures that the first constructor of a serializable type can have its name minimized.
					_constructorSemantics[constructor] = isSerializable ? ConstructorScriptSemantics.StaticMethod("$ctor", expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Unnamed(expandParams: epa != null, skipInInitializer: skipInInitializer);
					usedNames["$ctor"] = true;
					return;
				}
				else {
					string name;
					if (_minimizeNames && MetadataUtils.CanBeMinimized(source)) {
						name = GetUniqueName(null, usedNames);
					}
					else {
						int i = 1;
						do {
							name = "$ctor" + MetadataUtils.EncodeNumber(i, false);
							i++;
						} while (usedNames.ContainsKey(name));
					}

					_constructorSemantics[constructor] = isSerializable ? ConstructorScriptSemantics.StaticMethod(name, expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Named(name, expandParams: epa != null, skipInInitializer: skipInInitializer);
					usedNames[name] = true;
					return;
				}
			}
		}

		private void ProcessProperty(IProperty property, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (GetTypeSemanticsInternal(property.DeclaringTypeDefinition).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || AttributeReader.HasAttribute<NonScriptableAttribute>(property)) {
				_propertySemantics[property] = PropertyScriptSemantics.NotUsableFromScript();
				return;
			}
			else if (preferredName == "") {
				Message(property.IsIndexer ? Messages._7104 : Messages._7105, property);
				_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.NormalMethod("get") : null, property.CanSet ? MethodScriptSemantics.NormalMethod("set") : null);
				return;
			}
			else if (GetTypeSemanticsInternal(property.DeclaringTypeDefinition).IsSerializable && !property.IsStatic) {
				var getica = property.Getter != null ? AttributeReader.ReadAttribute<InlineCodeAttribute>(property.Getter) : null;
				var setica = property.Setter != null ? AttributeReader.ReadAttribute<InlineCodeAttribute>(property.Setter) : null;
				if (property.Getter != null && property.Setter != null && (getica != null) != (setica != null)) {
					Message(Messages._7028, property);
				}
				else if (getica != null || setica != null) {
					bool hasError = false;
					if (property.Getter != null && !ValidateInlineCode(property.Getter, property.Getter, getica.Code, Messages._7130)) {
						hasError = true;
					}
					if (property.Setter != null && !ValidateInlineCode(property.Setter, property.Setter, setica.Code, Messages._7130)) {
						hasError = true;
					}

					if (!hasError) {
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(getica != null ? MethodScriptSemantics.InlineCode(getica.Code) : null, setica != null ? MethodScriptSemantics.InlineCode(setica.Code) : null);
						return;
					}
				}

				if (property.IsIndexer) {
					if (property.DeclaringType.Kind == TypeKind.Interface) {
						Message(Messages._7161, property.Region);
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.Getter != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null, property.Setter != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null);
					}
					else if (property.Parameters.Count == 1) {
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.Getter != null ? MethodScriptSemantics.NativeIndexer() : null, property.Setter != null ? MethodScriptSemantics.NativeIndexer() : null);
					}
					else {
						Message(Messages._7116, property.Region);
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.Getter != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null, property.Setter != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null);
					}
				}
				else {
					usedNames[preferredName] = true;
					_propertySemantics[property] = PropertyScriptSemantics.Field(preferredName);
				}
				return;
			}

			var saa = AttributeReader.ReadAttribute<ScriptAliasAttribute>(property);

			if (saa != null) {
				if (property.IsIndexer) {
					Message(Messages._7106, property.Region);
				}
				else if (!property.IsStatic) {
					Message(Messages._7107, property);
				}
				else {
					_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.InlineCode(saa.Alias) : null, property.CanSet ? MethodScriptSemantics.InlineCode(saa.Alias + " = {value}") : null);
					return;
				}
			}

			if (AttributeReader.HasAttribute<IntrinsicPropertyAttribute>(property)) {
				if (property.DeclaringType.Kind == TypeKind.Interface) {
					if (property.IsIndexer)
						Message(Messages._7108, property.Region);
					else
						Message(Messages._7109, property);
				}
				else if (property.IsOverride && GetPropertySemantics((IProperty)InheritanceHelper.GetBaseMember(property).MemberDefinition).Type != PropertyScriptSemantics.ImplType.NotUsableFromScript) {
					if (property.IsIndexer)
						Message(Messages._7110, property.Region);
					else
						Message(Messages._7111, property);
				}
				else if (property.IsOverridable) {
					if (property.IsIndexer)
						Message(Messages._7112, property.Region);
					else
						Message(Messages._7113, property);
				}
				else if (property.IsExplicitInterfaceImplementation || property.ImplementedInterfaceMembers.Any(m => GetPropertySemantics((IProperty)m.MemberDefinition).Type != PropertyScriptSemantics.ImplType.NotUsableFromScript)) {
					if (property.IsIndexer)
						Message(Messages._7114, property.Region);
					else
						Message(Messages._7115, property);
				}
				else if (property.IsIndexer) {
					if (property.Parameters.Count == 1) {
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.NativeIndexer() : null, property.CanSet ? MethodScriptSemantics.NativeIndexer() : null);
						return;
					}
					else {
						Message(Messages._7116, property.Region);
					}
				}
				else {
					usedNames[preferredName] = true;
					_propertySemantics[property] = PropertyScriptSemantics.Field(preferredName);
					return;
				}
			}

			if (property.IsExplicitInterfaceImplementation && property.ImplementedInterfaceMembers.Any(m => GetPropertySemantics((IProperty)m.MemberDefinition).Type == PropertyScriptSemantics.ImplType.NotUsableFromScript)) {
				// Inherit [NonScriptable] for explicit interface implementations.
				_propertySemantics[property] = PropertyScriptSemantics.NotUsableFromScript();
				return;
			}

			if (property.ImplementedInterfaceMembers.Count > 0) {
				var bases = property.ImplementedInterfaceMembers.Where(b => GetPropertySemantics((IProperty)b).Type != PropertyScriptSemantics.ImplType.NotUsableFromScript).ToList();
				var firstField = bases.FirstOrDefault(b => GetPropertySemantics((IProperty)b).Type == PropertyScriptSemantics.ImplType.Field);
				if (firstField != null) {
					var firstFieldSemantics = GetPropertySemantics((IProperty)firstField);
					if (property.IsOverride) {
						Message(Messages._7154, property, firstField.FullName);
					}
					else if (property.IsOverridable) {
						Message(Messages._7153, property, firstField.FullName);
					}

					if (IsAutoProperty(property) == false) {
						Message(Messages._7156, property, firstField.FullName);
					}

					_propertySemantics[property] = firstFieldSemantics;
					return;
				}
			}

			MethodScriptSemantics getter, setter;
			if (property.CanGet) {
				var getterName = DeterminePreferredMemberName(property.Getter);
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && property.DeclaringType.Kind != TypeKind.Interface && MetadataUtils.CanBeMinimized(property) ? null : (nameSpecified ? "get_" + preferredName : GetUniqueName("get_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Getter, getterName.Item1, getterName.Item2, usedNames);
				getter = GetMethodSemanticsInternal(property.Getter);
			}
			else {
				getter = null;
			}

			if (property.CanSet) {
				var setterName = DeterminePreferredMemberName(property.Setter);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && property.DeclaringType.Kind != TypeKind.Interface && MetadataUtils.CanBeMinimized(property) ? null : (nameSpecified ? "set_" + preferredName : GetUniqueName("set_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Setter, setterName.Item1, setterName.Item2, usedNames);
				setter = GetMethodSemanticsInternal(property.Setter);
			}
			else {
				setter = null;
			}

			_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(getter, setter);
		}

		private void ProcessMethod(IMethod method, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			var eaa = AttributeReader.ReadAttribute<EnumerateAsArrayAttribute>(method);
			var ssa = AttributeReader.ReadAttribute<ScriptSkipAttribute>(method);
			var saa = AttributeReader.ReadAttribute<ScriptAliasAttribute>(method);
			var ica = AttributeReader.ReadAttribute<InlineCodeAttribute>(method);
			var ifa = AttributeReader.ReadAttribute<InstanceMethodOnFirstArgumentAttribute>(method);
			var nsa = AttributeReader.ReadAttribute<NonScriptableAttribute>(method);
			var ioa = AttributeReader.ReadAttribute<IntrinsicOperatorAttribute>(method);
			var epa = AttributeReader.ReadAttribute<ExpandParamsAttribute>(method);
			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(method);

			bool? includeGenericArguments = method.TypeParameters.Count > 0 ? MetadataUtils.ShouldGenericArgumentsBeIncluded(method) : false;

			if (eaa != null && (method.Name != "GetEnumerator" || method.IsStatic || method.TypeParameters.Count > 0 || method.Parameters.Count > 0)) {
				Message(Messages._7151, method);
				eaa = null;
			}

			if (nsa != null || GetTypeSemanticsInternal(method.DeclaringTypeDefinition).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_methodSemantics[method] = MethodScriptSemantics.NotUsableFromScript();
				return;
			}
			if (ioa != null) {
				if (!method.IsOperator) {
					Message(Messages._7117, method);
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
				}
				if (method.Name == "op_Implicit" || method.Name == "op_Explicit") {
					Message(Messages._7118, method);
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
				}
				else {
					_methodSemantics[method] = MethodScriptSemantics.NativeOperator();
				}
				return;
			}
			else {
				var interfaceImplementations = method.ImplementedInterfaceMembers.Where(m => method.IsExplicitInterfaceImplementation || _methodSemantics[(IMethod)m.MemberDefinition].Type != MethodScriptSemantics.ImplType.NotUsableFromScript).ToList();

				if (ssa != null) {
					// [ScriptSkip] - Skip invocation of the method entirely.
					if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
						Message(Messages._7119, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverride && GetMethodSemanticsInternal((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(Messages._7120, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverridable) {
						Message(Messages._7121, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (interfaceImplementations.Count > 0) {
						Message(Messages._7122, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else {
						if (method.IsStatic) {
							if (method.Parameters.Count != 1) {
								Message(Messages._7123, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							_methodSemantics[method] = MethodScriptSemantics.InlineCode("{" + method.Parameters[0].Name + "}", enumerateAsArray: eaa != null);
							return;
						}
						else {
							if (method.Parameters.Count != 0)
								Message(Messages._7124, method);
							_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}", enumerateAsArray: eaa != null);
							return;
						}
					}
				}
				else if (saa != null) {
					if (method.IsStatic) {
						_methodSemantics[method] = MethodScriptSemantics.InlineCode(saa.Alias + "(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")");
						return;
					}
					else {
						Message(Messages._7125, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
				}
				else if (ica != null) {
					string code = ica.Code ?? "", nonVirtualCode = ica.NonVirtualCode, nonExpandedFormCode = ica.NonExpandedFormCode;

					if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
						Message(Messages._7126, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverride && GetMethodSemanticsInternal((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(Messages._7127, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverridable && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
						Message(Messages._7128, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else {
						if (!ValidateInlineCode(method, method, code, Messages._7130)) {
							code = nonVirtualCode = nonExpandedFormCode = "X";
						}
						if (!string.IsNullOrEmpty(ica.NonVirtualCode) && !ValidateInlineCode(method, method, ica.NonVirtualCode, Messages._7130)) {
							code = nonVirtualCode = nonExpandedFormCode = "X";
						}
						if (!string.IsNullOrEmpty(ica.NonExpandedFormCode)) {
							if (!method.Parameters.Any(p => p.IsParams)) {
								Message(Messages._7029, method.Region, "method", method.FullName);
								code = nonVirtualCode = nonExpandedFormCode = "X";
							}
							if (!ValidateInlineCode(method, method, ica.NonExpandedFormCode, Messages._7130)) {
								code = nonVirtualCode = nonExpandedFormCode = "X";
							}
						}
						_methodSemantics[method] = MethodScriptSemantics.InlineCode(code, enumerateAsArray: eaa != null, generatedMethodName: !string.IsNullOrEmpty(ica.GeneratedMethodName) ? ica.GeneratedMethodName : null, nonVirtualInvocationLiteralCode: nonVirtualCode, nonExpandedFormLiteralCode: nonExpandedFormCode);
						if (!string.IsNullOrEmpty(ica.GeneratedMethodName))
							usedNames[ica.GeneratedMethodName] = true;
						return;
					}
				}
				else if (ifa != null) {
					if (method.IsStatic) {
						if (epa != null && !method.Parameters.Any(p => p.IsParams)) {
							Message(Messages._7137, method);
						}
						if (method.Parameters.Count == 0) {
							Message(Messages._7149, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						else if (method.Parameters[0].IsParams) {
							Message(Messages._7150, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						else {
							var sb = new StringBuilder();
							sb.Append("{" + method.Parameters[0].Name + "}." + preferredName + "(");
							for (int i = 1; i < method.Parameters.Count; i++) {
								var p = method.Parameters[i];
								if (i > 1)
									sb.Append(", ");
								sb.Append((epa != null && p.IsParams ? "{*" : "{") + p.Name + "}");
							}
							sb.Append(")");
							_methodSemantics[method] = MethodScriptSemantics.InlineCode(sb.ToString());
							return;
						}
					}
					else {
						Message(Messages._7131, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
				}
				else {
					if (method.IsOverride && GetMethodSemanticsInternal((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						if (nameSpecified) {
							Message(Messages._7132, method);
						}
						if (AttributeReader.HasAttribute<IncludeGenericArgumentsAttribute>(method)) {
							Message(Messages._7133, method);
						}

						var semantics = GetMethodSemanticsInternal((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition);
						if (semantics.Type == MethodScriptSemantics.ImplType.InlineCode && semantics.GeneratedMethodName != null)
							semantics = MethodScriptSemantics.NormalMethod(semantics.GeneratedMethodName, ignoreGenericArguments: semantics.IgnoreGenericArguments, expandParams: semantics.ExpandParams);	// Methods derived from methods with [InlineCode(..., GeneratedMethodName = "Something")] are treated as normal methods.
						if (eaa != null)
							semantics = semantics.WithEnumerateAsArray();
						if (semantics.Type == MethodScriptSemantics.ImplType.NormalMethod) {
							var errorMethod = interfaceImplementations.FirstOrDefault(im => GetMethodSemanticsInternal((IMethod)im.MemberDefinition).Name != semantics.Name);
							if (errorMethod != null) {
								Message(Messages._7134, method, errorMethod.FullName);
							}
						}

						_methodSemantics[method] = semantics;
						return;
					}
					else if (interfaceImplementations.Count > 0) {
						if (nameSpecified) {
							Message(Messages._7135, method);
						}

						var candidateNames = interfaceImplementations
						                     .Select(im => GetMethodSemanticsInternal((IMethod)im.MemberDefinition))
						                     .Select(s => s.Type == MethodScriptSemantics.ImplType.NormalMethod ? s.Name : (s.Type == MethodScriptSemantics.ImplType.InlineCode ? s.GeneratedMethodName : null))
						                     .Where(name => name != null)
						                     .Distinct();

						if (candidateNames.Count() > 1) {
							Message(Messages._7136, method);
						}

						// If the method implements more than one interface member, prefer to take the implementation from one that is not unusable.
						var sem = interfaceImplementations.Select(im => GetMethodSemanticsInternal((IMethod)im.MemberDefinition)).FirstOrDefault() ?? MethodScriptSemantics.NotUsableFromScript();
						if (sem.Type == MethodScriptSemantics.ImplType.InlineCode && sem.GeneratedMethodName != null)
							sem = MethodScriptSemantics.NormalMethod(sem.GeneratedMethodName, ignoreGenericArguments: sem.IgnoreGenericArguments, expandParams: sem.ExpandParams);	// Methods implementing methods with [InlineCode(..., GeneratedMethodName = "Something")] are treated as normal methods.
						if (eaa != null)
							sem = sem.WithEnumerateAsArray();
						_methodSemantics[method] = sem;
						return;
					}
					else {
						if (includeGenericArguments == null) {
							_errorReporter.Region = method.Region;
							Message(Messages._7027, method);
							includeGenericArguments = true;
						}

						if (epa != null) {
							if (!method.Parameters.Any(p => p.IsParams)) {
								Message(Messages._7137, method);
							}
						}

						if (preferredName == "") {
							// Special case - Script# supports setting the name of a method to an empty string, which means that it simply removes the name (eg. "x.M(a)" becomes "x(a)"). We model this with literal code.
							if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
								Message(Messages._7138, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							else if (method.IsOverridable) {
								Message(Messages._7139, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							else {
								_methodSemantics[method] = MethodScriptSemantics.InlineCode((method.IsStatic ? "{$" + method.DeclaringType.FullName + "}" : "{this}") + "(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")", enumerateAsArray: eaa != null);
								return;
							}
						}
						else {
							string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
							if (asa == null)
								usedNames[name] = true;
							if (GetTypeSemanticsInternal(method.DeclaringTypeDefinition).IsSerializable && !method.IsStatic) {
								_methodSemantics[method] = MethodScriptSemantics.StaticMethodWithThisAsFirstArgument(name, generateCode: !AttributeReader.HasAttribute<AlternateSignatureAttribute>(method), ignoreGenericArguments: !includeGenericArguments.Value, expandParams: epa != null, enumerateAsArray: eaa != null);
							}
							else {
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(name, generateCode: !AttributeReader.HasAttribute<AlternateSignatureAttribute>(method), ignoreGenericArguments: !includeGenericArguments.Value, expandParams: epa != null, enumerateAsArray: eaa != null);
							}
						}
					}
				}
			}
		}

		private void ProcessEvent(IEvent evt, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (GetTypeSemanticsInternal(evt.DeclaringTypeDefinition).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || AttributeReader.HasAttribute<NonScriptableAttribute>(evt)) {
				_eventSemantics[evt] = EventScriptSemantics.NotUsableFromScript();
				return;
			}
			else if (preferredName == "") {
				Message(Messages._7141, evt);
				_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add"), MethodScriptSemantics.NormalMethod("remove"));
				return;
			}

			MethodScriptSemantics adder, remover;
			if (evt.CanAdd) {
				var getterName = DeterminePreferredMemberName(evt.AddAccessor);
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && evt.DeclaringType.Kind != TypeKind.Interface && MetadataUtils.CanBeMinimized(evt) ? null : (nameSpecified ? "add_" + preferredName : GetUniqueName("add_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.AddAccessor, getterName.Item1, getterName.Item2, usedNames);
				adder = GetMethodSemanticsInternal(evt.AddAccessor);
			}
			else {
				adder = null;
			}

			if (evt.CanRemove) {
				var setterName = DeterminePreferredMemberName(evt.RemoveAccessor);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && evt.DeclaringType.Kind != TypeKind.Interface && MetadataUtils.CanBeMinimized(evt) ? null : (nameSpecified ? "remove_" + preferredName : GetUniqueName("remove_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.RemoveAccessor, setterName.Item1, setterName.Item2, usedNames);
				remover = GetMethodSemanticsInternal(evt.RemoveAccessor);
			}
			else {
				remover = null;
			}

			_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(adder, remover);
		}

		private void ProcessField(IField field, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (GetTypeSemanticsInternal(field.DeclaringTypeDefinition).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || AttributeReader.HasAttribute<NonScriptableAttribute>(field)) {
				_fieldSemantics[field] = FieldScriptSemantics.NotUsableFromScript();
			}
			else if (preferredName == "") {
				Message(Messages._7142, field);
				_fieldSemantics[field] = FieldScriptSemantics.Field("X");
			}
			else {
				string name = (nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames));
				if (AttributeReader.HasAttribute<InlineConstantAttribute>(field)) {
					if (field.IsConst) {
						name = null;
					}
					else {
						Message(Messages._7152, field);
					}
				}
				else {
					usedNames[name] = true;
				}

				if (AttributeReader.HasAttribute<NoInlineAttribute>(field) && !field.IsConst) {
					Message(Messages._7160, field);
				}

				if (GetTypeSemanticsInternal(field.DeclaringTypeDefinition).IsNamedValues) {
					string value = preferredName;
					if (!nameSpecified) {	// This code handles the feature that it is possible to specify an invalid ScriptName for a member of a NamedValues enum, in which case that value has to be use as the constant value.
						var sna = AttributeReader.ReadAttribute<ScriptNameAttribute>(field);
						if (sna != null)
							value = sna.Name;
					}

					_fieldSemantics[field] = FieldScriptSemantics.StringConstant(value, name);
				}
				else if (field.DeclaringType.Kind == TypeKind.Enum && AttributeReader.HasAttribute<ImportedAttribute>(field.DeclaringTypeDefinition) && AttributeReader.HasAttribute<ScriptNameAttribute>(field.DeclaringTypeDefinition)) {
					// Fields of enums that are imported and have an explicit [ScriptName] are treated as normal fields.
					_fieldSemantics[field] = FieldScriptSemantics.Field(name);
				}
				else if (name == null || (field.IsConst && !AttributeReader.HasAttribute<NoInlineAttribute>(field) && (field.DeclaringType.Kind == TypeKind.Enum || _minimizeNames))) {
					object value = Saltarelle.Compiler.JSModel.Utils.ConvertToDoubleOrStringOrBoolean(field.ConstantValue);
					if (value is bool)
						_fieldSemantics[field] = FieldScriptSemantics.BooleanConstant((bool)value, name);
					else if (value is double)
						_fieldSemantics[field] = FieldScriptSemantics.NumericConstant((double)value, name);
					else if (value is string)
						_fieldSemantics[field] = FieldScriptSemantics.StringConstant((string)value, name);
					else
						_fieldSemantics[field] = FieldScriptSemantics.NullConstant(name);
				}
				else {
					_fieldSemantics[field] = FieldScriptSemantics.Field(name);
				}
			}
		}

		public void Prepare(ITypeDefinition type) {
			try {
				ProcessType(type);
				ProcessTypeMembers(type);
			}
			catch (Exception ex) {
				_errorReporter.Region = type.Region;
				_errorReporter.InternalError(ex, "Error importing type " + type.FullName);
			}
		}

		public void ReserveMemberName(ITypeDefinition type, string name, bool isStatic) {
			HashSet<string> names;
			if (!isStatic) {
				if (!_instanceMemberNamesByType.TryGetValue(type, out names))
					_instanceMemberNamesByType[type] = names = new HashSet<string>();
			}
			else {
				if (!_staticMemberNamesByType.TryGetValue(type, out names))
					_staticMemberNamesByType[type] = names = new HashSet<string>();
			}
			names.Add(name);
		}

		public bool IsMemberNameAvailable(ITypeDefinition type, string name, bool isStatic) {
			if (isStatic) {
				if (_unusableStaticFieldNames.Contains(name))
					return false;
				HashSet<string> names;
				if (!_staticMemberNamesByType.TryGetValue(type, out names))
					return true;
				return !names.Contains(name);
			}
			else {
				if (_unusableInstanceFieldNames.Contains(name))
					return false;
				if (type.DirectBaseTypes.Select(d => d.GetDefinition()).Any(t => !IsMemberNameAvailable(t, name, false)))
					return false;
				HashSet<string> names;
				if (!_instanceMemberNamesByType.TryGetValue(type, out names))
					return true;
				return !names.Contains(name);
			}
		}

		public void SetMethodSemantics(IMethod method, MethodScriptSemantics semantics) {
			_methodSemantics[method] = semantics;
			_ignoredMembers.Add(method);
		}

		public void SetConstructorSemantics(IMethod method, ConstructorScriptSemantics semantics) {
			_constructorSemantics[method] = semantics;
			_ignoredMembers.Add(method);
		}

		public void SetPropertySemantics(IProperty property, PropertyScriptSemantics semantics) {
			_propertySemantics[property] = semantics;
			_ignoredMembers.Add(property);
		}

		public void SetFieldSemantics(IField field, FieldScriptSemantics semantics) {
			_fieldSemantics[field] = semantics;
			_ignoredMembers.Add(field);
		}

		public void SetEventSemantics(IEvent evt,EventScriptSemantics semantics) {
			_eventSemantics[evt] = semantics;
			_ignoredMembers.Add(evt);
		}

		private TypeSemantics GetTypeSemanticsInternal(ITypeDefinition typeDefinition) {
			TypeSemantics ts;
			if (_typeSemantics.TryGetValue(typeDefinition, out ts))
				return ts;
			throw new ArgumentException(string.Format("Type semantics for type {0} were not correctly imported", typeDefinition.FullName));
		}

		public TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition) {
			if (typeDefinition.Kind == TypeKind.Delegate)
				return TypeScriptSemantics.NormalType("Function");
			else if (typeDefinition.Kind == TypeKind.Array)
				return TypeScriptSemantics.NormalType("Array");
			return GetTypeSemanticsInternal(typeDefinition).Semantics;
		}

		private MethodScriptSemantics GetMethodSemanticsInternal(IMethod method) {
			switch (method.DeclaringType.Kind) {
				case TypeKind.Delegate:
					return MethodScriptSemantics.NotUsableFromScript();
				default:
					MethodScriptSemantics result;
					if (!_methodSemantics.TryGetValue((IMethod)method.MemberDefinition, out result))
						throw new ArgumentException(string.Format("Semantics for method " + method + " were not imported"));
					return result;
			}
		}

		public MethodScriptSemantics GetMethodSemantics(IMethod method) {
			if (method.IsAccessor)
				throw new ArgumentException("GetMethodSemantics should not be called for the accessor " + method);
			return GetMethodSemanticsInternal(method);
		}

		public ConstructorScriptSemantics GetConstructorSemantics(IMethod method) {
			switch (method.DeclaringType.Kind) {
				case TypeKind.Anonymous:
					return ConstructorScriptSemantics.Json(new IMember[0]);
				case TypeKind.Delegate:
					return ConstructorScriptSemantics.NotUsableFromScript();
				default:
					ConstructorScriptSemantics result;
					if (!_constructorSemantics.TryGetValue((IMethod)method.MemberDefinition, out result))
						throw new ArgumentException(string.Format("Semantics for constructor " + method + " were not imported"));
					return result;
			}
		}

		public PropertyScriptSemantics GetPropertySemantics(IProperty property) {
			switch (property.DeclaringType.Kind) {
				case TypeKind.Anonymous:
					return PropertyScriptSemantics.Field(property.Name.Replace("<>", "$"));
				case TypeKind.Delegate:
					return PropertyScriptSemantics.NotUsableFromScript();
				default:
					PropertyScriptSemantics result;
					if (!_propertySemantics.TryGetValue((IProperty)property.MemberDefinition, out result))
						throw new ArgumentException(string.Format("Semantics for property " + property + " were not imported"));
					return result;
			}
		}

		public DelegateScriptSemantics GetDelegateSemantics(ITypeDefinition delegateType) {
			DelegateScriptSemantics result;
			if (!_delegateSemantics.TryGetValue(delegateType, out result))
				throw new ArgumentException(string.Format("Semantics for delegate " + delegateType + " were not imported"));
			return result;
		}

		private string GetBackingFieldName(ITypeDefinition declaringTypeDefinition, string memberName) {
			int inheritanceDepth = declaringTypeDefinition.GetAllBaseTypes().Count(b => b.Kind != TypeKind.Interface) - 1;

			if (_minimizeNames) {
				int count;
				_backingFieldCountPerType.TryGetValue(declaringTypeDefinition, out count);
				count++;
				_backingFieldCountPerType[declaringTypeDefinition] = count;
				return string.Format(CultureInfo.InvariantCulture, "${0}${1}", inheritanceDepth, count);
			}
			else {
				return string.Format(CultureInfo.InvariantCulture, "${0}${1}Field", inheritanceDepth, memberName);
			}
		}

		public string GetAutoPropertyBackingFieldName(IProperty property) {
			property = (IProperty)property.MemberDefinition;
			string result;
			if (_propertyBackingFieldNames.TryGetValue(property, out result))
				return result;
			result = GetBackingFieldName(property.DeclaringTypeDefinition, property.Name);
			_propertyBackingFieldNames[property] = result;
			return result;
		}

		public FieldScriptSemantics GetFieldSemantics(IField field) {
			switch (field.DeclaringType.Kind) {
				case TypeKind.Delegate:
					return FieldScriptSemantics.NotUsableFromScript();
				default:
					FieldScriptSemantics result;
					if (!_fieldSemantics.TryGetValue((IField)field.MemberDefinition, out result))
						throw new ArgumentException(string.Format("Semantics for field " + field + " were not imported"));
					return result;
			}
		}

		public EventScriptSemantics GetEventSemantics(IEvent evt) {
			switch (evt.DeclaringType.Kind) {
				case TypeKind.Delegate:
					return EventScriptSemantics.NotUsableFromScript();
				default:
					EventScriptSemantics result;
					if (!_eventSemantics.TryGetValue((IEvent)evt.MemberDefinition, out result))
						throw new ArgumentException(string.Format("Semantics for field " + evt + " were not imported"));
					return result;
			}
		}

		public string GetAutoEventBackingFieldName(IEvent evt) {
			evt = (IEvent)evt.MemberDefinition;
			string result;
			if (_eventBackingFieldNames.TryGetValue(evt, out result))
				return result;
			result = GetBackingFieldName(evt.DeclaringTypeDefinition, evt.Name);
			_eventBackingFieldNames[evt] = result;
			return result;
		}
	}
}
