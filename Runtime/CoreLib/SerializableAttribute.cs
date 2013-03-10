using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {
	/// <summary>
	/// This attribute indicates that a class is a serializable type, and can be used as an alternative to inheriting from <see cref="Record"/>. Record classes must inherit directly from object, be sealed, and cannot contain any instance events.
	/// Instance properties in serializable types are implemented as fields.
	/// All instance fields and properties on serializable types will act as they were decorated with a [PreserveNameAttribute], unless another renaming attribute was specified.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	[Imported]
	public sealed class SerializableAttribute : Attribute {
		/// <summary>
		/// Code used to check whether an object is of this type. Can use the placeholder {this} to reference the object being checked, as well as all type parameter for the type.
		/// </summary>
		public string TypeCheckCode { get; set; }
	}
}
