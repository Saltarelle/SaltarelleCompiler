using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;

namespace Saltarelle.Compiler {
	public class ResolveResultVisitor : IResolveResultVisitor {
        public virtual void VisitResolveResult(ResolveResult rr) {
			this.DefaultVisitResolveResult(rr);
        }

        protected virtual void VisitChildResolveResults(ResolveResult rr) {
			this.DefaultVisitChildResolveResults(rr);
        }

        public virtual void VisitDefaultResolveResult(ResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitTypeOfResolveResult(TypeOfResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitThisResolveResult(ThisResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitOperatorResolveResult(OperatorResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitMemberResolveResult(MemberResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitLocalResolveResult(LocalResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitMethodGroupResolveResult(MethodGroupResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitLambdaResolveResult(LambdaResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitInvocationResolveResult(InvocationResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitConversionResolveResult(ConversionResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitConstantResolveResult(ConstantResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitByReferenceResolveResult(ByReferenceResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitArrayCreateResolveResult(ArrayCreateResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        public virtual void VisitArrayAccessResolveResult(ArrayAccessResolveResult rr) {
            VisitChildResolveResults(rr);
        }
	}
}
