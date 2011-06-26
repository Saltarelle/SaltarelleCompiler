using System;
using System.Globalization;
using System.Text;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class ConstantExpression : Expression {
        public class RegexpData {
            public string Pattern { get; private set; }
            public string Options { get; private set; }

            public RegexpData(string pattern, string options) {
                Pattern = pattern;
                Options = options;
            }
        }

        public ConstantType Type { get; private set; }
        private readonly object _value;

        public override int Precedence { get { return ExpressionPrecedence.Terminal; } }

        private ConstantExpression(ConstantType type, object value) {
            Type = type;
            _value = value;
        }

        public RegexpData RegexpValue {
            get {
                if (Type != ConstantType.Regexp) throw new InvalidOperationException();
                return (RegexpData)_value;
            }
        }

        public double NumberValue {
            get {
                if (Type != ConstantType.Number) throw new InvalidOperationException();
                return (double)_value;
            }
        }

        public string StringValue {
            get {
                if (Type != ConstantType.String) throw new InvalidOperationException();
                return (string)_value;
            }
        }

        public string Format() {
            switch (Type) {
                case ConstantType.Null:
                    return "null";
                case ConstantType.Number:
                    return NumberValue.ToString(CultureInfo.InvariantCulture);
                case ConstantType.Regexp:
                    return "/" + FixStringLiteral(RegexpValue.Pattern, true) + "/" + RegexpValue.Options;
                case ConstantType.String:
                    return "'" + FixStringLiteral(StringValue, false) + "'";
                default:
                    throw new ArgumentException("expression");
            }
        }

        private static string FixStringLiteral(string s, bool isRegexp) {
			var sb = new StringBuilder();
			for (int i = 0; i < s.Length; i++) {
				switch (s[i]) {
					case '\'': sb.Append("\\\'"); break;
					case '\\': sb.Append("\\\\"); break;
					case '\r': sb.Append("\\r"); break;
					case '\n': sb.Append("\\n"); break;
                    case '/':  sb.Append(isRegexp ? "\\/" : "/"); break;
					default:   sb.Append(s[i]); break;
				}
			}
			return sb.ToString();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }

        public override string ToString() {
            return _value != null ? _value.ToString() : "null";
        }

        // Static factory methods.

        public static ConstantExpression Regexp(string pattern, string options = null) {
            if (pattern == null) throw new ArgumentNullException("pattern");
            return new ConstantExpression(ConstantType.Regexp, new RegexpData(pattern, options));
        }

        public static ConstantExpression Number(double value) {
            return new ConstantExpression(ConstantType.Number, value);
        }

        public static ConstantExpression String(string value) {
            if (value == null) throw new ArgumentNullException("value");
            return new ConstantExpression(ConstantType.String, value);
        }

        private static readonly ConstantExpression _null = new ConstantExpression(ConstantType.Null, null);

        public static ConstantExpression Null { get { return _null; } }
    }

    public enum ConstantType {
        Number,
        String,
        Regexp,
        Null,
    }
}
