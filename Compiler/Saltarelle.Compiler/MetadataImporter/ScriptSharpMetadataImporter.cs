using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	public class ScriptSharpMetadataImporter : IMetadataImporter, IScriptSharpMetadataImporter {
		private static readonly ReadOnlySet<string> _unusableStaticFieldNames = new ReadOnlySet<string>(new HashSet<string>() { "__defineGetter__", "__defineSetter__", "apply", "arguments", "bind", "call", "caller", "constructor", "hasOwnProperty", "isPrototypeOf", "length", "name", "propertyIsEnumerable", "prototype", "toLocaleString", "toString", "valueOf" });
		private static readonly ReadOnlySet<string> _unusableInstanceFieldNames = new ReadOnlySet<string>(new HashSet<string>() { "__defineGetter__", "__defineSetter__", "constructor", "hasOwnProperty", "isPrototypeOf", "propertyIsEnumerable", "toLocaleString", "toString", "valueOf" });

		/// <summary>
		/// Used to deterministically order members. It is assumed that all members belong to the same type.
		/// </summary>
		private class MemberOrderer : IComparer<IMember> {
			public static readonly MemberOrderer Instance = new MemberOrderer();

			private MemberOrderer() {
			}

			private int CompareMethods(IMethod x, IMethod y) {
				int result = string.CompareOrdinal(x.Name, y.Name);
				if (result != 0)
					return result;
				if (x.Parameters.Count > y.Parameters.Count)
					return 1;
				else if (x.Parameters.Count < y.Parameters.Count)
					return -1;

				var xparms = string.Join(",", x.Parameters.Select(p => p.Type.FullName));
				var yparms = string.Join(",", y.Parameters.Select(p => p.Type.FullName));

				var presult = string.CompareOrdinal(xparms, yparms);
				if (presult != 0)
					return presult;

				return string.CompareOrdinal(x.ReturnType.FullName, y.ReturnType.FullName);
			}

			public int Compare(IMember x, IMember y) {
				if (x is IMethod) {
					if (y is IMethod) {
						return CompareMethods((IMethod)x, (IMethod)y);
					}
					else
						return -1;
				}
				else if (y is IMethod) {
					return 1;
				}

				if (x is IProperty) {
					if (y is IProperty) {
						return string.CompareOrdinal(x.Name, y.Name);
					}
					else 
						return -1;
				}
				else if (y is IProperty) {
					return 1;
				}

				if (x is IField) {
					if (y is IField) {
						return string.CompareOrdinal(x.Name, y.Name);
					}
					else 
						return -1;
				}
				else if (y is IField) {
					return 1;
				}

				if (x is IEvent) {
					if (y is IEvent) {
						return string.CompareOrdinal(x.Name, y.Name);
					}
					else 
						return -1;
				}
				else if (y is IEvent) {
					return 1;
				}

				throw new ArgumentException("Invalid member type" + x.GetType().FullName);
			}
		}

		private class TypeSemantics {
			public TypeScriptSemantics Semantics { get; private set; }
			public bool PreserveMemberNames { get; private set; }
			public bool PreserveMemberCases { get; private set; }
			public bool IsSerializable { get; private set; }
			public bool IsNamedValues { get; private set; }
			public bool IsImported { get; private set; }
			public bool ObeysTypeSystem { get; private set; }
			public bool IsResources { get; private set; }
			public bool IsMixin { get; private set; }
			public string ModuleName { get; private set; }

			public TypeSemantics(TypeScriptSemantics semantics, bool preserveMemberNames, bool preserveMemberCases, bool isSerializable, bool isNamedValues, bool isImported, bool obeysTypeSystem, bool isResources, bool isMixin, string moduleName) {
				Semantics           = semantics;
				PreserveMemberNames = preserveMemberNames;
				PreserveMemberCases = preserveMemberCases;
				IsSerializable      = isSerializable;
				IsNamedValues       = isNamedValues;
				IsImported          = isImported;
				ObeysTypeSystem     = obeysTypeSystem;
				IsResources         = isResources;
				IsMixin             = isMixin;
				ModuleName          = moduleName;
			}
		}

		private Dictionary<ITypeDefinition, TypeSemantics> _typeSemantics;
		private Dictionary<ITypeDefinition, DelegateScriptSemantics> _delegateSemantics;
		private Dictionary<ITypeDefinition, HashSet<string>> _instanceMemberNamesByType;
		private Dictionary<IMethod, MethodScriptSemantics> _methodSemantics;
		private Dictionary<IProperty, PropertyScriptSemantics> _propertySemantics;
		private Dictionary<IField, FieldScriptSemantics> _fieldSemantics;
		private Dictionary<IEvent, EventScriptSemantics> _eventSemantics;
		private Dictionary<IMethod, ConstructorScriptSemantics> _constructorSemantics;
		private Dictionary<ITypeParameter, string> _typeParameterNames;
		private Dictionary<IProperty, string> _propertyBackingFieldNames;
		private Dictionary<IEvent, string> _eventBackingFieldNames;
		private Dictionary<ITypeDefinition, int> _backingFieldCountPerType;
		private Dictionary<Tuple<IAssembly, string>, int> _internalTypeCountPerAssemblyAndNamespace;
		private IErrorReporter _errorReporter;
		private IType _systemObject;
		private IType _systemRecord;
		private ICompilation _compilation;
		private bool _omitDowncasts;
		private bool _omitNullableChecks;
		private string _mainModuleName;
		private bool _isAsyncModule;

		private bool _minimizeNames;

		public ScriptSharpMetadataImporter(IErrorReporter errorReporter) {
			_errorReporter = errorReporter;
		}

		private void Message(int code, DomRegion r, params object[] additionalArgs) {
			_errorReporter.Region = r;
			_errorReporter.Message(code, additionalArgs);
		}

		private void Message(int code, ITypeDefinition t, params object[] additionalArgs) {
			_errorReporter.Region = t.Region;
			_errorReporter.Message(code, new object[] { t.FullName }.Concat(additionalArgs).ToArray());
		}

		private void Message(int code, IMember m, params object[] additionalArgs) {
			var name = (m is IMethod && ((IMethod)m).IsConstructor ? m.DeclaringType.Name : m.Name);
			_errorReporter.Region = m.Region;
			_errorReporter.Message(code, new object[] { m.DeclaringType.FullName + "." + name }.Concat(additionalArgs).ToArray());
		}

		private string MakeCamelCase(string s) {
			if (string.IsNullOrEmpty(s))
				return s;
			if (s.Equals("ID", StringComparison.Ordinal))
				return "id";

			bool hasNonUppercase = false;
			int numUppercaseChars = 0;
			for (int index = 0; index < s.Length; index++) {
				if (char.IsUpper(s, index)) {
					numUppercaseChars++;
				}
				else {
					hasNonUppercase = true;
					break;
				}
			}

			if ((!hasNonUppercase && s.Length != 1) || numUppercaseChars == 0)
				return s;
			else if (numUppercaseChars > 1)
				return s.Substring(0, numUppercaseChars - 1).ToLower(CultureInfo.InvariantCulture) + s.Substring(numUppercaseChars - 1);
			else if (s.Length == 1)
				return s.ToLower(CultureInfo.InvariantCulture);
			else
				return char.ToLower(s[0], CultureInfo.InvariantCulture) + s.Substring(1);
		}

		private static readonly string _encodeNumberTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string EncodeNumber(int i, bool ensureValidIdentifier) {
			if (ensureValidIdentifier) {
				string result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1);
				while (i >= _encodeNumberTable.Length - 10) {
					i /= _encodeNumberTable.Length - 10;
					result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1) + result;
				}
				return JSModel.Utils.IsJavaScriptReservedWord(result) ? "_" + result : result;
			}
			else {
				string result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1);
				while (i >= _encodeNumberTable.Length) {
					i /= _encodeNumberTable.Length;
					result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1) + result;
				}
				return result;
			}
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

		private string DetermineNamespace(ITypeDefinition typeDefinition) {
			while (typeDefinition.DeclaringTypeDefinition != null) {
				typeDefinition = typeDefinition.DeclaringTypeDefinition;
			}

			var ina = AttributeReader.ReadAttribute<IgnoreNamespaceAttribute>(typeDefinition);
			var sna = AttributeReader.ReadAttribute<ScriptNamespaceAttribute>(typeDefinition);
			if (ina != null) {
				if (sna != null) {
					Message(7001, typeDefinition);
					return typeDefinition.FullName;
				}
				else {
					return "";
				}
			}
			else {
				if (sna != null) {
					if (sna.Name == null || (sna.Name != "" && !sna.Name.IsValidNestedJavaScriptIdentifier()))
						Message(7002, typeDefinition);
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
				Message(7147, delegateDefinition, delegateDefinition.FullName);
				bindThisToFirstParameter = false;
			}
			if (expandParams && !delegateDefinition.GetDelegateInvokeMethod().Parameters.Any(p => p.IsParams)) {
				Message(7148, delegateDefinition, delegateDefinition.FullName);
				expandParams = false;
			}

			_delegateSemantics[delegateDefinition] = new DelegateScriptSemantics(expandParams: expandParams, bindThisToFirstParameter: bindThisToFirstParameter);
		}

		private void ProcessType(ITypeDefinition typeDefinition) {
			if (typeDefinition.Kind == TypeKind.Delegate) {
				ProcessDelegate(typeDefinition);
				return;
			}

			if (_typeSemantics.ContainsKey(typeDefinition))
				return;
			foreach (var b in typeDefinition.DirectBaseTypes)
				ProcessType(b.GetDefinition());
			if (typeDefinition.DeclaringType != null)
				ProcessType(typeDefinition.DeclaringTypeDefinition);

			if (AttributeReader.HasAttribute<NonScriptableAttribute>(typeDefinition) || typeDefinition.DeclaringTypeDefinition != null && GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NotUsableFromScript(), false, false, false, false, false, true, false, false, null);
				return;
			}

			var scriptNameAttr = AttributeReader.ReadAttribute<ScriptNameAttribute>(typeDefinition);
			var importedAttr = AttributeReader.ReadAttribute<ImportedAttribute>(typeDefinition.Attributes);
			bool isImported = importedAttr != null;
			bool obeysTypeSystem = importedAttr == null || importedAttr.ObeysTypeSystem;
			bool preserveName = isImported || AttributeReader.HasAttribute<PreserveNameAttribute>(typeDefinition);

			bool ignoreGenericArguments = AttributeReader.HasAttribute<IgnoreGenericArgumentsAttribute>(typeDefinition) || isImported;
			bool isResources = false;

			if (AttributeReader.HasAttribute<ResourcesAttribute>(typeDefinition)) {
				if (!typeDefinition.IsStatic) {
					Message(7003, typeDefinition);
				}
				else if (typeDefinition.TypeParameterCount > 0) {
					Message(7004, typeDefinition);
				}
				else if (typeDefinition.Members.Any(m => !(m is IField && ((IField)m).IsConst))) {
					Message(7005, typeDefinition);
				}
				isResources = true;
			}

			string typeName, nmspace;
			if (scriptNameAttr != null && scriptNameAttr.Name != null && scriptNameAttr.Name.IsValidJavaScriptIdentifier()) {
				typeName = scriptNameAttr.Name;
				nmspace = DetermineNamespace(typeDefinition);
			}
			else {
				if (scriptNameAttr != null) {
					Message(7006, typeDefinition);
				}

				if (_minimizeNames && !Utils.IsPublic(typeDefinition) && !preserveName) {
					nmspace = DetermineNamespace(typeDefinition);
					var key = Tuple.Create(typeDefinition.ParentAssembly, nmspace);
					int index;
					_internalTypeCountPerAssemblyAndNamespace.TryGetValue(key, out index);
					_internalTypeCountPerAssemblyAndNamespace[key] = index + 1;
					typeName = "$" + index.ToString(CultureInfo.InvariantCulture);
				}
				else {
					typeName = GetDefaultTypeName(typeDefinition, ignoreGenericArguments);
					if (typeDefinition.DeclaringTypeDefinition != null) {
						if (AttributeReader.HasAttribute<IgnoreNamespaceAttribute>(typeDefinition) || AttributeReader.HasAttribute<ScriptNamespaceAttribute>(typeDefinition)) {
							Message(7007, typeDefinition);
						}

						var declaringName = SplitNamespacedName(GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Name);
						nmspace = declaringName.Item1;
						typeName = declaringName.Item2 + "$" + typeName;
					}
					else {
						nmspace = DetermineNamespace(typeDefinition);
					}

					if (!Utils.IsPublic(typeDefinition) && !preserveName && !typeName.StartsWith("$")) {
						typeName = "$" + typeName;
					}
				}
			}

			bool hasSerializableAttr = AttributeReader.HasAttribute<SerializableAttribute>(typeDefinition);
			bool inheritsRecord = typeDefinition.GetAllBaseTypeDefinitions().Any(td => td.Equals(_systemRecord)) && !typeDefinition.Equals(_systemRecord);
			
			bool isMixin = false, isSerializable = hasSerializableAttr || inheritsRecord;

			if (isSerializable) {
				var baseClass = typeDefinition.DirectBaseTypes.Single(c => c.Kind == TypeKind.Class).GetDefinition();
				if (!baseClass.Equals(_systemObject) && !baseClass.Equals(_systemRecord) && !GetTypeSemanticsInternal(baseClass).IsSerializable) {
					Message(7009, typeDefinition);
					isSerializable = false;
				}
				if (typeDefinition.DirectBaseTypes.Any(b => b.Kind == TypeKind.Interface)) {
					Message(7010, typeDefinition);
					isSerializable = false;
				}
				if (typeDefinition.Events.Any(evt => !evt.IsStatic)) {
					Message(7011, typeDefinition);
					isSerializable = false;
				}
				foreach (var m in typeDefinition.Members.Where(m => m.IsVirtual)) {
					Message(7023, typeDefinition, m.Name);
					isSerializable = false;
				}
				foreach (var m in typeDefinition.Members.Where(m => m.IsOverride)) {
					Message(7024, typeDefinition, m.Name);
					isSerializable = false;
				}
			}
			else {
				var baseClass = typeDefinition.DirectBaseTypes.SingleOrDefault(c => c.Kind == TypeKind.Class);
				if (baseClass != null && GetTypeSemanticsInternal(baseClass.GetDefinition()).IsSerializable) {
					Message(7008, typeDefinition, baseClass.FullName);
				}

				var globalMethodsAttr = AttributeReader.ReadAttribute<GlobalMethodsAttribute>(typeDefinition);
				var mixinAttr = AttributeReader.ReadAttribute<MixinAttribute>(typeDefinition);
				if (mixinAttr != null) {
					if (!typeDefinition.IsStatic) {
						Message(7012, typeDefinition);
					}
					else if (typeDefinition.Members.Any(m => !(m is IMethod) || ((IMethod)m).IsConstructor)) {
						Message(7013, typeDefinition);
					}
					else if (typeDefinition.TypeParameterCount > 0) {
						Message(7014, typeDefinition);
					}
					else if (string.IsNullOrEmpty(mixinAttr.Expression)) {
						Message(7025, typeDefinition);
					}
					else {
						var split = SplitNamespacedName(mixinAttr.Expression);
						nmspace   = split.Item1;
						typeName  = split.Item2;
						isMixin   = true;
					}
				}
				else if (globalMethodsAttr != null) {
					if (!typeDefinition.IsStatic) {
						Message(7015, typeDefinition);
					}
					else if (typeDefinition.TypeParameterCount > 0) {
						Message(7017, typeDefinition);
					}
					else {
						nmspace  = "";
						typeName = "";
					}
				}
			}

			for (int i = 0; i < typeDefinition.TypeParameterCount; i++) {
				var tp = typeDefinition.TypeParameters[i];
				_typeParameterNames[tp] = _minimizeNames ? EncodeNumber(i, true) : tp.Name;
			}

			var pmca = AttributeReader.ReadAttribute<PreserveMemberCaseAttribute>(typeDefinition) ?? AttributeReader.ReadAttribute<PreserveMemberCaseAttribute>(typeDefinition.ParentAssembly.AssemblyAttributes);

			bool preserveMemberCases = pmca != null && pmca.Preserve;
			bool preserveMemberNames = isImported || typeName == ""; // [Imported] and global methods

			var nva = AttributeReader.ReadAttribute<NamedValuesAttribute>(typeDefinition);
			var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(typeDefinition);
			_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NormalType(!string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName, ignoreGenericArguments: ignoreGenericArguments, generateCode: !isImported), preserveMemberNames: preserveMemberNames, preserveMemberCases: preserveMemberCases, isSerializable: isSerializable, isNamedValues: nva != null, isImported: isImported, obeysTypeSystem: obeysTypeSystem, isResources: isResources, isMixin: isMixin, moduleName: mna != null ? (mna.ModuleName ?? "") : null);
		}

		private HashSet<string> GetInstanceMemberNames(ITypeDefinition typeDefinition) {
			ProcessType(typeDefinition);
			HashSet<string> result;
			if (!_instanceMemberNamesByType.TryGetValue(typeDefinition, out result))
				ProcessTypeMembers(typeDefinition);
			return _instanceMemberNamesByType[typeDefinition];
		}

		private IMember UnwrapValueTypeConstructor(IMember m) {
			if (m is IMethod && !m.IsStatic && m.DeclaringType.Kind == TypeKind.Struct && ((IMethod)m).IsConstructor && ((IMethod)m).Parameters.Count == 0) {
				var other = m.DeclaringType.GetConstructors().SingleOrDefault(c => c.Parameters.Count == 1 && c.Parameters[0].Type.FullName == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName);
				if (other != null)
					return other;
			}
			return m;
		}

		private Tuple<string, bool> DeterminePreferredMemberName(IMember member) {
			member = UnwrapValueTypeConstructor(member);

			bool isConstructor = member is IMethod && ((IMethod)member).IsConstructor;
			bool isAccessor = member is IMethod && ((IMethod)member).IsAccessor;

			string defaultName;
			if (isConstructor) {
				defaultName = "$ctor";
			}
			else if (Utils.IsPublic(member)) {
				defaultName = GetTypeSemanticsInternal(member.DeclaringTypeDefinition).PreserveMemberCases ? member.Name : MakeCamelCase(member.Name);
			}
			else {
				if (_minimizeNames && member.DeclaringType.Kind != TypeKind.Interface)
					defaultName = null;
				else
					defaultName = "$" + (GetTypeSemanticsInternal(member.DeclaringTypeDefinition).PreserveMemberCases ? member.Name : MakeCamelCase(member.Name));
			}


			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(member);
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && !AttributeReader.HasAttribute<AlternateSignatureAttribute>(m) && !AttributeReader.HasAttribute<NonScriptableAttribute>(m) && !AttributeReader.HasAttribute<InlineCodeAttribute>(m)).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0]);
				}
				else {
					Message(7100, member);
					return Tuple.Create(member.Name, false);
				}
			}

			var typeSemantics = GetTypeSemanticsInternal(member.DeclaringTypeDefinition);

			var sna = AttributeReader.ReadAttribute<ScriptNameAttribute>(member);
			if (sna != null) {
				string name = sna.Name;
				if (typeSemantics.IsNamedValues && (name == "" || !name.IsValidJavaScriptIdentifier())) {
					return Tuple.Create(defaultName, false);	// For named values enum, allow the use to specify an empty or invalid value, which will only be used as the literal value for the field, not for the name.
				}
				if (name == "" && isConstructor)
					name = "$ctor";
				return Tuple.Create(name, true);
			}

			var ica = AttributeReader.ReadAttribute<InlineCodeAttribute>(member);
			if (ica != null) {
				if (ica.GeneratedMethodName != null)
					return Tuple.Create(ica.GeneratedMethodName, true);
			}


			if (AttributeReader.HasAttribute<PreserveCaseAttribute>(member))
				return Tuple.Create(member.Name, true);

			bool preserveName = (!isConstructor && !isAccessor && (   AttributeReader.HasAttribute<PreserveNameAttribute>(member)
			                                                       || AttributeReader.HasAttribute<InstanceMethodOnFirstArgumentAttribute>(member)
			                                                       || AttributeReader.HasAttribute<IntrinsicPropertyAttribute>(member)
			                                                       || typeSemantics.PreserveMemberNames && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (typeSemantics.IsSerializable && !member.IsStatic && (member is IProperty || member is IField)))
			                                                       || (typeSemantics.IsNamedValues && member is IField);

			if (preserveName)
				return Tuple.Create(typeSemantics.PreserveMemberCases ? member.Name : MakeCamelCase(member.Name), true);

			return Tuple.Create(defaultName, false);
		}

		public string GetQualifiedMemberName(IMember member) {
			return member.DeclaringType.FullName + "." + member.Name;
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
						foreach (var dup in b.MemberNames.Where(x => b2.MemberNames.Contains(x))) {
							Message(7018, typeDefinition, b.Type.FullName, b2.Type.FullName, dup);
						}
					}
				}
			}

			var instanceMembers = baseMembersByType.SelectMany(m => m.MemberNames).Distinct().ToDictionary(m => m, m => false);
			var staticMembers = _unusableStaticFieldNames.ToDictionary(n => n, n => false);
			_unusableInstanceFieldNames.ForEach(n => instanceMembers[n] = false);

			var membersByName =   from m in typeDefinition.GetMembers(options: GetMemberOptions.IgnoreInheritedMembers)
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

			_unusableInstanceFieldNames.ForEach(n => instanceMembers.Remove(n));
			_instanceMemberNamesByType[typeDefinition] = new HashSet<string>(instanceMembers.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
		}

		private string GetUniqueName(string preferredName, Dictionary<string, bool> usedNames) {
			// The name was not explicitly specified, so ensure that we have a unique name.
			string name = preferredName;
			int i = (name == null ? 0 : 1);
			while (name == null || usedNames.ContainsKey(name)) {
				name = preferredName + "$" + EncodeNumber(i, false);
				i++;
			}
			return name;
		}

		private void ProcessConstructor(IMethod constructor, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (constructor.Parameters.Count == 1 && constructor.Parameters[0].Type.FullName == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			var source = (IMethod)UnwrapValueTypeConstructor(constructor);

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
				Message(7102, constructor);
			}

			bool isSerializable = GetTypeSemanticsInternal(source.DeclaringTypeDefinition).IsSerializable;
			bool isImported     = GetTypeSemanticsInternal(source.DeclaringTypeDefinition).IsImported;

			var ica = AttributeReader.ReadAttribute<InlineCodeAttribute>(source);
			if (ica != null) {
				var errors = InlineCodeMethodCompiler.ValidateLiteralCode(source, ica.Code, t => t.Resolve(_compilation));
				if (errors.Count > 0) {
					Message(7103, source, string.Join(", ", errors));
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
					return;
				}

				_constructorSemantics[constructor] = ConstructorScriptSemantics.InlineCode(ica.Code);
				return;
			}
			else if (asa != null) {
				_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(generateCode: false, expandParams: epa != null) : ConstructorScriptSemantics.Named(preferredName, generateCode: false, expandParams: epa != null);
				return;
			}
			else if (ola != null || (isSerializable && GetTypeSemanticsInternal(source.DeclaringTypeDefinition).IsImported)) {
				if (isSerializable) {
					bool hasError = false;
					var members = source.DeclaringTypeDefinition.Members.Where(m => m.EntityType == EntityType.Property || m.EntityType == EntityType.Field).ToDictionary(m => m.Name.ToLowerInvariant());
					var parameterToMemberMap = new List<IMember>();
					foreach (var p in source.Parameters) {
						IMember member;
						if (p.IsOut || p.IsRef) {
							Message(7145, p.Region, p.Name);
							hasError = true;
						}
						else if (members.TryGetValue(p.Name.ToLowerInvariant(), out member)) {
							if (member.ReturnType.Equals(p.Type)) {
								parameterToMemberMap.Add(member);
							}
							else {
								Message(7144, p.Region, p.Name, p.Type.FullName, member.ReturnType.FullName);
								hasError = true;
							}
						}
						else {
							Message(7143, p.Region, source.DeclaringTypeDefinition.FullName, p.Name);
							hasError = true;
						}
					}
					_constructorSemantics[constructor] = hasError ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Json(parameterToMemberMap);
				}
				else {
					Message(7146, constructor.Region, source.DeclaringTypeDefinition.FullName);
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
				}
				return;
			}
			else if (source.Parameters.Count == 1 && source.Parameters[0].Type is ArrayType && ((ArrayType)source.Parameters[0].Type).ElementType.IsKnownType(KnownTypeCode.Object) && source.Parameters[0].IsParams && isImported) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.InlineCode("ss.mkdict({" + source.Parameters[0].Name + "})");
				return;
			}
			else if (nameSpecified) {
				if (isSerializable)
					_constructorSemantics[constructor] = ConstructorScriptSemantics.StaticMethod(preferredName, expandParams: epa != null);
				else
					_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(expandParams: epa != null) : ConstructorScriptSemantics.Named(preferredName, expandParams: epa != null);
				usedNames[preferredName] = true;
				return;
			}
			else {
				if (!usedNames.ContainsKey("$ctor") && !(isSerializable && _minimizeNames && !Utils.IsPublic(source))) {	// The last part ensures that the first constructor of a serializable type can have its name minimized.
					_constructorSemantics[constructor] = isSerializable ? ConstructorScriptSemantics.StaticMethod("$ctor", expandParams: epa != null) : ConstructorScriptSemantics.Unnamed(expandParams: epa != null);
					usedNames["$ctor"] = true;
					return;
				}
				else {
					string name;
					if (_minimizeNames && !Utils.IsPublic(source)) {
						name = GetUniqueName(null, usedNames);
					}
					else {
						int i = 1;
						do {
							name = "$ctor" + EncodeNumber(i, false);
							i++;
						} while (usedNames.ContainsKey(name));
					}

					_constructorSemantics[constructor] = isSerializable ? ConstructorScriptSemantics.StaticMethod(name, expandParams: epa != null) : ConstructorScriptSemantics.Named(name, expandParams: epa != null);
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
				if (property.IsIndexer) {
					Message(7104, property);
				}
				else {
					Message(7105, property);
				}
				_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.NormalMethod("get") : null, property.CanSet ? MethodScriptSemantics.NormalMethod("set") : null);
				return;
			}
			else if (GetTypeSemanticsInternal(property.DeclaringTypeDefinition).IsSerializable && !property.IsStatic) {
				usedNames[preferredName] = true;
				_propertySemantics[property] = PropertyScriptSemantics.Field(preferredName);
				return;
			}

			var saa = AttributeReader.ReadAttribute<ScriptAliasAttribute>(property);

			if (saa != null) {
				if (property.IsIndexer) {
					Message(7106, property.Region);
				}
				else if (!property.IsStatic) {
					Message(7107, property);
				}
				else {
					_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.InlineCode(saa.Alias) : null, property.CanSet ? MethodScriptSemantics.InlineCode(saa.Alias + " = {value}") : null);
					return;
				}
			}

			if (AttributeReader.HasAttribute<IntrinsicPropertyAttribute>(property)) {
				if (property.DeclaringType.Kind == TypeKind.Interface) {
					if (property.IsIndexer)
						Message(7108, property.Region);
					else
						Message(7109, property);
				}
				else if (property.IsOverride && GetPropertySemantics((IProperty)InheritanceHelper.GetBaseMember(property).MemberDefinition).Type != PropertyScriptSemantics.ImplType.NotUsableFromScript) {
					if (property.IsIndexer)
						Message(7110, property.Region);
					else
						Message(7111, property);
				}
				else if (property.IsOverridable) {
					if (property.IsIndexer)
						Message(7112, property.Region);
					else
						Message(7113, property);
				}
				else if (property.IsExplicitInterfaceImplementation || property.ImplementedInterfaceMembers.Any(m => GetPropertySemantics((IProperty)m.MemberDefinition).Type != PropertyScriptSemantics.ImplType.NotUsableFromScript)) {
					if (property.IsIndexer)
						Message(7114, property.Region);
					else
						Message(7115, property);
				}
				else if (property.IsIndexer) {
					if (property.Parameters.Count == 1) {
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.NativeIndexer() : null, property.CanSet ? MethodScriptSemantics.NativeIndexer() : null);
						return;
					}
					else {
						Message(7116, property.Region);
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

			MethodScriptSemantics getter, setter;
			if (property.CanGet) {
				var getterName = DeterminePreferredMemberName(property.Getter);
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && property.DeclaringType.Kind != TypeKind.Interface && !Utils.IsPublic(property) ? null : (nameSpecified ? "get_" + preferredName : GetUniqueName("get_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Getter, getterName.Item1, getterName.Item2, usedNames);
				getter = GetMethodSemantics(property.Getter);
			}
			else {
				getter = null;
			}

			if (property.CanSet) {
				var setterName = DeterminePreferredMemberName(property.Setter);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && property.DeclaringType.Kind != TypeKind.Interface && !Utils.IsPublic(property) ? null : (nameSpecified ? "set_" + preferredName : GetUniqueName("set_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Setter, setterName.Item1, setterName.Item2, usedNames);
				setter = GetMethodSemantics(property.Setter);
			}
			else {
				setter = null;
			}

			_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(getter, setter);
		}

		private void ProcessMethod(IMethod method, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			for (int i = 0; i < method.TypeParameters.Count; i++) {
				var tp = method.TypeParameters[i];
				_typeParameterNames[tp] = _minimizeNames ? EncodeNumber(method.DeclaringType.TypeParameterCount + i, true) : tp.Name;
			}

			var eaa = AttributeReader.ReadAttribute<EnumerateAsArrayAttribute>(method);
			var ssa = AttributeReader.ReadAttribute<ScriptSkipAttribute>(method);
			var saa = AttributeReader.ReadAttribute<ScriptAliasAttribute>(method);
			var ica = AttributeReader.ReadAttribute<InlineCodeAttribute>(method);
			var ifa = AttributeReader.ReadAttribute<InstanceMethodOnFirstArgumentAttribute>(method);
			var nsa = AttributeReader.ReadAttribute<NonScriptableAttribute>(method);
			var iga = AttributeReader.ReadAttribute<IgnoreGenericArgumentsAttribute>(method);
			var ioa = AttributeReader.ReadAttribute<IntrinsicOperatorAttribute>(method);
			var epa = AttributeReader.ReadAttribute<ExpandParamsAttribute>(method);
			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(method);

			bool isImported = GetTypeSemanticsInternal(method.DeclaringTypeDefinition).IsImported;

			if (eaa != null && (method.Name != "GetEnumerator" || method.IsStatic || method.TypeParameters.Count > 0 || method.Parameters.Count > 0)) {
				Message(7151, method);
				eaa = null;
			}

			if (nsa != null || GetTypeSemanticsInternal(method.DeclaringTypeDefinition).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_methodSemantics[method] = MethodScriptSemantics.NotUsableFromScript();
				return;
			}
			if (ioa != null) {
				if (!method.IsOperator) {
					Message(7117, method);
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
				}
				if (method.Name == "op_Implicit" || method.Name == "op_Explicit") {
					Message(7118, method);
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
						Message(7119, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverride && GetMethodSemantics((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(7120, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverridable) {
						Message(7121, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (interfaceImplementations.Count > 0) {
						Message(7122, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else {
						if (method.IsStatic) {
							if (method.Parameters.Count != 1) {
								Message(7123, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							_methodSemantics[method] = MethodScriptSemantics.InlineCode("{" + method.Parameters[0].Name + "}", enumerateAsArray: eaa != null);
							return;
						}
						else {
							if (method.Parameters.Count != 0)
								Message(7124, method);
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
						Message(7125, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
				}
				else if (ica != null) {
					string code = ica.Code ?? "", nonVirtualCode = ica.NonVirtualCode ?? ica.Code ?? "";

					if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
						Message(7126, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverride && GetMethodSemantics((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(7127, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverridable && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
						Message(7128, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else {
						var errors = InlineCodeMethodCompiler.ValidateLiteralCode(method, code, t => t.Resolve(_compilation));
						if (!string.IsNullOrEmpty(ica.NonVirtualCode))
							errors.AddRange(InlineCodeMethodCompiler.ValidateLiteralCode(method, ica.NonVirtualCode, t => t.Resolve(_compilation)));
						if (errors.Count > 0) {
							Message(7130, method, string.Join(", ", errors));
							code = nonVirtualCode = "X";
						}

						_methodSemantics[method] = MethodScriptSemantics.InlineCode(code, enumerateAsArray: eaa != null, generatedMethodName: !string.IsNullOrEmpty(ica.GeneratedMethodName) ? ica.GeneratedMethodName : null, nonVirtualInvocationLiteralCode: nonVirtualCode);
						if (!string.IsNullOrEmpty(ica.GeneratedMethodName))
							usedNames[ica.GeneratedMethodName] = true;
						return;
					}
				}
				else if (ifa != null) {
					if (method.IsStatic) {
						if (epa != null && !method.Parameters.Any(p => p.IsParams)) {
							Message(7137, method);
						}
						if (method.Parameters.Count == 0) {
							Message(7149, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						else if (method.Parameters[0].IsParams) {
							Message(7150, method);
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
						Message(7131, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
				}
				else {
					if (method.IsOverride && GetMethodSemantics((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						if (nameSpecified) {
							Message(7132, method);
						}
						if (iga != null) {
							Message(7133, method);
						}

						var semantics = GetMethodSemantics((IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition);
						if (semantics.Type == MethodScriptSemantics.ImplType.InlineCode && semantics.GeneratedMethodName != null)
							semantics = MethodScriptSemantics.NormalMethod(semantics.GeneratedMethodName, ignoreGenericArguments: semantics.IgnoreGenericArguments, expandParams: semantics.ExpandParams);	// Methods derived from methods with [InlineCode(..., GeneratedMethodName = "Something")] are treated as normal methods.
						if (eaa != null)
							semantics = semantics.WithEnumerateAsArray();
						if (semantics.Type == MethodScriptSemantics.ImplType.NormalMethod) {
							var errorMethod = method.ImplementedInterfaceMembers.FirstOrDefault(im => GetMethodSemantics((IMethod)im.MemberDefinition).Name != semantics.Name);
							if (errorMethod != null) {
								Message(7134, method, GetQualifiedMemberName(errorMethod));
							}
						}

						_methodSemantics[method] = semantics;
						return;
					}
					else if (interfaceImplementations.Count > 0) {
						if (nameSpecified) {
							Message(7135, method);
						}

						var candidateNames = method.ImplementedInterfaceMembers
						                           .Select(im => GetMethodSemantics((IMethod)im.MemberDefinition))
						                           .Select(s => s.Type == MethodScriptSemantics.ImplType.NormalMethod ? s.Name : (s.Type == MethodScriptSemantics.ImplType.InlineCode ? s.GeneratedMethodName : null))
						                           .Where(name => name != null)
						                           .Distinct();

						if (candidateNames.Count() > 1) {
							Message(7136, method);
						}

						// If the method implements more than one interface member, prefer to take the implementation from one that is not unusable.
						var sem = method.ImplementedInterfaceMembers.Select(im => GetMethodSemantics((IMethod)im.MemberDefinition)).FirstOrDefault(x => x.Type != MethodScriptSemantics.ImplType.NotUsableFromScript) ?? MethodScriptSemantics.NotUsableFromScript();
						if (sem.Type == MethodScriptSemantics.ImplType.InlineCode && sem.GeneratedMethodName != null)
							sem = MethodScriptSemantics.NormalMethod(sem.GeneratedMethodName, ignoreGenericArguments: sem.IgnoreGenericArguments, expandParams: sem.ExpandParams);	// Methods implementing methods with [InlineCode(..., GeneratedMethodName = "Something")] are treated as normal methods.
						if (eaa != null)
							sem = sem.WithEnumerateAsArray();
						_methodSemantics[method] = sem;
						return;
					}
					else {
						if (epa != null) {
							if (!method.Parameters.Any(p => p.IsParams)) {
								Message(7137, method);
							}
						}

						if (preferredName == "") {
							// Special case - Script# supports setting the name of a method to an empty string, which means that it simply removes the name (eg. "x.M(a)" becomes "x(a)"). We model this with literal code.
							if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
								Message(7138, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							else if (method.IsOverridable) {
								Message(7139, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							else if (method.IsStatic) {
								Message(7140, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
								return;
							}
							else {
								_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")", enumerateAsArray: eaa != null);
								return;
							}
						}
						else {
							string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
							if (asa == null)
								usedNames[name] = true;
							if (GetTypeSemanticsInternal(method.DeclaringTypeDefinition).IsSerializable && !method.IsStatic) {
								_methodSemantics[method] = MethodScriptSemantics.StaticMethodWithThisAsFirstArgument(name, generateCode: !AttributeReader.HasAttribute<AlternateSignatureAttribute>(method), ignoreGenericArguments: iga != null || isImported, expandParams: epa != null, enumerateAsArray: eaa != null);
							}
							else {
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(name, generateCode: !AttributeReader.HasAttribute<AlternateSignatureAttribute>(method), ignoreGenericArguments: iga != null || isImported, expandParams: epa != null, enumerateAsArray: eaa != null);
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
				Message(7141, evt);
				_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add"), MethodScriptSemantics.NormalMethod("remove"));
				return;
			}

			MethodScriptSemantics adder, remover;
			if (evt.CanAdd) {
				var getterName = DeterminePreferredMemberName(evt.AddAccessor);
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && evt.DeclaringType.Kind != TypeKind.Interface && !Utils.IsPublic(evt) ? null : (nameSpecified ? "add_" + preferredName : GetUniqueName("add_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.AddAccessor, getterName.Item1, getterName.Item2, usedNames);
				adder = GetMethodSemantics(evt.AddAccessor);
			}
			else {
				adder = null;
			}

			if (evt.CanRemove) {
				var setterName = DeterminePreferredMemberName(evt.RemoveAccessor);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && evt.DeclaringType.Kind != TypeKind.Interface && !Utils.IsPublic(evt) ? null : (nameSpecified ? "remove_" + preferredName : GetUniqueName("remove_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.RemoveAccessor, setterName.Item1, setterName.Item2, usedNames);
				remover = GetMethodSemantics(evt.RemoveAccessor);
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
				Message(7142, field);
				_fieldSemantics[field] = FieldScriptSemantics.Field("X");
			}
			else {
				string name = (nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames));
				if (AttributeReader.HasAttribute<InlineConstantAttribute>(field)) {
					if (field.IsConst) {
						name = null;
					}
					else {
						Message(7152, field);
					}
				}
				else {
					usedNames[name] = true;
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
				else if (name == null || (field.IsConst && (field.DeclaringType.Kind == TypeKind.Enum || _minimizeNames))) {
					object value = JSModel.Utils.ConvertToDoubleOrStringOrBoolean(field.ConstantValue);
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

		public void Prepare(IEnumerable<ITypeDefinition> types, bool minimizeNames, IAssembly mainAssembly) {
			_minimizeNames = minimizeNames;
			_systemObject = mainAssembly.Compilation.FindType(KnownTypeCode.Object);
			_systemRecord = ReflectionHelper.ParseReflectionName("System.Record").Resolve(mainAssembly.Compilation.TypeResolveContext);
			_compilation = mainAssembly.Compilation;
			_typeSemantics = new Dictionary<ITypeDefinition, TypeSemantics>();
			_delegateSemantics = new Dictionary<ITypeDefinition, DelegateScriptSemantics>();
			_instanceMemberNamesByType = new Dictionary<ITypeDefinition, HashSet<string>>();
			_methodSemantics = new Dictionary<IMethod, MethodScriptSemantics>();
			_propertySemantics = new Dictionary<IProperty, PropertyScriptSemantics>();
			_fieldSemantics = new Dictionary<IField, FieldScriptSemantics>();
			_eventSemantics = new Dictionary<IEvent, EventScriptSemantics>();
			_constructorSemantics = new Dictionary<IMethod, ConstructorScriptSemantics>();
			_typeParameterNames = new Dictionary<ITypeParameter, string>();
			_propertyBackingFieldNames = new Dictionary<IProperty, string>();
			_eventBackingFieldNames = new Dictionary<IEvent, string>();
			_backingFieldCountPerType = new Dictionary<ITypeDefinition, int>();
			_internalTypeCountPerAssemblyAndNamespace = new Dictionary<Tuple<IAssembly, string>, int>();

			var sna = mainAssembly.AssemblyAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(ScriptNamespaceAttribute).FullName);
			if (sna != null) {
				var data = AttributeReader.ReadAttribute<ScriptNamespaceAttribute>(sna);
				if (data.Name == null || (data.Name != "" && !data.Name.IsValidNestedJavaScriptIdentifier())) {
					Message(7002, sna.Region, "assembly");
				}
			}

			var sca = AttributeReader.ReadAttribute<ScriptSharpCompatibilityAttribute>(mainAssembly.AssemblyAttributes);
			_omitDowncasts      = sca != null && sca.OmitDowncasts;
			_omitNullableChecks = sca != null && sca.OmitNullableChecks;

			var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(mainAssembly.AssemblyAttributes);
			_mainModuleName = (mna != null && !string.IsNullOrEmpty((string)mna.ModuleName) ? mna.ModuleName : null);

			_isAsyncModule = AttributeReader.HasAttribute<AsyncModuleAttribute>(mainAssembly.AssemblyAttributes);

			foreach (var t in types.OrderBy(x => x.ParentAssembly.AssemblyName).ThenBy(x => x.ReflectionName)) {
				try {
					ProcessType(t);
					ProcessTypeMembers(t);
				}
				catch (Exception ex) {
					_errorReporter.Region = t.Region;
					_errorReporter.InternalError(ex, "Error importing type " + t.FullName);
				}
			}
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

		public string GetTypeParameterName(ITypeParameter typeParameter) {
			return _typeParameterNames[typeParameter];
		}

		public MethodScriptSemantics GetMethodSemantics(IMethod method) {
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

		public bool IsNamedValues(ITypeDefinition t) {
			return GetTypeSemanticsInternal(t).IsNamedValues;
		}

		public bool IsResources(ITypeDefinition t) {
			return GetTypeSemanticsInternal(t).IsResources;
		}

		public bool IsMixin(ITypeDefinition t) {
			return GetTypeSemanticsInternal(t).IsMixin;
		}

		public bool IsSerializable(ITypeDefinition t) {
			return GetTypeSemanticsInternal(t).IsSerializable;
		}

		public bool DoesTypeObeyTypeSystem(ITypeDefinition t) {
			return GetTypeSemanticsInternal(t).ObeysTypeSystem;
		}

		public bool IsImported(ITypeDefinition t) {
			return GetTypeSemanticsInternal(t).IsImported;
		}

		public string GetModuleName(ITypeDefinition t) {
			for (var current = t; current != null; current = current.DeclaringTypeDefinition) {
				var n = GetTypeSemanticsInternal(current).ModuleName;
				if (n != null)
					return n != "" ? n : null;
			}
			var asmModuleName = AttributeReader.ReadAttribute<ModuleNameAttribute>(t.ParentAssembly.AssemblyAttributes);
			if (asmModuleName != null && !string.IsNullOrEmpty(asmModuleName.ModuleName))
				return asmModuleName.ModuleName;

			return null;
		}

		public bool OmitDowncasts {
			get { return _omitDowncasts; }
		}

		public bool OmitNullableChecks {
			get { return _omitNullableChecks; }
		}

		public string MainModuleName {
			get { return _mainModuleName; }
		}

		public bool IsAsyncModule {
			get { return _isAsyncModule; }
		}
	}
}
