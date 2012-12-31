using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// A GrayColor object is used to represent any gray color value.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class GrayColor : Color
    {
    
        #region Constructors

        protected GrayColor(){ }
        
        /// <summary>
        /// Creates a GrayColor object
        /// </summary>
        /// <param name="gray">the amount of gray in the color as a value between 0 and 1</param>
        /// <param name="alpha">the alpha of the color as a value between 0 and 1 - optional</param>
        [ScriptName("")]
        public GrayColor(double gray, double alpha){ }
        
        /// <summary>
        /// Creates a GrayColor object
        /// </summary>
        /// <param name="gray">the amount of gray in the color as a value between 0 and 1</param>
        [ScriptName("")]
        public GrayColor(double gray){ }

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