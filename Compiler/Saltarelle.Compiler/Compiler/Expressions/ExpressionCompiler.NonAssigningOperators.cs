using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private JsExpression CompileBinaryNonAssigningOperator(ExpressionSyntax left, ExpressionSyntax right, Func<JsExpression, JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = InnerCompile(right, false, ref jsLeft);
			var result = resultFactory(jsLeft, jsRight);
			return isLifted ? _runtimeLibrary.Lift(result, this) : result;
		}

		private JsExpression CompileUnaryOperator(ExpressionSyntax operand, Func<JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsOperand = InnerCompile(operand, false);
			var result = resultFactory(jsOperand);
			return isLifted ? _runtimeLibrary.Lift(result, this) : result;
		}

		private JsExpression CompileConditionalOperator(ExpressionSyntax test, ExpressionSyntax truePath, ExpressionSyntax falsePath) {
			var jsTest      = Visit(test, true, _returnMultidimArrayValueByReference);
			var trueResult  = CloneAndCompile(truePath, true);
			var falseResult = CloneAndCompile(falsePath, true);

			if (trueResult.AdditionalStatements.Count > 0 || falseResult.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable();
				var trueBlock  = JsStatement.Block(trueResult.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), trueResult.Expression) }));
				var falseBlock = JsStatement.Block(falseResult.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), falseResult.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, null));
				_additionalStatements.Add(JsStatement.If(jsTest, trueBlock, falseBlock));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return JsExpression.Conditional(jsTest, trueResult.Expression, falseResult.Expression);
			}
		}

		private JsExpression CompileCoalesce(ExpressionSyntax left, ExpressionSyntax right) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = CloneAndCompile(right, true);
			var leftType = _semanticModel.GetTypeInfo(left).Type;

			if (jsRight.AdditionalStatements.Count == 0 && !CanTypeBeFalsy(leftType)) {
				return JsExpression.LogicalOr(jsLeft, jsRight.Expression);
			}
			else if (jsRight.AdditionalStatements.Count == 0 && (jsRight.Expression.NodeType == ExpressionNodeType.Identifier || (jsRight.Expression.NodeType >= ExpressionNodeType.ConstantFirst && jsRight.Expression.NodeType <= ExpressionNodeType.ConstantLast))) {
				return _runtimeLibrary.Coalesce(jsLeft, jsRight.Expression, this);
			}
			else {
				var temp = _createTemporaryVariable();
				var nullBlock  = JsStatement.Block(jsRight.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsLeft));
				_additionalStatements.Add(JsStatement.If(_runtimeLibrary.ReferenceEquals(JsExpression.Identifier(_variables[temp].Name), JsExpression.Null, this), nullBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
		}

		private bool CanTypeBeFalsy(ITypeSymbol type) {
			type = type.UnpackNullable();
			return IsIntegerType(type) || type.SpecialType == SpecialType.System_Single || type.SpecialType == SpecialType.System_Double || type.SpecialType == SpecialType.System_Decimal || type.SpecialType == SpecialType.System_Boolean || type.SpecialType == SpecialType.System_String // Numbers, boolean and string have falsy values that are not null...
			    || type.TypeKind == TypeKind.Enum || type.TypeKind == TypeKind.Dynamic // ... so do enum types...
			    || type.SpecialType == SpecialType.System_Object || type.SpecialType == SpecialType.System_ValueType || type.SpecialType == SpecialType.System_Enum; // These reference types might contain types that have falsy values, so we need to be safe.
		}

		private JsExpression CompileAndAlsoOrOrElse(ExpressionSyntax left, ExpressionSyntax right, bool isAndAlso) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = CloneAndCompile(right, true);
			if (jsRight.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable();
				var ifBlock = JsStatement.Block(jsRight.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsLeft));
				JsExpression test = JsExpression.Identifier(_variables[temp].Name);
				if (!isAndAlso)
					test = JsExpression.LogicalNot(test);
				_additionalStatements.Add(JsStatement.If(test, ifBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return isAndAlso ? JsExpression.LogicalAnd(jsLeft, jsRight.Expression) : JsExpression.LogicalOr(jsLeft, jsRight.Expression);
			}
		}

		private bool CanDoSimpleComparisonForEquals(ExpressionSyntax a, ExpressionSyntax b) {
			var tiA = _semanticModel.GetTypeInfo(a);
			var tiB = _semanticModel.GetTypeInfo(b);
			var typeA = tiA.ConvertedType;
			var typeB = tiB.ConvertedType;

			if (typeA != null && typeA.IsNullable()) {
				// in an expression such as myNullableInt == 3, an implicit nullable conversion is performed on the non-nullable value, but we can know for sure that it will never be null.
				var ca = _semanticModel.GetConversion(a);
				if (ca.IsNullable && ca.IsImplicit)
					typeA = tiA.Type;
			}

			if (typeB != null && typeB.IsNullable()) {
				var cb = _semanticModel.GetConversion(b);
				if (cb.IsNullable && cb.IsImplicit)
					typeB = tiB.Type;
			}
			
			bool aCanBeNull = typeA == null || !typeA.IsValueType || typeA.IsNullable();
			bool bCanBeNull = typeB == null || !typeB.IsValueType || typeB.IsNullable();
			return !aCanBeNull || !bCanBeNull;
		}
	}
}
