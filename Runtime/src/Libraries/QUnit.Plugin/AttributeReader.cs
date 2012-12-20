using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.TypeSystem;

namespace QUnit.Plugin {
	public class AttributeReader {
		private static object ChangeType(object source, Type type) {
			if (type.IsArray) {
				var arr = (Array)source;
				var result = new object[arr.Length];
				for (int i = 0; i < arr.Length; i++) {
					result[i] = ChangeType(arr.GetValue(i), type.GetElementType());
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
					prop.SetValue(result, value);
				}
			}

			return result;
		}

		public static TAttribute ReadAttribute<TAttribute>(IEntity entity) where TAttribute : Attribute {
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(TAttribute).FullName);
			return attr != null ? ReadAttribute<TAttribute>(attr) : null;
		}
	}
}
