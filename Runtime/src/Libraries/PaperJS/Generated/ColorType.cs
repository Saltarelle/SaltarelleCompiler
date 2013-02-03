using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [NamedValues]
    public enum ColorType
    {
        
        /// <summary>
        /// Javascript value: 'rgb'
        /// </summary>
        [ScriptName("rgb")] Rgb,
        
        /// <summary>
        /// Javascript value: 'hsb'
        /// </summary>
        [ScriptName("hsb")] Hsb,
        
        /// <summary>
        /// Javascript value: 'gray'
        /// </summary>
        [ScriptName("gray")] Gray,
    }
}