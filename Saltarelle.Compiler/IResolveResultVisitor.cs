using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;

namespace Saltarelle.Compiler {
	public interface IResolveResultVisitor {
        void VisitResolveResult(ResolveResult rr);
        void VisitDefaultResolveResult(ResolveResult rr);
        void VisitTypeOfResolveResult(TypeOfResolveResult rr);
        void VisitThisResolveResult(ThisResolveResult rr);
        void VisitOperatorResolveResult(OperatorResolveResult rr);
        void VisitMemberResolveResult(MemberResolveResult rr);
        void VisitLocalResolveResult(LocalResolveResult rr);
        void VisitMethodGroupResolveResult(MethodGroupResolveResult rr);
        void VisitLambdaResolveResult(LambdaResolveResult rr);
        void VisitInvocationResolveResult(InvocationResolveResult rr);
        void VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr);
        void VisitConversionResolveResult(ConversionResolveResult rr);
        void VisitConstantResolveResult(ConstantResolveResult rr);
        void VisitByReferenceResolveResult(ByReferenceResolveResult rr);
        void VisitArrayCreateResolveResult(ArrayCreateResolveResult rr);
        void VisitArrayAccessResolveResult(ArrayAccessResolveResult rr);
	}

	public static class ResolveResultVisitorExtensions {
		public static void DefaultVisitResolveResult(this IResolveResultVisitor visitor, ResolveResult rr) {
            if (rr is ArrayAccessResolveResult) {
                visitor.VisitArrayAccessResolveResult((ArrayAccessResolveResult)rr);
            }
            else if (rr is ArrayCreateResolveResult) {
                visitor.VisitArrayCreateResolveResult((ArrayCreateResolveResult)rr);
            }
            else if (rr is ByReferenceResolveResult) {
                visitor.VisitByReferenceResolveResult((ByReferenceResolveResult)rr);
            }
            else if (rr is ConstantResolveResult) {
                visitor.VisitConstantResolveResult((ConstantResolveResult)rr);
            }
            else if (rr is ConversionResolveResult) {
                visitor.VisitConversionResolveResult((ConversionResolveResult)rr);
            }
            else if (rr is CSharpInvocationResolveResult) {
                // TODO: What is this? Might be needed for named arguments
                visitor.VisitCSharpInvocationResolveResult((CSharpInvocationResolveResult)rr);
            }
            else if (rr is InvocationResolveResult) {
                visitor.VisitInvocationResolveResult((InvocationResolveResult)rr);
            }
            else if (rr is LambdaResolveResult) {
                visitor.VisitLambdaResolveResult((LambdaResolveResult)rr);
            }
            else if (rr is MethodGroupResolveResult) {
                // TODO: Is this really needed?
                visitor.VisitMethodGroupResolveResult((MethodGroupResolveResult)rr);
            }
            else if (rr is LocalResolveResult) {
                visitor.VisitLocalResolveResult((LocalResolveResult)rr);
            }
            else if (rr is MemberResolveResult) {
                visitor.VisitMemberResolveResult((MemberResolveResult)rr);
            }
            else if (rr is OperatorResolveResult) {
                visitor.VisitOperatorResolveResult((OperatorResolveResult)rr);
            }
            else if (rr is ThisResolveResult) {
                visitor.VisitThisResolveResult((ThisResolveResult)rr);
            }
            else if (rr is TypeOfResolveResult) {
                visitor.VisitTypeOfResolveResult((TypeOfResolveResult)rr);
            }
            else {
                visitor.VisitDefaultResolveResult(rr);
            }
        }

        public static void DefaultVisitChildResolveResults(this IResolveResultVisitor visitor, ResolveResult rr) {
            foreach (var r in rr.GetChildResults())
                visitor.VisitResolveResult(r);
        }
	}
}
