using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
    public class MethodCompiler {
		private class ThisReplacer : RewriterVisitorBase<object> {
			private readonly JsExpression _replaceWith;

			public ThisReplacer(JsExpression replaceWith) {
				_replaceWith = replaceWith;
			}

			public override JsExpression VisitThisExpression(JsThisExpression expression, object data) {
				return _replaceWith;
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				// Inside a function, "this" is in another context and should thus not be replaced.
				return expression;
			}
		}

		private class StaticMethodConstructorReturnPatcher : RewriterVisitorBase<object> {
			private JsExpression _identifier;

			private StaticMethodConstructorReturnPatcher(string identifier) {
				this._identifier = JsExpression.Identifier(identifier);
			}

			public override JsStatement VisitReturnStatement(JsReturnStatement statement, object data) {
				return new JsReturnStatement(_identifier);
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				return expression;	// Don't patch return values of nested functions.
			}

			public override JsStatement  VisitFunctionStatement(JsFunctionStatement statement, object data) {
				return statement;	// Don't patch return values of nested functions.
			}

			public static IList<JsStatement> Process(IList<JsStatement> statements, string identifierToReturn) {
				var obj = new StaticMethodConstructorReturnPatcher(identifierToReturn);
				return obj.VisitStatements(statements, null);
			}
		}

		private class IsIteratorBlockVisitor : DepthFirstAstVisitor {
			private bool _result;

			public override void VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement) {
				_result = true;
			}

			public override void VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement) {
				_result = true;
			}

			private IsIteratorBlockVisitor() {
			}

			public static bool Analyze(AstNode node) {
				var obj = new IsIteratorBlockVisitor();
				node.AcceptVisitor(obj);
				return obj._result;
			}
		}

        private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
        private readonly IErrorReporter _errorReporter;
        private readonly ICompilation _compilation;
        private readonly CSharpAstResolver _resolver;
    	private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly ISet<string> _definedSymbols;

    	internal IDictionary<IVariable, VariableData> variables;
        internal NestedFunctionData nestedFunctionsRoot;
		private StatementCompiler _statementCompiler;
		private ISet<string> _usedNames;
		private string _thisAlias;

        public MethodCompiler(IMetadataImporter metadataImporter, INamer namer, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IRuntimeLibrary runtimeLibrary, ISet<string> definedSymbols) {
            _metadataImporter = metadataImporter;
			_namer            = namer;
            _errorReporter    = errorReporter;
            _compilation      = compilation;
            _resolver         = resolver;
        	_runtimeLibrary   = runtimeLibrary;
			_definedSymbols   = definedSymbols;
        }

		private void CreateCompilationContext(AstNode entity, IMethod method, string thisAlias) {
			_thisAlias = thisAlias;
            _usedNames = method != null ? new HashSet<string>(method.DeclaringTypeDefinition.TypeParameters.Concat(method.TypeParameters).Select(p => _namer.GetTypeParameterName(p))) : new HashSet<string>();
			if (entity != null) {
				var x = new VariableGatherer(_resolver, _namer, _errorReporter).GatherVariables(entity, method, _usedNames);
				variables  = x.Item1;
				_usedNames = x.Item2;
			}
            nestedFunctionsRoot     = entity != null ? new NestedFunctionGatherer(_resolver).GatherNestedFunctions(entity, variables) : new NestedFunctionData(null);
			var nestedFunctionsDict = new[] { nestedFunctionsRoot }.Concat(nestedFunctionsRoot.DirectlyOrIndirectlyNestedFunctions).Where(f => f.ResolveResult != null).ToDictionary(f => f.ResolveResult);

			_statementCompiler = new StatementCompiler(_metadataImporter, _namer, _errorReporter, _compilation, _resolver, variables, nestedFunctionsDict, _runtimeLibrary, thisAlias, _usedNames, null, method, _definedSymbols);
		}

		public JsFunctionDefinitionExpression CompileMethod(EntityDeclaration entity, BlockStatement body, IMethod method, MethodScriptSemantics impl) {
			bool isIEnumerable = method.ReturnType.IsKnownType(KnownTypeCode.IEnumerable) || method.ReturnType.IsKnownType(KnownTypeCode.IEnumerableOfT);
			bool isIEnumerator = method.ReturnType.IsKnownType(KnownTypeCode.IEnumerator) || method.ReturnType.IsKnownType(KnownTypeCode.IEnumeratorOfT);
			StateMachineType smt = StateMachineType.NormalMethod;
			IType iteratorBlockYieldTypeOrAsyncTaskGenericArgument = null;
			if ((isIEnumerable || isIEnumerator) && IsIteratorBlockVisitor.Analyze(body)) {
				smt = isIEnumerable ? StateMachineType.IteratorBlockReturningIEnumerable : StateMachineType.IteratorBlockReturningIEnumerator;
				iteratorBlockYieldTypeOrAsyncTaskGenericArgument = method.ReturnType is ParameterizedType ? ((ParameterizedType)method.ReturnType).TypeArguments[0] : _compilation.FindType(KnownTypeCode.Object);
			}
			else if (entity.HasModifier(Modifiers.Async)) {
				smt = (method.ReturnType.IsKnownType(KnownTypeCode.Void) ? StateMachineType.AsyncVoid : StateMachineType.AsyncTask);
				iteratorBlockYieldTypeOrAsyncTaskGenericArgument = method.ReturnType is ParameterizedType ? ((ParameterizedType)method.ReturnType).TypeArguments[0] : null;
			}

			CreateCompilationContext(entity, method, (impl.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument ? _namer.ThisAlias : null));
			return _statementCompiler.CompileMethod(method.Parameters, variables, body, impl.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument, smt, iteratorBlockYieldTypeOrAsyncTaskGenericArgument);
		}

        public JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclaration ctor, IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorScriptSemantics impl) {
			var region = _errorReporter.Region = ctor != null ? ctor.GetRegion() : constructor.DeclaringTypeDefinition.Region;
			try {
				CreateCompilationContext(ctor, constructor, (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod ? _namer.ThisAlias : null));
				IList<JsStatement> body = new List<JsStatement>();
				body.AddRange(FixByRefParameters(constructor.Parameters, variables));

				var systemObject = _compilation.FindType(KnownTypeCode.Object);
				if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (ctor != null && !ctor.Initializer.IsNull) {
						body.AddRange(_statementCompiler.CompileConstructorInitializer(ctor.Initializer, true));
					}
					else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
						body.AddRange(_statementCompiler.CompileImplicitBaseConstructorCall(constructor.DeclaringTypeDefinition, true));
					}
					else {
						body.Add(new JsVariableDeclarationStatement(_namer.ThisAlias, JsExpression.ObjectLiteral()));
					}
				}

				if (ctor == null || ctor.Initializer.IsNull || ctor.Initializer.ConstructorInitializerType != ConstructorInitializerType.This) {
					if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
						// The compiler one step up has created the statements as "this.a = b;", but we need to replace that with "$this.a = b;" (or whatever name the this alias has).
						var replacer = new ThisReplacer(JsExpression.Identifier(_namer.ThisAlias));
						instanceInitStatements = instanceInitStatements.Select(s => replacer.VisitStatement(s, null)).ToList();
					}
					body.AddRange(instanceInitStatements);	// Don't initialize fields when we are chaining, but do it when we 1) compile the default constructor, 2) don't have an initializer, or 3) when the initializer is not this(...).
				}

				if (impl.Type != ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (ctor != null && !ctor.Initializer.IsNull) {
						body.AddRange(_statementCompiler.CompileConstructorInitializer(ctor.Initializer, false));
					}
					else if (!constructor.DeclaringType.DirectBaseTypes.Any(t => t.Equals(systemObject))) {
						body.AddRange(_statementCompiler.CompileImplicitBaseConstructorCall(constructor.DeclaringTypeDefinition, false));
					}
				}

				if (ctor != null) {
					body.AddRange(_statementCompiler.Compile(ctor.Body).Statements);
				}

				if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (body.Count == 0 || !(body[body.Count - 1] is JsReturnStatement))
						body.Add(new JsReturnStatement());
					body = StaticMethodConstructorReturnPatcher.Process(body, _namer.ThisAlias).AsReadOnly();
				}

				var compiled = JsExpression.FunctionDefinition(constructor.Parameters.Select(p => variables[p].Name), new JsBlockStatement(body));
				return _statementCompiler.StateMachineRewriteNormalMethod(compiled);
			}
			catch (Exception ex) {
				_errorReporter.Region = region;
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
        }

        public JsFunctionDefinitionExpression CompileDefaultConstructor(IMethod constructor, List<JsStatement> instanceInitStatements, ConstructorScriptSemantics impl) {
            return CompileConstructor(null, constructor, instanceInitStatements, impl);
        }

        public IList<JsStatement> CompileFieldInitializer(DomRegion region, JsExpression field, Expression expression) {
			_errorReporter.Region = region;
			try {
	            CreateCompilationContext(expression, null, null);
		        return _statementCompiler.CompileFieldInitializer(region, field, expression);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
        }

        public IList<JsStatement> CompileDefaultFieldInitializer(DomRegion region, JsExpression field, IType type) {
			_errorReporter.Region = region;
			try {
	            CreateCompilationContext(null, null, null);
		        return _statementCompiler.CompileDefaultFieldInitializer(region, field, type);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
        }

		public JsFunctionDefinitionExpression CompileAutoPropertyGetter(IProperty property, PropertyScriptSemantics impl, string backingFieldName) {
			try {
				if (property.IsStatic) {
					CreateCompilationContext(null, null, null);
					var jsType = _runtimeLibrary.GetScriptType(property.DeclaringType, TypeContext.UseStaticMember);
					return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Member(jsType, backingFieldName)));
				}
				else if (impl.GetMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					return JsExpression.FunctionDefinition(new[] { _namer.ThisAlias }, new JsReturnStatement(JsExpression.Member(JsExpression.Identifier(_namer.ThisAlias), backingFieldName)));
				}
				else {
					return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Member(JsExpression.This, backingFieldName)));
				}
			}
			catch (Exception ex) {
				_errorReporter.Region = property.Getter.Region;
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoPropertySetter(IProperty property, PropertyScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namer.GetVariableName(property.Setter.Parameters[0].Name, new HashSet<string>(property.DeclaringTypeDefinition.TypeParameters.Select(p => _namer.GetTypeParameterName(p))));

				if (property.IsStatic) {
					CreateCompilationContext(null, null, null);
					var jsType = _runtimeLibrary.GetScriptType(property.DeclaringType, TypeContext.UseStaticMember);
					return JsExpression.FunctionDefinition(new[] { valueName }, new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(jsType, backingFieldName), JsExpression.Identifier(valueName))));
				}
				else if (impl.SetMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					return JsExpression.FunctionDefinition(new[] { _namer.ThisAlias, valueName }, new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(_namer.ThisAlias), backingFieldName), JsExpression.Identifier(valueName))));
				}
				else {
					return JsExpression.FunctionDefinition(new[] { valueName }, new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(JsExpression.This, backingFieldName), JsExpression.Identifier(valueName))));
				}
			}
			catch (Exception ex) {
				_errorReporter.Region = property.Setter.Region;
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoEventAdder(IEvent @event, EventScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namer.GetVariableName(@event.AddAccessor.Parameters[0].Name, new HashSet<string>(@event.DeclaringTypeDefinition.TypeParameters.Select(p => _namer.GetTypeParameterName(p))));
				CreateCompilationContext(null, null, null);

				JsExpression target;
				string[] args;
				if (@event.IsStatic) {
					target = _runtimeLibrary.GetScriptType(@event.DeclaringType, TypeContext.UseStaticMember);
					args = new[] { valueName };
				}
				else if (impl.AddMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					target = JsExpression.Identifier(_namer.ThisAlias);
					args = new[] { _namer.ThisAlias, valueName };
				}
				else {
					target = JsExpression.This;
					args = new[] { valueName };
				}

				var bfAccessor = JsExpression.Member(target, backingFieldName);
				var combineCall = _statementCompiler.CompileDelegateCombineCall(@event.AddAccessor.Region, bfAccessor, JsExpression.Identifier(valueName));
				return JsExpression.FunctionDefinition(args, new JsBlockStatement(new JsExpressionStatement(JsExpression.Assign(bfAccessor, combineCall))));
			}
			catch (Exception ex) {
				_errorReporter.Region = @event.Region;
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoEventRemover(IEvent @event, EventScriptSemantics impl, string backingFieldName) {
			try {
				CreateCompilationContext(null, null, null);
				string valueName = _namer.GetVariableName(@event.RemoveAccessor.Parameters[0].Name, new HashSet<string>(@event.DeclaringTypeDefinition.TypeParameters.Select(p => _namer.GetTypeParameterName(p))));

				CreateCompilationContext(null, null, null);

				JsExpression target;
				string[] args;
				if (@event.IsStatic) {
					target = _runtimeLibrary.GetScriptType(@event.DeclaringType, TypeContext.UseStaticMember);
					args = new[] { valueName };
				}
				else if (impl.RemoveMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					target = JsExpression.Identifier(_namer.ThisAlias);
					args = new[] { _namer.ThisAlias, valueName };
				}
				else {
					target = JsExpression.This;
					args = new[] { valueName };
				}

				var bfAccessor = JsExpression.Member(target, backingFieldName);
				var combineCall = _statementCompiler.CompileDelegateRemoveCall(@event.RemoveAccessor.Region, bfAccessor, JsExpression.Identifier(valueName));
				return JsExpression.FunctionDefinition(args, new JsBlockStatement(new JsExpressionStatement(JsExpression.Assign(bfAccessor, combineCall))));
			}
			catch (Exception ex) {
				_errorReporter.Region = @event.Region;
				_errorReporter.InternalError(ex);
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
