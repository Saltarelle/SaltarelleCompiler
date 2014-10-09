// this source maps is based on Dart2Js implementation. See the file Dart.original.cs.

using System;
using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler.JSModel.SourceMaps
{  
   public class SourceMapBuilder 
   {
      const int VLQ_BASE_SHIFT = 5;
      const int VLQ_BASE_MASK = (1 << 5) - 1;
      const int VLQ_CONTINUATION_BIT = 1 << 5;
      const int VLQ_CONTINUATION_MASK = 1 << 5;
      const String BASE64_DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

      public string uri;        
      public string scriptUri; 
      public string sourceRoot; 
     
      public List<SourceMapEntry> entries;

      public Map<String, int> sourceUrlMap;
      public List<String> sourceUrlList;
      public Map<String, int> sourceNameMap;
      public List<String> sourceNameList;

      public int previousTargetLine;
      public int previousTargetColumn;
      public int previousSourceUrlIndex;
      public int previousSourceLine;
      public int previousSourceColumn;
      public int previousSourceNameIndex;
      public bool firstEntryInLine;
      
      public SourceMapBuilder(string sourceMapUri, string scriptUri, string sourceRoot) 
      {
         this.uri = sourceMapUri;
         this.scriptUri = scriptUri;         
         this.sourceRoot = sourceRoot;

         entries = new List<SourceMapEntry>();

         sourceUrlMap = new Map<String, int>();
         sourceUrlList = new List<String>();
         sourceNameMap = new Map<String, int>();
         sourceNameList = new List<String>();

         previousTargetLine = 0;
         previousTargetColumn = 0;
         previousSourceUrlIndex = 0;
         previousSourceLine = 0;
         previousSourceColumn = 0;
         previousSourceNameIndex = 0;
         firstEntryInLine = true;
      }

      public void resetPreviousSourceLocation() 
      {
         previousSourceUrlIndex = 0;
         previousSourceLine = 0;
         previousSourceColumn = 0;
         previousSourceNameIndex = 0;
      }

      public void updatePreviousSourceLocation(SourceLocation sourceLocation) 
      {
         previousSourceLine = sourceLocation.Line;
         previousSourceColumn = sourceLocation.Column;
         String sourceUrl = sourceLocation.SourceUrl;
         previousSourceUrlIndex = indexOf(sourceUrlList, sourceUrl, sourceUrlMap);
         String sourceName = sourceLocation.SourceName;
         if (sourceName != null) 
         {
            previousSourceNameIndex = indexOf(sourceNameList, sourceName, sourceNameMap);
         }
      }

      bool sameAsPreviousLocation(SourceLocation sourceLocation) 
      {
         if (sourceLocation == null) {
            return true;
         }
         int sourceUrlIndex =
            indexOf(sourceUrlList, sourceLocation.SourceUrl, sourceUrlMap);
         return
            sourceUrlIndex == previousSourceUrlIndex &&
            sourceLocation.Line == previousSourceLine &&
            sourceLocation.Column == previousSourceColumn;
      }

      public void addMapping(int scriptLine, int scriptColumn, SourceLocation sourceLocation) 
      {
         if (!entries.isEmpty() && (scriptLine == entries.last().scriptLine))  // same line ?
         {
            if (sameAsPreviousLocation(sourceLocation)) 
            {
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

         if (sourceLocation != null) 
         {
            updatePreviousSourceLocation(sourceLocation);
         }
         entries.Add(new SourceMapEntry(sourceLocation, scriptLine, scriptColumn));
      }

      private void printStringListOn(List<String> strings, StringBuilder buffer)
      {
         bool first = true;
         buffer.Append("[");
         foreach(String str in strings) 
         {
            if (!first) buffer.Append(",");
            buffer.Append("\u0022");
            buffer.writeJsonEscapedCharsOn(str);
            buffer.Append("\u0022");
            first = false;
         }
         buffer.Append("]");
      }

      public String build() 
      {
         resetPreviousSourceLocation();
         StringBuilder mappingsBuffer = new StringBuilder();
         entries.ForEach((SourceMapEntry entry) => writeEntry(entry, mappingsBuffer));
         StringBuilder buffer = new StringBuilder();
         buffer.Append("{\n");
         buffer.Append("  \u0022version\u0022: 3,\n");
         if (uri != null && scriptUri != null) {
            buffer.Append(string.Format("  \u0022file\u0022: \u0022{0}\u0022,\n", /*uri.MakeRelativeUri(scriptUri).ToString())*/ scriptUri) );
         }
         buffer.Append("  \u0022sourceRoot\u0022: \u0022"+sourceRoot+"\u0022,\n");
         buffer.Append("  \u0022sources\u0022: ");
         if(uri != null) 
         {            
            for(int t=0;t<sourceUrlList.Count;t++) 
            {
               sourceUrlList[t] = sourceUrlList[t];
               /*
               Uri U = new Uri(sourceUrlList[t]);
               sourceUrlList[t] = uri.MakeRelativeUri(U).ToString();
               */
            }
         }
         printStringListOn(sourceUrlList, buffer);
         buffer.Append(",\n");
         buffer.Append("  \u0022names\u0022: ");
         printStringListOn(sourceNameList, buffer);
         buffer.Append(",\n");
         buffer.Append("  \u0022mappings\u0022: \u0022");
         buffer.Append(mappingsBuffer);
         buffer.Append("\u0022\n}\n");
         return buffer.ToString();
      }

      public void writeEntry(SourceMapEntry entry, StringBuilder output) 
      {
         int targetLine = entry.scriptLine;
         int targetColumn = entry.scriptColumn;

         if (targetLine > previousTargetLine) {
            for (int i = previousTargetLine; i < targetLine; ++i) {
               output.Append(";");
            }
            previousTargetLine = targetLine;
            previousTargetColumn = 0;
            firstEntryInLine = true;
         }

         if (!firstEntryInLine) {
            output.Append(",");
         }
         firstEntryInLine = false;

         encodeVLQ(output, targetColumn - previousTargetColumn);
         previousTargetColumn = targetColumn;

         if (entry.sourceLocation == null) return;

         String sourceUrl = entry.sourceLocation.SourceUrl;
         int sourceLine = entry.sourceLocation.Line;
         int sourceColumn = entry.sourceLocation.Column;
         String sourceName = entry.sourceLocation.SourceName;

         int sourceUrlIndex = indexOf(sourceUrlList, sourceUrl, sourceUrlMap);
         encodeVLQ(output, sourceUrlIndex - previousSourceUrlIndex);
         encodeVLQ(output, sourceLine - previousSourceLine);
         encodeVLQ(output, sourceColumn - previousSourceColumn);

         if (sourceName != null) {
            int sourceNameIndex = indexOf(sourceNameList, sourceName, sourceNameMap);
            encodeVLQ(output, sourceNameIndex - previousSourceNameIndex);
         }

         // Update previous source location to ensure the next indices are relative
         // to those if [entry.sourceLocation].
         updatePreviousSourceLocation(entry.sourceLocation);
      }

      public int indexOf(List<String> list, String value, Map<String, int> map) 
      {         
         return map.putIfAbsent(value, ()=>
         {
            int index = list.Count;
            list.Add(value);
            return index;
         });
      }

      public static void encodeVLQ(StringBuilder output, int value) 
      {
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
