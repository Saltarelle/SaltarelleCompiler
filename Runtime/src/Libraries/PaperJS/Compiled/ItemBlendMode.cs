using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum ItemBlendMode
    {
          [ScriptName("normal")] Normal,
          [ScriptName("multiply")] Multiply,
          [ScriptName("screen")] Screen,
          [ScriptName("overlay")] Overlay,
          [ScriptName("soft-light")] SoftLight,
          [ScriptName("hard-light")] HardLight,
          [ScriptName("color-dodge")] ColorDodge,
          [ScriptName("color-burn")] ColorBurn,
          [ScriptName("darken")] Darken,
          [ScriptName("lighten")] Lighten,
          [ScriptName("difference")] Difference,
          [ScriptName("exclusion")] Exclusion,
          [ScriptName("hue")] Hue,
          [ScriptName("saturation")] Saturation,
          [ScriptName("luminosity")] Luminosity,
          [ScriptName("color")] Color,
          [ScriptName("add")] Add,
          [ScriptName("subtract")] Subtract,
          [ScriptName("average")] Average,
          [ScriptName("pin-light")] PinLight,
          [ScriptName("negation")] Negation,
    }
}