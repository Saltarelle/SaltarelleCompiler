using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.MetadataImporter {
	public class ScriptSharpMetadataUtils {
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
			return (mna != null && !string.IsNullOrEmpty((string)mna.ModuleName) ? mna.ModuleName : null);
		}

		public static string GetModuleName(ITypeDefinition type) {
			for (var current = type; current != null; current = current.DeclaringTypeDefinition) {
				var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(type);
				if (mna != null)
					return !string.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null;
			}
			return GetModuleName(type.ParentAssembly);
		}
	}
}
