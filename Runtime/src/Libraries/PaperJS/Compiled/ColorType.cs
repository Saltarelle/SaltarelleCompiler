using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum ColorType
    {
          [ScriptName("rgb")] Rgb,
          [ScriptName("hsb")] Hsb,
          [ScriptName("gray")] Gray,
    }
}