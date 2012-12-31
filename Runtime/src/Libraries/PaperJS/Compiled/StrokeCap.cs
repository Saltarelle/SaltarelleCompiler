using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum StrokeCap
    {
          [ScriptName("round")] Round,
          [ScriptName("square")] Square,
          [ScriptName("butt")] Butt,
    }
}