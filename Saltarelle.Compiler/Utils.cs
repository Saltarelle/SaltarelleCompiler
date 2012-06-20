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

		public static JsTypeReferenceExpression CreateJsTypeReferenceExpression(ITypeDefinition type, INamingConventionResolver namingConvention) {
			var sem = namingConvention.GetTypeSemantics(type);
			if (sem.Type != TypeScriptSemantics.ImplType.NormalType)
				throw new InvalidOperationException("Cannot create a reference to the type " + type.FullName);
			return new JsTypeReferenceExpression(type.ParentAssembly, sem.Name);
		}

		public static object ConvertToDoubleOrStringOrBoolean(object value) {
			if (value is bool || value is string || value == null)
				return value;
			else if (value is sbyte)
				return (double)(sbyte)value;
			else if (value is byte)
				return (double)(byte)value;
			else if (value is char)
				return (double)(char)value;
			else if (value is short)
				return (double)(short)value;
			else if (value is ushort)
				return (double)(ushort)value;
			else if (value is int)
				return (double)(int)value;
			else if (value is uint)
				return (double)(uint)value;
			else if (value is long)
				return (double)(long)value;
			else if (value is ulong)
				return (double)(ulong)value;
			else if (value is float)
				return (double)(float)value;
			else if (value is double)
				return (double)value;
			else if (value is decimal)
				return (double)(decimal)value;
			else
				throw new NotSupportedException("Unsupported constant " + value.ToString() + "(" + value.GetType().ToString() + ")");
		}
    }
}
