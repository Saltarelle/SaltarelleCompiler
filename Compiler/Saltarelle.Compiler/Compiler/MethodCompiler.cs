using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
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
		private StatementCompiler _statementCompiler;

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

			_statementCompiler = new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, variables, nestedFunctionsDict, _runtimeLibrary, thisAlias, usedNames, null, method);
		}

        public JsFunctionDefinitionExpression CompileMethod(EntityDeclaration entity, BlockStatement body, IMethod method, MethodScriptSemantics impl) {
			CreateCompilationContext(entity, method, (impl.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument ? _namingConvention.ThisAlias : null));
            return _statementCompiler.CompileMethod(method.Parameters, variables, body);
        }

        public JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclaration ctor, IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorScriptSemantics impl) {
			string       filename = ctor != null ? ctor.GetRegion().FileName : constructor.DeclaringTypeDefinition.Region.FileName;
			TextLocation location = ctor != null ? ctor.StartLocation : constructor.DeclaringTypeDefinition.Region.Begin;

			try {
				CreateCompilationContext(ctor, constructor, (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod ? _namingConvention.ThisAlias : null));
				var body = new List<JsStatement>();
				body.AddRange(FixByRefParameters(constructor.Parameters, variables));

				var systemObject = _compilation.FindType(KnownTypeCode.Object);
				if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (ctor != null && !ctor.Initializer.IsNull) {
						body.AddRange(_statementCompiler.CompileConstructorInitializer(ctor.Initializer, true));
					}
					else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
						body.AddRange(_statementCompiler.CompileImplicitBaseConstructorCall(filename, location, constructor.DeclaringType, true));
					}
					else {
						body.Add(new JsVariableDeclarationStatement(_namingConvention.ThisAlias, JsExpression.ObjectLiteral()));
					}
				}

				if (ctor == null || ctor.Initializer.IsNull || ctor.Initializer.ConstructorInitializerType != ConstructorInitializerType.This) {
					if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
						// The compiler one step up has created the statements as "this.a = b;", but we need to replace that with "$this.a = b;" (or whatever name the this alias has).
						var replacer = new ThisReplacer(JsExpression.Identifier(_namingConvention.ThisAlias));
						instanceInitStatements = instanceInitStatements.Select(s => replacer.Visit(s, null)).ToList();
					}
					body.AddRange(instanceInitStatements);	// Don't initialize fields when we are chaining, but do it when we 1) compile the default constructor, 2) don't have an initializer, or 3) when the initializer is not this(...).
				}

				if (impl.Type != ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (ctor != null && !ctor.Initializer.IsNull) {
						body.AddRange(_statementCompiler.CompileConstructorInitializer(ctor.Initializer, false));
					}
					else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
						body.AddRange(_statementCompiler.CompileImplicitBaseConstructorCall(filename, location, constructor.DeclaringType, false));
					}
				}

				if (ctor != null) {
					body.AddRange(_statementCompiler.Compile(ctor.Body).Statements);
				}

				return JsExpression.FunctionDefinition(constructor.Parameters.Select(p => variables[p].Name), new JsBlockStatement(body));
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, filename, location);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
        }

        public JsFunctionDefinitionExpression CompileDefaultConstructor(IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorScriptSemantics impl) {
            return CompileConstructor(null, constructor, instanceInitStatements, impl);
        }

        public IList<JsStatement> CompileFieldInitializer(string filename, TextLocation location, JsExpression field, Expression expression) {
			try {
	            CreateCompilationContext(expression, null, null);
		        return _statementCompiler.CompileFieldInitializer(filename, location, field, expression);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, filename, location);
				return new JsStatement[0];
			}
        }

        public IList<JsStatement> CompileDefaultFieldInitializer(string filename, TextLocation location, JsExpression field, IType type) {
			try {
	            CreateCompilationContext(null, null, null);
		        return _statementCompiler.CompileDefaultFieldInitializer(filename, location, field, type);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, filename, location);
				return new JsStatement[0];
			}
        }

		public JsFunctionDefinitionExpression CompileAutoPropertyGetter(IProperty property, PropertyScriptSemantics impl, string backingFieldName) {
			try {
				if (property.IsStatic) {
					CreateCompilationContext(null, null, null);
					var jsType = _runtimeLibrary.GetScriptType(property.DeclaringType, false);
					return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.MemberAccess(jsType, backingFieldName)));
				}
				else if (impl.GetMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					return JsExpression.FunctionDefinition(new[] { _namingConvention.ThisAlias }, new JsReturnStatement(JsExpression.MemberAccess(JsExpression.Identifier(_namingConvention.ThisAlias), backingFieldName)));
				}
				else {
					return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.MemberAccess(JsExpression.This, backingFieldName)));
				}
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, property.Getter.Region);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoPropertySetter(IProperty property, PropertyScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namingConvention.GetVariableName(property.Setter.Parameters[0], new HashSet<string>(property.DeclaringTypeDefinition.TypeParameters.Select(p => _namingConvention.GetTypeParameterName(p))));

				if (property.IsStatic) {
					CreateCompilationContext(null, null, null);
					var jsType = _runtimeLibrary.GetScriptType(property.DeclaringType, false);
					return JsExpression.FunctionDefinition(new[] { valueName }, new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(jsType, backingFieldName), JsExpression.Identifier(valueName))));
				}
				else if (impl.SetMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					return JsExpression.FunctionDefinition(new[] { _namingConvention.ThisAlias, valueName }, new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(JsExpression.Identifier(_namingConvention.ThisAlias), backingFieldName), JsExpression.Identifier(valueName))));
				}
				else {
					return JsExpression.FunctionDefinition(new[] { valueName }, new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(JsExpression.This, backingFieldName), JsExpression.Identifier(valueName))));
				}
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, property.Setter.Region);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoEventAdder(IEvent @event, EventScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namingConvention.GetVariableName(@event.AddAccessor.Parameters[0], new HashSet<string>(@event.DeclaringTypeDefinition.TypeParameters.Select(p => _namingConvention.GetTypeParameterName(p))));
				CreateCompilationContext(null, null, null);

				JsExpression target;
				string[] args;
				if (@event.IsStatic) {
					target = _runtimeLibrary.GetScriptType(@event.DeclaringType, false);
					args = new[] { valueName };
				}
				else if (impl.AddMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					target = JsExpression.Identifier(_namingConvention.ThisAlias);
					args = new[] { _namingConvention.ThisAlias, valueName };
				}
				else {
					target = JsExpression.This;
					args = new[] { valueName };
				}

				var bfAccessor = JsExpression.MemberAccess(target, backingFieldName);
				var combineCall = _statementCompiler.CompileDelegateCombineCall(@event.AddAccessor.Region.FileName, @event.AddAccessor.Region.Begin, bfAccessor, JsExpression.Identifier(valueName));
				return JsExpression.FunctionDefinition(args, new JsBlockStatement(new JsExpressionStatement(JsExpression.Assign(bfAccessor, combineCall))));
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, @event.Region);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoEventRemover(IEvent @event, EventScriptSemantics impl, string backingFieldName) {
			try {
				CreateCompilationContext(null, null, null);
				string valueName = _namingConvention.GetVariableName(@event.RemoveAccessor.Parameters[0], new HashSet<string>(@event.DeclaringTypeDefinition.TypeParameters.Select(p => _namingConvention.GetTypeParameterName(p))));

				CreateCompilationContext(null, null, null);

				JsExpression target;
				string[] args;
				if (@event.IsStatic) {
					target = _runtimeLibrary.GetScriptType(@event.DeclaringType, false);
					args = new[] { valueName };
				}
				else if (impl.RemoveMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					target = JsExpression.Identifier(_namingConvention.ThisAlias);
					args = new[] { _namingConvention.ThisAlias, valueName };
				}
				else {
					target = JsExpression.This;
					args = new[] { valueName };
				}

				var bfAccessor = JsExpression.MemberAccess(target, backingFieldName);
				var combineCall = _statementCompiler.CompileDelegateRemoveCall(@event.RemoveAccessor.Region.FileName, @event.RemoveAccessor.Region.Begin, bfAccessor, JsExpression.Identifier(valueName));
				return JsExpression.FunctionDefinition(args, new JsBlockStatement(new JsExpressionStatement(JsExpression.Assign(bfAccessor, combineCall))));
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex, @event.Region);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public static List<JsStatement> FixByRefParameters(IEnumerable<IParameter> parameters, IDictionary<IVariable, VariableData> variables) {
			List<JsStatement> result = null;
			foreach (var p in parameters) {
				if (!p.IsOut && !p.IsRef && variables[p].UseByRefSemantics) {
					result = result ?? new List<JsStatement>();
					result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(variables[p].Name), JsExpression.ObjectLiteral(new JsObjectLiteralProperty("$", JsExpression.Identifier(variables[p].Name))))));
				}
			}
			return result ?? new List<JsStatement>();
		}
    }
}
