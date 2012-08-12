using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler {
    class Utils {
        public static bool IsPublic(ITypeDefinition type) {
            // A type is public if the type and all its declaring types are public or protected (or protected internal).
            while (type != null) {
                bool isPublic = (type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Protected || type.Accessibility == Accessibility.ProtectedOrInternal);
                if (!isPublic)
                    return false;
                type = type.DeclaringTypeDefinition;
            }
            return true;
        }

		public static bool IsPublic(IMember member) {
			return IsPublic(member.DeclaringType.GetDefinition()) && (member.Accessibility == Accessibility.Public || member.Accessibility == Accessibility.Protected || member.Accessibility == Accessibility.ProtectedOrInternal);
		}

		private static void FindUsedUnusableTypes(IEnumerable<IType> types, INamingConventionResolver namingConvention, HashSet<ITypeDefinition> result) {
			foreach (var t in types) {
				if (t is ITypeDefinition) {
					if (namingConvention.GetTypeSemantics((ITypeDefinition)t).Type == TypeScriptSemantics.ImplType.NotUsableFromScript)
						result.Add((ITypeDefinition)t);
				}
				else if (t is ParameterizedType) {
					var pt = (ParameterizedType)t;
					FindUsedUnusableTypes(new[] { pt.GetDefinition() }.Concat(pt.TypeArguments), namingConvention, result);
				}
			}
		}

		public static IEnumerable<ITypeDefinition> FindUsedUnusableTypes(IEnumerable<IType> types, INamingConventionResolver namingConvention) {
			var s = new HashSet<ITypeDefinition>();
			FindUsedUnusableTypes(types, namingConvention, s);
			return s;
		}
    }
}
