using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum HitResultName
    {
          [ScriptName("top-left")] TopLeft,
          [ScriptName("top-right")] TopRight,
          [ScriptName("bottom-left")] BottomLeft,
          [ScriptName("bottom-right")] BottomRight,
          [ScriptName("left-center")] LeftCenter,
          [ScriptName("top-center")] TopCenter,
          [ScriptName("right-center")] RightCenter,
          [ScriptName("bottom-center")] BottomCenter,
    }
}