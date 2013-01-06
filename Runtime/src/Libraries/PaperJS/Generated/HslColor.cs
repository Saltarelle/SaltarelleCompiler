using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// An HslColor object is used to represent any HSL color value.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class HslColor : Color
    {
    
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
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
    }
}