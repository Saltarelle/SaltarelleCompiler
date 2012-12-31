using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The Point object represents a point in the two dimensional space of the Paper.js project. It is also used to represent two dimensional vector objects.  Example &mdash; Create a point at x: 10, y: 5   var point = new Point(10, 5); console.log(point.x); // 10 console.log(point.y); // 5
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Point
    {
        #region Properties
        
        
        /// <summary>
        /// The x coordinate of the point
        /// </summary>
        public double X;
        
        /// <summary>
        /// The y coordinate of the point
        /// </summary>
        public double Y;
        
        /// <summary>
        /// The length of the vector that is represented by this point's coordinates. Each point can be interpreted as a vector that points from the origin (x = 0, y = 0) to the point's location. Setting the length changes the location but keeps the vector's angle.
        /// </summary>
        public double Length;
        
        /// <summary>
        /// The vector's angle in degrees, measured from the x-axis to the vector. The angle is unsigned, no information about rotational direction is given.
        /// </summary>
        public double Angle;
        
        /// <summary>
        /// The vector's angle in radians, measured from the x-axis to the vector. The angle is unsigned, no information about rotational direction is given.
        /// </summary>
        public double AngleInRadians;
        
        /// <summary>
        /// The quadrant of the angle of the point. Angles between 0 and 90 degrees are in quadrant 1. Angles between 90 and 180 degrees are in quadrant 2, angles between 180 and 270 degrees are in quadrant 3 and angles between 270 and 360 degrees are in quadrant 4.
        /// </summary>
        public double Quadrant;
        
        /// <summary>
        /// This property is only present if the point is an anchor or control point of a Segment or a Curve. In this case, it returns true it is selected, false otherwise
        /// </summary>
        public bool Selected;
        
        #endregion
        
        #region Constructors

        protected Point(){ }
        
        /// <summary>
        /// Creates a Point object with the given x and y coordinates.
        /// </summary>
        /// <param name="x">the x coordinate</param>
        /// <param name="y">the y coordinate</param>
        [ScriptName("")]
        public Point(double x, double y){ }
        
        /// <summary>
        /// Creates a Point object using the numbers in the given array as coordinates.
        /// </summary>
        /// <param name="array"></param>
        [ScriptName("")]
        public Point(Array array){ }
        
        /// <summary>
        /// Creates a Point object using the properties in the given object.
        /// </summary>
        /// <param name="_object"></param>
        [ScriptName("")]
        public Point(object _object){ }
        
        /// <summary>
        /// Creates a Point object using the width and height values of the given Size object.
        /// </summary>
        /// <param name="size"></param>
        [ScriptName("")]
        public Point(Size size){ }
        
        /// <summary>
        /// Creates a Point object using the coordinates of the given Point object.
        /// </summary>
        /// <param name="point"></param>
        [ScriptName("")]
        public Point(Point point){ }

        #endregion
        
        #region Operators
        
        /// <summary>
        /// Returns the addition of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the point and the value as a new point</returns>
        public Point Add(double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the addition of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the point and the value as a new point</returns>
        [InlineCode("{point}.add({operand})")]
        static public Point operator +(Point point, double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the addition of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the two points as a new point</returns>
        public Point Add(Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the addition of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the two points as a new point</returns>
        [InlineCode("{point}.add({operand})")]
        static public Point operator +(Point point, Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the subtraction of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the point and the value as a new point</returns>
        public Point Subtract(double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the subtraction of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the point and the value as a new point</returns>
        [InlineCode("{point}.subtract({operand})")]
        static public Point operator -(Point point, double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the subtraction of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the two points as a new point</returns>
        public Point Subtract(Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the subtraction of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the two points as a new point</returns>
        [InlineCode("{point}.subtract({operand})")]
        static public Point operator -(Point point, Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the multiplication of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the point and the value as a new point</returns>
        public Point Multiply(double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the multiplication of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the point and the value as a new point</returns>
        [InlineCode("{point}.multiply({operand})")]
        static public Point operator *(Point point, double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the multiplication of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the two points as a new point</returns>
        public Point Multiply(Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the multiplication of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the two points as a new point</returns>
        [InlineCode("{point}.multiply({operand})")]
        static public Point operator *(Point point, Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the division of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the point and the value as a new point</returns>
        public Point Divide(double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the division of the supplied value to both coordinates of the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the point and the value as a new point</returns>
        [InlineCode("{point}.divide({operand})")]
        static public Point operator /(Point point, double operand) { return default(Point); }
        
        /// <summary>
        /// Returns the division of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the two points as a new point</returns>
        public Point Divide(Point operand) { return default(Point); }
        
        /// <summary>
        /// Returns the division of the supplied point to the point as a new point. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the two points as a new point</returns>
        [InlineCode("{point}.divide({operand})")]
        static public Point operator /(Point point, Point operand) { return default(Point); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the point by the supplied value as a new point.
        /// </summary>
        /// <returns>the integer remainders of dividing the point by the value as a new point</returns>
        public Point Modulo(double operand) { return default(Point); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the point by the supplied value as a new point.
        /// </summary>
        /// <returns>the integer remainders of dividing the point by the value as a new point</returns>
        [InlineCode("{point}.modulo({operand})")]
        static public Point operator %(Point point, double operand) { return default(Point); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the point by the supplied value as a new point.
        /// </summary>
        /// <returns>the integer remainders of dividing the points by each other as a new point</returns>
        public Point Modulo(Point operand) { return default(Point); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the point by the supplied value as a new point.
        /// </summary>
        /// <returns>the integer remainders of dividing the points by each other as a new point</returns>
        [InlineCode("{point}.modulo({operand})")]
        static public Point operator %(Point point, Point operand) { return default(Point); }
        
        /// <summary>
        /// Checks whether the coordinates of the point are equal to that of the supplied point.
        /// </summary>
        /// <returns>true if the points are equal, false otherwise</returns>
        public bool Equals(Point operand) { return default(bool); }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Returns a copy of the point.
        /// </summary>
        /// <returns>the cloned point</returns>
        public Point Clone() { return default(Point); }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>A string representation of the point.</returns>
        public string ToString() { return default(string); }
        
        /// <summary>
        /// Transforms the point by the matrix as a new point. The object itself is not modified!
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>The transformed point</returns>
        public Point Transform(Matrix matrix) { return default(Point); }
        
        /// <summary>
        /// Returns the distance between the point and another point.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="squared">Controls whether the distance should remain squared, or its square root should be calculated.</param>
        /// <returns></returns>
        public double GetDistance(Point point, bool squared) { return default(double); }
        
        /// <summary>
        /// Normalize modifies the length of the vector to 1 without changing its angle and returns it as a new point. The optional length parameter defines the length to normalize to. The object itself is not modified!
        /// </summary>
        /// <param name="length">The length of the normalized vector - optional, default: 1</param>
        /// <returns>The normalized vector of the vector that is represented by this point's coordinates.</returns>
        public Point Normalize(double length) { return default(Point); }
        
        /// <summary>
        /// Normalize modifies the length of the vector to 1 without changing its angle and returns it as a new point. The optional length parameter defines the length to normalize to. The object itself is not modified!
        /// </summary>
        /// <returns>The normalized vector of the vector that is represented by this point's coordinates.</returns>
        public Point Normalize() { return default(Point); }
        
        /// <summary>
        /// Returns the smaller angle between two vectors. The angle is unsigned, no information about rotational direction is given.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>the angle in degrees</returns>
        public double GetAngle(Point point) { return default(double); }
        
        /// <summary>
        /// Returns the smaller angle between two vectors in radians. The angle is unsigned, no information about rotational direction is given.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>the angle in radians</returns>
        public double GetAngleInRadians(Point point) { return default(double); }
        
        /// <summary>
        /// Returns the angle between two vectors. The angle is directional and signed, giving information about the rotational direction. Read more about angle units and orientation in the description of the angle property.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>the angle between the two vectors</returns>
        public double GetDirectedAngle(Point point) { return default(double); }
        
        /// <summary>
        /// Rotates the point by the given angle around an optional center point. The object itself is not modified. Read more about angle units and orientation in the description of the angle property.
        /// </summary>
        /// <param name="angle">the rotation angle</param>
        /// <param name="center">the center point of the rotation</param>
        /// <returns>the rotated point</returns>
        public Point Rotate(double angle, Point center) { return default(Point); }
        
        /// <summary>
        /// Checks whether the point is inside the boundaries of the rectangle.
        /// </summary>
        /// <param name="rect">the rectangle to check against</param>
        /// <returns>true if the point is inside the rectangle, false otherwise</returns>
        public bool IsInside(Rectangle rect) { return default(bool); }
        
        /// <summary>
        /// Checks if the point is within a given distance of another point.
        /// </summary>
        /// <param name="point">the point to check against</param>
        /// <param name="tolerance">the maximum distance allowed</param>
        /// <returns>true if it is within the given distance, false otherwise</returns>
        public bool IsClose(Point point, double tolerance) { return default(bool); }
        
        /// <summary>
        /// Checks if the vector represented by this point is colinear (parallel) to another vector.
        /// </summary>
        /// <param name="point">the vector to check against</param>
        /// <returns>true it is parallel, false otherwise</returns>
        public bool IsColinear(Point point) { return default(bool); }
        
        /// <summary>
        /// Checks if the vector represented by this point is orthogonal (perpendicular) to another vector.
        /// </summary>
        /// <param name="point">the vector to check against</param>
        /// <returns>true it is orthogonal, false otherwise</returns>
        public bool IsOrthogonal(Point point) { return default(bool); }
        
        /// <summary>
        /// Checks if this point has both the x and y coordinate set to 0.
        /// </summary>
        /// <returns>true both x and y are 0, false otherwise</returns>
        public bool IsZero() { return default(bool); }
        
        /// <summary>
        /// Checks if this point has an undefined value for at least one of its coordinates.
        /// </summary>
        /// <returns>true if either x or y are not a number, false otherwise</returns>
        public bool IsNaN() { return default(bool); }
        
        /// <summary>
        /// Returns the dot product of the point and another point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>the dot product of the two points</returns>
        public double Dot(Point point) { return default(double); }
        
        /// <summary>
        /// Returns the cross product of the point and another point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>the cross product of the two points</returns>
        public double Cross(Point point) { return default(double); }
        
        /// <summary>
        /// Returns the projection of the point on another point. Both points are interpreted as vectors.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>the projection of the point on another point</returns>
        public Point Project(Point point) { return default(Point); }
        
        /// <summary>
        /// Returns a new point with rounded x and y values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Point Round() { return default(Point); }
        
        /// <summary>
        /// Returns a new point with the nearest greater non-fractional values to the specified x and y values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Point Ceil() { return default(Point); }
        
        /// <summary>
        /// Returns a new point with the nearest smaller non-fractional values to the specified x and y values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Point Floor() { return default(Point); }
        
        /// <summary>
        /// Returns a new point with the absolute values of the specified x and y values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Point Abs() { return default(Point); }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// Returns a new point object with the smallest x and y of the supplied points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>The newly created point object</returns>
        public static Point Min(Point point1, Point point2) { return default(Point); }
        
        /// <summary>
        /// Returns a new point object with the largest x and y of the supplied points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>The newly created point object</returns>
        public static Point Max(Point point1, Point point2) { return default(Point); }
        
        /// <summary>
        /// Returns a point object with random x and y values between 0 and 1.
        /// </summary>
        /// <returns>The newly created point object</returns>
        public static Point Random() { return default(Point); }
        
        #endregion
    }
}