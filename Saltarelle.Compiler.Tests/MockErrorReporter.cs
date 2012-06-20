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
			Code = code;
			File = file;
			Location = location;
			Args = args;
		}
	}

	class MockErrorReporter : IErrorReporter {
        public List<Message> AllMessages { get; set; }

		public IEnumerable<string> AllMessagesText {
			get {
				return AllMessages.Select(m => string.Format(Messages.Get(m.Code).Item2, m.Args));
			}
		}

        public MockErrorReporter(bool logToConsole) {
            AllMessages = new List<Message>();
        }

		public void Message(int code, string file, TextLocation location, params object[] args) {
			AllMessages.Add(new Message(code, file, location, args));
		}
	}
}
