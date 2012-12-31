using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The GradientStop object.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class GradientStop
    {
        #region Properties
        
        
        /// <summary>
        /// The ramp-point of the gradient stop as a value between 0 and 1.
        /// </summary>
        public double RampPoint;
        
        /// <summary>
        /// The color of the gradient stop.
        /// </summary>
        public Color Color;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a GradientStop object.
        /// </summary>
        /// <param name="color">the color of the stop - optional, default: new RgbColor(0, 0, 0)</param>
        /// <param name="rampPoint">the position of the stop on the gradient ramp {@default 0} - optional, default: 0</param>
        [ScriptName("")]
        public GradientStop(Color color, double rampPoint){ }
        
        /// <summary>
        /// Creates a GradientStop object.
        /// </summary>
        /// <param name="color">the color of the stop - optional, default: new RgbColor(0, 0, 0)</param>
        [ScriptName("")]
        public GradientStop(Color color){ }
        
        /// <summary>
        /// Creates a GradientStop object.
        /// </summary>
        [ScriptName("")]
        public GradientStop(){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>a copy of the gradient-stop</returns>
        public GradientColor Clone() { return default(GradientColor); }
        
        #endregion
        
    }
}