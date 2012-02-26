using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler
{
    public class NestedFunctionGatherer {
        private class StructureGatherer : DepthFirstAstVisitor<object, object> {
            private readonly CSharpAstResolver _resolver;
            private List<NestedFunctionData> currentList;

            public StructureGatherer(CSharpAstResolver resolver) {
                _resolver = resolver;
            }

            public List<NestedFunctionData> GatherNestedFunctions(AstNode node) {
                currentList = new List<NestedFunctionData>();
                node.AcceptVisitor(this);
                return currentList;
            }

            private void VisitNestedFunction(AstNode node, AstNode body) {
                var parentList = currentList;

                currentList = new List<NestedFunctionData>();
                body.AcceptVisitor(this);
                var newData = new NestedFunctionData { DefinitionNode = node, BodyNode = body, ResolveResult = (LambdaResolveResult)_resolver.Resolve(node) };
                foreach (var x in currentList)
                    newData.NestedFunctions.Add(x);
                parentList.Add(newData);

                currentList = parentList;
            }

            public override object VisitLambdaExpression(LambdaExpression lambdaExpression, object data) {
                VisitNestedFunction(lambdaExpression, lambdaExpression.Body);
                return null;
            }

            public override object VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, object data) {
                VisitNestedFunction(anonymousMethodExpression, anonymousMethodExpression.Body);
                return null;
            }
        }

        private class CaptureAnalyzer : CombinedAstAndResolveResultVisitor {
            private bool _usesThis;
            private HashSet<IVariable> _usedVariables = new HashSet<IVariable>();

            public bool UsesThis { get { return _usesThis; } }
            public HashSet<IVariable> UsedVariables { get { return _usedVariables; } }

            public CaptureAnalyzer(CSharpAstResolver resolver) : base(resolver) {
            }

            public void Analyze(AstNode node) {
                _usesThis = false;
                _usedVariables.Clear();
                node.AcceptVisitor(this);
            }

            public override object VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression, object data) {
                _usesThis = true;
                return base.VisitBaseReferenceExpression(baseReferenceExpression, data);
            }

            protected override void VisitThisResolveResult(ThisResolveResult rr) {
                _usesThis = true;
                base.VisitThisResolveResult(rr);
            }

            protected override void VisitLocalResolveResult(LocalResolveResult rr) {
                _usedVariables.Add(rr.Variable);
                base.VisitLocalResolveResult(rr);
            }
        }

        private readonly CSharpAstResolver _resolver;

        public NestedFunctionGatherer(CSharpAstResolver resolver) {
            _resolver = resolver;
        }

        public List<NestedFunctionData> GatherNestedFunctions(AstNode node) {
            var result = new StructureGatherer(_resolver).GatherNestedFunctions(node);
            var analyzer = new CaptureAnalyzer(_resolver);
            foreach (var f in result.SelectMany(f => f.SelfAndDirectlyOrIndirectlyNestedFunctions)) {
                analyzer.Analyze(f.BodyNode);
                f.DirectlyUsesThis = analyzer.UsesThis;
                foreach (var v in analyzer.UsedVariables)
                    f.DirectlyUsedVariables.Add(v);
                f.Freeze();
            }

            return result;
        }
    }
}
