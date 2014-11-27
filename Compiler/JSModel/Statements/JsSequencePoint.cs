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

		internal JsSequencePoint(Location location) {
			Location = location;
		}

		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitSequencePoint(this, data);
		}
	}
}
