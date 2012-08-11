using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel
{
    public class OutputFormatter : IExpressionVisitor<object, bool>, IStatementVisitor<object, bool> {
    	private readonly bool _allowIntermediates;
    	private CodeBuilder _cb = new CodeBuilder();

        private OutputFormatter(bool allowIntermediates) {
			_allowIntermediates = allowIntermediates;
		}

    	public static string Format(JsExpression expression, bool allowIntermediates = false) {
            var fmt = new OutputFormatter(allowIntermediates);
            fmt.VisitExpression(expression, false);
            return fmt._cb.ToString();
        }

        public static string Format(JsStatement statement, bool allowIntermediates = false) {
            var fmt = new OutputFormatter(allowIntermediates);
            fmt.VisitStatement(statement, true);
            return fmt._cb.ToString();
        }

        public object VisitExpression(JsExpression expression, bool parenthesized) {
            if (parenthesized)
                _cb.Append("(");
            expression.Accept(this, parenthesized);
            if (parenthesized)
                _cb.Append(")");
            return null;
        }

        private void VisitExpressionList(IEnumerable<JsExpression> expressions) {
            bool first = true;
            foreach (var x in expressions) {
                if (!first)
                    _cb.Append(", ");
                VisitExpression(x, GetPrecedence(x.NodeType) >= PrecedenceComma); // We need to parenthesize comma expressions, eg. [1, (2, 3), 4]
                first = false;
            }
        }

        public object VisitArrayLiteralExpression(JsArrayLiteralExpression expression, bool parenthesized) {
            _cb.Append("[");
            VisitExpressionList(expression.Elements);
            _cb.Append("]");
            return null;
        }

        public object VisitBinaryExpression(JsBinaryExpression expression, bool parenthesized) {
            int expressionPrecedence = GetPrecedence(expression.NodeType);
            if (expression.NodeType == ExpressionNodeType.Index) {
                VisitExpression(expression.Left, GetPrecedence(expression.Left.NodeType) > expressionPrecedence);
                _cb.Append("[");
                VisitExpression(expression.Right, false);
                _cb.Append("]");
            }
            else {
                bool isRightAssociative = expression.NodeType >= ExpressionNodeType.AssignFirst && expression.NodeType <= ExpressionNodeType.AssignLast;

                VisitExpression(expression.Left, GetPrecedence(expression.Left.NodeType) > expressionPrecedence - (isRightAssociative ? 1 : 0));
                _cb.Append(" ").Append(GetBinaryOperatorString(expression.NodeType)).Append(" ");
                VisitExpression(expression.Right, GetPrecedence(expression.Right.NodeType) > expressionPrecedence - (isRightAssociative ? 0 : 1));
            }
            return null;
        }

        public object VisitCommaExpression(JsCommaExpression expression, bool parenthesized) {
            int expressionPrecedence = GetPrecedence(expression.NodeType);
            for (int i = 0; i < expression.Expressions.Count; i++) {
                if (i > 0)
                    _cb.Append(", ");
            	VisitExpression(expression.Expressions[i], GetPrecedence(expression.Expressions[i].NodeType) > expressionPrecedence);
            }
            return null;
        }

        public object VisitConditionalExpression(JsConditionalExpression expression, bool parenthesized) {
            // Always parenthesize conditionals (but beware of double parentheses). Better this than accidentally getting the tricky precedence wrong sometimes.
            if (!parenthesized)
                _cb.Append("(");

            // Also, be rather liberal when parenthesizing the operands, partly to avoid bugs, partly for readability.
            VisitExpression(expression.Test, GetPrecedence(expression.Test.NodeType) >= PrecedenceMultiply);
            _cb.Append(" ? ");
            VisitExpression(expression.TruePart, GetPrecedence(expression.TruePart.NodeType) >= PrecedenceMultiply);
            _cb.Append(" : ");
            VisitExpression(expression.FalsePart, GetPrecedence(expression.FalsePart.NodeType) >= PrecedenceMultiply);

            if (!parenthesized)
                _cb.Append(")");

            return null;
        }

        public object VisitConstantExpression(JsConstantExpression expression, bool parenthesized) {
            switch (expression.NodeType) {
                case ExpressionNodeType.Null:
                    _cb.Append("null");
                    break;
                case ExpressionNodeType.Number:
                    _cb.Append(expression.NumberValue.ToString(CultureInfo.InvariantCulture));
                    break;
                case ExpressionNodeType.Regexp:
                    _cb.Append("/" + expression.RegexpValue.Pattern.EscapeJavascriptStringLiteral(true) + "/" + expression.RegexpValue.Options);
                    break;
                case ExpressionNodeType.String:
                    _cb.Append("'" + expression.StringValue.EscapeJavascriptStringLiteral() + "'");
                    break;
				case ExpressionNodeType.Boolean:
					_cb.Append(expression.BooleanValue ? "true" : "false");
					break;
                default:
                    throw new ArgumentException("expression");
            }
            return null;
        }

        public object VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, bool parenthesized) {
            _cb.Append("function");
            if (expression.Name != null)
                _cb.Append(" ").Append(expression.Name);
            _cb.Append("(");

            bool first = true;
            foreach (var arg in expression.ParameterNames) {
                if (!first)
                    _cb.Append(", ");
                _cb.Append(arg);
                first = false;
            }
            _cb.Append(") ");
			VisitStatement(expression.Body, false);

            return null;
        }

        public object VisitIdentifierExpression(JsIdentifierExpression expression, bool parenthesized) {
            _cb.Append(expression.Name);
            return null;
        }

        public object VisitInvocationExpression(JsInvocationExpression expression, bool parenthesized) {
            VisitExpression(expression.Method, GetPrecedence(expression.Method.NodeType) > GetPrecedence(expression.NodeType) || (expression.Method is JsNewExpression)); // Ugly code to make sure that we put parentheses around "new", eg. "(new X())(1)" rather than "new X()(1)"
            _cb.Append("(");
            VisitExpressionList(expression.Arguments);
            _cb.Append(")");
            return null;
        }

        public object VisitObjectLiteralExpression(JsObjectLiteralExpression expression, bool parenthesized) {
            if (expression.Values.Count == 0) {
                _cb.Append("{}");
            }
            else {
				bool multiline = expression.Values.Any(p => p.Value is JsFunctionDefinitionExpression);
				if (multiline)
					_cb.AppendLine("{").Indent();
				else
					_cb.Append("{ ");

                bool first = true;
                foreach (var v in expression.Values) {
                    if (!first) {
						if (multiline)
							_cb.AppendLine(",");
						else
							_cb.Append(", ");
					}
                    _cb.Append(v.Name.IsValidJavaScriptIdentifier() ? v.Name : ("'" + v.Name.EscapeJavascriptStringLiteral() + "'"))
                       .Append(": ");
                    VisitExpression(v.Value, GetPrecedence(v.Value.NodeType) >= PrecedenceComma); // We ned to parenthesize comma expressions, eg. [1, (2, 3), 4]
                    first = false;
                }
				if (multiline)
					_cb.AppendLine().Outdent().Append("}");
				else
	                _cb.Append(" }");
            }
            return null;
        }

        public object VisitMemberAccessExpression(JsMemberAccessExpression expression, bool parenthesized) {
            VisitExpression(expression.Target, (GetPrecedence(expression.Target.NodeType) > GetPrecedence(expression.NodeType)) && expression.Target.NodeType != ExpressionNodeType.MemberAccess && expression.Target.NodeType != ExpressionNodeType.Invocation); // Ugly code to ensure that nested member accesses are not parenthesized, but member access nested in new are (and vice versa)
            _cb.Append(".");
            _cb.Append(expression.Member);
            return null;
        }

        public object VisitNewExpression(JsNewExpression expression, bool parenthesized) {
            _cb.Append("new ");
            VisitExpression(expression.Constructor, GetPrecedence(expression.Constructor.NodeType) >= GetPrecedence(expression.NodeType));
            _cb.Append("(");
            VisitExpressionList(expression.Arguments);
            _cb.Append(")");
            return null;
        }

        public object VisitUnaryExpression(JsUnaryExpression expression, bool parenthesized) {
            string prefix = "", postfix = "";
            bool alwaysParenthesize = false;
            switch (expression.NodeType) {
                case ExpressionNodeType.PrefixPlusPlus:        prefix = "++"; break;
                case ExpressionNodeType.PrefixMinusMinus:      prefix = "--"; break;
                case ExpressionNodeType.PostfixPlusPlus:       postfix = "++"; break;
                case ExpressionNodeType.PostfixMinusMinus:     postfix = "--"; break;
                case ExpressionNodeType.LogicalNot:            prefix = "!"; break;
                case ExpressionNodeType.BitwiseNot:            prefix = "~"; break;
                case ExpressionNodeType.Positive:              prefix = "+"; break;
                case ExpressionNodeType.Negate:                prefix = "-"; break;
                case ExpressionNodeType.TypeOf:                prefix = "typeof"; alwaysParenthesize = true; break;
                case ExpressionNodeType.Void:                  prefix = "void"; alwaysParenthesize = true; break;
                case ExpressionNodeType.Delete:                prefix = "delete "; break;
                default: throw new ArgumentException("expression");
            }
            _cb.Append(prefix);
            VisitExpression(expression.Operand, (GetPrecedence(expression.Operand.NodeType) >= PrecedenceIncrDecr) || alwaysParenthesize);
            _cb.Append(postfix);
            return null;
        }

        public object VisitTypeReferenceExpression(JsTypeReferenceExpression expression, bool parenthesized) {
			if (!_allowIntermediates)
				throw new NotSupportedException("TypeReferenceExpressions should not occur in the output stage");
			int lastDot = expression.TypeName.LastIndexOf('.');
			string name = (lastDot >= 0 ? expression.TypeName.Substring(lastDot + 1) : expression.TypeName);
			_cb.Append("{").Append(name).Append("}");
			return null;
        }

        public object VisitThisExpression(JsThisExpression expression, bool parenthesized) {
            _cb.Append("this");
			return null;
        }

		public object VisitLiteralExpression(JsLiteralExpression expression, bool parenthesized) {
            int expressionPrecedence = GetPrecedence(expression.NodeType);
			var oldCB = _cb;
			var arguments = new string[expression.Arguments.Count];
			for (int i = 0; i < expression.Arguments.Count; i++) {
				_cb = new CodeBuilder(indentLevel: oldCB.IndentLevel);
				_cb.PreventIndent();
				VisitExpression(expression.Arguments[i], GetPrecedence(expression.Arguments[i].NodeType) > expressionPrecedence);
				arguments[i] = _cb.ToString();
			}
			_cb = oldCB;

			_cb.Append(string.Format(expression.Format, arguments));

			return null;
		}

        private static string GetBinaryOperatorString(ExpressionNodeType oper) {
            switch (oper) {
                case ExpressionNodeType.Multiply:                 return "*";
                case ExpressionNodeType.Divide:                   return "/";
                case ExpressionNodeType.Modulo:                   return "%";
                case ExpressionNodeType.Add:                      return "+";
                case ExpressionNodeType.Subtract:                 return "-";
                case ExpressionNodeType.LeftShift:                return "<<";
                case ExpressionNodeType.RightShiftSigned:         return ">>";
                case ExpressionNodeType.RightShiftUnsigned:       return ">>>";
                case ExpressionNodeType.Lesser:                   return "<";
                case ExpressionNodeType.LesserOrEqual:            return "<=";
                case ExpressionNodeType.Greater:                  return ">";
                case ExpressionNodeType.GreaterOrEqual:           return ">=";
                case ExpressionNodeType.In:                       return "in";
                case ExpressionNodeType.InstanceOf:               return "instanceof";
                case ExpressionNodeType.Equal:                    return "==";
                case ExpressionNodeType.NotEqual:                 return "!=";
                case ExpressionNodeType.Same:                     return "===";
                case ExpressionNodeType.NotSame:                  return "!==";
                case ExpressionNodeType.BitwiseAnd:               return "&";
                case ExpressionNodeType.BitwiseXor:               return "^";
                case ExpressionNodeType.BitwiseOr:                return "|";
                case ExpressionNodeType.LogicalAnd:               return "&&";
                case ExpressionNodeType.LogicalOr:                return "||";
                case ExpressionNodeType.Assign:                   return "=";
                case ExpressionNodeType.MultiplyAssign:           return "*=";
                case ExpressionNodeType.DivideAssign:             return "/=";
                case ExpressionNodeType.ModuloAssign:             return "%=";
                case ExpressionNodeType.AddAssign:                return "+=";
                case ExpressionNodeType.SubtractAssign:           return "-=";
                case ExpressionNodeType.LeftShiftAssign:          return "<<=";
                case ExpressionNodeType.RightShiftSignedAssign:   return ">>=";
                case ExpressionNodeType.RightShiftUnsignedAssign: return ">>>=";
                case ExpressionNodeType.BitwiseAndAssign:         return "&=";
                case ExpressionNodeType.BitwiseOrAssign:          return "|=";
                case ExpressionNodeType.BitwiseXOrAssign:         return "^=";
                case ExpressionNodeType.Index:
                default:
                    throw new InvalidOperationException("Invalid operator " + oper.ToString());
            }
        }

        private const int PrecedenceTerminal                = 0;
        private const int PrecedenceMemberOrNewOrInvocation = PrecedenceTerminal                + 1;
        private const int PrecedenceFunctionDefinition      = PrecedenceMemberOrNewOrInvocation + 1; // The function definition precedence is kind of strange. function() {}(x) does not invoke the function, although I guess this is due to semicolon insertion rather than precedence. Cheating with the precedence solves the problem.
        private const int PrecedenceIncrDecr                = PrecedenceFunctionDefinition      + 1;
        private const int PrecedenceOtherUnary              = PrecedenceIncrDecr                + 1;
        private const int PrecedenceMultiply                = PrecedenceOtherUnary              + 1;
        private const int PrecedenceAddition                = PrecedenceMultiply                + 1;
        private const int PrecedenceBitwiseShift            = PrecedenceAddition                + 1;
        private const int PrecedenceRelational              = PrecedenceBitwiseShift            + 1;
        private const int PrecedenceEquality                = PrecedenceRelational              + 1;
        private const int PrecedenceBitwiseAnd              = PrecedenceEquality                + 1;
        private const int PrecedenceBitwiseXor              = PrecedenceBitwiseAnd              + 1;
        private const int PrecedenceBitwiseOr               = PrecedenceBitwiseXor              + 1;
        private const int PrecedenceLogicalAnd              = PrecedenceBitwiseOr               + 1;
        private const int PrecedenceLogicalOr               = PrecedenceLogicalAnd              + 1;
        private const int PrecedenceConditional             = PrecedenceLogicalOr               + 1;
        private const int PrecedenceAssignment              = PrecedenceConditional             + 1;
        private const int PrecedenceComma                   = PrecedenceAssignment              + 1;
		private const int PrecedenceLiteral                 = PrecedenceComma                   + 1;

        private static int GetPrecedence(ExpressionNodeType nodeType) {
            switch (nodeType) {
                case ExpressionNodeType.ArrayLiteral:
                    return PrecedenceTerminal;

                case ExpressionNodeType.LogicalAnd:
                    return PrecedenceLogicalAnd;

                case ExpressionNodeType.LogicalOr:
                    return PrecedenceLogicalOr;

                case ExpressionNodeType.NotEqual:
                case ExpressionNodeType.Equal:
                case ExpressionNodeType.Same:
                case ExpressionNodeType.NotSame:
                    return PrecedenceEquality;

                case ExpressionNodeType.LesserOrEqual:
                case ExpressionNodeType.GreaterOrEqual:
                case ExpressionNodeType.Lesser:
                case ExpressionNodeType.Greater:
                case ExpressionNodeType.InstanceOf:
                case ExpressionNodeType.In:
                    return PrecedenceRelational;

                case ExpressionNodeType.Subtract:
                case ExpressionNodeType.Add:
                    return PrecedenceAddition;

                case ExpressionNodeType.Modulo:
                case ExpressionNodeType.Divide:
                case ExpressionNodeType.Multiply:
                    return PrecedenceMultiply;

                case ExpressionNodeType.BitwiseAnd:
                    return PrecedenceBitwiseAnd;

                case ExpressionNodeType.BitwiseOr:
                    return PrecedenceBitwiseOr;

                case ExpressionNodeType.BitwiseXor:
                    return PrecedenceBitwiseXor;

                case ExpressionNodeType.LeftShift:
                case ExpressionNodeType.RightShiftSigned:
                case ExpressionNodeType.RightShiftUnsigned:
                    return PrecedenceBitwiseShift;

                case ExpressionNodeType.Assign:
                case ExpressionNodeType.MultiplyAssign:
                case ExpressionNodeType.DivideAssign:
                case ExpressionNodeType.ModuloAssign:
                case ExpressionNodeType.AddAssign:
                case ExpressionNodeType.SubtractAssign:
                case ExpressionNodeType.LeftShiftAssign:
                case ExpressionNodeType.RightShiftSignedAssign:
                case ExpressionNodeType.RightShiftUnsignedAssign:
                case ExpressionNodeType.BitwiseAndAssign:
                case ExpressionNodeType.BitwiseOrAssign:
                case ExpressionNodeType.BitwiseXOrAssign:
                    return PrecedenceAssignment;

                case ExpressionNodeType.Comma:
                    return PrecedenceComma;

                case ExpressionNodeType.Conditional:
                    return PrecedenceConditional;

                case ExpressionNodeType.Number:
                case ExpressionNodeType.String:
                case ExpressionNodeType.Regexp:
                case ExpressionNodeType.Null:
				case ExpressionNodeType.Boolean:
                    return PrecedenceTerminal;

                case ExpressionNodeType.FunctionDefinition:
                    return PrecedenceFunctionDefinition;

                case ExpressionNodeType.Identifier:
				case ExpressionNodeType.This:
                    return PrecedenceTerminal;

                case ExpressionNodeType.MemberAccess:
					return PrecedenceTerminal;

                case ExpressionNodeType.New:
                case ExpressionNodeType.Index:
				case ExpressionNodeType.Invocation:
                    return PrecedenceMemberOrNewOrInvocation;

                case ExpressionNodeType.ObjectLiteral:
                    return PrecedenceTerminal;

                case ExpressionNodeType.PrefixPlusPlus:
                case ExpressionNodeType.PrefixMinusMinus:
                case ExpressionNodeType.PostfixPlusPlus:
                case ExpressionNodeType.PostfixMinusMinus:
                    return PrecedenceIncrDecr;

                case ExpressionNodeType.TypeOf:
                case ExpressionNodeType.LogicalNot:
                case ExpressionNodeType.Negate:
                case ExpressionNodeType.Positive:
                case ExpressionNodeType.Delete:
                case ExpressionNodeType.Void:
                case ExpressionNodeType.BitwiseNot:
                    return PrecedenceOtherUnary;

				case ExpressionNodeType.TypeReference:
					return PrecedenceTerminal;
					
				case ExpressionNodeType.Literal:
					return PrecedenceLiteral;

                default:
                    throw new ArgumentException("nodeType");
            }
        }

    	public object VisitStatement(JsStatement statement, bool addNewline) {
			return statement.Accept(this, addNewline);
    	}

    	public object VisitComment(JsComment comment, bool data) {
			foreach (var l in comment.Text.Replace("\r", "").Split('\n'))
				_cb.AppendLine("//" + l);
			return null;
    	}

    	public object VisitBlockStatement(JsBlockStatement statement, bool addNewline) {
			_cb.AppendLine("{").Indent();
			foreach (var c in statement.Statements)
				VisitStatement(c, true);
			_cb.Outdent().Append("}");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitBreakStatement(JsBreakStatement statement, bool addNewline) {
			_cb.Append("break");
			if (statement.TargetLabel != null)
				_cb.Append(" ").Append(statement.TargetLabel);
			_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitContinueStatement(JsContinueStatement statement, bool addNewline) {
			_cb.Append("continue");
			if (statement.TargetLabel != null)
				_cb.Append(" ").Append(statement.TargetLabel);
			_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitDoWhileStatement(JsDoWhileStatement statement, bool addNewline) {
    		_cb.Append("do ");
			VisitStatement(statement.Body, false);
			_cb.Append(" while (");
			VisitExpression(statement.Condition, false);
			_cb.Append(");");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitEmptyStatement(JsEmptyStatement statement, bool addNewline) {
    		_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitExpressionStatement(JsExpressionStatement statement, bool addNewline) {
    		VisitExpression(statement.Expression, false);
			_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitForEachInStatement(JsForEachInStatement statement, bool addNewline) {
    		_cb.Append("for (");
			if (statement.IsLoopVariableDeclared)
				_cb.Append("var ");
			_cb.Append(statement.LoopVariableName)
			   .Append(" in ");
			VisitExpression(statement.ObjectToIterateOver, false);
			_cb.Append(") ");
			VisitStatement(statement.Body, addNewline);
			return null;
    	}

    	public object VisitForStatement(JsForStatement statement, bool addNewline) {
    		_cb.Append("for (");
			VisitStatement(statement.InitStatement, false);

			if (statement.ConditionExpression != null) {
				_cb.Append(" ");
				VisitExpression(statement.ConditionExpression, false);
			}
			_cb.Append(";");

			if (statement.IteratorExpression != null) {
				_cb.Append(" ");
				VisitExpression(statement.IteratorExpression, false);
			}
			_cb.Append(") ");
			VisitStatement(statement.Body, addNewline);
			return null;
    	}

    	public object VisitIfStatement(JsIfStatement statement, bool addNewline) {
redo:
			_cb.Append("if (");
			VisitExpression(statement.Test, false);
			_cb.Append(") ");
			VisitStatement(statement.Then, statement.Else != null || addNewline);
			if (statement.Else != null) {
				_cb.Append("else ");
				if (statement.Else.Statements.Count == 1 && statement.Else.Statements[0] is JsIfStatement) {
					statement = (JsIfStatement)statement.Else.Statements[0];
					goto redo;
				}
			}

			if (statement.Else != null)
				VisitStatement(statement.Else, addNewline);

			return null;
    	}

    	public object VisitReturnStatement(JsReturnStatement statement, bool addNewline) {
    		_cb.Append("return");
			if (statement.Value != null) {
				_cb.Append(" ");
				VisitExpression(statement.Value, false);
			}
			_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitSwitchStatement(JsSwitchStatement statement, bool addNewline) {
    		_cb.Append("switch (");
			VisitExpression(statement.Expression, false);
			_cb.AppendLine(") {").Indent();
			foreach (var clause in statement.Sections) {
				bool first = true;
				foreach (var v in clause.Values) {
					if (!first)
						_cb.AppendLine();
					if (v != null) {
						_cb.Append("case ");
						VisitExpression(v, false);
						_cb.Append(":");
					}
					else {
						_cb.Append("default:");
					}
					first = false;
				}
				_cb.Append(" ");
				VisitStatement(clause.Body, true);
			}
			_cb.Outdent().Append("}");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitThrowStatement(JsThrowStatement statement, bool addNewline) {
    		_cb.Append("throw ");
			VisitExpression(statement.Expression, false);
			_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitTryStatement(JsTryStatement statement, bool addNewline) {
			_cb.Append("try ");
			VisitStatement(statement.GuardedStatement, true);
			if (statement.Catch != null) {
				_cb.AppendFormat("catch ({0}) ", statement.Catch.Identifier);
				VisitStatement(statement.Catch.Body, addNewline || statement.Finally != null);
			}
			if (statement.Finally != null) {
				_cb.AppendFormat("finally ");
				VisitStatement(statement.Finally, addNewline);
			}
			return null;
    	}

    	public object VisitVariableDeclarationStatement(JsVariableDeclarationStatement statement, bool addNewline) {
    		_cb.Append("var ");
			bool first = true;
			foreach (var d in statement.Declarations) {
				if (!first)
					_cb.Append(", ");
				_cb.Append(d.Name);
				if (d.Initializer != null) {
					_cb.Append(" = ");
					this.VisitExpression(d.Initializer, false);
				}
				first = false;
			}
			_cb.Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitWhileStatement(JsWhileStatement statement, bool addNewline) {
			_cb.Append("while (");
			VisitExpression(statement.Condition, false);
			_cb.Append(") ");
			VisitStatement(statement.Body, addNewline);
			return null;
    	}

    	public object VisitWithStatement(JsWithStatement statement, bool data) {
    		throw new NotImplementedException();
    	}

    	public object VisitLabelledStatement(JsLabelledStatement statement, bool addNewline) {
    		_cb.Append(statement.Label).AppendLine(":");
			VisitStatement(statement.Statement, addNewline);
			return null;
    	}

    	public object VisitFunctionStatement(JsFunctionStatement statement, bool addNewline) {
			_cb.Append("function " + statement.Name + "(");
			for (int i = 0; i < statement.ParameterNames.Count; i++) {
				if (i != 0)
					_cb.Append(", ");
				_cb.Append(statement.ParameterNames[i]);
			}
			_cb.Append(") ");
			VisitStatement(statement.Body, addNewline);
			return null;
		}

    	public object VisitGotoStatement(JsGotoStatement statement, bool addNewline) {
    		_cb.Append("goto ").Append(statement.TargetLabel).Append(";");
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}

    	public object VisitYieldStatement(JsYieldStatement statement, bool addNewline) {
			if (!_allowIntermediates)
				throw new NotSupportedException("yield return should not occur in the output stage");
			if (statement.Value != null) {
				_cb.Append("yield return ");
				VisitExpression(statement.Value, false);
				_cb.Append(";");
			}
			else {
				_cb.Append("yield break;");
			}
			if (addNewline)
				_cb.AppendLine();
			return null;
    	}
    }
}
