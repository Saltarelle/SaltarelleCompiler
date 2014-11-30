using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsSequencePoint : JsStatement {
		public Location Location { get; private set; }

		[Obsolete("Use JsStatement.SequencePoint instead")]
		internal JsSequencePoint(Location location) {
			Location = location;
		}

		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitSequencePoint(this, data);
		}

		public override string DebugToString() {
			if (Location == null) {
				return "@ <none>";
			}
			else {
				var loc = Location.GetMappedLineSpan();
				return string.Format("@ ({0}, {1}) - ({2}, {3})", (loc.StartLinePosition.Line + 1), (loc.StartLinePosition.Character + 1), (loc.EndLinePosition.Line + 1), (loc.EndLinePosition.Character + 1));
			}
		}
	}
}
