using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [NamedValues]
    public enum ToolEventType
    {
        
        /// <summary>
        /// Javascript value: 'mousedown'
        /// </summary>
        [ScriptName("mousedown")] MouseDown,
        
        /// <summary>
        /// Javascript value: 'mouseup'
        /// </summary>
        [ScriptName("mouseup")] MouseUp,
        
        /// <summary>
        /// Javascript value: 'mousemove'
        /// </summary>
        [ScriptName("mousemove")] MouseMove,
        
        /// <summary>
        /// Javascript value: 'mousedrag'
        /// </summary>
        [ScriptName("mousedrag")] MouseDrag,
    }
}