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
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class ExpressionTreeBuilder : CSharpSyntaxVisitor<JsExpression> {
		private readonly SemanticModel _semanticModel;
		private readonly IMetadataImporter _metadataImporter;
		private readonly List<JsStatement> _additionalStatements;
		private readonly INamedTypeSymbol _expression;
		private readonly Func<string> _createTemporaryVariable;
		private readonly Func<IMethodSymbol, JsExpression, JsExpression[], ExpressionCompileResult> _compileMethodCall;
		private readonly Func<ITypeSymbol, JsExpression> _instantiateType;
		private readonly Func<ITypeSymbol, JsExpression> _getDefaultValue;
		private readonly Func<ISymbol, JsExpression> _getMember;
		private readonly Func<ISymbol, JsExpression> _createLocalReferenceExpression;
		private readonly JsExpression _this;
		private readonly Dictionary<IParameterSymbol, string> _allParameters;
		#warning TODO: Replace with check of IMethodSymbol.CheckForOverflow
		private bool _checkForOverflow;

		public ExpressionTreeBuilder(SemanticModel semanticModel, IMetadataImporter metadataImporter, Func<string> createTemporaryVariable, Func<IMethodSymbol, JsExpression, JsExpression[], ExpressionCompileResult> compileMethodCall, Func<ITypeSymbol, JsExpression> instantiateType, Func<ITypeSymbol, JsExpression> getDefaultValue, Func<ISymbol, JsExpression> getMember, Func<ISymbol, JsExpression> createLocalReferenceExpression, JsExpression @this, bool checkForOverflow) {
			_semanticModel = semanticModel;
			_metadataImporter = metadataImporter;
			_expression = (INamedTypeSymbol)semanticModel.Compilation.GetTypeByMetadataName(typeof(System.Linq.Expressions.Expression).FullName);
			_createTemporaryVariable = createTemporaryVariable;
			_compileMethodCall = compileMethodCall;
			_instantiateType = instantiateType;
			_getDefaultValue = getDefaultValue;
			_getMember = getMember;
			_createLocalReferenceExpression = createLocalReferenceExpression;
			_this = @this;
			_allParameters = new Dictionary<IParameterSymbol, string>();
			_additionalStatements = new List<JsStatement>();
			_checkForOverflow = checkForOverflow;
		}

		public ExpressionCompileResult BuildExpressionTree(IReadOnlyList<ParameterSyntax> parameters, ExpressionSyntax body) {
			var expr = HandleLambda(parameters, body);
			return new ExpressionCompileResult(expr, _additionalStatements);
		}

		private bool TypeMatches(ITypeSymbol t1, Type t2) {
			var at = t1 as IArrayTypeSymbol;
			if (at != null)
				return t2.IsArray && at.Rank == t2.GetArrayRank() && TypeMatches(at.ElementType, t2.GetElementType());
			else if (t2.IsArray)
				return false;

			return t1.Name == t2.Name && t1.ContainingNamespace.FullyQualifiedName() == t2.Namespace;
		}

		private bool TypesMatch(IMethodSymbol method, Type[] argumentTypes) {
			if (method.Parameters.Length != argumentTypes.Length)
				return false;
			for (int i = 0; i < argumentTypes.Length; i++) {
				if (!TypeMatches(method.Parameters[i].Type, argumentTypes[i]))
					return false;
			}
			return true;
		}

		private JsExpression CompileFactoryCall(string factoryMethodName, Type[] argumentTypes, JsExpression[] arguments) {
			var method = _expression.GetMembers().OfType<IMethodSymbol>().Single(m => m.Name == factoryMethodName && m.TypeParameters.Length == 0 && TypesMatch(m, argumentTypes));
			var result = _compileMethodCall(method, JsExpression.Null, arguments);
			_additionalStatements.AddRange(result.AdditionalStatements);
			return result.Expression;
		}

		private JsExpression HandleLambda(IReadOnlyList<ParameterSyntax> parameters, ExpressionSyntax body) {
			var jsparams = new JsExpression[parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				var paramSymbol = _semanticModel.GetDeclaredSymbol(parameters[i]);
				var temp = _createTemporaryVariable();
				_allParameters[paramSymbol] = temp;
				_additionalStatements.Add(JsStatement.Var(temp, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(paramSymbol.Type), JsExpression.String(paramSymbol.Name) })));
				jsparams[i] = JsExpression.Identifier(temp);
			}

			var jsbody = Visit(body);
			return CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { jsbody, JsExpression.ArrayLiteral(jsparams) });
		}

		public override JsExpression Visit(SyntaxNode node) {
			var expr = node as ExpressionSyntax;
			if (expr == null) {
				throw new Exception("Unexpected node " + node);
			}

			var result = base.Visit(node);

			return ProcessConversion(result, expr);
		}

		private JsExpression Visit(ArgumentForCall a) {
			if (a.Argument != null)
				return Visit(a.Argument);
			else if (a.ParamArray != null)
				return CompileFactoryCall("NewArrayInit", new[] { typeof(Type), typeof(Expression[]) }, new[] { _instantiateType(a.ParamArray.Item1), JsExpression.ArrayLiteral(a.ParamArray.Item2.Select(Visit)) });
			else
				throw new Exception("Default values are not supported in expression trees");	// C# does not support this at all
		}

		private JsExpression ProcessConversion(JsExpression js, ExpressionSyntax cs) {
			var typeInfo = _semanticModel.GetTypeInfo(cs);
			var conversion = _semanticModel.GetConversion(cs);
			return PerformConversion(js, conversion, typeInfo.Type, typeInfo.ConvertedType, cs);
		}

		public override JsExpression VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) {
			return Visit(node.Expression);
		}

		public override JsExpression VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) {
			return HandleLambda(new[] { node.Parameter }, (ExpressionSyntax)node.Body);
		}

		public override JsExpression VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) {
			return HandleLambda(node.ParameterList.Parameters, (ExpressionSyntax)node.Body);
		}

		public override JsExpression VisitIdentifierName(IdentifierNameSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			string name;
			if (symbol is IParameterSymbol && _allParameters.TryGetValue((IParameterSymbol)symbol, out name)) {
				return JsExpression.Identifier(name);
			}
			if (symbol is ILocalSymbol || symbol is IParameterSymbol) {
				return _createLocalReferenceExpression(symbol);
			}
			else if (symbol is IPropertySymbol) {
				return CompileFactoryCall("Property", new[] { typeof(Expression), typeof(PropertyInfo) }, new[] { symbol.IsStatic ? JsExpression.Null : CreateThis(symbol.ContainingType), _getMember(symbol) });
			}
			else if (symbol is IFieldSymbol) {
				return CompileFactoryCall("Field", new[] { typeof(Expression), typeof(FieldInfo) }, new[] { symbol.IsStatic ? JsExpression.Null : CreateThis(symbol.ContainingType), _getMember(symbol) });
			}
			else if (symbol is IMethodSymbol) {
				// Must be the target of a method group conversion
				return symbol.IsStatic ? JsExpression.Null : CreateThis(symbol.ContainingType);
			}
			
			throw new Exception("Invalid identifier " + node);
		}

		public override JsExpression VisitGenericName(GenericNameSyntax node) {
			// Must be the target of a method group conversion
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			return symbol.IsStatic ? JsExpression.Null : CreateThis(symbol.ContainingType);
		}

		private ExpressionType MapNodeType(SyntaxKind syntaxKind) {
			switch (syntaxKind) {
				case SyntaxKind.SimpleAssignmentExpression:      return ExpressionType.Assign;
				case SyntaxKind.AddAssignmentExpression:         return _checkForOverflow ? ExpressionType.AddAssignChecked : ExpressionType.AddAssign;
				case SyntaxKind.AndAssignmentExpression:         return ExpressionType.AndAssign;
				case SyntaxKind.DivideAssignmentExpression:      return ExpressionType.DivideAssign;
				case SyntaxKind.ExclusiveOrAssignmentExpression: return ExpressionType.ExclusiveOrAssign;
				case SyntaxKind.LeftShiftAssignmentExpression:   return ExpressionType.LeftShiftAssign;
				case SyntaxKind.ModuloAssignmentExpression:      return ExpressionType.ModuloAssign;
				case SyntaxKind.MultiplyAssignmentExpression:    return _checkForOverflow ? ExpressionType.MultiplyAssignChecked : ExpressionType.Multiply;
				case SyntaxKind.OrAssignmentExpression:          return ExpressionType.OrAssign;
				case SyntaxKind.RightShiftAssignmentExpression:  return ExpressionType.RightShiftAssign;
				case SyntaxKind.SubtractAssignmentExpression:    return _checkForOverflow ? ExpressionType.SubtractAssignChecked : ExpressionType.SubtractAssign;
				case SyntaxKind.AddExpression:                   return _checkForOverflow ? ExpressionType.AddChecked : ExpressionType.Add;
				case SyntaxKind.BitwiseAndExpression:            return ExpressionType.And;
				case SyntaxKind.LogicalAndExpression:            return ExpressionType.AndAlso;
				case SyntaxKind.CoalesceExpression:              return ExpressionType.Coalesce;
				case SyntaxKind.DivideExpression:                return ExpressionType.Divide;
				case SyntaxKind.ExclusiveOrExpression:           return ExpressionType.ExclusiveOr;
				case SyntaxKind.GreaterThanExpression:           return ExpressionType.GreaterThan;
				case SyntaxKind.GreaterThanOrEqualExpression:    return ExpressionType.GreaterThanOrEqual;
				case SyntaxKind.EqualsExpression:                return ExpressionType.Equal;
				case SyntaxKind.LeftShiftExpression:             return ExpressionType.LeftShift;
				case SyntaxKind.LessThanExpression:              return ExpressionType.LessThan;
				case SyntaxKind.LessThanOrEqualExpression:       return ExpressionType.LessThanOrEqual;
				case SyntaxKind.ModuloExpression:                return ExpressionType.Modulo;
				case SyntaxKind.MultiplyExpression:              return _checkForOverflow ? ExpressionType.MultiplyChecked : ExpressionType.Multiply;
				case SyntaxKind.NotEqualsExpression:             return ExpressionType.NotEqual;
				case SyntaxKind.BitwiseOrExpression:             return ExpressionType.Or;
				case SyntaxKind.LogicalOrExpression:             return ExpressionType.OrElse;
				case SyntaxKind.RightShiftExpression:            return ExpressionType.RightShift;
				case SyntaxKind.SubtractExpression:              return _checkForOverflow ? ExpressionType.SubtractChecked : ExpressionType.Subtract;
				case SyntaxKind.AsExpression:                    return ExpressionType.TypeAs;
				case SyntaxKind.IsExpression:                    return ExpressionType.TypeIs;

				case SyntaxKind.PreIncrementExpression:          return ExpressionType.PreIncrementAssign;
				case SyntaxKind.PreDecrementExpression:          return ExpressionType.PreDecrementAssign;
				case SyntaxKind.UnaryMinusExpression:            return _checkForOverflow ? ExpressionType.NegateChecked : ExpressionType.Negate;
				case SyntaxKind.UnaryPlusExpression:             return ExpressionType.UnaryPlus;
				case SyntaxKind.LogicalNotExpression:            return ExpressionType.Not;
				case SyntaxKind.BitwiseNotExpression:            return ExpressionType.OnesComplement;

				case SyntaxKind.PostIncrementExpression:         return ExpressionType.PostIncrementAssign;
				case SyntaxKind.PostDecrementExpression:         return ExpressionType.PostDecrementAssign;

				default: throw new ArgumentException("Invalid syntax kind " + syntaxKind);
			}
		}

		public override JsExpression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node) {
			var methodSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			bool isUserDefined = methodSymbol.MethodKind == MethodKind.UserDefinedOperator && _metadataImporter.GetMethodSemantics(methodSymbol).Type != MethodScriptSemantics.ImplType.NativeOperator;
			var arguments = new[] { Visit(node.Operand), isUserDefined ? _getMember(methodSymbol) : _instantiateType(_semanticModel.GetTypeInfo(node).Type) };
			return CompileFactoryCall(MapNodeType(node.CSharpKind()).ToString(), new[] { typeof(Expression), isUserDefined ? typeof(MethodInfo) : typeof(Type) }, arguments);
		}

		public override JsExpression VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node) {
			var methodSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			bool isUserDefined = methodSymbol.MethodKind == MethodKind.UserDefinedOperator && _metadataImporter.GetMethodSemantics(methodSymbol).Type != MethodScriptSemantics.ImplType.NativeOperator;
			var arguments = new[] { Visit(node.Operand), isUserDefined ? _getMember(methodSymbol) : _instantiateType(_semanticModel.GetTypeInfo(node).Type) };
			return CompileFactoryCall(MapNodeType(node.CSharpKind()).ToString(), new[] { typeof(Expression), isUserDefined ? typeof(MethodInfo) : typeof(Type) }, arguments);
		}

		public override JsExpression VisitBinaryExpression(BinaryExpressionSyntax node) {
			var syntaxKind = node.CSharpKind();
			if (syntaxKind == SyntaxKind.IsExpression || syntaxKind == SyntaxKind.AsExpression) {
				return CompileFactoryCall(MapNodeType(syntaxKind).ToString(), new[] { typeof(Expression), typeof(Type) }, new[] { Visit(node.Left), _instantiateType((ITypeSymbol)_semanticModel.GetSymbolInfo(node.Right).Symbol) });
			}
			else {
				var methodSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				bool isUserDefined = methodSymbol != null && methodSymbol.MethodKind == MethodKind.UserDefinedOperator && _metadataImporter.GetMethodSemantics(methodSymbol).Type != MethodScriptSemantics.ImplType.NativeOperator;
				var arguments = new[] { Visit(node.Left), Visit(node.Right), isUserDefined ? _getMember(methodSymbol) : _instantiateType(_semanticModel.GetTypeInfo(node).Type) };
				return CompileFactoryCall(MapNodeType(syntaxKind).ToString(), new[] { typeof(Expression), typeof(Expression), isUserDefined ? typeof(MethodInfo) : typeof(Type) }, arguments);
			}
		}

		public override JsExpression VisitConditionalExpression(ConditionalExpressionSyntax node) {
			var arguments = new[] { Visit(node.Condition), Visit(node.WhenTrue), Visit(node.WhenFalse), _instantiateType(_semanticModel.GetTypeInfo(node).Type) };
			return CompileFactoryCall("Condition", new[] { typeof(Expression), typeof(Expression), typeof(Expression), typeof(Type) }, arguments);
		}

		private JsExpression PerformConversion(JsExpression input, Conversion c, ITypeSymbol fromType, ITypeSymbol toType, ExpressionSyntax csharpInput) {
			if (c.IsIdentity || csharpInput is LiteralExpressionSyntax || csharpInput is DefaultExpressionSyntax || csharpInput is SizeOfExpressionSyntax) {
				return input;
			}
			else if (c.IsAnonymousFunction) {
				var result = input;
				if (toType.MetadataName == typeof(Expression<>).Name && toType.ContainingNamespace.FullyQualifiedName() == typeof(Expression<>).Namespace)
					result = CompileFactoryCall("Quote", new[] { typeof(Expression) }, new[] { result });
				return result;
			}
			else if (c.IsNullLiteral) {
				return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { input, _instantiateType(toType) });
			}
			else if (c.IsMethodGroup) {
				var methodInfo = _semanticModel.Compilation.GetTypeByMetadataName(typeof(MethodInfo).FullName);
				return CompileFactoryCall("Convert", new[] { typeof(Expression), typeof(Type) }, new[] {
				           CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { 
				               CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _getMember(c.MethodSymbol), _instantiateType(methodInfo) }),
				               _getMember(methodInfo.GetMembers("CreateDelegate").OfType<IMethodSymbol>().Single(m => m.Parameters.Length == 2 && m.Parameters[0].Type.FullyQualifiedName() == typeof(Type).FullName && m.Parameters[1].Type.FullyQualifiedName() == typeof(object).FullName)),
				               JsExpression.ArrayLiteral(
				                   _instantiateType(toType),
				                   c.MethodSymbol.IsStatic ? JsExpression.Null : input
				               )
				           }),
				           _instantiateType(toType)
				       });
			}
			else {
				string methodName = _checkForOverflow ? "ConvertChecked" : "Convert";
				if (c.IsUserDefined)
					return CompileFactoryCall(methodName, new[] { typeof(Expression), typeof(Type), typeof(MethodInfo) }, new[] { input, _instantiateType(toType), _getMember(c.MethodSymbol) });
				else
					return CompileFactoryCall(methodName, new[] { typeof(Expression), typeof(Type) }, new[] { input, _instantiateType(toType) });
			}
		}

		public override JsExpression VisitCastExpression(CastExpressionSyntax node) {
			var info = _semanticModel.GetCastInfo(node);
			var input = Visit(node.Expression);
			return PerformConversion(input, info.Conversion, info.FromType, info.ToType, node.Expression);
		}

		public override JsExpression VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;

			var instance = symbol.IsStatic ? JsExpression.Null : Visit(node.Expression);
			if (_semanticModel.GetTypeInfo(node.Expression).Type.TypeKind == TypeKind.ArrayType && symbol.Name == "Length")
				return CompileFactoryCall("ArrayLength", new[] { typeof(Expression) }, new[] { instance });

			if (symbol is IPropertySymbol) {
				return CompileFactoryCall("Property", new[] { typeof(Expression), typeof(PropertyInfo) }, new[] { instance, _getMember(symbol) });
			}
			else if (symbol is IFieldSymbol) {
				return CompileFactoryCall("Field", new[] { typeof(Expression), typeof(FieldInfo) }, new[] { instance, _getMember(symbol) });
			}
			else if (symbol is IMethodSymbol) {
				// Must be the target of a method group conversion
				return Visit(node.Expression);
			}
			else {
				throw new ArgumentException("Unsupported member " + symbol + " in expression tree");
			}
		}

		private JsExpression GetTargetForInvocation(ExpressionSyntax expression, ISymbol method) {
			var mae = expression as MemberAccessExpressionSyntax;
			if (mae != null) {
				return Visit(mae.Expression);
			}

			if (expression is IdentifierNameSyntax || expression is GenericNameSyntax)
				return method.IsStatic ? JsExpression.Null : CreateThis(method.ContainingType);

			throw new Exception("Unsupported target for invocation " + expression);
		}

		public override JsExpression VisitInvocationExpression(InvocationExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			var arguments = _semanticModel.GetArgumentMap(node).ArgumentsForCall.Select(Visit);
			if (symbol.ContainingType.TypeKind == TypeKind.Delegate && symbol.Name == "Invoke") {
				return CompileFactoryCall("Invoke", new[] { typeof(Type), typeof(Expression), typeof(Expression[]) }, new[] { _instantiateType(_semanticModel.GetTypeInfo(node).Type), Visit(node.Expression), JsExpression.ArrayLiteral(arguments) });
			}
			else {
				return CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { GetTargetForInvocation(node.Expression, symbol), _getMember(symbol), JsExpression.ArrayLiteral(arguments) });
			}
		}

		private JsExpression GenerateElementInits(IEnumerable<ExpressionSyntax> initializers) {
			var result = new List<JsExpression>();
			foreach (var initializer in initializers) {
				var collectionInitializer = _semanticModel.GetCollectionInitializerSymbolInfo(initializer).Symbol;
				if (collectionInitializer == null)
					throw new Exception("Expected a collection initializer");

				var elements = initializer is InitializerExpressionSyntax ? ((InitializerExpressionSyntax)initializer).Expressions.Select(Visit) : new[] { Visit(initializer) };

				result.Add(CompileFactoryCall("ElementInit", new[] { typeof(MethodInfo), typeof(Expression[]) }, new[] { _getMember(collectionInitializer), JsExpression.ArrayLiteral(elements) }));
			}

			return JsExpression.ArrayLiteral(result);
		}

		private JsExpression HandleInitializers(IEnumerable<ExpressionSyntax> initializers) {
			var result = new List<JsExpression>();
			foreach (var initializer in initializers) {
				if (initializer.CSharpKind() != SyntaxKind.SimpleAssignmentExpression)
					throw new Exception("Invalid initializer " + initializer);
				var be = (BinaryExpressionSyntax)initializer;
				var member = _semanticModel.GetSymbolInfo(be.Left).Symbol;

				var ies = be.Right as InitializerExpressionSyntax;
				if (ies != null) {
					if (ies.CSharpKind() == SyntaxKind.CollectionInitializerExpression) {
						var elements = GenerateElementInits(ies.Expressions);
						result.Add(CompileFactoryCall("ListBind", new[] { typeof(MemberInfo), typeof(ElementInit[]) }, new[] { _getMember(member), elements }));
					}
					else {
						var inner = HandleInitializers(ies.Expressions);
						result.Add(CompileFactoryCall("MemberBind", new[] { typeof(MemberInfo), typeof(MemberBinding[]) }, new[] { _getMember(member), inner }));
					}
				}
				else {
					result.Add(CompileFactoryCall("Bind", new[] { typeof(MemberInfo), typeof(Expression) }, new[] { _getMember(member), Visit(be.Right) }));
				}
			}

			return JsExpression.ArrayLiteral(result);
		}

		public override JsExpression VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
			var ctor = _semanticModel.GetSymbolInfo(node).Symbol;
			var arguments = _semanticModel.GetArgumentMap(node).ArgumentsForCall.Select(Visit);
			var result = CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]) }, new[] { _getMember(ctor), JsExpression.ArrayLiteral(arguments) });

			if (node.Initializer != null) {
				if (node.Initializer.CSharpKind() == SyntaxKind.CollectionInitializerExpression) {
					var elements = GenerateElementInits(node.Initializer.Expressions);
					result = CompileFactoryCall("ListInit", new[] { typeof(NewExpression), typeof(ElementInit[]) }, new[] { result, elements });
				}
				else {
					var initializers = HandleInitializers(node.Initializer.Expressions);
					result = CompileFactoryCall("MemberInit", new[] { typeof(NewExpression), typeof(MemberBinding[]) }, new[] { result, initializers });
				}
			}
			return result;
		}

		public override JsExpression VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node) {
			var ctor = _semanticModel.GetSymbolInfo(node).Symbol;

			var args    = new List<JsExpression>();
			var members = new List<JsExpression>();
			foreach (var init in node.Initializers) {
				args.Add(Visit(init.Expression));
				members.Add(_getMember(_semanticModel.GetDeclaredSymbol(init)));
			}
			return CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]), typeof(MemberInfo[]) }, new[] { _getMember(ctor), JsExpression.ArrayLiteral(args), JsExpression.ArrayLiteral(members) });
		}

		public override JsExpression VisitTypeOfExpression(TypeOfExpressionSyntax node) {
			var type = (ITypeSymbol)_semanticModel.GetSymbolInfo(node.Type).Symbol;
			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _instantiateType(type), _instantiateType(_semanticModel.GetTypeInfo(node).Type) });
		}

		private JsExpression MakeConstant(object value, ITypeSymbol type) {
			JsExpression jsvalue;
			if (value == null) {
				jsvalue = type.IsReferenceType || type.IsNullable() ? JsExpression.Null : _getDefaultValue(type);
			}
			else {
				object o = JSModel.Utils.ConvertToDoubleOrStringOrBoolean(value);
				if (o is double)
					jsvalue = JsExpression.Number((double)o);
				else if (o is bool)
					jsvalue = JsExpression.Boolean((bool)o);
				else
					jsvalue = JsExpression.String((string)o);
			}

			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { jsvalue, _instantiateType(type) });
		}

		public override JsExpression VisitLiteralExpression(LiteralExpressionSyntax node) {
			var value = _semanticModel.GetConstantValue(node);
			if (!value.HasValue) {
				throw new Exception("Literal does not have constant value");
			}
			return MakeConstant(value.Value, _semanticModel.GetTypeInfo(node).ConvertedType);
		}

		public override JsExpression VisitDefaultExpression(DefaultExpressionSyntax node) {
			return MakeConstant(null, _semanticModel.GetTypeInfo(node).ConvertedType);
		}

		public override JsExpression VisitSizeOfExpression(SizeOfExpressionSyntax node) {
			var value = _semanticModel.GetConstantValue(node);
			if (!value.HasValue) {
				// This is an internal error because AFAIK, using sizeof() with anything that doesn't return a compile-time constant (with our enum extensions) can only be done in an unsafe context.
				throw new Exception("sizeof is not constant");
			}
			return MakeConstant(value.Value, _semanticModel.GetTypeInfo(node).ConvertedType);
		}

		public override JsExpression VisitElementAccessExpression(ElementAccessExpressionSyntax node) {
			var target = Visit(node.Expression);

			if (_semanticModel.GetTypeInfo(node.Expression).ConvertedType.TypeKind == TypeKind.ArrayType) {
				var arguments = node.ArgumentList.Arguments.Select(a => Visit(a.Expression));

				if (node.ArgumentList.Arguments.Count == 1) {
					return CompileFactoryCall("ArrayIndex", new[] { typeof(Type), typeof(Expression), typeof(Expression) }, new[] { _instantiateType(_semanticModel.GetTypeInfo(node).Type), target, arguments.Single() });
				}
				else {
					return CompileFactoryCall("ArrayIndex", new[] { typeof(Type), typeof(Expression), typeof(Expression[]) }, new[] { _instantiateType(_semanticModel.GetTypeInfo(node).Type), target, JsExpression.ArrayLiteral(arguments) });
				}
			}
			else {
				var property = (IPropertySymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				var arguments = _semanticModel.GetArgumentMap(node).ArgumentsForCall.Select(Visit);
				return CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { target, _getMember(property.GetMethod), JsExpression.ArrayLiteral(arguments) });
			}
		}

		private JsExpression HandleArrayCreation(IArrayTypeSymbol arrayType, IReadOnlyList<ArrayRankSpecifierSyntax> rankSpecifiers, InitializerExpressionSyntax initializer) {
			if (initializer != null)
				return CompileFactoryCall("NewArrayInit", new[] { typeof(Type), typeof(Expression[]) }, new[] { _instantiateType(arrayType.ElementType), JsExpression.ArrayLiteral(initializer.Expressions.Select(Visit)) });
			else
				return CompileFactoryCall("NewArrayBounds", new[] { typeof(Type), typeof(Expression[]) }, new[] { _instantiateType(arrayType.ElementType), JsExpression.ArrayLiteral(rankSpecifiers[0].Sizes.Select(Visit)) });
		}

		public override JsExpression VisitArrayCreationExpression(ArrayCreationExpressionSyntax node) {
			return HandleArrayCreation((IArrayTypeSymbol)_semanticModel.GetTypeInfo(node).Type, node.Type.RankSpecifiers, node.Initializer);
		}

		public override JsExpression VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node) {
			return HandleArrayCreation((IArrayTypeSymbol)_semanticModel.GetTypeInfo(node).Type, null, node.Initializer);
		}

		public override JsExpression VisitThisExpression(ThisExpressionSyntax node) {
			return CreateThis(_semanticModel.GetTypeInfo(node).Type);
		}

		public override JsExpression VisitBaseExpression(BaseExpressionSyntax node) {
			return CreateThis(_semanticModel.GetTypeInfo(node).Type);
		}

		public override JsExpression VisitCheckedExpression(CheckedExpressionSyntax node) {
			var oldCheckForOverflow = _checkForOverflow;
			_checkForOverflow = node.CSharpKind() == SyntaxKind.CheckedExpression;
			var result = Visit(node.Expression);
			_checkForOverflow = oldCheckForOverflow;
			return result;
		}

		private JsExpression CreateThis(ITypeSymbol type) {
			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _this, _instantiateType(type) });
		}
	}
}
