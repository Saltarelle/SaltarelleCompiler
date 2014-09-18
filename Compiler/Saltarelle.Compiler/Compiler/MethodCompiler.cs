using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
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

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				// Inside a function, "this" is in another context and should thus not be replaced.
				return statement;
			}
		}

		private class StaticMethodConstructorReturnPatcher : RewriterVisitorBase<object> {
			private JsExpression _identifier;

			private StaticMethodConstructorReturnPatcher(string identifier) {
				this._identifier = JsExpression.Identifier(identifier);
			}

			public override JsStatement VisitReturnStatement(JsReturnStatement statement, object data) {
				return JsStatement.Return(_identifier);
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

		private class IsIteratorBlockVisitor : CSharpSyntaxWalker {
			private bool _result;

			public override void VisitYieldStatement(YieldStatementSyntax yieldStatement) {
				_result = true;
			}

			private IsIteratorBlockVisitor() {
			}

			public static bool Analyze(SyntaxNode node) {
				var obj = new IsIteratorBlockVisitor();
				obj.Visit(node);
				return obj._result;
			}
		}

		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;
		private readonly CSharpCompilation _compilation;
		private readonly SemanticModel _semanticModel;
		private readonly IRuntimeLibrary _runtimeLibrary;

		internal IDictionary<ISymbol, VariableData> variables;
		private StatementCompiler _statementCompiler;
		private ISet<string> _usedNames;

		public MethodCompiler(IMetadataImporter metadataImporter, INamer namer, IErrorReporter errorReporter, CSharpCompilation compilation, SemanticModel semanticModel, IRuntimeLibrary runtimeLibrary) {
			_metadataImporter = metadataImporter;
			_namer            = namer;
			_errorReporter    = errorReporter;
			_compilation      = compilation;
			_semanticModel    = semanticModel;
			_runtimeLibrary   = runtimeLibrary;
		}

		private void CreateCompilationContext(SyntaxNode entity, IMethodSymbol method, INamedTypeSymbol type, string thisAlias) {
			_usedNames = method != null ? new HashSet<string>(type.GetAllTypeParameters().Concat(method.TypeParameters).Select(p => _namer.GetTypeParameterName(p))) : new HashSet<string>();
			if (entity != null) {
				var x = new VariableGatherer(_semanticModel, _namer, _errorReporter).GatherVariables(entity, method, _usedNames);
				variables  = x.Item1;
				_usedNames = x.Item2;
			}

			_statementCompiler = new StatementCompiler(_metadataImporter, _namer, _errorReporter, _semanticModel, variables, _runtimeLibrary, thisAlias, _usedNames, null);
		}

		public JsFunctionDefinitionExpression CompileMethod(SyntaxNode entity, BlockSyntax body, IMethodSymbol method, MethodScriptSemantics impl) {
			bool isIEnumerable = method.ReturnType.SpecialType == SpecialType.System_Collections_IEnumerable || method.ReturnType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T;
			bool isIEnumerator = method.ReturnType.SpecialType == SpecialType.System_Collections_IEnumerator || method.ReturnType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerator_T;
			StateMachineType smt = StateMachineType.NormalMethod;
			ITypeSymbol iteratorBlockYieldTypeOrAsyncTaskGenericArgument = null;
			if ((isIEnumerable || isIEnumerator) && IsIteratorBlockVisitor.Analyze(body)) {
				smt = isIEnumerable ? StateMachineType.IteratorBlockReturningIEnumerable : StateMachineType.IteratorBlockReturningIEnumerator;
				iteratorBlockYieldTypeOrAsyncTaskGenericArgument = method.ReturnType is INamedTypeSymbol && ((INamedTypeSymbol)method.ReturnType).TypeArguments.Length > 0 ? ((INamedTypeSymbol)method.ReturnType).TypeArguments[0] : _compilation.GetSpecialType(SpecialType.System_Object);
			}
			else if (method.IsAsync) {
				smt = (method.ReturnType.SpecialType == SpecialType.System_Void ? StateMachineType.AsyncVoid : StateMachineType.AsyncTask);
				iteratorBlockYieldTypeOrAsyncTaskGenericArgument = method.ReturnType is INamedTypeSymbol && ((INamedTypeSymbol)method.ReturnType).TypeArguments.Length > 0 ? ((INamedTypeSymbol)method.ReturnType).TypeArguments[0] : null;
			}

			CreateCompilationContext(entity, method, method.ContainingType, (impl.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument ? _namer.ThisAlias : null));
			return _statementCompiler.CompileMethod(method.Parameters, variables, body, impl.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument, impl.ExpandParams, smt, iteratorBlockYieldTypeOrAsyncTaskGenericArgument);
		}

		public JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclarationSyntax ctor, IMethodSymbol constructor, List<JsStatement> instanceInitStatements, ConstructorScriptSemantics impl) {
			var location = _errorReporter.Location = ctor != null ? ctor.GetLocation() : constructor.ContainingType.Locations[0];
			try {
				CreateCompilationContext(ctor, constructor, constructor.ContainingType, (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod ? _namer.ThisAlias : null));
				IList<JsStatement> body = new List<JsStatement>();
				body.AddRange(PrepareParameters(constructor.Parameters, variables, expandParams: impl.ExpandParams, staticMethodWithThisAsFirstArgument: false));

				if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (ctor != null && ctor.Initializer != null) {
						body.AddRange(_statementCompiler.CompileConstructorInitializer(ctor.Initializer, true));
					}
					else {
						body.AddRange(_statementCompiler.CompileImplicitBaseConstructorCall(constructor.ContainingType, true));
					}
				}

				if (ctor == null || ctor.Initializer == null || ctor.Initializer.ThisOrBaseKeyword.CSharpKind() != SyntaxKind.ThisKeyword) {
					if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
						// The compiler one step up has created the statements as "this.a = b;", but we need to replace that with "$this.a = b;" (or whatever name the this alias has).
						var replacer = new ThisReplacer(JsExpression.Identifier(_namer.ThisAlias));
						instanceInitStatements = instanceInitStatements.Select(s => replacer.VisitStatement(s, null)).ToList();
					}
					body.AddRange(instanceInitStatements);	// Don't initialize fields when we are chaining, but do it when we 1) compile the default constructor, 2) don't have an initializer, or 3) when the initializer is not this(...).
				}

				if (impl.Type != ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (ctor != null && ctor.Initializer != null) {
						body.AddRange(_statementCompiler.CompileConstructorInitializer(ctor.Initializer, false));
					}
					else {
						body.AddRange(_statementCompiler.CompileImplicitBaseConstructorCall(constructor.ContainingType, false));
					}
				}

				if (ctor != null) {
					body.AddRange(_statementCompiler.Compile(ctor.Body).Statements);
				}

				if (impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod) {
					if (body.Count == 0 || !(body[body.Count - 1] is JsReturnStatement))
						body.Add(JsStatement.Return());
					body = StaticMethodConstructorReturnPatcher.Process(body, _namer.ThisAlias).AsReadOnly();
				}

				var compiled = JsExpression.FunctionDefinition(constructor.Parameters.Where((p, i) => i != constructor.Parameters.Length - 1 || !impl.ExpandParams).Select(p => variables[p].Name), JsStatement.Block(body));
				return _statementCompiler.StateMachineRewriteNormalMethod(compiled);
			}
			catch (Exception ex) {
				_errorReporter.Location = location;
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock);
			}
		}

		public JsFunctionDefinitionExpression CompileDefaultConstructor(IMethodSymbol constructor, List<JsStatement> instanceInitStatements, ConstructorScriptSemantics impl) {
			return CompileConstructor(null, constructor, instanceInitStatements, impl);
		}

		public IList<JsStatement> CompileFieldInitializer(Location location, JsExpression jsThis, string scriptName, ISymbol member, ExpressionSyntax value) {
			_errorReporter.Location = location;
			try {
				CreateCompilationContext(value, null, member.ContainingType, null);
				return _statementCompiler.CompileFieldInitializer(location, jsThis, scriptName, member, value);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public IList<JsStatement> CompileDefaultFieldInitializer(Location location, JsExpression jsThis, string scriptName, ISymbol member, ITypeSymbol memberType) {
			_errorReporter.Location = location;
			try {
				CreateCompilationContext(null, null, member.ContainingType, null);
				return _statementCompiler.CompileDefaultFieldInitializer(location, jsThis, scriptName, member, memberType);
			}
			catch (Exception ex) {
				_errorReporter.InternalError(ex);
				return new JsStatement[0];
			}
		}

		public JsFunctionDefinitionExpression CompileAutoPropertyGetter(IPropertySymbol property, PropertyScriptSemantics impl, string backingFieldName) {
			try {
				CreateCompilationContext(null, null, property.ContainingType, null);
				if (property.IsStatic) {
					var jsType = _runtimeLibrary.InstantiateType(property.ContainingType, _statementCompiler);
					return JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Member(jsType, backingFieldName)));
				}
				else if (impl.GetMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					return JsExpression.FunctionDefinition(new[] { _namer.ThisAlias }, JsStatement.Return(JsExpression.Member(JsExpression.Identifier(_namer.ThisAlias), backingFieldName)));
				}
				else {
					return JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Member(JsExpression.This, backingFieldName)));
				}
			}
			catch (Exception ex) {
				_errorReporter.Location = property.GetMethod.Locations[0];
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoPropertySetter(IPropertySymbol property, PropertyScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namer.GetVariableName(property.SetMethod.Parameters[0].Name, new HashSet<string>(property.ContainingType.GetAllTypeParameters().Select(p => _namer.GetTypeParameterName(p))));
				CreateCompilationContext(null, null, property.ContainingType, null);

				if (property.IsStatic) {
					var jsType = _runtimeLibrary.InstantiateType(property.ContainingType, _statementCompiler);
					return JsExpression.FunctionDefinition(new[] { valueName }, JsExpression.Assign(JsExpression.Member(jsType, backingFieldName), JsExpression.Identifier(valueName)));
				}
				else if (impl.SetMethod.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
					return JsExpression.FunctionDefinition(new[] { _namer.ThisAlias, valueName }, JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(_namer.ThisAlias), backingFieldName), JsExpression.Identifier(valueName)));
				}
				else {
					return JsExpression.FunctionDefinition(new[] { valueName }, JsExpression.Assign(JsExpression.Member(JsExpression.This, backingFieldName), JsExpression.Identifier(valueName)));
				}
			}
			catch (Exception ex) {
				_errorReporter.Location = property.SetMethod.Locations[0];
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoEventAdder(IEventSymbol @event, EventScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namer.GetVariableName(@event.AddMethod.Parameters[0].Name, new HashSet<string>(@event.ContainingType.GetAllTypeParameters().Select(p => _namer.GetTypeParameterName(p))));
				CreateCompilationContext(null, null, @event.ContainingType, null);

				JsExpression target;
				string[] args;
				if (@event.IsStatic) {
					target = _runtimeLibrary.InstantiateType(@event.ContainingType, _statementCompiler);
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
				var combineCall = _statementCompiler.CompileDelegateCombineCall(@event.Locations[0], bfAccessor, JsExpression.Identifier(valueName));
				return JsExpression.FunctionDefinition(args, JsStatement.Block(JsExpression.Assign(bfAccessor, combineCall)));
			}
			catch (Exception ex) {
				_errorReporter.Location = @event.Locations[0];
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock);
			}
		}

		public JsFunctionDefinitionExpression CompileAutoEventRemover(IEventSymbol @event, EventScriptSemantics impl, string backingFieldName) {
			try {
				string valueName = _namer.GetVariableName(@event.RemoveMethod.Parameters[0].Name, new HashSet<string>(@event.ContainingType.GetAllTypeParameters().Select(p => _namer.GetTypeParameterName(p))));
				CreateCompilationContext(null, null, @event.ContainingType, null);

				JsExpression target;
				string[] args;
				if (@event.IsStatic) {
					target = _runtimeLibrary.InstantiateType(@event.ContainingType, _statementCompiler);
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
				var combineCall = _statementCompiler.CompileDelegateRemoveCall(@event.Locations[0], bfAccessor, JsExpression.Identifier(valueName));
				return JsExpression.FunctionDefinition(args, JsStatement.Block(JsExpression.Assign(bfAccessor, combineCall)));
			}
			catch (Exception ex) {
				_errorReporter.Location = @event.Locations[0];
				_errorReporter.InternalError(ex);
				return JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock);
			}
		}

		public static List<JsStatement> PrepareParameters(IReadOnlyList<IParameterSymbol> parameters, IDictionary<ISymbol, VariableData> variables, bool expandParams, bool staticMethodWithThisAsFirstArgument) {
			List<JsStatement> result = null;
			if (expandParams && parameters.Count > 0) {
				result = result ?? new List<JsStatement>();
				result.Add(JsStatement.Var(variables[parameters[parameters.Count - 1]].Name, JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"), JsExpression.Number(parameters.Count - 1 + (staticMethodWithThisAsFirstArgument ? 1 : 0)))));
			}
			foreach (var p in parameters) {
				if (p.RefKind == RefKind.None && variables[p].UseByRefSemantics) {
					result = result ?? new List<JsStatement>();
					result.Add(JsExpression.Assign(JsExpression.Identifier(variables[p].Name), JsExpression.ObjectLiteral(new JsObjectLiteralProperty("$", JsExpression.Identifier(variables[p].Name)))));
				}
			}
			return result ?? new List<JsStatement>();
		}
	}
}
