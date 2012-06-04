using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
    public class MethodCompiler {
		private class ThisReplacer : RewriterVisitorBase<object> {
			private JsExpression _replaceWith;

			public ThisReplacer(JsExpression replaceWith) {
				_replaceWith = replaceWith;
			}

			public override JsExpression Visit(JsThisExpression expression, object data) {
				return _replaceWith;
			}

			public override JsExpression Visit(JsFunctionDefinitionExpression expression, object data) {
				// Inside a function, "this" is in another context and should thus not be replaced.
				return expression;
			}
		}

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

		private void CreateCompilationContext(AstNode entity, IMethod method, string thisAlias) {
            var usedNames           = method != null ? new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => _namingConvention.GetTypeParameterName(p))) : new HashSet<string>();
            variables               = entity != null ? new VariableGatherer(_resolver, _namingConvention, _errorReporter).GatherVariables(entity, method, usedNames) : new Dictionary<IVariable, VariableData>();
            nestedFunctionsRoot     = entity != null ? new NestedFunctionGatherer(_resolver).GatherNestedFunctions(entity, variables) : new NestedFunctionData(null);
			var nestedFunctionsDict = new[] { nestedFunctionsRoot }.Concat(nestedFunctionsRoot.DirectlyOrIndirectlyNestedFunctions).Where(f => f.ResolveResult != null).ToDictionary(f => f.ResolveResult);

			statementCompiler = new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, variables, nestedFunctionsDict, _runtimeLibrary, thisAlias, null);
		}

        public JsFunctionDefinitionExpression CompileMethod(EntityDeclaration entity, Statement body, IMethod method, MethodImplOptions impl) {
			CreateCompilationContext(entity, method, (impl.Type == MethodImplOptions.ImplType.StaticMethodWithThisAsFirstArgument ? _namingConvention.ThisAlias : null));
            return JsExpression.FunctionDefinition(method.Parameters.Select(p => variables[p].Name), statementCompiler.Compile(body), null);
        }

        public JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclaration ctor, IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorImplOptions impl) {
			CreateCompilationContext(ctor, constructor, (impl.Type == ConstructorImplOptions.ImplType.StaticMethod ? _namingConvention.ThisAlias : null));
			var body = new List<JsStatement>();

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

			if (ctor == null || ctor.Initializer.IsNull || ctor.Initializer.ConstructorInitializerType != ConstructorInitializerType.This) {
				if (impl.Type == ConstructorImplOptions.ImplType.StaticMethod) {
					// The compiler one step up has created the statements as "this.a = b;", but we need to replace that with "$this.a = b;" (or whatever name the this alias has).
					var replacer = new ThisReplacer(JsExpression.Identifier(_namingConvention.ThisAlias));
					instanceInitStatements = instanceInitStatements.Select(s => replacer.Visit(s, null)).ToList();
				}
	            body.AddRange(instanceInitStatements);	// Don't initialize fields when we are chaining, but do it when we 1) compile the default constructor, 2) don't have an initializer, or 3) when the initializer is not this(...).
			}

			if (impl.Type != ConstructorImplOptions.ImplType.StaticMethod) {
				if (ctor != null && !ctor.Initializer.IsNull) {
					body.AddRange(statementCompiler.CompileConstructorInitializer(ctor.Initializer, false));
				}
				else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
					body.AddRange(statementCompiler.CompileImplicitBaseConstructorCall(constructor.DeclaringType, false));
				}
			}

            if (ctor != null) {
			    body.AddRange(statementCompiler.Compile(ctor.Body).Statements);
			}

			return JsExpression.FunctionDefinition(constructor.Parameters.Select(p => variables[p].Name), new JsBlockStatement(body));
        }

        public JsFunctionDefinitionExpression CompileDefaultConstructor(IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorImplOptions impl) {
            return CompileConstructor(null, constructor, instanceInitStatements, impl);
        }

        public IList<JsStatement> CompileFieldInitializer(JsExpression field, Expression expression) {
            CreateCompilationContext(expression, null, null);
            return statementCompiler.CompileFieldInitializer(field, expression);
        }

        public IList<JsStatement> CompileDefaultFieldInitializer(JsExpression field, IType type) {
            CreateCompilationContext(null, null, null);
            return statementCompiler.CompileDefaultFieldInitializer(field, type);
        }
    }
}
