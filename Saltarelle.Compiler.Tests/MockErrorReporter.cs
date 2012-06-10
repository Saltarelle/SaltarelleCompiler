using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.Tests {
	class MockErrorReporter : IErrorReporter {
        public List<string> AllMessages { get; set; }

        public MockErrorReporter(bool logToConsole) {
            AllMessages = new List<string>();
            Error   = s => { s = "Error: " + s; if (logToConsole) Console.WriteLine(s); AllMessages.Add(s); };
            Warning = s => { s = "Warning: " + s; if (logToConsole) Console.WriteLine(s); AllMessages.Add(s); };
        }

        public Action<string> Error { get; set; }
        public Action<string> Warning { get; set; }

        void IErrorReporter.Error(string message) {
            Error(message);
        }

        void IErrorReporter.Warning(string message) {
            Warning(message);
        }
    }
}
