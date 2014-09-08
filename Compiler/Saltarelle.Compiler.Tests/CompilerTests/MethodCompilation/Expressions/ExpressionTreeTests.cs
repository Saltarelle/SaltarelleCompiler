using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class ExpressionTreeTests : MethodCompilerTestBase {
		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => Common.LoadAssemblyFile(typeof(object).Assembly.Location));

		private static readonly Lazy<MetadataReference> _expressionAssembly = new Lazy<MetadataReference>(() => {
			var c = Common.CreateCompilation(@"
using System.Collections.Generic;
using System.Reflection;

namespace System.Linq.Expressions {
	public class Expression {
		public static Expression Assign(Expression left, Expression right, Type type) { return null; }
		public static Expression Equal(Expression left, Expression right, Type type) { return null; }
		public static Expression Equal(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ReferenceEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression NotEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression NotEqual(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ReferenceNotEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression GreaterThan(Expression left, Expression right, Type type) { return null; }
		public static Expression GreaterThan(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LessThan(Expression left, Expression right, Type type) { return null; }
		public static Expression LessThan(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression GreaterThanOrEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression GreaterThanOrEqual(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LessThanOrEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression LessThanOrEqual(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AndAlso(Expression left, Expression right, Type type) { return null; }
		public static Expression AndAlso(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression OrElse(Expression left, Expression right, Type type) { return null; }
		public static Expression OrElse(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression Coalesce(Expression left, Expression right, Type type) { return null; }
		public static Expression Coalesce(Expression left, Expression right, LambdaExpression conversion, Type type) { return null; }
		public static Expression Add(Expression left, Expression right, Type type) { return null; }
		public static Expression Add(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AddAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression AddAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AddAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression AddAssignChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression AddAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AddAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression AddChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression AddChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression Subtract(Expression left, Expression right, Type type) { return null; }
		public static Expression Subtract(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression SubtractAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression SubtractAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression SubtractAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression SubtractAssignChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression SubtractAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression SubtractAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression SubtractChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression SubtractChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression Divide(Expression left, Expression right, Type type) { return null; }
		public static Expression Divide(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression DivideAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression DivideAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression DivideAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Modulo(Expression left, Expression right, Type type) { return null; }
		public static Expression Modulo(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ModuloAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression ModuloAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ModuloAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Multiply(Expression left, Expression right, Type type) { return null; }
		public static Expression Multiply(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression MultiplyAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression MultiplyAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression MultiplyAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression MultiplyAssignChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression MultiplyChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression MultiplyChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LeftShift(Expression left, Expression right, Type type) { return null; }
		public static Expression LeftShift(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LeftShiftAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression LeftShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LeftShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression RightShift(Expression left, Expression right, Type type) { return null; }
		public static Expression RightShift(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression RightShiftAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression RightShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression RightShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression And(Expression left, Expression right, Type type) { return null; }
		public static Expression And(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AndAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression AndAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AndAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Or(Expression left, Expression right, Type type) { return null; }
		public static Expression Or(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression OrAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression OrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression OrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression ExclusiveOr(Expression left, Expression right, Type type) { return null; }
		public static Expression ExclusiveOr(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ExclusiveOrAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Power(Expression left, Expression right, Type type) { return null; }
		public static Expression Power(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression PowerAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression PowerAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression PowerAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		public static Expression ArrayIndex(Type type, Expression array, Expression index) { return null; }
		public static Expression ArrayIndex(Type type, Expression array, params Expression[] indexes) { return null; }
		public static Expression ArrayIndex(Type type, Expression array, IEnumerable<Expression> indexes) { return null; }

		public static Expression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) { return null; }

		public static Expression Constant(object value, Type type) { return null; }

		public static Expression Default(Type type) { return null; }

		public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) { return null; }
		public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) { return null; }

		public static Expression MakeIndex(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }
		public static Expression ArrayAccess(Type type, Expression array, params Expression[] indexes) { return null; }
		public static Expression ArrayAccess(Type type, Expression array, IEnumerable<Expression> indexes) { return null; }
		public static Expression Property(Type type, Expression instance, string propertyName, params Expression[] arguments) { return null; }
		public static Expression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) { return null; }
		public static Expression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }

		public static Expression Invoke(Type type, Expression expression, params Expression[] arguments) { return null; }
		public static Expression Invoke(Type type, Expression expression, IEnumerable<Expression> arguments) { return null; }

		public static Expression Lambda(Expression body, params ParameterExpression[] parameters) { return null; }

		public static Expression ListInit(NewExpression newExpression, params Expression[] initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, params ElementInit[] initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers) { return null; }

		public static MemberBinding Bind(MemberInfo member, Expression expression) { return null; }
		public static MemberBinding Bind(MethodInfo propertyAccessor, Expression expression) { return null; }

		public static Expression Field(Expression expression, FieldInfo field) { return null; }
		public static Expression Field(Expression expression, string fieldName) { return null; }
		public static Expression Field(Expression expression, Type type, string fieldName) { return null; }
		public static Expression Property(Expression expression, string propertyName) { return null; }
		public static Expression Property(Expression expression, Type type, string propertyName) { return null; }
		public static Expression Property(Expression expression, PropertyInfo property) { return null; }
		public static Expression Property(Expression expression, MethodInfo propertyAccessor) { return null; }
		public static Expression PropertyOrField(Expression expression, string propertyOrFieldName) { return null; }
		public static Expression MakeMemberAccess(Expression expression, MemberInfo member) { return null; }

		public static Expression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) { return null; }
		public static Expression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) { return null; }

		public static MemberBinding ListBind(MemberInfo member, params ElementInit[] initializers) { return null; }
		public static MemberBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) { return null; }
		public static MemberBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) { return null; }
		public static MemberBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) { return null; }

		public static MemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings) { return null; }
		public static MemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings) { return null; }
		public static MemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings) { return null; }
		public static MemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings) { return null; }

		public static Expression Call(MethodInfo method, Expression arg0) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) { return null; }
		public static Expression Call(MethodInfo method, params Expression[] arguments) { return null; }
		public static Expression Call(MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		public static Expression Call(Expression instance, MethodInfo method) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, params Expression[] arguments) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static Expression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments) { return null; }
		public static Expression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		public static Expression ArrayIndex(Expression array, params Expression[] indexes) { return null; }
		public static Expression ArrayIndex(Expression array, IEnumerable<Expression> indexes) { return null; }

		public static Expression NewArrayInit(Type type, params Expression[] initializers) { return null; }
		public static Expression NewArrayInit(Type type, IEnumerable<Expression> initializers) { return null; }
		public static Expression NewArrayBounds(Type type, params Expression[] bounds) { return null; }
		public static Expression NewArrayBounds(Type type, IEnumerable<Expression> bounds) { return null; }

		public static NewExpression New(ConstructorInfo constructor) { return null; }
		public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members) { return null; }
		public static NewExpression New(ConstructorInfo constructor, Expression[] arguments, params MemberInfo[] members) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members) { return null; }
		public static NewExpression New(Type type) { return null; }

		public static ParameterExpression Parameter(Type type) { return null; }
		public static ParameterExpression Variable(Type type) { return null; }
		public static ParameterExpression Parameter(Type type, string name) { return null; }
		public static ParameterExpression Variable(Type type, string name) { return null; }

		public static Expression TypeIs(Expression expression, Type type) { return null; }
		public static Expression TypeEqual(Expression expression, Type type) { return null; }

		public static Expression MakeUnary(ExpressionType unaryType, Expression operand, Type type) { return null; }
		public static Expression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) { return null; }
		public static Expression Negate(Expression expression, Type type) { return null; }
		public static Expression Negate(Expression expression, MethodInfo method) { return null; }
		public static Expression UnaryPlus(Expression expression, Type type) { return null; }
		public static Expression UnaryPlus(Expression expression, MethodInfo method) { return null; }
		public static Expression NegateChecked(Expression expression, Type type) { return null; }
		public static Expression NegateChecked(Expression expression, MethodInfo method) { return null; }
		public static Expression Not(Expression expression, Type type) { return null; }
		public static Expression Not(Expression expression, MethodInfo method) { return null; }
		public static Expression IsFalse(Expression expression, Type type) { return null; }
		public static Expression IsFalse(Expression expression, MethodInfo method) { return null; }
		public static Expression IsTrue(Expression expression, Type type) { return null; }
		public static Expression IsTrue(Expression expression, MethodInfo method) { return null; }
		public static Expression OnesComplement(Expression expression, Type type) { return null; }
		public static Expression OnesComplement(Expression expression, MethodInfo method) { return null; }
		public static Expression TypeAs(Expression expression, Type type) { return null; }
		public static Expression Unbox(Expression expression, Type type) { return null; }
		public static Expression Convert(Expression expression, Type type) { return null; }
		public static Expression Convert(Expression expression, Type type, MethodInfo method) { return null; }
		public static Expression ConvertChecked(Expression expression, Type type) { return null; }
		public static Expression ConvertChecked(Expression expression, Type type, MethodInfo method) { return null; }
		public static Expression ArrayLength(Expression array) { return null; }
		public static Expression Quote(Expression expression) { return null; }
		public static Expression Increment(Expression expression, Type type) { return null; }
		public static Expression Increment(Expression expression, MethodInfo method) { return null; }
		public static Expression Decrement(Expression expression, Type type) { return null; }
		public static Expression Decrement(Expression expression, MethodInfo method) { return null; }
		public static Expression PreIncrementAssign(Expression expression, Type type) { return null; }
		public static Expression PreIncrementAssign(Expression expression, MethodInfo method) { return null; }
		public static Expression PreDecrementAssign(Expression expression, Type type) { return null; }
		public static Expression PreDecrementAssign(Expression expression, MethodInfo method) { return null; }
		public static Expression PostIncrementAssign(Expression expression, Type type) { return null; }
		public static Expression PostIncrementAssign(Expression expression, MethodInfo method) { return null; }
		public static Expression PostDecrementAssign(Expression expression, Type type) { return null; }
		public static Expression PostDecrementAssign(Expression expression, MethodInfo method) { return null; }
	}
	public enum ExpressionType {}
	public class ParameterExpression : Expression {}
	public class NewExpression : Expression {}
	public class LambdaExpression : Expression {}
	public class Expression<T> : LambdaExpression {}
	public class MemberBinding {}
	public class ElementInit {}
}
			", new[] { _mscorlibLazy.Value }, new string[0]);

			return c.ToMetadataReference();
		});

		private static readonly Lazy<MetadataReference[]> _referencesLazy = new Lazy<MetadataReference[]>(() => new[] { _mscorlibLazy.Value, _expressionAssembly.Value });

		private void AssertCorrect(string csharp, string expected, IRuntimeLibrary runtimeLibrary = null, IMetadataImporter metadataImporter = null, string methodName = "M") {
			base.AssertCorrect("using System; using System.Linq.Expressions; using System.Collections.Generic; class C { " + csharp + "}", expected, references: _referencesLazy.Value, methodName: methodName, metadataImporter: metadataImporter, addSkeleton: false, runtimeLibrary: runtimeLibrary);
		}

		[Test]
		public void CanConvertExpressionLambdaToExpressionTree() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int>> e = a => a;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e = {sm_Expression}.$Lambda($tmp1, [$tmp1]);
");
		}

		[Test]
		public void CanUseUnaryOperators() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int>> e = a => +a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$UnaryPlus($tmp1, {sm_Int32}), [$tmp1]);
