using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;

namespace Saltarelle.Compiler {
	using Message = Tuple<int, MessageSeverity, string>;
	public static class Messages {
		public static readonly Message _7500 = Tuple.Create(7500, MessageSeverity.Error, "Cannot use the type {0} in the inheritance list for type {1} because it is marked as not usable from script.");
		public static readonly Message _7501 = Tuple.Create(7501, MessageSeverity.Error, "More than one unnamed constructor for the type {0}.");
		public static readonly Message _7502 = Tuple.Create(7502, MessageSeverity.Error, "The constructor {0} must be invoked in expanded form for its its param array.");
		public static readonly Message _7503 = Tuple.Create(7503, MessageSeverity.Error, "Chaining from a normal constructor to a static method constructor is not supported.");
		public static readonly Message _7504 = Tuple.Create(7504, MessageSeverity.Error, "Chaining from a normal constructor to a constructor implemented as inline code is not supported.");
		public static readonly Message _7505 = Tuple.Create(7505, MessageSeverity.Error, "This constructor cannot be used from script.");
		public static readonly Message _7506 = Tuple.Create(7506, MessageSeverity.Error, "Property {0}, declared as being a native indexer, is not an indexer with exactly one argument.");
		public static readonly Message _7507 = Tuple.Create(7507, MessageSeverity.Error, "Cannot use the property {0} from script.");
		public static readonly Message _7508 = Tuple.Create(7508, MessageSeverity.Error, "The field {0} is constant in script and cannot be assigned to.");
		public static readonly Message _7509 = Tuple.Create(7509, MessageSeverity.Error, "The field {0} is not usable from script.");
		public static readonly Message _7511 = Tuple.Create(7511, MessageSeverity.Error, "The event {0} is not usable from script.");
		public static readonly Message _7512 = Tuple.Create(7512, MessageSeverity.Error, "The property {0} is not usable from script.");
		public static readonly Message _7513 = Tuple.Create(7513, MessageSeverity.Error, "Only locals can be passed by reference.");
		public static readonly Message _7514 = Tuple.Create(7514, MessageSeverity.Error, "The method {0} must be invoked in expanded form for its its param array.");
		public static readonly Message _7515 = Tuple.Create(7515, MessageSeverity.Error, "Cannot use the type {0} in as a generic argument to the method {1} because it is marked as not usable from script.");
		public static readonly Message _7516 = Tuple.Create(7516, MessageSeverity.Error, "The method {0} cannot be used from script.");
		public static readonly Message _7517 = Tuple.Create(7517, MessageSeverity.Error, "Cannot use the the property {0} in an anonymous object initializer.");
		public static readonly Message _7518 = Tuple.Create(7518, MessageSeverity.Error, "Cannot use the field {0} in an anonymous object initializer.");
		public static readonly Message _7519 = Tuple.Create(7519, MessageSeverity.Error, "Cannot create an instance of the type {0} because it is marked as not usable from script.");
		public static readonly Message _7520 = Tuple.Create(7520, MessageSeverity.Error, "Cannot use the type {0} in as a type argument for the class {1} because it is marked as not usable from script.");
		public static readonly Message _7522 = Tuple.Create(7522, MessageSeverity.Error, "Cannot use the type {0} in a typeof expression because it is marked as not usable from script.");
		public static readonly Message _7523 = Tuple.Create(7523, MessageSeverity.Error, "Cannot perform method group conversion on {0} because {1}.");
		public static readonly Message _7524 = Tuple.Create(7524, MessageSeverity.Error, "Cannot convert the method '{0}' to the delegate type '{1}' because the method and delegate type differ in whether they expand their param array.");
		public static readonly Message _7525 = Tuple.Create(7525, MessageSeverity.Error, "Error in inline code compilation: {0}.");
		public static readonly Message _7526 = Tuple.Create(7526, MessageSeverity.Error, "Dynamic invocations cannot use named arguments.");
		public static readonly Message _7527 = Tuple.Create(7527, MessageSeverity.Error, "The member {0} cannot be initialized in an initializer statement because it was also initialized by the constructor call.");
		public static readonly Message _7528 = Tuple.Create(7528, MessageSeverity.Error, "Dynamic indexing must have exactly one argument.");
		public static readonly Message _7529 = Tuple.Create(7529, MessageSeverity.Error, "Cannot compile this dynamic invocation because all the applicable methods do not have the same script name. If you want to call the method with this exact name, cast the invocation target to dynamic.");
		public static readonly Message _7530 = Tuple.Create(7530, MessageSeverity.Error, "Cannot compile this dynamic invocation because at least one of the applicable methods is not a normal method. If you want to call the method with this exact name, cast the invocation target to dynamic.");
		public static readonly Message _7531 = Tuple.Create(7531, MessageSeverity.Error, "Cannot compile this dynamic invocation because the applicable methods are compiled in different ways.");
		public static readonly Message _7532 = Tuple.Create(7532, MessageSeverity.Error, "Chaining from a normal constructor to a JSON constructor is not supported.");
		public static readonly Message _7533 = Tuple.Create(7533, MessageSeverity.Error, "Cannot convert the delegate type {0} to {1} because they differ in whether the Javascript 'this' is bound to the first parameter.");
		public static readonly Message _7534 = Tuple.Create(7534, MessageSeverity.Error, "Delegates of type {0} must be invoked in expanded form for its its param array.");
		public static readonly Message _7535 = Tuple.Create(7535, MessageSeverity.Error, "The OnCompleted method used by an 'await' statement must be implemented as a normal method in script.");
		public static readonly Message _7536 = Tuple.Create(7536, MessageSeverity.Error, "The type parameter {0} is not available for use in script. You must specify [IncludeGenericArguments] on the {1} {2} and/or any method it overrides or implements.");

		public static readonly Message _7950 = Tuple.Create(7950, MessageSeverity.Error, "Error writing assembly: {0}.");
		public static readonly Message _7951 = Tuple.Create(7951, MessageSeverity.Error, "Error writing script: {0}.");
		public static readonly Message _7952 = Tuple.Create(7952, MessageSeverity.Error, "Error writing documentation file: {0}.");

		public static readonly Message _7996 = Tuple.Create(7996, MessageSeverity.Error, "Indirectly referenced assembly {0} must be referenced.");
		public static readonly Message _7997 = Tuple.Create(7997, MessageSeverity.Error, "Unable to resolve the assembly reference {0}.");
		public static readonly Message _7998 = Tuple.Create(7998, MessageSeverity.Error, "Use of unsupported feature {0}.");

		public static readonly Message InternalError = Tuple.Create(7999, MessageSeverity.Error, "INTERNAL ERROR: {0}. Please report this as an issue on https://github.com/erik-kallen/SaltarelleCompiler/");
	}
}
