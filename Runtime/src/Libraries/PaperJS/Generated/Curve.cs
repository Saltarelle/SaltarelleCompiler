using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The Curve object represents the parts of a path that are connected by two following Segment objects. The curves of a path can be accessed through its path.curves array. While a segment describe the anchor point and its incoming and outgoing handles, a Curve object describes the curve passing between two such segments. Curves and segments represent two different ways of looking at the same thing, but focusing on different aspects. Curves for example offer many convenient ways to work with parts of the path, finding lengths, positions or tangents at given offsets.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Curve
    {
        #region Properties
        
        
        /// <summary>
        /// The first anchor point of the curve.
        /// </summary>
        public Point Point1;
        
        /// <summary>
        /// The second anchor point of the curve.
        /// </summary>
        public Point Point2;
        
        /// <summary>
        /// The handle point that describes the tangent in the first anchor point.
        /// </summary>
        public Point Handle1;
        
        /// <summary>
        /// The handle point that describes the tangent in the second anchor point.
        /// </summary>
        public Point Handle2;
        
        /// <summary>
        /// The first segment of the curve.
        /// </summary>
        public Segment Segment1;
        
        /// <summary>
        /// The second segment of the curve.
        /// </summary>
        public Segment Segment2;
        
        /// <summary>
        /// The path that the curve belongs to.
        /// </summary>
        public Path Path;
        
        /// <summary>
        /// The index of the curve in the path.curves array.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// The next curve in the path.curves array that the curve belongs to.
        /// </summary>
        public Curve Next;
        
        /// <summary>
        /// The previous curve in the path.curves array that the curve belongs to.
        /// </summary>
        public Curve Previous;
        
        /// <summary>
        /// Specifies whether the handles of the curve are selected.
        /// </summary>
        public bool Selected;
        
        /// <summary>
        /// The approximated length of the curve in points.
        /// </summary>
        public double Length;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected Curve(){ }
        
        /// <summary>
        /// Creates a new curve object.
        /// </summary>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        [ScriptName("")]
        public Curve(Segment segment1, Segment segment2, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Checks if this curve is linear, meaning it does not define any curve handle.
        /// </summary>
        /// <returns>true the curve is linear, false otherwise</returns>
        public bool IsLinear() { return default(bool); }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="start">optional</param>
        /// <returns></returns>
        public double GetParameterAt(double offset, double start) { return default(double); }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public double GetParameterAt(double offset) { return default(double); }
        
        /// <summary>
        /// Returns the point on the curve at the specified position.
        /// </summary>
        /// <param name="parameter">the position at which to find the point as a value between 0 and 1.</param>
        /// <returns></returns>
        public Point GetPoint(double parameter) { return default(Point); }
        
        /// <summary>
        /// Returns the tangent point on the curve at the specified position.
        /// </summary>
        /// <param name="parameter">the position at which to find the tangent point as a value between 0 and 1.</param>
        public void GetTangent(double parameter) { }
        
        /// <summary>
        /// Returns the normal point on the curve at the specified position.
        /// </summary>
        /// <param name="parameter">the position at which to find the normal point as a value between 0 and 1.</param>
        public void GetNormal(double parameter) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double GetParameter(Point point) { return default(double); }
        
        /// <summary>
        /// Returns a reversed version of the curve, without modifying the curve itself.
        /// </summary>
        /// <returns>a reversed version of the curve</returns>
        public Curve Reverse() { return default(Curve); }
        
        /// <summary>
        /// Returns a copy of the curve.
        /// </summary>
        /// <returns></returns>
        public Curve Clone() { return default(Curve); }
        
        #endregion
        
    }
}