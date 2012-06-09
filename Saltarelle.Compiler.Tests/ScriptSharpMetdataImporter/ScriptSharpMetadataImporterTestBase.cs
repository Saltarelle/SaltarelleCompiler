using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetdataImporter {
    public class ScriptSharpMetadataImporterTestBase {
        private class MockSourceFile : ISourceFile {
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

        protected class MockErrorReporter : IErrorReporter {
        	private readonly bool _logToConsole;
        	public List<string> AllMessages { get; set; }

            public MockErrorReporter(bool logToConsole) {
            	_logToConsole = logToConsole;
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

        private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location));
        protected IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

        protected void Process(IEnumerable<string> sources) {
        }

        protected void Process(params string[] sources) {
            Process((IEnumerable<string>)sources);
        }
    }
}
