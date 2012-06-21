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

		private static bool ArgsEqual(object[] a, object[] b) {
			if ((a == null) != (b == null))
				return false;
			return a == null || a.SequenceEqual(b);
		}

		public bool Equals(Message other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Severity, Severity) && other.Code == Code && Equals(other.File, File) && other.Location.Equals(Location) && Equals(other.Format, Format) && ArgsEqual(other.Args, Args);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Message)) return false;
			return Equals((Message) obj);
		}

		public override int GetHashCode() {
			unchecked {
				int result = Severity.GetHashCode();
				result = (result*397) ^ Code;
				result = (result*397) ^ (File != null ? File.GetHashCode() : 0);
				result = (result*397) ^ Location.GetHashCode();
				result = (result*397) ^ (Format != null ? Format.GetHashCode() : 0);
				return result;
			}
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

        public MockErrorReporter(bool logToConsole = false) {
        	_logToConsole = logToConsole;
        	AllMessages   = new List<Message>();
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
