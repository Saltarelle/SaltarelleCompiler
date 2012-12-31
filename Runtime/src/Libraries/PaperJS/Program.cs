using System;
using System.Collections.Generic;
using System.Html;
using System.Html.Media.Graphics;
using System.Runtime.CompilerServices;
using jQueryApi;

namespace PaperJs
{
    public partial class Color
    {
        /// <summary>
        /// Uses string instead of Color class. Color names (eg. blue, red, green, etc) and hex values (eg. #ff0000, etc) can be used.
        /// </summary>
        /// <param name="value">The string representation of the color.</param>
        /// <returns>An implicit color class. In JS it remain plain string.</returns>
        [InlineCode("{value}")]
        static public implicit operator Color(string value) { return null; }
    }

    /// <summary>
    /// Modifiers of a Tool Event
    /// </summary>
    [Imported, IgnoreNamespace]
    public class EventModifiers
    {
        /// <summary>
        /// True if the Shift key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Shift;

        /// <summary>
        /// True if the Control key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Control;

        /// <summary>
        /// True if the Option (Mac) key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Option;

        /// <summary>
        /// True if the Command (Mac) key was pressed when the event happened, false otherwise
        /// </summary>
        public bool Command;

        /// <summary>
        /// True if the CapsLock key was pressed when the event happened, false otherwise
        /// </summary>
        public bool CapsLock;
    }

    /// <summary>
    /// Base class for Paper.js events
    /// </summary>
    public class Event { }

    /// <summary>
    /// DOM representation of a HTML img element.
    /// </summary>
    public class HTMLImageElement { }

    /// <summary>
    /// DOM representation of a HTML canvas element.
    /// </summary>
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
        public CanvasContext2D GetContext2D() { return null; }
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
        /// <param name="canvasId">The id of the canvas this scope should be associated with.</param>
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
