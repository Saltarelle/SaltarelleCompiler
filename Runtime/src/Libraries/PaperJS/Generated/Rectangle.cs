using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// A Rectangle specifies an area that is enclosed by it's top-left point (x, y), its width, and its height. It should not be confused with a rectangular path, it is not an item.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Rectangle
    {
        #region Properties
        
        
        /// <summary>
        /// The x position of the rectangle.
        /// </summary>
        public double X;
        
        /// <summary>
        /// The y position of the rectangle.
        /// </summary>
        public double Y;
        
        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public double Width;
        
        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public double Height;
        
        /// <summary>
        /// The top-left point of the rectangle
        /// </summary>
        public Point Point;
        
        /// <summary>
        /// The size of the rectangle
        /// </summary>
        public Size Size;
        
        /// <summary>
        /// The position of the left hand side of the rectangle. Note that this doesn't move the whole rectangle; the right hand side stays where it was.
        /// </summary>
        public double Left;
        
        /// <summary>
        /// The top coordinate of the rectangle. Note that this doesn't move the whole rectangle: the bottom won't move.
        /// </summary>
        public double Top;
        
        /// <summary>
        /// The position of the right hand side of the rectangle. Note that this doesn't move the whole rectangle; the left hand side stays where it was.
        /// </summary>
        public double Right;
        
        /// <summary>
        /// The bottom coordinate of the rectangle. Note that this doesn't move the whole rectangle: the top won't move.
        /// </summary>
        public double Bottom;
        
        /// <summary>
        /// The center point of the rectangle.
        /// </summary>
        public Point Center;
        
        /// <summary>
        /// The top-left point of the rectangle.
        /// </summary>
        public Point TopLeft;
        
        /// <summary>
        /// The top-right point of the rectangle.
        /// </summary>
        public Point TopRight;
        
        /// <summary>
        /// The bottom-left point of the rectangle.
        /// </summary>
        public Point BottomLeft;
        
        /// <summary>
        /// The bottom-right point of the rectangle.
        /// </summary>
        public Point BottomRight;
        
        /// <summary>
        /// The left-center point of the rectangle.
        /// </summary>
        public Point LeftCenter;
        
        /// <summary>
        /// The top-center point of the rectangle.
        /// </summary>
        public Point TopCenter;
        
        /// <summary>
        /// The right-center point of the rectangle.
        /// </summary>
        public Point RightCenter;
        
        /// <summary>
        /// The bottom-center point of the rectangle.
        /// </summary>
        public Point BottomCenter;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected Rectangle(){ }
        
        /// <summary>
        /// Creates a Rectangle object.
        /// </summary>
        /// <param name="point">the top-left point of the rectangle</param>
        /// <param name="size">the size of the rectangle</param>
        [ScriptName("")]
        public Rectangle(Point point, Size size){ }
        
        /// <summary>
        /// Creates a rectangle object.
        /// </summary>
        /// <param name="x">the left coordinate</param>
        /// <param name="y">the top coordinate</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        [ScriptName("")]
        public Rectangle(double x, double y, double width, double height){ }
        
        /// <summary>
        /// Creates a rectangle object from the passed points. These do not necessarily need to be the top left and bottom right corners, the constructor figures out how to fit a rectangle between them.
        /// </summary>
        /// <param name="point1">The first point defining the rectangle</param>
        /// <param name="point2">The second point defining the rectangle</param>
        [ScriptName("")]
        public Rectangle(Point point1, Point point2){ }
        
        /// <summary>
        /// Creates a new rectangle object from the passed rectangle object.
        /// </summary>
        /// <param name="rt"></param>
        [ScriptName("")]
        public Rectangle(Rectangle rt){ }

        #endregion
        
        #region Operators
        
        /// <summary>
        /// Checks whether the coordinates and size of the rectangle are equal to that of the supplied rectangle.
        /// </summary>
        /// <returns>true if the rectangles are equal, false otherwise</returns>
        public bool Equals(Rectangle operand) { return default(bool); }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>true the rectangle is empty, false otherwise</returns>
        public bool IsEmpty() { return default(bool); }
        
        /// <summary>
        /// Tests if the specified point is inside the boundary of the rectangle.
        /// </summary>
        /// <param name="point">the specified point</param>
        /// <returns>true if the point is inside the rectangle's boundary, false otherwise</returns>
        public bool Contains(Point point) { return default(bool); }
        
        /// <summary>
        /// Tests if the interior of the rectangle entirely contains the specified rectangle.
        /// </summary>
        /// <param name="rect">The specified rectangle</param>
        /// <returns>true if the rectangle entirely contains the specified rectangle, false otherwise</returns>
        public bool Contains(Rectangle rect) { return default(bool); }
        
        /// <summary>
        /// Tests if the interior of this rectangle intersects the interior of another rectangle.
        /// </summary>
        /// <param name="rect">the specified rectangle</param>
        /// <returns>true if the rectangle and the specified rectangle intersect each other, false otherwise</returns>
        public bool Intersects(Rectangle rect) { return default(bool); }
        
        /// <summary>
        /// Returns a new rectangle representing the intersection of this rectangle with the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to be intersected with this rectangle</param>
        /// <returns>The largest rectangle contained in both the specified rectangle and in this rectangle.</returns>
        public Rectangle Intersect(Rectangle rect) { return default(Rectangle); }
        
        /// <summary>
        /// Returns a new rectangle representing the union of this rectangle with the specified rectangle.
        /// </summary>
        /// <param name="rect">the rectangle to be combined with this rectangle</param>
        /// <returns>the smallest rectangle containing both the specified rectangle and this rectangle.</returns>
        public Rectangle Unite(Rectangle rect) { return default(Rectangle); }
        
        /// <summary>
        /// Adds a point to this rectangle. The resulting rectangle is the smallest rectangle that contains both the original rectangle and the specified point. After adding a point, a call to contains(point) with the added point as an argument does not necessarily return true. The rectangle.contains(point) method does not return true for points on the right or bottom edges of a rectangle. Therefore, if the added point falls on the left or bottom edge of the enlarged rectangle, rectangle.contains(point) returns false for that point.
        /// </summary>
        /// <param name="point"></param>
        public void Include(Point point) { }
        
        #endregion
        
    }
}