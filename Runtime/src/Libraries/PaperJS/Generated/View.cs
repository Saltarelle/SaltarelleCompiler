using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// The View object wraps a canvas element and handles drawing and user interaction through mouse and keyboard for it. It offer means to scroll the view, find the currently visible bounds in project coordinates, or the center, both useful for constructing artwork that should appear centered on screen.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class View
    {
        #region Properties
        
        
        /// <summary>
        /// The underlying native canvas element.
        /// </summary>
        public CanvasElement Canvas;
        
        /// <summary>
        /// The size of the view canvas. Changing the view's size will resize it's underlying canvas.
        /// </summary>
        public Size ViewSize;
        
        /// <summary>
        /// The bounds of the currently visible area in project coordinates.
        /// </summary>
        public Rectangle Bounds;
        
        /// <summary>
        /// The size of the visible area in project coordinates.
        /// </summary>
        public Size Size;
        
        /// <summary>
        /// The center of the visible area in project coordinates.
        /// </summary>
        public Point Center;
        
        /// <summary>
        /// The zoom factor by which the project coordinates are magnified.
        /// </summary>
        public double Zoom;
        
        /// <summary>
        /// Handler function to be called on each frame of an animation. The function receives an event object which contains information about the frame event: event.count: the number of times the frame event was fired. event.time: the total amount of time passed since the first frame event in seconds. event.delta: the time passed in seconds since the last frame event.
        /// </summary>
        public Action<Event> OnFrame;
        
        /// <summary>
        /// Handler function that is called whenever a view is resized.
        /// </summary>
        public Action<Event> OnResize;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// Constructor for enable inheritance
        /// </summary>
        protected View(){ }
        
        /// <summary>
        /// Creates a view object
        /// </summary>
        /// <param name="canvas">The canvas object that this view should wrap, or the String id that represents it</param>
        [ScriptName("")]
        public View(CanvasElement canvas){ }
        
        /// <summary>
        /// Creates a view object
        /// </summary>
        /// <param name="canvas">The canvas object that this view should wrap, or the String id that represents it</param>
        [ScriptName("")]
        public View(string canvas){ }

        #endregion
        
        #region Methods
        
        /// <summary>
        ///
        /// </summary>
        public void Activate() { }
        
        /// <summary>
        ///
        /// </summary>
        public void Remove() { }
        
        /// <summary>
        /// Checks whether the view is currently visible within the current browser viewport.
        /// </summary>
        /// <returns>Whether the view is visible.</returns>
        public bool IsVisible() { return default(bool); }
        
        /// <summary>
        /// Scrolls the view by the given vector.
        /// </summary>
        /// <param name="point"></param>
        public void ScrollBy(Point point) { }
        
        /// <summary>
        ///
        /// </summary>
        public void Draw() { }
        
        #endregion
        
    }
}