");

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int>> e = a => -a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Negate($tmp1, {sm_Int32}), [$tmp1]);
");

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int>> e = a => checked(-a);
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$NegateChecked($tmp1, {sm_Int32}), [$tmp1]);
");

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int>> e = a => ~a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$OnesComplement($tmp1, {sm_Int32}), [$tmp1]);
");

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<bool, bool>> e = a => !a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Boolean}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Not($tmp1, {sm_Boolean}), [$tmp1]);
");
		}

		[Test]
		public void CanUseBinaryOperators() {
			Action<string, string, string, string> testUnchecked = (op, name, resultType, operandType) => {
				AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<OPERAND_TYPE, OPERAND_TYPE, RESULT_TYPE>> e = (a, b) => a OP b;
	// END
}
".Replace("RESULT_TYPE", resultType).Replace("OPERAND_TYPE", operandType).Replace("OP", op),
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_OPERAND_TYPE}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_OPERAND_TYPE}, 'b');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.METHOD_NAME($tmp1, $tmp2, {sm_RESULT_TYPE}), [$tmp1, $tmp2]);
".Replace("METHOD_NAME", "$" + name).Replace("RESULT_TYPE", resultType).Replace("OPERAND_TYPE", operandType));
			};

			Action<string, string> testChecked = (op, name) => {
				AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int, int>> e = (a, b) => checked(a OP b);
	// END
}
".Replace("OP", op),
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.METHOD_NAME($tmp1, $tmp2, {sm_Int32}), [$tmp1, $tmp2]);
".Replace("METHOD_NAME", "$" + name));
			};

			testUnchecked("*", "Multiply", "Int32", "Int32");
			testUnchecked("%", "Modulo", "Int32", "Int32");
			testUnchecked("/", "Divide", "Int32", "Int32");
			testUnchecked("+", "Add", "Int32", "Int32");
			testUnchecked("-", "Subtract", "Int32", "Int32");
			testUnchecked("<<", "LeftShift", "Int32", "Int32");
			testUnchecked(">>", "RightShift", "Int32", "Int32");
			testUnchecked("<", "LessThan", "Boolean", "Int32");
			testUnchecked(">", "GreaterThan", "Boolean", "Int32");
			testUnchecked("<=", "LessThanOrEqual", "Boolean", "Int32");
			testUnchecked(">=", "GreaterThanOrEqual", "Boolean", "Int32");
			testUnchecked("==", "Equal", "Boolean", "Int32");
			testUnchecked("!=", "NotEqual", "Boolean", "Int32");
			testUnchecked("&", "And", "Int32", "Int32");
			testUnchecked("^", "ExclusiveOr", "Int32", "Int32");
			testUnchecked("|", "Or", "Int32", "Int32");
			testUnchecked("&&", "AndAlso", "Boolean", "Boolean");
			testUnchecked("||", "OrElse", "Boolean", "Boolean");
			testChecked("*", "MultiplyChecked");
			testChecked("+", "AddChecked");
			testChecked("-", "SubtractChecked");

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int?, int?, int?>> e1 = (a, b) => a ?? b;
	Expression<Func<int?, int, int>> e2 = (a, b) => a ?? b;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Nullable}, {ga_Int32}), 'a');
	var $tmp2 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Nullable}, {ga_Int32}), 'b');
	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$Coalesce($tmp1, $tmp2, sm_$InstantiateGenericType({Nullable}, {ga_Int32})), [$tmp1, $tmp2]);
	var $tmp3 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Nullable}, {ga_Int32}), 'a');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Coalesce($tmp3, $tmp4, {sm_Int32}), [$tmp3, $tmp4]);
