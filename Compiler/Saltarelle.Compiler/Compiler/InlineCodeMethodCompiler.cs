using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
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
				ExpandedParamArrayParameterWithCommaBefore,
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
					if (Type != TokenType.Parameter && Type != TokenType.TypeParameter && Type != TokenType.LiteralStringParameterToUseAsIdentifier && Type != TokenType.ExpandedParamArrayParameter && Type != TokenType.ExpandedParamArrayParameterWithCommaBefore)
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
					return new InlineCodeToken(InlineCodeToken.TokenType.Text, text: text);
				}
			}
			else if (text == "this")
				return new InlineCodeToken(InlineCodeToken.TokenType.This);

			string argName = text.TrimStart('@', '*', ',');

			if (parameterNames != null) {
				int i = FindParameter(argName, parameterNames);
				if (i >= 0) {
					if (text[0] == '@')
						return new InlineCodeToken(InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier, index: i);
					else if (text[0] == '*')
						return new InlineCodeToken(InlineCodeToken.TokenType.ExpandedParamArrayParameter, index: i);
					else if (text[0] == ',')
						return new InlineCodeToken(InlineCodeToken.TokenType.ExpandedParamArrayParameterWithCommaBefore, index: i);
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
			return new InlineCodeToken(InlineCodeToken.TokenType.Text, text);
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
							result.Add(ParsePlaceholder(currentChunk.ToString(), parameterNames, typeParameterNames, errorReporter));
						}
						else {
							errorReporter("expected '}'");
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

		private static string Escape(string s) {
			return s.Replace("{", "{{").Replace("}", "}}");
		}

		public static JsExpression CompileInlineCodeMethodInvocation(IMethod method, string literalCode, JsExpression @this, IList<JsExpression> arguments, Func<ITypeReference, JsExpression> getType, bool isParamArrayExpanded, Action<string> errorReporter) {
			List<string> typeParameterNames = new List<string>();
			List<IType>  typeArguments      = new List<IType>();
			var parameterizedType = method.DeclaringType as ParameterizedType;
			if (parameterizedType != null) {
				typeParameterNames.AddRange(parameterizedType.GetDefinition().TypeParameters.Select(p => p.Name));
				typeArguments.AddRange(parameterizedType.TypeArguments);
			}

			var specializedMethod = method as SpecializedMethod;
			if (specializedMethod != null) {
				typeParameterNames.AddRange(specializedMethod.TypeParameters.Select(p => p.Name).ToList());
				typeArguments.AddRange(specializedMethod.TypeArguments);
			}

			var tokens = Tokenize(literalCode, method.Parameters.Select(p => p.Name).ToList(), typeParameterNames, s => errorReporter("Error in literal code pattern: " + s));

			var fmt = new StringBuilder();
			var fmtargs = new List<JsExpression>();
			foreach (var token in tokens) {
				switch (token.Type) {
					case InlineCodeToken.TokenType.Text:
						fmt.Append(Escape(token.Text));
						break;

					case InlineCodeToken.TokenType.This:
						if (@this == null) {
							errorReporter("Cannot use {this} in the literal code for a static method");
						}
						else {
							fmt.Append(CreatePlaceholder(fmtargs.Count));
							fmtargs.Add(@this);
						}
						break;
					
					case InlineCodeToken.TokenType.Parameter:
						fmt.Append(CreatePlaceholder(fmtargs.Count));
						fmtargs.Add(arguments[token.Index]);
						break;

					case InlineCodeToken.TokenType.TypeParameter:
						fmt.Append(CreatePlaceholder(fmtargs.Count));
						fmtargs.Add(getType(typeArguments[token.Index].ToTypeReference()));
						break;

					case InlineCodeToken.TokenType.TypeRef:
						var typeRef = getType(ReflectionHelper.ParseReflectionName(token.Text));
						if (typeRef == null) {
							errorReporter("Unknown type '" + token.Text + "' specified in inline implementation.");
						}
						else {
							fmt.Append(CreatePlaceholder(fmtargs.Count));
							fmtargs.Add(typeRef);
						}
						break;

					case InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier: {
						var jce = arguments[token.Index] as JsConstantExpression;
						if (jce != null && jce.NodeType == ExpressionNodeType.String) {
							fmt.Append(Escape(jce.StringValue));
						}
						else {
							errorReporter("The argument specified for parameter " + method.Parameters[token.Index].Name + " must be a literal string.");
						}
						break;
					}

					case InlineCodeToken.TokenType.ExpandedParamArrayParameter:
					case InlineCodeToken.TokenType.ExpandedParamArrayParameterWithCommaBefore: {
						if (!isParamArrayExpanded) {
							errorReporter("The method " + method.DeclaringType.FullName + "." + method.FullName + " can only be invoked with its params parameter expanded.");
						}
						else {
							var arr = (JsArrayLiteralExpression)arguments[token.Index];
							for (int i = 0; i < arr.Elements.Count; i++) {
								if (i > 0 || token.Type == InlineCodeToken.TokenType.ExpandedParamArrayParameterWithCommaBefore)
									fmt.Append(", ");
								fmt.Append(CreatePlaceholder(fmtargs.Count));
								fmtargs.Add(arr.Elements[i]);
							}
						}
						break;
					}
					default:
						throw new ArgumentException("Unknown token type " + token.Type);
				}
			}

			return JsExpression.Literal(fmt.ToString(), fmtargs);
		}

		public static IList<string> ValidateLiteralCode(IMethod method, string literalCode, Func<ITypeReference, bool> doesTypeExist) {
			List<string> typeParameterNames = new List<string>();
			typeParameterNames.AddRange(method.DeclaringType.GetDefinition().TypeParameters.Select(p => p.Name));
			typeParameterNames.AddRange(method.TypeParameters.Select(p => p.Name));

			IList<string> parameterNames = method.Parameters.Count > 0 ? method.Parameters.Select(p => p.Name).ToList() : null;

			var result = new List<string>();
			var tokens = Tokenize(literalCode, parameterNames, typeParameterNames, s => result.Add(s));

			foreach (var token in tokens) {
				switch (token.Type) {
					case InlineCodeToken.TokenType.Text:
					case InlineCodeToken.TokenType.Parameter:
					case InlineCodeToken.TokenType.TypeParameter:
						// Can't be bad.
						break;

					case InlineCodeToken.TokenType.This:
						if (method.IsStatic)
							result.Add("Cannot use the placeholder {this} in inline code for a static method.");
						break;

					case InlineCodeToken.TokenType.TypeRef:
						if (!doesTypeExist(ReflectionHelper.ParseReflectionName(token.Text)))
							result.Add("Cannot find the type '" + token.Text + "'");
						break;

					case InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier: {
						if (method.Parameters[token.Index].Type.FullName != "System.String")
							result.Add("The type of the parameter " + method.Parameters[token.Index].Name + " must be string in order to use it with the '@' modifier.");
						break;
					}

					case InlineCodeToken.TokenType.ExpandedParamArrayParameter:
					case InlineCodeToken.TokenType.ExpandedParamArrayParameterWithCommaBefore: {
						if (!method.Parameters[token.Index].IsParams)
							result.Add("The parameter " + method.Parameters[token.Index].Name + " must be a param array in order to use it with the '" + (token.Type == InlineCodeToken.TokenType.ExpandedParamArrayParameterWithCommaBefore ? "," : "*") + "' modifier.");
						break;
					}
					default:
						throw new ArgumentException("Unknown token type " + token.Type);
				}
			}
			return result;
		}
	}
}
