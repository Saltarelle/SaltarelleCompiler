using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Plugin {
#warning TODO: Json constructor verifier must validate accessibility
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

		private readonly Dictionary<INamedTypeSymbol, TypeSemantics> _typeSemantics;
		private readonly Dictionary<INamedTypeSymbol, DelegateScriptSemantics> _delegateSemantics;
		private readonly Dictionary<INamedTypeSymbol, HashSet<string>> _instanceMemberNamesByType;
		private readonly Dictionary<INamedTypeSymbol, HashSet<string>> _staticMemberNamesByType;
		private readonly Dictionary<IMethodSymbol, MethodScriptSemantics> _methodSemantics;
		private readonly Dictionary<IPropertySymbol, PropertyScriptSemantics> _propertySemantics;
		private readonly Dictionary<IFieldSymbol, FieldScriptSemantics> _fieldSemantics;
		private readonly Dictionary<IEventSymbol, EventScriptSemantics> _eventSemantics;
		private readonly Dictionary<IMethodSymbol, ConstructorScriptSemantics> _constructorSemantics;
		private readonly Dictionary<IPropertySymbol, Tuple<string, bool>> _propertyBackingFieldNames;
		private readonly Dictionary<IEventSymbol, Tuple<string, bool>> _eventBackingFieldNames;
		private readonly Dictionary<INamedTypeSymbol, int> _backingFieldCountPerType;
		private readonly Dictionary<Tuple<IAssemblySymbol, string>, int> _internalTypeCountPerAssemblyAndNamespace;
		private readonly HashSet<ISymbol> _ignoredMembers;
		private readonly IMetadataImporter _prev;
		private readonly IErrorReporter _errorReporter;
		private readonly ITypeSymbol _systemObject;
		private readonly Compilation _compilation;
		private readonly IAttributeStore _attributeStore;

		private readonly bool _minimizeNames;

		public MetadataImporter(IMetadataImporter prev, IErrorReporter errorReporter, Compilation compilation, IAttributeStore attributeStore, CompilerOptions options) {
			_prev = prev;
			_errorReporter = errorReporter;
			_compilation = compilation;
			_attributeStore = attributeStore;
			_minimizeNames = options.MinimizeScript;
			_systemObject = compilation.GetSpecialType(SpecialType.System_Object);
			_typeSemantics = new Dictionary<INamedTypeSymbol, TypeSemantics>();
			_delegateSemantics = new Dictionary<INamedTypeSymbol, DelegateScriptSemantics>();
			_instanceMemberNamesByType = new Dictionary<INamedTypeSymbol, HashSet<string>>();
			_staticMemberNamesByType = new Dictionary<INamedTypeSymbol, HashSet<string>>();
			_methodSemantics = new Dictionary<IMethodSymbol, MethodScriptSemantics>();
			_propertySemantics = new Dictionary<IPropertySymbol, PropertyScriptSemantics>();
			_fieldSemantics = new Dictionary<IFieldSymbol, FieldScriptSemantics>();
			_eventSemantics = new Dictionary<IEventSymbol, EventScriptSemantics>();
			_constructorSemantics = new Dictionary<IMethodSymbol, ConstructorScriptSemantics>();
			_propertyBackingFieldNames = new Dictionary<IPropertySymbol, Tuple<string, bool>>();
			_eventBackingFieldNames = new Dictionary<IEventSymbol, Tuple<string, bool>>();
			_backingFieldCountPerType = new Dictionary<INamedTypeSymbol, int>();
			_internalTypeCountPerAssemblyAndNamespace = new Dictionary<Tuple<IAssemblySymbol, string>, int>();
			_ignoredMembers = new HashSet<ISymbol>();

			var sna = _attributeStore.AttributesFor(compilation.Assembly).GetAttribute<ScriptNamespaceAttribute>();
			if (sna != null) {
				if (sna.Name == null || (sna.Name != "" && !sna.Name.IsValidNestedJavaScriptIdentifier())) {
					Message(Messages._7002, default(Location), "assembly");
				}
			}
		}

		private void Message(Tuple<int, DiagnosticSeverity, string> message, Location l, params object[] additionalArgs) {
			_errorReporter.Location = l;
			_errorReporter.Message(message, additionalArgs);
		}

		private void Message(Tuple<int, DiagnosticSeverity, string> message, ISymbol e, params object[] additionalArgs) {
			var name = (e is IMethodSymbol && ((IMethodSymbol)e).MethodKind == MethodKind.Constructor ? e.ContainingType.FullyQualifiedName() : e.FullyQualifiedName());
			_errorReporter.Location = e.Locations.First();
			_errorReporter.Message(message, new object[] { name }.Concat(additionalArgs).ToArray());
		}

		private string GetDefaultTypeName(INamedTypeSymbol def, bool ignoreGenericArguments) {
			if (ignoreGenericArguments) {
				return def.Name;
			}
			else {
				return def.Name + (def.TypeParameters.Length > 0 ? "$" + def.TypeParameters.Length.ToString(CultureInfo.InvariantCulture) : "");
			}
		}

		private string DetermineNamespace(INamedTypeSymbol typeDefinition) {
			while (typeDefinition.ContainingType != null) {
				typeDefinition = typeDefinition.ContainingType;
			}

			var attributes = _attributeStore.AttributesFor(typeDefinition);
			var ina = attributes.GetAttribute<IgnoreNamespaceAttribute>();
			var sna = attributes.GetAttribute<ScriptNamespaceAttribute>();
			if (ina != null) {
				if (sna != null) {
					Message(Messages._7001, typeDefinition);
					return typeDefinition.FullyQualifiedName();
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
					var asna = _attributeStore.AttributesFor(typeDefinition.ContainingAssembly).GetAttribute<ScriptNamespaceAttribute>();
					if (asna != null) {
						if (asna.Name != null && (asna.Name == "" || asna.Name.IsValidNestedJavaScriptIdentifier()))
							return asna.Name;
					}

					return typeDefinition.ContainingNamespace.FullyQualifiedName();
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

		private void ProcessDelegate(INamedTypeSymbol delegateDefinition) {
			var attributes = _attributeStore.AttributesFor(delegateDefinition);
			bool bindThisToFirstParameter = attributes.HasAttribute<BindThisToFirstParameterAttribute>();
			bool expandParams = attributes.HasAttribute<ExpandParamsAttribute>();

			if (bindThisToFirstParameter && delegateDefinition.DelegateInvokeMethod.Parameters.Length == 0) {
				Message(Messages._7147, delegateDefinition, delegateDefinition.FullyQualifiedName());
				bindThisToFirstParameter = false;
			}
			if (expandParams && !delegateDefinition.DelegateInvokeMethod.Parameters.Any(p => p.IsParams)) {
				Message(Messages._7148, delegateDefinition, delegateDefinition.FullyQualifiedName());
				expandParams = false;
			}

			_delegateSemantics[delegateDefinition] = new DelegateScriptSemantics(expandParams: expandParams, bindThisToFirstParameter: bindThisToFirstParameter);
		}

		private void ProcessType(INamedTypeSymbol typeDefinition) {
			if (typeDefinition.TypeKind == TypeKind.Delegate) {
				ProcessDelegate(typeDefinition);
				return;
			}

			var attributes = _attributeStore.AttributesFor(typeDefinition);
			if (attributes.HasAttribute<NonScriptableAttribute>() || typeDefinition.ContainingType != null && GetTypeSemantics(typeDefinition.ContainingType).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NotUsableFromScript(), false, false, false);
				return;
			}

			var scriptNameAttr = attributes.GetAttribute<ScriptNameAttribute>();
			var importedAttr = attributes.GetAttribute<ImportedAttribute>();
			bool preserveName = importedAttr != null || attributes.HasAttribute<PreserveNameAttribute>();

			bool? includeGenericArguments = typeDefinition.TypeParameters.Length > 0 ? MetadataUtils.ShouldGenericArgumentsBeIncluded(typeDefinition, _attributeStore) : false;
			if (includeGenericArguments == null) {
				_errorReporter.Location = typeDefinition.Locations[0];
				Message(Messages._7026, typeDefinition);
				includeGenericArguments = true;
			}

			if (attributes.HasAttribute<ResourcesAttribute>()) {
				if (!typeDefinition.IsStatic) {
					Message(Messages._7003, typeDefinition);
				}
				else if (typeDefinition.TypeParameters.Length > 0) {
					Message(Messages._7004, typeDefinition);
				}
				else if (typeDefinition.GetMembers().Any(m => !(m is IFieldSymbol && ((IFieldSymbol)m).IsConst))) {
					Message(Messages._7005, typeDefinition);
				}
			}

			string typeName, nmspace;
			if (scriptNameAttr != null && scriptNameAttr.Name != null && scriptNameAttr.Name.IsValidJavaScriptIdentifier()) {
				typeName = scriptNameAttr.Name;
				nmspace = DetermineNamespace(typeDefinition);
			}
			else if (scriptNameAttr != null && string.IsNullOrEmpty(scriptNameAttr.Name) && !string.IsNullOrEmpty(MetadataUtils.GetModuleName(typeDefinition, _attributeStore))) {
				typeName = "";
				nmspace = "";
			}
			else {
				if (scriptNameAttr != null) {
					Message(Messages._7006, typeDefinition);
				}

				if (_minimizeNames && MetadataUtils.CanBeMinimized(typeDefinition) && !preserveName) {
					nmspace = DetermineNamespace(typeDefinition);
					var key = Tuple.Create(typeDefinition.ContainingAssembly, nmspace);
					int index;
					_internalTypeCountPerAssemblyAndNamespace.TryGetValue(key, out index);
					_internalTypeCountPerAssemblyAndNamespace[key] = index + 1;
					typeName = "$" + index.ToString(CultureInfo.InvariantCulture);
				}
				else {
					typeName = GetDefaultTypeName(typeDefinition, !includeGenericArguments.Value);
					if (typeDefinition.ContainingType != null) {
						if (attributes.HasAttribute<IgnoreNamespaceAttribute>() || attributes.HasAttribute<ScriptNamespaceAttribute>()) {
							Message(Messages._7007, typeDefinition);
						}

						var declaringName = SplitNamespacedName(GetTypeSemantics(typeDefinition.ContainingType).Name);
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

			bool isSerializable = MetadataUtils.IsSerializable(typeDefinition, _attributeStore);

			if (isSerializable) {
				if (typeDefinition.BaseType != null && !typeDefinition.BaseType.Equals(_systemObject) && typeDefinition.BaseType.FullyQualifiedName() != "System.Record" && !MetadataUtils.IsSerializable(typeDefinition.BaseType.OriginalDefinition, _attributeStore)) {
					Message(Messages._7009, typeDefinition);
				}
				foreach (var i in typeDefinition.AllInterfaces.Where(b => !MetadataUtils.IsSerializable(b.OriginalDefinition, _attributeStore))) {
					Message(Messages._7010, typeDefinition, i.FullyQualifiedName());
				}
				var members = typeDefinition.GetMembers();

				if (members.OfType<IEventSymbol>().Any(evt => !evt.IsStatic)) {
					Message(Messages._7011, typeDefinition);
				}
				foreach (var m in members.Where(m => m.IsVirtual)) {
					Message(Messages._7023, typeDefinition, m.Name);
				}
				foreach (var m in members.Where(m => m.IsOverride)) {
					Message(Messages._7024, typeDefinition, m.Name);
				}

				if (typeDefinition.TypeKind == TypeKind.Interface && members.OfType<IMethodSymbol>().Any(m => !m.IsAccessor())) {
					Message(Messages._7155, typeDefinition);
				}
			}
			else {
				var globalMethodsAttr = attributes.GetAttribute<GlobalMethodsAttribute>();
				var mixinAttr = attributes.GetAttribute<MixinAttribute>();
				if (mixinAttr != null) {
					if (!typeDefinition.IsStatic) {
						Message(Messages._7012, typeDefinition);
					}
					else if (typeDefinition.GetMembers().Any(m => !m.IsImplicitlyDeclared && !_attributeStore.AttributesFor(m).HasAttribute<CompilerGeneratedAttribute>() && (!(m is IMethodSymbol) || ((IMethodSymbol)m).MethodKind == MethodKind.Constructor || ((IMethodSymbol)m).MethodKind == MethodKind.StaticConstructor))) {
						Message(Messages._7013, typeDefinition);
					}
					else if (typeDefinition.TypeParameters.Length > 0) {
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
					else if (typeDefinition.TypeParameters.Length > 0) {
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
				if (!string.IsNullOrEmpty(MetadataUtils.GetSerializableTypeCheckCode(typeDefinition, _attributeStore))) {
					Message(Messages._7159, typeDefinition);
				}
			}

			bool isMutableValueType = false;
			if (typeDefinition.TypeKind == TypeKind.Struct) {
				isMutableValueType = attributes.HasAttribute<MutableAttribute>();
				if (!isMutableValueType && typeDefinition.ContainingAssembly.Equals(_compilation.Assembly)) {
					var members = typeDefinition.GetMembers();
					foreach (var p in members.OfType<IPropertySymbol>().Where(p => !p.IsStatic && MetadataUtils.IsAutoProperty(_compilation, p) == true)) {
						Message(Messages._7162, p.Locations[0], typeDefinition.FullyQualifiedName());
					}
					foreach (var e in members.OfType<IEventSymbol>().Where(e => !e.IsStatic && MetadataUtils.IsAutoEvent(_compilation, e) == true)) {
						Message(Messages._7162, e.Locations[0], typeDefinition.FullyQualifiedName());
					}
					foreach (var f in members.OfType<IFieldSymbol>().Where(f => !f.IsStatic && !f.IsReadOnly)) {
						Message(Messages._7162, f.Locations[0], typeDefinition.FullyQualifiedName());
					}
				}
			}

			string name = !string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName;
			_typeSemantics[typeDefinition] = new TypeSemantics(isMutableValueType ? TypeScriptSemantics.MutableValueType(name, ignoreGenericArguments: !includeGenericArguments.Value, generateCode: importedAttr == null) : TypeScriptSemantics.NormalType(name, ignoreGenericArguments: !includeGenericArguments.Value, generateCode: importedAttr == null), isSerializable: isSerializable, isNamedValues: MetadataUtils.IsNamedValues(typeDefinition, _attributeStore), isImported: importedAttr != null);
		}

		private Tuple<string, bool> DeterminePreferredMemberName(ISymbol member) {
			var asa = _attributeStore.AttributesFor(member).GetAttribute<AlternateSignatureAttribute>();
			if (asa != null) {
				var otherMembers = member.ContainingType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MetadataName == member.MetadataName && !_attributeStore.AttributesFor(m).HasAttribute<AlternateSignatureAttribute>() && !_attributeStore.AttributesFor(m).HasAttribute<NonScriptableAttribute>() && !_attributeStore.AttributesFor(m).HasAttribute<InlineCodeAttribute>()).ToList();
				if (otherMembers.Count != 1) {
					Message(Messages._7100, member);
					return Tuple.Create(member.MetadataName, false);
				}
			}
			return MetadataUtils.DeterminePreferredMemberName(member, _minimizeNames, _attributeStore);
		}

		private bool HasExplicitNameAttribute(AttributeList attributes) {
			return attributes.HasAttribute<PreserveNameAttribute>() || attributes.HasAttribute<PreserveCaseAttribute>() || attributes.HasAttribute<ScriptNameAttribute>() || attributes.HasAttribute<AlternateSignatureAttribute>();
		}

		private void ValidateAndProcessMethodImplementingInterfaceMember(INamedTypeSymbol type, IMethodSymbol interfaceMethod, IMethodSymbol implementorMethod, Dictionary<string, bool> instanceMembers, HashSet<ISymbol> symbolsImplementingInterfaceMembers) {
			var interfaceSemantics = GetMethodSemantics(interfaceMethod);
			if (interfaceSemantics.GeneratedMethodName != null) {
				MethodScriptSemantics implementorSemantics;
				var declaringMethod = implementorMethod.DeclaringMethod();

				if (declaringMethod.ContainingType != type)
					implementorSemantics = GetMethodSemantics(declaringMethod);
				else
					_methodSemantics.TryGetValue(declaringMethod, out implementorSemantics);

				if (implementorSemantics != null) {
					if (implementorSemantics.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
						// Don't need to do anything - this kind of method is only created for serializable types, and those can only inherit serializable interfaces, and those in turn are not allowed to have methods.
					}
					else if (implementorSemantics.GeneratedMethodName == null) {
						if (implementorSemantics.Type != MethodScriptSemantics.ImplType.NativeIndexer) {	// The case of native indexers implementing non-native indexer properties will be handled later.
							if (declaringMethod.ContainingType == type)
								Message(Messages._7173, implementorMethod, interfaceMethod.FullyQualifiedName(), interfaceSemantics.GeneratedMethodName);
							else
								Message(Messages._7173, type.Locations[0], implementorMethod.FullyQualifiedName(), interfaceMethod.FullyQualifiedName(), interfaceSemantics.GeneratedMethodName);
						}
					}
					else if (implementorSemantics.GeneratedMethodName != interfaceSemantics.GeneratedMethodName) {
						if (declaringMethod.ContainingType == type)
							Message(Messages._7136, implementorMethod, interfaceMethod.FullyQualifiedName(), interfaceSemantics.GeneratedMethodName, implementorSemantics.GeneratedMethodName);
						else
							Message(Messages._7171, type.Locations[0], implementorMethod.FullyQualifiedName(), interfaceMethod.FullyQualifiedName(), interfaceSemantics.GeneratedMethodName, implementorSemantics.GeneratedMethodName);
					}
				}
				else {
					if (instanceMembers.ContainsKey(interfaceSemantics.GeneratedMethodName)) {
						Message(Messages._7172, implementorMethod, interfaceMethod.FullyQualifiedName(), interfaceSemantics.GeneratedMethodName);
					}
					ProcessMethodImplementingInterfaceMember(implementorMethod, interfaceSemantics);
				}

				symbolsImplementingInterfaceMembers.Add(declaringMethod);
				instanceMembers[interfaceSemantics.GeneratedMethodName] = true;
			}
		}

		private string FormatPropertyImplementation(PropertyScriptSemantics s) {
			switch (s.Type) {
				case PropertyScriptSemantics.ImplType.Field:
					return "a field";
				case PropertyScriptSemantics.ImplType.GetAndSetMethods:
					if ((s.GetMethod ?? s.SetMethod).Type == MethodScriptSemantics.ImplType.NativeIndexer)
						return "a native indexer";
					return "get and set methods";
				default:
					throw new ArgumentException("Unknown property semantics" + s.Type);
			}
		}

		private MethodScriptSemantics GetAccessorSemanticsForPropertyImplementingInterfaceMember(INamedTypeSymbol type, IPropertySymbol property, IMethodSymbol accessor, Dictionary<string, bool> instanceMembers) {
			if (accessor == null)
				return null;

			if (!Equals(property.ContainingType, type)) {
				return GetMethodSemantics(accessor);
			}

			MethodScriptSemantics existing;
			_methodSemantics.TryGetValue(accessor, out existing);
			if (existing != null)
				return existing;

			var name = DeterminePreferredMemberName(accessor);
			ProcessMethod(accessor, name.Item1, name.Item2, instanceMembers);
			return _methodSemantics[accessor];
		}

		private void ValidateAndProcessPropertyImplementingInterfaceMember(INamedTypeSymbol type, IPropertySymbol interfaceProperty, IPropertySymbol implementorProperty, Dictionary<string, bool> instanceMembers, HashSet<ISymbol> symbolsImplementingInterfaceMembers) {
			var interfaceSemantics = GetPropertySemantics(interfaceProperty);
			if (interfaceSemantics.Type != PropertyScriptSemantics.ImplType.NotUsableFromScript) {
				PropertyScriptSemantics implementorSemantics;
				var declaringProperty = implementorProperty.DeclaringProperty();

				if (declaringProperty.ContainingType != type) {
					implementorSemantics = GetPropertySemantics(declaringProperty);
				}
				else {
					_propertySemantics.TryGetValue(declaringProperty, out implementorSemantics);
					if (implementorSemantics == null && HasExplicitNameAttribute(_attributeStore.AttributesFor(declaringProperty))) {
						Message(Messages._7135, declaringProperty);
					}
				}

				if (implementorSemantics != null && implementorSemantics.Type != interfaceSemantics.Type) {
					if (declaringProperty.ContainingType == type)
						Message(Messages._7175, implementorProperty, interfaceProperty.FullyQualifiedName(), FormatPropertyImplementation(interfaceSemantics), FormatPropertyImplementation(implementorSemantics));
					else
						Message(Messages._7174, type.Locations[0], implementorProperty.FullyQualifiedName(), interfaceProperty.FullyQualifiedName(), FormatPropertyImplementation(interfaceSemantics), FormatPropertyImplementation(implementorSemantics));
				}
				else  {
					ValidatePropertyImplementingInterfaceMember(implementorProperty, interfaceProperty, interfaceSemantics);

					if (interfaceSemantics.Type == PropertyScriptSemantics.ImplType.Field) {
						if (implementorSemantics != null) {
							if (implementorSemantics.FieldName != interfaceSemantics.FieldName) {
								if (declaringProperty.ContainingType == type)
									Message(Messages._7136, implementorProperty, interfaceProperty.FullyQualifiedName(), interfaceSemantics.FieldName, implementorSemantics.FieldName);
								else
									Message(Messages._7171, type.Locations[0], implementorProperty.FullyQualifiedName(), interfaceProperty.FullyQualifiedName(), interfaceSemantics.FieldName, implementorSemantics.FieldName);
							}
						}
						else if (instanceMembers.ContainsKey(interfaceSemantics.FieldName)) {
							Message(Messages._7172, implementorProperty, interfaceProperty.FullyQualifiedName(), interfaceSemantics.FieldName);
						}

						SetField(implementorProperty, interfaceSemantics.FieldName);
						instanceMembers[interfaceSemantics.FieldName] = true;
					}
					else if (interfaceSemantics.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods) {
						if (implementorSemantics == null) {
							if (_attributeStore.AttributesFor(implementorProperty).HasAttribute<IntrinsicPropertyAttribute>()) {
								if (implementorProperty.IsIndexer)
									Message(Messages._7114, implementorProperty.Locations[0]);
								else
									Message(Messages._7115, implementorProperty);
							}

							_propertySemantics[implementorProperty] = PropertyScriptSemantics.GetAndSetMethods(GetAccessorSemanticsForPropertyImplementingInterfaceMember(type, implementorProperty, implementorProperty.GetMethod, instanceMembers), GetAccessorSemanticsForPropertyImplementingInterfaceMember(type, implementorProperty, implementorProperty.SetMethod, instanceMembers));
						}
						else if ((implementorSemantics.GetMethod ?? implementorSemantics.SetMethod).Type == MethodScriptSemantics.ImplType.NativeIndexer) {
							Message(Messages._7176, type.Locations[0]);
						}
					}
					else {
						_errorReporter.InternalError("Unknown property semantics");
					}
				}

				symbolsImplementingInterfaceMembers.Add(declaringProperty);
			}
		}

		private void ValidateAndProcessEventImplementingInterfaceMember(INamedTypeSymbol type, IEventSymbol interfaceEvent, IEventSymbol implementorEvent, Dictionary<string, bool> instanceMembers, HashSet<ISymbol> symbolsImplementingInterfaceMembers) {
			var interfaceSemantics = GetEventSemantics(interfaceEvent);
			if (interfaceSemantics.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods) {
				var declaringEvent = implementorEvent.DeclaringEvent();

				if (declaringEvent.ContainingType == type) {
					if (!_eventSemantics.ContainsKey(declaringEvent) && HasExplicitNameAttribute(_attributeStore.AttributesFor(declaringEvent))) {
						Message(Messages._7135, declaringEvent);
					}
				}

				symbolsImplementingInterfaceMembers.Add(declaringEvent);
				_eventSemantics[implementorEvent] = EventScriptSemantics.AddAndRemoveMethods(_methodSemantics[declaringEvent.AddMethod], _methodSemantics[declaringEvent.RemoveMethod]);
			}
		}

		private void HandleInterfaceImplementations(INamedTypeSymbol type, Dictionary<string, bool> instanceMembers, HashSet<ISymbol> symbolsImplementingInterfaceMembers) {
			var interfaces = type.Interfaces.SelectMany(i => new[] { i }.Concat(i.AllInterfaces)).Distinct();
			var interfaceMembers = interfaces.SelectMany(i => i.GetMembers()).ToList();
			foreach (var interfaceMethod in interfaceMembers.OfType<IMethodSymbol>()) {
				var implementor = (IMethodSymbol)type.FindImplementationForInterfaceMember(interfaceMethod);
				if (!Equals(implementor.ContainingType, type) && type.BaseType != null && Equals(type.BaseType.FindImplementationForInterfaceMember(interfaceMethod), implementor))
					continue;	// We have already verified (or errored) when verifying the base type.

				if (interfaceMethod.IsAccessor()) {
					if (interfaceMethod.AssociatedSymbol is IPropertySymbol) {
						var psem = GetPropertySemantics((IPropertySymbol)interfaceMethod.AssociatedSymbol);
						if (psem.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods)
							continue;
					}
					else if (interfaceMethod.AssociatedSymbol is IEventSymbol) {
						var esem = GetEventSemantics((IEventSymbol)interfaceMethod.AssociatedSymbol);
						if (esem.Type != EventScriptSemantics.ImplType.AddAndRemoveMethods)
							continue;
					}
				}

				ValidateAndProcessMethodImplementingInterfaceMember(type, interfaceMethod, implementor, instanceMembers, symbolsImplementingInterfaceMembers);
			}

			foreach (var interfaceProperty in interfaceMembers.OfType<IPropertySymbol>()) {
				var implementor = (IPropertySymbol)type.FindImplementationForInterfaceMember(interfaceProperty);
				if (!Equals(implementor.ContainingType, type) && type.BaseType != null && Equals(type.BaseType.FindImplementationForInterfaceMember(interfaceProperty), implementor))
					continue;	// We have already verified (or errored) when verifying the base type.

				ValidateAndProcessPropertyImplementingInterfaceMember(type, interfaceProperty, implementor, instanceMembers, symbolsImplementingInterfaceMembers);
			}

			foreach (var interfaceEvent in interfaceMembers.OfType<IEventSymbol>()) {
				var implementor = (IEventSymbol)type.FindImplementationForInterfaceMember(interfaceEvent);
				if (!Equals(implementor.ContainingType, type) && type.BaseType != null && Equals(type.BaseType.FindImplementationForInterfaceMember(interfaceEvent), implementor))
					continue;	// We have already verified (or errored) when verifying the base type.

				ValidateAndProcessEventImplementingInterfaceMember(type, interfaceEvent, implementor, instanceMembers, symbolsImplementingInterfaceMembers);
			}
		}

		private void ProcessTypeMembers(INamedTypeSymbol typeDefinition) {
			if (typeDefinition.TypeKind == TypeKind.Delegate)
				return;

			var instanceMembers = typeDefinition.BaseType != null ? GetUsedInstanceMemberNames(typeDefinition.BaseType.OriginalDefinition).ToDictionary(m => m, m => false) : new Dictionary<string, bool>();
			_unusableInstanceFieldNames.ForEach(n => instanceMembers[n] = false);
			if (_instanceMemberNamesByType.ContainsKey(typeDefinition))
				_instanceMemberNamesByType[typeDefinition].ForEach(s => instanceMembers[s] = true);
			var symbolsImplementingInterfaceMembers = new HashSet<ISymbol>();

			if (typeDefinition.TypeKind == TypeKind.Class || typeDefinition.TypeKind == TypeKind.Struct) {
				HandleInterfaceImplementations(typeDefinition, instanceMembers, symbolsImplementingInterfaceMembers);
				foreach (var i in typeDefinition.AllInterfaces) {
					HashSet<string> hs;
					if (_instanceMemberNamesByType.TryGetValue(i, out hs)) {
						hs.ForEach(n => instanceMembers[n] = true);
					}
				}
			}

			var staticMembers = _unusableStaticFieldNames.ToDictionary(n => n, n => false);
			if (_staticMemberNamesByType.ContainsKey(typeDefinition))
				_staticMemberNamesByType[typeDefinition].ForEach(s => staticMembers[s] = true);

			var membersByName =   from m in typeDefinition.GetMembers().Where(m => !(m is ITypeSymbol) && !m.IsAccessor() && !_ignoredMembers.Contains(m) && !symbolsImplementingInterfaceMembers.Contains(m))
			                       let name = DeterminePreferredMemberName(m)
			                     group new { m, name } by name.Item1 into g
			                    select new { Name = g.Key, Members = g.Select(x => new { Member = x.m, NameSpecified = x.name.Item2 }).ToList() };

			bool isSerializable = GetTypeSemanticsInternal(typeDefinition).IsSerializable;
			foreach (var current in membersByName) {
				foreach (var m in current.Members.OrderByDescending(x => x.NameSpecified).ThenBy(x => x.Member, MemberOrderer.Instance)) {
					if (m.Member is IMethodSymbol) {
						var method = (IMethodSymbol)m.Member;

						if (method.MethodKind == MethodKind.Constructor) {
							ProcessConstructor(method, current.Name, m.NameSpecified, staticMembers);
						}
						else {
							ProcessMethod(method, current.Name, m.NameSpecified, m.Member.IsStatic || isSerializable ? staticMembers : instanceMembers);
						}
					}
					else if (m.Member is IPropertySymbol) {
						var p = (IPropertySymbol)m.Member;
						ProcessProperty(p, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
						var ps = GetPropertySemantics(p);
						if (p.GetMethod != null)
							_methodSemantics[p.GetMethod] = ps.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods ? ps.GetMethod : MethodScriptSemantics.NotUsableFromScript();
						if (p.SetMethod != null)
							_methodSemantics[p.SetMethod] = ps.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods ? ps.SetMethod : MethodScriptSemantics.NotUsableFromScript();
					}
					else if (m.Member is IFieldSymbol) {
						if (!m.Member.IsImplicitlyDeclared) {
							ProcessField((IFieldSymbol)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
						}
					}
					else if (m.Member is IEventSymbol) {
						var e = (IEventSymbol)m.Member;
						ProcessEvent((IEventSymbol)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
						var es = GetEventSemantics(e);
						_methodSemantics[e.AddMethod]    = es.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods ? es.AddMethod    : MethodScriptSemantics.NotUsableFromScript();
						_methodSemantics[e.RemoveMethod] = es.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods ? es.RemoveMethod : MethodScriptSemantics.NotUsableFromScript();
					}
				}
			}

			_instanceMemberNamesByType[typeDefinition] = new HashSet<string>(instanceMembers.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
			_staticMemberNamesByType[typeDefinition] = new HashSet<string>(staticMembers.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
		}

		private string GetUniqueName(string preferredName, Dictionary<string, bool> usedNames) {
			return MetadataUtils.GetUniqueName(preferredName, n => !usedNames.ContainsKey(n));
		}

		private bool ValidateInlineCode(IMethodSymbol method, ISymbol errorEntity, string code, Tuple<int, DiagnosticSeverity, string> errorTemplate) {
			var typeErrors = new List<string>();
			IList<string> errors;
			Func<string, JsExpression> resolveType = n => {
				var type = _compilation.GetTypeByMetadataName(n);
				if (type == null) {
					typeErrors.Add("Unknown type '" + n + "' specified in inline implementation");
				}
				return JsExpression.Null;
			};
			
			if (method.ReturnsVoid && method.MethodKind != MethodKind.Constructor) {
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

		private void ProcessConstructor(IMethodSymbol constructor, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (constructor.Parameters.Length == 1 && constructor.Parameters[0].Type.FullyQualifiedName() == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			var source = (IMethodSymbol)MetadataUtils.UnwrapValueTypeConstructor(constructor);

			var attributes = _attributeStore.AttributesFor(source);
			var nsa = attributes.GetAttribute<NonScriptableAttribute>();
			var asa = attributes.GetAttribute<AlternateSignatureAttribute>();
			var epa = attributes.GetAttribute<ExpandParamsAttribute>();
			var ola = attributes.GetAttribute<ObjectLiteralAttribute>();
			bool generateCode = !attributes.HasAttribute<DontGenerateAttribute>() && asa == null;

			if (nsa != null || GetTypeSemanticsInternal(source.ContainingType).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
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

			bool isSerializable    = GetTypeSemanticsInternal(source.ContainingType).IsSerializable;
			bool isImported        = GetTypeSemanticsInternal(source.ContainingType).IsImported;
			bool skipInInitializer = attributes.HasAttribute<ScriptSkipAttribute>();

			var ica = attributes.GetAttribute<InlineCodeAttribute>();
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
					Message(Messages._7029, constructor.Locations[0], "constructor", constructor.ContainingType.FullyQualifiedName());
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
			else if (ola != null || (isSerializable && GetTypeSemanticsInternal(source.ContainingType).IsImported)) {
				if (isSerializable) {
					bool hasError = false;
					var members = source.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Property || m.Kind == SymbolKind.Field).ToDictionary(m => m.MetadataName.ToLowerInvariant());
					var parameterToMemberMap = new List<ISymbol>();
					foreach (var p in source.Parameters) {
						ISymbol member;
						if (p.RefKind != RefKind.None) {
							Message(Messages._7145, p.Locations[0], p.Name);
							hasError = true;
						}
						else if (members.TryGetValue(p.Name.ToLowerInvariant(), out member)) {
							var memberReturnType = member is IFieldSymbol ? ((IFieldSymbol)member).Type : ((IPropertySymbol)member).Type;

							if (p.Type.GetSelfAndAllBaseTypes().Any(b => b.Equals(memberReturnType)) || (memberReturnType.IsNullable() && memberReturnType.UnpackNullable().Equals(p.Type))) {
								parameterToMemberMap.Add(member);
							}
							else {
								Message(Messages._7144, p.Locations[0], p.Name, p.Type.FullyQualifiedName(), memberReturnType.FullyQualifiedName());
								hasError = true;
							}
						}
						else {
							Message(Messages._7143, p.Locations[0], source.ContainingType.FullyQualifiedName(), p.Name);
							hasError = true;
						}
					}
					_constructorSemantics[constructor] = hasError ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Json(parameterToMemberMap, skipInInitializer: skipInInitializer || constructor.Parameters.Length == 0);
				}
				else {
					Message(Messages._7146, constructor.Locations[0], source.ContainingType.FullyQualifiedName());
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
				}
				return;
			}
			else if (source.Parameters.Length == 1 && source.Parameters[0].Type.TypeKind == TypeKind.ArrayType && ((IArrayTypeSymbol)source.Parameters[0].Type).ElementType.SpecialType == SpecialType.System_Object && source.Parameters[0].IsParams && isImported) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.InlineCode("ss.mkdict({" + source.Parameters[0].Name + "})", skipInInitializer: skipInInitializer);
				return;
			}
			else if (nameSpecified) {
				if (isSerializable)
					_constructorSemantics[constructor] = ConstructorScriptSemantics.StaticMethod(preferredName, generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer);
				else
					_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Named(preferredName, generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer);
				usedNames[preferredName] = true;
				return;
			}
			else {
				if (!usedNames.ContainsKey("$ctor") && !(isSerializable && _minimizeNames && MetadataUtils.CanBeMinimized(source, _attributeStore))) {	// The last part ensures that the first constructor of a serializable type can have its name minimized.
					_constructorSemantics[constructor] = isSerializable ? ConstructorScriptSemantics.StaticMethod("$ctor", generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Unnamed(generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer);
					usedNames["$ctor"] = true;
					return;
				}
				else {
					string name;
					if (_minimizeNames && MetadataUtils.CanBeMinimized(source, _attributeStore)) {
						name = GetUniqueName(null, usedNames);
					}
					else {
						int i = 1;
						do {
							name = "$ctor" + MetadataUtils.EncodeNumber(i, false);
							i++;
						} while (usedNames.ContainsKey(name));
					}

					_constructorSemantics[constructor] = isSerializable ? ConstructorScriptSemantics.StaticMethod(name, generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer) : ConstructorScriptSemantics.Named(name, generateCode: generateCode, expandParams: epa != null, skipInInitializer: skipInInitializer);
					usedNames[name] = true;
					return;
				}
			}
		}

		private void SetNotUsableFromScript(IPropertySymbol property) {
			_propertySemantics[property] = PropertyScriptSemantics.NotUsableFromScript();
			if (property.GetMethod != null)
				_methodSemantics[property.GetMethod] = MethodScriptSemantics.NotUsableFromScript();
			if (property.SetMethod != null)
				_methodSemantics[property.SetMethod] = MethodScriptSemantics.NotUsableFromScript();
		}

		private void SetField(IPropertySymbol property, string fieldName) {
			_propertySemantics[property] = PropertyScriptSemantics.Field(fieldName);
			if (property.GetMethod != null)
				_methodSemantics[property.GetMethod] = MethodScriptSemantics.NotUsableFromScript();
			if (property.SetMethod != null)
				_methodSemantics[property.SetMethod] = MethodScriptSemantics.NotUsableFromScript();
		}

		private void SetGetAndSetMethods(IPropertySymbol property, MethodScriptSemantics getMethod, MethodScriptSemantics setMethod) {
			_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(getMethod, setMethod);
			if (property.GetMethod != null)
				_methodSemantics[property.GetMethod] = getMethod;
			if (property.SetMethod != null)
				_methodSemantics[property.SetMethod] = setMethod;
		}

		private void ValidateCustomInitialization(IPropertySymbol property, AttributeList attributes) {
			var cia = attributes.GetAttribute<CustomInitializationAttribute>();
			if (cia != null) {
				if (MetadataUtils.IsAutoProperty(_compilation, property) == false) {
					Message(Messages._7166, property);
				}
				else {
					if (!string.IsNullOrEmpty(cia.Code)) {
						ValidateInlineCode(MetadataUtils.CreateDummyMethodForFieldInitialization(property, _compilation), property, cia.Code, Messages._7163);
					}
				}
			}
		}

		private void ValidatePropertyImplementingInterfaceMember(IPropertySymbol property, IPropertySymbol interfaceProperty, PropertyScriptSemantics interfacePropertySemantics) {
			var attributes = _attributeStore.AttributesFor(property);
			ValidateCustomInitialization(property, attributes);

			if (interfacePropertySemantics.Type == PropertyScriptSemantics.ImplType.Field) {
				if (property.IsOverride) {
					Message(Messages._7154, property, interfaceProperty.FullyQualifiedName());
				}
				else if (property.IsOverridable()) {
					Message(Messages._7153, property, interfaceProperty.FullyQualifiedName());
				}
			
				if (MetadataUtils.IsAutoProperty(_compilation, property) == false) {
					Message(Messages._7156, property, interfaceProperty.FullyQualifiedName());
				}
			}
		}

		private void ProcessProperty(IPropertySymbol property, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			var attributes = _attributeStore.AttributesFor(property);

			ValidateCustomInitialization(property, attributes);

			if (GetTypeSemanticsInternal(property.ContainingType).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || attributes.HasAttribute<NonScriptableAttribute>() || !(property.ExplicitInterfaceImplementations.IsEmpty && property.ExplicitInterfaceImplementations.All(x => GetPropertySemantics(x).Type == PropertyScriptSemantics.ImplType.NotUsableFromScript))) {
				SetNotUsableFromScript(property);
				return;
			}
			else if (preferredName == "") {
				Message(property.IsIndexer ? Messages._7104 : Messages._7105, property);
				SetGetAndSetMethods(property, property.GetMethod != null ? MethodScriptSemantics.NormalMethod("get") : null, property.SetMethod != null ? MethodScriptSemantics.NormalMethod("set") : null);
				return;
			}
			else if (GetTypeSemanticsInternal(property.ContainingType).IsSerializable && !property.IsStatic) {
				var getica = property.GetMethod != null ? _attributeStore.AttributesFor(property.GetMethod).GetAttribute<InlineCodeAttribute>() : null;
				var setica = property.SetMethod != null ? _attributeStore.AttributesFor(property.SetMethod).GetAttribute<InlineCodeAttribute>() : null;
				if (property.GetMethod != null && property.SetMethod != null && (getica != null) != (setica != null)) {
					Message(Messages._7028, property);
				}
				else if (getica != null || setica != null) {
					bool hasError = false;
					if (property.GetMethod != null && !ValidateInlineCode(property.GetMethod, property.GetMethod, getica.Code, Messages._7130)) {
						hasError = true;
					}
					if (property.SetMethod != null && !ValidateInlineCode(property.SetMethod, property.SetMethod, setica.Code, Messages._7130)) {
						hasError = true;
					}

					if (!hasError) {
						SetGetAndSetMethods(property, getica != null ? MethodScriptSemantics.InlineCode(getica.Code) : null, setica != null ? MethodScriptSemantics.InlineCode(setica.Code) : null);
						return;
					}
				}

				if (property.IsIndexer) {
					if (property.ContainingType.TypeKind == TypeKind.Interface) {
						Message(Messages._7161, property.Locations[0]);
						SetGetAndSetMethods(property, property.GetMethod != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null, property.SetMethod != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null);
					}
					else if (property.Parameters.Length == 1) {
						SetGetAndSetMethods(property, property.GetMethod != null ? MethodScriptSemantics.NativeIndexer() : null, property.SetMethod != null ? MethodScriptSemantics.NativeIndexer() : null);
					}
					else {
						Message(Messages._7116, property.Locations[0]);
						SetGetAndSetMethods(property, property.GetMethod != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null, property.SetMethod != null ? MethodScriptSemantics.NormalMethod("X", generateCode: false) : null);
					}
				}
				else {
					string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
					usedNames[name] = true;
					SetField(property, name);
				}
				return;
			}

			var saa = attributes.GetAttribute<ScriptAliasAttribute>();

			if (saa != null) {
				if (property.IsIndexer) {
					Message(Messages._7106, property.Locations[0]);
				}
				else if (!property.IsStatic) {
					Message(Messages._7107, property);
				}
				else {
					SetGetAndSetMethods(property, property.GetMethod != null ? MethodScriptSemantics.InlineCode(saa.Alias) : null, property.SetMethod != null ? MethodScriptSemantics.InlineCode(saa.Alias + " = {value}") : null);
					return;
				}
			}

			if (attributes.HasAttribute<IntrinsicPropertyAttribute>()) {
				if (property.ContainingType.TypeKind == TypeKind.Interface) {
					if (property.IsIndexer)
						Message(Messages._7108, property.Locations[0]);
					else
						Message(Messages._7109, property);
				}
				else if (property.IsOverride && GetPropertySemantics(property.OverriddenProperty.OriginalDefinition).Type != PropertyScriptSemantics.ImplType.NotUsableFromScript) {
					if (property.IsIndexer)
						Message(Messages._7110, property.Locations[0]);
					else
						Message(Messages._7111, property);
				}
				else if (property.IsOverridable()) {
					if (property.IsIndexer)
						Message(Messages._7112, property.Locations[0]);
					else
						Message(Messages._7113, property);
				}
				else if (property.IsIndexer) {
					if (property.Parameters.Length == 1) {
						SetGetAndSetMethods(property, property.GetMethod != null ? MethodScriptSemantics.NativeIndexer() : null, property.SetMethod != null ? MethodScriptSemantics.NativeIndexer() : null);
						return;
					}
					else {
						Message(Messages._7116, property.Locations[0]);
					}
				}
				else {
					string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
					usedNames[name] = true;
					SetField(property, name);
					return;
				}
			}

			var bfn = attributes.GetAttribute<BackingFieldNameAttribute>();
			string backingFieldName = null;
			if (bfn != null) {
				if (MetadataUtils.IsAutoProperty(_compilation, property) == false) {
					Message(Messages._7167, property);
				}
				else {
					if (bfn.Name != null && bfn.Name.Replace("{owner}", "X").IsValidJavaScriptIdentifier()) {
						backingFieldName = bfn.Name;
					}
					else {
						Message(Messages._7168, property);
					}
				}
			}

			MethodScriptSemantics getter, setter;
			var getterName = property.GetMethod != null ? DeterminePreferredMemberName(property.GetMethod) : null;
			var setterName = property.SetMethod != null ? DeterminePreferredMemberName(property.SetMethod) : null;
			bool needOwner = (backingFieldName != null && backingFieldName.Contains("{owner}")) || (getterName != null && getterName.Item1 != null && getterName.Item1.Contains("{owner}")) || (setterName != null && setterName.Item1 != null && setterName.Item1.Contains("{owner}"));
			if (needOwner) {
				string owner = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
				usedNames[owner] = true;
				if (getterName != null && getterName.Item1 != null)
					getterName = Tuple.Create(getterName.Item1.Replace("{owner}", owner), getterName.Item2);
				if (setterName != null && setterName.Item1 != null)
					setterName = Tuple.Create(setterName.Item1.Replace("{owner}", owner), setterName.Item2);

				if (backingFieldName != null) {
					backingFieldName = backingFieldName.Replace("{owner}", owner);
				}
			}

			if (backingFieldName != null) {
				usedNames[backingFieldName] = true;
				_propertyBackingFieldNames[property] = Tuple.Create(backingFieldName, true);
			}

			if (property.GetMethod != null) {
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && property.ContainingType.TypeKind != TypeKind.Interface && MetadataUtils.CanBeMinimized(property, _attributeStore) ? null : (nameSpecified ? "get_" + preferredName : GetUniqueName("get_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.GetMethod, getterName.Item1, getterName.Item2, usedNames);
				getter = GetMethodSemantics(property.GetMethod);
			}
			else {
				getter = null;
			}

			if (property.SetMethod != null) {
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && property.ContainingType.TypeKind != TypeKind.Interface && MetadataUtils.CanBeMinimized(property, _attributeStore) ? null : (nameSpecified ? "set_" + preferredName : GetUniqueName("set_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.SetMethod, setterName.Item1, setterName.Item2, usedNames);
				setter = GetMethodSemantics(property.SetMethod);
			}
			else {
				setter = null;
			}

			SetGetAndSetMethods(property, getter, setter);
		}

		private void ProcessMethodImplementingInterfaceMember(IMethodSymbol method, MethodScriptSemantics interfaceMethodSemantics) {
			var attributes = _attributeStore.AttributesFor(method);

			if (attributes.HasAttribute<ScriptSkipAttribute>()) {
				Message(Messages._7122, method);
				_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
				return;
			}

			if (attributes.HasAttribute<AlternateSignatureAttribute>() || attributes.HasAttribute<ScriptNameAttribute>() || attributes.HasAttribute<PreserveNameAttribute>() || attributes.HasAttribute<PreserveCaseAttribute>()) {
				Message(Messages._7135, method);
				_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
				return;
			}

			var ica = attributes.GetAttribute<InlineCodeAttribute>();
			if (ica != null) {
				string code = ica.Code ?? "", nonVirtualCode = ica.NonVirtualCode, nonExpandedFormCode = ica.NonExpandedFormCode;

				if (method.IsOverridable() && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
					Message(Messages._7128, method);
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
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
							Message(Messages._7029, method.Locations[0], "method", method.FullyQualifiedName());
							code = nonVirtualCode = nonExpandedFormCode = "X";
						}
						if (!ValidateInlineCode(method, method, ica.NonExpandedFormCode, Messages._7130)) {
							code = nonVirtualCode = nonExpandedFormCode = "X";
						}
					}
				}

				_methodSemantics[method] = MethodScriptSemantics.InlineCode(code, enumerateAsArray: attributes.HasAttribute<EnumerateAsArrayAttribute>(), generatedMethodName: ica.GeneratedMethodName, nonVirtualInvocationLiteralCode: nonVirtualCode, nonExpandedFormLiteralCode: nonExpandedFormCode);
			}
			else {
				var sem = interfaceMethodSemantics;
				if (sem.Type == MethodScriptSemantics.ImplType.InlineCode && sem.GeneratedMethodName != null)
					sem = MethodScriptSemantics.NormalMethod(sem.GeneratedMethodName, generateCode: !attributes.HasAttribute<DontGenerateAttribute>(), ignoreGenericArguments: sem.IgnoreGenericArguments, expandParams: sem.ExpandParams);	// Methods implementing methods with [InlineCode(..., GeneratedMethodName = "Something")] are treated as normal methods.
				if (attributes.HasAttribute<EnumerateAsArrayAttribute>())
					sem = sem.WithEnumerateAsArray();
				_methodSemantics[method] = sem;
			}
		}

		private void ProcessMethod(IMethodSymbol method, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			var attributes = _attributeStore.AttributesFor(method);
			var eaa = attributes.GetAttribute<EnumerateAsArrayAttribute>();
			var ssa = attributes.GetAttribute<ScriptSkipAttribute>();
			var saa = attributes.GetAttribute<ScriptAliasAttribute>();
			var ica = attributes.GetAttribute<InlineCodeAttribute>();
			var ifa = attributes.GetAttribute<InstanceMethodOnFirstArgumentAttribute>();
			var nsa = attributes.GetAttribute<NonScriptableAttribute>();
			var ioa = attributes.GetAttribute<IntrinsicOperatorAttribute>();
			var epa = attributes.GetAttribute<ExpandParamsAttribute>();
			var asa = attributes.GetAttribute<AlternateSignatureAttribute>();
			bool generateCode = !attributes.HasAttribute<DontGenerateAttribute>() && !attributes.HasAttribute<AlternateSignatureAttribute>();

			bool? includeGenericArguments = method.TypeParameters.Length > 0 ? MetadataUtils.ShouldGenericArgumentsBeIncluded(method, _attributeStore) : false;

			if (eaa != null && (method.MetadataName != "GetEnumerator" || method.IsStatic || method.TypeParameters.Length > 0 || method.Parameters.Length > 0)) {
				Message(Messages._7151, method);
				eaa = null;
			}

			if (nsa != null || GetTypeSemanticsInternal(method.ContainingType).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || !(method.ExplicitInterfaceImplementations.IsEmpty && method.ExplicitInterfaceImplementations.All(x => GetMethodSemantics(x).Type == MethodScriptSemantics.ImplType.NotUsableFromScript))) {
				_methodSemantics[method] = MethodScriptSemantics.NotUsableFromScript();
				return;
			}
			if (ioa != null) {
				if (method.MethodKind != MethodKind.UserDefinedOperator && method.MethodKind != MethodKind.Conversion) {
					Message(Messages._7117, method);
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
				}
				else if (method.MetadataName == "op_Implicit" || method.MetadataName == "op_Explicit") {
					Message(Messages._7118, method);
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
				}
				else {
					_methodSemantics[method] = MethodScriptSemantics.NativeOperator();
				}
				return;
			}
			else {
				if (ssa != null) {
					// [ScriptSkip] - Skip invocation of the method entirely.
					if (method.ContainingType.TypeKind == TypeKind.Interface) {
						Message(Messages._7119, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
					else if (method.IsOverride && GetMethodSemantics(method.OverriddenMethod.OriginalDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(Messages._7120, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
					else if (method.IsOverridable()) {
						Message(Messages._7121, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
					else if (method.IsStatic) {
						if (method.Parameters.Length != 1) {
							Message(Messages._7123, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
							return;
						}
						_methodSemantics[method] = MethodScriptSemantics.InlineCode("{" + method.Parameters[0].Name + "}", enumerateAsArray: eaa != null);
						return;
					}
					else {
						if (method.Parameters.Length != 0) {
							Message(Messages._7124, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
							return;
						}
						_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}", enumerateAsArray: eaa != null);
						return;
					}
				}
				else if (saa != null) {
					if (method.IsStatic) {
						_methodSemantics[method] = MethodScriptSemantics.InlineCode(saa.Alias + "(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")");
						return;
					}
					else {
						Message(Messages._7125, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
				}
				else if (ica != null) {
					string code = ica.Code ?? "", nonVirtualCode = ica.NonVirtualCode, nonExpandedFormCode = ica.NonExpandedFormCode;

					if (method.ContainingType.TypeKind == TypeKind.Interface && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
						Message(Messages._7126, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
					else if (method.IsOverride && GetMethodSemantics(method.OverriddenMethod.OriginalDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(Messages._7127, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
					else if (method.IsOverridable() && string.IsNullOrEmpty(ica.GeneratedMethodName)) {
						Message(Messages._7128, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
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
								Message(Messages._7029, method.Locations[0], "method", method.FullyQualifiedName());
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
						if (method.Parameters.Length == 0) {
							Message(Messages._7149, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
							return;
						}
						else if (method.Parameters[0].IsParams) {
							Message(Messages._7150, method);
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
							return;
						}
						else {
							var sb = new StringBuilder();
							sb.Append("{" + method.Parameters[0].Name + "}." + preferredName + "(");
							for (int i = 1; i < method.Parameters.Length; i++) {
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
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
						return;
					}
				}
				else {
					if (method.IsOverride && GetMethodSemantics(method.OverriddenMethod.OriginalDefinition).Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						if (nameSpecified) {
							Message(Messages._7132, method);
						}
						if (attributes.HasAttribute<IncludeGenericArgumentsAttribute>()) {
							Message(Messages._7133, method);
						}
					
						var semantics = GetMethodSemantics(method.OverriddenMethod.OriginalDefinition);
						if (semantics.Type == MethodScriptSemantics.ImplType.InlineCode && semantics.GeneratedMethodName != null)
							semantics = MethodScriptSemantics.NormalMethod(semantics.GeneratedMethodName, generateCode: generateCode, ignoreGenericArguments: semantics.IgnoreGenericArguments, expandParams: semantics.ExpandParams);	// Methods derived from methods with [InlineCode(..., GeneratedMethodName = "Something")] are treated as normal methods.
						if (eaa != null)
							semantics = semantics.WithEnumerateAsArray();
					
						_methodSemantics[method] = semantics;
						return;
					}
					else {
						if (includeGenericArguments == null) {
							_errorReporter.Location = method.Locations[0];
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
							if (method.ContainingType.TypeKind == TypeKind.Interface) {
								Message(Messages._7138, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
								return;
							}
							else if (method.IsOverridable()) {
								Message(Messages._7139, method);
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.MetadataName);
								return;
							}
							else {
								_methodSemantics[method] = MethodScriptSemantics.InlineCode((method.IsStatic ? "{$" + method.ContainingType.FullyQualifiedName() + "}" : "{this}") + "(" + string.Join(", ", method.Parameters.Select(p => "{" + (p.IsParams && epa != null ? "*" : "") + p.Name + "}")) + ")", enumerateAsArray: eaa != null);
								return;
							}
						}
						else {
							string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
							if (asa == null)
								usedNames[name] = true;
							if (GetTypeSemanticsInternal(method.ContainingType).IsSerializable && !method.IsStatic) {
								_methodSemantics[method] = MethodScriptSemantics.StaticMethodWithThisAsFirstArgument(name, generateCode: generateCode, ignoreGenericArguments: !includeGenericArguments.Value, expandParams: epa != null, enumerateAsArray: eaa != null);
							}
							else {
								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(name, generateCode: generateCode, ignoreGenericArguments: !includeGenericArguments.Value, expandParams: epa != null, enumerateAsArray: eaa != null);
							}
						}
					}
				}
			}
		}

		private void ProcessEvent(IEventSymbol evt, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			var attributes = _attributeStore.AttributesFor(evt);

			var cia = attributes.GetAttribute<CustomInitializationAttribute>();
			if (cia != null) {
				if (MetadataUtils.IsAutoEvent(_compilation, evt) == false) {
					Message(Messages._7165, evt);
				}
				else {
					if (!string.IsNullOrEmpty(cia.Code)) {
						ValidateInlineCode(MetadataUtils.CreateDummyMethodForFieldInitialization(evt, _compilation), evt, cia.Code, Messages._7163);
					}
				}
			}

			if (GetTypeSemanticsInternal(evt.ContainingType).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || attributes.HasAttribute<NonScriptableAttribute>()) {
				_eventSemantics[evt] = EventScriptSemantics.NotUsableFromScript();
				return;
			}
			else if (preferredName == "") {
				Message(Messages._7141, evt);
				_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add"), MethodScriptSemantics.NormalMethod("remove"));
				return;
			}

			var bfn = attributes.GetAttribute<BackingFieldNameAttribute>();
			string backingFieldName = null;
			if (bfn != null) {
				if (MetadataUtils.IsAutoEvent(_compilation, evt) == false) {
					Message(Messages._7169, evt);
				}
				else {
					if (bfn.Name != null && bfn.Name.Replace("{owner}", "X").IsValidJavaScriptIdentifier()) {
						backingFieldName = bfn.Name;
					}
					else {
						Message(Messages._7170, evt);
					}
				}
			}

			MethodScriptSemantics adder, remover;
			var adderName = evt.AddMethod != null ? DeterminePreferredMemberName(evt.AddMethod) : null;
			var removerName = evt.RemoveMethod != null ? DeterminePreferredMemberName(evt.RemoveMethod) : null;
			bool needOwner = (backingFieldName != null && backingFieldName.Contains("{owner}")) || (adderName != null && adderName.Item1 != null && adderName.Item1.Contains("{owner}")) || (removerName != null && removerName.Item1 != null && removerName.Item1.Contains("{owner}"));
			if (needOwner) {
				string owner = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
				usedNames[owner] = true;
				if (adderName != null)
					adderName = Tuple.Create(adderName.Item1.Replace("{owner}", owner), adderName.Item2);
				if (removerName != null)
					removerName = Tuple.Create(removerName.Item1.Replace("{owner}", owner), removerName.Item2);

				if (backingFieldName != null) {
					backingFieldName = backingFieldName.Replace("{owner}", owner);
				}
			}

			if (backingFieldName != null) {
				usedNames[backingFieldName] = true;
				_eventBackingFieldNames[evt] = Tuple.Create(backingFieldName, true);
			}

			if (evt.AddMethod != null) {
				if (!adderName.Item2)
					adderName = Tuple.Create(!nameSpecified && _minimizeNames && evt.ContainingType.TypeKind != TypeKind.Interface && MetadataUtils.CanBeMinimized(evt, _attributeStore) ? null : (nameSpecified ? "add_" + preferredName : GetUniqueName("add_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.AddMethod, adderName.Item1, adderName.Item2, usedNames);
				adder = GetMethodSemantics(evt.AddMethod);
			}
			else {
				adder = null;
			}

			if (evt.RemoveMethod != null) {
				if (!removerName.Item2)
					removerName = Tuple.Create(!nameSpecified && _minimizeNames && evt.ContainingType.TypeKind != TypeKind.Interface && MetadataUtils.CanBeMinimized(evt, _attributeStore) ? null : (nameSpecified ? "remove_" + preferredName : GetUniqueName("remove_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.RemoveMethod, removerName.Item1, removerName.Item2, usedNames);
				remover = GetMethodSemantics(evt.RemoveMethod);
			}
			else {
				remover = null;
			}

			_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(adder, remover);
		}

		private void ProcessField(IFieldSymbol field, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			var attributes = _attributeStore.AttributesFor(field);

			var cia = attributes.GetAttribute<CustomInitializationAttribute>();
			if (cia != null) {
				if (field.IsConst) {
					Message(Messages._7164, field);
				}
				else {
					if (!string.IsNullOrEmpty(cia.Code)) {
						ValidateInlineCode(MetadataUtils.CreateDummyMethodForFieldInitialization(field, _compilation), field, cia.Code, Messages._7163);
					}
				}
			}

			if (GetTypeSemanticsInternal(field.ContainingType).Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || attributes.HasAttribute<NonScriptableAttribute>()) {
				_fieldSemantics[field] = FieldScriptSemantics.NotUsableFromScript();
			}
			else if (preferredName == "") {
				Message(Messages._7142, field);
				_fieldSemantics[field] = FieldScriptSemantics.Field("X");
			}
			else {
				string name = (nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames));
				if (attributes.HasAttribute<InlineConstantAttribute>()) {
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

				if (attributes.HasAttribute<NoInlineAttribute>() && !field.IsConst) {
					Message(Messages._7160, field);
				}

				if (GetTypeSemanticsInternal(field.ContainingType).IsNamedValues) {
					string value = preferredName;
					if (!nameSpecified) {	// This code handles the feature that it is possible to specify an invalid ScriptName for a member of a NamedValues enum, in which case that value has to be use as the constant value.
						var sna = attributes.GetAttribute<ScriptNameAttribute>();
						if (sna != null)
							value = sna.Name;
					}

					_fieldSemantics[field] = FieldScriptSemantics.StringConstant(value, name);
				}
				else if (field.ContainingType.TypeKind == TypeKind.Enum && _attributeStore.AttributesFor(field.ContainingType).HasAttribute<ImportedAttribute>() && _attributeStore.AttributesFor(field.ContainingType).HasAttribute<ScriptNameAttribute>()) {
					// Fields of enums that are imported and have an explicit [ScriptName] are treated as normal fields.
					_fieldSemantics[field] = FieldScriptSemantics.Field(name);
				}
				else if (name == null || (field.IsConst && !attributes.HasAttribute<NoInlineAttribute>() && (field.ContainingType.TypeKind == TypeKind.Enum || _minimizeNames))) {
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

		public void Prepare(INamedTypeSymbol type) {
			_prev.Prepare(type);

			if (Equals(type.ContainingAssembly, _compilation.Assembly)) {
				try {
					ProcessType(type);
					ProcessTypeMembers(type);
				}
				catch (Exception ex) {
					_errorReporter.Location = type.Locations[0];
					_errorReporter.InternalError(ex, "Error importing type " + type.FullyQualifiedName());
				}
			}
		}

		public void ReserveMemberName(INamedTypeSymbol type, string name, bool isStatic) {
			if (!Equals(type.ContainingAssembly, _compilation.Assembly)) {
				_prev.ReserveMemberName(type, name, isStatic);
				return;
			}

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

		public bool IsMemberNameAvailable(INamedTypeSymbol type, string name, bool isStatic) {
			if (type.ContainingAssembly != _compilation.Assembly)
				return _prev.IsMemberNameAvailable(type, name, isStatic);

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
				if (type.BaseType != null && !IsMemberNameAvailable(type.BaseType.OriginalDefinition, name, false))
					return false;
				if (type.AllInterfaces.Any(t => !IsMemberNameAvailable(t.OriginalDefinition, name, false)))
					return false;
				HashSet<string> names;
				if (!_instanceMemberNamesByType.TryGetValue(type, out names))
					return true;
				return !names.Contains(name);
			}
		}

		public void SetMethodSemantics(IMethodSymbol method, MethodScriptSemantics semantics) {
			if (!Equals(method.ContainingAssembly, _compilation.Assembly)) {
				_prev.SetMethodSemantics(method, semantics);
				return;
			}

			_methodSemantics[method] = semantics;
			_ignoredMembers.Add(method);
		}

		public void SetConstructorSemantics(IMethodSymbol method, ConstructorScriptSemantics semantics) {
			if (!Equals(method.ContainingAssembly, _compilation.Assembly)) {
				_prev.SetConstructorSemantics(method, semantics);
				return;
			}

			_constructorSemantics[method] = semantics;
			_ignoredMembers.Add(method);
		}

		public void SetPropertySemantics(IPropertySymbol property, PropertyScriptSemantics semantics) {
			if (!Equals(property.ContainingAssembly, _compilation.Assembly)) {
				_prev.SetPropertySemantics(property, semantics);
				return;
			}

			_propertySemantics[property] = semantics;
			_ignoredMembers.Add(property);
		}

		public void SetFieldSemantics(IFieldSymbol field, FieldScriptSemantics semantics) {
			if (!Equals(field.ContainingAssembly, _compilation.Assembly)) {
				_prev.SetFieldSemantics(field, semantics);
				return;
			}

			_fieldSemantics[field] = semantics;
			_ignoredMembers.Add(field);
		}

		public void SetEventSemantics(IEventSymbol evt, EventScriptSemantics semantics) {
			if (!Equals(evt.ContainingAssembly, _compilation.Assembly)) {
				_prev.SetEventSemantics(evt, semantics);
				return;
			}

			_eventSemantics[evt] = semantics;
			_ignoredMembers.Add(evt);
		}

		private TypeSemantics GetTypeSemanticsInternal(INamedTypeSymbol typeDefinition) {
			TypeSemantics ts;
			_typeSemantics.TryGetValue(typeDefinition, out ts);
			return ts;
		}

		public TypeScriptSemantics GetTypeSemantics(INamedTypeSymbol typeDefinition) {
			if (!Equals(typeDefinition.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetTypeSemantics(typeDefinition);
			}

			if (typeDefinition.TypeKind == TypeKind.Delegate)
				return TypeScriptSemantics.NormalType("Function");
			else if (typeDefinition.TypeKind == TypeKind.ArrayType)
				return TypeScriptSemantics.NormalType("Array");
			var s = GetTypeSemanticsInternal(typeDefinition);
			return s != null ? s.Semantics : null;
		}

		public MethodScriptSemantics GetMethodSemantics(IMethodSymbol method) {
			if (!Equals(method.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetMethodSemantics(method);
			}

			switch (method.ContainingType.TypeKind) {
				case TypeKind.Delegate:
					return MethodScriptSemantics.NotUsableFromScript();
				default:
					MethodScriptSemantics result;
					_methodSemantics.TryGetValue((IMethodSymbol)method.OriginalDefinition, out result);
					return result;
			}
		}

		public ConstructorScriptSemantics GetConstructorSemantics(IMethodSymbol method) {
			if (!Equals(method.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetConstructorSemantics(method);
			}

			if (method.ContainingType.IsAnonymousType) {
				throw new ArgumentException("Should not call GetConstructorSemantics for anonymous type constructor");
			}
			else if (method.ContainingType.TypeKind == TypeKind.Delegate) {
				return ConstructorScriptSemantics.NotUsableFromScript();
			}
			else {
				ConstructorScriptSemantics result;
				_constructorSemantics.TryGetValue((IMethodSymbol)method.OriginalDefinition, out result);
				return result;
			}
		}

		public PropertyScriptSemantics GetPropertySemantics(IPropertySymbol property) {
			if (!Equals(property.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetPropertySemantics(property);
			}

			if (property.ContainingType.IsAnonymousType) {
				return PropertyScriptSemantics.Field(property.MetadataName.Replace("<>", "$"));
			}
			else if (property.ContainingType.TypeKind == TypeKind.Delegate) {
				return PropertyScriptSemantics.NotUsableFromScript();
			}
			else {
				PropertyScriptSemantics result;
				_propertySemantics.TryGetValue((IPropertySymbol)property.OriginalDefinition, out result);
				return result;
			}
		}

		public DelegateScriptSemantics GetDelegateSemantics(INamedTypeSymbol delegateType) {
			if (!Equals(delegateType.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetDelegateSemantics(delegateType);
			}

			DelegateScriptSemantics result;
			_delegateSemantics.TryGetValue(delegateType, out result);
			return result;
		}

		private string GetBackingFieldName(INamedTypeSymbol containingType, string memberName) {
			int inheritanceDepth = containingType.GetSelfAndAllBaseTypes().Count(b => b.TypeKind != TypeKind.Interface) - 1;

			if (_minimizeNames) {
				int count;
				_backingFieldCountPerType.TryGetValue(containingType, out count);
				count++;
				_backingFieldCountPerType[containingType] = count;
				return string.Format(CultureInfo.InvariantCulture, "${0}${1}", inheritanceDepth, count);
			}
			else {
				return string.Format(CultureInfo.InvariantCulture, "${0}${1}Field", inheritanceDepth, memberName);
			}
		}

		public string GetAutoPropertyBackingFieldName(IPropertySymbol property) {
			if (!Equals(property.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetAutoPropertyBackingFieldName(property);
			}

			property = (IPropertySymbol)property.OriginalDefinition;
			Tuple<string, bool> result;
			if (_propertyBackingFieldNames.TryGetValue(property, out result))
				return result.Item1;
			result = Tuple.Create(GetBackingFieldName(property.ContainingType, property.MetadataName), false);
			_propertyBackingFieldNames[property] = result;
			return result.Item1;
		}

		public bool ShouldGenerateAutoPropertyBackingField(IPropertySymbol property) {
			if (!Equals(property.ContainingAssembly, _compilation.Assembly)) {
				return _prev.ShouldGenerateAutoPropertyBackingField(property);
			}

			var impl = GetPropertySemantics(property);
			if (impl.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods && ((impl.GetMethod != null && impl.GetMethod.GeneratedMethodName != null) || (impl.SetMethod != null && impl.SetMethod.GeneratedMethodName != null)))
				return true;

			Tuple<string, bool> result;
			return _propertyBackingFieldNames.TryGetValue(property, out result) && result.Item2;
		}

		public FieldScriptSemantics GetFieldSemantics(IFieldSymbol field) {
			if (!Equals(field.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetFieldSemantics(field);
			}

			switch (field.ContainingType.TypeKind) {
				case TypeKind.Delegate:
					return FieldScriptSemantics.NotUsableFromScript();
				default:
					FieldScriptSemantics result;
					_fieldSemantics.TryGetValue((IFieldSymbol)field.OriginalDefinition, out result);
					return result;
			}
		}

		public EventScriptSemantics GetEventSemantics(IEventSymbol evt) {
			if (!Equals(evt.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetEventSemantics(evt);
			}

			switch (evt.ContainingType.TypeKind) {
				case TypeKind.Delegate:
					return EventScriptSemantics.NotUsableFromScript();
				default:
					EventScriptSemantics result;
					_eventSemantics.TryGetValue((IEventSymbol)evt.OriginalDefinition, out result);
					return result;
			}
		}

		public string GetAutoEventBackingFieldName(IEventSymbol evt) {
			if (!Equals(evt.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetAutoEventBackingFieldName(evt);
			}

			evt = (IEventSymbol)evt.OriginalDefinition;
			Tuple<string, bool> result;
			if (_eventBackingFieldNames.TryGetValue(evt, out result))
				return result.Item1;
			result = Tuple.Create(GetBackingFieldName(evt.ContainingType, evt.MetadataName), false);
			_eventBackingFieldNames[evt] = result;
			return result.Item1;
		}

		public bool ShouldGenerateAutoEventBackingField(IEventSymbol evt) {
			if (!Equals(evt.ContainingAssembly, _compilation.Assembly)) {
				return _prev.ShouldGenerateAutoEventBackingField(evt);
			}

			var impl = GetEventSemantics(evt);
			if (impl.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods && ((impl.AddMethod != null && impl.AddMethod.GeneratedMethodName != null) || (impl.RemoveMethod != null && impl.RemoveMethod.GeneratedMethodName != null)))
				return true;

			Tuple<string, bool> result;
			return _eventBackingFieldNames.TryGetValue(evt, out result) && result.Item2;
		}

		public IReadOnlyList<string> GetUsedInstanceMemberNames(INamedTypeSymbol type) {
			if (!Equals(type.ContainingAssembly, _compilation.Assembly)) {
				return _prev.GetUsedInstanceMemberNames(type);
			}

			IEnumerable<string> result = _instanceMemberNamesByType[type];
			if (type.BaseType != null)
				result = result.Concat(GetUsedInstanceMemberNames(type.BaseType)).Distinct();
			return ImmutableArray.CreateRange(result);
		}
	}
}
