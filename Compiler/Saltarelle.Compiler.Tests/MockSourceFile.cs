using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.Tests {
	class MockSourceFile : ISourceFile {
        private readonly string _filename;
        private readonly string _content;

        public MockSourceFile(string filename, string content) {
            _filename = filename;
            _content  = content;
        }

        public string Filename {
            get { return _filename; }
        }

        public TextReader Open() {
            return new StringReader(_content);
        }
	}
}
