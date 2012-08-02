using System;
using System.Globalization;
using System.Text;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class JsConstantExpression : JsExpression {
        public class RegexpData {
            public string Pattern { get; private set; }
            public string Options { get; private set; }

            public RegexpData(string pattern, string options) {
                if (pattern == null) throw new ArgumentNullException("pattern");

                Pattern = pattern;
                Options = options;
            }
        }

        private readonly object _value;

        public RegexpData RegexpValue {
            get {
                if (NodeType != ExpressionNodeType.Regexp) throw new InvalidOperationException();
                return (RegexpData)_value;
            }
        }

        public double NumberValue {
            get {
                if (NodeType != ExpressionNodeType.Number) throw new InvalidOperationException();
                return (double)_value;
            }
        }

        public string StringValue {
            get {
                if (NodeType != ExpressionNodeType.String) throw new InvalidOperationException();
                return (string)_value;
            }
        }

        public bool BooleanValue {
            get {
                if (NodeType != ExpressionNodeType.Boolean) throw new InvalidOperationException();
                return (bool)_value;
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitConstantExpression(this, data);
        }

        public override string ToString() {
            return _value != null ? _value.ToString() : "null";
        }

        // Static factory methods.

        internal JsConstantExpression(RegexpData regexp) : base(ExpressionNodeType.Regexp) {
            if (regexp == null) throw new ArgumentNullException("regexp");
            _value = regexp;
        }

        internal JsConstantExpression(double value) : base(ExpressionNodeType.Number) {
            _value = value;
        }

        internal JsConstantExpression(string value) : base(ExpressionNodeType.String) {
            if (value == null) throw new ArgumentNullException("value");
            _value = value;
        }

        private JsConstantExpression(bool value) : base(ExpressionNodeType.Boolean) {
            _value = value;
        }

        private JsConstantExpression() : base(ExpressionNodeType.Null) {
        }

        private static readonly JsConstantExpression _null = new JsConstantExpression();
        private static readonly JsConstantExpression _true = new JsConstantExpression(true);
        private static readonly JsConstantExpression _false = new JsConstantExpression(false);

        public static new JsConstantExpression Null { get { return _null; } }
		public static new JsConstantExpression True { get { return _true; } }
		public static new JsConstantExpression False { get { return _false; } }
    }
}
