using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptParser {
	public interface IErrorReporter {
		void ReportError(int line, int col, string message);
	}
}
