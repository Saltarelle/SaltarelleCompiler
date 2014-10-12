// this source maps is based on Dart2Js implementation. See the file Dart.original.cs.

using System;
using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler.JSModel.SourceMaps
{
   public class SourceLocation 
   {
      private String sourceUrl;
      private String sourceName;     
      private int line;
      private int column;   

      public SourceLocation(String sourceUrl, String sourceName, int line, int column) 
      {
         this.sourceUrl = sourceUrl;         
         this.sourceName = sourceName;
         this.line = line;
         this.column = column;
      }

      public String SourceUrl 
      { 
         get { return sourceUrl; }
      }

      public int Line 
      {         
         get { return line; }
      }

      public int Column
      { 
         get { return column; }
      }

      public String SourceName 
      {
         get { return sourceName; }
      }      
   }   
}
