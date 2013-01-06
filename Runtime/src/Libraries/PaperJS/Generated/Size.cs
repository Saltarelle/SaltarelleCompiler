using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The Size object is used to describe the size of something, through its width and height properties.  Example  Create a size that is 10pt wide and 5pt high   var size = new Size(10, 5); console.log(size.width); // 10 console.log(size.height); // 5
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Size
    {
        #region Properties
        
        
        /// <summary>
        /// The width of the size
        /// </summary>
        public double Width;
        
        /// <summary>
        /// The height of the size
        /// </summary>
        public double Height;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected Size(){ }
        
        /// <summary>
        /// Creates a Size object with the given width and height values.
        /// </summary>
        /// <param name="width">the width</param>
        /// <param name="height">the height</param>
        /// <param name="array"></param>
        [ScriptName("")]
        public Size(double width, double height, Array array){ }
        
        /// <summary>
        /// Creates a Size object using the properties in the given object.
        /// </summary>
        /// <param name="_object"></param>
        [ScriptName("")]
        public Size(object _object){ }
        
        /// <summary>
        /// Creates a Size object using the coordinates of the given Size object.
        /// </summary>
        /// <param name="size"></param>
        [ScriptName("")]
        public Size(Size size){ }
        
        /// <summary>
        /// Creates a Size object using the point.x and point.y values of the given Point object.
        /// </summary>
        /// <param name="point"></param>
        [ScriptName("")]
        public Size(Point point){ }

        #endregion
        
        #region Operators
        
        /// <summary>
        /// Returns the addition of the supplied value to the width and height of the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the size and the value as a new size</returns>
        public Size Add(double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the addition of the supplied value to the width and height of the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the size and the value as a new size</returns>
        [InlineCode("{size}.add({operand})")]
        static public Size operator +(Size size, double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the addition of the width and height of the supplied size to the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the two sizes as a new size</returns>
        public Size Add(Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the addition of the width and height of the supplied size to the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the addition of the two sizes as a new size</returns>
        [InlineCode("{size}.add({operand})")]
        static public Size operator +(Size size, Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the subtraction of the supplied value from the width and height of the size as a new size. The object itself is not modified! The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the size and the value as a new size</returns>
        public Size Subtract(double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the subtraction of the supplied value from the width and height of the size as a new size. The object itself is not modified! The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the size and the value as a new size</returns>
        [InlineCode("{size}.subtract({operand})")]
        static public Size operator -(Size size, double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the subtraction of the width and height of the supplied size from the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the two sizes as a new size</returns>
        public Size Subtract(Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the subtraction of the width and height of the supplied size from the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the subtraction of the two sizes as a new size</returns>
        [InlineCode("{size}.subtract({operand})")]
        static public Size operator -(Size size, Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the multiplication of the supplied value with the width and height of the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the size and the value as a new size</returns>
        public Size Multiply(double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the multiplication of the supplied value with the width and height of the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the size and the value as a new size</returns>
        [InlineCode("{size}.multiply({operand})")]
        static public Size operator *(Size size, double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the multiplication of the width and height of the supplied size with the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the two sizes as a new size</returns>
        public Size Multiply(Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the multiplication of the width and height of the supplied size with the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the multiplication of the two sizes as a new size</returns>
        [InlineCode("{size}.multiply({operand})")]
        static public Size operator *(Size size, Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the division of the supplied value by the width and height of the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the size and the value as a new size</returns>
        public Size Divide(double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the division of the supplied value by the width and height of the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the size and the value as a new size</returns>
        [InlineCode("{size}.divide({operand})")]
        static public Size operator /(Size size, double operand) { return default(Size); }
        
        /// <summary>
        /// Returns the division of the width and height of the supplied size by the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the two sizes as a new size</returns>
        public Size Divide(Size operand) { return default(Size); }
        
        /// <summary>
        /// Returns the division of the width and height of the supplied size by the size as a new size. The object itself is not modified!
        /// </summary>
        /// <returns>the division of the two sizes as a new size</returns>
        [InlineCode("{size}.divide({operand})")]
        static public Size operator /(Size size, Size operand) { return default(Size); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the size by the supplied value as a new size.
        /// </summary>
        /// <returns>the integer remainders of dividing the size by the value as a new size</returns>
        public Size Modulo(double operand) { return default(Size); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the size by the supplied value as a new size.
        /// </summary>
        /// <returns>the integer remainders of dividing the size by the value as a new size</returns>
        [InlineCode("{size}.modulo({operand})")]
        static public Size operator %(Size size, double operand) { return default(Size); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the size by the supplied size as a new size.
        /// </summary>
        /// <returns>the integer remainders of dividing the sizes by each other as a new size</returns>
        public Size Modulo(Size operand) { return default(Size); }
        
        /// <summary>
        /// The modulo operator returns the integer remainders of dividing the size by the supplied size as a new size.
        /// </summary>
        /// <returns>the integer remainders of dividing the sizes by each other as a new size</returns>
        [InlineCode("{size}.modulo({operand})")]
        static public Size operator %(Size size, Size operand) { return default(Size); }
        
        /// <summary>
        /// Checks whether the width and height of the size are equal to those of the supplied size.
        /// </summary>
        /// <returns></returns>
        public bool Equals(Size operand) { return default(bool); }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        public void Clone() { }
        
        /// <summary>
        /// Checks if this size has both the width and height set to 0.
        /// </summary>
        /// <returns>true both width and height are 0, false otherwise</returns>
        public bool IsZero() { return default(bool); }
        
        /// <summary>
        /// Checks if the width or the height of the size are NaN.
        /// </summary>
        /// <returns>true if the width or height of the size are NaN, false otherwise</returns>
        public bool IsNaN() { return default(bool); }
        
        /// <summary>
        /// Returns a new size with rounded width and height values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Size Round() { return default(Size); }
        
        /// <summary>
        /// Returns a new size with the nearest greater non-fractional values to the specified width and height values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Size Ceil() { return default(Size); }
        
        /// <summary>
        /// Returns a new size with the nearest smaller non-fractional values to the specified width and height values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Size Floor() { return default(Size); }
        
        /// <summary>
        /// Returns a new size with the absolute values of the specified width and height values. The object itself is not modified!
        /// </summary>
        /// <returns></returns>
        public Size Abs() { return default(Size); }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// Returns a new size object with the smallest width and height of the supplied sizes.
        /// </summary>
        /// <param name="size1"></param>
        /// <param name="size2"></param>
        /// <returns>The newly created size object</returns>
        public static Size Min(Size size1, Size size2) { return default(Size); }
        
        /// <summary>
        /// Returns a new size object with the largest width and height of the supplied sizes.
        /// </summary>
        /// <param name="size1"></param>
        /// <param name="size2"></param>
        /// <returns>The newly created size object</returns>
        public static Size Max(Size size1, Size size2) { return default(Size); }
        
        /// <summary>
        /// Returns a size object with random width and height values between 0 and 1.
        /// </summary>
        /// <returns>The newly created size object</returns>
        public static Size Random() { return default(Size); }
        
        #endregion
    }
}