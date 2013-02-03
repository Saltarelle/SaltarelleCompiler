using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler {
	public static class Utils {
		/// <summary>
		/// A type is externally visible if it and all its declaring types are public or protected (or protected internal).
		/// </summary>
		public static bool IsExternallyVisible(this ITypeDefinition type) {
			while (type != null) {
				bool isPublic = (type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Protected || type.Accessibility == Accessibility.ProtectedOrInternal);
				if (!isPublic)
					return false;
				type = type.DeclaringTypeDefinition;
			}
			return true;
		}

		/// <summary>
		/// A member is externally visible if it is public or protected (or protected internal) and its declaring type is externally visible.
		/// </summary>
		public static bool IsExternallyVisible(this IMember member) {
			return IsExternallyVisible(member.DeclaringType.GetDefinition()) && (member.Accessibility == Accessibility.Public || member.Accessibility == Accessibility.Protected || member.Accessibility == Accessibility.ProtectedOrInternal);
		}

		private static void FindUsedUnusableTypes(IEnumerable<IType> types, IMetadataImporter metadataImporter, HashSet<ITypeDefinition> result) {
			foreach (var t in types) {
				if (t is ITypeDefinition) {
					if (metadataImporter.GetTypeSemantics((ITypeDefinition)t).Type == TypeScriptSemantics.ImplType.NotUsableFromScript)
						result.Add((ITypeDefinition)t);
				}
				else if (t is ParameterizedType) {
					var pt = (ParameterizedType)t;
					FindUsedUnusableTypes(new[] { pt.GetDefinition() }.Concat(pt.TypeArguments), metadataImporter, result);
				}
			}
		}

		public static IEnumerable<ITypeDefinition> FindUsedUnusableTypes(IEnumerable<IType> types, IMetadataImporter metadataImporter) {
			var s = new HashSet<ITypeDefinition>();
			FindUsedUnusableTypes(types, metadataImporter, s);
			return s;
		}

		/// <summary>
		/// For generic types, returns a ParameterizedType with each type argument being a TypeParameter with the name of the type parameter in the type definition. Returns the TypeDefinition itself for non-generic types.
		/// </summary>
		public static IType SelfParameterize(ITypeDefinition type) {
			return type.TypeParameterCount == 0 ? (IType)type : new ParameterizedType(type, type.TypeParameters.Select(tp => new DefaultTypeParameter(type, tp.Index, tp.Name)));
		}
	}
}
