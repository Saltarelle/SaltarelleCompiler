using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The PathItem class is the base for any items that describe paths and offer standardised methods for drawing and path manipulation, such as Path and CompoundPath.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PathItem : Item
    {
    
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        public void Smooth() { }
        
        /// <summary>
        /// If called on a CompoundPath, a new Path is created as a child and the point is added as its first segment. On a normal empty Path, the point is simply added as its first segment.
        /// </summary>
        /// <param name="point"></param>
        public void MoveTo(Point point) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        public void LineTo(Point point) { }
        
        /// <summary>
        /// Adds a cubic bezier curve to the path, defined by two handles and a to point.
        /// </summary>
        /// <param name="handle1"></param>
        /// <param name="handle2"></param>
        /// <param name="to"></param>
        public void CubicCurveTo(Point handle1, Point handle2, Point to) { }
        
        /// <summary>
        /// Adds a quadratic bezier curve to the path, defined by a handle and a to point.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="to"></param>
        public void QuadraticCurveTo(Point handle, Point to) { }
        
        /// <summary>
        /// Draws a curve from the position of the last segment point in the path that goes through the specified through point, to the specified to point by adding one segment to the path.
        /// </summary>
        /// <param name="through">the point through which the curve should go</param>
        /// <param name="to">the point where the curve should end</param>
        /// <param name="parameter">optional, default: 0.5</param>
        public void CurveTo(Point through, Point to, double parameter) { }
        
        /// <summary>
        /// Draws a curve from the position of the last segment point in the path that goes through the specified through point, to the specified to point by adding one segment to the path.
        /// </summary>
        /// <param name="through">the point through which the curve should go</param>
        /// <param name="to">the point where the curve should end</param>
        public void CurveTo(Point through, Point to) { }
        
        /// <summary>
        /// Draws an arc from the position of the last segment point in the path that goes through the specified through point, to the specified to point by adding one or more segments to the path.
        /// </summary>
        /// <param name="through">the point where the arc should pass through</param>
        /// <param name="to">the point where the arc should end</param>
        public void ArcTo(Point through, Point to) { }
        
        /// <summary>
        /// Draws an arc from the position of the last segment point in the path to the specified point by adding one or more segments to the path.
        /// </summary>
        /// <param name="to">the point where the arc should end</param>
        /// <param name="clockwise">specifies whether the arc should be drawn in clockwise direction. - optional, default: true</param>
        public void ArcTo(Point to, bool clockwise) { }
        
        /// <summary>
        /// Draws an arc from the position of the last segment point in the path to the specified point by adding one or more segments to the path.
        /// </summary>
        /// <param name="to">the point where the arc should end</param>
        public void ArcTo(Point to) { }
        
        /// <summary>
        ///
        /// </summary>
        public void ClosePath() { }
        
        /// <summary>
        /// If called on a CompoundPath, a new Path is created as a child and the point is added as its first segment relative to the position of the last segment of the current path.
        /// </summary>
        /// <param name="point"></param>
        public void MoveBy(Point point) { }
        
        /// <summary>
        /// Adds a segment relative to the last segment point of the path.
        /// </summary>
        /// <param name="vector">The vector which is added to the position of the last segment of the path, to become the new segment.</param>
        public void LineBy(Point vector) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="throughVector"></param>
        /// <param name="toVector"></param>
        /// <param name="parameter">optional, default: 0.5</param>
        public void CurveBy(Point throughVector, Point toVector, double parameter) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="throughVector"></param>
        /// <param name="toVector"></param>
        public void CurveBy(Point throughVector, Point toVector) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="throughVector"></param>
        /// <param name="toVector"></param>
        public void ArcBy(Point throughVector, Point toVector) { }
        
        /// <summary>
        /// Inserts the specified item as a child of this item by appending it to the list of children and moving it above all other children. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="item">The item to be appended as a child</param>
        public void AppendTop(Item item) { }
        
        /// <summary>
        /// Inserts the specified item as a child of this item by appending it to the list of children and moving it below all other children. You can use this function for groups, compound paths and layers.
        /// </summary>
        /// <param name="item">The item to be appended as a child</param>
        public void AppendBottom(Item item) { }
        
        /// <summary>
        /// Moves this item above the specified item.
        /// </summary>
        /// <param name="item">The item above which it should be moved</param>
        /// <returns>true it was moved, false otherwise</returns>
        public bool MoveAbove(Item item) { return default(bool); }
        
        /// <summary>
        /// Moves the item below the specified item.
        /// </summary>
        /// <param name="item">the item below which it should be moved</param>
        /// <returns>true it was moved, false otherwise</returns>
        public bool MoveBelow(Item item) { return default(bool); }
        
        #endregion
        
    }
}