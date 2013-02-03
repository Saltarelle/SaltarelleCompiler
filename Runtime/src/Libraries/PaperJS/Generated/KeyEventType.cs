using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [NamedValues]
    public enum KeyEventType
    {
        
        /// <summary>
        /// Javascript value: 'keydown'
        /// </summary>
        [ScriptName("keydown")] KeyDown,
        
        /// <summary>
        /// Javascript value: 'keyup'
        /// </summary>
        [ScriptName("keyup")] KeyUp,
    }
}