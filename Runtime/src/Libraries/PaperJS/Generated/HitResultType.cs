using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [NamedValues]
    public enum HitResultType
    {
        
        /// <summary>
        /// Javascript value: 'segment'
        /// </summary>
        [ScriptName("segment")] Segment,
        
        /// <summary>
        /// Javascript value: 'handle-in'
        /// </summary>
        [ScriptName("handle-in")] HandleIn,
        
        /// <summary>
        /// Javascript value: 'handle-out'
        /// </summary>
        [ScriptName("handle-out")] HandleOut,
        
        /// <summary>
        /// Javascript value: 'stroke'
        /// </summary>
        [ScriptName("stroke")] Stroke,
        
        /// <summary>
        /// Javascript value: 'fill'
        /// </summary>
        [ScriptName("fill")] Fill,
        
        /// <summary>
        /// Javascript value: 'bounds'
        /// </summary>
        [ScriptName("bounds")] Bounds,
        
        /// <summary>
        /// Javascript value: 'center'
        /// </summary>
        [ScriptName("center")] Center,
    }
}