using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;

namespace Saltarelle.Compiler {
	public class DefaultErrorReporter : IErrorReporter {
		public void Message(int code, string file, TextLocation location, params object[] args) {
			var msg = Messages.Get(code);
			if (msg == null)
				throw new ArgumentException("A message with code " + code + " does not exist.", "code");
			// TODO: Something... errorReporter.Message(msg.Item1, code, location, string.Format(msg.Item2, args));
		}
	}
}
