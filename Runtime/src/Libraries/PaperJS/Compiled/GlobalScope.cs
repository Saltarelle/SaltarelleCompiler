using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [Imported, IgnoreNamespace]
    public partial class GlobalScope
    {
        #region Properties
        
        
        /// <summary>
        /// A reference to the currently active PaperScope object.
        /// </summary>
        public PaperScope Paper;
        
        /// <summary>
        /// The currently active project.
        /// </summary>
        public Project Project;
        
        /// <summary>
        /// The list of all open projects within the current Paper.js context.
        /// </summary>
        public Project[] Projects;
        
        /// <summary>
        /// The active view of the active project.
        /// </summary>
        public View View;
        
        /// <summary>
        /// The list of view of the active project.
        /// </summary>
        public View[] Views;
        
        /// <summary>
        /// The reference to the active tool.
        /// </summary>
        public Tool Tool;
        
        /// <summary>
        /// The list of available tools.
        /// </summary>
        public Tool[] Tools;
        
        /// <summary>
        /// A reference to the view.onFrame handler function.
        /// </summary>
        public Action<Event> OnFrame;
        
        /// <summary>
        /// A reference to the view.onResize handler function.
        /// </summary>
        public Action<Event> OnResize;
        
        /// <summary>
        /// A reference to the tool.onMouseDown handler function.
        /// </summary>
        public Action<Event> OnMouseDown;
        
        /// <summary>
        /// A reference to the tool.onMouseDrag handler function.
        /// </summary>
        public Action<Event> OnMouseDrag;
        
        /// <summary>
        /// A reference to the tool.onMouseMove handler function.
        /// </summary>
        public Action<Event> OnMouseMove;
        
        /// <summary>
        /// A reference to the tool.onMouseUp handler function.
        /// </summary>
        public Action<Event> OnMouseUp;
        
        /// <summary>
        /// A reference to the tool.onKeyDown handler function.
        /// </summary>
        public Action<Event> OnKeyDown;
        
        /// <summary>
        /// A reference to the tool.onKeyUp handler function.
        /// </summary>
        public Action<Event> OnKeyUp;
        
        #endregion
    }
}