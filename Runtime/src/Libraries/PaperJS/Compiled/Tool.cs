using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    /// <summary>
    /// The Tool object refers to a script that the user can interact with by using the mouse and keyboard and can be accessed through the global tool variable. All its properties are also available in the paper scope. The global tool variable only exists in scripts that contain mouse handler functions (onMouseMove, onMouseDown, onMouseDrag, onMouseUp) or a keyboard handler function (onKeyDown, onKeyUp).  Example   var path; // Only execute onMouseDrag when the mouse // has moved at least 10 points: tool.distanceThreshold = 10; function onMouseDown(event) { // Create a new path every time the mouse is clicked path = new Path(); path.add(event.point); path.strokeColor = 'black'; } function onMouseDrag(event) { // Add a point to the path every time the mouse is dragged path.add(event.point); }
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class Tool
    {
        #region Properties
        
        
        /// <summary>
        /// The fixed time delay in milliseconds between each call to the onMouseDrag event. Setting this to an interval means the onMouseDrag event is called repeatedly after the initial onMouseDown until the user releases the mouse.
        /// </summary>
        public double EventInterval;
        
        /// <summary>
        /// The minimum distance the mouse has to drag before firing the onMouseDrag event, since the last onMouseDrag event.
        /// </summary>
        public double MinDistance;
        
        /// <summary>
        /// The maximum distance the mouse has to drag before firing the onMouseDrag event, since the last onMouseDrag event.
        /// </summary>
        public double MaxDistance;
        
        /// <summary>
        ///
        /// </summary>
        public double FixedDistance;
        
        /// <summary>
        /// The function to be called when the mouse button is pushed down. The function receives a ToolEvent object which contains information about the mouse event.
        /// </summary>
        public Action<ToolEvent> OnMouseDown;
        
        /// <summary>
        /// The function to be called when the mouse position changes while the mouse is being dragged. The function receives a ToolEvent object which contains information about the mouse event. This function can also be called periodically while the mouse doesn't move by setting the eventInterval
        /// </summary>
        public Action<ToolEvent> OnMouseDrag;
        
        /// <summary>
        /// The function to be called the mouse moves within the project view. The function receives a ToolEvent object which contains information about the mouse event.
        /// </summary>
        public Action<ToolEvent> OnMouseMove;
        
        /// <summary>
        /// The function to be called when the mouse button is released. The function receives a ToolEvent object which contains information about the mouse event.
        /// </summary>
        public Action<ToolEvent> OnMouseUp;
        
        /// <summary>
        /// The function to be called when the user presses a key on the keyboard. The function receives a KeyEvent object which contains information about the keyboard event. If the function returns false, the keyboard event will be prevented from bubbling up. This can be used for example to stop the window from scrolling, when you need the user to interact with arrow keys.
        /// </summary>
        public Action<KeyEvent> OnKeyDown;
        
        /// <summary>
        /// The function to be called when the user releases a key on the keyboard. The function receives a KeyEvent object which contains information about the keyboard event. If the function returns false, the keyboard event will be prevented from bubbling up. This can be used for example to stop the window from scrolling, when you need the user to interact with arrow keys.
        /// </summary>
        public Action<KeyEvent> OnKeyUp;
        
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
        
        #endregion
        
    }
}