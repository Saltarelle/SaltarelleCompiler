using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	public class ScriptSharpMetadataImporter : INamingConventionResolver {
		private const string ScriptSkipAttribute = "ScriptSkipAttribute";
		private const string ScriptAliasAttribute = "ScriptAliasAttribute";
		private const string InlineCodeAttribute = "InlineCodeAttribute";
		private const string InstanceMethodOnFirstArgumentAttribute = "InstanceMethodOnFirstArgumentAttribute";
		private const string NonScriptableAttribute = "NonScriptableAttribute";
		private const string IgnoreGenericArgumentsAttribute = "IgnoreGenericArgumentsAttribute";
		private const string IgnoreNamespaceAttribute = "IgnoreNamespaceAttribute";
		private const string ScriptNamespaceAttribute = "ScriptNamespaceAttribute";
		private const string AlternateSignatureAttribute = "AlternateSignatureAttribute";
		private const string ScriptNameAttribute = "ScriptNameAttribute";
		private const string PreserveNameAttribute = "PreserveNameAttribute";
		private const string PreserveCaseAttribute = "PreserveCaseAttribute";
		private const string IntrinsicPropertyAttribute = "IntrinsicPropertyAttribute";
		private const string GlobalMethodsAttribute = "GlobalMethodsAttribute";
		private const string ImportedAttribute = "ImportedAttribute";
		private const string RecordAttribute = "RecordAttribute";
		private const string IntrinsicOperatorAttribute = "IntrinsicOperatorAttribute";
		private const string ExpandParamsAttribute = "ExpandParamsAttribute";
		private const string NamedValuesAttribute = "NamedValuesAttribute";
		private const string Function = "Function";
		private const string Array = "Array";

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

				return string.CompareOrdinal(xparms, yparms);
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
			public bool GlobalMethods { get; private set; }
			public bool IsRecord { get; private set; }
			public bool IsNamedValues { get; private set; }

			public TypeSemantics(TypeScriptSemantics semantics, bool globalMethods, bool isRecord, bool isNamedValues) {
				Semantics     = semantics;
				GlobalMethods = globalMethods;
				IsRecord      = isRecord;
				IsNamedValues = isNamedValues;
			}
		}

		private Dictionary<ITypeDefinition, TypeSemantics> _typeSemantics;
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
		private Dictionary<string, string> _errors;
		private Dictionary<IAssembly, int> _internalInterfaceMemberCountPerAssembly;
		private IType _systemObject;
		private IType _systemRecord;

		private readonly bool _minimizeNames;

		public ScriptSharpMetadataImporter(bool minimizeNames) {
			_minimizeNames = minimizeNames;
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

		public static string EncodeNumber(int i, bool allowDigitFirst) {
			if (allowDigitFirst) {
				string result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1);
				while (i >= _encodeNumberTable.Length) {
					i /= _encodeNumberTable.Length;
					result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1) + result;
				}
				return result;
			}
			else {
				string result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1);
				while (i >= _encodeNumberTable.Length - 10) {
					i /= _encodeNumberTable.Length - 10;
					result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1) + result;
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

		private IList<object> GetAttributePositionalArgs(IEntity entity, string attributeName) {
			attributeName = "System.Runtime.CompilerServices." + attributeName;
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == attributeName);
			return attr != null ? attr.PositionalArguments.Select(arg => arg.ConstantValue).ToList() : null;
		}

		private string DetermineNamespace(ITypeDefinition typeDefinition) {
			while (typeDefinition.DeclaringTypeDefinition != null) {
				typeDefinition = typeDefinition.DeclaringTypeDefinition;
			}

			var ina = GetAttributePositionalArgs(typeDefinition, IgnoreNamespaceAttribute);
			var sna = GetAttributePositionalArgs(typeDefinition, ScriptNamespaceAttribute);
			if (ina != null) {
				if (sna != null) {
					_errors[typeDefinition.FullName + ":Namespace"] = "The type " + typeDefinition.FullName + " has both [IgnoreNamespace] and [ScriptNamespace] specified. At most one of these attributes can be specified for a type.";
					return typeDefinition.FullName;
				}
				else {
					return "";
				}
			}
			else {
				if (sna != null) {
					string arg = (string)sna[0];
					if (arg == null || (arg != "" && !arg.IsValidNestedJavaScriptIdentifier()))
						_errors[typeDefinition.FullName + ":Namespace"] = typeDefinition.FullName + ": The argument for [ScriptNamespace], when applied to a type, must be a valid JavaScript qualified identifier.";
					return arg;
				}
				else {
					return typeDefinition.Namespace;
				}
			}
		}

		private Tuple<string, string> SplitName(string typeName) {
			int dot = typeName.LastIndexOf('.');
			return dot > 0 ? Tuple.Create(typeName.Substring(0, dot), typeName.Substring(dot + 1)) : Tuple.Create("", typeName);
		}

		private void ProcessType(ITypeDefinition typeDefinition) {
			if (_typeSemantics.ContainsKey(typeDefinition))
				return;

			if (GetAttributePositionalArgs(typeDefinition, NonScriptableAttribute) != null || typeDefinition.DeclaringTypeDefinition != null && GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NotUsableFromScript(), false, false, false);
				return;
			}

			var scriptNameAttr = GetAttributePositionalArgs(typeDefinition, ScriptNameAttribute);
			bool isImported = GetAttributePositionalArgs(typeDefinition, ImportedAttribute) != null;
			bool preserveName = isImported || GetAttributePositionalArgs(typeDefinition, PreserveNameAttribute) != null;

			bool ignoreGenericArguments = GetAttributePositionalArgs(typeDefinition, IgnoreGenericArgumentsAttribute) != null;

			string typeName, nmspace;
			if (scriptNameAttr != null && scriptNameAttr[0] != null && ((string)scriptNameAttr[0]).IsValidJavaScriptIdentifier()) {
				typeName = (string)scriptNameAttr[0];
				nmspace = DetermineNamespace(typeDefinition);
			}
			else {
				if (scriptNameAttr != null) {
					_errors[typeDefinition.FullName + ":Name"] = typeDefinition.FullName + ": The argument for [ScriptName], when applied to a type, must be a valid JavaScript identifier.";
				}

				if (_minimizeNames && !Utils.IsPublic(typeDefinition) && !preserveName) {
					nmspace = DetermineNamespace(typeDefinition);
					int index = _typeSemantics.Values.Where(ts => ts.Semantics.Type == TypeScriptSemantics.ImplType.NormalType).Select(ts => SplitName(ts.Semantics.Name)).Count(tn => tn.Item1 == nmspace && tn.Item2.StartsWith("$"));
					typeName = "$" + index.ToString(CultureInfo.InvariantCulture);
				}
				else {
					typeName = GetDefaultTypeName(typeDefinition, ignoreGenericArguments);
					if (typeDefinition.DeclaringTypeDefinition != null) {
						if (GetAttributePositionalArgs(typeDefinition, IgnoreNamespaceAttribute) != null || GetAttributePositionalArgs(typeDefinition, ScriptNamespaceAttribute) != null) {
							_errors[typeDefinition.FullName + ":Namespace"] = "[IgnoreNamespace] or [ScriptNamespace] cannot be specified for the nested type " + typeDefinition.FullName + ".";
						}

						typeName = GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Name + "$" + typeName;
						nmspace = "";
					}
					else {
						nmspace = DetermineNamespace(typeDefinition);
					}

					if (!Utils.IsPublic(typeDefinition) && !preserveName && !typeName.StartsWith("$")) {
						typeName = "$" + typeName;
					}
				}
			}

			bool hasRecordAttr = GetAttributePositionalArgs(typeDefinition, RecordAttribute) != null;
			bool inheritsRecord = typeDefinition.GetAllBaseTypeDefinitions().Any(td => td == _systemRecord) && typeDefinition != _systemRecord;
			
			bool globalMethods = false, isRecord = hasRecordAttr || inheritsRecord;

			if (isRecord) {
				if (!typeDefinition.IsSealed) {
					_errors[typeDefinition.FullName + ":RecordMustBeSealed"] = "The record type " + typeDefinition.FullName + " must be sealed.";
					isRecord = false;
				}
				if (!typeDefinition.DirectBaseTypes.Contains(_systemObject) && !typeDefinition.DirectBaseTypes.Contains(_systemRecord)) {
					_errors[typeDefinition.FullName + ":MustInheritFromObject"] = "The record type " + typeDefinition.FullName + " must inherit from either System.Object or System.Record.";
					isRecord = false;
				}
				if (typeDefinition.DirectBaseTypes.Any(b => b.Kind == TypeKind.Interface)) {
					_errors[typeDefinition.FullName + ":RecordCannotImplementInterface"] = "The record type " + typeDefinition.FullName + " cannot implement interfaces.";
					isRecord = false;
				}
				if (typeDefinition.Events.Any(evt => !evt.IsStatic)) {
					_errors[typeDefinition.FullName + ":RecordCannotImplementInterface"] = "The record type " + typeDefinition.FullName + " cannot declare instance events.";
					isRecord = false;
				}
			}
			else {
				var globalMethodsAttr = GetAttributePositionalArgs(typeDefinition, GlobalMethodsAttribute);
				if (globalMethodsAttr != null) {
					if (!typeDefinition.IsStatic) {
						_errors[typeDefinition.FullName + ":GlobalMethods"] = "The type " + typeDefinition.FullName + " must be static in order to be decorated with a [GlobalMethodsAttribute]";
					}
					else if (typeDefinition.Fields.Any() || typeDefinition.Events.Any() || typeDefinition.Properties.Any()) {
						_errors[typeDefinition.FullName + ":GlobalMethods"] = "The type " + typeDefinition.FullName + " cannot have any fields, events or properties in order to be decorated with a [GlobalMethodsAttribute]";
					}
					else if (typeDefinition.DeclaringTypeDefinition != null) {
						_errors[typeDefinition.FullName + ":GlobalMethods"] = "[GlobalMethodsAttribute] cannot be applied to the nested type " + typeDefinition.FullName + ".";
					}
					else {
						nmspace = "";
						globalMethods = true;
					}
				}
			}

			for (int i = 0; i < typeDefinition.TypeParameterCount; i++) {
				var tp = typeDefinition.TypeParameters[i];
				_typeParameterNames[tp] = _minimizeNames ? EncodeNumber(i, false) : tp.Name;
			}

			var nva = GetAttributePositionalArgs(typeDefinition, NamedValuesAttribute);
			_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NormalType(!string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName, ignoreGenericArguments: ignoreGenericArguments, generateCode: !isImported), globalMethods: globalMethods, isRecord: isRecord, isNamedValues: nva != null);
		}

		private HashSet<string> GetInstanceMemberNames(ITypeDefinition typeDefinition, ICompilation compilation) {
			ProcessType(typeDefinition);
			HashSet<string> result;
			if (!_instanceMemberNamesByType.TryGetValue(typeDefinition, out result))
				ProcessTypeMembers(typeDefinition, compilation);
			return _instanceMemberNamesByType[typeDefinition];
		}

		private HashSet<string> GetInstanceMemberNames(IEnumerable<ITypeDefinition> typeDefinitions, ICompilation compilation) {
			var result = new HashSet<string>();
			foreach (var n in typeDefinitions.SelectMany(t => GetInstanceMemberNames(t, compilation))) {
				result.Add(n);
			}
			return result;
		}

		private Tuple<string, bool> DeterminePreferredMemberName(IMember member) {
			var asa = GetAttributePositionalArgs(member, AlternateSignatureAttribute);
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && GetAttributePositionalArgs(m, AlternateSignatureAttribute) == null).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0]);
				}
				else {
					_errors[GetQualifiedMemberName(member) + ":NoMainMethod"] = "The member " + GetQualifiedMemberName(member) + " has an [AlternateSignatureAttribute], but there is not exactly one other method with the same name that does not have that attribute.";
					return Tuple.Create(member.Name, false);
				}
			}

			bool isConstructor = member is IMethod && ((IMethod)member).IsConstructor;
			bool isAccessor = member is IMethod && ((IMethod)member).IsAccessor;

			var sna = GetAttributePositionalArgs(member, ScriptNameAttribute);
			if (sna != null) {
				string name = (string)sna[0] ?? "";
				if (name != "" && !name.IsValidJavaScriptIdentifier()) {
					_errors[GetQualifiedMemberName(member) + ":InvalidName"] = "The name specified in the [ScriptName] attribute for member " + GetQualifiedMemberName(member) + " must be a valid JavaScript identifier, or be blank.";
				}
				if (name == "" && isConstructor)
					name = "$ctor";
				return Tuple.Create(name, true);
			}
			var pca = GetAttributePositionalArgs(member, PreserveCaseAttribute);
			if (pca != null)
				return Tuple.Create(member.Name, true);

			bool preserveName = (!isConstructor && !isAccessor && (   GetAttributePositionalArgs(member, PreserveNameAttribute) != null
			                                                       || GetAttributePositionalArgs(member, InstanceMethodOnFirstArgumentAttribute) != null
			                                                       || GetAttributePositionalArgs(member, IntrinsicPropertyAttribute) != null
			                                                       || _typeSemantics[member.DeclaringTypeDefinition].GlobalMethods
			                                                       || (!_typeSemantics[member.DeclaringTypeDefinition].Semantics.GenerateCode && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (_typeSemantics[member.DeclaringTypeDefinition].IsRecord && !member.IsStatic && (member is IProperty || member is IField)))
			                                                       || (_typeSemantics[member.DeclaringTypeDefinition].IsNamedValues && member is IField));

			if (preserveName)
				return Tuple.Create(MakeCamelCase(member.Name), true);

			if (isConstructor) {
				return Tuple.Create("$ctor", false);
			}
			if (Utils.IsPublic(member)) {
				return Tuple.Create(MakeCamelCase(member.Name), false);
			}
			else {
				if (_minimizeNames)
					return Tuple.Create((string)null, false);
				else
					return Tuple.Create("$" + MakeCamelCase(member.Name), false);
			}
		}

		public string GetQualifiedMemberName(IMember member) {
			return member.DeclaringType.FullName + "." + member.Name;
		}

		private void ProcessTypeMembers(ITypeDefinition typeDefinition, ICompilation compilation) {
			var instanceMembers = GetInstanceMemberNames(typeDefinition.GetAllBaseTypeDefinitions().Where(x => x != typeDefinition), compilation);
			var staticMembers = new HashSet<string>();

			var membersByName =   from m in typeDefinition.GetMembers(options: GetMemberOptions.IgnoreInheritedMembers)
			                       let name = DeterminePreferredMemberName(m)
			                     group new { m, name } by name.Item1 into g
			                    select new { Name = g.Key, Members = g.Select(x => new { Member = x.m, NameSpecified = x.name.Item2 }).ToList() };

			bool isRecord = _typeSemantics[typeDefinition].IsRecord;
			foreach (var current in membersByName) {
				foreach (var m in current.Members.OrderByDescending(x => x.NameSpecified).ThenBy(x => x.Member, MemberOrderer.Instance)) {
					if (m.Member is IMethod) {
						var method = (IMethod)m.Member;

						if (method.IsConstructor) {
							ProcessConstructor(method, current.Name, m.NameSpecified, staticMembers, compilation);
						}
						else {
							ProcessMethod(method, current.Name, m.NameSpecified, m.Member.IsStatic || isRecord ? staticMembers : instanceMembers, compilation);
						}
					}
					else if (m.Member is IProperty) {
						ProcessProperty((IProperty)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers, compilation);
					}
					else if (m.Member is IField) {
						ProcessField((IField)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers);
					}
					else if (m.Member is IEvent) {
						ProcessEvent((IEvent)m.Member, current.Name, m.NameSpecified, m.Member.IsStatic ? staticMembers : instanceMembers, compilation);
					}
				}
			}

			_instanceMemberNamesByType[typeDefinition] = instanceMembers;
		}

		private string GetUniqueName(IMember member, string preferredName, HashSet<string> usedNames) {
			// The name was not explicitly specified, so ensure that we have a unique name.
			if (preferredName == null && member.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
				// Minimized interface names need to be unique within the assembly, otherwise we have a very high risk of collisions (100% when a type implements more than one internal interface).
				int c;
				_internalInterfaceMemberCountPerAssembly.TryGetValue(member.ParentAssembly, out c);
				_internalInterfaceMemberCountPerAssembly[member.ParentAssembly] = ++c;
				return "$I" + EncodeNumber(c, true);
			}
			else {
				string name = preferredName;
				int i = (name == null ? 0 : 1);
				while (name == null || usedNames.Contains(name)) {
					name = preferredName + "$" + EncodeNumber(i, true);
					i++;
				}
				return name;
			}
		}

		private void ProcessConstructor(IMethod constructor, string preferredName, bool nameSpecified, HashSet<string> usedNames, ICompilation compilation) {
			var nsa = GetAttributePositionalArgs(constructor, NonScriptableAttribute);
			var asa = GetAttributePositionalArgs(constructor, AlternateSignatureAttribute);
			var epa = GetAttributePositionalArgs(constructor, ExpandParamsAttribute);

			if (nsa != null || _typeSemantics[constructor.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			if (constructor.DeclaringType.Kind == TypeKind.Delegate) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			if (epa != null && !constructor.Parameters.Any(p => p.IsParams)) {
				_errors[GetQualifiedMemberName(constructor) + ":ExpandParamsOnMethodWithoutParamArray"] = "The constructor for type " + constructor.DeclaringType.FullName + " cannot have an [ExpandParamsAttribute] because it does not have a parameter with the 'params' modifier.";
			}

			bool isRecord = _typeSemantics[constructor.DeclaringTypeDefinition].IsRecord;

			var ica = GetAttributePositionalArgs(constructor, InlineCodeAttribute);
			if (ica != null) {
				string code = (string)ica[0] ?? "";

				var errors = InlineCodeMethodCompiler.ValidateLiteralCode(constructor, code, t => t.Resolve(compilation).Kind != TypeKind.Unknown);
				if (errors.Count > 0) {
					_errors[GetQualifiedMemberName(constructor) + ":ErrorInInlineCode"] = "The inline code for the constructor for " + constructor.DeclaringType.FullName + " contained errors: " + string.Join(", ", errors);
					_constructorSemantics[constructor] = ConstructorScriptSemantics.Unnamed();
					return;
				}

				_constructorSemantics[constructor] = ConstructorScriptSemantics.InlineCode(code);
				return;
			}
			else if (asa != null) {
				_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(generateCode: false, expandParams: epa != null) : ConstructorScriptSemantics.Named(preferredName, generateCode: false, expandParams: epa != null);
				return;
			}
			else if (nameSpecified) {
				if (isRecord)
					_constructorSemantics[constructor] = ConstructorScriptSemantics.StaticMethod(preferredName, expandParams: epa != null);
				else
					_constructorSemantics[constructor] = preferredName == "$ctor" ? ConstructorScriptSemantics.Unnamed(expandParams: epa != null) : ConstructorScriptSemantics.Named(preferredName, expandParams: epa != null);
				usedNames.Add(preferredName);
				return;
			}
			else {
				if (!usedNames.Contains("$ctor") && !(isRecord && _minimizeNames && !Utils.IsPublic(constructor))) {	// The last part ensures that the first constructor of a record type can have its name minimized. 
					_constructorSemantics[constructor] = isRecord ? ConstructorScriptSemantics.StaticMethod("$ctor", expandParams: epa != null) : ConstructorScriptSemantics.Unnamed(expandParams: epa != null);
					usedNames.Add("$ctor");
					return;
				}
				else {
					string name;
					if (_minimizeNames && !Utils.IsPublic(constructor)) {
						name = GetUniqueName(constructor, null, usedNames);
					}
					else {
						int i = 1;
						do {
							name = "$ctor" + EncodeNumber(i, true);
							i++;
						} while (usedNames.Contains(name));
					}

					_constructorSemantics[constructor] = isRecord ? ConstructorScriptSemantics.StaticMethod(name, expandParams: epa != null) : ConstructorScriptSemantics.Named(name, expandParams: epa != null);
					usedNames.Add(name);
					return;
				}
			}
		}

		private void ProcessProperty(IProperty property, string preferredName, bool nameSpecified, HashSet<string> usedNames, ICompilation compilation) {
			if (_typeSemantics[property.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || GetAttributePositionalArgs(property, NonScriptableAttribute) != null) {
				_propertySemantics[property] = PropertyScriptSemantics.NotUsableFromScript();
				return;
			}
			else if (preferredName == "") {
				if (property.IsIndexer) {
					_errors[GetQualifiedMemberName(property) + ":NameCannotBeEmpty"] = "The named specified in a [ScriptNameAttribute] for the indexer of type " + property.DeclaringType.FullName + " cannot be empty.";
				}
				else {
					_errors[GetQualifiedMemberName(property) + ":NameCannotBeEmpty"] = "The named specified in a [ScriptNameAttribute] for the property " + GetQualifiedMemberName(property) + " cannot be empty.";
				}
				_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.NormalMethod("get") : null, property.CanSet ? MethodScriptSemantics.NormalMethod("set") : null);
				return;
			}
			else if (_typeSemantics[property.DeclaringTypeDefinition].IsRecord && !property.IsStatic) {
				usedNames.Add(preferredName);
				_propertySemantics[property] = PropertyScriptSemantics.Field(preferredName);
				return;
			}

			var saa = GetAttributePositionalArgs(property, ScriptAliasAttribute);

			if (saa != null) {
				if (property.IsIndexer) {
					_errors[GetQualifiedMemberName(property) + ":IndexerCannotHaveScriptAlias"] = "The indexer on type " + property.DeclaringType.FullName + " cannot have a [ScriptAliasAttribute].";
				}
				else if (!property.IsStatic) {
					_errors[GetQualifiedMemberName(property) + ":InstancePropertyCannotHaveScriptAlias"] = "The property " + GetQualifiedMemberName(property) + " cannot have a [ScriptAliasAttribute] because it is an instance member.";
				}
				else {
					string alias = (string)saa[0] ?? "";
					_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.InlineCode(alias) : null, property.CanSet ? MethodScriptSemantics.InlineCode(alias) : null);
					return;
				}
			}

			var ipa = GetAttributePositionalArgs(property, IntrinsicPropertyAttribute);
			if (ipa != null) {
				if (property.DeclaringType.Kind == TypeKind.Interface) {
					if (property.IsIndexer)
						_errors[GetQualifiedMemberName(property) + ":InterfacePropertyCannotBeIntrinsic"] = "The indexer on type " + property.DeclaringType.FullName + " cannot have an [IntrinsicPropertyAttribute] because it is an interface member.";
					else
						_errors[GetQualifiedMemberName(property) + ":InterfacePropertyCannotBeIntrinsic"] = "The property " + GetQualifiedMemberName(property) + " cannot have an [IntrinsicPropertyAttribute] because it is an interface member.";
				}
				else if (property.IsOverride) {
					if (property.IsIndexer)
						_errors[GetQualifiedMemberName(property) + ":OverridingPropertyCannotBeIntrinsic"] = "The indexer on type " + property.DeclaringType.FullName + " cannot have an [IntrinsicPropertyAttribute] because it overrides a base member.";
					else
						_errors[GetQualifiedMemberName(property) + ":OverridingPropertyCannotBeIntrinsic"] = "The property " + GetQualifiedMemberName(property) + " cannot have an [IntrinsicPropertyAttribute] because it overrides a base member.";
				}
				else if (property.IsOverridable) {
					if (property.IsIndexer)
						_errors[GetQualifiedMemberName(property) + ":OverridablePropertyCannotBeIntrinsic"] = "The indexer on type " + property.DeclaringType.FullName + " cannot have an [IntrinsicPropertyAttribute] because it is overridable.";
					else
						_errors[GetQualifiedMemberName(property) + ":OverridablePropertyCannotBeIntrinsic"] = "The property " + GetQualifiedMemberName(property) + " cannot have an [IntrinsicPropertyAttribute] because it is overridable.";
				}
				else if (property.ImplementedInterfaceMembers.Count > 0) {
					if (property.IsIndexer)
						_errors[GetQualifiedMemberName(property) + ":ImplementingPropertyCannotBeIntrinsic"] = "The indexer on type" + property.DeclaringType.FullName + " cannot have an [IntrinsicPropertyAttribute] because it implements an interface member.";
					else
						_errors[GetQualifiedMemberName(property) + ":ImplementingPropertyCannotBeIntrinsic"] = "The property " + GetQualifiedMemberName(property) + " cannot have an [IntrinsicPropertyAttribute] because it implements an interface member.";
				}
				else if (property.IsIndexer) {
					if (property.Parameters.Count == 1) {
						_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.NativeIndexer() : null, property.CanSet ? MethodScriptSemantics.NativeIndexer() : null);
						return;
					}
					else {
						_errors[GetQualifiedMemberName(property) + ":NativeIndexerArgument"] = "The indexer for type " + property.DeclaringType.FullName + " must have exactly one parameter in order to have an [IntrinsicPropertyAttribute].";
					}
				}
				else {
					usedNames.Add(preferredName);
					_propertySemantics[property] = PropertyScriptSemantics.Field(preferredName);
					return;
				}
			}

			MethodScriptSemantics getter, setter;
			if (property.CanGet) {
				var getterName = DeterminePreferredMemberName(property.Getter);
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && !Utils.IsPublic(property) ? null : (nameSpecified ? "get_" + preferredName : GetUniqueName(property, "get_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Getter, getterName.Item1, getterName.Item2, usedNames, compilation);
				getter = _methodSemantics[property.Getter];
			}
			else {
				getter = null;
			}

			if (property.CanSet) {
				var setterName = DeterminePreferredMemberName(property.Setter);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && !Utils.IsPublic(property) ? null : (nameSpecified ? "set_" + preferredName : GetUniqueName(property, "set_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Setter, setterName.Item1, setterName.Item2, usedNames, compilation);
				setter = _methodSemantics[property.Setter];
			}
			else {
				setter = null;
			}

			_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(getter, setter);
		}

		private void ProcessMethod(IMethod method, string preferredName, bool nameSpecified, HashSet<string> usedNames, ICompilation compilation) {
			for (int i = 0; i < method.TypeParameters.Count; i++) {
				var tp = method.TypeParameters[i];
				_typeParameterNames[tp] = _minimizeNames ? EncodeNumber(method.DeclaringType.TypeParameterCount + i, false) : tp.Name;
			}

			var ssa = GetAttributePositionalArgs(method, ScriptSkipAttribute);
			var saa = GetAttributePositionalArgs(method, ScriptAliasAttribute);
			var ica = GetAttributePositionalArgs(method, InlineCodeAttribute);
			var ifa = GetAttributePositionalArgs(method, InstanceMethodOnFirstArgumentAttribute);
			var nsa = GetAttributePositionalArgs(method, NonScriptableAttribute);
			var iga = GetAttributePositionalArgs(method, IgnoreGenericArgumentsAttribute);
			var noa = GetAttributePositionalArgs(method, IntrinsicOperatorAttribute);
			var epa = GetAttributePositionalArgs(method, ExpandParamsAttribute);

			if (nsa != null || _typeSemantics[method.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_methodSemantics[method] = MethodScriptSemantics.NotUsableFromScript();
				return;
			}
			if (noa != null) {
				if (!method.IsOperator) {
					_errors[GetQualifiedMemberName(method) + ":IntrinsicOperator"] = "The member " + GetQualifiedMemberName(method) + " cannot have an [IntrinsicOperatorAttribute] because it is not an operator method.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
				}
				if (method.Name == "op_Implicit" || method.Name == "op_Explicit") {
					_errors[GetQualifiedMemberName(method) + ":IntrinsicOperator"] = "The [IntrinsicOperatorAttribute] cannot be applied to the operator " + method.DeclaringType.FullName + ".operator " + method.ReturnType.FullName + " because it is a conversion operator.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
				}
				else {
					_methodSemantics[method] = MethodScriptSemantics.NativeOperator();
				}
				return;
			}
			else if (ssa != null) {
				// [ScriptSkip] - Skip invocation of the method entirely.
				if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
					_errors[GetQualifiedMemberName(method) + ":ScriptSkipOnInterfaceMember"] = "The member " + GetQualifiedMemberName(method) + " cannot have a [ScriptSkipAttribute] because it is an interface method.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else if (method.IsOverride) {
					_errors[GetQualifiedMemberName(method) + ":ScriptSkipOnOverridable"] = "The member " + GetQualifiedMemberName(method) + " cannot have a [ScriptSkipAttribute] because it overrides a base member.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else if (method.IsOverridable) {
					_errors[GetQualifiedMemberName(method) + ":ScriptSkipOnOverridable"] = "The member " + GetQualifiedMemberName(method) + " cannot have a [ScriptSkipAttribute] because it is overridable.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else if (method.ImplementedInterfaceMembers.Count > 0) {
					_errors[GetQualifiedMemberName(method) + ":ScriptSkipOnInterfaceImplementation"] = "The member " + GetQualifiedMemberName(method) + " cannot have a [ScriptSkipAttribute] because it implements an interface member.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else {
					if (method.IsStatic) {
						if (method.Parameters.Count != 1) {
							_errors[GetQualifiedMemberName(method) + ":ScriptSkipParameterCount"] = "The static method " + GetQualifiedMemberName(method) + " must have exactly one parameter in order to have a [ScriptSkipAttribute].";
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						_methodSemantics[method] = MethodScriptSemantics.InlineCode("{" + method.Parameters[0].Name + "}");
						return;
					}
					else {
						if (method.Parameters.Count != 0)
							_errors[GetQualifiedMemberName(method) + ":ScriptSkipParameterCount"] = "The instance method " + GetQualifiedMemberName(method) + " must have no parameters in order to have a [ScriptSkipAttribute].";
						_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}");
						return;
					}
				}
			}
			else if (saa != null) {
				if (method.IsStatic) {
					_methodSemantics[method] = MethodScriptSemantics.InlineCode((string) saa[0] + "(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")");
					return;
				}
				else {
					_errors[GetQualifiedMemberName(method) + ":NonStaticWithAlias"] = "The method " + GetQualifiedMemberName(method) + " must be static in order to have a [ScriptAliasAttribute].";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
			}
			else if (ica != null) {
				if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
					_errors[GetQualifiedMemberName(method) + ":InlineCodeOnInterfaceMember"] = "The member " + GetQualifiedMemberName(method) + " cannot have an [InlineCodeAttribute] because it is an interface method.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else if (method.IsOverride) {
					_errors[GetQualifiedMemberName(method) + ":InlineCodeOnOverridable"] = "The member " + GetQualifiedMemberName(method) + " cannot have an [InlineCodeAttribute] because it overrides a base member.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else if (method.IsOverridable) {
					_errors[GetQualifiedMemberName(method) + ":InlineCodeOnOverridable"] = "The member " + GetQualifiedMemberName(method) + " cannot have an [InlineCodeAttribute] because it is overridable.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else if (method.ImplementedInterfaceMembers.Count > 0) {
					_errors[GetQualifiedMemberName(method) + ":InlineCodeOnInterfaceImplementation"] = "The member " + GetQualifiedMemberName(method) + " cannot have a [InlineCodeAttribute] because it implements an interface member.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
				else {
					string code = (string) ica[0];

					var errors = InlineCodeMethodCompiler.ValidateLiteralCode(method, code, t => t.Resolve(compilation).Kind != TypeKind.Unknown);
					if (errors.Count > 0) {
						_errors[GetQualifiedMemberName(method) + ":ErrorInInlineCode"] = "The inline code for the method " + GetQualifiedMemberName(method) + " contained errors: " + string.Join(", ", errors);
						code = "X";
					}

					_methodSemantics[method] = MethodScriptSemantics.InlineCode(code);
					return;
				}
			}
			else if (ifa != null) {
				if (method.IsStatic) {
					_methodSemantics[method] = MethodScriptSemantics.InstanceMethodOnFirstArgument(preferredName, expandParams: epa != null);
					return;
				}
				else {
					_errors[GetQualifiedMemberName(method) + ":InstanceMethodOnFirstArgument"] = "The method " + GetQualifiedMemberName(method) + " cannot have an [InstanceMethodOnFirstArgumentAttribute] because it is not static.";
					_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
					return;
				}
			}
			else {
				if (method.IsOverride) {
					if (nameSpecified) {
						_errors[GetQualifiedMemberName(method) + ":CannotSpecifyName"] = "The [ScriptName], [PreserveName] and [PreserveCase] attributes cannot be specified on method the method " + GetQualifiedMemberName(method) + " because it overrides a base member. Specify the attribute on the base member instead.";
					}
					if (iga != null) {
						_errors[GetQualifiedMemberName(method) + ":CannotSpecifyIgnoreGenericArguments"] = "The [IgnoreGenericArgumentsAttribute] attribute cannot be specified on the method " + GetQualifiedMemberName(method) + " because it overrides a base member. Specify the attribute on the base member instead.";
					}

					var semantics = _methodSemantics[(IMethod)InheritanceHelper.GetBaseMember(method)];
					if (semantics.Type == MethodScriptSemantics.ImplType.NormalMethod) {
						var errorMethod = method.ImplementedInterfaceMembers.FirstOrDefault(im => GetMethodSemantics((IMethod)im.MemberDefinition).Name != semantics.Name);
						if (errorMethod != null) {
							_errors[GetQualifiedMemberName(method) + ":MultipleInterfaceImplementations"] = "The overriding member " + GetQualifiedMemberName(method) + " cannot implement the interface method " + GetQualifiedMemberName(errorMethod) + " because it has a different script name. Consider using explicit interface implementation";
						}
					}

					_methodSemantics[method] = semantics;
					return;
				}
				else if (method.ImplementedInterfaceMembers.Count > 0) {
					if (nameSpecified) {
						_errors[GetQualifiedMemberName(method) + ":CannotSpecifyName"] = "The [ScriptName], [PreserveName] and [PreserveCase] attributes cannot be specified on the method " + GetQualifiedMemberName(method) + " because it implements an interface member. Specify the attribute on the interface member instead, or consider using explicit interface implementation.";
					}

					if (method.ImplementedInterfaceMembers.Select(im => GetMethodSemantics((IMethod)im.MemberDefinition).Name).Distinct().Count() > 1) {
						_errors[GetQualifiedMemberName(method) + ":MultipleInterfaceImplementations"] = "The member " + GetQualifiedMemberName(method) + " cannot implement multiple interface methods with differing script names. Consider using explicit interface implementation.";
					}

					_methodSemantics[method] = _methodSemantics[(IMethod)method.ImplementedInterfaceMembers[0].MemberDefinition];
					return;
				}
				else {
					if (method.DeclaringType.Kind == TypeKind.Delegate && method.Name != "Invoke") {
						_methodSemantics[method] = MethodScriptSemantics.NotUsableFromScript();
						return;
					}
					if (epa != null) {
						if (!method.Parameters.Any(p => p.IsParams)) {
							_errors[GetQualifiedMemberName(method) + ":ExpandParamsOnMethodWithoutParamArray"] = "The member " + GetQualifiedMemberName(method) + " cannot have an [ExpandParamsAttribute] because it does not have a parameter with the 'params' modifier.";
						}
					}

					if (preferredName == "") {
						// Special case - Script# supports setting the name of a method to an empty string, which means that it simply removes the name (eg. "x.M(a)" becomes "x(a)"). We model this with literal code.
						if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
							_errors[GetQualifiedMemberName(method) + ":InterfaceMethodWithEmptyName"] = "The member " + GetQualifiedMemberName(method) + " cannot have an empty name specified in its [ScriptName] because it is an interface method.";
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						else if (method.IsOverridable) {
							_errors[GetQualifiedMemberName(method) + ":OverridableWithEmptyName"] = "The member " + GetQualifiedMemberName(method) + " cannot have an empty name specified in its [ScriptName] because it is overridable.";
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						else if (method.IsStatic) {
							_errors[GetQualifiedMemberName(method) + ":StaticWithEmptyName"] = "The member " + GetQualifiedMemberName(method) + " cannot have an empty name specified in its [ScriptName] because it is static.";
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
							return;
						}
						else {
							_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")");
							return;
						}
					}
					else if (_typeSemantics[method.DeclaringTypeDefinition].GlobalMethods) {
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(preferredName, isGlobal: true, expandParams: epa != null);
						return;
					}
					else {
						string name = nameSpecified ? preferredName : GetUniqueName(method, preferredName, usedNames);
						usedNames.Add(name);
						if (_typeSemantics[method.DeclaringTypeDefinition].IsRecord && !method.IsStatic)
							_methodSemantics[method] = MethodScriptSemantics.StaticMethodWithThisAsFirstArgument(name, generateCode: GetAttributePositionalArgs(method, AlternateSignatureAttribute) == null, ignoreGenericArguments: iga != null, expandParams: epa != null);
						else
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(name, generateCode: GetAttributePositionalArgs(method, AlternateSignatureAttribute) == null, ignoreGenericArguments: iga != null, expandParams: epa != null);
					}
				}
			}
		}

		private void ProcessEvent(IEvent evt, string preferredName, bool nameSpecified, HashSet<string> usedNames, ICompilation compilation) {
			if (_typeSemantics[evt.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || GetAttributePositionalArgs(evt, NonScriptableAttribute) != null) {
				_eventSemantics[evt] = EventScriptSemantics.NotUsableFromScript();
				return;
			}
			else if (preferredName == "") {
				_errors[GetQualifiedMemberName(evt) + ":NameCannotBeEmpty"] = "The named specified in a [ScriptNameAttribute] for the event " + GetQualifiedMemberName(evt) + " cannot be empty.";
				_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add"), MethodScriptSemantics.NormalMethod("remove"));
				return;
			}

			MethodScriptSemantics adder, remover;
			if (evt.CanAdd) {
				var getterName = DeterminePreferredMemberName(evt.AddAccessor);
				if (!getterName.Item2)
					getterName = Tuple.Create(!nameSpecified && _minimizeNames && !Utils.IsPublic(evt) ? null : (nameSpecified ? "add_" + preferredName : GetUniqueName(evt, "add_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.AddAccessor, getterName.Item1, getterName.Item2, usedNames, compilation);
				adder = _methodSemantics[evt.AddAccessor];
			}
			else {
				adder = null;
			}

			if (evt.CanRemove) {
				var setterName = DeterminePreferredMemberName(evt.RemoveAccessor);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && !Utils.IsPublic(evt) ? null : (nameSpecified ? "remove_" + preferredName : GetUniqueName(evt, "remove_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.RemoveAccessor, setterName.Item1, setterName.Item2, usedNames, compilation);
				remover = _methodSemantics[evt.RemoveAccessor];
			}
			else {
				remover = null;
			}

			_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(adder, remover);
		}

		private void ProcessField(IField field, string preferredName, bool nameSpecified, HashSet<string> usedNames) {
			if (_typeSemantics[field.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || GetAttributePositionalArgs(field, NonScriptableAttribute) != null) {
				_fieldSemantics[field] = FieldScriptSemantics.NotUsableFromScript();
			}
			else if (preferredName == "") {
				_errors[GetQualifiedMemberName(field) + ":NameCannotBeEmpty"] = "The named specified in a [ScriptNameAttribute] for the field " + GetQualifiedMemberName(field) + " cannot be empty.";
				_fieldSemantics[field] = FieldScriptSemantics.Field("X");
			}
			else {
				string name = nameSpecified ? preferredName : GetUniqueName(field, preferredName, usedNames);
				usedNames.Add(name);
				if (_typeSemantics[field.DeclaringTypeDefinition].IsNamedValues) {
					_fieldSemantics[field] = FieldScriptSemantics.StringConstant(name, name);
				}
				else if (field.IsConst && (field.DeclaringType.Kind == TypeKind.Enum || _minimizeNames)) {
					object value = Utils.ConvertToDoubleOrStringOrBoolean(field.ConstantValue);
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

		public void Prepare(IEnumerable<ITypeDefinition> types, IAssembly mainAssembly, IErrorReporter errorReporter) {
			_systemObject = mainAssembly.Compilation.FindType(KnownTypeCode.Object);
			_systemRecord = ReflectionHelper.ParseReflectionName("System.Record").Resolve(mainAssembly.Compilation.TypeResolveContext);
			_internalInterfaceMemberCountPerAssembly = new Dictionary<IAssembly, int>();
			_errors = new Dictionary<string, string>();
			var l = types.ToList();
			_typeSemantics = new Dictionary<ITypeDefinition, TypeSemantics>();
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

			foreach (var t in l.Where(t => t.ParentAssembly == mainAssembly || Utils.IsPublic(t))) {
				ProcessType(t);
				ProcessTypeMembers(t, mainAssembly.Compilation);
			}

			foreach (var e in _errors.Values)
				errorReporter.Error(e);
		}

		public TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition) {
			if (typeDefinition.Kind == TypeKind.Delegate)
				return TypeScriptSemantics.NormalType(Function);
			else if (typeDefinition.Kind == TypeKind.Array)
				return TypeScriptSemantics.NormalType(Array);
			return _typeSemantics[typeDefinition].Semantics;
		}

		public string GetTypeParameterName(ITypeParameter typeParameter) {
			return _typeParameterNames[typeParameter];
		}

		public MethodScriptSemantics GetMethodSemantics(IMethod method) {
			return _methodSemantics[(IMethod)method.MemberDefinition];
		}

		public ConstructorScriptSemantics GetConstructorSemantics(IMethod method) {
			if (method.DeclaringType.Kind == TypeKind.Anonymous)
				return ConstructorScriptSemantics.Json();
			return _constructorSemantics[(IMethod)method.MemberDefinition];
		}

		public PropertyScriptSemantics GetPropertySemantics(IProperty property) {
			if (property.DeclaringType.Kind == TypeKind.Anonymous)
				return PropertyScriptSemantics.Field(property.Name);
			return _propertySemantics[(IProperty)property.MemberDefinition];
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
			return _fieldSemantics[(IField)field.MemberDefinition];
		}

		public EventScriptSemantics GetEventSemantics(IEvent evt) {
			return _eventSemantics[(IEvent)evt.MemberDefinition];
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

		public string GetVariableName(IVariable variable, ISet<string> usedNames) {
			if (_minimizeNames) {
				// We know that (as long as all used names come from us), all names are generated in sequence. Therefore, the number of used name is a good starting guess for a unique name.
				int i = usedNames.Count;
				string name;
				do {
					name = EncodeNumber(i++, false);
				} while (usedNames.Contains(name));
				return name;
			}
			else {
                string baseName = (variable != null ? variable.Name : "$t");
                if (variable != null && !usedNames.Contains(baseName))
                    return baseName;
                int i = 1;
				string name;
				do {
					name = baseName + (i++).ToString(CultureInfo.InvariantCulture);
				} while (usedNames.Contains(name));

                return name;
			}
		}

		public string ThisAlias {
			get { return _minimizeNames ? "$_" : "$this"; }
		}
	}
}
