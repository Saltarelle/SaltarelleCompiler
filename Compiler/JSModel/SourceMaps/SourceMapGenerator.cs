using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler.JSModel.SourceMaps
{
   public class SourceMapsGenerator : ISourceMapRecorder
   {
      private SourceMapBuilder sourceMapBuilder;

      // TODO: make it configurable by the user
      private string sourceRoot = @"../sources/";

      public SourceMapsGenerator(string scriptPath, string mapPath)
      {
         string scriptUri    = Path.GetFileName(scriptPath);
         string sourceMapUri = Path.GetFileName(mapPath);         
         sourceMapBuilder = new SourceMapBuilder(sourceMapUri, scriptUri, sourceRoot);
      }

      public void RecordLocation(int scriptLine, int scriptCol, string sourcePath, int sourceLine, int sourceCol)
      {         
         // patch MSDOS-like path separator
         var path = sourcePath.Replace(@"\","/");         

         SourceLocation s = new SourceLocation(path, "", sourceLine-1, sourceCol-1);   // convert line and column to 0-based
         sourceMapBuilder.addMapping(scriptLine-1, scriptCol-1, s);                    //
      }
      
      public void WriteSourceMap(StreamWriter target) {			
			string mapFileContent = sourceMapBuilder.build();
         target.Write(mapFileContent);         
		}      
   }
}