");
		}

		[Test]
		public void CanUseConditionalOperator() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<bool, int, int, int>> e = (a, b, c) => a ? b : c;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Boolean}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, 'c');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Condition($tmp1, $tmp2, $tmp3, {sm_Int32}), [$tmp1, $tmp2, $tmp3]);
");
		}

		[Test]
		public void CanUseUnaryOperatorsWithUserDefinedMethod() {
			AssertCorrect(@"
class X { public static X operator+(X a) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X>> e = a => +a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$UnaryPlus($tmp1, $GetMember({to_X}, 'op_UnaryPlus')), [$tmp1]);
");

			AssertCorrect(@"
class X { public static X operator-(X a) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X>> e = a => -a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Negate($tmp1, $GetMember({to_X}, 'op_UnaryNegation')), [$tmp1]);
");

			AssertCorrect(@"
class X { public static X operator-(X a) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X>> e = a => checked(-a);
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$NegateChecked($tmp1, $GetMember({to_X}, 'op_UnaryNegation')), [$tmp1]);
");

			AssertCorrect(@"
class X { public static X operator~(X a) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X>> e = a => ~a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$OnesComplement($tmp1, $GetMember({to_X}, 'op_OnesComplement')), [$tmp1]);
");

			AssertCorrect(@"
class X { public static X operator!(X a) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X>> e = a => !a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Not($tmp1, $GetMember({to_X}, 'op_LogicalNot')), [$tmp1]);
");
		}

		[Test]
		public void CanUseBinaryOperatorsWithUserDefinedMethod() {
			Action<string, string, string> testUnchecked = (op, name, opName) => {
				AssertCorrect(@"
class X {
	public static X operator *(X a, int b) { return null; }
	public static X operator %(X a, int b) { return null; }
	public static X operator /(X a, int b) { return null; }
	public static X operator +(X a, int b) { return null; }
	public static X operator -(X a, int b) { return null; }
	public static X operator <<(X a, int b) { return null; }
	public static X operator >>(X a, int b) { return null; }
	public static X operator <(X a, int b) { return null; }
	public static X operator >(X a, int b) { return null; }
	public static X operator <=(X a, int b) { return null; }
	public static X operator >=(X a, int b) { return null; }
	public static X operator &(X a, int b) { return null; }
	public static X operator ^(X a, int b) { return null; }
	public static X operator |(X a, int b) { return null; }
}
void M() {
	// BEGIN
	Expression<Func<X, int, X>> e = (a, b) => a OP b;
	// END
}
".Replace("OP", op),
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.METHOD_NAME($tmp1, $tmp2, $GetMember({to_X}, 'OPERATOR_NAME')), [$tmp1, $tmp2]);
".Replace("METHOD_NAME", "$" + name).Replace("OPERATOR_NAME", opName));
			};

			Action<string, string, string> testChecked = (op, name, opName) => {
				AssertCorrect(@"
class X { public static X operator OP(X a, X b) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X, X>> e = (a, b) => checked(a OP b);
	// END
}
".Replace("OP", op),
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_X}, 'b');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.METHOD_NAME($tmp1, $tmp2, $GetMember({to_X}, 'OPERATOR_NAME')), [$tmp1, $tmp2]);
".Replace("METHOD_NAME", "$" + name).Replace("OPERATOR_NAME", opName));
			};

			testUnchecked("*", "Multiply", "op_Multiply");
			testUnchecked("%", "Modulo", "op_Modulus");
			testUnchecked("/", "Divide", "op_Division");
			testUnchecked("+", "Add", "op_Addition");
			testUnchecked("-", "Subtract", "op_Subtraction");
			testUnchecked("<<", "LeftShift", "op_LeftShift");
			testUnchecked(">>", "RightShift", "op_RightShift");
			testUnchecked("<", "LessThan", "op_LessThan");
			testUnchecked(">", "GreaterThan", "op_GreaterThan");
			testUnchecked("<=", "LessThanOrEqual", "op_LessThanOrEqual");
			testUnchecked(">=", "GreaterThanOrEqual", "op_GreaterThanOrEqual");
			testUnchecked("&", "And", "op_BitwiseAnd");
			testUnchecked("^", "ExclusiveOr", "op_ExclusiveOr");
			testUnchecked("|", "Or", "op_BitwiseOr");
			testChecked("*", "MultiplyChecked", "op_Multiply");
			testChecked("+", "AddChecked", "op_Addition");
			testChecked("-", "SubtractChecked", "op_Subtraction");
		}

		[Test]
		public void OperatorMethodImplementedAsNativeOperatorIsNotConsideredAMethod() {
			AssertCorrect(@"
class X { public static X operator+(X a, X b) { return null; } public static X operator-(X a) { return null; } }
void M() {
	// BEGIN
	Expression<Func<X, X, X>> e1 = (a, b) => a + b;
	Expression<Func<X, X>>    e2 = a => -a;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_X}, 'b');
	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$Add($tmp1, $tmp2, {sm_X}), [$tmp1, $tmp2]);
	var $tmp3 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Negate($tmp3, {sm_X}), [$tmp3]);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.MethodKind == MethodKind.UserDefinedOperator ? MethodScriptSemantics.NativeOperator() : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void CanUseConversion() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<C, object>> e1 = o => o;
	Expression<Func<object, C>> e2 = o => (C)o;
	Expression<Func<int, short>> e3 = i => (short)i;
	Expression<Func<int, short>> e4 = i => checked((short)i);
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'o');
	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$Convert($tmp1, {sm_Object}), [$tmp1]);
	var $tmp2 = {sm_Expression}.$Parameter({sm_Object}, 'o');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Convert($tmp2, {sm_C}), [$tmp2]);
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, 'i');
	var $e3 = {sm_Expression}.$Lambda({sm_Expression}.$Convert($tmp3, {sm_Int16}), [$tmp3]);
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, 'i');
	var $e4 = {sm_Expression}.$Lambda({sm_Expression}.$ConvertChecked($tmp4, {sm_Int16}), [$tmp4]);
