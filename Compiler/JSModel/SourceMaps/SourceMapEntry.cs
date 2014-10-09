// this source maps is based on Dart2Js implementation. See the file Dart.original.cs.

using System;
using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler.JSModel.SourceMaps
{
   public class SourceMapEntry 
   {      
      public int scriptLine;
      public int scriptColumn;
      public SourceLocation sourceLocation;      

      public SourceMapEntry(SourceLocation sourceLocation, int scriptLine, int scriptColumn)
      {
         this.sourceLocation = sourceLocation;         
         this.scriptLine   = scriptLine;
         this.scriptColumn = scriptColumn;
      }
   }  
}
