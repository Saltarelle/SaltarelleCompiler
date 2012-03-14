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

    	internal IDictionary<DomRegion, VariableData> variables;
        internal List<NestedFunctionData> nestedFunctions;

        public MethodCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IRuntimeLibrary runtimeLibrary) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
            _compilation = compilation;
            _resolver = resolver;
        	_runtimeLibrary = runtimeLibrary;
        }

        public JsFunctionDefinitionExpression CompileMethod(EntityDeclaration entity, Statement body, IMethod method, MethodImplOptions impl) {
            var usedNames    = new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => "$" + p.Name));
            variables        = new VariableGatherer(_resolver, _namingConvention, _errorReporter).GatherVariables(entity, method, usedNames);
            nestedFunctions  = new NestedFunctionGatherer(_resolver).GatherNestedFunctions(entity);
			var nestedFunctionsDict = nestedFunctions.SelectMany(f => f.SelfAndDirectlyOrIndirectlyNestedFunctions).ToDictionary(f => f.ResolveResult);
			var bodyCompiler = new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, variables, nestedFunctionsDict, _runtimeLibrary);

            return JsExpression.FunctionDefinition(method.Parameters.Select(p => variables[p.Region].Name), bodyCompiler.Compile(body), null);
        }
    }
}
