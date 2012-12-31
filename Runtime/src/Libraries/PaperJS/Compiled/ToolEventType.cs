using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum ToolEventType
    {
          [ScriptName("mousedown")] MouseDown,
          [ScriptName("mouseup")] MouseUp,
          [ScriptName("mousemove")] MouseMove,
          [ScriptName("mousedrag")] MouseDrag,
    }
}