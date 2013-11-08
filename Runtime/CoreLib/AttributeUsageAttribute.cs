using System.Runtime.CompilerServices;

namespace System {
	/// <summary>Specifies the application elements on which it is valid to apply an attribute.</summary>
	[NonScriptable]
	[Flags]
	public enum AttributeTargets {
		Assembly         = 0x0001,
		Module           = 0x0002,
		Class            = 0x0004,
		Struct           = 0x0008,
		Enum             = 0x0010,
		Constructor      = 0x0020,
		Method           = 0x0040,
		Property         = 0x0080,
		Field            = 0x0100,
		Event            = 0x0200,
		Interface        = 0x0400,
		Parameter        = 0x0800,
		Delegate         = 0x1000,
		ReturnValue      = 0x2000,
		GenericParameter = 0x4000,
		All = Assembly | Module | Class | Struct | Enum | Constructor | Method | Property | Field | Event | Interface | Parameter | Delegate | ReturnValue | GenericParameter,
	}

	/// <summary>Specifies the usage of another attribute class. This class cannot be inherited.</summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	[NonScriptable]
	public sealed class AttributeUsageAttribute : Attribute {
		internal AttributeTargets m_attributeTarget = AttributeTargets.All;
		internal bool m_inherited = true;
		internal bool m_allowMultiple;

		/// <summary>Gets a set of values identifying which program elements that the indicated attribute can be applied to.</summary>
		/// <returns>One or several <see cref="T:System.AttributeTargets"/> values. The default is All.</returns>
		public AttributeTargets ValidOn { get { return this.m_attributeTarget; } }

		/// <summary>Gets or sets a Boolean value indicating whether more than one instance of the indicated attribute can be specified for a single program element.</summary>
		/// <returns>true if more than one instance is allowed to be specified; otherwise, false. The default is false.</returns>
		public bool AllowMultiple {
			get { return this.m_allowMultiple; }
			set { this.m_allowMultiple = value; }
		}

		/// <summary>Gets or sets a Boolean value indicating whether the indicated attribute can be inherited by derived classes and overriding members.</summary>
		/// <returns>true if the attribute can be inherited by derived classes and overriding members; otherwise, false. The default is true.</returns>
		public bool Inherited {
			get { return this.m_inherited; }
			set { this.m_inherited = value; }
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.AttributeUsageAttribute"/> class with the specified list of <see cref="T:System.AttributeTargets"/>, the <see cref="P:System.AttributeUsageAttribute.AllowMultiple"/> value, and the <see cref="P:System.AttributeUsageAttribute.Inherited"/> value.</summary>
		/// <param name="validOn">The set of values combined using a bitwise OR operation to indicate which program elements are valid.</param>
		public AttributeUsageAttribute(AttributeTargets validOn) {
			this.m_attributeTarget = validOn;
		}

		internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited) {
			this.m_attributeTarget = validOn;
			this.m_allowMultiple = allowMultiple;
			this.m_inherited = inherited;
		}
	}
}
