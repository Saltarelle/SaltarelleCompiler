using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class DynamicExpression : Expression {
		[NonScriptable]
		public CallSiteBinder Binder { get; private set; }

		public Type DelegateType { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		public DynamicExpression Update(IEnumerable<Expression> arguments) { return null; }

		internal DynamicExpression() {}

		public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) { return null; }
		public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) { return null; }
		public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) { return null; }
		public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) { return null; }
		public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) { return null; }
		public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) { return null; }
		public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0) { return null; }
		public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1) { return null; }
		public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
	}
}