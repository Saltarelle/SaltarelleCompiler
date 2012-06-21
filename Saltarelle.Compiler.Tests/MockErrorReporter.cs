using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;

namespace Saltarelle.Compiler.Tests {
	class Message {
		public MessageSeverity Severity { get; private set; }
		public int Code { get; private set; }
		public string File { get; private set; }
		public TextLocation Location { get; private set; }
		public string Format { get; private set; }
		public object[] Args { get; private set; }

		public Message(MessageSeverity severity, int code, string file, TextLocation location, string format, params object[] args) {
			Severity = severity;
			Code = code;
			File = file;
			Location = location;
			Format = format;
			Args = args;
		}

		public override string ToString() {
			return Severity.ToString() + ": " + (Args.Length > 0 ? string.Format(Format, Args) : Format);
		}
	}

	class MockErrorReporter : IErrorReporter {
		private readonly bool _logToConsole;
		public List<Message> AllMessages { get; set; }

		public List<string> AllMessagesText {
			get {
				return AllMessages.Select(m => m.ToString()).ToList();
			}
		}

        public MockErrorReporter(bool logToConsole) {
        	_logToConsole = logToConsole;
        	AllMessages = new List<Message>();
        }

		public void Message(MessageSeverity severity, int code, string file, TextLocation location, string message, params object[] args) {
			var msg = new Message(severity, code, file, location, message, args);
			string s = msg.ToString();	// Ensure this does not throw an exception
			AllMessages.Add(msg);
			if (_logToConsole)
				Console.WriteLine(s);
		}

		public void InternalError(string text, string file, TextLocation location) {
			throw new NotImplementedException();
		}

		public void InternalError(Exception ex, string file, TextLocation location, string additionalText = null) {
			throw new NotImplementedException();
		}
	}
}
