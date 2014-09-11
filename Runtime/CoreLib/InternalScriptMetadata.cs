using System.ComponentModel;

#warning TODO: Re-enable signing of CoreLib when Roslyn supports it.

namespace System.Runtime.CompilerServices.Internal {
#if !PLUGIN

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

#endif

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	#if !PLUGIN
	[Obsolete("This attribute supports the infrastructure and should not be applied manually")]
	#endif
	public sealed class ScriptSerializableAttribute : Attribute {
		public string TypeCheckCode { get; private set; }
		public ScriptSerializableAttribute(string typeCheckCode) {
			TypeCheckCode = typeCheckCode;
		}
	}
}
