using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.Tests {
	class MockSourceFile : ISourceFile {
        private readonly string _fileName;
        private readonly string _content;

        public MockSourceFile(string fileName, string content) {
            _fileName = fileName;
            _content  = content;
        }

        public string FileName {
            get { return _fileName; }
        }

        public TextReader Open() {
            return new StringReader(_content);
        }
	}
}
