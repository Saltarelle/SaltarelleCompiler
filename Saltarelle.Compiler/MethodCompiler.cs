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
    public class MethodCompiler : DepthFirstAstVisitor<object, object> {
        private readonly INamingConventionResolver _namingConvention;
        private readonly IErrorReporter _errorReporter;
        private ICompilation _compilation;
        private readonly CSharpAstResolver _resolver;

        internal IDictionary<IVariable, VariableData> variables;

        public MethodCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
            _compilation = compilation;
            _resolver = resolver;
        }

        public JsFunctionDefinitionExpression CompileMethod(AttributedNode methodNode, IMethod method, MethodImplOptions impl) {
            var usedNames = new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => "$" + p.Name));
            variables     = new VariableGatherer(_resolver, _namingConvention, _errorReporter).GatherVariables(methodNode, method, usedNames);

        	methodNode.AcceptVisitor(this);

            return new JsFunctionDefinitionExpression(method.Parameters.Select(p => variables[p].Name), JsBlockStatement.EmptyStatement, "X");
        }
    }
}
