using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel
{
	public static class Utils {
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

		public static JsExpression MakeConstantExpression(object value) {
			value = Utils.ConvertToDoubleOrStringOrBoolean(value);
			if (value is bool)
				return (bool)value ? JsExpression.True : JsExpression.False;
			else if (value is double)
				return JsExpression.Number((double)value);
			if (value is string)
				return JsExpression.String((string)value);
			else if (value == null)
				return JsExpression.Null;
			else
				throw new ArgumentException("value");
		}

		private static readonly HashSet<string> _keywords = new HashSet<string>() { "abstract", "arguments", "as", "boolean", "break", "byte", "case", "catch", "char", "class", "continue", "const", "debugger", "default", "delete", "do", "double", "else", "enum", "eval", "export", "extends", "false", "final", "finally", "float", "for", "function", "goto", "if", "implements", "import", "in", "instanceof", "int", "interface", "is", "let", "long", "namespace", "native", "new", "null", "package", "private", "protected", "public", "return", "short", "static", "super", "switch", "synchronized", "this", "throw", "throws", "transient", "true", "try", "typeof", "use", "var", "void", "volatile", "while", "with", "yield", };
		public static readonly ReadOnlyCollection<string> AllKeywords = _keywords.AsReadOnly();

		public static bool IsJavaScriptReservedWord(string word) {
			return _keywords.Contains(word);
		}
	}
}
