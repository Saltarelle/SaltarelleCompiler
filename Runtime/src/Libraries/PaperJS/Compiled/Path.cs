using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The Path item represents a path in a Paper.js project.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Path : PathItem
    {
        #region Properties
        
        
        /// <summary>
        /// The segments contained within the path.
        /// </summary>
        public Segment[] Segments;
        
        /// <summary>
        /// The first Segment contained within the path.
        /// </summary>
        public Segment FirstSegment;
        
        /// <summary>
        /// The last Segment contained within the path.
        /// </summary>
        public Segment LastSegment;
        
        /// <summary>
        /// The curves contained within the path.
        /// </summary>
        public Curve[] Curves;
        
        /// <summary>
        /// The first Curve contained within the path.
        /// </summary>
        public Curve FirstCurve;
        
        /// <summary>
        /// The last Curve contained within the path.
        /// </summary>
        public Curve LastCurve;
        
        /// <summary>
        /// Specifies whether the path is closed. If it is closed, Paper.js connects the first and last segments.
        /// </summary>
        public bool Closed;
        
        /// <summary>
        /// Specifies whether the path and all its segments are selected.
        /// </summary>
        public bool FullySelected;
        
        /// <summary>
        /// Specifies whether the path is oriented clock-wise.
        /// </summary>
        public bool Clockwise;
        
        /// <summary>
        /// The length of the perimeter of the path.
        /// </summary>
        public double Length;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new Path item and places it at the top of the active layer.
        /// </summary>
        /// <param name="segments">An array of segments (or points to be converted to segments) that will be added to the path. - optional</param>
        [ScriptName("")]
        public Path(Segment[] segments){ }
        
        /// <summary>
        /// Creates a new Path item and places it at the top of the active layer.
        /// </summary>
        [ScriptName("")]
        public Path(){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Adds one or more segments to the end of the segments array of this path.
        /// </summary>
        /// <param name="segment">the segment or point to be added.</param>
        /// <returns>the added segment. This is not necessarily the same object, e.g. if the segment to be added already belongs to another path.</returns>
        public Segment Add(Segment segment) { return default(Segment); }
        
        /// <summary>
        /// Adds one or more segments to the end of the segments array of this path.
        /// </summary>
        /// <param name="segment">the segment or point to be added.</param>
        /// <returns>the added segment. This is not necessarily the same object, e.g. if the segment to be added already belongs to another path.</returns>
        public Segment Add(Point segment) { return default(Segment); }
        
        /// <summary>
        /// Inserts one or more segments at a given index in the list of this path's segments.
        /// </summary>
        /// <param name="index">the index at which to insert the segment.</param>
        /// <param name="segment">the segment or point to be inserted.</param>
        /// <returns>the added segment. This is not necessarily the same object, e.g. if the segment to be added already belongs to another path.</returns>
        public Segment Insert(int index, Segment segment) { return default(Segment); }
        
        /// <summary>
        /// Inserts one or more segments at a given index in the list of this path's segments.
        /// </summary>
        /// <param name="index">the index at which to insert the segment.</param>
        /// <param name="segment">the segment or point to be inserted.</param>
        /// <returns>the added segment. This is not necessarily the same object, e.g. if the segment to be added already belongs to another path.</returns>
        public Segment Insert(int index, Point segment) { return default(Segment); }
        
        /// <summary>
        /// Adds an array of segments (or types that can be converted to segments) to the end of the segments array.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns>an array of the added segments. These segments are not necessarily the same objects, e.g. if the segment to be added already belongs to another path.</returns>
        public Segment[] AddSegments(Segment[] segments) { return default(Segment[]); }
        
        /// <summary>
        /// Inserts an array of segments at a given index in the path's segments array.
        /// </summary>
        /// <param name="index">the index at which to insert the segments.</param>
        /// <param name="segments">the segments to be inserted.</param>
        /// <returns>an array of the added segments. These segments are not necessarily the same objects, e.g. if the segment to be added already belongs to another path.</returns>
        public Segment[] InsertSegments(int index, Segment[] segments) { return default(Segment[]); }
        
        /// <summary>
        /// Removes the segment at the specified index of the path's segments array.
        /// </summary>
        /// <param name="index">the index of the segment to be removed</param>
        /// <returns>the removed segment</returns>
        public Segment RemoveSegment(int index) { return default(Segment); }
        
        /// <summary>
        /// Removes all segments from the path's segments array.
        /// </summary>
        /// <returns>an array containing the removed segments</returns>
        public Segment[] RemoveSegments() { return default(Segment[]); }
        
        /// <summary>
        /// Removes the segments from the specified from index to the to index from the path's segments array.
        /// </summary>
        /// <param name="from">the beginning index, inclusive</param>
        /// <param name="to">the ending index, exclusive - optional, default: segments.length</param>
        /// <returns>an array containing the removed segments</returns>
        public Segment[] RemoveSegments(double from, double to) { return default(Segment[]); }
        
        /// <summary>
        /// Removes the segments from the specified from index to the to index from the path's segments array.
        /// </summary>
        /// <param name="from">the beginning index, inclusive</param>
        /// <returns>an array containing the removed segments</returns>
        public Segment[] RemoveSegments(double from) { return default(Segment[]); }
        
        /// <summary>
        /// Converts the curves in a path to straight lines with an even distribution of points. The distance between the produced segments is as close as possible to the value specified by the maxDistance parameter.
        /// </summary>
        /// <param name="maxDistance">the maximum distance between the points</param>
        public void Flatten(double maxDistance) { }
        
        /// <summary>
        /// Smooths a path by simplifying it. The path.segments array is analyzed and replaced by a more optimal set of segments, reducing memory usage and speeding up drawing.
        /// </summary>
        /// <param name="tolerance">optional, default: 2.5</param>
        public void Simplify(double tolerance) { }
        
        /// <summary>
        /// Smooths a path by simplifying it. The path.segments array is analyzed and replaced by a more optimal set of segments, reducing memory usage and speeding up drawing.
        /// </summary>
        public void Simplify() { }
        
        /// <summary>
        ///
        /// </summary>
        public void Reverse() { }
        
        /// <summary>
        /// Joins the path with the specified path, which will be removed in the process.
        /// </summary>
        /// <param name="path"></param>
        public void Join(Path path) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="isParameter">optional, default: false</param>
        /// <returns></returns>
        public CurveLocation GetLocationAt(double offset, bool isParameter) { return default(CurveLocation); }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public CurveLocation GetLocationAt(double offset) { return default(CurveLocation); }
        
        /// <summary>
        /// Get the point on the path at the given offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="isParameter">optional, default: false</param>
        /// <returns>the point at the given offset</returns>
        public Point GetPointAt(double offset, bool isParameter) { return default(Point); }
        
        /// <summary>
        /// Get the point on the path at the given offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>the point at the given offset</returns>
        public Point GetPointAt(double offset) { return default(Point); }
        
        /// <summary>
        /// Get the tangent to the path at the given offset as a vector point.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="isParameter">optional, default: false</param>
        /// <returns>the tangent vector at the given offset</returns>
        public Point GetTangentAt(double offset, bool isParameter) { return default(Point); }
        
        /// <summary>
        /// Get the tangent to the path at the given offset as a vector point.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>the tangent vector at the given offset</returns>
        public Point GetTangentAt(double offset) { return default(Point); }
        
        /// <summary>
        /// Get the normal to the path at the given offset as a vector point.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="isParameter">optional, default: false</param>
        /// <returns>the normal vector at the given offset</returns>
        public Point GetNormalAt(double offset, bool isParameter) { return default(Point); }
        
        /// <summary>
        /// Get the normal to the path at the given offset as a vector point.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>the normal vector at the given offset</returns>
        public Point GetNormalAt(double offset) { return default(Point); }
        
        /// <summary>
        /// Returns the nearest location on the path to the specified point.
        /// </summary>
        /// <param name="point">{Point} The point for which we search the nearest location</param>
        /// <returns>The location on the path that's the closest to the specified point</returns>
        public CurveLocation GetNearestLocation(Point point) { return default(CurveLocation); }
        
        /// <summary>
        /// Returns the nearest point on the path to the specified point.
        /// </summary>
        /// <param name="point">{Point} The point for which we search the nearest point</param>
        /// <returns>The point on the path that's the closest to the specified point</returns>
        public Point GetNearestPoint(Point point) { return default(Point); }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// Creates a Path Item with two anchor points forming a line.
        /// </summary>
        /// <param name="pt1">the first anchor point of the path</param>
        /// <param name="pt2">the second anchor point of the path</param>
        /// <returns>the newly created path</returns>
        public static Path Line(Point pt1, Point pt2) { return default(Path); }
        
        /// <summary>
        /// Creates a rectangle shaped Path Item from the passed point and size.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <returns>the newly created path</returns>
        public static Path Rectangle(Point point, Size size) { return default(Path); }
        
        /// <summary>
        /// Creates a rectangle shaped Path Item from the passed points. These do not necessarily need to be the top left and bottom right corners, the constructor figures out how to fit a rectangle between them.
        /// </summary>
        /// <param name="point1">The first point defining the rectangle</param>
        /// <param name="point2">The second point defining the rectangle</param>
        /// <returns>the newly created path</returns>
        public static Path Rectangle(Point point1, Point point2) { return default(Path); }
        
        /// <summary>
        /// Creates a rectangle shaped Path Item from the passed abstract Rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>the newly created path</returns>
        public static Path Rectangle(Rectangle rect) { return default(Path); }
        
        /// <summary>
        /// Creates a rectangular Path Item with rounded corners.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="size">the size of the rounded corners</param>
        /// <returns>the newly created path</returns>
        public static Path RoundRectangle(Rectangle rect, Size size) { return default(Path); }
        
        /// <summary>
        /// Creates an oval shaped Path Item.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="circumscribed">when set to true the oval shaped path will be created so the rectangle fits into it. When set to false the oval path will fit within the rectangle. - optional, default: false</param>
        /// <returns>the newly created path</returns>
        public static Path Oval(Rectangle rect, bool circumscribed) { return default(Path); }
        
        /// <summary>
        /// Creates an oval shaped Path Item.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>the newly created path</returns>
        public static Path Oval(Rectangle rect) { return default(Path); }
        
        /// <summary>
        /// Creates a circle shaped Path Item.
        /// </summary>
        /// <param name="center">the center point of the circle</param>
        /// <param name="radius">the radius of the circle</param>
        /// <returns>the newly created path</returns>
        public static Path Circle(Point center, double radius) { return default(Path); }
        
        /// <summary>
        /// Creates a circular arc shaped Path Item.
        /// </summary>
        /// <param name="from">the starting point of the circular arc</param>
        /// <param name="through">the point the arc passes through</param>
        /// <param name="to">the end point of the arc</param>
        /// <returns>the newly created path</returns>
        public static Path Arc(Point from, Point through, Point to) { return default(Path); }
        
        /// <summary>
        /// Creates a regular polygon shaped Path Item.
        /// </summary>
        /// <param name="center">the center point of the polygon</param>
        /// <param name="numSides">the number of sides of the polygon</param>
        /// <param name="radius">the radius of the polygon</param>
        /// <returns>the newly created path</returns>
        public static Path RegularPolygon(Point center, int numSides, double radius) { return default(Path); }
        
        /// <summary>
        /// Creates a star shaped Path Item. The largest of radius1 and radius2 will be the outer radius of the star. The smallest of radius1 and radius2 will be the inner radius.
        /// </summary>
        /// <param name="center">the center point of the star</param>
        /// <param name="numPoints">the number of points of the star</param>
        /// <param name="radius1"></param>
        /// <param name="radius2"></param>
        /// <returns>the newly created path</returns>
        public static Path Star(Point center, int numPoints, double radius1, double radius2) { return default(Path); }
        
        #endregion
    }
}