");
		}

		[Test]
		public void CanUseUserDefinedConversion() {
			AssertCorrect(@"
class X { public static explicit operator int(X x) { return 0; } }
void M() {
	// BEGIN
	Expression<Func<X, int>> e = a => (int)a;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_X}, 'a');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Convert($tmp1, {sm_Int32}, $GetMember({to_X}, 'op_Explicit')), [$tmp1]);
");
		}

		[Test]
		public void CanUseTypeIs() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<object, bool>> e = o => o is C;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Object}, 'o');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$TypeIs($tmp1, {sm_C}), [$tmp1]);
");
		}

		[Test]
		public void CanUseTypeAs() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<object, C>> e = o => o as C;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Object}, 'o');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$TypeAs($tmp1, {sm_C}), [$tmp1]);
");
		}

		[Test]
		public void CanInvokeDelegate() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<Func<int>, int>> e1 = f => f();
	Expression<Func<Func<int, int>, int, int>> e2 = (f, a) => f(a);
	Expression<Func<Func<int, int, int>, int, int, int>> e3 = (f, a, b) => f(b, a);
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Func}, {ga_Int32}), 'f');
	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $tmp1, []), [$tmp1]);
	var $tmp2 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Func}, {ga_Int32}, {ga_Int32}), 'f');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $tmp2, [$tmp3]), [$tmp2, $tmp3]);
	var $tmp4 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Func}, {ga_Int32}, {ga_Int32}, {ga_Int32}), 'f');
	var $tmp5 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp6 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $e3 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $tmp4, [$tmp6, $tmp5]), [$tmp4, $tmp5, $tmp6]);
");
		}

		[Test]
		public void CanNestLambdas() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<Func<int, int, int, int>, Func<int, Func<int, Func<int, int>>>>> curry = f => (a => b => c => f(a, b, c));
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({Func}, {ga_Int32}, {ga_Int32}, {ga_Int32}, {ga_Int32}), 'f');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, 'c');
	var $curry = {sm_Expression}.$Lambda({sm_Expression}.$Lambda({sm_Expression}.$Lambda({sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $tmp1, [$tmp2, $tmp3, $tmp4]), [$tmp4]), [$tmp3]), [$tmp2]), [$tmp1]);
");
		}

		[Test]
		public void CanQuote() {
			AssertCorrect(@"
static int F(Expression<Func<int, int>> f) { return 0; }
void M() {
	// BEGIN
	Expression<Func<int, int>> f = a => F(x => x + a);
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'x');
	var $f = {sm_Expression}.$Lambda({sm_Expression}.$Call(null, $GetMember({to_C}, 'F'), [{sm_Expression}.$Quote({sm_Expression}.$Lambda({sm_Expression}.$Add($tmp2, $tmp1, {sm_Int32}), [$tmp2]))]), [$tmp1]);
");
		}

		[Test]
		public void CanUseConstants() {
			AssertCorrect(@"
void M<T>() {
	// BEGIN
	Expression<Func<object>> f1 = () => null;
	Expression<Func<C>> f2 = () => null;
	Expression<Func<int?>> f3 = () => null;
	Expression<Func<int>> f4 = () => 42;
	Expression<Func<DateTime>> f5 = () => default(DateTime);
	Expression<Func<T>> f6 = () => default(T);
	// END
}
",
@"	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Constant(null, {sm_Object}), []);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Constant(null, {sm_C}), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Constant(null, sm_$InstantiateGenericType({Nullable}, {ga_Int32})), []);
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$Constant(42, {sm_Int32}), []);
	var $f5 = {sm_Expression}.$Lambda({sm_Expression}.$Constant($Default({def_DateTime}), {sm_DateTime}), []);
	var $f6 = {sm_Expression}.$Lambda({sm_Expression}.$Constant($Default($T), $T), []);
");
		}

		[Test]
		public void CanUseTypeOf() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<Type>> f = () => typeof(C);
	// END
}
",
@"	var $f = {sm_Expression}.$Lambda({sm_Expression}.$Constant({sm_C}, {sm_Type}), []);
");
		}

		[Test]
		public void CanUseSizeOf() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int>> f = () => sizeof(int);
	// END
}
",
@"	var $f = {sm_Expression}.$Lambda({sm_Expression}.$Constant(4, {sm_Int32}), []);
");
		}

		#warning TODO: Changed meaning of first parameter to NewArrayInit and NewArrayBounds
		[Test]
		public void CanUseArrayCreate() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int[]>> f1 = () => new[] { 42, 43, 44 };
	Expression<Func<double[]>> f2 = () => new double[13];
	Expression<Func<double[,]>> f3 = () => new double[5,3];
	Expression<Func<double[,][]>> f4 = () => new double[5,3][];
	// END
}
",
@"	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(42, {sm_Int32}), {sm_Expression}.$Constant(43, {sm_Int32}), {sm_Expression}.$Constant(44, {sm_Int32})]), []);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$NewArrayBounds({sm_Double}, [{sm_Expression}.$Constant(13, {sm_Int32})]), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$NewArrayBounds({sm_Double}, [{sm_Expression}.$Constant(5, {sm_Int32}), {sm_Expression}.$Constant(3, {sm_Int32})]), []);
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$NewArrayBounds(sm_$Array({ga_Double}), [{sm_Expression}.$Constant(5, {sm_Int32}), {sm_Expression}.$Constant(3, {sm_Int32})]), []);
");
		}

		[Test]
		public void CanUseArrayIndex() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int[], int, int>> f1 = (a, b) => a[b];
	Expression<Func<double[,], int, int, double>> f2 = (a, b, c) => a[b, c];
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter(sm_$Array({ga_Int32}), 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$ArrayIndex({sm_Int32}, $tmp1, $tmp2), [$tmp1, $tmp2]);
	var $tmp3 = {sm_Expression}.$Parameter(sm_$Array({ga_Double}), 'a');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $tmp5 = {sm_Expression}.$Parameter({sm_Int32}, 'c');
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$ArrayIndex({sm_Double}, $tmp3, [$tmp4, $tmp5]), [$tmp3, $tmp4, $tmp5]);
");
		}

		[Test]
		public void CanUseArrayLength() {
			// This is what csc does
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int[], int>> e1 = a => a.Length;
	Expression<Func<Array, int>> e2 = a => a.Length;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter(sm_$Array({ga_Int32}), 'a');
	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$ArrayLength($tmp1), [$tmp1]);
	var $tmp2 = {sm_Expression}.$Parameter({sm_Array}, 'a');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Property($tmp2, $GetMember({to_Array}, 'Length')), [$tmp2]);
