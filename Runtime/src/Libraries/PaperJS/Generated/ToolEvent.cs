using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    /// ToolEvent The ToolEvent object is received by the Tool's mouse event handlers tool.onMouseDown, tool.onMouseDrag, tool.onMouseMove and tool.onMouseUp. The ToolEvent object is the only parameter passed to these functions and contains information about the mouse event.
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class ToolEvent
    {
        #region Properties
        
        
        /// <summary>
        /// The type of tool event.
        /// </summary>
        public ToolEventType Type;
        
        /// <summary>
        /// The position of the mouse in project coordinates when the event was fired.
        /// </summary>
        public Point Point;
        
        /// <summary>
        /// The position of the mouse in project coordinates when the previous event was fired.
        /// </summary>
        public Point LastPoint;
        
        /// <summary>
        /// The position of the mouse in project coordinates when the mouse button was last clicked.
        /// </summary>
        public Point DownPoint;
        
        /// <summary>
        /// The point in the middle between lastPoint and point. This is a useful position to use when creating artwork based on the moving direction of the mouse, as returned by delta.
        /// </summary>
        public Point MiddlePoint;
        
        /// <summary>
        /// The difference between the current position and the last position of the mouse when the event was fired. In case of the mouseup event, the difference to the mousedown position is returned.
        /// </summary>
        public Point Delta;
        
        /// <summary>
        /// The number of times the mouse event was fired.
        /// </summary>
        public double Count;
        
        /// <summary>
        /// The item at the position of the mouse (if any). If the item is contained within one or more Group or CompoundPath items, the most top level group or compound path that it is contained within is returned.
        /// </summary>
        public Item Item;
        
        /// <summary>
        ///
        /// </summary>
        public EventModifiers Modifiers;
        
        #endregion
        
        #region Methods
        
        
        
        #endregion
        
    }
}