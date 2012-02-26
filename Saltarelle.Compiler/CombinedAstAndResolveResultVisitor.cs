using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using Attribute = ICSharpCode.NRefactory.CSharp.Attribute;

namespace Saltarelle.Compiler {
    public class CombinedAstAndResolveResultVisitor : DepthFirstAstVisitor<object, object> {
        private CSharpAstResolver _resolver;

        public CombinedAstAndResolveResultVisitor(CSharpAstResolver resolver) {
            _resolver = resolver;
        }

        public override object VisitLambdaExpression(LambdaExpression lambdaExpression, object data) {
            return HandleExpressionNode(lambdaExpression);
        }

        public override object VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, object data) {
            return HandleExpressionNode(anonymousMethodExpression);
        }

        public override object VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression, object data) {
            return HandleExpressionNode(undocumentedExpression);
        }

        public override object VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression, object data) {
            return HandleExpressionNode(arrayInitializerExpression);
        }

        public override object VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression, object data) {
            return HandleExpressionNode(namedArgumentExpression);
        }

        public override object VisitNamedExpression(NamedExpression namedExpression, object data) {
            return HandleExpressionNode(namedExpression);
        }

        public override object VisitEmptyExpression(EmptyExpression emptyExpression, object data) {
            return HandleExpressionNode(emptyExpression);
        }

        public override object VisitIsExpression(IsExpression isExpression, object data) {
            return HandleExpressionNode(isExpression);
        }

        public override object VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression, object data) {
            return HandleExpressionNode(defaultValueExpression);
        }

        public override object VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression, object data) {
            return HandleExpressionNode(memberReferenceExpression);
        }

        public override object VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression, object data) {
            return HandleExpressionNode(nullReferenceExpression);
        }

        public override object VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, object data) {
            return HandleExpressionNode(objectCreateExpression);
        }

        public override object VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression, object data) {
            return HandleExpressionNode(anonymousTypeCreateExpression);
        }

        public override object VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression, object data) {
            return HandleExpressionNode(parenthesizedExpression);
        }

        public override object VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression, object data) {
            return HandleExpressionNode(pointerReferenceExpression);
        }

        public override object VisitPrimitiveExpression(PrimitiveExpression primitiveExpression, object data) {
            return HandleExpressionNode(primitiveExpression);
        }

        public override object VisitSizeOfExpression(SizeOfExpression sizeOfExpression, object data) {
            return HandleExpressionNode(sizeOfExpression);
        }

        public override object VisitStackAllocExpression(StackAllocExpression stackAllocExpression, object data) {
            return HandleExpressionNode(stackAllocExpression);
        }

        public override object VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression, object data) {
            return HandleExpressionNode(thisReferenceExpression);
        }

        public override object VisitTypeOfExpression(TypeOfExpression typeOfExpression, object data) {
            return HandleExpressionNode(typeOfExpression);
        }

        public override object VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression, object data) {
            return HandleExpressionNode(typeReferenceExpression);
        }

        public override object VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, object data) {
            return HandleExpressionNode(unaryOperatorExpression);
        }

        public override object VisitUncheckedExpression(UncheckedExpression uncheckedExpression, object data) {
            return HandleExpressionNode(uncheckedExpression);
        }

        public override object VisitQueryExpression(QueryExpression queryExpression, object data) {
            return HandleExpressionNode(queryExpression);
        }

        public override object VisitAsExpression(AsExpression asExpression, object data) {
            return HandleExpressionNode(asExpression);
        }

        public override object VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression, object data) {
            return HandleExpressionNode(arrayCreateExpression);
        }

        public override object VisitAssignmentExpression(AssignmentExpression assignmentExpression, object data) {
            return HandleExpressionNode(assignmentExpression);
        }

        public override object VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression, object data) {
            return HandleExpressionNode(baseReferenceExpression);
        }

        public override object VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, object data) {
            return HandleExpressionNode(binaryOperatorExpression);
        }

        public override object VisitCastExpression(CastExpression castExpression, object data) {
            return HandleExpressionNode(castExpression);
        }

        public override object VisitCheckedExpression(CheckedExpression checkedExpression, object data) {
            return HandleExpressionNode(checkedExpression);
        }

        public override object VisitConditionalExpression(ConditionalExpression conditionalExpression, object data) {
            return HandleExpressionNode(conditionalExpression);
        }

        public override object VisitDirectionExpression(DirectionExpression directionExpression, object data) {
            return HandleExpressionNode(directionExpression);
        }

        public override object VisitIdentifierExpression(IdentifierExpression identifierExpression, object data) {
            return HandleExpressionNode(identifierExpression);
        }

        public override object VisitIndexerExpression(IndexerExpression indexerExpression, object data) {
            return HandleExpressionNode(indexerExpression);
        }

        public override object VisitInvocationExpression(InvocationExpression invocationExpression, object data) {
            return HandleExpressionNode(invocationExpression);
        }

        protected object HandleExpressionNode(AstNode node) {
            var rr = _resolver.Resolve(node);
            VisitResolveResult(rr);
			return null;
        }

        protected virtual void VisitResolveResult(ResolveResult rr) {
            if (rr is ArrayAccessResolveResult) {
                VisitArrayAccessResolveResult((ArrayAccessResolveResult)rr);
            }
            else if (rr is ArrayCreateResolveResult) {
                VisitArrayCreateResolveResult((ArrayCreateResolveResult)rr);
            }
            else if (rr is ByReferenceResolveResult) {
                VisitByReferenceResolveResult((ByReferenceResolveResult)rr);
            }
            else if (rr is ConstantResolveResult) {
                VisitConstantResolveResult((ConstantResolveResult)rr);
            }
            else if (rr is ConversionResolveResult) {
                VisitConversionResolveResult((ConversionResolveResult)rr);
            }
            else if (rr is CSharpInvocationResolveResult) {
                // TODO: What is this? Might be needed for named arguments
                VisitCSharpInvocationResolveResult((CSharpInvocationResolveResult)rr);
            }
            else if (rr is InvocationResolveResult) {
                VisitInvocationResolveResult((InvocationResolveResult)rr);
            }
            else if (rr is LambdaResolveResult) {
                VisitLambdaResolveResult((LambdaResolveResult)rr);
            }
            else if (rr is MethodGroupResolveResult) {
                // TODO: Is this really needed?
                VisitMethodGroupResolveResult((MethodGroupResolveResult)rr);
            }
            else if (rr is LocalResolveResult) {
                VisitLocalResolveResult((LocalResolveResult)rr);
            }
            else if (rr is MemberResolveResult) {
                VisitMemberResolveResult((MemberResolveResult)rr);
            }
            else if (rr is OperatorResolveResult) {
                VisitOperatorResolveResult((OperatorResolveResult)rr);
            }
            else if (rr is ThisResolveResult) {
                VisitThisResolveResult((ThisResolveResult)rr);
            }
            else if (rr is TypeOfResolveResult) {
                VisitTypeOfResolveResult((TypeOfResolveResult)rr);
            }
            else {
                VisitDefaultResolveResult(rr);
            }
        }

        protected virtual void VisitChildResolveResults(ResolveResult rr) {
            foreach (var r in rr.GetChildResults())
                VisitResolveResult(r);
        }

        protected virtual void VisitDefaultResolveResult(ResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitTypeOfResolveResult(TypeOfResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitThisResolveResult(ThisResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitOperatorResolveResult(OperatorResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitMemberResolveResult(MemberResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitLocalResolveResult(LocalResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitMethodGroupResolveResult(MethodGroupResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitLambdaResolveResult(LambdaResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitInvocationResolveResult(InvocationResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitConversionResolveResult(ConversionResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitConstantResolveResult(ConstantResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitByReferenceResolveResult(ByReferenceResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitArrayCreateResolveResult(ArrayCreateResolveResult rr) {
            VisitChildResolveResults(rr);
        }

        protected virtual void VisitArrayAccessResolveResult(ArrayAccessResolveResult rr) {
            VisitChildResolveResults(rr);
        }
    }
}
