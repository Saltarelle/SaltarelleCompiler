// Type.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {

    /// <summary>
    /// The Type data type which is mapped to the Function type in Javascript.
    /// </summary>
    [IgnoreNamespace]
    [Imported(IsRealType = true)]
    public sealed class Type {

        public Type BaseType {
            get {
                return null;
            }
        }

        public string FullName {
            get {
                return null;
            }
        }

        public string Name {
            get {
                return null;
            }
        }

        /// <summary>
        /// Gets the prototype associated with the type.
        /// </summary>
        [IntrinsicProperty]
        public JsDictionary Prototype {
            get {
                return null;
            }
        }

        [InlineCode("{instance}['add_' + {name}]({handler})")]
        public static void AddHandler(object instance, string name, Delegate handler) {
        }

        [InlineCode("new {type}({*arguments})")]
        public static object CreateInstance(Type type, params object[] arguments) {
            return null;
        }

        [InlineCode("new {T}({*arguments})")]
        public static T CreateInstance<T>(params object[] arguments) where T : class {
            return null;
        }

        [InlineCode("delete {instance}[{name}]")]
        public static void DeleteField(object instance, string name) {
        }

        [InlineCode("{instance}[{name}]")]
        public static object GetField(object instance, string name) {
            return null;
        }

        public Type[] GetInterfaces() {
            return null;
        }

        [InlineCode("{instance}['get_' + {name}]()")]
        public static object GetProperty(object instance, string name) {
            return null;
        }

        [ScriptAlias("typeof")]
        public static string GetScriptType(object instance) {
            return null;
        }

        public static Type GetType(string typeName) {
            return null;
        }

        [InlineCode("({name} in {instance})")]
        public static bool HasField(object instance, string name) {
            return false;
        }

        [InlineCode("(typeof({instance}[{name}]) === 'function')")]
        public static bool HasMethod(object instance, string name) {
            return false;
        }

        public static bool HasProperty(object instance, string name) {
            return false;
        }

        [InlineCode("{instance}[{name}]({*args})")]
        [Obsolete("Script# allows the instance parameter to be null, which is not supported by Saltarelle. Ensure that you are not using a null instance argument. It is recommended to modify your code to use 'dynamic' instead.")]
        public static object InvokeMethod(object instance, string name, params object[] args) {
            return null;
        }

        public bool IsAssignableFrom(Type type) {
            return false;
        }

        public bool IsClass {
            get { return false; }
        }

        public bool IsEnum {
            get { return false; }
        }

        public bool IsFlags {
            get { return false; }
        }

        public bool IsInterface {
            get { return false; }
        }

        public static bool IsNamespace(object obj) {
            return false;
        }

        public static bool IsInstanceOfType(object instance, Type type) {
            return false;
        }

        public static Type Parse(string s) {
            return null;
        }

        [InlineCode("{instance}['remove_' + {name}]({handler})")]
        public static void RemoveHandler(object instance, string name, Delegate handler) {
        }

        [InlineCode("{instance}[{name}] = {value}")]
        public static void SetField(object instance, string name, object value) {
        }

        [InlineCode("{instance}['set_' + {name}]({value})")]
        public static void SetProperty(object instance, string name, object value) {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [NonScriptable]
        public static Type GetTypeFromHandle(RuntimeTypeHandle typeHandle) {
            return null;
        }

        public static Type MakeGenericType(Type genericType, Type[] typeArguments) {
            return null;
        }

        public Type GetGenericTypeDefinition() {
            return null;
        }

        public bool IsGenericTypeDefinition {
            get { return false; }
        }

        public int GenericParameterCount {
            get { return 0; }
        }

        public Type[] GetGenericArguments() {
            return null;
        }
    }
}
