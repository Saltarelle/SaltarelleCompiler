using System;
using System.Runtime.CompilerServices;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel {
	public class JsDeclarationScope : IEquatable<JsDeclarationScope> {
		public JsFunctionDefinitionExpression Expr { get; private set; }
		public JsFunctionStatement Stmt { get; private set; }
		public JsCatchClause Catch { get; private set; }

		private JsDeclarationScope() {
		}

		public static readonly JsDeclarationScope Root = new JsDeclarationScope();

		public static implicit operator JsDeclarationScope(JsFunctionDefinitionExpression expr) {
			return new JsDeclarationScope { Expr = expr };
		}

		public static implicit operator JsDeclarationScope(JsFunctionStatement stmt) {
			return new JsDeclarationScope { Stmt = stmt };
		}

		public static implicit operator JsDeclarationScope(JsCatchClause @catch) {
			return new JsDeclarationScope { Catch = @catch };
		}

		public bool Equals(JsDeclarationScope other) {
			return ReferenceEquals(other.Expr, Expr) && ReferenceEquals(other.Stmt, Stmt) && ReferenceEquals(other.Catch, Catch);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof(JsDeclarationScope)) return false;
			return Equals((JsDeclarationScope)obj);
		}

		public override int GetHashCode() {
			return RuntimeHelpers.GetHashCode(Expr) ^ RuntimeHelpers.GetHashCode(Stmt) ^ RuntimeHelpers.GetHashCode(Catch);
		}
	}
}