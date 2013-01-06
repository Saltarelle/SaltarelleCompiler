using System.Runtime.CompilerServices;

namespace PaperJs
{
    public partial class Color
    {
        /// <summary>
        /// Uses string instead of Color class. Color names (eg. blue, red, green, etc) and hex values (eg. #ff0000, etc) can be used.
        /// </summary>
        /// <param name="value">The string representation of the color.</param>
        /// <returns>An implicit color class. In JS it remain plain string.</returns>
        [ScriptSkip]
        static public implicit operator Color(string value) { return null; }
    }
}