using System;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;
using jQueryApi;

namespace Script.PaperJs
{
    public partial class Color
    {
        [InlineCode("{value}")]
        static public implicit operator Color(string value) { return null; }
    }

    [Imported, IgnoreNamespace]
    public class EventModifiers
    {
        public bool Shift;
        public bool Control;
        public bool Option;
        public bool Command;
        public bool CapsLock;
    }

    public class Event { }
    public class Context { }
    public class Canvas { }
    public class CanvasRenderingContext2D { }
    public class ImageData
    {
        public ulong Width { get { return 0; } }
        public ulong Height { get { return 0; } }
        public CanvasPixelArray Data { get { return null; } }
    }
    public class CanvasPixelArray
    {
        public ulong Length { get { return 0; } }
        public byte this[ulong i] { get { return 0; } set { } }
    }
    public class HTMLImageElement { }
    public class HTMLCanvasElement
    {
        /// <summary>
        /// Reflects the height HTML attribute, specifying the height of the coordinate space in CSS pixels.
        /// </summary>
        public ulong Height { get { return 0; } set { } }

        /// <summary>
        /// Reflects the width HTML attribute, specifying the width of the coordinate space in CSS pixels.
        /// </summary>
        public ulong Width { get { return 0; } set { } }

        /// <summary>
        /// Returns a drawing context on the canvas. A drawing context lets you draw on the canvas. Calling getContext returns a CanvasRenderingContext2D Object.
        /// </summary>
        /// <returns>A drawing context on the canvas</returns>
        [InlineCode("getContext('2d')")]
        public CanvasRenderingContext2D GetContext2D() { return null; }
    }

    public partial class Point
    {
        /// <summary>
        /// Rotates the point by the given angle. The object itself is not modified. Read more about angle units and orientation in the description of the angle property.
        /// </summary>
        /// <param name="angle">the rotation angle</param>
        /// <returns>the rotated point</returns>
        public Point Rotate(double angle) { return default(Point); }        
    }

    public partial class PaperScope
    {
        /// <summary>
        /// Sets up an empty project for us. If a canvas is provided, it also creates a View for it, both linked to this scope.
        /// </summary>
        /// <param name="canvas">The canvas this scope should be associated with.</param>
        public void Setup(string canvasId) { }        
    }

    public partial class Path
    {
        /// <summary>
        /// Creates a new Path item and places it at the top of the active layer.
        /// </summary>
        /// <param name="points">An array of points to be converted to segments that will be added to the path</param>
        [ScriptName("")]
        public Path(Point[] points) { }
    }
}
