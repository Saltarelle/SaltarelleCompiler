using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Compiler {
	public static class InlineCodeMethodCompiler {
		private static InlineCodeToken ParsePlaceholder(IMethod method, string text, Action<string> errorReporter) {
			if (text[0] == '$') {
				try {
					var s = text.Substring(1).Trim();
					ReflectionHelper.ParseReflectionName(s);
					return new InlineCodeToken(InlineCodeToken.TokenType.TypeRef, text: s);
				}
				catch (ReflectionNameParseException) {
					errorReporter("Invalid type reference " + text);
					return null;
				}
			}
			else if (text == "this")
				return new InlineCodeToken(InlineCodeToken.TokenType.This);

			string argName = text.TrimStart('@', '*');

			for (int i = 0; i < method.Parameters.Count; i++) {
				string paramName = method.Parameters[i].Name;
				if (paramName[0] == '@')
					paramName = paramName.Substring(1);
				if (paramName == argName) {
					if (i >= 0) {
						if (text[0] == '@')
							return new InlineCodeToken(InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier, index: i);
						else if (text[0] == '*')
							return new InlineCodeToken(InlineCodeToken.TokenType.Parameter, index: i, isExpandedParamArray: true);
						else
							return new InlineCodeToken(InlineCodeToken.TokenType.Parameter, index: i);
					}
				}
			}

			for (int i = 0; i < method.DeclaringTypeDefinition.TypeParameterCount; i++) {
				if (method.DeclaringTypeDefinition.TypeParameters[i].Name == text)
					return new InlineCodeToken(InlineCodeToken.TokenType.TypeParameter, index: i, ownerType: SymbolKind.TypeDefinition);
			}

			for (int i = 0; i < method.TypeParameters.Count; i++) {
				if (method.TypeParameters[i].Name == text)
					return new InlineCodeToken(InlineCodeToken.TokenType.TypeParameter, index: i, ownerType: SymbolKind.Method);
			}

			errorReporter("Unknown placeholder '{" + text + "}'");
			return null;
		}

		public static IList<InlineCodeToken> Tokenize(IMethod method, string code, Action<string> errorReporter) {
			var currentChunk = new StringBuilder();
			var result = new List<InlineCodeToken>();

			for (int i = 0; i < code.Length; i++) {
				if (code[i] == '{') {
					i++;

					if (i < code.Length && code[i] == '{') {
						currentChunk.Append('{');	// double {{ represents a single {.
						continue;
					}
					else if (i < code.Length && code[i] == '}') {	// Support {} as inline (common in JS)
						currentChunk.Append("{}");
						continue;
					}
					else {
						if (currentChunk.Length > 0) {
							result.Add(new InlineCodeToken(InlineCodeToken.TokenType.Text, currentChunk.ToString()));
							currentChunk.Clear();
						}
						while (i < code.Length && code[i] != '}') {
							currentChunk.Append(code[i]);
							i++;
						}
						if (i < code.Length) {
							var ph = ParsePlaceholder(method, currentChunk.ToString(), errorReporter);
							if (ph == null)
								return null;
							result.Add(ph);
						}
						else {
							errorReporter("expected '}'");
							return null;
						}
						currentChunk.Clear();
					}
				}
				else {
					currentChunk.Append(code[i]);
					if (code[i] == '}' && i < code.Length - 1 && code[i + 1] == '}') {
						i++;	// Skip optional double }}
					}
				}
			}

			if (currentChunk.Length > 0)
				result.Add(new InlineCodeToken(InlineCodeToken.TokenType.Text, currentChunk.ToString()));

			return result;
		}

		public static JsExpression CompileInlineCodeMethodInvocation(IMethod method, IList<InlineCodeToken> tokens, JsExpression @this, IList<JsExpression> arguments, Func<string, JsExpression> resolveType, Func<IType, JsExpression> resolveTypeArgument, Action<string> errorReporter) {
			var text = new StringBuilder();
			var substitutions = new Dictionary<string, Tuple<JsExpression, bool>>();
			bool hasErrors = false;

			foreach (var token in tokens) {
				switch (token.Type) {
					case InlineCodeToken.TokenType.Text:
						text.Append(token.Text);
						break;

					case InlineCodeToken.TokenType.This: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);

						if (@this == null) {
							hasErrors = true;
							errorReporter("Cannot use {this} in the literal code for a static method");
							substitutions[s] = Tuple.Create((JsExpression)JsExpression.Null, false);
						}
						else {
							substitutions[s] = Tuple.Create(@this, false);
						}
						break;
					}
					
					case InlineCodeToken.TokenType.Parameter: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);
						substitutions[s] = Tuple.Create(arguments[token.Index], token.IsExpandedParamArray);

						if (token.IsExpandedParamArray) {
							if (!method.Parameters[token.Index].IsParams) {
								hasErrors = true;
								errorReporter("The parameter " + method.Parameters[token.Index].Name + " must be a param array in order to use it with the * modifier.");
								substitutions[s] = Tuple.Create((JsExpression)JsExpression.ArrayLiteral(), true);
							}
							else if (arguments[arguments.Count - 1].NodeType != ExpressionNodeType.ArrayLiteral) {
								throw new Exception("The last argument must be a literal array if using the {*arg} placeholder");
							}
						}
						break;
					}

					case InlineCodeToken.TokenType.TypeParameter: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);
						var l = token.OwnerType == SymbolKind.TypeDefinition ? method.DeclaringType.TypeArguments : method.TypeArguments;
						substitutions[s] = Tuple.Create(l != null ? resolveTypeArgument(l[token.Index]) : JsExpression.Null, false);
						break;
					}

					case InlineCodeToken.TokenType.TypeRef: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);

						substitutions[s] = Tuple.Create(resolveType(token.Text), false);
						break;
					}

					case InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier: {
						if (!method.Parameters[token.Index].Type.IsKnownType(KnownTypeCode.String)) {
							text.Append("X");	// Just something that should not cause an error.
							hasErrors = true;
							errorReporter("The type of the parameter " + method.Parameters[token.Index].Name + " must be string in order to use it with the '@' modifier.");
						}
						else {
							var jce = arguments[token.Index] as JsConstantExpression;
							if (jce != null && jce.NodeType == ExpressionNodeType.String) {
								text.Append(jce.StringValue);
							}
							else {
								text.Append("X");	// Just something that should not cause an error.
								hasErrors = true;
								errorReporter("The argument specified for parameter " + method.Parameters[token.Index].Name + " must be a literal string");
							}
						}
						break;
					}

					default:
						throw new ArgumentException("Unknown token type " + token.Type);
				}
			}

			if (hasErrors)
				return JsExpression.Number(0);

			try {
				var expr = JavaScriptParser.Parser.ParseExpression(text.ToString());
				return new Substituter(substitutions, errorReporter).Process(expr);
			}
			catch (RecognitionException) {
				errorReporter("syntax error in inline code");
				return JsExpression.Number(0);
			}
		}

		private class Substituter : RewriterVisitorBase<object> {
			private readonly Dictionary<string, Tuple<JsExpression, bool>> _substitutions;
			private readonly Action<string> _errorReporter;

			public Substituter(Dictionary<string, Tuple<JsExpression, bool>> substitutions, Action<string> errorReporter) {
				_substitutions = substitutions;
				_errorReporter = errorReporter;
			}

			private IList<JsExpression> VisitWithParamExpansion(IList<JsExpression> list) {
				if (!_substitutions.Values.Any(v => v.Item2))
					return base.VisitExpressions(list, null);

				return VisitCollection(list, expr => {
				           var ident = expr as JsIdentifierExpression;
				           Tuple<JsExpression, bool> v;
				           if (ident == null || !_substitutions.TryGetValue(ident.Name, out v) || !v.Item2)
				               return new[] { VisitExpression(expr, null) };
				           return ((JsArrayLiteralExpression)v.Item1).Elements;
				       });
			}

			public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, object data) {
				Tuple<JsExpression, bool> value;
				if (_substitutions.TryGetValue(expression.Name, out value)) {
					if (value.Item2) {
						_errorReporter("Expanded parameters in inline code can only be used in array literals, function invocations, or 'new' expressions");
						return expression;
					}
					else
						return value.Item1;
				}
				else {
					return expression;
				}
			}

			public override JsExpression VisitArrayLiteralExpression(JsArrayLiteralExpression expression, object data) {
				var l = VisitWithParamExpansion(expression.Elements);
				return ReferenceEquals(l, expression.Elements) ? expression : JsExpression.ArrayLiteral(l);
			}

			public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
				var m = VisitExpression(expression.Method, data);
				var a = VisitWithParamExpansion(expression.Arguments);
				return ReferenceEquals(m, expression.Method) && ReferenceEquals(a, expression.Arguments) ? expression : JsExpression.Invocation(m, a);
			}

			public override JsExpression VisitNewExpression(JsNewExpression expression, object data) {
				var c = VisitExpression(expression.Constructor, null);
				var a = expression.Arguments != null ? VisitWithParamExpansion(expression.Arguments) : null;
				return ReferenceEquals(c, expression.Constructor) && ReferenceEquals(a, expression.Arguments) ? expression : JsExpression.New(c, a);
			}

			public JsExpression Process(JsExpression expr) {
				return VisitExpression(expr, null);
			}
		}

		public static IList<string> ValidateLiteralCode(IMethod method, string literalCode, Func<string, JsExpression> resolveType, Func<IType, JsExpression> resolveTypeArgument) {
			var errors = new List<string>();

			var tokens = Tokenize(method, literalCode, s => errors.Add("Error in literal code pattern: " + s));
			if (tokens == null)
				return errors;

			CompileInlineCodeMethodInvocation(method,
			                                  tokens,
			                                  method.IsStatic || method.IsConstructor ? null : JsExpression.Null,
			                                  method.Parameters.Select(p => p.IsParams ? (JsExpression)JsExpression.ArrayLiteral() : JsExpression.String("X")).ToList(),
			                                  resolveType,
			                                  resolveTypeArgument,
			                                  errors.Add);
			return errors;
		}
	}
}
