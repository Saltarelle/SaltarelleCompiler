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

		protected Expression(ExpressionType nodeType, Type type) {}

		internal Expression() {}

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public Expression Reduce() { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public Expression ReduceAndCheck() { return null; }
		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public Expression ReduceExtensions() { return null; }

		public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right) { return null; }
		public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion) { return null; }

		public static BinaryExpression Assign(Expression left, Expression right) { return null; }
		public static BinaryExpression Equal(Expression left, Expression right) { return null; }
		public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression ReferenceEqual(Expression left, Expression right) { return null; }
		public static BinaryExpression NotEqual(Expression left, Expression right) { return null; }
		public static BinaryExpression NotEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression ReferenceNotEqual(Expression left, Expression right) { return null; }
		public static BinaryExpression GreaterThan(Expression left, Expression right) { return null; }
		public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression LessThan(Expression left, Expression right) { return null; }
		public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right) { return null; }
		public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression LessThanOrEqual(Expression left, Expression right) { return null; }
		public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method) { return null; }
		public static BinaryExpression AndAlso(Expression left, Expression right) { return null; }
		public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression OrElse(Expression left, Expression right) { return null; }
		public static BinaryExpression OrElse(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression Coalesce(Expression left, Expression right) { return null; }
		public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion) { return null; }
		public static BinaryExpression Add(Expression left, Expression right) { return null; }
		public static BinaryExpression Add(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression AddAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression AddAssignChecked(Expression left, Expression right) { return null; }
		public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression AddChecked(Expression left, Expression right) { return null; }
		public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression Subtract(Expression left, Expression right) { return null; }
		public static BinaryExpression Subtract(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression SubtractAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression SubtractAssignChecked(Expression left, Expression right) { return null; }
		public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression SubtractChecked(Expression left, Expression right) { return null; }
		public static BinaryExpression SubtractChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression Divide(Expression left, Expression right) { return null; }
		public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression DivideAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression Modulo(Expression left, Expression right) { return null; }
		public static BinaryExpression Modulo(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression ModuloAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression Multiply(Expression left, Expression right) { return null; }
		public static BinaryExpression Multiply(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression MultiplyAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right) { return null; }
		public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression MultiplyChecked(Expression left, Expression right) { return null; }
		public static BinaryExpression MultiplyChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression LeftShift(Expression left, Expression right) { return null; }
		public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression LeftShiftAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression RightShift(Expression left, Expression right) { return null; }
		public static BinaryExpression RightShift(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression RightShiftAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression And(Expression left, Expression right) { return null; }
		public static BinaryExpression And(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression AndAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression Or(Expression left, Expression right) { return null; }
		public static BinaryExpression Or(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression OrAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression ExclusiveOr(Expression left, Expression right) { return null; }
		public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression Power(Expression left, Expression right) { return null; }
		public static BinaryExpression Power(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression PowerAssign(Expression left, Expression right) { return null; }
		public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static BinaryExpression ArrayIndex(Expression array, Expression index) { return null; }

		public static BlockExpression Block(Expression arg0, Expression arg1) { return null; }
		public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) { return null; }
		public static BlockExpression Block(params Expression[] expressions) { return null; }
		public static BlockExpression Block(IEnumerable<Expression> expressions) { return null; }
		public static BlockExpression Block(Type type, params Expression[] expressions) { return null; }
		public static BlockExpression Block(Type type, IEnumerable<Expression> expressions) { return null; }
		public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions) { return null; }
		public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, params Expression[] expressions) { return null; }
		public static BlockExpression Block(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) { return null; }
		public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) { return null; }

		public static CatchBlock Catch(Type type, Expression body) { return null; }
		public static CatchBlock Catch(ParameterExpression variable, Expression body) { return null; }
		public static CatchBlock Catch(Type type, Expression body, Expression filter) { return null; }
		public static CatchBlock Catch(ParameterExpression variable, Expression body, Expression filter) { return null; }
		public static CatchBlock MakeCatchBlock(Type type, ParameterExpression variable, Expression body, Expression filter) { return null; }

		public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) { return null; }
		public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) { return null; }
		public static ConditionalExpression IfThen(Expression test, Expression ifTrue) { return null; }
		public static ConditionalExpression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse) { return null; }

		public static ConstantExpression Constant(object value) { return null; }
		public static ConstantExpression Constant(object value, Type type) { return null; }

		public static DefaultExpression Empty() { return null; }
		public static DefaultExpression Default(Type type) { return null; }

		public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) { return null; }
		public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) { return null; }
		public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0) { return null; }
		public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) { return null; }
		public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) { return null; }
		public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) { return null; }
		public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) { return null; }
		public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) { return null; }

		public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) { return null; }
		public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) { return null; }

		public static GotoExpression Break(LabelTarget target) { return null; }
		public static GotoExpression Break(LabelTarget target, Expression value) { return null; }
		public static GotoExpression Break(LabelTarget target, Type type) { return null; }
		public static GotoExpression Break(LabelTarget target, Expression value, Type type) { return null; }
		public static GotoExpression Continue(LabelTarget target) { return null; }
		public static GotoExpression Continue(LabelTarget target, Type type) { return null; }
		public static GotoExpression Return(LabelTarget target) { return null; }
		public static GotoExpression Return(LabelTarget target, Type type) { return null; }
		public static GotoExpression Return(LabelTarget target, Expression value) { return null; }
		public static GotoExpression Return(LabelTarget target, Expression value, Type type) { return null; }
		public static GotoExpression Goto(LabelTarget target) { return null; }
		public static GotoExpression Goto(LabelTarget target, Type type) { return null; }
		public static GotoExpression Goto(LabelTarget target, Expression value) { return null; }
		public static GotoExpression Goto(LabelTarget target, Expression value, Type type) { return null; }
		public static GotoExpression MakeGoto(GotoExpressionKind kind, LabelTarget target, Expression value, Type type) { return null; }

		public static IndexExpression MakeIndex(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }
		public static IndexExpression ArrayAccess(Expression array, params Expression[] indexes) { return null; }
		public static IndexExpression ArrayAccess(Expression array, IEnumerable<Expression> indexes) { return null; }
		public static IndexExpression Property(Expression instance, string propertyName, params Expression[] arguments) { return null; }
		public static IndexExpression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) { return null; }
		public static IndexExpression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }

		public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) { return null; }
		public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments) { return null; }

		public static LabelExpression Label(LabelTarget target) { return null; }
		public static LabelExpression Label(LabelTarget target, Expression defaultValue) { return null; }

		public static LabelTarget Label() { return null; }
		public static LabelTarget Label(string name) { return null; }
		public static LabelTarget Label(Type type) { return null; }
		public static LabelTarget Label(Type type, string name) { return null; }

		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) { return null; }
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, bool tailCall, params ParameterExpression[] parameters) { return null; }
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters) { return null; }
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, bool tailCall, IEnumerable<ParameterExpression> parameters) { return null; }
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, string name, IEnumerable<ParameterExpression> parameters) { return null; }
		public static Expression<TDelegate> Lambda<TDelegate>(Expression body, string name, bool tailCall, IEnumerable<ParameterExpression> parameters) { return null; }

		public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) { return null; }
		public static LambdaExpression Lambda(Expression body, bool tailCall, params ParameterExpression[] parameters) { return null; }
		public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Expression body, bool tailCall, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters) { return null; }
		public static LambdaExpression Lambda(Type delegateType, Expression body, bool tailCall, params ParameterExpression[] parameters) { return null; }
		public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Type delegateType, Expression body, bool tailCall, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Expression body, string name, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Expression body, string name, bool tailCall, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Type delegateType, Expression body, string name, IEnumerable<ParameterExpression> parameters) { return null; }
		public static LambdaExpression Lambda(Type delegateType, Expression body, string name, bool tailCall, IEnumerable<ParameterExpression> parameters) { return null; }

		public static Type GetFuncType(params Type[] typeArgs) { return null; }
		public static bool TryGetFuncType(Type[] typeArgs, out Type funcType) { funcType = null; return false; }
		public static Type GetActionType(params Type[] typeArgs) { return null; }
		public static bool TryGetActionType(Type[] typeArgs, out Type actionType) { actionType = null; return false; }
		public static Type GetDelegateType(params Type[] typeArgs) { return null; }

		public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers) { return null; }
		public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers) { return null; }
		public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers) { return null; }
		public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers) { return null; }
		public static ListInitExpression ListInit(NewExpression newExpression, params ElementInit[] initializers) { return null; }
		public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers) { return null; }

		public static LoopExpression Loop(Expression body) { return null; }
		public static LoopExpression Loop(Expression body, LabelTarget @break) { return null; }
		public static LoopExpression Loop(Expression body, LabelTarget @break, LabelTarget @continue) { return null; }

		public static MemberAssignment Bind(MemberInfo member, Expression expression) { return null; }
		public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression) { return null; }

		public static MemberExpression Field(Expression expression, FieldInfo field) { return null; }
		public static MemberExpression Field(Expression expression, string fieldName) { return null; }
		public static MemberExpression Field(Expression expression, Type type, string fieldName) { return null; }
		public static MemberExpression Property(Expression expression, string propertyName) { return null; }
		public static MemberExpression Property(Expression expression, Type type, string propertyName) { return null; }
		public static MemberExpression Property(Expression expression, PropertyInfo property) { return null; }
		public static MemberExpression Property(Expression expression, MethodInfo propertyAccessor) { return null; }
		public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName) { return null; }
		public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member) { return null; }

		public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) { return null; }
		public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) { return null; }

		public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers) { return null; }
		public static MemberListBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) { return null; }
		public static MemberListBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) { return null; }
		public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) { return null; }

		public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings) { return null; }
		public static MemberMemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings) { return null; }
		public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings) { return null; }
		public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings) { return null; }

		public static MethodCallExpression Call(MethodInfo method, Expression arg0) { return null; }
		public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1) { return null; }
		public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) { return null; }
		public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) { return null; }
		public static MethodCallExpression Call(MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		public static MethodCallExpression Call(Expression instance, MethodInfo method) { return null; }
		public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments) { return null; }
		public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1) { return null; }
		public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments) { return null; }
		public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments) { return null; }
		public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) { return null; }
		public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes) { return null; }

		public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers) { return null; }
		public static NewArrayExpression NewArrayInit(Type type, IEnumerable<Expression> initializers) { return null; }
		public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds) { return null; }
		public static NewArrayExpression NewArrayBounds(Type type, IEnumerable<Expression> bounds) { return null; }

		public static NewExpression New(ConstructorInfo constructor) { return null; }
		public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members) { return null; }
		public static NewExpression New(Type type) { return null; }

		public static ParameterExpression Parameter(Type type) { return null; }
		public static ParameterExpression Variable(Type type) { return null; }
		public static ParameterExpression Parameter(Type type, string name) { return null; }
		public static ParameterExpression Variable(Type type, string name) { return null; }

		public static RuntimeVariablesExpression RuntimeVariables(params ParameterExpression[] variables) { return null; }
		public static RuntimeVariablesExpression RuntimeVariables(IEnumerable<ParameterExpression> variables) { return null; }

		public static SwitchCase SwitchCase(Expression body, params Expression[] testValues) { return null; }
		public static SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues) { return null; }

		public static SwitchExpression Switch(Expression switchValue, params SwitchCase[] cases) { return null; }
		public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, params SwitchCase[] cases) { return null; }
		public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) { return null; }
		public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) { return null; }
		public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) { return null; }
		public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) { return null; }

		public static TryExpression TryFault(Expression body, Expression fault) { return null; }
		public static TryExpression TryFinally(Expression body, Expression @finally) { return null; }
		public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers) { return null; }
		public static TryExpression TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers) { return null; }
		public static TryExpression MakeTry(Type type, Expression body, Expression @finally, Expression fault, IEnumerable<CatchBlock> handlers) { return null; }

		public static TypeBinaryExpression TypeIs(Expression expression, Type type) { return null; }
		public static TypeBinaryExpression TypeEqual(Expression expression, Type type) { return null; }

		public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) { return null; }
		public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) { return null; }
		public static UnaryExpression Negate(Expression expression) { return null; }
		public static UnaryExpression Negate(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression UnaryPlus(Expression expression) { return null; }
		public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression NegateChecked(Expression expression) { return null; }
		public static UnaryExpression NegateChecked(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression Not(Expression expression) { return null; }
		public static UnaryExpression Not(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression IsFalse(Expression expression) { return null; }
		public static UnaryExpression IsFalse(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression IsTrue(Expression expression) { return null; }
		public static UnaryExpression IsTrue(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression OnesComplement(Expression expression) { return null; }
		public static UnaryExpression OnesComplement(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression TypeAs(Expression expression, Type type) { return null; }
		public static UnaryExpression Unbox(Expression expression, Type type) { return null; }
		public static UnaryExpression Convert(Expression expression, Type type) { return null; }
		public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method) { return null; }
		public static UnaryExpression ConvertChecked(Expression expression, Type type) { return null; }
		public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method) { return null; }
		public static UnaryExpression ArrayLength(Expression array) { return null; }
		public static UnaryExpression Quote(Expression expression) { return null; }
		public static UnaryExpression Rethrow() { return null; }
		public static UnaryExpression Rethrow(Type type) { return null; }
		public static UnaryExpression Throw(Expression value) { return null; }
		public static UnaryExpression Throw(Expression value, Type type) { return null; }
		public static UnaryExpression Increment(Expression expression) { return null; }
		public static UnaryExpression Increment(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression Decrement(Expression expression) { return null; }
		public static UnaryExpression Decrement(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression PreIncrementAssign(Expression expression) { return null; }
		public static UnaryExpression PreIncrementAssign(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression PreDecrementAssign(Expression expression) { return null; }
		public static UnaryExpression PreDecrementAssign(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression PostIncrementAssign(Expression expression) { return null; }
		public static UnaryExpression PostIncrementAssign(Expression expression, MethodInfo method) { return null; }
		public static UnaryExpression PostDecrementAssign(Expression expression) { return null; }
		public static UnaryExpression PostDecrementAssign(Expression expression, MethodInfo method) { return null; }
	}
}
