using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class ExpressionTreeBuilder : CSharpSyntaxVisitor {
		private readonly CSharpCompilation _compilation;
		private readonly SemanticModel _semanticModel;
		private readonly IMetadataImporter _metadataImporter;
		private readonly List<JsStatement> _additionalStatements;
		private readonly INamedTypeSymbol _expression;
		private readonly Func<ITypeSymbol, string> _createTemporaryVariable;
		private readonly Func<IMethodSymbol, JsExpression, JsExpression[], ExpressionCompileResult> _compileMethodCall;
		private readonly Func<ITypeSymbol, JsExpression> _instantiateType;
		private readonly Func<ITypeSymbol, JsExpression> _getDefaultValue;
		private readonly Func<ISymbol, JsExpression> _getMember;
		private readonly Func<ILocalSymbol, JsExpression> _createLocalReferenceExpression;
		private readonly JsExpression _this;
		private readonly Dictionary<ILocalSymbol, string> _allParameters;

		public ExpressionTreeBuilder(CSharpCompilation compilation, SemanticModel semanticModel, IMetadataImporter metadataImporter, Func<ITypeSymbol, string> createTemporaryVariable, Func<IMethodSymbol, JsExpression, JsExpression[], ExpressionCompileResult> compileMethodCall, Func<ITypeSymbol, JsExpression> instantiateType, Func<ITypeSymbol, JsExpression> getDefaultValue, Func<ISymbol, JsExpression> getMember, Func<ILocalSymbol, JsExpression> createLocalReferenceExpression, JsExpression @this) {
			_compilation = compilation;
			_semanticModel = semanticModel;
			_metadataImporter = metadataImporter;
			_expression = (INamedTypeSymbol)compilation.GetTypeByMetadataName(typeof(System.Linq.Expressions.Expression).FullName);
			_createTemporaryVariable = createTemporaryVariable;
			_compileMethodCall = compileMethodCall;
			_instantiateType = instantiateType;
			_getDefaultValue = getDefaultValue;
			_getMember = getMember;
			_createLocalReferenceExpression = createLocalReferenceExpression;
			_this = @this;
			_allParameters = new Dictionary<ILocalSymbol, string>();
			_additionalStatements = new List<JsStatement>();
		}

		public ExpressionCompileResult BuildExpressionTree(SimpleLambdaExpressionSyntax lambda) {
			//var expr = VisitLambdaResolveResult(lambda, null);
			//return new ExpressionCompileResult(expr, _additionalStatements);
			return null;
		}

		public ExpressionCompileResult BuildExpressionTree(ParenthesizedLambdaExpressionSyntax lambda) {
			//var expr = VisitLambdaResolveResult(lambda, null);
			//return new ExpressionCompileResult(expr, _additionalStatements);
			return null;
		}

#if false

		private bool TypesMatch(IMethodSymbol method, Type[] argumentTypes) {
			if (method.Parameters.Length != argumentTypes.Length)
				return false;
			for (int i = 0; i < argumentTypes.Length; i++) {
				if (!method.Parameters[i].Type.Equals(_compilation.GetTypeByMetadataName(argumentTypes[i].FullName)))
					return false;
			}
			return true;
		}

		private JsExpression CompileFactoryCall(string factoryMethodName, Type[] argumentTypes, JsExpression[] arguments) {
			var method = _expression.Methods.Single(m => m.Name == factoryMethodName && m.TypeParameters.Count == 0 && TypesMatch(m, argumentTypes));
			var result = _compileMethodCall(method, JsExpression.Null, arguments);
			_additionalStatements.AddRange(result.AdditionalStatements);
			return result.Expression;
		}

		public override JsExpression VisitResolveResult(ResolveResult rr, object data) {
			if (rr.IsError)
				throw new InvalidOperationException("ResolveResult" + rr + " is an error.");
			return base.VisitResolveResult(rr, data);
		}

		public override JsExpression VisitLambdaResolveResult(LambdaResolveResult rr, object data) {
			var parameters = new JsExpression[rr.Parameters.Count];
			for (int i = 0; i < rr.Parameters.Count; i++) {
				var temp = _createTemporaryVariable(rr.Parameters[i].Type);
				_allParameters[rr.Parameters[i]] = temp;
				_additionalStatements.Add(JsStatement.Var(temp, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(rr.Parameters[i].Type), JsExpression.String(rr.Parameters[i].Name) })));
				parameters[i] = JsExpression.Identifier(temp);
			}

			var body = VisitResolveResult(rr.Body, null);
			return CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { body, JsExpression.ArrayLiteral(parameters) });
		}

		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, object data) {
			string name;
			if (_allParameters.TryGetValue(rr.Variable, out name))
				return JsExpression.Identifier(name);
			else
				return _createLocalReferenceExpression(rr.Variable);
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, object data) {
			bool isUserDefined = (rr.UserDefinedOperatorMethod != null && _metadataImporter.GetMethodSemantics(rr.UserDefinedOperatorMethod).Type != MethodScriptSemantics.ImplType.NativeOperator);
			var arguments = new JsExpression[rr.Operands.Count + 1];
			for (int i = 0; i < rr.Operands.Count; i++)
				arguments[i] = VisitResolveResult(rr.Operands[i], null);
			arguments[arguments.Length - 1] = isUserDefined ? _getMember(rr.UserDefinedOperatorMethod) : _instantiateType(rr.Type);
			if (rr.OperatorType == ExpressionType.Conditional)
				return CompileFactoryCall("Condition", new[] { typeof(Expression), typeof(Expression), typeof(Expression), typeof(Type) }, arguments);
			else {
				return CompileFactoryCall(rr.OperatorType.ToString(), rr.Operands.Count == 1 ? new[] { typeof(Expression), isUserDefined ? typeof(MethodInfo) : typeof(Type) } : new[] { typeof(Expression), typeof(Expression), isUserDefined ? typeof(MethodInfo) : typeof(Type) }, arguments);
			}
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, object data) {
			var input = VisitResolveResult(rr.Input, null);
			if (rr.Conversion.IsIdentityConversion) {
				return input;
			}
			else if (rr.Conversion.IsAnonymousFunctionConversion) {
				var result = input;
				if (rr.Type.Name == "Expression")
					result = CompileFactoryCall("Quote", new[] { typeof(Expression) }, new[] { result });
				return result;
			}
			else if (rr.Conversion.IsNullLiteralConversion) {
				return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { input, _instantiateType(rr.Type) });
			}
			else if (rr.Conversion.IsMethodGroupConversion) {
				var methodInfo = _compilation.FindType(typeof(MethodInfo));
				return CompileFactoryCall("Convert", new[] { typeof(Expression), typeof(Type) }, new[] {
				           CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { 
				               CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _getMember(rr.Conversion.Method), _instantiateType(methodInfo) }),
				               _getMember(methodInfo.GetMethods().Single(m => m.Name == "CreateDelegate" && m.Parameters.Count == 2 && m.Parameters[0].Type.FullName == typeof(Type).FullName && m.Parameters[1].Type.FullName == typeof(object).FullName)),
				               JsExpression.ArrayLiteral(
				                   _instantiateType(rr.Type),
				                   rr.Conversion.Method.IsStatic ? JsExpression.Null : VisitResolveResult(((MethodGroupResolveResult)rr.Input).TargetResult, null)
				               )
				           }),
				           _instantiateType(rr.Type)
				       });
			}
			else {
				string methodName;
				if (rr.Conversion.IsTryCast)
					methodName = "TypeAs";
				else if (rr.CheckForOverflow)
					methodName = "ConvertChecked";
				else
					methodName = "Convert";
				if (rr.Conversion.IsUserDefined)
					return CompileFactoryCall(methodName, new[] { typeof(Expression), typeof(Type), typeof(MethodInfo) }, new[] { input, _instantiateType(rr.Type), _getMember(rr.Conversion.Method) });
				else
					return CompileFactoryCall(methodName, new[] { typeof(Expression), typeof(Type) }, new[] { input, _instantiateType(rr.Type) });
			}
		}

		public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, object data) {
			return CompileFactoryCall("TypeIs", new[] { typeof(Expression), typeof(Type) }, new[] { VisitResolveResult(rr.Input, null), _instantiateType(rr.TargetType) });
		}

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, object data) {
			var instance = rr.Member.IsStatic ? JsExpression.Null : VisitResolveResult(rr.TargetResult, null);
			if (rr.TargetResult.Type.Kind == TypeKind.Array && rr.Member.Name == "Length")
				return CompileFactoryCall("ArrayLength", new[] { typeof(Expression) }, new[] { instance });

			if (rr.Member is IPropertySymbol)
				return CompileFactoryCall("Property", new[] { typeof(Expression), typeof(PropertyInfo) }, new[] { instance, _getMember(rr.Member) });
			if (rr.Member is IFieldSymbol)
				return CompileFactoryCall("Field", new[] { typeof(Expression), typeof(FieldInfo) }, new[] { instance, _getMember(rr.Member) });
			else
				throw new ArgumentException("Unsupported member " + rr + " in expression tree");
		}

		private List<IMember> GetMemberPath(ResolveResult rr) {
			var result = new List<IMember>();
			for (var mrr = rr as MemberResolveResult; mrr != null; mrr = mrr.TargetResult as MemberResolveResult) {
				result.Insert(0, mrr.Member);
			}
			return result;
		}

		private List<Tuple<List<IMember>, IList<ResolveResult>, IMethodSymbol>> BuildAssignmentMap(IEnumerable<ResolveResult> initializers) {
			var result = new List<Tuple<List<IMember>, IList<ResolveResult>, IMethodSymbol>>();
			foreach (var init in initializers) {
				if (init is OperatorResolveResult) {
					var orr = init as OperatorResolveResult;
					if (orr.OperatorType != ExpressionType.Assign)
						throw new InvalidOperationException("Invalid initializer " + init);
					result.Add(Tuple.Create(GetMemberPath(orr.Operands[0]), (IList<ResolveResult>)new[] { orr.Operands[1] }, (IMethodSymbol)null));
				}
				else if (init is InvocationResolveResult) {
					var irr = init as InvocationResolveResult;
					if (irr.Member.Name != "Add")
						throw new InvalidOperationException("Invalid initializer " + init);
					result.Add(Tuple.Create(GetMemberPath(irr.TargetResult), irr.GetArgumentsForCall(), (IMethodSymbol)irr.Member));
				}
				else
					throw new InvalidOperationException("Invalid initializer " + init);
			}
			return result;
		}

		private bool FirstNEqual<T>(IList<T> first, IList<T> second, int count) {
			if (first.Count < count || second.Count < count)
				return false;
			for (int i = 0; i < count; i++) {
				if (!Equals(first[i], second[i]))
					return false;
			}
			return true;
		}

		private Tuple<List<JsExpression>, bool> GenerateMemberBindings(IEnumerator<Tuple<List<IMember>, IList<ResolveResult>, IMethodSymbol>> initializers, int index) {
			var firstPath = initializers.Current.Item1;
			var result = new List<JsExpression>();
			bool hasMore = true;
			do {
				var currentTarget = initializers.Current.Item1[index];
				if (initializers.Current.Item1.Count > index + 1) {
					var innerBindings = GenerateMemberBindings(initializers, index + 1);
					result.Add(CompileFactoryCall("MemberBind", new[] { typeof(MemberInfo), typeof(MemberBinding[]) }, new[] { _getMember(currentTarget), JsExpression.ArrayLiteral(innerBindings.Item1) }));

					if (!innerBindings.Item2) {
						hasMore = false;
						break;
					}
				}
				else if (initializers.Current.Item3 != null) {
					var currentPath = initializers.Current.Item1;
					var elements = new List<JsExpression>();
					do {
						elements.Add(CompileFactoryCall("ElementInit", new[] { typeof(MethodInfo), typeof(Expression[]) }, new[] { _getMember(initializers.Current.Item3), JsExpression.ArrayLiteral(initializers.Current.Item2.Select(i => VisitResolveResult(i, null))) }));
						if (!initializers.MoveNext()) {
							hasMore = false;
							break;
						}
					} while (FirstNEqual(currentPath, initializers.Current.Item1, index + 1));

					result.Add(CompileFactoryCall("ListBind", new[] { typeof(MemberInfo), typeof(ElementInit[]) }, new[] { _getMember(currentTarget), JsExpression.ArrayLiteral(elements) }));

					if (!hasMore)
						break;
				}
				else {
					result.Add(CompileFactoryCall("Bind", new[] { typeof(MemberInfo), typeof(Expression) }, new[] { _getMember(currentTarget), VisitResolveResult(initializers.Current.Item2[0], null) }));

					if (!initializers.MoveNext()) {
						hasMore = false;
						break;
					}
				}
			} while (FirstNEqual(firstPath, initializers.Current.Item1, index));

			return Tuple.Create(result, hasMore);
		}

		public override JsExpression VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, object data) {
			return VisitInvocationResolveResult(rr, data);
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, object data) {
			if (rr.Member.ContainingType.Kind == TypeKind.Delegate && rr.Member.Name == "Invoke") {
				return CompileFactoryCall("Invoke", new[] { typeof(Type), typeof(Expression), typeof(Expression[]) }, new[] { _instantiateType(rr.Type), VisitResolveResult(rr.TargetResult, null), JsExpression.ArrayLiteral(rr.GetArgumentsForCall().Select(a => VisitResolveResult(a, null))) });
			}
			else if (rr.Member is IMethodSymbol && ((IMethodSymbol)rr.Member).IsConstructor) {
				if (rr.Member.ContainingType.Kind == TypeKind.Anonymous) {
					var args    = new List<JsExpression>();
					var members = new List<JsExpression>();
					foreach (var init in rr.InitializerStatements) {
						var assign = init as OperatorResolveResult;
						if (assign == null || assign.OperatorType != ExpressionType.Assign || !(assign.Operands[0] is MemberResolveResult) || !(((MemberResolveResult)assign.Operands[0]).Member is IPropertySymbol))
							throw new Exception("Invalid anonymous type initializer " + init);
						args.Add(VisitResolveResult(assign.Operands[1], null));
						members.Add(_getMember(((MemberResolveResult)assign.Operands[0]).Member));
					}
					return CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]), typeof(MemberInfo[]) }, new[] { _getMember(rr.Member), JsExpression.ArrayLiteral(args), JsExpression.ArrayLiteral(members) });
				}
				else {
					var result = CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]) }, new[] { _getMember(rr.Member), JsExpression.ArrayLiteral(rr.GetArgumentsForCall().Select(a => VisitResolveResult(a, null))) });
					if (rr.InitializerStatements.Count > 0) {
						if (rr.InitializerStatements[0] is InvocationResolveResult && ((InvocationResolveResult)rr.InitializerStatements[0]).TargetResult is InitializedObjectResolveResult) {
							var elements = new List<JsExpression>();
							foreach (var stmt in rr.InitializerStatements) {
								var irr = stmt as InvocationResolveResult;
								if (irr == null)
									throw new Exception("Expected list initializer, was " + stmt);
								elements.Add(CompileFactoryCall("ElementInit", new[] { typeof(MethodInfo), typeof(Expression[]) }, new[] { _getMember(irr.Member), JsExpression.ArrayLiteral(irr.Arguments.Select(i => VisitResolveResult(i, null))) }));
							}
							result = CompileFactoryCall("ListInit", new[] { typeof(NewExpression), typeof(ElementInit[]) }, new[] { result, JsExpression.ArrayLiteral(elements) });
						}
						else {
							var map = BuildAssignmentMap(rr.InitializerStatements);
							using (IEnumerator<Tuple<List<IMember>, IList<ResolveResult>, IMethodSymbol>> enm = map.GetEnumerator()) {
								enm.MoveNext();
								var bindings = GenerateMemberBindings(enm, 0);
								result = CompileFactoryCall("MemberInit", new[] { typeof(NewExpression), typeof(MemberBinding[]) }, new[] { result, JsExpression.ArrayLiteral(bindings.Item1) });
							}
						}
					}
					return result;
				}
			}
			else {
				var member = rr.Member is IPropertySymbol ? ((IPropertySymbol)rr.Member).Getter : rr.Member;	// If invoking a property (indexer), use the get method.
				return CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { member.IsStatic ? JsExpression.Null : VisitResolveResult(rr.TargetResult, null), _getMember(member), JsExpression.ArrayLiteral(rr.GetArgumentsForCall().Select(a => VisitResolveResult(a, null))) });
			}
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, object data) {
			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _instantiateType(rr.ReferencedType), _instantiateType(rr.Type) });
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, object data) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			throw new InvalidOperationException("Resolve result " + rr + " is not handled.");
		}

		private JsExpression MakeConstant(ResolveResult rr) {
			JsExpression value;
			if (rr.ConstantValue == null) {
				value = _getDefaultValue(rr.Type);
			}
			else {
				object o = JSModel.Utils.ConvertToDoubleOrStringOrBoolean(rr.ConstantValue);
				if (o is double)
					value = JsExpression.Number((double)o);
				else if (o is bool)
					value = JsExpression.Boolean((bool)o);
				else
					value = JsExpression.String((string)o);
			}

			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { value, _instantiateType(rr.Type) });
		}

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, object data) {
			return MakeConstant(rr);
		}

		public override JsExpression VisitSizeOfResolveResult(SizeOfResolveResult rr, object data) {
			if (rr.ConstantValue == null) {
				// This is an internal error because AFAIK, using sizeof() with anything that doesn't return a compile-time constant (with our enum extensions) can only be done in an unsafe context.
				throw new Exception("Cannot take the size of type " + rr.ReferencedType.FullName);
			}
			return MakeConstant(rr);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, object data)
		{
			var array = VisitResolveResult(rr.Array, null);
			if (rr.Indexes.Count == 1)
				return CompileFactoryCall("ArrayIndex", new[] { typeof(Type), typeof(Expression), typeof(Expression) }, new[] { _instantiateType(rr.Type), array, VisitResolveResult(rr.Indexes[0], null) });
			else
				return CompileFactoryCall("ArrayIndex", new[] { typeof(Type), typeof(Expression), typeof(Expression[]) }, new[] { _instantiateType(rr.Type), array, JsExpression.ArrayLiteral(rr.Indexes.Select(i => VisitResolveResult(i, null))) });
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, object data) {
			if (rr.InitializerElements != null)
				return CompileFactoryCall("NewArrayInit", new[] { typeof(Type), typeof(Expression[]) }, new[] { _instantiateType(rr.Type), JsExpression.ArrayLiteral(rr.InitializerElements.Select(e => VisitResolveResult(e, null))) });
			else
				return CompileFactoryCall("NewArrayBounds", new[] { typeof(Type), typeof(Expression[]) }, new[] { _instantiateType(rr.Type), JsExpression.ArrayLiteral(rr.SizeArguments.Select(a => VisitResolveResult(a, null))) });
		}

		public override JsExpression VisitThisResolveResult(ThisExpressionSyntax rr, object data) {
			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _this, _instantiateType(rr.Type) });
		}
#endif
	}
}
