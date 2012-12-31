using System;
using System.Runtime.CompilerServices;

namespace Script.PaperJs
{
    [NamedValues]
    public enum KeyEventType
    {
          [ScriptName("keydown")] KeyDown,
          [ScriptName("keyup")] KeyUp,
    }
}