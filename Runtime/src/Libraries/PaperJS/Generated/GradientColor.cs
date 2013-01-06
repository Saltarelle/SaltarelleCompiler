using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The GradientColor object.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class GradientColor
    {
        #region Properties
        
        
        /// <summary>
        /// The origin point of the gradient.
        /// </summary>
        public Point Origin;
        
        /// <summary>
        /// The destination point of the gradient.
        /// </summary>
        public Point Destination;
        
        /// <summary>
        /// The hilite point of the gradient.
        /// </summary>
        public Point Hilite;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected GradientColor(){ }
        
        /// <summary>
        /// Creates a gradient color object.
        /// </summary>
        /// <param name="gradient"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="hilite">optional</param>
        [ScriptName("")]
        public GradientColor(Gradient gradient, Point origin, Point destination, Point hilite){ }
        
        /// <summary>
        /// Creates a gradient color object.
        /// </summary>
        /// <param name="gradient"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        [ScriptName("")]
        public GradientColor(Gradient gradient, Point origin, Point destination){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>a copy of the gradient color</returns>
        public GradientColor Clone() { return default(GradientColor); }
        
        /// <summary>
        /// Checks if the gradient color has the same properties as that of the supplied one.
        /// </summary>
        /// <param name="color"></param>
        /// <returns>otherwise</returns>
        public bool Equals(GradientColor color) { return default(bool); }
        
        /// <summary>
        /// Transform the gradient color by the specified matrix.
        /// </summary>
        /// <param name="matrix">the matrix to transform the gradient color by</param>
        public void Transform(Matrix matrix) { }
        
        #endregion
        
    }
}