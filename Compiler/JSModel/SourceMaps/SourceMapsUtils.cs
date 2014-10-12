using System;
using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler.JSModel.SourceMaps {   
   public class Map<K,V> : Dictionary<K,V> {
      public V PutIfAbsent(K value, Func<V> func) {
         if (!this.ContainsKey(value)) {
            this[value] = func();
         }
         return this[value];
      }
   } 

   public static class ListExtensions {
      public static bool IsEmpty<T>(this List<T> list) {
         return list.Count == 0;
      } 

      public static T Last<T>(this List<T> list) {
         return list[list.Count - 1]; 
      }
   }

   public static class StringBuilderExtensions {
      public static void WriteJsonEscapedCharsOn(this StringBuilder sb, string s) {
         sb.Append(s);  // TODO: is writeJsonEscapedCharsOn necessary ?
      }
   }    
}

/*
/// Writes the characters of [string] on [buffer].  The characters
/// are escaped as suitable for JavaScript and JSON.  [buffer] is
/// anything which supports [:write:] and [:writeCharCode:], for example,
/// [StringBuffer].  Note that JS supports \xnn and \unnnn whereas JSON only
/// supports the \unnnn notation.  Therefore we use the \unnnn notation.
void writeJsonEscapedCharsOn(String string, buffer) {
  void addCodeUnitEscaped(var buffer, int code) {
    assert(code < 0x10000);
    buffer.write(r'\u');
    if (code < 0x1000) {
      buffer.write('0');
      if (code < 0x100) {
        buffer.write('0');
        if (code < 0x10) {
          buffer.write('0');
        }
      }
    }
    buffer.write(code.toRadixString(16));
  }

  void writeEscapedOn(String string, var buffer) {
    for (int i = 0; i < string.length; i++) {
      int code = string.codeUnitAt(i);
      if (code == $DQ) {
        buffer.write(r'\"');
      } else if (code == $TAB) {
        buffer.write(r'\t');
      } else if (code == $LF) {
        buffer.write(r'\n');
      } else if (code == $CR) {
        buffer.write(r'\r');
      } else if (code == $DEL) {
        addCodeUnitEscaped(buffer, $DEL);
      } else if (code == $LS) {
        // This Unicode line terminator and $PS are invalid in JS string
        // literals.
        addCodeUnitEscaped(buffer, $LS);  // 0x2028.
      } else if (code == $PS) {
        addCodeUnitEscaped(buffer, $PS);  // 0x2029.
      } else if (code == $BACKSLASH) {
        buffer.write(r'\\');
      } else {
        if (code < 0x20) {
          addCodeUnitEscaped(buffer, code);
          // We emit DEL (ASCII 0x7f) as an escape because it would be confusing
          // to have it unescaped in a string literal.  We also escape
          // everything above 0x7f because that means we don't have to worry
          // about whether the web server serves it up as Latin1 or UTF-8.
        } else if (code < 0x7f) {
          buffer.writeCharCode(code);
        } else {
          // This will output surrogate pairs in the form \udxxx\udyyy, rather
          // than the more logical \u{zzzzzz}.  This should work in JavaScript
          // (especially old UCS-2 based implementations) and is the only
          // format that is allowed in JSON.
          addCodeUnitEscaped(buffer, code);
        }
      }
    }
  }

  for (int i = 0; i < string.length; i++) {
    int code = string.codeUnitAt(i);
    if (code < 0x20 || code == $DEL || code == $DQ || code == $LS ||
        code == $PS || code == $BACKSLASH || code >= 0x80) {
      writeEscapedOn(string, buffer);
      return;
    }
  }
  buffer.write(string);
}
*/