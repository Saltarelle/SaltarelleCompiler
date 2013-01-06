using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// An HsbColor object is used to represent any HSB color value.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class HsbColor : Color
    {
    
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected HsbColor(){ }
        
        /// <summary>
        /// Creates an HsbColor object
        /// </summary>
        /// <param name="hue">the hue of the color as a value in degrees between 0 and 360.</param>
        /// <param name="saturation">the saturation of the color as a value between 0 and 1</param>
        /// <param name="brightness">the brightness of the color as a value between 0 and 1</param>
        /// <param name="alpha">the alpha of the color as a value between 0 and 1 - optional</param>
        [ScriptName("")]
        public HsbColor(double hue, double saturation, double brightness, double alpha){ }
        
        /// <summary>
        /// Creates an HsbColor object
        /// </summary>
        /// <param name="hue">the hue of the color as a value in degrees between 0 and 360.</param>
        /// <param name="saturation">the saturation of the color as a value between 0 and 1</param>
        /// <param name="brightness">the brightness of the color as a value between 0 and 1</param>
        [ScriptName("")]
        public HsbColor(double hue, double saturation, double brightness){ }

        #endregion
    }
}