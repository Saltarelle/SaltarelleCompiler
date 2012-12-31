using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum ParagraphStyleJustification
    {
          [ScriptName("left")] Left,
          [ScriptName("right")] Right,
          [ScriptName("center")] Center,
    }
}