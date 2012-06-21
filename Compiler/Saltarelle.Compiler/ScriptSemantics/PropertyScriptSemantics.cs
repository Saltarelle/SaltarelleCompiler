using System;

namespace Saltarelle.Compiler.ScriptSemantics {
    public class PropertyScriptSemantics {
        public enum ImplType {
            /// <summary>
            /// The property is implemented as a get/set method pair, perhaps with a backing field. All of the GetMethodName, SetMethodName and FieldName are valid (but the FieldName might be null if no backing field is needed).
            /// </summary>
            GetAndSetMethods,
            /// <summary>
            /// The property is implemented as a simple field. Only the FieldName is valid.
            /// </summary>
            Field,
            /// <summary>
            /// The property is not usable from script. No code is generated for it, and any usages of it will give an error.
            /// </summary>
            NotUsableFromScript,
        }

        /// <summary>
        /// Implementation type.
        /// </summary>
        public ImplType Type { get; private set; }

        private string _fieldName;
        private MethodScriptSemantics _getMethod;
        private MethodScriptSemantics _setMethod;

        public MethodScriptSemantics GetMethod {
            get {
                if (Type != ImplType.GetAndSetMethods)
                    throw new InvalidOperationException();
                return _getMethod;
            }
        }

        public MethodScriptSemantics SetMethod {
            get {
                if (Type != ImplType.GetAndSetMethods)
                    throw new InvalidOperationException();
                return _setMethod;
            }
        }

        public string FieldName {
            get {
                if (Type != ImplType.Field)
                    throw new InvalidOperationException();
                return _fieldName;
            }
        }

        public static PropertyScriptSemantics GetAndSetMethods(MethodScriptSemantics getMethod, MethodScriptSemantics setMethod) {
            return new PropertyScriptSemantics { Type = ImplType.GetAndSetMethods, _getMethod = getMethod, _setMethod = setMethod };
        }

        public static PropertyScriptSemantics Field(string fieldName) {
            return new PropertyScriptSemantics { Type = ImplType.Field, _fieldName = fieldName };
        }

        public static PropertyScriptSemantics NativeIndexer() {
            return new PropertyScriptSemantics { Type = ImplType.GetAndSetMethods, _getMethod = MethodScriptSemantics.NativeIndexer(), _setMethod = MethodScriptSemantics.NativeIndexer() };
        }

        public static PropertyScriptSemantics NotUsableFromScript() {
            return new PropertyScriptSemantics { Type = ImplType.NotUsableFromScript };
        }
    }
}