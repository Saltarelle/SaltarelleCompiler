using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// CurveLocation objects describe a location on Curve objects, as defined by the curve parameter, a value between 0 (beginning of the curve) and 1 (end of the curve). If the curve is part of a Path item, its index inside the path.curves array is also provided. The class is in use in many places, such as path.getLocationAt(offset), Path#getNearestLocation(point), etc.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class CurveLocation
    {
        #region Properties
        
        
        /// <summary>
        /// The segment of the curve which is closer to the described location.
        /// </summary>
        public Segment Segment;
        
        /// <summary>
        /// The curve by which the location is defined.
        /// </summary>
        public Curve Curve;
        
        /// <summary>
        /// The path this curve belongs to, if any.
        /// </summary>
        public Item Path;
        
        /// <summary>
        /// The index of the curve within the path.curves list, if the curve is part of a Path item.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// The length of the path from its beginning up to the location described by this object.
        /// </summary>
        public double Offset;
        
        /// <summary>
        /// The length of the curve from its beginning up to the location described by this object.
        /// </summary>
        public double CurveOffset;
        
        /// <summary>
        /// The curve parameter, as used by various bezier curve calculations. It is value between 0 (beginning of the curve) and 1 (end of the curve).
        /// </summary>
        public double Parameter;
        
        /// <summary>
        /// The point which is defined by the curve and parameter.
        /// </summary>
        public Point Point;
        
        /// <summary>
        /// The tangential vector to the curve at the given location.
        /// </summary>
        public Point Tangent;
        
        /// <summary>
        /// The normal vector to the curve at the given location.
        /// </summary>
        public Point Normal;
        
        /// <summary>
        /// The distance from the queried point to the returned location.
        /// </summary>
        public double Distance;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected CurveLocation(){ }
        
        /// <summary>
        /// Creates a new CurveLocation object.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="parameter"></param>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        [ScriptName("")]
        public CurveLocation(Curve curve, double parameter, Point point, object distance){ }

        #endregion
        
        #region Methods
        
        
        
        #endregion
        
    }
}