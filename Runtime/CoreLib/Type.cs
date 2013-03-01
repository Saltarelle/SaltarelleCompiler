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
	[ScriptName("Function")]
	public sealed class Type {
		#region .Net reflection

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

		[InlineCode("{$System.Script}.getType({typeName})")]
		public static Type GetType(string typeName) {
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

		[InlineCode("{$System.Script}.getInterfaces({this})")]
		public Type[] GetInterfaces() {
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

		[InlineCode("{$System.Script}.isInstanceOfType({instance}, {this})")]
		public bool IsInstanceOfType(object instance) {
			return false;
		}

		[InlineCode("{$System.Script}.isInstanceOfType({instance}, {type})")]
		public static bool IsInstanceOfType(object instance, Type type) {
			return false;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 31, 28)")]
		public MemberInfo[] GetMembers() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 31, {bindingAttr})")]
		public MemberInfo[] GetMembers(BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 31, 28, {name})")]
		public MemberInfo[] GetMember(string name) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 31, {bindingAttr}, {name})")]
		public MemberInfo[] GetMember(string name, BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 1, 28)")]
		public ConstructorInfo[] GetConstructors() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 1, 284, null, {parameterTypes})")]
		public ConstructorInfo GetConstructor(Type[] parameterTypes) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 8, 28)")]
		public MethodInfo[] GetMethods() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 8, {bindingAttr})")]
		public MethodInfo[] GetMethods(BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 8, 284, {name})")]
		public MethodInfo GetMethod(string name) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 8, {bindingAttr} | 256, {name})")]
		public MethodInfo GetMethod(string name, BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 8, 284, {name}, {parameterTypes})")]
		public MethodInfo GetMethod(string name, Type[] parameterTypes) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 8, {bindingAttr} | 256, {name}, {parameterTypes})")]
		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Type[] parameterTypes) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 16, 28)")]
		public PropertyInfo[] GetProperties() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 16, {bindingAttr})")]
		public PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 16, 284, {name})")]
		public PropertyInfo GetProperty(string name) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 16, {bindingAttr} | 256, {name})")]
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 16, 284, {name}, {parameterTypes})")]
		public PropertyInfo GetProperty(string name, Type[] parameterTypes) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 16, {bindingAttr} | 256, {name}, {parameterTypes})")]
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Type[] parameterTypes) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 2, 28)")]
		public EventInfo[] GetEvents() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 2, {bindingAttr})")]
		public EventInfo[] GetEvents(BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 2, 284, {name})")]
		public EventInfo GetEvent(string name) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 2, {bindingAttr} | 256, {name})")]
		public EventInfo GetEvent(string name, BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 4, 28)")]
		public FieldInfo[] GetFields() {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 4, {bindingAttr})")]
		public FieldInfo[] GetFields(BindingFlags bindingAttr) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 4, 284, {name})")]
		public FieldInfo GetField(string name) {
			return null;
		}

		[InlineCode("{$System.Script}.getMembers({this}, 4, {bindingAttr} | 256, {name})")]
		public FieldInfo GetField(string name, BindingFlags bindingAttr) {
			return null;
		}

		#endregion

		#region Script# reflection

		[InlineCode("{instance}['add_' + {name}]({handler})")]
		public static void AddHandler(object instance, string name, Delegate handler) {
		}

		[InlineCode("delete {instance}[{name}]")]
		public static void DeleteField(object instance, string name) {
		}

		[InlineCode("{instance}[{name}]")]
		public static object GetField(object instance, string name) {
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

		[InlineCode("{instance}['remove_' + {name}]({handler})")]
		public static void RemoveHandler(object instance, string name, Delegate handler) {
		}

		[InlineCode("{instance}[{name}] = {value}")]
		public static void SetField(object instance, string name, object value) {
		}

		[InlineCode("{instance}['set_' + {name}]({value})")]
		public static void SetProperty(object instance, string name, object value) {
		}

		#endregion

		/// <summary>
		/// Gets the prototype associated with the type.
		/// </summary>
		[IntrinsicProperty]
		public JsDictionary Prototype {
			get {
				return null;
			}
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[NonScriptable]
		public static Type GetTypeFromHandle(RuntimeTypeHandle typeHandle) {
			return null;
		}
	}
}
