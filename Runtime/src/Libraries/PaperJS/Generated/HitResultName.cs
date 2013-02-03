using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [NamedValues]
    public enum HitResultName
    {
        
        /// <summary>
        /// Javascript value: 'top-left'
        /// </summary>
        [ScriptName("top-left")] TopLeft,
        
        /// <summary>
        /// Javascript value: 'top-right'
        /// </summary>
        [ScriptName("top-right")] TopRight,
        
        /// <summary>
        /// Javascript value: 'bottom-left'
        /// </summary>
        [ScriptName("bottom-left")] BottomLeft,
        
        /// <summary>
        /// Javascript value: 'bottom-right'
        /// </summary>
        [ScriptName("bottom-right")] BottomRight,
        
        /// <summary>
        /// Javascript value: 'left-center'
        /// </summary>
        [ScriptName("left-center")] LeftCenter,
        
        /// <summary>
        /// Javascript value: 'top-center'
        /// </summary>
        [ScriptName("top-center")] TopCenter,
        
        /// <summary>
        /// Javascript value: 'right-center'
        /// </summary>
        [ScriptName("right-center")] RightCenter,
        
        /// <summary>
        /// Javascript value: 'bottom-center'
        /// </summary>
        [ScriptName("bottom-center")] BottomCenter,
    }
}