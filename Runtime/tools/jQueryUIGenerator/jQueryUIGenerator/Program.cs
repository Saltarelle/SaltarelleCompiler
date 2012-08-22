// Program.cs
// jQueryUIGenerator
//
// Copyright 2012 Ivaylo Gochkov
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using ScriptSharp.Tools.jQueryUIGenerator.Model;

namespace ScriptSharp.Tools.jQueryUIGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // arguments
            if (string.IsNullOrEmpty(args[0])) {
                throw new ArgumentException("Source path is not specified.");
            }
            if (string.IsNullOrEmpty(args[1])) {
                throw new ArgumentException("Destination path is not specified.");
            }
            
            string sourcePath = args[0];
            string destinationPath = args[1];

            bool includeProjectFile = false;

            if (args.Length > 2) { 
                includeProjectFile = (args[2].ToLowerInvariant() == "/p");
            }

            // parse sources
            Parser xmlParser = new Parser(sourcePath, Console.Out);
            IList<Entry> entries = xmlParser.Parse();
            
            // generate files
            Generator generator = new Generator(destinationPath, Console.Out);
            generator.Render(entries);

            foreach (var f in Directory.EnumerateFiles(sourcePath, "*.js")) {
                File.Copy(f, Path.Combine(destinationPath, Path.GetFileName(f)));
            }

            if (includeProjectFile) {
                generator.RenderProjectFile(entries);
            }
        }
    }
}
