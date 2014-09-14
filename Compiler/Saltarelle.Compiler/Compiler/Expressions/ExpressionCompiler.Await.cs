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
		private JsExpression CompileAwait(PrefixUnaryExpressionSyntax node) {
			var awaitInfo = _semanticModel.GetAwaitExpressionInfo(node);

			JsExpression operand;
			if (awaitInfo.IsDynamic) {
				// If the GetAwaiter call is dynamic, we need to camel-case it.
				operand = JsExpression.Invocation(JsExpression.Member(InnerCompile(node.Operand, false), "getAwaiter"));
			}
			else {
				bool isExtensionMethod = awaitInfo.GetAwaiterMethod.Parameters.Length == 1;
				var sem = _metadataImporter.GetMethodSemantics(awaitInfo.GetAwaiterMethod);
				operand = CompileMethodInvocation(sem, awaitInfo.GetAwaiterMethod, usedMultipleTimes => InnerCompile(node.Operand, usedMultipleTimes), IsReadonlyField(node.Operand), isExtensionMethod ? ArgumentMap.CreateIdentity(node.Operand) : ArgumentMap.Empty, false);
			}
			var temp = _createTemporaryVariable();
			_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, operand));
			operand = JsExpression.Identifier(_variables[temp].Name);

			if (awaitInfo.IsDynamic || awaitInfo.GetAwaiterMethod.ReturnType.TypeKind == TypeKind.DynamicType) {
				_additionalStatements.Add(JsStatement.Await(operand, "onCompleted"));
				return JsExpression.Invocation(JsExpression.Member(operand, "getResult"));
			}
			else {
				var getResultMethodImpl = _metadataImporter.GetMethodSemantics(awaitInfo.GetResultMethod);

				var onCompletedMethod =    (IMethodSymbol)awaitInfo.GetAwaiterMethod.ReturnType.FindImplementationForInterfaceMember(_compilation.GetTypeByMetadataName(typeof(System.Runtime.CompilerServices.ICriticalNotifyCompletion).FullName).GetMembers("UnsafeOnCompleted").Single())
				                        ?? (IMethodSymbol)awaitInfo.GetAwaiterMethod.ReturnType.FindImplementationForInterfaceMember(_compilation.GetTypeByMetadataName(typeof(System.Runtime.CompilerServices.INotifyCompletion).FullName).GetMembers("OnCompleted").Single());
				var onCompletedMethodImpl = _metadataImporter.GetMethodSemantics(onCompletedMethod);
	
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
