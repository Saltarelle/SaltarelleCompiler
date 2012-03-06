using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;

namespace Saltarelle.Compiler {
	public interface IResolveResultVisitor<out TResult, in TData> {
        TResult VisitResolveResult(ResolveResult rr, TData data);
        TResult VisitDefaultResolveResult(ResolveResult rr, TData data);
        TResult VisitTypeOfResolveResult(TypeOfResolveResult rr, TData data);
        TResult VisitThisResolveResult(ThisResolveResult rr, TData data);
        TResult VisitOperatorResolveResult(OperatorResolveResult rr, TData data);
        TResult VisitMemberResolveResult(MemberResolveResult rr, TData data);
        TResult VisitLocalResolveResult(LocalResolveResult rr, TData data);
        TResult VisitMethodGroupResolveResult(MethodGroupResolveResult rr, TData data);
        TResult VisitLambdaResolveResult(LambdaResolveResult rr, TData data);
        TResult VisitInvocationResolveResult(InvocationResolveResult rr, TData data);
        TResult VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, TData data);
        TResult VisitConversionResolveResult(ConversionResolveResult rr, TData data);
        TResult VisitConstantResolveResult(ConstantResolveResult rr, TData data);
        TResult VisitByReferenceResolveResult(ByReferenceResolveResult rr, TData data);
        TResult VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, TData data);
        TResult VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, TData data);
		TResult VisitTypeResolveResult(TypeResolveResult rr, TData data);
	}

	public static class ResolveResultVisitorExtensions {
		public static TResult DefaultVisitResolveResult<TResult, TData>(this IResolveResultVisitor<TResult, TData> visitor, ResolveResult rr, TData data) {
            if (rr is ArrayAccessResolveResult) {
                return visitor.VisitArrayAccessResolveResult((ArrayAccessResolveResult)rr, data);
            }
            else if (rr is ArrayCreateResolveResult) {
                return visitor.VisitArrayCreateResolveResult((ArrayCreateResolveResult)rr, data);
            }
            else if (rr is ByReferenceResolveResult) {
                return visitor.VisitByReferenceResolveResult((ByReferenceResolveResult)rr, data);
            }
            else if (rr is ConstantResolveResult) {
                return visitor.VisitConstantResolveResult((ConstantResolveResult)rr, data);
            }
            else if (rr is ConversionResolveResult) {
                return visitor.VisitConversionResolveResult((ConversionResolveResult)rr, data);
            }
            else if (rr is CSharpInvocationResolveResult) {
                // TODO: What is this? Might be needed for named arguments
                return visitor.VisitCSharpInvocationResolveResult((CSharpInvocationResolveResult)rr, data);
            }
            else if (rr is InvocationResolveResult) {
                return visitor.VisitInvocationResolveResult((InvocationResolveResult)rr, data);
            }
            else if (rr is LambdaResolveResult) {
                return visitor.VisitLambdaResolveResult((LambdaResolveResult)rr, data);
            }
            else if (rr is MethodGroupResolveResult) {
                // TODO: Is this really needed?
                return visitor.VisitMethodGroupResolveResult((MethodGroupResolveResult)rr, data);
            }
            else if (rr is LocalResolveResult) {
                return visitor.VisitLocalResolveResult((LocalResolveResult)rr, data);
            }
            else if (rr is MemberResolveResult) {
                return visitor.VisitMemberResolveResult((MemberResolveResult)rr, data);
            }
            else if (rr is OperatorResolveResult) {
                return visitor.VisitOperatorResolveResult((OperatorResolveResult)rr, data);
            }
            else if (rr is ThisResolveResult) {
                return visitor.VisitThisResolveResult((ThisResolveResult)rr, data);
            }
            else if (rr is TypeOfResolveResult) {
                return visitor.VisitTypeOfResolveResult((TypeOfResolveResult)rr, data);
            }
			else if (rr is TypeResolveResult) {
				return visitor.VisitTypeResolveResult((TypeResolveResult)rr, data);
			}
            else {
                return visitor.VisitDefaultResolveResult(rr, data);
            }
        }

        public static void DefaultVisitChildResolveResults<TResult, TData>(this IResolveResultVisitor<TResult, TData> visitor, ResolveResult rr, TData data) {
            foreach (var r in rr.GetChildResults())
                visitor.VisitResolveResult(r, data);
        }
	}
}
