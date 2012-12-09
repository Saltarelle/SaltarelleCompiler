using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Compiler {
	public static class InlineCodeMethodCompiler {
		public class InlineCodeToken {
			public enum TokenType {
				Text,
				This,
				Parameter,
				TypeParameter,
				TypeRef,
				LiteralStringParameterToUseAsIdentifier,
				ExpandedParamArrayParameter,
			}

			public TokenType Type { get; private set; }
			private string _text;

			public string Text {
				get {
					if (Type != TokenType.Text && Type != TokenType.TypeRef)
						throw new InvalidOperationException();
					return _text;
				}
			}

			private int _index;
			public int Index {
				get {
					if (Type != TokenType.Parameter && Type != TokenType.TypeParameter && Type != TokenType.LiteralStringParameterToUseAsIdentifier && Type != TokenType.ExpandedParamArrayParameter)
						throw new InvalidOperationException();
					return _index;
				}
			}

			public InlineCodeToken(TokenType type, string text = null, int index = -1) {
				Type   = type;
				_text  = text;
				_index = index;
			}

			public bool Equals(InlineCodeToken other) {
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return Equals(other._text, _text) && other._index == _index && Equals(other.Type, Type);
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != typeof (InlineCodeToken)) return false;
				return Equals((InlineCodeToken) obj);
			}

			public override int GetHashCode() {
				unchecked {
					int result = (_text != null ? _text.GetHashCode() : 0);
					result = (result*397) ^ _index;
					result = (result*397) ^ Type.GetHashCode();
					return result;
				}
			}

			public override string ToString() {
				return string.Format("Text: {0}, Index: {1}, Type: {2}", _text, _index, Type);
			}
		}

		private static int FindParameter(string name, IList<string> allParameters) {
			int i = allParameters.IndexOf(name);
			if (i >= 0)
				return i;
			return allParameters.IndexOf("@" + name);
		}

		private static InlineCodeToken ParsePlaceholder(string text, IList<string> parameterNames, IList<string> typeParameterNames, Action<string> errorReporter) {
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

			if (parameterNames != null) {
				int i = FindParameter(argName, parameterNames);
				if (i >= 0) {
					if (text[0] == '@')
						return new InlineCodeToken(InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier, index: i);
					else if (text[0] == '*')
						return new InlineCodeToken(InlineCodeToken.TokenType.ExpandedParamArrayParameter, index: i);
					else
						return new InlineCodeToken(InlineCodeToken.TokenType.Parameter, index: i);
				}
			}

			if (typeParameterNames != null) {
				int i = FindParameter(text, typeParameterNames);
				if (i >= 0)
					return new InlineCodeToken(InlineCodeToken.TokenType.TypeParameter, index: i);
			}

			errorReporter("Unknown placeholder '{" + text + "}'");
			return null;
		}

		public static IList<InlineCodeToken> Tokenize(string code, IList<string> parameterNames, IList<string> typeParameterNames, Action<string> errorReporter) {
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
							var ph = ParsePlaceholder(currentChunk.ToString(), parameterNames, typeParameterNames, errorReporter);
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

		private static string CreatePlaceholder(int index) {
			return "{" + index.ToString(CultureInfo.InvariantCulture) + "}";
		}

		public static JsExpression CompileInlineCodeMethodInvocation(IMethod method, string literalCode, JsExpression @this, IList<JsExpression> arguments, Func<ITypeReference, IType> resolveType, Func<IType, TypeContext, JsExpression> getJsType, bool isParamArrayExpanded, Action<string> errorReporter) {
			List<string> typeParameterNames = new List<string>();
			List<IType>  typeArguments      = new List<IType>();

			if (method.DeclaringTypeDefinition.TypeParameterCount > 0) {
				var parameterizedType = method.DeclaringType as ParameterizedType;
				typeParameterNames.AddRange(method.DeclaringTypeDefinition.TypeParameters.Select(p => p.Name));
				if (parameterizedType != null) {
					typeArguments.AddRange(parameterizedType.TypeArguments);
				}
				else {
					typeArguments.AddRange(Enumerable.Repeat(resolveType(ReflectionHelper.ParseReflectionName("System.Object")), method.DeclaringType.TypeParameterCount));
				}
			}

			if (method.TypeParameters.Count > 0) {
				typeParameterNames.AddRange(method.TypeParameters.Select(p => p.Name).ToList());
				var specializedMethod = method as SpecializedMethod;
				if (specializedMethod != null) {
					typeArguments.AddRange(specializedMethod.TypeArguments);
				}
				else {
					typeArguments.AddRange(Enumerable.Repeat(resolveType(ReflectionHelper.ParseReflectionName("System.Object")), method.TypeParameters.Count));
				}
			}

			var tokens = Tokenize(literalCode, method.Parameters.Select(p => p.Name).ToList(), typeParameterNames, s => errorReporter("Error in literal code pattern: " + s));
			if (tokens == null)
				return JsExpression.Number(0);

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
						substitutions[s] = Tuple.Create(arguments[token.Index], false);
						break;
					}

					case InlineCodeToken.TokenType.TypeParameter: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);
						substitutions[s] = Tuple.Create(getJsType(typeArguments[token.Index], TypeContext.InlineCode), false);
						break;
					}

					case InlineCodeToken.TokenType.TypeRef: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);

						var type = resolveType(ReflectionHelper.ParseReflectionName(token.Text));
						if (type.Kind == TypeKind.Unknown) {
							hasErrors = true;
							errorReporter("Unknown type '" + token.Text + "' specified in inline implementation");
							substitutions[s] = Tuple.Create((JsExpression)JsExpression.Null, false);
						}
						else {
							substitutions[s] = Tuple.Create(getJsType(type, TypeContext.InlineCode), false);
						}
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

					case InlineCodeToken.TokenType.ExpandedParamArrayParameter: {
						string s = string.Format(CultureInfo.InvariantCulture, "$$__{0}__$$", substitutions.Count);
						text.Append(s);

						if (!method.Parameters[token.Index].IsParams) {
							hasErrors = true;
							errorReporter("The parameter " + method.Parameters[token.Index].Name + " must be a param array in order to use it with the * modifier.");
							substitutions[s] = Tuple.Create((JsExpression)JsExpression.ArrayLiteral(), true);
						}
						else if (!isParamArrayExpanded) {
							hasErrors = true;
							errorReporter("The method " + method.DeclaringType.FullName + "." + method.Name + " can only be invoked with its params parameter expanded");
							substitutions[s] = Tuple.Create((JsExpression)JsExpression.ArrayLiteral(), true);
						}
						else {
							substitutions[s] = Tuple.Create(arguments[token.Index], true);
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
				var a = VisitWithParamExpansion(expression.Arguments);
				return ReferenceEquals(c, expression.Constructor) && ReferenceEquals(a, expression.Arguments) ? expression : JsExpression.New(c, a);
			}

			public JsExpression Process(JsExpression expr) {
				return VisitExpression(expr, null);
			}
		}

		public static IList<string> ValidateLiteralCode(IMethod method, string literalCode, Func<ITypeReference, IType> resolveType) {
			var errors = new List<string>();
			CompileInlineCodeMethodInvocation(method,
			                                  literalCode,
			                                  method.IsStatic ? null : JsExpression.Null,
			                                  method.Parameters.Select(p => p.IsParams ? (JsExpression)JsExpression.ArrayLiteral() : JsExpression.String("X")).ToList(),
			                                  resolveType,
			                                  (t, c) => JsExpression.Null,
			                                  true,
			                                  errors.Add);
			return errors;
		}
	}
}
