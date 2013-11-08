using System;

namespace Saltarelle.Compiler.ScriptSemantics {
	public class FieldScriptSemantics {
		public enum ImplType {
			/// <summary>
			/// The field is a field in script. The Name member is valid.
			/// </summary>
			Field,
			/// <summary>
			/// The field should be emitted as a constant. The Value field is valid, and has to be a boolean, a double, a string, or null.
			/// </summary>
			Constant,
			/// <summary>
			/// The field is not usable from script. No other members are valid.
			/// </summary>
			NotUsableFromScript,
		}

		public ImplType Type { get; private set; }
		private string _name;

		public String Name {
			get {
				if (Type != ImplType.Field && Type != ImplType.Constant)
					throw new InvalidOperationException();
				return _name;
			}
		}

		private object _value;
		public object Value {
			get {
				if (Type != ImplType.Constant)
					throw new InvalidOperationException();
				return _value;
			}
		}

		/// <summary>
		/// Whether this field should appear in the type declaring it.
		/// </summary>
		public bool GenerateCode { get { return _name != null; } }

		private FieldScriptSemantics() {
		}

		public static FieldScriptSemantics Field(string name) {
			return new FieldScriptSemantics { Type = ImplType.Field, _name = name };
		}

		public static FieldScriptSemantics NotUsableFromScript() {
			return new FieldScriptSemantics { Type = ImplType.NotUsableFromScript };
		}

		public static FieldScriptSemantics NullConstant(string name = null) {
			return new FieldScriptSemantics { Type = ImplType.Constant, _value = null, _name = name };
		}

		public static FieldScriptSemantics StringConstant(string value, string name = null) {
			return new FieldScriptSemantics { Type = ImplType.Constant, _value = value, _name = name };
		}

		public static FieldScriptSemantics NumericConstant(double value, string name = null) {
			return new FieldScriptSemantics { Type = ImplType.Constant, _value = value, _name = name };
		}

		public static FieldScriptSemantics BooleanConstant(bool value, string name = null) {
			return new FieldScriptSemantics { Type = ImplType.Constant, _value = value, _name = name };
		}
	}
}