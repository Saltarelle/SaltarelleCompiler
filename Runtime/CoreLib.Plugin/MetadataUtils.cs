using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Utils = Saltarelle.Compiler.JSModel.Utils;

namespace CoreLib.Plugin {
	public static class MetadataUtils {
		public static string MakeCamelCase(string s) {
			if (String.IsNullOrEmpty(s))
				return s;
			if (s.Equals("ID", StringComparison.Ordinal))
				return "id";

			bool hasNonUppercase = false;
			int numUppercaseChars = 0;
			for (int index = 0; index < s.Length; index++) {
				if (Char.IsUpper(s, index)) {
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
				return Char.ToLower(s[0], CultureInfo.InvariantCulture) + s.Substring(1);
		}

		public static bool IsSerializable(ITypeDefinition type) {
			return AttributeReader.HasAttribute<SerializableAttribute>(type) || (type.GetAllBaseTypeDefinitions().Any(td => td.FullName == "System.Record") && type.FullName != "System.Record");
		}

		public static bool DoesTypeObeyTypeSystem(ITypeDefinition type) {
			var ia = AttributeReader.ReadAttribute<ImportedAttribute>(type);
			return ia == null || ia.ObeysTypeSystem;
		}

		public static bool IsMixin(ITypeDefinition type) {
			return AttributeReader.HasAttribute<MixinAttribute>(type);
		}

		public static bool IsImported(ITypeDefinition type) {
			return AttributeReader.HasAttribute<ImportedAttribute>(type);
		}

		public static bool IsResources(ITypeDefinition type) {
			return AttributeReader.HasAttribute<ResourcesAttribute>(type);
		}

		public static bool IsNamedValues(ITypeDefinition type) {
			return AttributeReader.HasAttribute<NamedValuesAttribute>(type);
		}

		public static bool IsGlobalMethods(ITypeDefinition type) {
			return AttributeReader.HasAttribute<GlobalMethodsAttribute>(type);
		}

		public static bool IsPreserveMemberCase(ITypeDefinition type) {
			var pmca = AttributeReader.ReadAttribute<PreserveMemberCaseAttribute>(type) ?? AttributeReader.ReadAttribute<PreserveMemberCaseAttribute>(type.ParentAssembly.AssemblyAttributes);
			return pmca != null && pmca.Preserve;
		}

		public static bool IsPreserveMemberNames(ITypeDefinition type) {
			return IsImported(type) || IsGlobalMethods(type);
		}

		public static bool OmitNullableChecks(ICompilation compilation) {
			var sca = AttributeReader.ReadAttribute<ScriptSharpCompatibilityAttribute>(compilation.MainAssembly.AssemblyAttributes);
			return sca != null && sca.OmitNullableChecks;
		}

		public static bool OmitDowncasts(ICompilation compilation) {
			var sca = AttributeReader.ReadAttribute<ScriptSharpCompatibilityAttribute>(compilation.MainAssembly.AssemblyAttributes);
			return sca != null && sca.OmitDowncasts;
		}

		public static bool IsAsyncModule(IAssembly assembly) {
			return AttributeReader.HasAttribute<AsyncModuleAttribute>(assembly.AssemblyAttributes);
		}

		public static string GetModuleName(IAssembly assembly) {
			var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(assembly.AssemblyAttributes);
			return (mna != null && !String.IsNullOrEmpty((string)mna.ModuleName) ? mna.ModuleName : null);
		}

		public static string GetModuleName(ITypeDefinition type) {
			for (var current = type; current != null; current = current.DeclaringTypeDefinition) {
				var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(type);
				if (mna != null)
					return !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null;
			}
			return GetModuleName(type.ParentAssembly);
		}

		public static bool? ShouldGenericArgumentsBeIncluded(ITypeDefinition type) {
			var iga = AttributeReader.ReadAttribute<IncludeGenericArgumentsAttribute>(type);
			if (iga != null)
				return iga.Include;
			var imp = AttributeReader.ReadAttribute<ImportedAttribute>(type);
			if (imp != null)
				return false;
			var def = AttributeReader.ReadAttribute<IncludeGenericArgumentsDefaultAttribute>(type.ParentAssembly.AssemblyAttributes);
			switch (def != null ? def.TypeDefault : GenericArgumentsDefault.IncludeExceptImported) {
				case GenericArgumentsDefault.IncludeExceptImported:
					return true;
				case GenericArgumentsDefault.Ignore:
					return false;
				case GenericArgumentsDefault.RequireExplicitSpecification:
					return null;
				default:
					throw new ArgumentException("Invalid generic arguments default " + def.TypeDefault);
			}
		}

		public static bool? ShouldGenericArgumentsBeIncluded(IMethod method) {
			var iga = AttributeReader.ReadAttribute<IncludeGenericArgumentsAttribute>(method);
			if (iga != null)
				return iga.Include;
			var imp = AttributeReader.ReadAttribute<ImportedAttribute>(method.DeclaringTypeDefinition);
			if (imp != null)
				return false;
			var def = AttributeReader.ReadAttribute<IncludeGenericArgumentsDefaultAttribute>(method.ParentAssembly.AssemblyAttributes);
			switch (def != null ? def.MethodDefault : GenericArgumentsDefault.IncludeExceptImported) {
				case GenericArgumentsDefault.IncludeExceptImported:
					return true;
				case GenericArgumentsDefault.Ignore:
					return false;
				case GenericArgumentsDefault.RequireExplicitSpecification:
					return null;
				default:
					throw new ArgumentException("Invalid generic arguments default " + def.TypeDefault);
			}
		}

		public static IMember UnwrapValueTypeConstructor(IMember m) {
			if (m is IMethod && !m.IsStatic && m.DeclaringType.Kind == TypeKind.Struct && ((IMethod)m).IsConstructor && ((IMethod)m).Parameters.Count == 0) {
				var other = m.DeclaringType.GetConstructors().SingleOrDefault(c => c.Parameters.Count == 1 && c.Parameters[0].Type.FullName == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName);
				if (other != null)
					return other;
			}
			return m;
		}

		public static bool CanBeMinimized(ITypeDefinition typeDefinition) {
			return !typeDefinition.IsExternallyVisible() || AttributeReader.HasAttribute<MinimizePublicNamesAttribute>(typeDefinition.ParentAssembly.AssemblyAttributes);
		}

		public static bool CanBeMinimized(IMember member) {
			return !member.IsExternallyVisible() || AttributeReader.HasAttribute<MinimizePublicNamesAttribute>(member.ParentAssembly.AssemblyAttributes);
		}

		/// <summary>
		/// Determines the preferred name for a member. The first item is the name, the second item is true if the name was explicitly specified.
		/// </summary>
		public static Tuple<string, bool> DeterminePreferredMemberName(IMember member, bool minimizeNames) {
			member = UnwrapValueTypeConstructor(member);

			bool isConstructor = member is IMethod && ((IMethod)member).IsConstructor;
			bool isAccessor = member is IMethod && ((IMethod)member).IsAccessor;
			bool isPreserveMemberCase = IsPreserveMemberCase(member.DeclaringTypeDefinition);

			string defaultName;
			if (isConstructor) {
				defaultName = "$ctor";
			}
			else if (!CanBeMinimized(member)) {
				defaultName = isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name);
			}
			else {
				if (minimizeNames && member.DeclaringType.Kind != TypeKind.Interface)
					defaultName = null;
				else
					defaultName = "$" + (isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name));
			}

			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(member);
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && !AttributeReader.HasAttribute<AlternateSignatureAttribute>(m) && !AttributeReader.HasAttribute<NonScriptableAttribute>(m) && !AttributeReader.HasAttribute<InlineCodeAttribute>(m)).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0], minimizeNames);
				}
				else {
					return Tuple.Create(member.Name, false);	// Error
				}
			}

			var sna = AttributeReader.ReadAttribute<ScriptNameAttribute>(member);
			if (sna != null) {
				string name = sna.Name;
				if (IsNamedValues(member.DeclaringTypeDefinition) && (name == "" || !name.IsValidJavaScriptIdentifier())) {
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
			                                                       || IsPreserveMemberNames(member.DeclaringTypeDefinition) && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (IsSerializable(member.DeclaringTypeDefinition) && !member.IsStatic && (member is IProperty || member is IField)))
			                                                       || (IsNamedValues(member.DeclaringTypeDefinition) && member is IField);

			if (preserveName)
				return Tuple.Create(isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name), true);

			return Tuple.Create(defaultName, false);
		}

		private static readonly string _encodeNumberTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string EncodeNumber(int i, bool ensureValidIdentifier) {
			if (ensureValidIdentifier) {
				string result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1);
				while (i >= _encodeNumberTable.Length - 10) {
					i /= _encodeNumberTable.Length - 10;
					result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1) + result;
				}
				return Utils.IsJavaScriptReservedWord(result) ? "_" + result : result;
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

		public static string GetUniqueName(string preferredName, Func<string, bool> isNameAvailable) {
			string name = preferredName;
			int i = (name == null ? 0 : 1);
			while (name == null || !isNameAvailable(name)) {
				name = preferredName + "$" + MetadataUtils.EncodeNumber(i, false);
				i++;
			}
			return name;
		}
	}
}
