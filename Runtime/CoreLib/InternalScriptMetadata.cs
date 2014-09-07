using System.ComponentModel;

namespace System.Runtime.CompilerServices.Internal {
	/// <summary>
	/// This attribute supports the infrastructure and should not be applied manually.
	/// </summary>
	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This attribute supports the infrastructure and should not be applied manually")]
	public sealed class ScriptSemanticsAttribute : Attribute {
		public object[] Data { get; private set; }
		public ScriptSemanticsAttribute(object[] data) {
			Data = data;
		}
	}

	/// <summary>
	/// This attribute supports the infrastructure and should not be applied manually.
	/// </summary>
	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This attribute supports the infrastructure and should not be applied manually")]
	public sealed class UsedMemberNamesAttribute : Attribute {
		public string[] Names { get; private set; }
		public UsedMemberNamesAttribute(string[] names) {
			Names = names;
		}
	}
}
