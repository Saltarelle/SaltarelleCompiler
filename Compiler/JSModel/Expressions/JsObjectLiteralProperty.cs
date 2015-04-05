using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.Expressions {
	public enum ObjectLiteralPropertyKind {
		Expression,
		GetAccessor,
		SetAccessor,
	}

	public class JsObjectLiteralProperty {
		public string Name { get; private set; }
		public ObjectLiteralPropertyKind Kind { get; set; }
		public JsExpression Value { get; private set; }

		public JsObjectLiteralProperty(string name, JsExpression value) : this(name, ObjectLiteralPropertyKind.Expression, value) {
		}

		public JsObjectLiteralProperty(string name, ObjectLiteralPropertyKind kind, JsExpression value) {
			if (name == null) throw new ArgumentNullException("name");
			if (value == null) throw new ArgumentNullException("value");
			if (kind != ObjectLiteralPropertyKind.Expression && (!(value is JsFunctionDefinitionExpression) || ((JsFunctionDefinitionExpression)value).Name != null))
				throw new ArgumentException("For accessors, the value must be a nameless function");

			Name  = name;
			Kind = kind;
			Value = value;
		}
	}
}
