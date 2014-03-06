using System.Runtime.CompilerServices;

namespace System {
	/// <summary>
	/// Provides functionality to format the value of an object into a string representation.
	/// </summary>
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public interface IFormattable {
		/// <summary>Formats the value of the current instance using the specified format.</summary>
		/// <returns>The value of the current instance in the specified format.</returns>
		/// <param name="format">The format to use.-or- A null reference to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation.</param>
		[InlineCode("{$System.Script}.format({this}, {format})", GeneratedMethodName = "format")]
		string ToString(string format);
    }

    /// <summary>
    /// Provides functionality to format the value of an object into a string representation using locale.
    /// </summary>
    [ScriptNamespace("ss")]
    [Imported(ObeysTypeSystem = true)]
    public interface ILocaleFormattable : IFormattable
    {
        /// <summary>Formats the value of the current instance using the specified format with locale.</summary>
        /// <returns>The value of the current instance in the specified format.</returns>
        /// <param name="format">The format to use.-or- A null reference to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation.</param>
        [InlineCode("{$System.Script}.format({this}, {format}, true)", GeneratedMethodName = "formatLocale")]
        string LocaleFormat(string format);
    }
}
