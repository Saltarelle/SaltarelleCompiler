using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum HitResultType
    {
          [ScriptName("segment")] Segment,
          [ScriptName("handle-in")] HandleIn,
          [ScriptName("handle-out")] HandleOut,
          [ScriptName("stroke")] Stroke,
          [ScriptName("fill")] Fill,
          [ScriptName("bounds")] Bounds,
          [ScriptName("center")] Center,
    }
}