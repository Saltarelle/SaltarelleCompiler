using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;

namespace Saltarelle.Compiler {
	public class ResolveResultVisitor<TResult, TData> : IResolveResultVisitor<TResult, TData> {
        public virtual TResult VisitResolveResult(ResolveResult rr, TData data) {
			return this.DefaultVisitResolveResult(rr, data);
        }

        protected virtual void VisitChildResolveResults(ResolveResult rr, TData data) {
			this.DefaultVisitChildResolveResults(rr, data);
        }

        public virtual TResult VisitDefaultResolveResult(ResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitTypeOfResolveResult(TypeOfResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitThisResolveResult(ThisResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitOperatorResolveResult(OperatorResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitMemberResolveResult(MemberResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitLocalResolveResult(LocalResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitMethodGroupResolveResult(MethodGroupResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitLambdaResolveResult(LambdaResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitInvocationResolveResult(InvocationResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitConversionResolveResult(ConversionResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitConstantResolveResult(ConstantResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitByReferenceResolveResult(ByReferenceResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }

        public virtual TResult VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, TData data) {
            VisitChildResolveResults(rr, data);
			return default(TResult);
        }
	}
}
