using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public static class AttributeReader {
		private static object ChangeType(object source, Type type) {
			if (type.IsArray) {
				var arr = (Array)source;
				var elemType = type.GetElementType();
				var result = Array.CreateInstance(elemType, arr.Length);
				for (int i = 0; i < arr.Length; i++) {
					result.SetValue(ChangeType(arr.GetValue(i), elemType), i);
				}
				return result;
			}
			else {
				return Convert.ChangeType(source, type);
			}
		}

		public static TAttribute ReadAttribute<TAttribute>(IAttribute attr) where TAttribute : Attribute {
			var ctorArgTypes = new Type[attr.PositionalArguments.Count];
			var ctorArgs = new object[attr.PositionalArguments.Count];
			for (int i = 0; i < attr.PositionalArguments.Count; i++) {
				var arg = attr.PositionalArguments[i];
				ctorArgTypes[i] = Type.GetType(arg.Type.FullName);
				ctorArgs[i]     = ChangeType(arg.ConstantValue, ctorArgTypes[i]);
			}
			var ctor = typeof(TAttribute).GetConstructor(ctorArgTypes);
			var result = (TAttribute)ctor.Invoke(ctorArgs);

			foreach (var arg in attr.NamedArguments) {
				var value = ChangeType(arg.Value.ConstantValue, Type.GetType(arg.Value.Type.FullName));
				if (arg.Key is IField) {
					var fld = typeof(TAttribute).GetField(arg.Key.Name);
					fld.SetValue(result, value);
				}
				else if (arg.Key is IProperty) {
					var prop = typeof(TAttribute).GetProperty(arg.Key.Name);
					prop.SetValue(result, value, null);
				}
			}

			return result;
		}

		public static TAttribute ReadAttribute<TAttribute>(IEntity entity) where TAttribute : Attribute {
			return ReadAttribute<TAttribute>(entity.Attributes);
		}

		public static TAttribute ReadAttribute<TAttribute>(IEnumerable<IAttribute> attributes) where TAttribute : Attribute {
			var attr = attributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(TAttribute).FullName);
			return attr != null ? ReadAttribute<TAttribute>(attr) : null;
		}

		public static bool HasAttribute<TAttribute>(IEntity entity) where TAttribute : Attribute {
			return HasAttribute<TAttribute>(entity.Attributes);
		}

		public static bool HasAttribute<TAttribute>(IEnumerable<IAttribute> attributes) where TAttribute : Attribute {
			return attributes.Any(a => a.AttributeType.FullName == typeof(TAttribute).FullName);
		}
	}
}
