using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.Expressions;

#pragma warning disable 618	// This file uses all the obsolete constructors that will be internal (non-obsolete) in a future release

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	[DebuggerDisplay("{DebugToString()}")]
	public abstract class JsStatement {
		[DebuggerStepThrough]
		public abstract TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data);

		public virtual string DebugToString() {
			return new Regex("\\s+").Replace(OutputFormatter.Format(this, allowIntermediates: true), " ");
		}

		public static JsAwaitStatement Await(JsExpression awaiter, string onCompletedMethodName) {
			return new JsAwaitStatement(awaiter, onCompletedMethodName);
		}

		public static JsBlockStatement Block(IEnumerable<JsStatement> statements) {
			return new JsBlockStatement(statements, false);
		}

		public static JsBlockStatement Block(params JsStatement[] statements) {
			return new JsBlockStatement(statements, false);
		}

		/// <summary>
		/// Convert a statement to a block statement. Returns null if the input is null. Returns <paramref name="content"/> if it is already a block statement.
		/// </summary>
		public static JsBlockStatement EnsureBlock(JsStatement content) {
			if (content == null)
				return null;
			else if (content is JsBlockStatement)
				return (JsBlockStatement)content;
			else
				return Block(content);
		}

		public static JsBlockStatement BlockMerged(IEnumerable<JsStatement> statements) {
			return new JsBlockStatement(statements, true);
		}

		public static JsBlockStatement BlockMerged(params JsStatement[] statements) {
			return new JsBlockStatement(statements, true);
		}

		private static readonly JsBlockStatement _emptyBlock = new JsBlockStatement(new JsStatement[0], false);
		public static JsBlockStatement EmptyBlock { get { return _emptyBlock; } }

		private static readonly JsBreakStatement _breakWithoutLabel = new JsBreakStatement(null);
		public static JsBreakStatement Break(string targetLabel = null) {
			return targetLabel == null ? _breakWithoutLabel : new JsBreakStatement(targetLabel);
		}

		public static JsComment Comment(string text) {
			return new JsComment(text);
		}

		private static readonly JsContinueStatement _continueWithoutLabel = new JsContinueStatement(null);
		public static JsContinueStatement Continue(string targetLabel = null) {
			return targetLabel == null ? _continueWithoutLabel : new JsContinueStatement(targetLabel);
		}

		public static JsDoWhileStatement DoWhile(JsExpression condition, JsStatement body) {
			return new JsDoWhileStatement(condition, body);
		}

		private static readonly JsEmptyStatement _empty = new JsEmptyStatement();
		public static JsEmptyStatement Empty {
			get { return _empty; }
		}

		/// <summary>
		/// Can also use the implicit conversion operator in JsExpression.
		/// </summary>
		public static JsExpressionStatement Expression(JsExpression expr) {
			return new JsExpressionStatement(expr);
		}

		public static JsForEachInStatement ForIn(string loopVariableName, JsExpression objectToIterateOver, JsStatement body, bool isLoopVariableDeclared = true) {
			return new JsForEachInStatement(loopVariableName, objectToIterateOver, body, isLoopVariableDeclared);
		}

		public static JsForStatement For(JsStatement initStatement, JsExpression conditionExpression, JsExpression iteratorExpression, JsStatement body) {
			return new JsForStatement(initStatement, conditionExpression, iteratorExpression, body);
		}

		public static JsFunctionStatement Function(string name, IEnumerable<string> parameterNames, JsStatement body) {
			return new JsFunctionStatement(name, parameterNames, body);
		}

		public static JsGotoStatement Goto(string targetLabel) {
			return new JsGotoStatement(targetLabel);
		}

		public static JsIfStatement If(JsExpression test, JsStatement then, JsStatement @else) {
			return new JsIfStatement(test, then, @else);
		}

		public static JsLabel Label(string label) {
			return new JsLabel(label);
		}

		private static readonly JsReturnStatement _returnNoValue = new JsReturnStatement(null);
		public static JsReturnStatement Return(JsExpression value = null) {
			return value == null ? _returnNoValue : new JsReturnStatement(value);
		}

		public static JsSwitchSection SwitchSection(IEnumerable<JsExpression> values, JsStatement body) {
			return new JsSwitchSection(values, body);
		}

		public static JsSwitchStatement Switch(JsExpression expression, IEnumerable<JsSwitchSection> sections) {
			return new JsSwitchStatement(expression, sections);
		}

		public static JsSwitchStatement Switch(JsExpression expression, params JsSwitchSection[] sections) {
			return new JsSwitchStatement(expression, sections);
		}

		public static JsThrowStatement Throw(JsExpression expression) {
			return new JsThrowStatement(expression);
		}

		public static JsCatchClause Catch(string identifier, JsStatement body) {
			return new JsCatchClause(identifier, body);
		}

		public static JsTryStatement Try(JsStatement guardedStatement, JsCatchClause catchClause, JsStatement @finally) {
			return new JsTryStatement(guardedStatement, catchClause, @finally);
		}

		public static JsVariableDeclaration Declaration(string name, JsExpression initializer) {
			return new JsVariableDeclaration(name, initializer);
		}

		public static JsVariableDeclarationStatement Var(IEnumerable<JsVariableDeclaration> declarations) {
			return new JsVariableDeclarationStatement(declarations);
		}

		public static JsVariableDeclarationStatement Var(params JsVariableDeclaration[] declarations) {
			return new JsVariableDeclarationStatement(declarations);
		}

		public static JsVariableDeclarationStatement Var(string name, JsExpression initializer) {
			return new JsVariableDeclarationStatement(new[] { Declaration(name, initializer) });
		}

		public static JsWhileStatement While(JsExpression condition, JsStatement body) {
			return new JsWhileStatement(condition, body);
		}

		public static JsWithStatement With(JsExpression @object, JsStatement body) {
			return new JsWithStatement(@object, body);
		}

		public static JsSequencePoint SequencePoint(Location location) {
			return new JsSequencePoint(location);
		}

		private static readonly JsYieldStatement _yieldBreakStatement = new JsYieldStatement(null);
		public static JsYieldStatement Yield(JsExpression value) {
			return value == null ? _yieldBreakStatement : new JsYieldStatement(value);
		}

		private static readonly JsExpressionStatement _useStrict = JsExpression.String("use strict");
		public static JsStatement UseStrict {
			get { return _useStrict; }
		}

		private static readonly JsExpressionStatement _debugger = JsExpression.Identifier("debugger");
		public static JsStatement Debugger {
			get { return _debugger; }
		}
	}
}
