using System;
using Saltarelle.Compiler;

namespace CoreLib.Plugin {
	public static class Messages {
		internal static readonly Tuple<int, MessageSeverity, string> _7001 = Tuple.Create(7001, MessageSeverity.Error, "The type {0} has both [IgnoreNamespace] and [ScriptNamespace] specified. At most one of these attributes can be specified for a type.");
		internal static readonly Tuple<int, MessageSeverity, string> _7002 = Tuple.Create(7002, MessageSeverity.Error, "{0}: The argument for [ScriptNamespace] must be a valid JavaScript qualified identifier, or be blank.");
		internal static readonly Tuple<int, MessageSeverity, string> _7003 = Tuple.Create(7003, MessageSeverity.Error, "The type {0} cannot have a [ResourcesAttribute] because it is not static.");
		internal static readonly Tuple<int, MessageSeverity, string> _7004 = Tuple.Create(7004, MessageSeverity.Error, "The type {0} cannot have a [ResourcesAttribute] because it is generic.");
		internal static readonly Tuple<int, MessageSeverity, string> _7005 = Tuple.Create(7005, MessageSeverity.Error, "The type {0} cannot have a [ResourcesAttribute] because it contains members that are not const fields.");
		internal static readonly Tuple<int, MessageSeverity, string> _7006 = Tuple.Create(7006, MessageSeverity.Error, "{0}: The argument for [ScriptName], when applied to a type, must be a valid JavaScript identifier.");
		internal static readonly Tuple<int, MessageSeverity, string> _7007 = Tuple.Create(7007, MessageSeverity.Error, "[IgnoreNamespace] or [ScriptNamespace] cannot be specified for the nested type {0}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7008 = Tuple.Create(7008, MessageSeverity.Error, "The non-serializable type {0} cannot inherit from the serializable type {1}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7009 = Tuple.Create(7009, MessageSeverity.Error, "The serializable type {0} must inherit from another serializable type, System.Object or System.Record.");
		internal static readonly Tuple<int, MessageSeverity, string> _7010 = Tuple.Create(7010, MessageSeverity.Error, "The serializable type {0} cannot implement interfaces.");
		internal static readonly Tuple<int, MessageSeverity, string> _7011 = Tuple.Create(7011, MessageSeverity.Error, "The serializable type {0} cannot declare instance events.");
		internal static readonly Tuple<int, MessageSeverity, string> _7012 = Tuple.Create(7012, MessageSeverity.Error, "The type {0} must be static in order to be decorated with a [MixinAttribute]");
		internal static readonly Tuple<int, MessageSeverity, string> _7013 = Tuple.Create(7013, MessageSeverity.Error, "The type {0} can contain only methods order to be decorated with a [MixinAttribute]");
		internal static readonly Tuple<int, MessageSeverity, string> _7014 = Tuple.Create(7014, MessageSeverity.Error, "[MixinAttribute] cannot be applied to the generic type {0}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7015 = Tuple.Create(7015, MessageSeverity.Error, "The type {0} must be static in order to be decorated with a [GlobalMethodsAttribute]");
		internal static readonly Tuple<int, MessageSeverity, string> _7017 = Tuple.Create(7017, MessageSeverity.Error, "[GlobalMethodsAttribute] cannot be applied to the generic type {0}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7018 = Tuple.Create(7018, MessageSeverity.Error, "The type {0} cannot inherit from both {1} and {2} because both those types have a member with the script name {3}. You have to rename the member on one of the base types, or refactor your code.");
		internal static readonly Tuple<int, MessageSeverity, string> _7023 = Tuple.Create(7023, MessageSeverity.Error, "The serializable type {0} cannot declare the virtual member {1}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7024 = Tuple.Create(7024, MessageSeverity.Error, "The serializable type {0} cannot override the member {1}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7025 = Tuple.Create(7025, MessageSeverity.Error, "The argument to the [MixinAttribute] for the type {0} must not be null or empty.");
		internal static readonly Tuple<int, MessageSeverity, string> _7026 = Tuple.Create(7026, MessageSeverity.Error, "The type {0} must have an [IncludeGenericArgumentsAttribute]");
		internal static readonly Tuple<int, MessageSeverity, string> _7027 = Tuple.Create(7027, MessageSeverity.Error, "The method {0} must have an [IncludeGenericArgumentsAttribute]");

		internal static readonly Tuple<int, MessageSeverity, string> _7100 = Tuple.Create(7100, MessageSeverity.Error, "The member {0} has an [AlternateSignatureAttribute], but there is not exactly one other method with the same name that does not have that attribute.");
		internal static readonly Tuple<int, MessageSeverity, string> _7102 = Tuple.Create(7102, MessageSeverity.Error, "The constructor {0} cannot have an [ExpandParamsAttribute] because it does not have a parameter with the 'params' modifier.");
		internal static readonly Tuple<int, MessageSeverity, string> _7103 = Tuple.Create(7103, MessageSeverity.Error, "The inline code for the constructor {0} contained errors: {1}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7104 = Tuple.Create(7104, MessageSeverity.Error, "The named specified in a [ScriptNameAttribute] for the indexer of type {0} cannot be empty.");
		internal static readonly Tuple<int, MessageSeverity, string> _7105 = Tuple.Create(7105, MessageSeverity.Error, "The named specified in a [ScriptNameAttribute] for the property {0} cannot be empty.");
		internal static readonly Tuple<int, MessageSeverity, string> _7106 = Tuple.Create(7106, MessageSeverity.Error, "Indexers cannot be decorated with [ScriptAliasAttribute].");
		internal static readonly Tuple<int, MessageSeverity, string> _7107 = Tuple.Create(7107, MessageSeverity.Error, "The property {0} cannot have a [ScriptAliasAttribute] because it is an instance member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7108 = Tuple.Create(7108, MessageSeverity.Error, "The indexer cannot be decorated with [IntrinsicPropertyAttribute] because it is an interface member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7109 = Tuple.Create(7109, MessageSeverity.Error, "The property {0} cannot have an [IntrinsicPropertyAttribute] because it is an interface member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7110 = Tuple.Create(7110, MessageSeverity.Error, "The indexer be decorated with an [IntrinsicPropertyAttribute] because it overrides a base member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7111 = Tuple.Create(7111, MessageSeverity.Error, "The property {0} cannot have an [IntrinsicPropertyAttribute] because it overrides a base member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7112 = Tuple.Create(7112, MessageSeverity.Error, "The indexer cannot be decorated with an [IntrinsicPropertyAttribute] because it is overridable.");
		internal static readonly Tuple<int, MessageSeverity, string> _7113 = Tuple.Create(7113, MessageSeverity.Error, "The property {0} cannot have an [IntrinsicPropertyAttribute] because it is overridable.");
		internal static readonly Tuple<int, MessageSeverity, string> _7114 = Tuple.Create(7114, MessageSeverity.Error, "The indexer cannot be decorated with an [IntrinsicPropertyAttribute] because it implements an interface member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7115 = Tuple.Create(7115, MessageSeverity.Error, "The property {0} cannot have an [IntrinsicPropertyAttribute] because it implements an interface member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7116 = Tuple.Create(7116, MessageSeverity.Error, "The indexer must have exactly one parameter in order to have an [IntrinsicPropertyAttribute].");
		internal static readonly Tuple<int, MessageSeverity, string> _7117 = Tuple.Create(7117, MessageSeverity.Error, "The method {0} cannot have an [IntrinsicOperatorAttribute] because it is not an operator method.");
		internal static readonly Tuple<int, MessageSeverity, string> _7118 = Tuple.Create(7118, MessageSeverity.Error, "The [IntrinsicOperatorAttribute] cannot be applied to the operator {0} because it is a conversion operator.");
		internal static readonly Tuple<int, MessageSeverity, string> _7119 = Tuple.Create(7119, MessageSeverity.Error, "The method {0} cannot have a [ScriptSkipAttribute] because it is an interface method.");
		internal static readonly Tuple<int, MessageSeverity, string> _7120 = Tuple.Create(7120, MessageSeverity.Error, "The member {0} cannot have a [ScriptSkipAttribute] because it overrides a base member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7121 = Tuple.Create(7121, MessageSeverity.Error, "The member {0} cannot have a [ScriptSkipAttribute] because it is overridable.");
		internal static readonly Tuple<int, MessageSeverity, string> _7122 = Tuple.Create(7122, MessageSeverity.Error, "The member {0} cannot have a [ScriptSkipAttribute] because it implements an interface member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7123 = Tuple.Create(7123, MessageSeverity.Error, "The static method {0} must have exactly one parameter in order to have a [ScriptSkipAttribute].");
		internal static readonly Tuple<int, MessageSeverity, string> _7124 = Tuple.Create(7124, MessageSeverity.Error, "The instance method {0} must have no parameters in order to have a [ScriptSkipAttribute].");
		internal static readonly Tuple<int, MessageSeverity, string> _7125 = Tuple.Create(7125, MessageSeverity.Error, "The method {0} must be static in order to have a [ScriptAliasAttribute].");
		internal static readonly Tuple<int, MessageSeverity, string> _7126 = Tuple.Create(7126, MessageSeverity.Error, "The member {0} needs a GeneratedMethodName property for its [InlineCodeAttribute] because it is an interface method.");
		internal static readonly Tuple<int, MessageSeverity, string> _7127 = Tuple.Create(7127, MessageSeverity.Error, "The member {0} cannot have an [InlineCodeAttribute] because it overrides a base member.");
		internal static readonly Tuple<int, MessageSeverity, string> _7128 = Tuple.Create(7128, MessageSeverity.Error, "The member {0} cannot have an [InlineCodeAttribute] because it is overridable.");
		internal static readonly Tuple<int, MessageSeverity, string> _7130 = Tuple.Create(7130, MessageSeverity.Error, "The inline code for the method {0} contained errors: {1}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7131 = Tuple.Create(7131, MessageSeverity.Error, "The method {0} cannot have an [InstanceMethodOnFirstArgumentAttribute] because it is not static.");
		internal static readonly Tuple<int, MessageSeverity, string> _7132 = Tuple.Create(7132, MessageSeverity.Error, "The [ScriptName], [PreserveName] and [PreserveCase] attributes cannot be specified on method the method {0} because it overrides a base member. Specify the attribute on the base member instead.");
		internal static readonly Tuple<int, MessageSeverity, string> _7133 = Tuple.Create(7133, MessageSeverity.Error, "The [IncludeGenericArguments] attribute cannot be specified on the method {0} because it overrides a base member. Specify the attribute on the base member instead.");
		internal static readonly Tuple<int, MessageSeverity, string> _7134 = Tuple.Create(7134, MessageSeverity.Error, "The overriding member {0} cannot implement the interface method {1} because it has a different script name. Consider using explicit interface implementation.");
		internal static readonly Tuple<int, MessageSeverity, string> _7135 = Tuple.Create(7135, MessageSeverity.Error, "The [ScriptName], [PreserveName] and [PreserveCase] attributes cannot be specified on the method {0} because it implements an interface member. Specify the attribute on the interface member instead, or consider using explicit interface implementation.");
		internal static readonly Tuple<int, MessageSeverity, string> _7136 = Tuple.Create(7136, MessageSeverity.Error, "The member {0} cannot implement multiple interface methods with differing script names. Consider using explicit interface implementation.");
		internal static readonly Tuple<int, MessageSeverity, string> _7137 = Tuple.Create(7137, MessageSeverity.Error, "The member {0} cannot have an [ExpandParamsAttribute] because it does not have a parameter with the 'params' modifier.");
		internal static readonly Tuple<int, MessageSeverity, string> _7138 = Tuple.Create(7138, MessageSeverity.Error, "The member {0} cannot have an empty name specified in its [ScriptName] because it is an interface method.");
		internal static readonly Tuple<int, MessageSeverity, string> _7139 = Tuple.Create(7139, MessageSeverity.Error, "The member {0} cannot have an empty name specified in its [ScriptName] because it is overridable.");
		internal static readonly Tuple<int, MessageSeverity, string> _7140 = Tuple.Create(7140, MessageSeverity.Error, "The member {0} cannot have an empty name specified in its [ScriptName] because it is static.");
		internal static readonly Tuple<int, MessageSeverity, string> _7141 = Tuple.Create(7141, MessageSeverity.Error, "The named specified in a [ScriptNameAttribute] for the event {0} cannot be empty.");
		internal static readonly Tuple<int, MessageSeverity, string> _7142 = Tuple.Create(7142, MessageSeverity.Error, "The named specified in a [ScriptNameAttribute] for the field {0} cannot be empty.");
		internal static readonly Tuple<int, MessageSeverity, string> _7143 = Tuple.Create(7143, MessageSeverity.Error, "The type {0} doesn't contain a matching property or field for the constructor parameter {1}.");
		internal static readonly Tuple<int, MessageSeverity, string> _7144 = Tuple.Create(7144, MessageSeverity.Error, "The parameter {0} has the type {1} but the matching member has type {2}. The types must be the same.");
		internal static readonly Tuple<int, MessageSeverity, string> _7145 = Tuple.Create(7145, MessageSeverity.Error, "The parameter {0} cannot be declared as ref or out.");
		internal static readonly Tuple<int, MessageSeverity, string> _7146 = Tuple.Create(7146, MessageSeverity.Error, "The constructor cannot have an [ObjectLiteralAttribute] because the type {0} is not a serializable type.");
		internal static readonly Tuple<int, MessageSeverity, string> _7147 = Tuple.Create(7147, MessageSeverity.Error, "The delegate type {0} cannot have a [BindThisToFirstParameterAttribute] because it does not have any parameters.");
		internal static readonly Tuple<int, MessageSeverity, string> _7148 = Tuple.Create(7148, MessageSeverity.Error, "The delegate type {0} cannot have an [ExpandParamsAttribute] because it does not have a parameter with the 'params' modifier.");
		internal static readonly Tuple<int, MessageSeverity, string> _7149 = Tuple.Create(7149, MessageSeverity.Error, "The method {0} cannot have an [InstanceMethodOnFirstArgumentAttribute] because it has no parameters.");
		internal static readonly Tuple<int, MessageSeverity, string> _7150 = Tuple.Create(7150, MessageSeverity.Error, "The method {0} cannot have an [InstanceMethodOnFirstArgumentAttribute] because its only parameter is a 'params' array.");
		internal static readonly Tuple<int, MessageSeverity, string> _7151 = Tuple.Create(7151, MessageSeverity.Error, "The method {0} cannot have an [EnumerateAsArrayAttribute] because it is not a GetEnumerator() method for the iterator pattern.");
		internal static readonly Tuple<int, MessageSeverity, string> _7152 = Tuple.Create(7152, MessageSeverity.Error, "The field {0} cannot have an [InlineConstantAttribute] because it is not constant.");

		internal static readonly Tuple<int, MessageSeverity, string> _7700 = Tuple.Create(7700, MessageSeverity.Error, "Boxing of 'char' is not allowed because this is likely to cause undesired behaviour. Insert a cast to 'int' or 'string' to tell the compiler about the desired behaviour.");

		internal static readonly Tuple<int, MessageSeverity, string> _7800 = Tuple.Create(7800, MessageSeverity.Error, "The program entry point {0} may not have any parameters.");
		internal static readonly Tuple<int, MessageSeverity, string> _7801 = Tuple.Create(7801, MessageSeverity.Error, "The program entry point {0} must be implemented as a normal method.");
	}
}
