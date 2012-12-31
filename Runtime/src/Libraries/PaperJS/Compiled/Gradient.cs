using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The Gradient object.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Gradient
    {
        #region Properties
        
        
        /// <summary>
        /// The gradient stops on the gradient ramp.
        /// </summary>
        public GradientStop[] Stops;
        
        #endregion
        
        #region Constructors

        protected Gradient(){ }
        
        /// <summary>
        /// Creates a gradient object
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="type">'linear' or 'radial' - optional, default: 'linear'</param>
        [ScriptName("")]
        public Gradient(GradientStop[] stops, string type){ }
        
        /// <summary>
        /// Creates a gradient object
        /// </summary>
        /// <param name="stops"></param>
        [ScriptName("")]
        public Gradient(GradientStop[] stops){ }

        #endregion
        
        #region Operators
        
        /// <summary>
        /// Checks whether the gradient is equal to the supplied gradient.
        /// </summary>
        /// <returns>true they are equal, false otherwise</returns>
        public bool Equals(Gradient operand) { return default(bool); }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>a copy of the gradient</returns>
        public Gradient Clone() { return default(Gradient); }
        
        #endregion
        
    }
}