");
		}

		[Test]
		public void CanUseProperty() {
			AssertCorrect(@"
int P { get; set; }
static double P2 { get; set; }
void M() {
	// BEGIN
	Expression<Func<C, int>> f1 = a => a.P;
	Expression<Func<double>> f2 = () => P2;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'a');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Property($tmp1, $GetMember({to_C}, 'P')), [$tmp1]);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Property(null, $GetMember({to_C}, 'P2')), []);
");
		}

		[Test]
		public void CanUseField() {
			AssertCorrect(@"
int F;
static double F2;
void M() {
	// BEGIN
	Expression<Func<C, int>> f1 = a => a.F;
	Expression<Func<double>> f2 = () => F2;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'a');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Field($tmp1, $GetMember({to_C}, 'F')), [$tmp1]);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Field(null, $GetMember({to_C}, 'F2')), []);
");
		}

		[Test]
		public void CanInvokeMethod() {
			AssertCorrect(@"
int F(string a, double b) { return 0; }
static double F2(string a, double b) { return 0; }
double F3(string a, double b) { return 0; }
static double F4<T>(string a, double b) { return 0; }
double F5<T>(string a, double b) { return 0; }
void M() {
	// BEGIN
	Expression<Func<C, string, double, int>> f1 = (a, b, c) => a.F(b, c);
	Expression<Func<string, double, double>> f2 = (a, b) => F2(a, b);
	Expression<Func<string, double, double>> f3 = (a, b) => F3(a, b);
	Expression<Func<string, double, double>> f4 = (a, b) => F4<int>(a, b);
	Expression<Func<string, double, double>> f5 = (a, b) => F5<int>(a, b);
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_String}, 'b');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Double}, 'c');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Call($tmp1, $GetMember({to_C}, 'F'), [$tmp2, $tmp3]), [$tmp1, $tmp2, $tmp3]);
	var $tmp4 = {sm_Expression}.$Parameter({sm_String}, 'a');
	var $tmp5 = {sm_Expression}.$Parameter({sm_Double}, 'b');
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Call(null, $GetMember({to_C}, 'F2'), [$tmp4, $tmp5]), [$tmp4, $tmp5]);
	var $tmp6 = {sm_Expression}.$Parameter({sm_String}, 'a');
	var $tmp7 = {sm_Expression}.$Parameter({sm_Double}, 'b');
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F3'), [$tmp6, $tmp7]), [$tmp6, $tmp7]);
	var $tmp8 = {sm_Expression}.$Parameter({sm_String}, 'a');
	var $tmp9 = {sm_Expression}.$Parameter({sm_Double}, 'b');
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$Call(null, $GetMember({to_C}, 'F4', [{ga_Int32}]), [$tmp8, $tmp9]), [$tmp8, $tmp9]);
	var $tmp10 = {sm_Expression}.$Parameter({sm_String}, 'a');
	var $tmp11 = {sm_Expression}.$Parameter({sm_Double}, 'b');
	var $f5 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F5', [{ga_Int32}]), [$tmp10, $tmp11]), [$tmp10, $tmp11]);
");
		}

		[Test]
		public void CanUseMethodGroupConversion() {
			if (typeof(MethodInfo).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object) }) == null) {
				Assert.Inconclusive("Cannot run this test on .net 4.0 because the MethodInfo.CreateDelegate method does not exist.");
			}

			AssertCorrect(@"
int F1(string a, double b) { return 0; }
static double F2(string a, double b) { return 0; }
double F3(string a, double b) { return 0; }
void M() {
	// BEGIN
	Expression<Func<C, Func<string, double, int>>> f1 = a => a.F1;
	Expression<Func<Func<string, double, double>>> f2 = () => F2;
	Expression<Func<Func<string, double, double>>> f3 = () => F3;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'a');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Call({sm_Expression}.$Constant($GetMember({to_C}, 'F1'), {sm_MethodInfo}), $GetMember({to_MethodInfo}, 'CreateDelegate'), [sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Int32}), $tmp1]), sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Int32})), [$tmp1]);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Call({sm_Expression}.$Constant($GetMember({to_C}, 'F2'), {sm_MethodInfo}), $GetMember({to_MethodInfo}, 'CreateDelegate'), [sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double}), null]), sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double})), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Call({sm_Expression}.$Constant($GetMember({to_C}, 'F3'), {sm_MethodInfo}), $GetMember({to_MethodInfo}, 'CreateDelegate'), [sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double}), {sm_Expression}.$Constant(this, {sm_C})]), sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double})), []);
");
		}

		[Test]
		public void CanUseMethodGroupConversionOnGenericMethod() {
			if (typeof(MethodInfo).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object) }) == null) {
				Assert.Inconclusive("Cannot run this test on .net 4.0 because the MethodInfo.CreateDelegate method does not exist.");
			}

			AssertCorrect(@"
int F1<T>(string a, double b) { return 0; }
static double F2<T>(string a, double b) { return 0; }
double F3<T>(string a, double b) { return 0; }
void M() {
	// BEGIN
	Expression<Func<C, Func<string, double, int>>> f1 = a => a.F1<string>;
	Expression<Func<Func<string, double, double>>> f2 = () => F2<string>;
	Expression<Func<Func<string, double, double>>> f3 = () => F3<string>;
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'a');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Call({sm_Expression}.$Constant($GetMember({to_C}, 'F1', [{ga_String}]), {sm_MethodInfo}), $GetMember({to_MethodInfo}, 'CreateDelegate'), [sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Int32}), $tmp1]), sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Int32})), [$tmp1]);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Call({sm_Expression}.$Constant($GetMember({to_C}, 'F2', [{ga_String}]), {sm_MethodInfo}), $GetMember({to_MethodInfo}, 'CreateDelegate'), [sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double}), null]), sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double})), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Call({sm_Expression}.$Constant($GetMember({to_C}, 'F3', [{ga_String}]), {sm_MethodInfo}), $GetMember({to_MethodInfo}, 'CreateDelegate'), [sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double}), {sm_Expression}.$Constant(this, {sm_C})]), sm_$InstantiateGenericType({Func}, {ga_String}, {ga_Double}, {ga_Double})), []);
");
		}

		[Test]
		public void CanUseIndexer() {
			AssertCorrect(@"
int this[string a, double b] { get { return 0; } set {} }
void M() {
	// BEGIN
	Expression<Func<C, string, double, int>> f1 = (a, b, c) => a[b, c];
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_String}, 'b');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Double}, 'c');
	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Call($tmp1, $GetMember({to_C}, 'get_Item'), [$tmp2, $tmp3]), [$tmp1, $tmp2, $tmp3]);
