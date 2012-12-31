using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The Raster item represents an image in a Paper.js project.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Raster : PlacedItem
    {
        #region Properties
        
        
        /// <summary>
        /// The size of the raster in pixels.
        /// </summary>
        public Size Size;
        
        /// <summary>
        /// The width of the raster in pixels.
        /// </summary>
        public double Width;
        
        /// <summary>
        /// The height of the raster in pixels.
        /// </summary>
        public double Height;
        
        /// <summary>
        /// Pixels per inch of the raster at its current size.
        /// </summary>
        public Size Ppi;
        
        /// <summary>
        /// The Canvas 2d drawing context of the raster.
        /// </summary>
        public Context Context;
        
        /// <summary>
        /// The HTMLImageElement or Canvas of the raster.
        /// </summary>
        public HTMLImageElement Image;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new raster item and places it in the active layer.
        /// </summary>
        /// <param name="_object">optional</param>
        [ScriptName("")]
        public Raster(HTMLImageElement _object){ }
        
        /// <summary>
        /// Creates a new raster item and places it in the active layer.
        /// </summary>
        [ScriptName("")]
        public Raster(){ }
        
        /// <summary>
        /// Creates a new raster item and places it in the active layer.
        /// </summary>
        /// <param name="_object">optional</param>
        [ScriptName("")]
        public Raster(Canvas _object){ }
        
        /// <summary>
        /// Creates a new raster item and places it in the active layer.
        /// </summary>
        /// <param name="_object">optional</param>
        [ScriptName("")]
        public Raster(string _object){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="rect">the boundaries of the sub image in pixel coordinates</param>
        /// <returns></returns>
        public Canvas GetSubImage(Rectangle rect) { return default(Canvas); }
        
        /// <summary>
        /// Draws an image on the raster.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="point">the offset of the image as a point in pixel coordinates</param>
        public void DrawImage(HTMLImageElement image, Point point) { }
        
        /// <summary>
        /// Draws an image on the raster.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="point">the offset of the image as a point in pixel coordinates</param>
        public void DrawImage(Canvas image, Point point) { }
        
        /// <summary>
        /// Calculates the average color of the image within the given path, rectangle or point. This can be used for creating raster image effects.
        /// </summary>
        /// <param name="_object"></param>
        /// <returns>the average color contained in the area covered by the specified path, rectangle or point.</returns>
        public Color GetAverageColor(Path _object) { return default(RgbColor); }
        
        /// <summary>
        /// Calculates the average color of the image within the given path, rectangle or point. This can be used for creating raster image effects.
        /// </summary>
        /// <param name="_object"></param>
        /// <returns>the average color contained in the area covered by the specified path, rectangle or point.</returns>
        public Color GetAverageColor(Rectangle _object) { return default(RgbColor); }
        
        /// <summary>
        /// Calculates the average color of the image within the given path, rectangle or point. This can be used for creating raster image effects.
        /// </summary>
        /// <param name="_object"></param>
        /// <returns>the average color contained in the area covered by the specified path, rectangle or point.</returns>
        public Color GetAverageColor(Point _object) { return default(RgbColor); }
        
        /// <summary>
        /// Gets the color of a pixel in the raster.
        /// </summary>
        /// <param name="x">the x offset of the pixel in pixel coordinates</param>
        /// <param name="y">the y offset of the pixel in pixel coordinates</param>
        /// <returns>the color of the pixel</returns>
        public Color GetPixel(double x, double y) { return default(RgbColor); }
        
        /// <summary>
        /// Gets the color of a pixel in the raster.
        /// </summary>
        /// <param name="point">the offset of the pixel as a point in pixel coordinates</param>
        /// <returns>the color of the pixel</returns>
        public Color GetPixel(Point point) { return default(RgbColor); }
        
        /// <summary>
        /// Sets the color of the specified pixel to the specified color.
        /// </summary>
        /// <param name="x">the x offset of the pixel in pixel coordinates</param>
        /// <param name="y">the y offset of the pixel in pixel coordinates</param>
        /// <param name="color">the color that the pixel will be set to</param>
        public void SetPixel(double x, double y, Color color) { }
        
        /// <summary>
        /// Sets the color of the specified pixel to the specified color.
        /// </summary>
        /// <param name="point">the offset of the pixel as a point in pixel coordinates</param>
        /// <param name="color">the color that the pixel will be set to</param>
        public void SetPixel(Point point, Color color) { }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public ImageData CreateData(Size size) { return default(ImageData); }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public ImageData GetData(Rectangle rect) { return default(ImageData); }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public ImageData SetData(ImageData data, Point point) { return default(ImageData); }
        
        #endregion
        
    }
}