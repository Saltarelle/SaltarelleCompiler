// Type.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// The Type data type which is mapped to the Function type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	public sealed class Type {
		public Type BaseType {
			[InlineCode("{$System.Script}.getBaseType({this})")]
			get {
				return null;
			}
		}

		public string FullName {
			[InlineCode("{$System.Script}.getTypeFullName({this})")]
			get {
				return null;
			}
		}

		public string Name {
			[InlineCode("{$System.Script}.getTypeName({this})")]
			get {
				return null;
			}
		}

		[InlineCode("{$System.Script}.getMembers({this}, 0, 0)")]
		public MemberInfo[] GetMembers() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 0, {bindingAttr})")]
		public MemberInfo[] GetMembers(BindingFlags bindingAttr) {
			return null;
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

		[Obsolete("Use Activator.CreateInstance() instead", true)]
		public static object CreateInstance(Type type, params object[] arguments) {
			return null;
		}

		[Obsolete("Use Activator.CreateInstance<T>() instead", true)]
		[IncludeGenericArguments]
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

		[InlineCode("{$System.Script}.getInterfaces({this})")]
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

		[InlineCode("{$System.Script}.getType({typeName})")]
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

		[InlineCode("{$System.Script}.hasProperty({instance}, {name})")]
		public static bool HasProperty(object instance, string name) {
			return false;
		}

		[InlineCode("{instance}[{name}]({*args})")]
		[Obsolete("Script# allows the instance parameter to be null, which is not supported by Saltarelle. Ensure that you are not using a null instance argument. It is recommended to modify your code to use 'dynamic' instead.")]
		public static object InvokeMethod(object instance, string name, params object[] args) {
			return null;
		}

		[InlineCode("{$System.Script}.isAssignableFrom({this}, {type})")]
		public bool IsAssignableFrom(Type type) {
			return false;
		}

		public bool IsClass {
			[InlineCode("{$System.Script}.isClass({this})")]
			get { return false; }
		}

		public bool IsEnum {
			[InlineCode("{$System.Script}.isEnum({this})")]
			get { return false; }
		}

		public bool IsFlags {
			[InlineCode("{$System.Script}.isFlags({this})")]
			get { return false; }
		}

		public bool IsInterface {
			[InlineCode("{$System.Script}.isInterface({this})")]
			get { return false; }
		}

		[InlineCode("{$System.Script}.getAttributes({this}, null, {inherit})")]
		public object[] GetCustomAttributes(bool inherit) {
			return null;
		}

		[InlineCode("{$System.Script}.getAttributes({this}, {attributeType}, {inherit})")]
		public object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}

		[InlineCode("{$System.Script}.isInstanceOfType({instance}, {type})")]
		public static bool IsInstanceOfType(object instance, Type type) {
			return false;
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

		[InlineCode("{$System.Script}.makeGenericType({genericType}, {typeArguments})")]
		public static Type MakeGenericType(Type genericType, Type[] typeArguments) {
			return null;
		}

		[InlineCode("{$System.Script}.getGenericTypeDefinition({this})")]
		public Type GetGenericTypeDefinition() {
			return null;
		}

		public bool IsGenericTypeDefinition {
			[InlineCode("{$System.Script}.isGenericTypeDefinition({this})")]
			get { return false; }
		}

		public int GenericParameterCount {
			[InlineCode("{$System.Script}.getGenericParameterCount({this})")]
			get { return 0; }
		}

		[InlineCode("{$System.Script}.getGenericArguments({this})")]
		public Type[] GetGenericArguments() {
			return null;
		}
	}
}
