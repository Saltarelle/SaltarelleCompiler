using System.Html.Media.Graphics;
using System.Runtime.CompilerServices;

namespace PaperJs
{
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
}