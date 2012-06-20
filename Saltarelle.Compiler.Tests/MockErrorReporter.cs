using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;

namespace Saltarelle.Compiler.Tests {
	class Message {
		public int Code { get; private set; }
		public string File { get; private set; }
		public TextLocation Location { get; private set; }
		public object[] Args { get; private set; }

		public Message(int code, string file, TextLocation location, params object[] args) {
			if (Messages.Get(code) == null)
				throw new InvalidOperationException("Invalid message code " + code);
			Code = code;
			File = file;
			Location = location;
			Args = args;
		}

		public override string ToString() {
			var msg = Messages.Get(Code);
			if (msg == null)
				throw new InvalidOperationException();
			return msg.Item1.ToString() + ": " + string.Format(msg.Item2, Args);
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

		public void Message(int code, string file, TextLocation location, params object[] args) {
			var msg = new Message(code, file, location, args);
			string s = msg.ToString();	// Ensure this does not throw an exception
			AllMessages.Add(msg);
			if (_logToConsole)
				Console.WriteLine(s);
		}

		public void InternalError(string text, string file, TextLocation location) {
			throw new Exception(text);
		}
	}
}
