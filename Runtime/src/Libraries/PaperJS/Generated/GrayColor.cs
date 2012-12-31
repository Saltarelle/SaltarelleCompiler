using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// A GrayColor object is used to represent any gray color value.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class GrayColor : Color
    {
    
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
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
    }
}