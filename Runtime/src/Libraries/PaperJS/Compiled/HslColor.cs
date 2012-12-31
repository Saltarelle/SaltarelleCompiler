using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// An HslColor object is used to represent any HSL color value.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class HslColor : Color
    {
    
        #region Constructors

        protected HslColor(){ }
        
        /// <summary>
        /// Creates an HslColor object
        /// </summary>
        /// <param name="hue">the hue of the color as a value in degrees between 0 and 360.</param>
        /// <param name="saturation">the saturation of the color as a value between 0 and 1</param>
        /// <param name="lightness">the lightness of the color as a value between 0 and 1</param>
        /// <param name="alpha">the alpha of the color as a value between 0 and 1 - optional</param>
        [ScriptName("")]
        public HslColor(double hue, double saturation, double lightness, double alpha){ }
        
        /// <summary>
        /// Creates an HslColor object
        /// </summary>
        /// <param name="hue">the hue of the color as a value in degrees between 0 and 360.</param>
        /// <param name="saturation">the saturation of the color as a value between 0 and 1</param>
        /// <param name="lightness">the lightness of the color as a value between 0 and 1</param>
        [ScriptName("")]
        public HslColor(double hue, double saturation, double lightness){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Checks if the component color values of the color are the same as those of the supplied one.
        /// </summary>
        /// <param name="color">the color to compare with</param>
        /// <returns>true if the colors are the same, false otherwise</returns>
        public bool Equals(Color color) { return default(bool); }
        
        #endregion
        
    }
}