");
		}

		[Test]
		public void CanUseLocal() {
			AssertCorrect(@"
void M() {
	int x = 0;
	// BEGIN
	Expression<Func<int>> e = () => x;
	// END
}
",
@"	var $e = {sm_Expression}.$Lambda($Local('x', {to_Int32}, $x), []);
");

			AssertCorrect(@"
void M() {
	for (int x = 0; x < 10; x++) {
		int x2 = x;
		Action a = () => {
			int y = x2;
			// BEGIN
			Expression<Func<int>> e = () => x2;
			// END
		};
	}
}
",
@"			var $e = {sm_Expression}.$Lambda($Local('x2', {to_Int32}, this.$x2.$), []);
");
		}

		[Test]
		public void CanUseImplicitThis() {
			AssertCorrect(@"
int F;
void M() {
	// BEGIN
	Expression<Func<int>> e = () => F;
	// END
}
",
@"	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Field({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F')), []);
");

			AssertCorrect(@"
int F;
void M() {
	for (int x = 0; x < 10; x++) {
		int x2 = x;
		Action a = () => {
			int y = x2;
			// BEGIN
			Expression<Func<int>> e = () => F;
			// END
		};
	}
}
",
@"			var $e = {sm_Expression}.$Lambda({sm_Expression}.$Field({sm_Expression}.$Constant(this.$this, {sm_C}), $GetMember({to_C}, 'F')), []);
");
		}

		[Test]
		public void CanUseExplicitThis() {
			AssertCorrect(@"
int F;
void M() {
	// BEGIN
	Expression<Func<int>> e = () => this.F;
	// END
}
",
@"	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Field({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F')), []);
");

			AssertCorrect(@"
int F;
void M() {
	for (int x = 0; x < 10; x++) {
		int x2 = x;
		Action a = () => {
			int y = x2;
			// BEGIN
			Expression<Func<int>> e = () => this.F;
			// END
		};
	}
}
",
@"			var $e = {sm_Expression}.$Lambda({sm_Expression}.$Field({sm_Expression}.$Constant(this.$this, {sm_C}), $GetMember({to_C}, 'F')), []);
");
		}

		[Test]
		public void ImplicitConversionsWork() {
			AssertCorrect(@"
void M() {
	int a = 0;
	short b = 0;
	// BEGIN
	Expression<Func<double>> e1 = () => a;
	Expression<Func<double>> e2 = () => a + b;
	// END
}
",
@"	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$Convert($Local('a', {to_Int32}, $a), {sm_Double}), []);
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$Add($Local('a', {to_Int32}, $a), {sm_Expression}.$Convert($Local('b', {to_Int16}, $b), {sm_Int32}), {sm_Int32}), {sm_Double}), []);
");
		}

		[Test]
		public void CanCreateObject() {
			AssertCorrect(@"
C() {}
C(int a) {}
C(int a, string b) {}
void M() {
	// BEGIN
	Expression<Func<C>> e1 = () => new C();
	Expression<Func<int, C>> e2 = (a) => new C(a);
	Expression<Func<int, string, C>> e3 = (a, b) => new C(a, b);
	// END
}
",
@"	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), []), []);
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), [$tmp1]), [$tmp1]);
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp3 = {sm_Expression}.$Parameter({sm_String}, 'b');
	var $e3 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), [$tmp2, $tmp3]), [$tmp2, $tmp3]);
");
		}

		[Test]
		public void CanCreateAnonymousObject() {
			AssertCorrect(@"
C() {}
C(int a) {}
C(int a, string b) {}
void M() {
	// BEGIN
	Expression<Func<int, string, object>> e = (a, b) => new { a, B = b };
	// END
}
",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_String}, 'b');
	var $e = {sm_Expression}.$Lambda({sm_Expression}.$Convert({sm_Expression}.$New($GetMember(to_$Anonymous, '.ctor'), [$tmp1, $tmp2], [$GetMember(to_$Anonymous, 'a'), $GetMember(to_$Anonymous, 'B')]), {sm_Object}), [$tmp1, $tmp2]);
");
		}

		[Test]
		public void CanUseObjectInitializers() {
			AssertCorrect(@"
int F;
int P { get; set; }
C X;
C Y { get; set; }
void M() {
	// BEGIN
	Expression<Func<C>> e1 = () => new C { F = 42, P = 17 };
	Expression<Func<C>> e2 = () => new C { X = { F = 42 } };
	Expression<Func<C>> e3 = () => new C { Y = { F = 448 } };
	Expression<Func<C>> e4 = () => new C { X = { F = 12, X = { P = 14, Y = { F = 89 }, F = 38 } } };
	// END
}
",
@"	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember({to_C}, '.ctor'), []), [{sm_Expression}.$Bind($GetMember({to_C}, 'F'), {sm_Expression}.$Constant(42, {sm_Int32})), {sm_Expression}.$Bind($GetMember({to_C}, 'P'), {sm_Expression}.$Constant(17, {sm_Int32}))]), []);
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember({to_C}, '.ctor'), []), [{sm_Expression}.$MemberBind($GetMember({to_C}, 'X'), [{sm_Expression}.$Bind($GetMember({to_C}, 'F'), {sm_Expression}.$Constant(42, {sm_Int32}))])]), []);
	var $e3 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember({to_C}, '.ctor'), []), [{sm_Expression}.$MemberBind($GetMember({to_C}, 'Y'), [{sm_Expression}.$Bind($GetMember({to_C}, 'F'), {sm_Expression}.$Constant(448, {sm_Int32}))])]), []);
	var $e4 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember({to_C}, '.ctor'), []), [{sm_Expression}.$MemberBind($GetMember({to_C}, 'X'), [{sm_Expression}.$Bind($GetMember({to_C}, 'F'), {sm_Expression}.$Constant(12, {sm_Int32})), {sm_Expression}.$MemberBind($GetMember({to_C}, 'X'), [{sm_Expression}.$Bind($GetMember({to_C}, 'P'), {sm_Expression}.$Constant(14, {sm_Int32})), {sm_Expression}.$MemberBind($GetMember({to_C}, 'Y'), [{sm_Expression}.$Bind($GetMember({to_C}, 'F'), {sm_Expression}.$Constant(89, {sm_Int32}))]), {sm_Expression}.$Bind($GetMember({to_C}, 'F'), {sm_Expression}.$Constant(38, {sm_Int32}))])])]), []);
