using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public abstract class Expression {
		[ScriptName("ntype")]
		public ExpressionType NodeType { get; private set; }
		public Type Type { get; private set; }

		[InlineCode("{{ ntype: {nodeType}, type: {type} }}")]
		protected Expression(ExpressionType nodeType, Type type) {}

		internal Expression() {}

		[InlineCode("{{ ntype: {binaryType}, type: {right}.type, left: {left}, right: {right} }}")]
		public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: {binaryType}, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion) { return null; }

		[InlineCode("{{ ntype: 46, type: {right}.type, left: {left}, right: {right} }}")]
		public static BinaryExpression Assign(Expression left, Expression right) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Equal(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 13, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Equal(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 13, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Equal(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression NotEqual(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression NotEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 35, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression NotEqual(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 35, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression NotEqual(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression GreaterThan(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 15, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression GreaterThan(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 15, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression GreaterThan(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression LessThan(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 20, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression LessThan(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 20, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression LessThan(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 16, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 16, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression LessThanOrEqual(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 21, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression LessThanOrEqual(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 21, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression LessThanOrEqual(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression AndAlso(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 3, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression AndAlso(Expression left, Expression right, Type type) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression OrElse(Expression left, Expression right) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression OrElse(Expression left, Expression right, MethodInfo method) { return null; }
		[InlineCode("{{ ntype: 37, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression OrElse(Expression left, Expression right, Type type) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Coalesce(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 7, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Coalesce(Expression left, Expression right, Type type) { return null; }
		//public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Add(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 0, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Add(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 0, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Add(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 63, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression AddAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 63, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[InlineCode("{{ ntype: 74, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression AddAssignChecked(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 74, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression AddChecked(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 1, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression AddChecked(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 1, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Subtract(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 42, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Subtract(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 42, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Subtract(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 73, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression SubtractAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 73, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[InlineCode("{{ ntype: 76, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 76, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression SubtractChecked(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 43, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression SubtractChecked(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 43, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression SubtractChecked(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Divide(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 12, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Divide(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 12, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 65, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression DivideAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 65, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Modulo(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 25, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Modulo(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 25, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Modulo(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 68, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression ModuloAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 68, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Multiply(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 26, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Multiply(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 26, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Multiply(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 69, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression MultiplyAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 69, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[InlineCode("{{ ntype: 75, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 75, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression MultiplyChecked(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 27, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression MultiplyChecked(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 27, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression MultiplyChecked(Expression left, Expression right, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression LeftShift(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 19, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression LeftShift(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 19, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 67, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression LeftShiftAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 67, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression RightShift(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 41, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression RightShift(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 41, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression RightShift(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 72, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression RightShiftAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 72, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression And(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 2, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression And(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 2, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression And(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 64, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression AndAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 64, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression Or(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 36, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Or(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 36, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Or(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 70, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression OrAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 70, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression ExclusiveOr(Expression left, Expression right) { return null; }
		[InlineCode("{{ ntype: 14, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression ExclusiveOr(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 14, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 66, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 66, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[InlineCode("{{ ntype: 39, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression Power(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 39, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression Power(Expression left, Expression right, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 71, type: {type}, left: {left}, right: {right} }}")]
		public static BinaryExpression PowerAssign(Expression left, Expression right, Type type) { return null; }
		[InlineCode("{{ ntype: 71, type: {method}.returnType, left: {left}, right: {right}, method: {method} }}")]
		public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method) { return null; }
		//public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static BinaryExpression ArrayIndex(Expression array, Expression index) { return null; }
		[InlineCode("{{ ntype: 5, type: {type}, left: {array}, right: {index} }}")]
		public static BinaryExpression ArrayIndex(Type type, Expression array, Expression index) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) { return null; }
		[InlineCode("{{ ntype: 6, type: {type}, obj: {array}, method: {{ type: 8, typeDef: {$System.Array}, name: 'Get', returnType: {type}, params: {$System.Script}.repeat({$System.Int32}, {indexes}.length), def: function() {{ return {$System.Script}.arrayGet2(this, arguments); }} }}, args: {indexes} }}")]
		public static MethodCallExpression ArrayIndex(Type type, Expression array, params Expression[] indexes) { return null; }
		[InlineCode("(function(a, b, c) {{ return {{ ntype: 6, type: a, obj: b, method: {{ type: 8, typeDef: {$System.Array}, name: 'Get', returnType: a, params: {$System.Script}.repeat({$System.Int32}, c.length), def: function() {{ return {$System.Script}.arrayGet2(this, arguments); }} }}, args: c }}; }})({type}, {array}, {$System.Script}.arrayFromEnumerable({indexes}))")]
		public static MethodCallExpression ArrayIndex(Type type, Expression array, IEnumerable<Expression> indexes) { return null; }

		[InlineCode("{{ ntype: 47, type: {expressions}[{expressions}.length - 1].type, expressions: {expressions} }}")]
		public static BlockExpression Block(params Expression[] expressions) { return null; }
		[InlineCode("(function(a) {{ return {{ ntype: 47, type: a[a.length - 1].type, expressions: a }}; }})({$System.Script}.arrayFromEnumerable({expressions}))")]
		public static BlockExpression Block(IEnumerable<Expression> expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {type}, expressions: {expressions} }}")]
		public static BlockExpression Block(Type type, params Expression[] expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {type}, expressions: {$System.Script}.arrayFromEnumerable({expressions}) }}")]
		public static BlockExpression Block(Type type, IEnumerable<Expression> expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {expressions}[{expressions}.length - 1].type, variables: {$System.Script}.arrayFromEnumerable({variables}), expressions: {expressions} }}")]
		public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions) { return null; }
		[InlineCode("(function(a, b) {{ return {{ ntype: 47, type: b[b.length - 1].type, variables: a, expressions: b }}; }})({$System.Script}.arrayFromEnumerable({variables}), {$System.Script}.arrayFromEnumerable({expressions}))")]
		public static BlockExpression Block(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {type}, variables: {$System.Script}.arrayFromEnumerable({variables}), expressions: {$System.Script}.arrayFromEnumerable({expressions}) }}")]
		public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, params Expression[] expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {type}, variables: {$System.Script}.arrayFromEnumerable({variables}), expressions: {$System.Script}.arrayFromEnumerable({expressions}) }}")]
		public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {expressions}[{expressions}.length - 1].type, variables: {variables}, expressions: {expressions} }}")]
		public static BlockExpression Block(ParameterExpression[] variables, params Expression[] expressions) { return null; }
		[InlineCode("(function(a, b) {{ return {{ ntype: 47, type: b[b.length - 1].type, variables: a, expressions: b }}; }})({variables}, {$System.Script}.arrayFromEnumerable({expressions}))")]
		public static BlockExpression Block(ParameterExpression[] variables, IEnumerable<Expression> expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {type}, variables: {variables}, expressions: {expressions} }}")]
		public static BlockExpression Block(Type type, ParameterExpression[] variables, params Expression[] expressions) { return null; }
		[InlineCode("{{ ntype: 47, type: {type}, variables: {variables}, expressions: {$System.Script}.arrayFromEnumerable({expressions}) }}")]
		public static BlockExpression Block(Type type, ParameterExpression[] variables, IEnumerable<Expression> expressions) { return null; }

		[InlineCode("{{ test: {type}, body: {body} }}")]
		public static CatchBlock Catch(Type type, Expression body) { return null; }
		[InlineCode("{{ test: {variable}.type, variable: {variable}, body: {body} }}")]
		public static CatchBlock Catch(ParameterExpression variable, Expression body) { return null; }
		[InlineCode("{{ test: {type}, body: {body}, filter: {filter} }}")]
		public static CatchBlock Catch(Type type, Expression body, Expression filter) { return null; }
		[InlineCode("{{ test: {variable}.type, variable: {variable}, body: {body}, filter: {filter} }}")]
		public static CatchBlock Catch(ParameterExpression variable, Expression body, Expression filter) { return null; }
		[InlineCode("{{ test: {type} || {variable}.type, variable: {variable}, body: {body}, filter: {filter} }}")]
		public static CatchBlock MakeCatchBlock(Type type, ParameterExpression variable, Expression body, Expression filter) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) { return null; }
		[InlineCode("{{ ntype: 8, type: {type}, test: {test}, ifTrue: {ifTrue}, ifFalse: {ifFalse} }}")]
		public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) { return null; }
		[InlineCode("{{ ntype: 8, type: {$System.Void}, test: {test}, ifTrue: {ifTrue}, ifFalse: {{ ntype: 51, type: {$System.Void} }} }}")]
		public static ConditionalExpression IfThen(Expression test, Expression ifTrue) { return null; }
		[InlineCode("{{ ntype: 8, type: {$System.Void}, test: {test}, ifTrue: {ifTrue}, ifFalse: {ifFalse} }}")]
		public static ConditionalExpression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static ConstantExpression Constant(object value) { return null; }
		[InlineCode("{{ ntype: 9, type: {type}, value: {value} }}")]
		public static ConstantExpression Constant(object value, Type type) { return null; }
		[InlineCode("{{ ntype: 9, type: {T}, value: {value} }}")]
		public static ConstantExpression Constant<T>(T value) { return null; }

		[InlineCode("{{ ntype: 51, type: {$System.Void} }}")]
		public static DefaultExpression Empty() { return null; }

		[InlineCode("{{ ntype: 51, type: {type} }}")]
		public static DefaultExpression Default(Type type) { return null; }

		//public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) { return null; }
		//public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) { return null; }
		//public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0) { return null; }
		//public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) { return null; }
		//public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) { return null; }
		//public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		//public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) { return null; }
		//public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) { return null; }
		//public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) { return null; }
		//public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) { return null; }
		//public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		//public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) { return null; }

		[InlineCode("{{ ntype: 50, type: {$System.Object}, dtype: 0, expression: {expression}, member: {member} }}")]
		public static DynamicMemberExpression DynamicMember(Expression expression, string member) { return null; }
		[InlineCode("{{ ntype: 50, type: {type}, dtype: 0, expression: {expression}, member: {member} }}")]
		public static DynamicMemberExpression DynamicMember(Type type, Expression expression, string member) { return null; }

		[InlineCode("{{ ntype: 50, type: {$System.Object}, dtype: 1, expression: {expression}, arguments: {arguments} }}")]
		public static DynamicInvocationExpression DynamicInvocation(Expression expression, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 50, type: {$System.Object}, dtype: 1, expression: {expression}, arguments: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static DynamicInvocationExpression DynamicInvocation(Expression expression, IEnumerable<Expression> arguments) { return null; }
		[InlineCode("{{ ntype: 50, type: {type}, dtype: 1, expression: {expression}, arguments: {arguments} }}")]
		public static DynamicInvocationExpression DynamicInvocation(Type type, Expression expression, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 50, type: {type}, dtype: 1, expression: {expression}, arguments: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static DynamicInvocationExpression DynamicInvocation(Type type, Expression expression, IEnumerable<Expression> arguments) { return null; }

		[InlineCode("{{ ntype: 50, type: {$System.Object}, dtype: 2, expression: {expression}, argument: {argument} }}")]
		public static DynamicIndexExpression DynamicIndex(Expression expression, Expression argument) { return null; }
		[InlineCode("{{ ntype: 50, type: {type}, dtype: 2, expression: {expression}, argument: {argument} }}")]
		public static DynamicIndexExpression DynamicIndex(Type type, Expression expression, Expression argument) { return null; }

		[InlineCode("{{ addMethod: {addMethod}, arguments: {arguments} }}")]
		public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) { return null; }
		[InlineCode("{{ addMethod: {addMethod}, arguments: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) { return null; }

		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 2, target: {target} }}")]
		public static GotoExpression Break(LabelTarget target) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 2, target: {target} }}")]
		public static GotoExpression Break(LabelTarget target, Type type) { return null; }
		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 2, target: {target}, value: {value} }}")]
		public static GotoExpression Break(LabelTarget target, Expression value) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 2, target: {target}, value: {value} }}")]
		public static GotoExpression Break(LabelTarget target, Expression value, Type type) { return null; }

		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 3, target: {target} }}")]
		public static GotoExpression Continue(LabelTarget target) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 3, target: {target} }}")]
		public static GotoExpression Continue(LabelTarget target, Type type) { return null; }

		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 1, target: {target} }}")]
		public static GotoExpression Return(LabelTarget target) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 1, target: {target} }}")]
		public static GotoExpression Return(LabelTarget target, Type type) { return null; }
		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 1, target: {target}, value: {value} }}")]
		public static GotoExpression Return(LabelTarget target, Expression value) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 1, target: {target}, value: {value} }}")]
		public static GotoExpression Return(LabelTarget target, Expression value, Type type) { return null; }

		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 0, target: {target} }}")]
		public static GotoExpression Goto(LabelTarget target) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 0, target: {target} }}")]
		public static GotoExpression Goto(LabelTarget target, Type type) { return null; }
		[InlineCode("{{ ntype: 53, type: {$System.Void}, kind: 0, target: {target}, value: {value} }}")]
		public static GotoExpression Goto(LabelTarget target, Expression value) { return null; }
		[InlineCode("{{ ntype: 53, type: {type}, kind: 0, target: {target}, value: {value} }}")]
		public static GotoExpression Goto(LabelTarget target, Expression value, Type type) { return null; }

		[InlineCode("{{ ntype: 53, type: {type}, kind: {kind}, target: {target}, value: {value} }}")]
		public static GotoExpression MakeGoto(GotoExpressionKind kind, LabelTarget target, Expression value, Type type) { return null; }

		//public static IndexExpression MakeIndex(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }
		[InlineCode("{{ ntype: 55, type: {type}, obj: {array}, arguments: {indexes} }}")]
		public static IndexExpression ArrayAccess(Type type, Expression array, params Expression[] indexes) { return null; }
		[InlineCode("{{ ntype: 55, type: {type}, obj: {array}, arguments: {$System.Script}.arrayFromEnumerable({indexes}) }}")]
		public static IndexExpression ArrayAccess(Type type, Expression array, IEnumerable<Expression> indexes) { return null; }
		//public static IndexExpression Property(Expression instance, string propertyName, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 55, type: {indexer}.returnType, obj: {instance}, indexer: {indexer}, arguments: {arguments} }}")]
		public static IndexExpression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 55, type: {indexer}.returnType, obj: {instance}, indexer: {indexer}, arguments: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static IndexExpression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 17, type: {type}, expression: {expression}, args: {arguments} }}")]
		public static InvocationExpression Invoke(Type type, Expression expression, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 17, type: {type}, expression: {expression}, args: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static InvocationExpression Invoke(Type type, Expression expression, IEnumerable<Expression> arguments) { return null; }

		[InlineCode("{{ ntype: 56, type: {target}.type, target: {target} }}")]
		public static LabelExpression Label(LabelTarget target) { return null; }
		[InlineCode("{{ ntype: 56, type: {target}.type, target: {target}, defaultValue: {defaultValue} }}")]
		public static LabelExpression Label(LabelTarget target, Expression defaultValue) { return null; }

		[InlineCode("{{ type: {$System.Void} }}")]
		public static LabelTarget Label() { return null; }
		[InlineCode("{{ type: {$System.Void}, name: {name} }}")]
		public static LabelTarget Label(string name) { return null; }
		[InlineCode("{{ type: {type} }}")]
		public static LabelTarget Label(Type type) { return null; }
		[InlineCode("{{ type: {type}, name: {name} }}")]
		public static LabelTarget Label(Type type, string name) { return null; }

		[InlineCode("{{ ntype: 18, type: {$System.Function}, returnType: {body}.type, body: {body}, params: {$System.Script}.arrayFromEnumerable({parameters}) }}")]
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters) { return null; }
		[InlineCode("{{ ntype: 18, type: {$System.Function}, returnType: {body}.type, body: {body}, params: {parameters} }}")]
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) { return null; }
		[InlineCode("{{ ntype: 18, type: {$System.Function}, returnType: {body}.type, body: {body}, params: {$System.Script}.arrayFromEnumerable({parameters}) }}")]
		public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters) { return null; }
		[InlineCode("{{ ntype: 18, type: {$System.Function}, returnType: {body}.type, body: {body}, params: {parameters} }}")]
		public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) { return null; }

		//public static Type GetFuncType(params Type[] typeArgs) { return null; }
		//public static bool TryGetFuncType(Type[] typeArgs, out Type funcType) { funcType = null; return false; }
		//public static Type GetActionType(params Type[] typeArgs) { return null; }
		//public static bool TryGetActionType(Type[] typeArgs, out Type actionType) { actionType = null; return false; }
		//public static Type GetDelegateType(params Type[] typeArgs) { return null; }

		//public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers) { return null; }
		//public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers) { return null; }
		[InlineCode("{{ ntype: 22, type: {newExpression}.type, newExpression: {newExpression}, initializers: {initializers}.map(function(i) {{ return {{ addMethod: {addMethod}, arguments: [i] }}; }}) }}")]
		public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers) { return null; }
		[InlineCode("{{ ntype: 22, type: {newExpression}.type, newExpression: {newExpression}, initializers: {$System.Script}.arrayFromEnumerable({initializers}).map(function(i) {{ return {{ addMethod: {addMethod}, arguments: [i] }}; }}) }}")]
		public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers) { return null; }
		[InlineCode("{{ ntype: 22, type: {newExpression}.type, newExpression: {newExpression}, initializers: {initializers} }}")]
		public static ListInitExpression ListInit(NewExpression newExpression, params ElementInit[] initializers) { return null; }
		[InlineCode("{{ ntype: 22, type: {newExpression}.type, newExpression: {newExpression}, initializers: {$System.Script}.arrayFromEnumerable({initializers}) }}")]
		public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers) { return null; }

		[InlineCode("{{ ntype: 58, type: {$System.Void}, body: {body} }}")]
		public static LoopExpression Loop(Expression body) { return null; }
		[InlineCode("{{ ntype: 58, type: {break}.type, body: {body}, breakLabel: {break} }}")]
		public static LoopExpression Loop(Expression body, LabelTarget @break) { return null; }
		[InlineCode("{{ ntype: 58, type: {break} ? {break}.type : {$System.Void}, body: {body}, breakLabel: {break}, continueLabel: {continue} }}")]
		public static LoopExpression Loop(Expression body, LabelTarget @break, LabelTarget @continue) { return null; }

		[InlineCode("{{ btype: 0, member: {member}, expression: {expression} }}")]
		public static MemberAssignment Bind(MemberInfo member, Expression expression) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression) { return null; }

		[InlineCode("{{ ntype: 23, type: {field}.returnType, expression: {expression}, member: {field} }}")]
		public static MemberExpression Field(Expression expression, FieldInfo field) { return null; }
		[InlineCode("{{ ntype: 23, type: {$System.Script}.getMembers({expression}.type, 4, 284, {fieldName}).returnType, expression: {expression}, member: {$System.Script}.getMembers({expression}.type, 4, 284, {fieldName}) }}")]
		public static MemberExpression Field(Expression expression, string fieldName) { return null; }
		[InlineCode("{{ ntype: 23, type: {type}, expression: {expression}, member: {$System.Script}.getMembers({expression}.type, 4, 284, {fieldName}) }}")]
		public static MemberExpression Field(Expression expression, Type type, string fieldName) { return null; }

		[InlineCode("{{ ntype: 23, type: {$System.Script}.getMembers({expression}.type, 16, 284, {propertyName}).returnType, expression: {expression}, member: {$System.Script}.getMembers({expression}.type, 16, 284, {propertyName}) }}")]
		public static MemberExpression Property(Expression expression, string propertyName) { return null; }
		[InlineCode("{{ ntype: 23, type: {type}, expression: {expression}, member: {$System.Script}.getMembers({expression}.type, 16, 284, {propertyName}) }}")]
		public static MemberExpression Property(Expression expression, Type type, string propertyName) { return null; }
		[InlineCode("{{ ntype: 23, type: {property}.returnType, expression: {expression}, member: {property} }}")]
		public static MemberExpression Property(Expression expression, PropertyInfo property) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberExpression Property(Expression expression, MethodInfo propertyAccessor) { return null; }
		[InlineCode("{{ ntype: 23, type: {$System.Script}.getMembers({expression}.type, 20, 284, {propertyOrFieldName}).returnType, expression: {expression}, member: {$System.Script}.getMembers({expression}.type, 20, 284, {propertyOrFieldName}) }}")]
		public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName) { return null; }

		[InlineCode("{{ ntype: 23, type: {member}.returnType, expression: {expression}, member: {member} }}")]
		public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member) { return null; }

		[InlineCode("{{ ntype: 24, type: {newExpression}.type, newExpression: {newExpression}, bindings: {bindings} }}")]
		public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) { return null; }
		[InlineCode("{{ ntype: 24, type: {newExpression}.type, newExpression: {newExpression}, bindings: {$System.Script}.arrayFromEnumerable({bindings}) }}")]
		public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) { return null; }

		[InlineCode("{{ btype: 2, member: {member}, initializers: {initializers} }}")]
		public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers) { return null; }
		[InlineCode("{{ btype: 2, member: {member}, initializers: {$System.Script}.arrayFromEnumerable({initializers}) }}")]
		public static MemberListBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberListBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) { return null; }

		[InlineCode("{{ btype: 1, member: {member}, bindings: {bindings} }}")]
		public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings) { return null; }
		[InlineCode("{{ btype: 1, member: {member}, bindings: {$System.Script}.arrayFromEnumerable({bindings}) }}")]
		public static MemberMemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings) { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings) { return null; }

		[InlineCode("{{ ntype: 6, type: {method}.returnType, method: {method}, args: {arguments} }}")]
		public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 6, type: {method}.returnType, method: {method}, args: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static MethodCallExpression Call(MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		[InlineCode("{{ ntype: 6, type: {method}.returnType, obj: {instance}, method: {method}, args: {arguments} }}")]
		public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 6, type: {method}.returnType, obj: {instance}, method: {method}, args: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments) { return null; }

		[InlineCode("{{ ntype: 32, type: {$System.Array}, expressions: {initializers} }}")]
		public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers) { return null; }
		[InlineCode("{{ ntype: 32, type: {$System.Array}, expressions: {$System.Script}.arrayFromEnumerable({initializers}) }}")]
		public static NewArrayExpression NewArrayInit(Type type, IEnumerable<Expression> initializers) { return null; }
		[InlineCode("{{ ntype: 33, type: {$System.Array}, expressions: {bounds} }}")]
		public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds) { return null; }
		[InlineCode("{{ ntype: 33, type: {$System.Array}, expressions: {$System.Script}.arrayFromEnumerable({bounds}) }}")]
		public static NewArrayExpression NewArrayBounds(Type type, IEnumerable<Expression> bounds) { return null; }

		[InlineCode("{{ ntype: 31, type: {type}, constructor: {$System.Script}.getMembers({type}, 1, 284, null, []), arguments: [] }}")]
		public static NewExpression New(Type type) { return null; }
		[InlineCode("{{ ntype: 31, type: {constructor}.typeDef, constructor: {constructor}, arguments: {arguments} }}")]
		public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) { return null; }
		[InlineCode("{{ ntype: 31, type: {constructor}.typeDef, constructor: {constructor}, arguments: {$System.Script}.arrayFromEnumerable({arguments}) }}")]
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) { return null; }
		[InlineCode("{{ ntype: 31, type: {constructor}.typeDef, constructor: {constructor}, arguments: {arguments}, members: {members} }}")]
		public static NewExpression New(ConstructorInfo constructor, Expression[] arguments, params MemberInfo[] members) { return null; }
		[InlineCode("{{ ntype: 31, type: {constructor}.typeDef, constructor: {constructor}, arguments: {$System.Script}.arrayFromEnumerable({arguments}), members: {$System.Script}.arrayFromEnumerable({members}) }}")]
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members) { return null; }
		[InlineCode("{{ ntype: 31, type: {constructor}.typeDef, constructor: {constructor}, arguments: {$System.Script}.arrayFromEnumerable({arguments}), members: {members} }}")]
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members) { return null; }

		[InlineCode("{{ ntype: 38, type: {type} }}")]
		public static ParameterExpression Parameter(Type type) { return null; }
		[InlineCode("{{ ntype: 38, type: {type}, name: {name} }}")]
		public static ParameterExpression Parameter(Type type, string name) { return null; }
		[InlineCode("{{ ntype: 38, type: {type} }}")]
		public static ParameterExpression Variable(Type type) { return null; }
		[InlineCode("{{ ntype: 38, type: {type}, name: {name} }}")]
		public static ParameterExpression Variable(Type type, string name) { return null; }

		//public static RuntimeVariablesExpression RuntimeVariables(params ParameterExpression[] variables) { return null; }
		//public static RuntimeVariablesExpression RuntimeVariables(IEnumerable<ParameterExpression> variables) { return null; }

		[InlineCode("{{ body: {body}, testValues: {testValues} }}")]
		public static SwitchCase SwitchCase(Expression body, params Expression[] testValues) { return null; }
		[InlineCode("{{ body: {body}, testValues: {$System.Script}.arrayFromEnumerable({testValues}) }}")]
		public static SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues) { return null; }

		[InlineCode("{{ ntype: 59, type: {cases}[0].body.type, switchValue: {switchValue}, cases: {cases} }}")]
		public static SwitchExpression Switch(Expression switchValue, params SwitchCase[] cases) { return null; }
		[InlineCode("{{ ntype: 59, type: {cases}[0].body.type, switchValue: {switchValue}, defaultBody: {defaultBody}, cases: {cases} }}")]
		public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, params SwitchCase[] cases) { return null; }
		[InlineCode("{{ ntype: 59, type: {cases}[0].body.type, switchValue: {switchValue}, defaultBody: {defaultBody}, comparison: {comparison}, cases: {cases} }}")]
		public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) { return null; }
		[InlineCode("{{ ntype: 59, type: {type}, switchValue: {switchValue}, defaultBody: {defaultBody}, comparison: {comparison}, cases: {cases} }}")]
		public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) { return null; }
		[InlineCode("(function(a, b, c, d) {{ return {{ ntype: 59, type: d[0].body.type, switchValue: a, defaultBody: b, comparison: c, cases: d }}; }})({switchValue}, {defaultBody}, {comparison}, {$System.Script}.arrayFromEnumerable({cases}))")]
		public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) { return null; }
		[InlineCode("{{ ntype: 59, type: {type}, switchValue: {switchValue}, defaultBody: {defaultBody}, comparison: {comparison}, cases: {$System.Script}.arrayFromEnumerable({cases}) }}")]
		public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) { return null; }

		[InlineCode("{{ ntype: 61, type: {body}.type, body: {body}, handlers: [], fault: {fault} }}")]
		public static TryExpression TryFault(Expression body, Expression fault) { return null; }
		[InlineCode("{{ ntype: 61, type: {body}.type, body: {body}, handlers: [], finallyExpr: {finally} }}")]
		public static TryExpression TryFinally(Expression body, Expression @finally) { return null; }
		[InlineCode("{{ ntype: 61, type: {body}.type, body: {body}, handlers: {handlers} }}")]
		public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers) { return null; }
		[InlineCode("{{ ntype: 61, type: {body}.type, body: {body}, finallyExpr: {finally}, handlers: {handlers} }}")]
		public static TryExpression TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers) { return null; }
		[InlineCode("{{ ntype: 61, type: {type} || {body}.type, body: {body}, finallyExpr: {finally}, fault: {fault}, handlers: {$System.Script}.arrayFromEnumerable({handlers} || []) }}")]
		public static TryExpression MakeTry(Type type, Expression body, Expression @finally, Expression fault, IEnumerable<CatchBlock> handlers) { return null; }

		[InlineCode("{{ ntype: 45, type: {$System.Boolean}, expression: {expression}, typeOperand: {type} }}")]
		public static TypeBinaryExpression TypeIs(Expression expression, Type type) { return null; }

		[InlineCode("{{ ntype: 81, type: {$System.Boolean}, expression: {expression}, typeOperand: {type} }}")]
		public static TypeBinaryExpression TypeEqual(Expression expression, Type type) { return null; }

		[InlineCode("{{ ntype: {unaryType}, type: {type}, operand: {operand} }}")]
		public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) { return null; }
		[InlineCode("{{ ntype: {unaryType}, type: {type} || {method}.returnType, operand: {operand}, method: {method} }}")]
		public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static UnaryExpression Negate(Expression expression) { return null; }
		[InlineCode("{{ ntype: 28, type: {type}, operand: {expression} }}")]
		public static UnaryExpression Negate(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 28, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression Negate(Expression expression, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static UnaryExpression UnaryPlus(Expression expression) { return null; }
		[InlineCode("{{ ntype: 29, type: {type}, operand: {expression} }}")]
		public static UnaryExpression UnaryPlus(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 29, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static UnaryExpression NegateChecked(Expression expression) { return null; }
		[InlineCode("{{ ntype: 30, type: {type}, operand: {expression} }}")]
		public static UnaryExpression NegateChecked(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 30, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression NegateChecked(Expression expression, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static UnaryExpression Not(Expression expression) { return null; }
		[InlineCode("{{ ntype: 34, type: {type}, operand: {expression} }}")]
		public static UnaryExpression Not(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 34, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression Not(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 84, type: {type}, operand: {expression} }}")]
		public static UnaryExpression IsFalse(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 84, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression IsFalse(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 83, type: {type}, operand: {expression} }}")]
		public static UnaryExpression IsTrue(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 83, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression IsTrue(Expression expression, MethodInfo method) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static UnaryExpression OnesComplement(Expression expression) { return null; }
		[InlineCode("{{ ntype: 82, type: {type}, operand: {expression} }}")]
		public static UnaryExpression OnesComplement(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 82, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression OnesComplement(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 44, type: {type}, operand: {expression} }}")]
		public static UnaryExpression TypeAs(Expression expression, Type type) { return null; }

		[InlineCode("{{ ntype: 62, type: {type}, operand: {expression} }}")]
		public static UnaryExpression Unbox(Expression expression, Type type) { return null; }

		[InlineCode("{{ ntype: 10, type: {type}, operand: {expression} }}")]
		public static UnaryExpression Convert(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 10, type: {type}, operand: {expression}, method: {method} }}")]
		public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 11, type: {type}, operand: {expression} }}")]
		public static UnaryExpression ConvertChecked(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 11, type: {type}, operand: {expression}, method: {method} }}")]
		public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 4, type: {$System.Int32}, operand: {array} }}")]
		public static UnaryExpression ArrayLength(Expression array) { return null; }

		[InlineCode("{{ ntype: 40, type: {$System.Linq.Expressions.Expression}, operand: {expression} }}")]
		public static UnaryExpression Quote(Expression expression) { return null; }

		[InlineCode("{{ ntype: 60, type: {$System.Void} }}")]
		public static UnaryExpression Rethrow() { return null; }
		[InlineCode("{{ ntype: 60, type: {type} }}")]
		public static UnaryExpression Rethrow(Type type) { return null; }

		[InlineCode("{{ ntype: 60, type: {$System.Void}, operand: {value} }}")]
		public static UnaryExpression Throw(Expression value) { return null; }
		[InlineCode("{{ ntype: 60, type: {type}, operand: {value} }}")]
		public static UnaryExpression Throw(Expression value, Type type) { return null; }

		[InlineCode("{{ ntype: 54, type: {type}, operand: {expression} }}")]
		public static UnaryExpression Increment(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 54, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression Increment(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 49, type: {type}, operand: {expression} }}")]
		public static UnaryExpression Decrement(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 49, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression Decrement(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 77, type: {type}, operand: {expression} }}")]
		public static UnaryExpression PreIncrementAssign(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 77, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression PreIncrementAssign(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 78, type: {type}, operand: {expression} }}")]
		public static UnaryExpression PreDecrementAssign(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 78, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression PreDecrementAssign(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 79, type: {type}, operand: {expression} }}")]
		public static UnaryExpression PostIncrementAssign(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 79, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression PostIncrementAssign(Expression expression, MethodInfo method) { return null; }

		[InlineCode("{{ ntype: 80, type: {type}, operand: {expression} }}")]
		public static UnaryExpression PostDecrementAssign(Expression expression, Type type) { return null; }
		[InlineCode("{{ ntype: 80, type: {method}.returnType, operand: {expression}, method: {method} }}")]
		public static UnaryExpression PostDecrementAssign(Expression expression, MethodInfo method) { return null; }
	}
}
