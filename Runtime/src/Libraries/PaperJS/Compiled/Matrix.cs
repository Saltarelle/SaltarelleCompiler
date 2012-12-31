using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// An affine transform performs a linear mapping from 2D coordinates to other 2D coordinates that preserves the "straightness" and "parallelness" of lines. Such a coordinate transformation can be represented by a 3 row by 3 column matrix with an implied last row of [ 0 0 1 ]. This matrix transforms source coordinates (x,y) into destination coordinates (x',y') by considering them to be a column vector and multiplying the coordinate vector by the matrix according to the following process:  [ x ]  [ a b tx ] [ x ]  [ a * x + b * y + tx ] [ y ] = [ c d ty ] [ y ] = [ c * x + d * y + ty ] [ 1 ]  [ 0 0 1 ] [ 1 ]  [     1     ]  This class is optimized for speed and minimizes calculations based on its knowledge of the underlying matrix (as opposed to say simply performing matrix multiplication).
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Matrix
    {
        #region Properties
        
        
        /// <summary>
        /// The scaling factor in the x-direction (a).
        /// </summary>
        public double ScaleX;
        
        /// <summary>
        /// The scaling factor in the y-direction (d).
        /// </summary>
        public double ScaleY;
        
        /// <summary>
        ///
        /// </summary>
        public double ShearX;
        
        /// <summary>
        ///
        /// </summary>
        public double ShearY;
        
        /// <summary>
        /// The translation in the x-direction (tx).
        /// </summary>
        public double TranslateX;
        
        /// <summary>
        /// The translation in the y-direction (ty).
        /// </summary>
        public double TranslateY;
        
        /// <summary>
        /// The transform values as an array, in the same sequence as they are passed to {@link #initialize(a, c, b, d, tx, ty)}.
        /// </summary>
        public double Values;
        
        /// <summary>
        /// The rotation angle of the matrix. If a non-uniform rotation is applied as a result of a shear() or scale() command, undefined is returned, as the resulting transformation cannot be expressed in one rotation angle.
        /// </summary>
        public double Rotation;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected Matrix(){ }
        
        /// <summary>
        /// Creates a 2D affine transform.
        /// </summary>
        /// <param name="a">The scaleX coordinate of the transform</param>
        /// <param name="c">The shearY coordinate of the transform</param>
        /// <param name="b">The shearX coordinate of the transform</param>
        /// <param name="d">The scaleY coordinate of the transform</param>
        /// <param name="tx">The translateX coordinate of the transform</param>
        /// <param name="ty">The translateY coordinate of the transform</param>
        [ScriptName("")]
        public Matrix(double a, double c, double b, double d, double tx, double ty){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>A copy of this transform.</returns>
        public Matrix Clone() { return default(Matrix); }
        
        /// <summary>
        /// Sets this transform to the matrix specified by the 6 values.
        /// </summary>
        /// <param name="a">The scaleX coordinate of the transform</param>
        /// <param name="c">The shearY coordinate of the transform</param>
        /// <param name="b">The shearX coordinate of the transform</param>
        /// <param name="d">The scaleY coordinate of the transform</param>
        /// <param name="tx">The translateX coordinate of the transform</param>
        /// <param name="ty">The translateY coordinate of the transform</param>
        /// <returns>This affine transform</returns>
        public Matrix Set(double a, double c, double b, double d, double tx, double ty) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a scaling transformation.
        /// </summary>
        /// <param name="scale">The scaling factor</param>
        /// <param name="center">The center for the scaling transformation - optional</param>
        /// <returns>This affine transform</returns>
        public Matrix Scale(double scale, Point center) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a scaling transformation.
        /// </summary>
        /// <param name="scale">The scaling factor</param>
        /// <returns>This affine transform</returns>
        public Matrix Scale(double scale) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a scaling transformation.
        /// </summary>
        /// <param name="hor">The horizontal scaling factor</param>
        /// <param name="ver">The vertical scaling factor</param>
        /// <param name="center">The center for the scaling transformation - optional</param>
        /// <returns>This affine transform</returns>
        public Matrix Scale(double hor, double ver, Point center) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a scaling transformation.
        /// </summary>
        /// <param name="hor">The horizontal scaling factor</param>
        /// <param name="ver">The vertical scaling factor</param>
        /// <returns>This affine transform</returns>
        public Matrix Scale(double hor, double ver) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a translate transformation.
        /// </summary>
        /// <param name="point">The vector to translate by</param>
        /// <returns>This affine transform</returns>
        public Matrix Translate(Point point) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a translate transformation.
        /// </summary>
        /// <param name="dx">The distance to translate in the x direction</param>
        /// <param name="dy">The distance to translate in the y direction</param>
        /// <returns>This affine transform</returns>
        public Matrix Translate(double dx, double dy) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a rotation transformation around an anchor point.
        /// </summary>
        /// <param name="angle">The angle of rotation measured in degrees</param>
        /// <param name="center">The anchor point to rotate around</param>
        /// <returns>This affine transform</returns>
        public Matrix Rotate(double angle, Point center) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a rotation transformation around an anchor point.
        /// </summary>
        /// <param name="angle">The angle of rotation measured in degrees</param>
        /// <param name="x">The x coordinate of the anchor point</param>
        /// <param name="y">The y coordinate of the anchor point</param>
        /// <returns>This affine transform</returns>
        public Matrix Rotate(double angle, double x, double y) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a shear transformation.
        /// </summary>
        /// <param name="point">The shear factor in x and y direction</param>
        /// <param name="center">The center for the shear transformation - optional</param>
        /// <returns>This affine transform</returns>
        public Matrix Shear(Point point, Point center) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a shear transformation.
        /// </summary>
        /// <param name="point">The shear factor in x and y direction</param>
        /// <returns>This affine transform</returns>
        public Matrix Shear(Point point) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a shear transformation.
        /// </summary>
        /// <param name="hor">The horizontal shear factor</param>
        /// <param name="ver">The vertical shear factor</param>
        /// <param name="center">The center for the shear transformation - optional</param>
        /// <returns>This affine transform</returns>
        public Matrix Shear(double hor, double ver, Point center) { return default(Matrix); }
        
        /// <summary>
        /// Concatentates this transform with a shear transformation.
        /// </summary>
        /// <param name="hor">The horizontal shear factor</param>
        /// <param name="ver">The vertical shear factor</param>
        /// <returns>This affine transform</returns>
        public Matrix Shear(double hor, double ver) { return default(Matrix); }
        
        /// <summary>
        /// Concatenates an affine transform to this transform.
        /// </summary>
        /// <param name="mx">The transform to concatenate</param>
        /// <returns>This affine transform</returns>
        public Matrix Concatenate(Matrix mx) { return default(Matrix); }
        
        /// <summary>
        /// Pre-concatenates an affine transform to this transform.
        /// </summary>
        /// <param name="mx">The transform to preconcatenate</param>
        /// <returns>This affine transform</returns>
        public Matrix PreConcatenate(Matrix mx) { return default(Matrix); }
        
        /// <summary>
        /// Transforms a point and returns the result.
        /// </summary>
        /// <param name="point">The point to be transformed</param>
        /// <returns>The transformed point</returns>
        public Point Transform(Point point) { return default(Point); }
        
        /// <summary>
        /// Transforms an array of coordinates by this matrix and stores the results into the destination array, which is also returned.
        /// </summary>
        /// <param name="src">The array containing the source points as x, y value pairs</param>
        /// <param name="srcOff">The offset to the first point to be transformed</param>
        /// <param name="dst">The array into which to store the transformed point pairs</param>
        /// <param name="dstOff">The offset of the location of the first transformed point in the destination array</param>
        /// <param name="numPts">The number of points to tranform</param>
        /// <returns>The dst array, containing the transformed coordinates.</returns>
        public double Transform(double src, double srcOff, double dst, double dstOff, int numPts) { return default(double); }
        
        /// <summary>
        /// Inverse transforms a point and returns the result.
        /// </summary>
        /// <param name="point">The point to be transformed</param>
        public void InverseTransform(Point point) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>Whether this transform is the identity transform</returns>
        public bool IsIdentity() { return default(bool); }
        
        /// <summary>
        /// Returns whether the transform is invertible. A transform is not invertible if the determinant is 0 or any value is non-finite or NaN.
        /// </summary>
        /// <returns>Whether the transform is invertible</returns>
        public bool IsInvertible() { return default(bool); }
        
        /// <summary>
        /// Checks whether the matrix is singular or not. Singular matrices cannot be inverted.
        /// </summary>
        /// <returns>Whether the matrix is singular</returns>
        public bool IsSingular() { return default(bool); }
        
        /// <summary>
        /// Inverts the transformation of the matrix. If the matrix is not invertible (in which case isSingular() returns true), null  is returned.
        /// </summary>
        /// <returns>The inverted matrix, or null , if the matrix is singular</returns>
        public Matrix CreateInverse() { return default(Matrix); }
        
        /// <summary>
        /// Sets this transform to a scaling transformation.
        /// </summary>
        /// <param name="hor">The horizontal scaling factor</param>
        /// <param name="ver">The vertical scaling factor</param>
        /// <returns>This affine transform</returns>
        public Matrix SetToScale(double hor, double ver) { return default(Matrix); }
        
        /// <summary>
        /// Sets this transform to a translation transformation.
        /// </summary>
        /// <param name="dx">The distance to translate in the x direction</param>
        /// <param name="dy">The distance to translate in the y direction</param>
        /// <returns>This affine transform</returns>
        public Matrix SetToTranslation(double dx, double dy) { return default(Matrix); }
        
        /// <summary>
        /// Sets this transform to a shearing transformation.
        /// </summary>
        /// <param name="hor">The horizontal shear factor</param>
        /// <param name="ver">The vertical shear factor</param>
        /// <returns>This affine transform</returns>
        public Matrix SetToShear(double hor, double ver) { return default(Matrix); }
        
        /// <summary>
        /// Sets this transform to a rotation transformation.
        /// </summary>
        /// <param name="angle">The angle of rotation measured in degrees</param>
        /// <param name="x">The x coordinate of the anchor point</param>
        /// <param name="y">The y coordinate of the anchor point</param>
        /// <returns>This affine transform</returns>
        public Matrix SetToRotation(double angle, double x, double y) { return default(Matrix); }
        
        /// <summary>
        /// Applies this matrix to the specified HTMLCanvasElement Context.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="reset">optional, default: false</param>
        public void ApplyToContext(System.Html.Media.Graphics.CanvasContext2D ctx, bool reset) { }
        
        /// <summary>
        /// Applies this matrix to the specified HTMLCanvasElement Context.
        /// </summary>
        /// <param name="ctx"></param>
        public void ApplyToContext(System.Html.Media.Graphics.CanvasContext2D ctx) { }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// Creates a transform representing a scaling transformation.
        /// </summary>
        /// <param name="hor">The horizontal scaling factor</param>
        /// <param name="ver">The vertical scaling factor</param>
        /// <returns>A transform representing a scaling transformation</returns>
        public static Matrix GetScaleInstance(double hor, double ver) { return default(Matrix); }
        
        /// <summary>
        /// Creates a transform representing a translation transformation.
        /// </summary>
        /// <param name="dx">The distance to translate in the x direction</param>
        /// <param name="dy">The distance to translate in the y direction</param>
        /// <returns>A transform representing a translation transformation</returns>
        public static Matrix GetTranslateInstance(double dx, double dy) { return default(Matrix); }
        
        /// <summary>
        /// Creates a transform representing a shearing transformation.
        /// </summary>
        /// <param name="hor">The horizontal shear factor</param>
        /// <param name="ver">The vertical shear factor</param>
        /// <param name="center"></param>
        /// <returns>A transform representing a shearing transformation</returns>
        public static Matrix GetShearInstance(double hor, double ver, Point center) { return default(Matrix); }
        
        /// <summary>
        /// Creates a transform representing a rotation transformation.
        /// </summary>
        /// <param name="angle">The angle of rotation measured in degrees</param>
        /// <param name="x">The x coordinate of the anchor point</param>
        /// <param name="y">The y coordinate of the anchor point</param>
        /// <returns>A transform representing a rotation transformation</returns>
        public static Matrix GetRotateInstance(double angle, double x, double y) { return default(Matrix); }
        
        #endregion
    }
}