");
		}

		[Test]
		public void CanUseCollectionInitializers() {
			AssertCorrect(@"
class MyDictionary : System.Collections.IEnumerable { public System.Collections.IEnumerator GetEnumerator() { return null; } public void Add(int a, string b) {} public void Add(int a) {} }
void M() {
	// BEGIN
	Expression<Func<List<int>>> e1 = () => new List<int> { 7, 4 };
	Expression<Func<MyDictionary>> e2 = () => new MyDictionary { { 14, ""X"" }, { 42, ""Y"" } };
	// END
}",
@"	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$ListInit({sm_Expression}.$New($GetMember('List', '.ctor$0'), []), [{sm_Expression}.$ElementInit($GetMember('List', 'Add$1'), [{sm_Expression}.$Constant(7, {sm_Int32})]), {sm_Expression}.$ElementInit($GetMember('List', 'Add$1'), [{sm_Expression}.$Constant(4, {sm_Int32})])]), []);
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$ListInit({sm_Expression}.$New($GetMember('MyDictionary', '.ctor$0'), []), [{sm_Expression}.$ElementInit($GetMember('MyDictionary', 'Add$2'), [{sm_Expression}.$Constant(14, {sm_Int32}), {sm_Expression}.$Constant('X', {sm_String})]), {sm_Expression}.$ElementInit($GetMember('MyDictionary', 'Add$2'), [{sm_Expression}.$Constant(42, {sm_Int32}), {sm_Expression}.$Constant('Y', {sm_String})])]), []);
", runtimeLibrary: new MockRuntimeLibrary { GetMember = (m, c) => JsExpression.Invocation(JsExpression.Identifier("$GetMember"), JsExpression.String(m.ContainingType.Name), JsExpression.String(m.Name + (m is IMethodSymbol ? "$" + ((IMethodSymbol)m).Parameters.Length : ""))) });
		}

		[Test, Category("Wait")] // Roslyn bug in GetCollectionInitializerSymbolInfo
		public void CanUseObjectAndCollectionInitializersNested1() {
			AssertCorrect(@"
List<int> LF = new List<int>();
List<int> LP { get; set; }
class MyDictionary : System.Collections.IEnumerable { public System.Collections.IEnumerator GetEnumerator() { return null; } public void Add(int a, string b) {} public void Add(int a) {} }
MyDictionary D = new MyDictionary();
void M() {
	// BEGIN
	Expression<Func<C>> e1 = () => new C { LF = { 7, 4 }, LP = {9, 78 } };
	Expression<Func<C>> e2 = () => new C { D = { { 42, ""Truth"" }, 13 } };
	Expression<Func<MyDictionary>> e3 = () => new MyDictionary { { 14, ""X"" }, { 42, ""Y"" } };
	// END
}",
@"	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember('C', '.ctor$0'), []), [{sm_Expression}.$ListBind($GetMember('C', 'LF'), [{sm_Expression}.$ElementInit($GetMember('List', 'Add$1'), [{sm_Expression}.$Constant(7, {sm_Int32})]), {sm_Expression}.$ElementInit($GetMember('List', 'Add$1'), [{sm_Expression}.$Constant(4, {sm_Int32})])]), {sm_Expression}.$ListBind($GetMember('C', 'LP'), [{sm_Expression}.$ElementInit($GetMember('List', 'Add$1'), [{sm_Expression}.$Constant(9, {sm_Int32})]), {sm_Expression}.$ElementInit($GetMember('List', 'Add$1'), [{sm_Expression}.$Constant(78, {sm_Int32})])])]), []);
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember('C', '.ctor$0'), []), [{sm_Expression}.$ListBind($GetMember('C', 'D'), [{sm_Expression}.$ElementInit($GetMember('MyDictionary', 'Add$2'), [{sm_Expression}.$Constant(42, {sm_Int32}), {sm_Expression}.$Constant('Truth', {sm_String})]), {sm_Expression}.$ElementInit($GetMember('MyDictionary', 'Add$1'), [{sm_Expression}.$Constant(13, {sm_Int32})])])]), []);
	var $e3 = {sm_Expression}.$Lambda({sm_Expression}.$ListInit({sm_Expression}.$New($GetMember('MyDictionary', '.ctor$0'), []), [{sm_Expression}.$ElementInit($GetMember('MyDictionary', 'Add$2'), [{sm_Expression}.$Constant(14, {sm_Int32}), {sm_Expression}.$Constant('X', {sm_String})]), {sm_Expression}.$ElementInit($GetMember('MyDictionary', 'Add$2'), [{sm_Expression}.$Constant(42, {sm_Int32}), {sm_Expression}.$Constant('Y', {sm_String})])]), []);
