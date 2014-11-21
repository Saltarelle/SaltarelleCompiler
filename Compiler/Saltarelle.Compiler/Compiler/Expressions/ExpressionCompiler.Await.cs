using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private JsExpression CompileAwait(AwaitExpressionSyntax node) {
			var awaitInfo = _semanticModel.GetAwaitExpressionInfo(node);

			JsExpression operand;
			if (awaitInfo.IsDynamic) {
				_errorReporter.Message(Messages._7541);
				return JsExpression.Null;
			}
			else {
				bool isExtensionMethod = awaitInfo.GetAwaiterMethod.Parameters.Length == 1;
				operand = CompileMethodInvocation(awaitInfo.GetAwaiterMethod, usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes), IsReadonlyField(node.Expression), isExtensionMethod ? ArgumentMap.CreateIdentity(node.Expression) : ArgumentMap.Empty, false);
			}
			var temp = _createTemporaryVariable();
			_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, operand));
			operand = JsExpression.Identifier(_variables[temp].Name);

			if (awaitInfo.IsDynamic || awaitInfo.GetAwaiterMethod.ReturnType.TypeKind == TypeKind.Dynamic) {
				_additionalStatements.Add(JsStatement.Await(operand, "onCompleted"));
				return JsExpression.Invocation(JsExpression.Member(operand, "getResult"));
			}
			else {
				var getResultMethodImpl = _metadataImporter.GetMethodSemantics(awaitInfo.GetResultMethod.OriginalDefinition);

				var onCompletedMethod =    (IMethodSymbol)awaitInfo.GetAwaiterMethod.ReturnType.FindImplementationForInterfaceMember(_compilation.GetTypeByMetadataName(typeof(System.Runtime.CompilerServices.ICriticalNotifyCompletion).FullName).GetMembers("UnsafeOnCompleted").Single())
				                        ?? (IMethodSymbol)awaitInfo.GetAwaiterMethod.ReturnType.FindImplementationForInterfaceMember(_compilation.GetTypeByMetadataName(typeof(System.Runtime.CompilerServices.INotifyCompletion).FullName).GetMembers("OnCompleted").Single());
				var onCompletedMethodImpl = _metadataImporter.GetMethodSemantics(onCompletedMethod.OriginalDefinition);
	
				if (onCompletedMethodImpl.Type != MethodScriptSemantics.ImplType.NormalMethod) {
					_errorReporter.Message(Messages._7535);
					return JsExpression.Null;
				}
	
				_additionalStatements.Add(JsStatement.Await(operand, onCompletedMethodImpl.Name));
				return CompileMethodInvocation(getResultMethodImpl, awaitInfo.GetResultMethod, new[] { operand }, false);
			}
		}
	}
}
