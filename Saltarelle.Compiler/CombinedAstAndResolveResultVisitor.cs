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
    public class CombinedAstAndResolveResultVisitor : DepthFirstAstVisitor, IResolveResultVisitor {
        private CSharpAstResolver _resolver;

        public CombinedAstAndResolveResultVisitor(CSharpAstResolver resolver) {
            _resolver = resolver;
        }

        public override void VisitLambdaExpression(LambdaExpression lambdaExpression) {
            HandleExpressionNode(lambdaExpression);
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression) {
            HandleExpressionNode(anonymousMethodExpression);
        }

        public override void VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression) {
            HandleExpressionNode(undocumentedExpression);
        }

        public override void VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression) {
            HandleExpressionNode(arrayInitializerExpression);
        }

        public override void VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression) {
            HandleExpressionNode(namedArgumentExpression);
        }

        public override void VisitNamedExpression(NamedExpression namedExpression) {
            HandleExpressionNode(namedExpression);
        }

        public override void VisitEmptyExpression(EmptyExpression emptyExpression) {
            HandleExpressionNode(emptyExpression);
        }

        public override void VisitIsExpression(IsExpression isExpression) {
            HandleExpressionNode(isExpression);
        }

        public override void VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression) {
            HandleExpressionNode(defaultValueExpression);
        }

        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression) {
            HandleExpressionNode(memberReferenceExpression);
        }

        public override void VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression) {
            HandleExpressionNode(nullReferenceExpression);
        }

        public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression) {
            HandleExpressionNode(objectCreateExpression);
        }

        public override void VisitAnonymousTypeCreateExpression(AnonymousTypeCreateExpression anonymousTypeCreateExpression) {
            HandleExpressionNode(anonymousTypeCreateExpression);
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression) {
            HandleExpressionNode(parenthesizedExpression);
        }

        public override void VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression) {
            HandleExpressionNode(pointerReferenceExpression);
        }

        public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression) {
            HandleExpressionNode(primitiveExpression);
        }

        public override void VisitSizeOfExpression(SizeOfExpression sizeOfExpression) {
            HandleExpressionNode(sizeOfExpression);
        }

        public override void VisitStackAllocExpression(StackAllocExpression stackAllocExpression) {
            HandleExpressionNode(stackAllocExpression);
        }

        public override void VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression) {
            HandleExpressionNode(thisReferenceExpression);
        }

        public override void VisitTypeOfExpression(TypeOfExpression typeOfExpression) {
            HandleExpressionNode(typeOfExpression);
        }

        public override void VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression) {
            HandleExpressionNode(typeReferenceExpression);
        }

        public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression) {
            HandleExpressionNode(unaryOperatorExpression);
        }

        public override void VisitUncheckedExpression(UncheckedExpression uncheckedExpression) {
            HandleExpressionNode(uncheckedExpression);
        }

        public override void VisitQueryExpression(QueryExpression queryExpression) {
            HandleExpressionNode(queryExpression);
        }

        public override void VisitAsExpression(AsExpression asExpression) {
            HandleExpressionNode(asExpression);
        }

        public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression) {
            HandleExpressionNode(arrayCreateExpression);
        }

        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression) {
            HandleExpressionNode(assignmentExpression);
        }

        public override void VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression) {
            HandleExpressionNode(baseReferenceExpression);
        }

        public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression) {
            HandleExpressionNode(binaryOperatorExpression);
        }

        public override void VisitCastExpression(CastExpression castExpression) {
            HandleExpressionNode(castExpression);
        }

        public override void VisitCheckedExpression(CheckedExpression checkedExpression) {
            HandleExpressionNode(checkedExpression);
        }

        public override void VisitConditionalExpression(ConditionalExpression conditionalExpression) {
            HandleExpressionNode(conditionalExpression);
        }

        public override void VisitDirectionExpression(DirectionExpression directionExpression) {
            HandleExpressionNode(directionExpression);
        }

        public override void VisitIdentifierExpression(IdentifierExpression identifierExpression) {
            HandleExpressionNode(identifierExpression);
        }

        public override void VisitIndexerExpression(IndexerExpression indexerExpression) {
            HandleExpressionNode(indexerExpression);
        }

        public override void VisitInvocationExpression(InvocationExpression invocationExpression) {
            HandleExpressionNode(invocationExpression);
        }

        protected void HandleExpressionNode(AstNode node) {
            var rr = _resolver.Resolve(node);
            VisitResolveResult(rr);
        }

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