", runtimeLibrary: new MockRuntimeLibrary { GetMember = (m, c) => JsExpression.Invocation(JsExpression.Identifier("$GetMember"), JsExpression.String(m.ContainingType.Name), JsExpression.String(m.Name + (m is IMethodSymbol ? "$" + ((IMethodSymbol)m).Parameters.Length : ""))) });
		}

		[Test, Category("Wait")] // Roslyn bug in GetCollectionInitializerSymbolInfo
		public void CanUseObjectAndCollectionInitializersNested2() {
			AssertCorrect(@"
C X;
C Y;
List<int> L1;
List<int> L2;
int F1;
int F2;
void M() {
	// BEGIN
	Expression<Func<C>> e1 = () => new C { F1 = 7, X = { Y = { L1 = { 9, 78 }, F1 = 23 } }, F2 = 12 };
	// END
}",
@"	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$MemberInit({sm_Expression}.$New($GetMember({to_C}, '.ctor'), []), [{sm_Expression}.$Bind($GetMember({to_C}, 'F1'), {sm_Expression}.$Constant(7, {sm_Int32})), {sm_Expression}.$MemberBind($GetMember({to_C}, 'X'), [{sm_Expression}.$MemberBind($GetMember({to_C}, 'Y'), [{sm_Expression}.$ListBind($GetMember({to_C}, 'L1'), [{sm_Expression}.$ElementInit($GetMember(to_$InstantiateGenericType({List}, {ga_Int32}), 'Add'), [{sm_Expression}.$Constant(9, {sm_Int32})]), {sm_Expression}.$ElementInit($GetMember(to_$InstantiateGenericType({List}, {ga_Int32}), 'Add'), [{sm_Expression}.$Constant(78, {sm_Int32})])]), {sm_Expression}.$Bind($GetMember({to_C}, 'F1'), {sm_Expression}.$Constant(23, {sm_Int32}))])]), {sm_Expression}.$Bind($GetMember({to_C}, 'F2'), {sm_Expression}.$Constant(12, {sm_Int32}))]), []);
");
		}

		[Test]
		public void TemporariesAreCreatedForArgumentsUsedMoreThanOnce() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int>> e1 = () => 42;
	Expression<Func<int, int>> e2 = a => a + 1;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Constant(42, {sm_Int32});
	var $e1 = _($tmp1)._($tmp1)._([])._([]);
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp3 = {sm_Expression}.$Add($tmp2, {sm_Expression}.$Constant(1, {sm_Int32}), {sm_Int32});
	var $e2 = _($tmp3)._($tmp3)._([$tmp2])._([$tmp2]);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "Lambda" ? MethodScriptSemantics.InlineCode("_({body})._({body})._({parameters})._({parameters})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int>> e = a => a + 1;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp2 = {sm_Expression}.$Add($tmp1, {sm_Expression}.$Constant(1, {sm_Int32}), {sm_Int32});
	var $e = _($tmp2)._($tmp2)._([$tmp1]);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "Lambda" ? MethodScriptSemantics.InlineCode("_({body})._({body})._({parameters})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });

			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int>> e1 = () => 42;
	Expression<Func<int, int>> e2 = a => a + 1;
	// END
}",
@"	var $e1 = _({sm_Expression}.$Constant(42, {sm_Int32}))._([]);
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $e2 = _({sm_Expression}.$Add($tmp1, {sm_Expression}.$Constant(1, {sm_Int32}), {sm_Int32}))._([$tmp1]);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "Lambda" ? MethodScriptSemantics.InlineCode("_({body})._({parameters})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void CanUseCheckedAndUnchecked() {
			AssertCorrect(@"
void M() {
	// BEGIN
	Expression<Func<int, int, int>> e1 = (a, b) => checked(a + b);
	Expression<Func<int, int, int>> e2 = (a, b) => unchecked(a + b);
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$AddChecked($tmp1, $tmp2, {sm_Int32}), [$tmp1, $tmp2]);
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
	var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Add($tmp3, $tmp4, {sm_Int32}), [$tmp3, $tmp4]);
");
		}

		[Test, Ignore("Not yet supported")]
		public void CheckedContextIsInheritedFromParent() {
			AssertCorrect(@"
void M() {
	// BEGIN
	checked {
		Expression<Func<int, int, int>> e1 = (a, b) => a + b;
	}
	unchecked {
		Expression<Func<int, int, int>> e2 = (a, b) => a + b;
	}
	// END
}",
@"	{
		var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
		var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
		var $e1 = {sm_Expression}.$Lambda({sm_Expression}.$AddChecked($tmp1, $tmp2, {sm_Int32}), [$tmp1, $tmp2]);
	}
	{
		var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, 'a');
		var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, 'b');
		var $e2 = {sm_Expression}.$Lambda({sm_Expression}.$Add($tmp3, $tmp4, {sm_Int32}), [$tmp3, $tmp4]);
	}
");
		}

		[Test]
		public void CanUseExpandedFormParamArrayWhenInvokingDelegate() {
			AssertCorrect(
@"delegate int D(int a, params int[] b);

void M() {
	D d = null;
	// BEGIN
	Expression<Func<int>> f1 = () => d(0);
	Expression<Func<int>> f2 = () => d(0, 1);
	Expression<Func<int>> f3 = () => d(0, 2, 3);
	Expression<Func<int>> f4 = () => d(0, 4, 5, 6);
	// END
}",
@"	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $Local('d', {to_D}, $d), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [])]), []);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $Local('d', {to_D}, $d), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(1, {sm_Int32})])]), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $Local('d', {to_D}, $d), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(2, {sm_Int32}), {sm_Expression}.$Constant(3, {sm_Int32})])]), []);
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$Invoke({sm_Int32}, $Local('d', {to_D}, $d), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(4, {sm_Int32}), {sm_Expression}.$Constant(5, {sm_Int32}), {sm_Expression}.$Constant(6, {sm_Int32})])]), []);
");
		}

		[Test]
		public void CanUseExpandedFormParamArrayWhenInvokingMember() {
			AssertCorrect(
@"int F(int a, params int[] b) { return 0; }

void M() {
	// BEGIN
	Expression<Func<int>> f1 = () => F(0);
	Expression<Func<int>> f2 = () => F(0, 1);
	Expression<Func<int>> f3 = () => F(0, 2, 3);
	Expression<Func<int>> f4 = () => F(0, 4, 5, 6);
	// END
}",
@"	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [])]), []);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(1, {sm_Int32})])]), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(2, {sm_Int32}), {sm_Expression}.$Constant(3, {sm_Int32})])]), []);
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'F'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(4, {sm_Int32}), {sm_Expression}.$Constant(5, {sm_Int32}), {sm_Expression}.$Constant(6, {sm_Int32})])]), []);
");
		}

		[Test]
		public void CanUseExpandedFormParamArrayWhenIndexing() {
			AssertCorrect(
@"int this[int a, params int[] b] { get { return 0; } }

void M() {
	// BEGIN
	Expression<Func<int>> f1 = () => this[0];
	Expression<Func<int>> f2 = () => this[0, 1];
	Expression<Func<int>> f3 = () => this[0, 2, 3];
	Expression<Func<int>> f4 = () => this[0, 4, 5, 6];
	// END
}",
@"	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'get_Item'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [])]), []);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'get_Item'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(1, {sm_Int32})])]), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'get_Item'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(2, {sm_Int32}), {sm_Expression}.$Constant(3, {sm_Int32})])]), []);
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$Call({sm_Expression}.$Constant(this, {sm_C}), $GetMember({to_C}, 'get_Item'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(4, {sm_Int32}), {sm_Expression}.$Constant(5, {sm_Int32}), {sm_Expression}.$Constant(6, {sm_Int32})])]), []);
");
		}

		[Test]
		public void CanUseExpandedFormParamArrayWhenCreatingObject() {
			AssertCorrect(
@"C(int a, params int[] b) {}

void M() {
	// BEGIN
	Expression<Func<C>> f1 = () => new C(0);
	Expression<Func<C>> f2 = () => new C(0, 1);
	Expression<Func<C>> f3 = () => new C(0, 2, 3);
	Expression<Func<C>> f4 = () => new C(0, 4, 5, 6);
	// END
}",
@"	var $f1 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [])]), []);
	var $f2 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(1, {sm_Int32})])]), []);
	var $f3 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(2, {sm_Int32}), {sm_Expression}.$Constant(3, {sm_Int32})])]), []);
	var $f4 = {sm_Expression}.$Lambda({sm_Expression}.$New($GetMember({to_C}, '.ctor'), [{sm_Expression}.$Constant(0, {sm_Int32}), {sm_Expression}.$NewArrayInit({sm_Int32}, [{sm_Expression}.$Constant(4, {sm_Int32}), {sm_Expression}.$Constant(5, {sm_Int32}), {sm_Expression}.$Constant(6, {sm_Int32})])]), []);
");
		}

		[Test, Category("Wait")]
		public void CanUseQueryExpressions() {
			Assert.Fail("TODO");
		}
	}
}
