using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// An RgbColor object is used to represent any RGB color value.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class RgbColor : Color
    {
    
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected RgbColor(){ }
        
        /// <summary>
        /// Creates an RgbColor object
        /// </summary>
        /// <param name="red">the amount of red in the color as a value between 0 and 1</param>
        /// <param name="green">the amount of green in the color as a value between 0 and 1</param>
        /// <param name="blue">the amount of blue in the color as a value between 0 and 1</param>
        /// <param name="alpha">the alpha of the color as a value between 0 and 1 - optional</param>
        [ScriptName("")]
        public RgbColor(double red, double green, double blue, double alpha){ }
        
        /// <summary>
        /// Creates an RgbColor object
        /// </summary>
        /// <param name="red">the amount of red in the color as a value between 0 and 1</param>
        /// <param name="green">the amount of green in the color as a value between 0 and 1</param>
        /// <param name="blue">the amount of blue in the color as a value between 0 and 1</param>
        [ScriptName("")]
        public RgbColor(double red, double green, double blue){ }

        #endregion
    }
}