using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported]
	[Serializable]
	public class MemberInfo {
		[ScriptName("type")]
		public MemberTypes MemberType { get; private set; }
		public string Name { get; private set; }
		[ScriptName("typeDef")]
		public Type DeclaringType { get; private set; } // TODO: Test
		public bool IsStatic { [InlineCode("{this}.isStatic || false")] get; [InlineCode("0")] private set; }

#warning TODO: GetCustomAttributes
		public object[] GetCustomAttributes(bool inherit) { return null; }
		public object[] GetCustomAttributes(Type attributeType, bool inherit) { return null; }
	}
}