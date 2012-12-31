using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// A HitResult object contains information about the results of a hit test. It is returned by item.hitTest(point) and project.hitTest(point).
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class HitResult
    {
        #region Properties
        
        
        /// <summary>
        /// Describes the type of the hit result. For example, if you hit a segment point, the type would be 'segment'.
        /// </summary>
        public HitResultType Type;
        
        /// <summary>
        /// If the HitResult has a hitResult.type of 'bounds', this property describes which corner of the bounding rectangle was hit.
        /// </summary>
        public HitResultName Name;
        
        /// <summary>
        /// The item that was hit.
        /// </summary>
        public Item Item;
        
        /// <summary>
        /// If the HitResult has a type of 'stroke', this property gives more information about the exact position that was hit on the path.
        /// </summary>
        public CurveLocation Location;
        
        /// <summary>
        /// If the HitResult has a type of 'stroke', 'segment', 'handle-in' or 'handle-out', this property refers to the Segment that was hit or that is closest to the hitResult.location on the curve.
        /// </summary>
        public Segment Segment;
        
        /// <summary>
        /// The hit point.
        /// </summary>
        public Point Point;
        
        #endregion
    }
}