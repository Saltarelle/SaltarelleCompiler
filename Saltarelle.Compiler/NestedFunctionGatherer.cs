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
        private class StructureGatherer : DepthFirstAstVisitor {
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

            public override void VisitLambdaExpression(LambdaExpression lambdaExpression) {
                VisitNestedFunction(lambdaExpression, lambdaExpression.Body);
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression) {
                VisitNestedFunction(anonymousMethodExpression, anonymousMethodExpression.Body);
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

            public override object VisitThisResolveResult(ThisResolveResult rr, object data) {
                _usesThis = true;
                return base.VisitThisResolveResult(rr, data);
            }

            public override object VisitLocalResolveResult(LocalResolveResult rr, object data) {
                _usedVariables.Add(rr.Variable);
                return base.VisitLocalResolveResult(rr, data);
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
