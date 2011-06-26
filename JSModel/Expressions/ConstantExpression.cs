using System;

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

        private ConstantExpression(ConstantType type, object value) {
            Type = type;
            _value = value;
        }

        public static ConstantExpression Regexp(string pattern, string options) {
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

        private static readonly ConstantExpression _null = new ConstantExpression(ConstantType.Null, null);
        public static ConstantExpression Null { get { return _null; } }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }

        public override string ToString() {
            return _value != null ? _value.ToString() : "null";
        }
    }

    public enum ConstantType {
        Number,
        String,
        Regexp,
        Null,
    }
}
