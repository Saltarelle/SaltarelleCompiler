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
			private NestedFunctionData currentFunction;

            public StructureGatherer(CSharpAstResolver resolver) {
                _resolver = resolver;
            }

			private AstNode GetBodyNode(AstNode methodNode) {
				if (methodNode is AnonymousMethodExpression)
					return ((AnonymousMethodExpression)methodNode).Body;
				else if (methodNode is LambdaExpression)
					return ((LambdaExpression)methodNode).Body;
				else if (methodNode is MethodDeclaration)
					return ((MethodDeclaration)methodNode).Body;
				else
					return methodNode;
			}

            public NestedFunctionData GatherNestedFunctions(AstNode node) {
				currentFunction = new NestedFunctionData(null) { DefinitionNode = node, BodyNode = GetBodyNode(node), ResolveResult = _resolver.Resolve(node) as LambdaResolveResult };
				VisitChildren(node);
                return currentFunction;
            }

            private void VisitNestedFunction(AstNode node, AstNode body) {
                var parentFunction = currentFunction;

                currentFunction = new NestedFunctionData(parentFunction) { DefinitionNode = node, BodyNode = body, ResolveResult = (LambdaResolveResult)_resolver.Resolve(node) };
                body.AcceptVisitor(this);

				parentFunction.NestedFunctions.Add(currentFunction);
                currentFunction = parentFunction;
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

        public NestedFunctionData GatherNestedFunctions(AstNode node, IDictionary<IVariable, VariableData> allVariables) {
            var result = new StructureGatherer(_resolver).GatherNestedFunctions(node);

			var allNestedFunctions = new[] { result }.Concat(result.DirectlyOrIndirectlyNestedFunctions).ToDictionary(f => f.DefinitionNode);
			foreach (var v in allVariables) {
				allNestedFunctions[v.Value.DeclaringMethod].DirectlyDeclaredVariables.Add(v.Key);
			}

            var analyzer = new CaptureAnalyzer(_resolver);
            foreach (var f in allNestedFunctions.Values) {
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
