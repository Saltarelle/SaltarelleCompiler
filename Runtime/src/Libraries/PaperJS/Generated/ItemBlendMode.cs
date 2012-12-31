using System;
using System.Runtime.CompilerServices;

namespace PaperJs
{
    /// <summary>
    ///
    /// </summary>
    [NamedValues]
    public enum ItemBlendMode
    {
        
        /// <summary>
        /// Javascript value: 'normal'
        /// </summary>
        [ScriptName("normal")] Normal,
        
        /// <summary>
        /// Javascript value: 'multiply'
        /// </summary>
        [ScriptName("multiply")] Multiply,
        
        /// <summary>
        /// Javascript value: 'screen'
        /// </summary>
        [ScriptName("screen")] Screen,
        
        /// <summary>
        /// Javascript value: 'overlay'
        /// </summary>
        [ScriptName("overlay")] Overlay,
        
        /// <summary>
        /// Javascript value: 'soft-light'
        /// </summary>
        [ScriptName("soft-light")] SoftLight,
        
        /// <summary>
        /// Javascript value: 'hard-light'
        /// </summary>
        [ScriptName("hard-light")] HardLight,
        
        /// <summary>
        /// Javascript value: 'color-dodge'
        /// </summary>
        [ScriptName("color-dodge")] ColorDodge,
        
        /// <summary>
        /// Javascript value: 'color-burn'
        /// </summary>
        [ScriptName("color-burn")] ColorBurn,
        
        /// <summary>
        /// Javascript value: 'darken'
        /// </summary>
        [ScriptName("darken")] Darken,
        
        /// <summary>
        /// Javascript value: 'lighten'
        /// </summary>
        [ScriptName("lighten")] Lighten,
        
        /// <summary>
        /// Javascript value: 'difference'
        /// </summary>
        [ScriptName("difference")] Difference,
        
        /// <summary>
        /// Javascript value: 'exclusion'
        /// </summary>
        [ScriptName("exclusion")] Exclusion,
        
        /// <summary>
        /// Javascript value: 'hue'
        /// </summary>
        [ScriptName("hue")] Hue,
        
        /// <summary>
        /// Javascript value: 'saturation'
        /// </summary>
        [ScriptName("saturation")] Saturation,
        
        /// <summary>
        /// Javascript value: 'luminosity'
        /// </summary>
        [ScriptName("luminosity")] Luminosity,
        
        /// <summary>
        /// Javascript value: 'color'
        /// </summary>
        [ScriptName("color")] Color,
        
        /// <summary>
        /// Javascript value: 'add'
        /// </summary>
        [ScriptName("add")] Add,
        
        /// <summary>
        /// Javascript value: 'subtract'
        /// </summary>
        [ScriptName("subtract")] Subtract,
        
        /// <summary>
        /// Javascript value: 'average'
        /// </summary>
        [ScriptName("average")] Average,
        
        /// <summary>
        /// Javascript value: 'pin-light'
        /// </summary>
        [ScriptName("pin-light")] PinLight,
        
        /// <summary>
        /// Javascript value: 'negation'
        /// </summary>
        [ScriptName("negation")] Negation,
    }
}