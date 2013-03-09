using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported]
	[Serializable]
	public class MemberInfo {
		[ScriptName("type")]
		public MemberTypes MemberType { get; private set; }
		public string Name { get; private set; }
		[ScriptName("typeDef")]
		public Type DeclaringType { get; private set; }
		public bool IsStatic { [InlineCode("{this}.isStatic || false")] get; [InlineCode("0")] private set; }

		/// <summary>
		/// Returns an array of all custom attributes applied to this member.
		/// </summary>
		/// <param name="inherit">Ignored for members. Base members will never be considered.</param>
		/// <returns>An array that contains all the custom attributes applied to this member, or an array with zero elements if no attributes are defined. </returns>
		[InlineCode("{this}.attr || []")]
		public object[] GetCustomAttributes(bool inherit) { return null; }

		/// <summary>
		/// Returns an array of custom attributes applied to this member and identified by <see cref="T:System.Type"/>.
		/// </summary>
		/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned. </param>
		/// <param name="inherit">Ignored for members. Base members will never be considered.</param>
		/// <returns>An array that contains all the custom attributes applied to this member, or an array with zero elements if no attributes are defined.</returns>
		[InlineCode("({this}.attr || []).filter(function(a) {{ return {$System.Script}.isInstanceOfType(a, {attributeType}); }})")]
		public object[] GetCustomAttributes(Type attributeType, bool inherit) { return null; }

		/// <summary>
		/// Returns an array of all custom attributes applied to this member.
		/// </summary>
		/// <returns>An array that contains all the custom attributes applied to this member, or an array with zero elements if no attributes are defined. </returns>
		[InlineCode("{this}.attr || []")]
		public object[] GetCustomAttributes() { return null; }

		/// <summary>
		/// Returns an array of custom attributes applied to this member and identified by <see cref="T:System.Type"/>.
		/// </summary>
		/// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned. </param>
		/// <returns>An array that contains all the custom attributes applied to this member, or an array with zero elements if no attributes are defined.</returns>
		[InlineCode("({this}.attr || []).filter(function(a) {{ return {$System.Script}.isInstanceOfType(a, {attributeType}); }})")]
		public object[] GetCustomAttributes(Type attributeType) { return null; }

		internal MemberInfo() {}
	}
}