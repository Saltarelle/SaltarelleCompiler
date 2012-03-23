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
		private StatementCompiler statementCompiler;

        public MethodCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IRuntimeLibrary runtimeLibrary) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
            _compilation = compilation;
            _resolver = resolver;
        	_runtimeLibrary = runtimeLibrary;
        }

		private void CreateCompilationContext(EntityDeclaration entity, IMethod method, string thisAlias) {
            var usedNames           = method != null ? new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => _namingConvention.GetTypeParameterName(p))) : new HashSet<string>();
            variables               = entity != null ? new VariableGatherer(_resolver, _namingConvention, _errorReporter).GatherVariables(entity, method, usedNames) : new Dictionary<IVariable, VariableData>();
            nestedFunctionsRoot     = entity != null ? new NestedFunctionGatherer(_resolver).GatherNestedFunctions(entity, variables) : new NestedFunctionData(null);
			var nestedFunctionsDict = nestedFunctionsRoot.DirectlyOrIndirectlyNestedFunctions.ToDictionary(f => f.ResolveResult);

			statementCompiler = new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, variables, nestedFunctionsDict, _runtimeLibrary, thisAlias, null);
		}

        public JsFunctionDefinitionExpression CompileMethod(EntityDeclaration entity, Statement body, IMethod method, MethodImplOptions impl) {
			CreateCompilationContext(entity, method, (impl.Type == MethodImplOptions.ImplType.StaticMethodWithThisAsFirstArgument ? _namingConvention.ThisAlias : null));
            return JsExpression.FunctionDefinition(method.Parameters.Select(p => variables[p].Name), statementCompiler.Compile(body), null);
        }

        public JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclaration ctor, IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorImplOptions impl) {
			CreateCompilationContext(ctor, constructor, (impl.Type == ConstructorImplOptions.ImplType.StaticMethod ? _namingConvention.ThisAlias : null));
			var body = new List<JsStatement>();

            body.AddRange(instanceInitStatements);

			var systemObject = _compilation.FindType(KnownTypeCode.Object);
			if (impl.Type == ConstructorImplOptions.ImplType.StaticMethod) {
				if (ctor != null && !ctor.Initializer.IsNull) {
					body.AddRange(statementCompiler.CompileConstructorInitializer(ctor.Initializer, true));
				}
				else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
					body.AddRange(statementCompiler.CompileImplicitBaseConstructorCall(constructor.DeclaringType, true));
				}
				else {
					body.Add(new JsVariableDeclarationStatement(_namingConvention.ThisAlias, JsExpression.ObjectLiteral()));
				}
			}
			else {
				if (ctor != null && !ctor.Initializer.IsNull) {
					body.AddRange(statementCompiler.CompileConstructorInitializer(ctor.Initializer, false));
				}
				else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
					body.AddRange(statementCompiler.CompileImplicitBaseConstructorCall(constructor.DeclaringType, false));
				}
			}

            if (ctor != null)
			    body.AddRange(statementCompiler.Compile(ctor.Body).Statements);

			return JsExpression.FunctionDefinition(constructor.Parameters.Select(p => variables[p].Name), new JsBlockStatement(body));
        }

        public JsFunctionDefinitionExpression CompileDefaultConstructor(IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorImplOptions impl) {
            return CompileConstructor(null, constructor, instanceInitStatements, impl);
        }

        public IList<JsStatement> CompileFieldInitializer(JsExpression field, Expression expression) {
            CreateCompilationContext(null, null, null);
            return statementCompiler.CompileFieldInitializer(field, expression);
        }
    }
}
