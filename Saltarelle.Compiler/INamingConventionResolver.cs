using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
    public interface INamingConventionResolver {
        /// <summary>
        /// Returns the name of a type as it should appear in the script. If null is included the class, and any nested class, will not appear in the output.
        /// </summary>
        string GetTypeName(ITypeResolveContext context, ITypeDefinition typeDefinition);
        string GetTypeParameterName(ITypeResolveContext context, ITypeParameter typeParameter);

        /// <summary>
        /// Gets the implementation of a method. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        MethodImplOptions GetMethodImplementation(ITypeResolveContext context, IMethod method);

        /// <summary>
        /// Returns the implementation of a constructor. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        ConstructorImplOptions GetConstructorImplementation(ITypeResolveContext context, IMethod method);

        /// <summary>
        /// Returns the implementation of an auto-implemented property. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        PropertyImplOptions GetPropertyImplementation(ITypeResolveContext context, IProperty property);

        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null.
        /// </summary>
        FieldOptions GetAutoPropertyBackingFieldImplementation(ITypeResolveContext context, IProperty property);
    }

    public class FieldOptions {
        public enum ImplType {
            NotUsableFromScript = 0,
            Instance = 1,
            Static = 2
        }

        public ImplType Type { get; private set; }
        private string _name;

        public String Name {
            get {
                if (Type != ImplType.Instance && Type != ImplType.Static)
                    throw new InvalidOperationException();
                return _name;
            }
        }

        private FieldOptions() {
        }

        public static FieldOptions Instance(string name) {
            return new FieldOptions { Type = ImplType.Instance, _name = name };
        }

        public static FieldOptions Static(string name) {
            return new FieldOptions { Type = ImplType.Static, _name = name };
        }

        public static FieldOptions NotUsableFromScript() {
            return new FieldOptions { Type = ImplType.NotUsableFromScript };
        }
    }

    public class PropertyImplOptions {
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
        private bool _isFieldStatic;
        private MethodImplOptions _getMethod;
        private MethodImplOptions _setMethod;

        public MethodImplOptions GetMethod {
            get {
                if (Type != ImplType.GetAndSetMethods)
                    throw new InvalidOperationException();
                return _getMethod;
            }
        }

        public MethodImplOptions SetMethod {
            get {
                if (Type != ImplType.GetAndSetMethods)
                    throw new InvalidOperationException();
                return _setMethod;
            }
        }

        public static PropertyImplOptions GetAndSetMethods(MethodImplOptions getMethod, MethodImplOptions setMethod) {
            return new PropertyImplOptions { Type = ImplType.GetAndSetMethods, _getMethod = getMethod, _setMethod = setMethod };
        }

        public static PropertyImplOptions InstanceField(string fieldName) {
            return new PropertyImplOptions { Type = ImplType.Field, _fieldName = fieldName, _isFieldStatic = false };
        }

        public static PropertyImplOptions StaticField(string fieldName) {
            return new PropertyImplOptions { Type = ImplType.Field, _fieldName = fieldName, _isFieldStatic = true };
        }

        public static PropertyImplOptions NotUsableFromScript() {
            return new PropertyImplOptions { Type = ImplType.NotUsableFromScript };
        }
    }
}
