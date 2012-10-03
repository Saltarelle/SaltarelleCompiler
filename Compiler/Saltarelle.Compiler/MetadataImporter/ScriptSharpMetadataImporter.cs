using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	public class ScriptSharpMetadataImporter : IMetadataImporter, IScriptSharpMetadataImporter {
		private const string ScriptSkipAttribute                    = "System.Runtime.CompilerServices.ScriptSkipAttribute";
		private const string ScriptAliasAttribute                   = "System.Runtime.CompilerServices.ScriptAliasAttribute";
		private const string InlineCodeAttribute                    = "System.Runtime.CompilerServices.InlineCodeAttribute";
		private const string InstanceMethodOnFirstArgumentAttribute = "System.Runtime.CompilerServices.InstanceMethodOnFirstArgumentAttribute";
		private const string NonScriptableAttribute                 = "System.Runtime.CompilerServices.NonScriptableAttribute";
		private const string IgnoreGenericArgumentsAttribute        = "System.Runtime.CompilerServices.IgnoreGenericArgumentsAttribute";
		private const string IgnoreNamespaceAttribute               = "System.Runtime.CompilerServices.IgnoreNamespaceAttribute";
		private const string ScriptNamespaceAttribute               = "System.Runtime.CompilerServices.ScriptNamespaceAttribute";
		private const string AlternateSignatureAttribute            = "System.Runtime.CompilerServices.AlternateSignatureAttribute";
		private const string ScriptNameAttribute                    = "System.Runtime.CompilerServices.ScriptNameAttribute";
		private const string PreserveNameAttribute                  = "System.Runtime.CompilerServices.PreserveNameAttribute";
		private const string PreserveCaseAttribute                  = "System.Runtime.CompilerServices.PreserveCaseAttribute";
		private const string PreserveMemberCaseAttribute            = "System.Runtime.CompilerServices.PreserveMemberCaseAttribute";
		private const string IntrinsicPropertyAttribute             = "System.Runtime.CompilerServices.IntrinsicPropertyAttribute";
		private const string GlobalMethodsAttribute                 = "System.Runtime.CompilerServices.GlobalMethodsAttribute";
		private const string ImportedAttribute                      = "System.Runtime.CompilerServices.ImportedAttribute";
		private const string SerializableAttribute                  = "System.SerializableAttribute";
		private const string IntrinsicOperatorAttribute             = "System.Runtime.CompilerServices.IntrinsicOperatorAttribute";
		private const string ExpandParamsAttribute                  = "System.Runtime.CompilerServices.ExpandParamsAttribute";
		private const string NamedValuesAttribute                   = "System.Runtime.CompilerServices.NamedValuesAttribute";
		private const string ResourcesAttribute                     = "System.Runtime.CompilerServices.ResourcesAttribute";
		private const string MixinAttribute                         = "System.Runtime.CompilerServices.MixinAttribute";
		private const string ObjectLiteralAttribute                 = "System.Runtime.CompilerServices.ObjectLiteralAttribute";
		private const string ScriptSharpCompatibilityAttribute      = "System.Runtime.CompilerServices.ScriptSharpCompatibilityAttribute";
		private const string BindThisToFirstParameterAttribute      = "System.Runtime.CompilerServices.BindThisToFirstParameterAttribute";
		private const string DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor = "System.Runtime.CompilerServices.DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor";
		private const string TestFixtureAttribute                   = "System.Testing.TestFixtureAttribute";
		private const string TestAttribute                          = "System.Testing.TestAttribute";
		private const string AsyncTestAttribute                     = "System.Testing.AsyncTestAttribute";
		private const string CategoryPropertyName               = "Category";
		private const string ExpectedAssertionCountPropertyName = "ExpectedAssertionCount";
		private const string IsRealTypePropertyName             = "IsRealType";
		private const string OmitDowncastsPropertyName          = "OmitDowncasts";
		private const string OmitNullableChecksPropertyName     = "OmitNullableChecks";
		private const string Function = "Function";
		private const string Array    = "Array";

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
			public bool IsGlobalMethods { get; private set; }
			public bool IsSerializable { get; private set; }
			public bool IsNamedValues { get; private set; }
			public bool IsImported { get; private set; }
			public bool IsRealType { get; private set; }
			public bool IsResources { get; private set; }
			public string MixinArg { get; private set; }
			public bool IsTestFixture { get; private set; }

			public TypeSemantics(TypeScriptSemantics semantics, bool isGlobalMethods, bool isSerializable, bool isNamedValues, bool isImported, bool isRealType, bool isResources, string mixinArg, bool isTestFixture) {
				Semantics       = semantics;
				IsGlobalMethods = isGlobalMethods;
				IsSerializable  = isSerializable;
				IsNamedValues   = isNamedValues;
				IsImported      = isImported;
				IsRealType      = isRealType;
				IsResources     = isResources;
				MixinArg        = mixinArg;
				IsTestFixture   = isTestFixture;
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
		private Dictionary<IMethod, TestMethodData> _methodTestData;
		private IErrorReporter _errorReporter;
		private IType _systemObject;
		private IType _systemRecord;
		private ICompilation _compilation;
		private bool _omitDowncasts;
		private bool _omitNullableChecks;

		private readonly bool _minimizeNames;

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

		private IList<object> GetAttributePositionalArgs(IEntity entity, string attributeName) {
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == attributeName);
			return attr != null ? attr.PositionalArguments.Select(arg => arg.ConstantValue).ToList() : null;
		}

		private static T GetNamedArgument<T>(IAttribute attr, string propertyName) {
			return attr.NamedArguments.Where(a => a.Key.Name == propertyName).Select(a => (T)a.Value.ConstantValue).FirstOrDefault();
		}

		private string DetermineNamespace(ITypeDefinition typeDefinition) {
			while (typeDefinition.DeclaringTypeDefinition != null) {
				typeDefinition = typeDefinition.DeclaringTypeDefinition;
			}

			var ina = GetAttributePositionalArgs(typeDefinition, IgnoreNamespaceAttribute);
			var sna = GetAttributePositionalArgs(typeDefinition, ScriptNamespaceAttribute);
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
					string arg = (string)sna[0];
					if (arg == null || (arg != "" && !arg.IsValidNestedJavaScriptIdentifier()))
						Message(7002, typeDefinition);
					return arg;
				}
				else {
					var asna = typeDefinition.ParentAssembly.AssemblyAttributes.SingleOrDefault(a => a.AttributeType.FullName == ScriptNamespaceAttribute);
					if (asna != null) {
						var arg = (string)asna.PositionalArguments[0].ConstantValue;
						if (arg != null && (arg == "" || arg.IsValidNestedJavaScriptIdentifier()))
							return arg;
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
			bool bindThisToFirstParameter = GetAttributePositionalArgs(delegateDefinition, BindThisToFirstParameterAttribute) != null;
			bool expandParams = GetAttributePositionalArgs(delegateDefinition, ExpandParamsAttribute) != null;

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

			if (GetAttributePositionalArgs(typeDefinition, NonScriptableAttribute) != null || typeDefinition.DeclaringTypeDefinition != null && GetTypeSemantics(typeDefinition.DeclaringTypeDefinition).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NotUsableFromScript(), false, false, false, false, true, false, null, false);
				return;
			}

			var scriptNameAttr = GetAttributePositionalArgs(typeDefinition, ScriptNameAttribute);
			var importedAttr = typeDefinition.Attributes.FirstOrDefault(a => a.AttributeType.FullName == ImportedAttribute);
			bool isImported = importedAttr != null;
			bool isRealType = importedAttr == null || GetNamedArgument<bool>(importedAttr, IsRealTypePropertyName);
			bool preserveName = isImported || GetAttributePositionalArgs(typeDefinition, PreserveNameAttribute) != null;

			bool ignoreGenericArguments = GetAttributePositionalArgs(typeDefinition, IgnoreGenericArgumentsAttribute) != null || isImported;
			bool isResources = false;

			if (GetAttributePositionalArgs(typeDefinition, ResourcesAttribute) != null) {
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
			if (scriptNameAttr != null && scriptNameAttr[0] != null && ((string)scriptNameAttr[0]).IsValidJavaScriptIdentifier()) {
				typeName = (string)scriptNameAttr[0];
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
						if (GetAttributePositionalArgs(typeDefinition, IgnoreNamespaceAttribute) != null || GetAttributePositionalArgs(typeDefinition, ScriptNamespaceAttribute) != null) {
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

			bool hasSerializableAttr = GetAttributePositionalArgs(typeDefinition, SerializableAttribute) != null;
			bool inheritsRecord = typeDefinition.GetAllBaseTypeDefinitions().Any(td => td.Equals(_systemRecord)) && !typeDefinition.Equals(_systemRecord);
			
			bool globalMethods = false, isSerializable = hasSerializableAttr || inheritsRecord;
			string mixinArg = null;

			if (isSerializable) {
				var baseClass = typeDefinition.DirectBaseTypes.Single(c => c.Kind == TypeKind.Class).GetDefinition();
				if (!baseClass.Equals(_systemObject) && !baseClass.Equals(_systemRecord) && !_typeSemantics[baseClass].IsSerializable) {
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
				if (baseClass != null && _typeSemantics[baseClass.GetDefinition()].IsSerializable) {
					Message(7008, typeDefinition, baseClass.FullName);
				}

				var globalMethodsAttr = GetAttributePositionalArgs(typeDefinition, GlobalMethodsAttribute);
				var mixinAttr = GetAttributePositionalArgs(typeDefinition, MixinAttribute);
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
					else if (mixinAttr[0] == null || !((string)mixinAttr[0]).IsValidNestedJavaScriptIdentifier()) {
						Message(7025, typeDefinition);
					}
					else {
						var split = SplitNamespacedName((string)mixinAttr[0]);
						nmspace   = split.Item1;
						typeName  = split.Item2;
						mixinArg  = (string)mixinAttr[0];
					}
				}
				else if (globalMethodsAttr != null) {
					if (!typeDefinition.IsStatic) {
						Message(7015, typeDefinition);
					}
					else if (typeDefinition.Fields.Any() || typeDefinition.Events.Any() || typeDefinition.Properties.Any()) {
						Message(7016, typeDefinition);
					}
					else if (typeDefinition.TypeParameterCount > 0) {
						Message(7017, typeDefinition);
					}
					else {
						nmspace = "";
						globalMethods = true;
					}
				}
			}

			for (int i = 0; i < typeDefinition.TypeParameterCount; i++) {
				var tp = typeDefinition.TypeParameters[i];
				_typeParameterNames[tp] = _minimizeNames ? EncodeNumber(i, true) : tp.Name;
			}

			var nva = GetAttributePositionalArgs(typeDefinition, NamedValuesAttribute);
			var tfa = GetAttributePositionalArgs(typeDefinition, TestFixtureAttribute);
			_typeSemantics[typeDefinition] = new TypeSemantics(TypeScriptSemantics.NormalType(!string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName, ignoreGenericArguments: ignoreGenericArguments, generateCode: !isImported), isGlobalMethods: globalMethods, isSerializable: isSerializable, isNamedValues: nva != null, isImported: isImported, isRealType: isRealType, isResources: isResources, mixinArg: mixinArg, isTestFixture: tfa != null);
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
				var other = m.DeclaringType.GetConstructors().SingleOrDefault(c => c.Parameters.Count == 1 && c.Parameters[0].Type.FullName == DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor);
				if (other != null)
					return other;
			}
			return m;
		}

		private Tuple<string, bool> DeterminePreferredMemberName(IMember member) {
			member = UnwrapValueTypeConstructor(member);

		    var typePreserveMemberCaseAttribute = member.DeclaringTypeDefinition.Attributes.FirstOrDefault( a => a.AttributeType.FullName == PreserveMemberCaseAttribute);
		    var assemblyPreserveMemberCaseAttribute = member.ParentAssembly.AssemblyAttributes.FirstOrDefault( a => a.AttributeType.FullName == PreserveMemberCaseAttribute);
		    bool preserveMemberCase = (typePreserveMemberCaseAttribute != null
		                               && (typePreserveMemberCaseAttribute.PositionalArguments.Count == 0
		                                   ||
		                                   (typePreserveMemberCaseAttribute.PositionalArguments.Count > 0
		                                    &&
		                                    typePreserveMemberCaseAttribute.PositionalArguments[0].ConstantValue.ToString() ==
		                                    true.ToString())))
		                              ||
		                              (assemblyPreserveMemberCaseAttribute != null
		                               && (typePreserveMemberCaseAttribute == null
		                                   || (typePreserveMemberCaseAttribute.PositionalArguments.Count > 0
		                                       &&
		                                       typePreserveMemberCaseAttribute.PositionalArguments[0].ConstantValue.ToString() ==
		                                       true.ToString())));

			bool isConstructor = member is IMethod && ((IMethod)member).IsConstructor;
			bool isAccessor = member is IMethod && ((IMethod)member).IsAccessor;

			string defaultName;
			if (isConstructor) {
				defaultName = "$ctor";
			}
			else if (Utils.IsPublic(member)) {
				defaultName = preserveMemberCase ? member.Name : MakeCamelCase(member.Name);
			}
			else {
				if (_minimizeNames && member.DeclaringType.Kind != TypeKind.Interface)
					defaultName = null;
				else
					defaultName = "$" + (preserveMemberCase ? member.Name : MakeCamelCase(member.Name));
			}


			var asa = GetAttributePositionalArgs(member, AlternateSignatureAttribute);
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && GetAttributePositionalArgs(m, AlternateSignatureAttribute) == null && GetAttributePositionalArgs(m, NonScriptableAttribute) == null).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0]);
				}
				else {
					Message(7100, member);
					return Tuple.Create(member.Name, false);
				}
			}

			var typeSemantics = _typeSemantics[member.DeclaringTypeDefinition];

			var sna = GetAttributePositionalArgs(member, ScriptNameAttribute);
			if (sna != null) {
				string name = (string)sna[0] ?? "";
				if (typeSemantics.IsNamedValues && (name == "" || !name.IsValidNestedJavaScriptIdentifier())) {
					return Tuple.Create(defaultName, false);	// For named values enum, allow the use to specify an empty or invalid value, which will only be used as the literal value for the field, not for the name.
				}
				else if (name != "" && !name.IsValidJavaScriptIdentifier()) {
					Message(7101, member);
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
			                                                       || typeSemantics.IsGlobalMethods
			                                                       || (!typeSemantics.Semantics.GenerateCode && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (typeSemantics.IsSerializable && !member.IsStatic && (member is IProperty || member is IField)))
			                                                       || (typeSemantics.IsNamedValues && member is IField));

			if (preserveName)
				return Tuple.Create(preserveMemberCase ? member.Name : MakeCamelCase(member.Name), true);

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

			bool isSerializable = _typeSemantics[typeDefinition].IsSerializable;
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
						var ps = _propertySemantics[p];
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
						var es = _eventSemantics[e];
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
			if (constructor.Parameters.Count == 1 && constructor.Parameters[0].Type.FullName == DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor) {
				_constructorSemantics[constructor] = ConstructorScriptSemantics.NotUsableFromScript();
				return;
			}

			var source = (IMethod)UnwrapValueTypeConstructor(constructor);

			var nsa = GetAttributePositionalArgs(source, NonScriptableAttribute);
			var asa = GetAttributePositionalArgs(source, AlternateSignatureAttribute);
			var epa = GetAttributePositionalArgs(source, ExpandParamsAttribute);
			var ola = GetAttributePositionalArgs(source, ObjectLiteralAttribute);

			if (nsa != null || _typeSemantics[source.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
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

			bool isSerializable = _typeSemantics[source.DeclaringTypeDefinition].IsSerializable;
			bool isImported     = _typeSemantics[source.DeclaringTypeDefinition].IsImported;

			var ica = GetAttributePositionalArgs(source, InlineCodeAttribute);
			if (ica != null) {
				string code = (string)ica[0] ?? "";

				var errors = InlineCodeMethodCompiler.ValidateLiteralCode(source, code, t => t.Resolve(_compilation));
				if (errors.Count > 0) {
					Message(7103, source, string.Join(", ", errors));
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
			else if (ola != null || (isSerializable && _typeSemantics[source.DeclaringTypeDefinition].IsImported)) {
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
			if (_typeSemantics[property.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || GetAttributePositionalArgs(property, NonScriptableAttribute) != null) {
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
			else if (_typeSemantics[property.DeclaringTypeDefinition].IsSerializable && !property.IsStatic) {
				usedNames[preferredName] = true;
				_propertySemantics[property] = PropertyScriptSemantics.Field(preferredName);
				return;
			}

			var saa = GetAttributePositionalArgs(property, ScriptAliasAttribute);

			if (saa != null) {
				if (property.IsIndexer) {
					Message(7106, property.Region);
				}
				else if (!property.IsStatic) {
					Message(7107, property);
				}
				else {
					string alias = (string)saa[0] ?? "";
					_propertySemantics[property] = PropertyScriptSemantics.GetAndSetMethods(property.CanGet ? MethodScriptSemantics.InlineCode(alias) : null, property.CanSet ? MethodScriptSemantics.InlineCode(alias + " = {value}") : null);
					return;
				}
			}

			var ipa = GetAttributePositionalArgs(property, IntrinsicPropertyAttribute);
			if (ipa != null) {
				if (property.DeclaringType.Kind == TypeKind.Interface) {
					if (property.IsIndexer)
						Message(7108, property.Region);
					else
						Message(7109, property);
				}
				else if (property.IsOverride && _propertySemantics[(IProperty)InheritanceHelper.GetBaseMember(property).MemberDefinition].Type != PropertyScriptSemantics.ImplType.NotUsableFromScript) {
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
				else if (property.IsExplicitInterfaceImplementation || property.ImplementedInterfaceMembers.Any(m => _propertySemantics[(IProperty)m.MemberDefinition].Type != PropertyScriptSemantics.ImplType.NotUsableFromScript)) {
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

			if (property.IsExplicitInterfaceImplementation && property.ImplementedInterfaceMembers.Any(m => _propertySemantics[(IProperty)m.MemberDefinition].Type == PropertyScriptSemantics.ImplType.NotUsableFromScript)) {
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
				getter = _methodSemantics[property.Getter];
			}
			else {
				getter = null;
			}

			if (property.CanSet) {
				var setterName = DeterminePreferredMemberName(property.Setter);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && property.DeclaringType.Kind != TypeKind.Interface && !Utils.IsPublic(property) ? null : (nameSpecified ? "set_" + preferredName : GetUniqueName("set_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(property.Setter, setterName.Item1, setterName.Item2, usedNames);
				setter = _methodSemantics[property.Setter];
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

			var ssa = GetAttributePositionalArgs(method, ScriptSkipAttribute);
			var saa = GetAttributePositionalArgs(method, ScriptAliasAttribute);
			var ica = GetAttributePositionalArgs(method, InlineCodeAttribute);
			var ifa = GetAttributePositionalArgs(method, InstanceMethodOnFirstArgumentAttribute);
			var nsa = GetAttributePositionalArgs(method, NonScriptableAttribute);
			var iga = GetAttributePositionalArgs(method, IgnoreGenericArgumentsAttribute);
			var ioa = GetAttributePositionalArgs(method, IntrinsicOperatorAttribute);
			var epa = GetAttributePositionalArgs(method, ExpandParamsAttribute);
			var asa = GetAttributePositionalArgs(method, AlternateSignatureAttribute);

			bool isImported = _typeSemantics[method.DeclaringTypeDefinition].IsImported;

			if (nsa != null || _typeSemantics[method.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
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
				bool implementsInterfaceMember = method.IsExplicitInterfaceImplementation || method.ImplementedInterfaceMembers.Any(m => _methodSemantics[(IMethod)m.MemberDefinition].Type != MethodScriptSemantics.ImplType.NotUsableFromScript);

				if (ssa != null) {
					// [ScriptSkip] - Skip invocation of the method entirely.
					if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
						Message(7119, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverride && _methodSemantics[(IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition].Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(7120, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverridable) {
						Message(7121, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (implementsInterfaceMember) {
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
							_methodSemantics[method] = MethodScriptSemantics.InlineCode("{" + method.Parameters[0].Name + "}");
							return;
						}
						else {
							if (method.Parameters.Count != 0)
								Message(7124, method);
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
						Message(7125, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
				}
				else if (ica != null) {
					if (method.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
						Message(7126, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverride && _methodSemantics[(IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition].Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						Message(7127, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (method.IsOverridable) {
						Message(7128, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else if (implementsInterfaceMember) {
						Message(7129, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
					else {
						string code = (string) ica[0];

						var errors = InlineCodeMethodCompiler.ValidateLiteralCode(method, code, t => t.Resolve(_compilation));
						if (errors.Count > 0) {
							Message(7130, method, string.Join(", ", errors));
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
						Message(7131, method);
						_methodSemantics[method] = MethodScriptSemantics.NormalMethod(method.Name);
						return;
					}
				}
				else {
					if (method.IsOverride && _methodSemantics[(IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition].Type != MethodScriptSemantics.ImplType.NotUsableFromScript) {
						if (nameSpecified) {
							Message(7132, method);
						}
						if (iga != null) {
							Message(7133, method);
						}

						var semantics = _methodSemantics[(IMethod)InheritanceHelper.GetBaseMember(method).MemberDefinition];
						if (semantics.Type == MethodScriptSemantics.ImplType.NormalMethod) {
							var errorMethod = method.ImplementedInterfaceMembers.FirstOrDefault(im => GetMethodSemantics((IMethod)im.MemberDefinition).Name != semantics.Name);
							if (errorMethod != null) {
								Message(7134, method, GetQualifiedMemberName(errorMethod));
							}
						}

						_methodSemantics[method] = semantics;
						return;
					}
					else if (implementsInterfaceMember) {
						if (nameSpecified) {
							Message(7135, method);
						}

						if (method.ImplementedInterfaceMembers.Select(im => GetMethodSemantics((IMethod)im.MemberDefinition)).Where(sem => sem.Type == MethodScriptSemantics.ImplType.NormalMethod).Select(sem => sem.Name).Distinct().Count() > 1) {
							Message(7136, method);
						}

						// If the method implements more than one interface member, prefer to take the implementation from one that is not unusable.
						_methodSemantics[method] = method.ImplementedInterfaceMembers.Select(im => _methodSemantics[(IMethod)im.MemberDefinition]).FirstOrDefault(sem => sem.Type != MethodScriptSemantics.ImplType.NotUsableFromScript) ?? MethodScriptSemantics.NotUsableFromScript();
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
								_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")");
								return;
							}
						}
						else if (_typeSemantics[method.DeclaringTypeDefinition].IsGlobalMethods) {
							_methodSemantics[method] = MethodScriptSemantics.NormalMethod(preferredName, isGlobal: _typeSemantics[method.DeclaringTypeDefinition].IsGlobalMethods, ignoreGenericArguments: iga != null || isImported, expandParams: epa != null);
							return;
						}
						else {
							string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
							if (asa == null)
								usedNames[name] = true;
							if (_typeSemantics[method.DeclaringTypeDefinition].IsSerializable && !method.IsStatic) {
								_methodSemantics[method] = MethodScriptSemantics.StaticMethodWithThisAsFirstArgument(name, generateCode: GetAttributePositionalArgs(method, AlternateSignatureAttribute) == null, ignoreGenericArguments: iga != null || isImported, expandParams: epa != null);
							}
							else {
								if (_typeSemantics[method.DeclaringTypeDefinition].IsTestFixture && name == "runTests") {
									Message(7019, method);
								}
								var sta = method.Attributes.FirstOrDefault(a => a.AttributeType.FullName == TestAttribute);
								var ata = method.Attributes.FirstOrDefault(a => a.AttributeType.FullName == AsyncTestAttribute);
								if (sta != null && ata != null) {
									Message(7021, method);
								}
								else if (sta != null || ata != null) {
									if (!_typeSemantics[method.DeclaringTypeDefinition].IsTestFixture) {
										Message(7022, method);
									}
									if (!method.ReturnType.Equals(_compilation.FindType(KnownTypeCode.Void)) || method.TypeParameters.Any() || method.Parameters.Any() || method.IsStatic || !method.IsPublic) {
										Message(7020, method);
									}
									else {
										var ta = sta ?? ata;
										bool isAsync = ata != null;
										string description = (ta.PositionalArguments.Count > 0 ? (string)ta.PositionalArguments[0].ConstantValue : null) ?? method.Name;
										string category = GetNamedArgument<string>(ta, CategoryPropertyName);
										int? expectedAssertionCount = GetNamedArgument<int?>(ta, ExpectedAssertionCountPropertyName) ?? -1;
										_methodTestData[method] = new TestMethodData(description, category, isAsync, expectedAssertionCount >= 0 ? expectedAssertionCount : (int?)null);
									}
								}

								_methodSemantics[method] = MethodScriptSemantics.NormalMethod(name, generateCode: GetAttributePositionalArgs(method, AlternateSignatureAttribute) == null, ignoreGenericArguments: iga != null || isImported, expandParams: epa != null);
							}
						}
					}
				}
			}
		}

		private void ProcessEvent(IEvent evt, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (_typeSemantics[evt.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || GetAttributePositionalArgs(evt, NonScriptableAttribute) != null) {
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
				adder = _methodSemantics[evt.AddAccessor];
			}
			else {
				adder = null;
			}

			if (evt.CanRemove) {
				var setterName = DeterminePreferredMemberName(evt.RemoveAccessor);
				if (!setterName.Item2)
					setterName = Tuple.Create(!nameSpecified && _minimizeNames && evt.DeclaringType.Kind != TypeKind.Interface && !Utils.IsPublic(evt) ? null : (nameSpecified ? "remove_" + preferredName : GetUniqueName("remove_" + preferredName, usedNames)), false);	// If the name was not specified, generate one.

				ProcessMethod(evt.RemoveAccessor, setterName.Item1, setterName.Item2, usedNames);
				remover = _methodSemantics[evt.RemoveAccessor];
			}
			else {
				remover = null;
			}

			_eventSemantics[evt] = EventScriptSemantics.AddAndRemoveMethods(adder, remover);
		}

		private void ProcessField(IField field, string preferredName, bool nameSpecified, Dictionary<string, bool> usedNames) {
			if (_typeSemantics[field.DeclaringTypeDefinition].Semantics.Type == TypeScriptSemantics.ImplType.NotUsableFromScript || GetAttributePositionalArgs(field, NonScriptableAttribute) != null) {
				_fieldSemantics[field] = FieldScriptSemantics.NotUsableFromScript();
			}
			else if (preferredName == "") {
				Message(7142, field);
				_fieldSemantics[field] = FieldScriptSemantics.Field("X");
			}
			else {
				string name = nameSpecified ? preferredName : GetUniqueName(preferredName, usedNames);
				usedNames[name] = true;
				if (_typeSemantics[field.DeclaringTypeDefinition].IsNamedValues) {
					string value = preferredName;
					if (!nameSpecified) {	// This code handles the feature that it is possible to specify an invalid ScriptName for a member of a NamedValues enum, in which case that value has to be use as the constant value.
						var sna = GetAttributePositionalArgs(field, ScriptNameAttribute);
						if (sna != null)
							value = (string)sna[0];
					}

					_fieldSemantics[field] = FieldScriptSemantics.StringConstant(value, name);
				}
				else if (field.IsConst && (field.DeclaringType.Kind == TypeKind.Enum || _minimizeNames)) {
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

		public void Prepare(IEnumerable<ITypeDefinition> types, IAssembly mainAssembly, IErrorReporter errorReporter) {
			_systemObject = mainAssembly.Compilation.FindType(KnownTypeCode.Object);
			_systemRecord = ReflectionHelper.ParseReflectionName("System.Record").Resolve(mainAssembly.Compilation.TypeResolveContext);
			_errorReporter = errorReporter;
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
			_methodTestData = new Dictionary<IMethod, TestMethodData>();

			var sna = mainAssembly.AssemblyAttributes.SingleOrDefault(a => a.AttributeType.FullName == ScriptNamespaceAttribute);
			if (sna != null) {
				var arg = (string)sna.PositionalArguments[0].ConstantValue;
				if (arg == null || (arg != "" && !arg.IsValidNestedJavaScriptIdentifier())) {
					Message(7002, sna.Region, "assembly");
				}
			}

			var sca = mainAssembly.AssemblyAttributes.SingleOrDefault(a => a.AttributeType.FullName == ScriptSharpCompatibilityAttribute);
			if (sca != null) {
				var oc = sca.NamedArguments.SingleOrDefault(x => x.Key.Name == OmitDowncastsPropertyName).Value;
				if (oc != null)
					_omitDowncasts = (bool)oc.ConstantValue;
				var on = sca.NamedArguments.SingleOrDefault(x => x.Key.Name == OmitNullableChecksPropertyName).Value;
				if (on != null)
					_omitNullableChecks = (bool)on.ConstantValue;
			}

			foreach (var t in types.OrderBy(x => x.ParentAssembly.AssemblyName).ThenBy(x => x.ReflectionName)) {
				try {
					ProcessType(t);
					ProcessTypeMembers(t);
				}
				catch (Exception ex) {
					errorReporter.Region = t.Region;
					errorReporter.InternalError(ex, "Error importing type " + t.FullName);
				}
			}
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
			switch (method.DeclaringType.Kind) {
				case TypeKind.Delegate:
					return MethodScriptSemantics.NotUsableFromScript();
				default:
					return _methodSemantics[(IMethod)method.MemberDefinition];
			}
		}

		public ConstructorScriptSemantics GetConstructorSemantics(IMethod method) {
			switch (method.DeclaringType.Kind) {
				case TypeKind.Anonymous:
					return ConstructorScriptSemantics.Json(new IMember[0]);
				case TypeKind.Delegate:
					return ConstructorScriptSemantics.NotUsableFromScript();
				default:
					return _constructorSemantics[(IMethod)method.MemberDefinition];
			}
		}

		public PropertyScriptSemantics GetPropertySemantics(IProperty property) {
			switch (property.DeclaringType.Kind) {
				case TypeKind.Anonymous:
					return PropertyScriptSemantics.Field(property.Name.Replace("<>", "$"));
				case TypeKind.Delegate:
					return PropertyScriptSemantics.NotUsableFromScript();
				default:
					return _propertySemantics[(IProperty)property.MemberDefinition];
			}
		}

		public DelegateScriptSemantics GetDelegateSemantics(ITypeDefinition delegateType) {
			return _delegateSemantics[delegateType];
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
					return _fieldSemantics[(IField)field.MemberDefinition];
			}
		}

		public EventScriptSemantics GetEventSemantics(IEvent evt) {
			switch (evt.DeclaringType.Kind) {
				case TypeKind.Delegate:
					return EventScriptSemantics.NotUsableFromScript();
				default:
					return _eventSemantics[(IEvent)evt.MemberDefinition];
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
			return _typeSemantics[t].IsNamedValues;
		}

		public bool IsResources(ITypeDefinition t) {
			return _typeSemantics[t].IsResources;
		}

		public string GetGlobalMethodsPrefix(ITypeDefinition t) {
			var s = _typeSemantics[t];
			if (s.MixinArg != null)
				return s.MixinArg;
			else if (s.IsGlobalMethods)
				return "";
			else
				return null;
		}

		public bool IsSerializable(ITypeDefinition t) {
			return _typeSemantics[t].IsSerializable;
		}

		public bool IsRealType(ITypeDefinition t) {
			return _typeSemantics[t].IsRealType;
		}

		public bool IsTestFixture(ITypeDefinition t) {
			return _typeSemantics[t].IsTestFixture;
		}

		public TestMethodData GetTestData(IMethod m) {
			TestMethodData result;
			_methodTestData.TryGetValue(m, out result);
			return result;
		}

		public bool OmitDowncasts {
			get {  return _omitDowncasts; }
		}

		public bool OmitNullableChecks {
			get {  return _omitNullableChecks; }
		}
	}
}
