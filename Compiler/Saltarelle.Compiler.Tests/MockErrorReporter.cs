using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Tests {
	public class Message {
		public MessageSeverity Severity { get; private set; }
		public int Code { get; private set; }
		public DomRegion Region { get; private set; }
		public string Format { get; private set; }
		public object[] Args { get; private set; }
		public string FormattedMessage { get; private set; }

		public Message(MessageSeverity severity, int code, DomRegion region, string format, params object[] args) {
			Severity = severity;
			Code = code;
			Region = region;
			Format = format;
			Args = args;
			FormattedMessage = Args.Length > 0 ? string.Format(Format, Args) : Format;
		}

		private static bool ArgsEqual(object[] a, object[] b) {
			if ((a == null) != (b == null))
				return false;
			return a == null || a.SequenceEqual(b);
		}

		public bool Equals(Message other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Severity, Severity) && other.Code == Code && Equals(other.Region, Region) && Equals(other.Format, Format) && ArgsEqual(other.Args, Args);
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
				result = (result*397) ^ Region.GetHashCode();
				result = (result*397) ^ (Format != null ? Format.GetHashCode() : 0);
				return result;
			}
		}

		public override string ToString() {
			return Severity.ToString() + ": " + FormattedMessage;
		}
	}

	public class MockErrorReporter : IErrorReporter {
		private readonly bool _logToConsole;
		public List<Message> AllMessages { get; set; }

		public MockErrorReporter(bool logToConsole = false) {
			_logToConsole = logToConsole;
			AllMessages   = new List<Message>();
		}

		public DomRegion Region { get; set; }

		public void Message(MessageSeverity severity, int code, string message, params object[] args) {
			var msg = new Message(severity, code, Region, message, args);
			foreach (var a in args) {
				try {
					new BinaryFormatter().Serialize(new MemoryStream(), a);
				}
				catch (Exception ex) {
					throw new Exception("Error serializing argument " + a);
				}
			}
			string s = msg.ToString();	// Ensure this does not throw an exception
			AllMessages.Add(msg);
			if (_logToConsole)
				Console.WriteLine(s);
		}

		public void InternalError(string text) {
			throw new Exception("Internal error: " + text);
		}

		public void InternalError(Exception ex, string additionalText = null) {
			throw ex;
		}
	}
}
