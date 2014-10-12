using System;
using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler.JSModel.SourceMaps {  
   public class SourceMapBuilder {
      private const int VLQ_BASE_SHIFT = 5;
      private const int VLQ_BASE_MASK = (1 << 5) - 1;
      private const int VLQ_CONTINUATION_BIT = 1 << 5;
      private const int VLQ_CONTINUATION_MASK = 1 << 5;
      private const String BASE64_DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

      private string SourceMapPath;        
      private string ScriptPath; 
      private string SourceRoot; 
     
      private List<SourceMapEntry> Entries;

      private Map<String, int> SourceUrlMap;
      private List<String> SourceUrlList;
      private Map<String, int> SourceNameMap;
      private List<String> SourceNameList;

      private int PreviousTargetLine;
      private int PreviousTargetColumn;
      private int PreviousSourceUrlIndex;
      private int PreviousSourceLine;
      private int PreviousSourceColumn;
      private int PreviousSourceNameIndex;
      private bool FirstEntryInLine;
      
      public SourceMapBuilder(string sourceMapUri, string scriptUri, string sourceRoot) {
         this.SourceMapPath = sourceMapUri;
         this.ScriptPath = scriptUri;         
         this.SourceRoot = sourceRoot;

         Entries = new List<SourceMapEntry>();

         SourceUrlMap = new Map<String, int>();
         SourceUrlList = new List<String>();
         SourceNameMap = new Map<String, int>();
         SourceNameList = new List<String>();

         PreviousTargetLine = 0;
         PreviousTargetColumn = 0;
         PreviousSourceUrlIndex = 0;
         PreviousSourceLine = 0;
         PreviousSourceColumn = 0;
         PreviousSourceNameIndex = 0;
         FirstEntryInLine = true;
      }

      private void ResetPreviousSourceLocation() {
         PreviousSourceUrlIndex = 0;
         PreviousSourceLine = 0;
         PreviousSourceColumn = 0;
         PreviousSourceNameIndex = 0;
      }

      private void UpdatePreviousSourceLocation(SourceLocation sourceLocation) {
         PreviousSourceLine = sourceLocation.Line;
         PreviousSourceColumn = sourceLocation.Column;
         String sourceUrl = sourceLocation.SourceUrl;
         PreviousSourceUrlIndex = indexOf(SourceUrlList, sourceUrl, SourceUrlMap);
         String sourceName = sourceLocation.SourceName;
         if (sourceName != null) {
            PreviousSourceNameIndex = indexOf(SourceNameList, sourceName, SourceNameMap);
         }
      }

      private bool SameAsPreviousLocation(SourceLocation sourceLocation) {
         if (sourceLocation == null) {
            return true;
         }

         int sourceUrlIndex = indexOf(SourceUrlList, sourceLocation.SourceUrl, SourceUrlMap);

         return
            sourceUrlIndex == PreviousSourceUrlIndex &&
            sourceLocation.Line == PreviousSourceLine &&
            sourceLocation.Column == PreviousSourceColumn;
      }

      public void AddMapping(int scriptLine, int scriptColumn, SourceLocation sourceLocation) {
         if (!Entries.IsEmpty() && (scriptLine == Entries.Last().ScriptLine)) {
            if (SameAsPreviousLocation(sourceLocation))  {
               // The entry points to the same source location as the previous entry in
               // the same line, hence it is not needed for the source map.
               //
               // TODO(zarah): Remove this check and make sure that [addMapping] is not
               // called for this position. Instead, when consecutive lines in the
               // generated code point to the same source location, record this and use
               // it to generate the entries of the source map.
               return;
            }
         }

         if (sourceLocation != null) {
            UpdatePreviousSourceLocation(sourceLocation);
         }
         Entries.Add(new SourceMapEntry(sourceLocation, scriptLine, scriptColumn));
      }

      private void PrintStringListOn(List<String> strings, StringBuilder buffer) {
         bool first = true;
         buffer.Append("[");
         foreach(String str in strings) {
            if (!first) buffer.Append(",");
            buffer.Append("\u0022");
            buffer.WriteJsonEscapedCharsOn(str);
            buffer.Append("\u0022");
            first = false;
         }
         buffer.Append("]");
      }

      public String Build() {
         ResetPreviousSourceLocation();
         StringBuilder mappingsBuffer = new StringBuilder();
         Entries.ForEach((SourceMapEntry entry) => WriteEntry(entry, mappingsBuffer));
         StringBuilder buffer = new StringBuilder();
         buffer.Append("{\n");
         buffer.Append("  \u0022version\u0022: 3,\n");
         if (SourceMapPath != null && ScriptPath != null) {
            buffer.Append(string.Format("  \u0022file\u0022: \u0022{0}\u0022,\n", /*uri.MakeRelativeUri(scriptUri).ToString())*/ ScriptPath) );
         }
         buffer.Append("  \u0022sourceRoot\u0022: \u0022"+SourceRoot+"\u0022,\n");
         buffer.Append("  \u0022sources\u0022: ");
         if(SourceMapPath != null) {            
            for(int t=0;t<SourceUrlList.Count;t++) {
               SourceUrlList[t] = SourceUrlList[t];
            }
         }
         PrintStringListOn(SourceUrlList, buffer);
         buffer.Append(",\n");
         buffer.Append("  \u0022names\u0022: ");
         PrintStringListOn(SourceNameList, buffer);
         buffer.Append(",\n");
         buffer.Append("  \u0022mappings\u0022: \u0022");
         buffer.Append(mappingsBuffer);
         buffer.Append("\u0022\n}\n");
         return buffer.ToString();
      }

      private void WriteEntry(SourceMapEntry entry, StringBuilder output) {
         int targetLine = entry.ScriptLine;
         int targetColumn = entry.ScriptColumn;

         if (targetLine > PreviousTargetLine) {
            for (int i = PreviousTargetLine; i < targetLine; ++i) {
               output.Append(";");
            }
            PreviousTargetLine = targetLine;
            PreviousTargetColumn = 0;
            FirstEntryInLine = true;
         }

         if (!FirstEntryInLine) {
            output.Append(",");
         }
         FirstEntryInLine = false;

         encodeVLQ(output, targetColumn - PreviousTargetColumn);
         PreviousTargetColumn = targetColumn;

         if (entry.SourceLocation == null) return;

         String sourceUrl = entry.SourceLocation.SourceUrl;
         int sourceLine = entry.SourceLocation.Line;
         int sourceColumn = entry.SourceLocation.Column;
         String sourceName = entry.SourceLocation.SourceName;

         int sourceUrlIndex = indexOf(SourceUrlList, sourceUrl, SourceUrlMap);
         encodeVLQ(output, sourceUrlIndex - PreviousSourceUrlIndex);
         encodeVLQ(output, sourceLine - PreviousSourceLine);
         encodeVLQ(output, sourceColumn - PreviousSourceColumn);

         if (sourceName != null) {
            int sourceNameIndex = indexOf(SourceNameList, sourceName, SourceNameMap);
            encodeVLQ(output, sourceNameIndex - PreviousSourceNameIndex);
         }

         // Update previous source location to ensure the next indices are relative
         // to those if [entry.sourceLocation].
         UpdatePreviousSourceLocation(entry.SourceLocation);
      }

      public int indexOf(List<String> list, String value, Map<String, int> map) {         
         return map.PutIfAbsent(value, ()=> {
            int index = list.Count;
            list.Add(value);
            return index;
         });
      }

      public static void encodeVLQ(StringBuilder output, int value) {
         int signBit = 0;
         if (value < 0) {
            signBit = 1;
            value = -value;
         }
         value = (value << 1) | signBit;
         do {
            int digit = value & VLQ_BASE_MASK;
            value >>= VLQ_BASE_SHIFT;
            if (value > 0) {
               digit |= VLQ_CONTINUATION_BIT;
            }
            output.Append(BASE64_DIGITS[digit]);
         } while (value > 0);
      }
   }  
}
