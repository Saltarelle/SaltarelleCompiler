using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsFunctionDefinitionExpression : JsExpression {
		public ReadOnlyCollection<string> ParameterNames { get; private set; }
		public JsBlockStatement Body { get; private set; }

		/// <summary>
		/// Null if the function does not have a name.
		/// </summary>
		public string Name { get; private set; }

		internal JsFunctionDefinitionExpression(IEnumerable<string> parameterNames, JsStatement body, string name = null) : base(ExpressionNodeType.FunctionDefinition) {
			if (parameterNames == null) throw new ArgumentNullException("parameterNames");
			if (body == null) throw new ArgumentNullException("body");
			if (name != null && !name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");

			ParameterNames = parameterNames.AsReadOnly();
			if (ParameterNames.Any(n => !n.IsValidJavaScriptIdentifier()))
				throw new ArgumentException("parameterNames");
			Body = JsStatement.EnsureBlock(body);
			Name = name;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitFunctionDefinitionExpression(this, data);
		}
	}
}
