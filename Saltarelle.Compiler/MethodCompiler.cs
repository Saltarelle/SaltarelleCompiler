using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
    public class MethodCompiler {
        private readonly INamingConventionResolver _namingConvention;
        private readonly IErrorReporter _errorReporter;
        private ICompilation _compilation;
        private readonly CSharpAstResolver _resolver;
    	private readonly IRuntimeLibrary _runtimeLibrary;

    	internal IDictionary<IVariable, VariableData> variables;
        internal NestedFunctionData nestedFunctionsRoot;

        public MethodCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IRuntimeLibrary runtimeLibrary) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
            _compilation = compilation;
            _resolver = resolver;
        	_runtimeLibrary = runtimeLibrary;
        }

        public JsFunctionDefinitionExpression CompileCommon(EntityDeclaration entity, Statement body, IMethod method, string thisAlias) {
            var usedNames      = new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => _namingConvention.GetTypeParameterName(p)));
            variables          = new VariableGatherer(_resolver, _namingConvention, _errorReporter).GatherVariables(entity, method, usedNames);
            nestedFunctionsRoot = new NestedFunctionGatherer(_resolver).GatherNestedFunctions(entity, variables);
			var nestedFunctionsDict = nestedFunctionsRoot.DirectlyOrIndirectlyNestedFunctions.ToDictionary(f => f.ResolveResult);

			var bodyCompiler = new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, variables, nestedFunctionsDict, _runtimeLibrary, thisAlias, null);

            return JsExpression.FunctionDefinition(method.Parameters.Select(p => variables[p].Name), bodyCompiler.Compile(body), null);
		}

        public JsFunctionDefinitionExpression CompileMethod(EntityDeclaration entity, Statement body, IMethod method, MethodImplOptions impl) {
			return CompileCommon(entity, body, method, (impl.Type == MethodImplOptions.ImplType.StaticMethodWithThisAsFirstArgument ? _namingConvention.ThisAlias : null));
        }

        public JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclaration ctor, IMethod method, ConstructorImplOptions impl) {
			var result = CompileCommon(ctor, ctor.Body, method, (impl.Type == ConstructorImplOptions.ImplType.StaticMethod ? _namingConvention.ThisAlias : null));
			if (!ctor.Initializer.IsNull) {
				switch (ctor.Initializer.ConstructorInitializerType) {
					case ConstructorInitializerType.Base: {
						throw new NotImplementedException("Calling base not yet implemented");
					}
					case ConstructorInitializerType.This: {
						break;
					}
					default:
						throw new ArgumentException("Unknown initializer type", "ctor");
				}
			}

			return body;
        }
    }
}
