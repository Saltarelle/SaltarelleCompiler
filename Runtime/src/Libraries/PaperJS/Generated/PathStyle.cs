using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// PathStyle is used for changing the visual styles of items contained within a Paper.js project and is returned by item.style and project.currentStyle. All properties of PathStyle are also reflected directly in Item, i.e.: item.fillColor. To set multiple style properties in one go, you can pass an object to item.style. This is a convenient way to define a style once and apply it to a series of items:  Example   Run
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class PathStyle
    {
        #region Properties
        
        
        /// <summary>
        /// The color of the stroke.
        /// </summary>
        public Color StrokeColor;
        
        /// <summary>
        /// The width of the stroke.
        /// </summary>
        public double StrokeWidth;
        
        /// <summary>
        /// The shape to be used at the end of open Path items, when they have a stroke.
        /// </summary>
        public StrokeCap StrokeCap;
        
        /// <summary>
        /// The shape to be used at the corners of paths when they have a stroke.
        /// </summary>
        public string StrokeJoin;
        
        /// <summary>
        /// The dash offset of the stroke.
        /// </summary>
        public double DashOffset;
        
        /// <summary>
        /// Specifies an array containing the dash and gap lengths of the stroke.
        /// </summary>
        public Array DashArray;
        
        /// <summary>
        /// The miter limit of the stroke. When two line segments meet at a sharp angle and miter joins have been specified for strokeJoin, it is possible for the miter to extend far beyond the strokeWidth of the path. The miterLimit imposes a limit on the ratio of the miter length to the strokeWidth.
        /// </summary>
        public double MiterLimit;
        
        /// <summary>
        /// The fill color.
        /// </summary>
        public Color FillColor;
        
        #endregion
    }
}