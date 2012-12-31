using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// All properties and functions that expect color values accept instances of the different color classes such as RgbColor, HsbColor and GrayColor, and also accept named colors and hex values as strings which are then converted to instances of RgbColor internally.  Example &mdash; Named color values:   Run
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Color
    {
        #region Properties
        
        
        /// <summary>
        /// Returns the type of the color as a string.
        /// </summary>
        public ColorType Type;
        
        /// <summary>
        /// The color's alpha value as a number between 0 and 1. All colors of the different subclasses support alpha values.
        /// </summary>
        public double Alpha;
        
        /// <summary>
        /// The amount of red in the color as a value between 0 and 1.
        /// </summary>
        public double Red;
        
        /// <summary>
        /// The amount of green in the color as a value between 0 and 1.
        /// </summary>
        public double Green;
        
        /// <summary>
        /// The amount of blue in the color as a value between 0 and 1.
        /// </summary>
        public double Blue;
        
        /// <summary>
        /// The amount of gray in the color as a value between 0 and 1.
        /// </summary>
        public double Gray;
        
        /// <summary>
        /// The hue of the color as a value in degrees between 0 and 360.
        /// </summary>
        public double Hue;
        
        /// <summary>
        /// The saturation of the color as a value between 0 and 1.
        /// </summary>
        public double Saturation;
        
        /// <summary>
        /// The brightness of the color as a value between 0 and 1.
        /// </summary>
        public double Brightness;
        
        /// <summary>
        /// The lightness of the color as a value between 0 and 1.
        /// </summary>
        public double Lightness;
        
        #endregion
        
        #region Operators
        
        /// <summary>
        /// Checks if the component color values of the color are the same as those of the supplied one.
        /// </summary>
        /// <returns>true if the colors are the same, false otherwise</returns>
        public bool Equals(Color operand) { return default(bool); }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns> / HsbColor&nbsp;- a copy of the color object</returns>
        public Color Clone() { return default(RgbColor); }
        
        /// <summary>
        /// Checks if the color has an alpha value.
        /// </summary>
        /// <returns>true if the color has an alpha value, false otherwise</returns>
        public bool HasAlpha() { return default(bool); }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>A string representation of the color.</returns>
        public string ToString() { return default(string); }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>A css string representation of the color.</returns>
        public string ToCssString() { return default(string); }
        
        #endregion
        
    }
}