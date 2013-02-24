// ScriptMetadata.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices {
	/// <summary>
	/// This attribute can be placed on types in system script assemblies that should not
	/// be imported. It is only meant to be used within mscorlib.dll.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class NonScriptableAttribute : Attribute {
	}

	/// <summary>
	/// This attribute can be placed on types that should not be emitted into generated
	/// script, as they represent existing script or native types. All members without another naming attribute are considered to use [PreserveName].
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct)]
	[NonScriptable]
	public sealed class ImportedAttribute : Attribute {
		/// <summary>
		/// Indicates that the type obeys the Saltarelle type system. If false (the default), the type is ignored in inheritance lists, casts to it is a no-op, and Object will be used if the type is used as a generic argument.
		/// The default is false. Requiring this to be set should be very uncommon.
		/// </summary>
		public bool ObeysTypeSystem { get; set; }
	}

	/// <summary>
	/// Marks an assembly as a script assembly that can be used with Script#.
	/// Additionally, each script must have a unique name that can be used as
	/// a dependency name.
	/// This name is also used to generate unique names for internal types defined
	/// within the assembly. The ScriptQualifier attribute can be used to provide a
	/// shorter name if needed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ScriptAssemblyAttribute : Attribute {
		public ScriptAssemblyAttribute(string name) {
			Name = name;
		}

		public string Name { get; private set; }
	}

	/// <summary>
	/// Provides a prefix to use when generating types internal to this assembly so that
	/// they can be unique within a given a script namespace.
	/// The specified prefix overrides the script name provided in the ScriptAssembly
	/// attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ScriptQualifierAttribute : Attribute {
		public ScriptQualifierAttribute(string prefix) {
			Prefix = prefix;
		}

		public string Prefix { get; private set; }
	}

	/// <summary>
	/// This attribute indicates that the namespace of type within a system assembly
	/// should be ignored at script generation time. It is useful for creating namespaces
	/// for the purpose of c# code that don't exist at runtime.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class IgnoreNamespaceAttribute : Attribute {
	}

	/// <summary>
	/// Specifies the namespace that should be used in generated script. The script namespace
	/// is typically a short name, that is often shared across multiple assemblies.
	/// The developer is responsible for ensuring that public types across assemblies that share
	/// a script namespace are unique.
	/// For internal types, the ScriptQualifier attribute can be used to provide a short prefix
	/// to generate unique names.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ScriptNamespaceAttribute : Attribute {
		public ScriptNamespaceAttribute(string name) {
			Name = name;
		}

		public string Name { get; private set; }
	}

	/// <summary>
	/// This attribute can be placed on a static class that only contains static string
	/// fields representing a set of resource strings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	[NonScriptable]
	public sealed class ResourcesAttribute : Attribute {
	}

	/// <summary>
	/// This attribute turns methods on a static class as global methods in the generated
	/// script. Note that the class must be static, and must contain only methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	[NonScriptable]
	public sealed class GlobalMethodsAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class MixinAttribute : Attribute {
		public MixinAttribute(string expression) {
			Expression = expression;
		}

		public string Expression { get; private set; }
	}

	/// <summary>
	/// This attribute marks an enumeration type within a system assembly as as a set of
	/// names. Rather than the specific value, the name of the enumeration field is
	/// used as a string.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class NamedValuesAttribute : Attribute {
	}

	/// <summary>
	/// This attribute marks an enumeration type within a system assembly as as a set of
	/// numeric values. Rather than the enum field, the value of the enumeration field is
	/// used as a literal.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class NumericValuesAttribute : Attribute {
	}

	/// <summary>
	/// This attribute allows defining an alternate method signature that is not generated
	/// into script, but can be used for defining overloads to enable optional parameter semantics
	/// for a method. It must be applied on a method defined as extern, since an alternate signature
	/// method does not contain an actual method body.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class AlternateSignatureAttribute : Attribute {
	}

	/// <summary>
	/// This attribute denotes a C# property that manifests like a field in the generated
	/// JavaScript (i.e. is not accessed via get/set methods). This is really meant only
	/// for use when defining OM corresponding to native objects exposed to script.
	/// If no other name is specified (and the property is not an indexer), the field is treated as if it were decorated with a [PreserveName] attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class IntrinsicPropertyAttribute : Attribute {
	}

	/// <summary>
	/// Allows specifying the name to use for a type or member in the generated script.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ScriptNameAttribute : Attribute {
		public ScriptNameAttribute(string name) {
			Name = name;
		}

		public string Name { get; private set; }
	}

	/// <summary>
	/// This attribute allows suppressing the default behavior of converting
	/// member names to camel-cased equivalents in the generated JavaScript.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class PreserveCaseAttribute : Attribute {
	}

	/// <summary>
	/// This attribute allows suppressing the default behavior of converting
	/// member names of attached type to camel-cased equivalents in the generated JavaScript.
	/// When applied to an assembly, all types in the assembly are considered to have this
	/// attribute by default</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Assembly, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class PreserveMemberCaseAttribute : Attribute {
		public PreserveMemberCaseAttribute() {
			Preserve = true;
		}

		public PreserveMemberCaseAttribute(bool preserve) {
			Preserve = preserve;
		}

		public bool Preserve { get; private set; }
	}

	/// <summary>
	/// This attribute allows suppressing the default behavior of minimizing
	/// private type names and member names in the generated JavaScript.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class PreserveNameAttribute : Attribute {
	}

	/// <summary>
	/// This attribute allows public symbols inside an assembly to be minimized, in addition to non-public ones, when generating release scripts.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	[NonScriptable]
	public sealed class MinimizePublicNamesAttribute : Attribute {
	}

	/// <summary>
	/// This attribute allows specifying a script name for an imported method.
	/// The method is interpreted as a global method. As a result it this attribute
	/// only applies to static methods.
	/// </summary>
	// REVIEW: Eventually do we want to support this on properties/field and instance methods as well?
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ScriptAliasAttribute : Attribute {
		public ScriptAliasAttribute(string alias) {
			Alias = alias;
		}

		public string Alias { get; private set; }
	}

	/// <summary>
	/// This attributes causes a method to not be invoked. The method must either be a static method with one argument (in case Foo.M(x) will become x), or an instance method with no arguments (in which x.M() will become x).
	/// Can also be applied to a constructor, in which case the constructor will not be called if used as an initializer (": base()" or ": this()").
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ScriptSkipAttribute : Attribute {
	}

	/// <summary>
	/// The method is implemented as inline code, eg Debugger.Break() => debugger. Can use the parameters {this} (for instance methods), as well as all typenames and argument names in braces (eg. {arg0}, {TArg0}).
	/// If a parameter name is preceeded by an @ sign, {@arg0}, that argument must be a literal string during invocation, and the supplied string will be inserted as an identifier into the script (eg '{this}.set_{@arg0}({arg1})' can transform the call 'c.F("MyProp", v)' to 'c.set_MyProp(v)'.
	/// If a parameter name is preceeded by an asterisk {*arg} that parameter must be a param array, and all invocations of the method must use the expanded invocation form. The entire array supplied for the parameter will be inserted into the call. Pretend that the parameter is a normal parameter, and commas will be inserted or omitted at the correct locations.
	/// The format string can also use identifiers starting with a dollar {$Namespace.Name} to construct type references. The name must be the fully qualified type name in this case.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class InlineCodeAttribute : Attribute {
		public InlineCodeAttribute(string code) {
			Code = code;
		}

		public string Code { get; private set; }

		/// <summary>
		/// If set, a method with this name will be generated from the method source.
		/// </summary>
		public string GeneratedMethodName { get; set; }
		
		/// <summary>
		/// This code is used when the method is invoked non-virtually (eg. in a base.Method() call).
		/// </summary>
		public string NonVirtualCode { get; set; }
	}

	/// <summary>
	/// This attribute specifies that a static method should be treated as an instance method on its first argument. This means that <c>MyClass.Method(x, a, b)</c> will be transformed to <c>x.Method(a, b)</c>.
	/// If no other name-preserving attribute is used on the member, it will be treated as if it were decorated with a [PreserveNameAttribute].
	/// Useful for extension methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class InstanceMethodOnFirstArgumentAttribute : Attribute {
	}

	/// <summary>
	/// This attribute specifies that a generic type or method should have script generated as if it was a non-generic one. Any uses of the type arguments inside the method (eg. <c>typeof(T)</c>, or calling another generic method with T as a type argument) will cause runtime errors.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class IncludeGenericArgumentsAttribute : Attribute {
		public IncludeGenericArgumentsAttribute() {
			Include = true;
		}

		public IncludeGenericArgumentsAttribute(bool include) {
			Include = include;
		}

		public bool Include { get; private set; }
	}

	/// <summary>
	/// This enum defines the possibilities for default values for generic argument handling in an assembly. 
	/// </summary>
	[NonScriptable]
	public enum GenericArgumentsDefault {
		/// <summary>
		/// Include generic arguments for all types that are not [Imported]
		/// </summary>
		IncludeExceptImported,
		/// <summary>
		/// Ignore generic arguments by default (this is the default)
		/// </summary>
		Ignore,
		/// <summary>
		/// Require an <see cref="IncludeGenericArgumentsAttribute"/> for all generic types/methods, excepts those that are imported, which will default to ignore their generic arguments.
		/// </summary>
		RequireExplicitSpecification,
	}

	/// <summary>
	/// This attribute indicates whether generic arguments for types and methods are included, but can always be overridden by specifying an <see cref="IncludeGenericArgumentsAttribute"/> on types or methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	[NonScriptable]
	public sealed class IncludeGenericArgumentsDefaultAttribute : Attribute {
		public GenericArgumentsDefault TypeDefault { get; set; }
		public GenericArgumentsDefault MethodDefault { get; set; }
	}

	/// <summary>
	/// This attribute indicates that a user-defined operator should be compiled as if it were builtin (eg. op_Addition(a, b) => a + b). It can only be used on non-conversion operator methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class IntrinsicOperatorAttribute : Attribute {
	}

	/// <summary>
	/// This attribute can be applied to a method with a "params" parameter to make the param array be expanded in script (eg. given 'void F(int a, params int[] b)', the invocation 'F(1, 2, 3)' will be translated to 'F(1, [2, 3])' without this attribute, but 'F(1, 2, 3)' with this attribute.
	/// Methods with this attribute can only be invoked in the expanded form.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Delegate, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ExpandParamsAttribute : Attribute {
	}

	/// <summary>
	/// Indicates that the Javascript 'this' should appear as the first argument to the delegate.
	/// </summary>
	[AttributeUsage(AttributeTargets.Delegate)]
	[NonScriptable]
	public sealed class BindThisToFirstParameterAttribute : Attribute {
	}

	/// <summary>
	/// If this attribute is applied to a constructor for a serializable type, it means that the constructor will not be called, but rather an object initializer will be created. Eg. 'new MyRecord(1, "X")' can become '{ a: 1, b: 'X' }'.
	/// All parameters must have a field or property with the same (case-insensitive) name, of the same type.
	/// This attribute is implicit on constructors of imported serializable types.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class ObjectLiteralAttribute : Attribute {
	}

	/// <summary>
	/// This attribute can be specified on an assembly to specify additional compatibility options to help migrating from Script#.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	[NonScriptable]
	public sealed class ScriptSharpCompatibilityAttribute : Attribute {
		/// <summary>
		/// If true, code will not be generated for casts of type '(MyClass)someValue'. Code will still be generated for 'someValue is MyClass' and 'someValue as MyClass'.
		/// </summary>
		public bool OmitDowncasts { get; set; }

		/// <summary>
		/// If true, code will not be generated to verify that a nullable value is not null before converting it to its underlying type.
		/// </summary>
		public bool OmitNullableChecks { get; set; }
	}

	/// <summary>
	/// If a constructor for a value type takes an instance of this type as a parameter, any attribute applied to that constructor will instead be applied to the default (undeclarable) constructor.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Imported]
	public sealed class DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor {
		private DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor() {}
	}

	/// <summary>
	/// Specifies that a type is defined in a module, which should be imported by a require() call.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Assembly)]
	[NonScriptable]
	public sealed class ModuleNameAttribute : Attribute {	
		public ModuleNameAttribute(string moduleName) {
			this.ModuleName = moduleName;
		}

		public string ModuleName { get; private set; }
	}
	
	/// <summary>
	/// When specified on an assembly, Javascript that adheres to the AMD pattern (require/define) will be generated.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	[NonScriptable]
	public sealed class AsyncModuleAttribute : Attribute {
	}

	/// <summary>
	/// Can be applied to a GetEnumerator() method to indicate that that array-style enumeration should be used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	[NonScriptable]
	public sealed class EnumerateAsArrayAttribute : Attribute {
	}

	/// <summary>
	/// Can be applied to a const field to indicate that the literal value of the constant should always be used instead of the symbolic field name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	[NonScriptable]
	public sealed class InlineConstantAttribute : Attribute {
	}

	/// <summary>
	/// Can be applied to a member to indicate that metadata for the member should be included in the compiled script.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	[NonScriptable]
	public sealed class ReflectableAttribute : Attribute {
		public bool Reflectable { get; private set; }

		public ReflectableAttribute() {
			Reflectable = true;
		}
		public ReflectableAttribute(bool reflectable) {
			Reflectable = reflectable;
		}
	}
}
