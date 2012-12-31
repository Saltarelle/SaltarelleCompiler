using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The Segment object represents the points of a path through which its Curve objects pass. The segments of a path can be accessed through its path.segments array. Each segment consists of an anchor point (segment.point) and optionaly an incoming and an outgoing handle (segment.handleIn and segment.handleOut), describing the tangents of the two Curve objects that are connected by this segment.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Segment
    {
        #region Properties
        
        
        /// <summary>
        /// The anchor point of the segment.
        /// </summary>
        public Point Point;
        
        /// <summary>
        /// The handle point relative to the anchor point of the segment that describes the in tangent of the segment.
        /// </summary>
        public Point HandleIn;
        
        /// <summary>
        /// The handle point relative to the anchor point of the segment that describes the out tangent of the segment.
        /// </summary>
        public Point HandleOut;
        
        /// <summary>
        /// Specifies whether the point of the segment is selected.
        /// </summary>
        public bool Selected;
        
        /// <summary>
        /// The index of the segment in the path.segments array that the segment belongs to.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// The path that the segment belongs to.
        /// </summary>
        public Path Path;
        
        /// <summary>
        /// The curve that the segment belongs to.
        /// </summary>
        public Curve Curve;
        
        /// <summary>
        /// The next segment in the path.segments array that the segment belongs to. If the segments belongs to a closed path, the first segment is returned for the last segment of the path.
        /// </summary>
        public Segment Next;
        
        /// <summary>
        /// The previous segment in the path.segments array that the segment belongs to. If the segments belongs to a closed path, the last segment is returned for the first segment of the path.
        /// </summary>
        public Segment Previous;
        
        #endregion
        
        #region Constructors

        protected Segment(){ }
        
        /// <summary>
        /// Creates a new Segment object.
        /// </summary>
        /// <param name="point">the anchor point of the segment - optional, default: {x: 0, y: 0}</param>
        /// <param name="handleIn">the handle point relative to the anchor point of the segment that describes the in tangent of the segment. - optional, default: {x: 0, y: 0}</param>
        /// <param name="handleOut">the handle point relative to the anchor point of the segment that describes the out tangent of the segment. - optional, default: {x: 0, y: 0}</param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        [ScriptName("")]
        public Segment(Point point, Point handleIn, Point handleOut, object arg3, object arg4, object arg5){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Returns the reversed the segment, without modifying the segment itself.
        /// </summary>
        /// <returns>the reversed segment</returns>
        public Segment Reverse() { return default(Segment); }
        
        /// <summary>
        ///
        /// </summary>
        public void Remove() { }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>A string representation of the segment.</returns>
        public string ToString() { return default(string); }
        
        #endregion
        
    }
}