using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.Compiler.Expressions;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
#warning TODO This class should have access to the error reporter
	public class ExpressionTreeBuilder : CSharpSyntaxVisitor<JsExpression> {
		private readonly SemanticModel _semanticModel;
		private readonly IMetadataImporter _metadataImporter;
		private readonly List<JsStatement> _additionalStatements;
		private readonly INamedTypeSymbol _expression;
		private readonly Func<string> _createTemporaryVariable;
		private readonly IDictionary<ISymbol, VariableData> _allVariables;
		private readonly Func<IMethodSymbol, JsExpression, JsExpression[], ExpressionCompileResult> _compileMethodCall;
		private readonly Func<ITypeSymbol, JsExpression> _instantiateType;
		private readonly Func<ITypeSymbol, IEnumerable<Tuple<JsExpression, string>>, JsIdentifierExpression> _instantiateTransparentType;
		private readonly Func<ITypeSymbol, JsExpression> _getDefaultValue;
		private readonly Func<ISymbol, JsExpression> _getMember;
		private readonly Func<ISymbol, JsExpression> _createLocalReferenceExpression;
		private readonly JsExpression _this;
		private Dictionary<ISymbol, JsExpression> _allParameters;
		#warning TODO: Replace with check of IMethodSymbol.CheckForOverflow
		private bool _checkForOverflow;

		#warning TODO same fix for anonymous types as for transparent types

		public ExpressionTreeBuilder(SemanticModel semanticModel, IMetadataImporter metadataImporter, Func<string> createTemporaryVariable, IDictionary<ISymbol, VariableData> allVariables, Func<IMethodSymbol, JsExpression, JsExpression[], ExpressionCompileResult> compileMethodCall, Func<ITypeSymbol, JsExpression> instantiateType, Func<ITypeSymbol, IEnumerable<Tuple<JsExpression, string>>, JsIdentifierExpression> instantiateTransparentType, Func<ITypeSymbol, JsExpression> getDefaultValue, Func<ISymbol, JsExpression> getMember, Func<ISymbol, JsExpression> createLocalReferenceExpression, JsExpression @this, bool checkForOverflow) {
			_semanticModel = semanticModel;
			_metadataImporter = metadataImporter;
			_expression = (INamedTypeSymbol)semanticModel.Compilation.GetTypeByMetadataName(typeof(System.Linq.Expressions.Expression).FullName);
			_createTemporaryVariable = createTemporaryVariable;
			_allVariables = allVariables;
			_compileMethodCall = compileMethodCall;
			_instantiateType = instantiateType;
			_instantiateTransparentType = instantiateTransparentType;
			_getDefaultValue = getDefaultValue;
			_getMember = getMember;
			_createLocalReferenceExpression = createLocalReferenceExpression;
			_this = @this;
			_allParameters = new Dictionary<ISymbol, JsExpression>();
			_additionalStatements = new List<JsStatement>();
			_checkForOverflow = checkForOverflow;
		}

		public ExpressionCompileResult BuildExpressionTree(IReadOnlyList<ParameterSyntax> parameters, ExpressionSyntax body) {
			var expr = HandleLambda(parameters, body);
			return new ExpressionCompileResult(expr, _additionalStatements);
		}

		private class RangeVariableSubstitutionBuilder : ExpressionCompiler.IQueryContextVisitor<Tuple<JsExpression, ITypeSymbol>, int> {
			private readonly ExpressionTreeBuilder _builder;

			private RangeVariableSubstitutionBuilder(ExpressionTreeBuilder builder) {
				_builder = builder;
			}

			public int VisitRange(ExpressionCompiler.RangeQueryContext c, Tuple<JsExpression, ITypeSymbol> data) {
				_builder._allParameters[c.Variable] = data.Item2 != null ? _builder.CompileFactoryCall("Property", new[] { typeof(Expression), typeof(PropertyInfo) }, new[] { data.Item1, _builder.GetProperty(_builder._instantiateType(data.Item2), c.Name) }) : data.Item1;
				return 0;
			}

			public int VisitTransparentType(ExpressionCompiler.TransparentTypeQueryContext c, Tuple<JsExpression, ITypeSymbol> data) {
				var target = data.Item2 != null ? _builder.CompileFactoryCall("Property", new[] { typeof(Expression), typeof(PropertyInfo) }, new[] { data.Item1, _builder.GetProperty(_builder._instantiateType(data.Item2), c.Name) }) : data.Item1;
				var innerData = Tuple.Create(target, c.TransparentType);
				c.Left.Accept(this, innerData);
				c.Right.Accept(this, innerData);
				return 0;
			}

			public static void Process(ExpressionCompiler.QueryContext c, JsExpression param, ExpressionTreeBuilder builder) {
				var obj = new RangeVariableSubstitutionBuilder(builder);
				c.Accept(obj, Tuple.Create(param, (ITypeSymbol)null));
			}
		}

		internal ExpressionCompileResult BuildQueryExpressionTree(ExpressionCompiler.QueryContext context, ITypeSymbol mainParameterType, Tuple<IRangeVariableSymbol, ITypeSymbol, string> additionalParameter, ExpressionSyntax body, Func<JsExpression, JsExpression> returnValueModifier) {
			var jsparams = new JsExpression[additionalParameter != null ? 2 : 1];
			var p1 = _createTemporaryVariable();
			jsparams[0] = JsExpression.Identifier(p1);
			RangeVariableSubstitutionBuilder.Process(context, jsparams[0], this);
			_additionalStatements.Add(JsStatement.Var(p1, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(mainParameterType), JsExpression.String(context.Name) })));

			if (additionalParameter != null) {
				var p2 = _createTemporaryVariable();
				jsparams[1] = JsExpression.Identifier(p2);
				_additionalStatements.Add(JsStatement.Var(p2, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(additionalParameter.Item2), JsExpression.String(additionalParameter.Item3) })));
				_allParameters[additionalParameter.Item1] = JsExpression.Identifier(p2);
			}

			var jsbody = Visit(body);
			jsbody = returnValueModifier(jsbody);
			var expr = CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { jsbody, JsExpression.ArrayLiteral(jsparams) });

			return new ExpressionCompileResult(expr, _additionalStatements);
		}

		internal ExpressionCompileResult AddMemberToTransparentType(ExpressionCompiler.QueryContext context, string newMemberName, ExpressionSyntax newMemberValue, ITypeSymbol oldType, ITypeSymbol newTransparentType) {
			var p1 = _createTemporaryVariable();
			RangeVariableSubstitutionBuilder.Process(context, JsExpression.Identifier(p1), this);

			_additionalStatements.Add(JsStatement.Var(p1, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(oldType), JsExpression.String(context.Name) })));

			var body = CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]), typeof(MemberInfo[]) }, new[] { GetConstructor(_instantiateType(newTransparentType)), JsExpression.ArrayLiteral(JsExpression.Identifier(p1), Visit(newMemberValue)), JsExpression.ArrayLiteral(GetProperty(_instantiateType(newTransparentType), context.Name), GetProperty(_instantiateType(newTransparentType), newMemberName)) });
			var expr = CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { body, JsExpression.ArrayLiteral(JsExpression.Identifier(p1)) });

			return new ExpressionCompileResult(expr, _additionalStatements);
		}

		internal ExpressionCompileResult AddMemberToTransparentType(ExpressionCompiler.QueryContext context, string newMemberName, ITypeSymbol newMemberType, ITypeSymbol oldType, ITypeSymbol newTransparentType) {
			var p1 = _createTemporaryVariable();
			var p2 = _createTemporaryVariable();
			RangeVariableSubstitutionBuilder.Process(context, JsExpression.Identifier(p1), this);

			_additionalStatements.Add(JsStatement.Var(p1, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(oldType), JsExpression.String(context.Name) })));
			_additionalStatements.Add(JsStatement.Var(p2, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(newMemberType), JsExpression.String(newMemberName) })));

			var body = CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]), typeof(MemberInfo[]) }, new[] { GetConstructor(_instantiateType(newTransparentType)), JsExpression.ArrayLiteral(JsExpression.Identifier(p1), JsExpression.Identifier(p2)), JsExpression.ArrayLiteral(GetProperty(_instantiateType(newTransparentType), context.Name), GetProperty(_instantiateType(newTransparentType), newMemberName)) });
			var expr = CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { body, JsExpression.ArrayLiteral(JsExpression.Identifier(p1), JsExpression.Identifier(p2)) });

			return new ExpressionCompileResult(expr, _additionalStatements);
		}

		internal JsExpression Call(IMethodSymbol method, JsExpression target, IEnumerable<JsExpression> arguments) {
			if (method.ReducedFrom != null) {
				arguments = new[] { target }.Concat(arguments).ToList();
				target = JsExpression.Null;
				method = method.UnReduceIfExtensionMethod();
			}

			return CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { target, _getMember(method), JsExpression.ArrayLiteral(arguments) });
		}

		private JsExpression GetProperty(JsExpression type, string propertyName) {
			var getPropertyMethod = _semanticModel.Compilation.GetTypeByMetadataName(typeof(Type).FullName).GetMembers("GetProperty").OfType<IMethodSymbol>().Single(m => m.Parameters.Length == 1 && m.Parameters[0].Type.SpecialType == SpecialType.System_String);
			var result = _compileMethodCall(getPropertyMethod, type, new[] { JsExpression.String(propertyName) });
			_additionalStatements.AddRange(result.AdditionalStatements);
			return result.Expression;
		}

		private JsExpression GetConstructor(JsExpression type) {
			var getConstructorsMethod = _semanticModel.Compilation.GetTypeByMetadataName(typeof(Type).FullName).GetMembers("GetConstructors").OfType<IMethodSymbol>().Single(m => m.Parameters.Length == 0);
			var result = _compileMethodCall(getConstructorsMethod, type, new JsExpression[0]);
			_additionalStatements.AddRange(result.AdditionalStatements);
			return JsExpression.Index(result.Expression, JsExpression.Number(0));
		}

		private JsExpression CreateThis(ITypeSymbol type) {
			return CompileFactoryCall("Constant", new[] { typeof(object), typeof(Type) }, new[] { _this, _instantiateType(type) });
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
			var method = _expression.GetNonConstructorNonAccessorMethods().Single(m => m.Name == factoryMethodName && m.TypeParameters.Length == 0 && TypesMatch(m, argumentTypes));
			var result = _compileMethodCall(method, JsExpression.Null, arguments);
			_additionalStatements.AddRange(result.AdditionalStatements);
			return result.Expression;
		}

		private JsExpression HandleLambda(IReadOnlyList<ParameterSyntax> parameters, ExpressionSyntax body) {
			var jsparams = new JsExpression[parameters.Count];
			for (int i = 0; i < parameters.Count; i++) {
				var paramSymbol = _semanticModel.GetDeclaredSymbol(parameters[i]);
				var temp = _createTemporaryVariable();
				_allParameters[paramSymbol] = JsExpression.Identifier(temp);
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
			JsExpression identifier;
			if (_allParameters.TryGetValue(symbol, out identifier)) {
				return identifier;
			}
			else if (symbol is ILocalSymbol || symbol is IParameterSymbol) {
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
				if (toType.IsExpressionOfT())
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
			if (method.IsStatic)
				return JsExpression.Null;

			var mae = expression as MemberAccessExpressionSyntax;
			if (mae != null)
				return Visit(mae.Expression);

			if (expression is IdentifierNameSyntax || expression is GenericNameSyntax)
				return CreateThis(method.ContainingType);

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
				var collectionInitializer = _semanticModel.GetCollectionInitializerSymbolInfoWorking(initializer);
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

		public override JsExpression VisitQueryExpression(QueryExpressionSyntax node) {
			var current = HandleFirstFromClause(node.FromClause);
			return HandleQueryBody(node.Body, current).Item2;
		}

		private ExpressionCompiler.QueryExpressionCompilationInfo HandleFirstFromClause(FromClauseSyntax node) {
			var result = Visit(node.Expression);
			var info = _semanticModel.GetQueryClauseInfo(node);
			var type = _semanticModel.GetTypeInfo(node.Expression).ConvertedType;
			if (info.CastInfo.Symbol != null) {
				result = CompileMethodInvocation((IMethodSymbol)info.CastInfo.Symbol, type, result);
				type = ((IMethodSymbol)info.CastInfo.Symbol).ReturnType;
			} 

			var rv = _semanticModel.GetDeclaredSymbol(node);
			return new ExpressionCompiler.QueryExpressionCompilationInfo(result, type, new ExpressionCompiler.RangeQueryContext(rv, _allVariables[rv].Name));
		}

		private Tuple<ITypeSymbol, JsExpression> HandleQueryBody(QueryBodySyntax body, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			for (int i = 0; i < body.Clauses.Count; i++) {
				var clause = body.Clauses[i];

				if (clause is LetClauseSyntax) {
					current = HandleLetClause((LetClauseSyntax)clause, current);
				}
				else if (clause is FromClauseSyntax) {
					current = HandleAdditionalFromClause((FromClauseSyntax)clause, i == body.Clauses.Count - 1 ? body.SelectOrGroup as SelectClauseSyntax : null, current);
				}
				else if (clause is JoinClauseSyntax) {
					current = HandleJoinClause((JoinClauseSyntax)clause, i == body.Clauses.Count - 1 ? body.SelectOrGroup as SelectClauseSyntax : null, current);
				}
				else if (clause is WhereClauseSyntax) {
					current = HandleWhereClause((WhereClauseSyntax)clause, current);
				}
				else if (clause is OrderByClauseSyntax) {
					current = HandleOrderByClause((OrderByClauseSyntax)clause, current);
				}
				else {
					throw new Exception("Invalid query clause " + clause);
				}
			}
			var result = HandleSelectOrGroupClause(body.SelectOrGroup, current);

			if (body.Continuation != null) {
				var continuationVariable = _semanticModel.GetDeclaredSymbol(body.Continuation);
				result = HandleQueryBody(body.Continuation.Body, new ExpressionCompiler.QueryExpressionCompilationInfo(result.Item2, result.Item1, new ExpressionCompiler.RangeQueryContext(continuationVariable, _allVariables[continuationVariable].Name)));
			}

			return result;
		}

		private ExpressionCompiler.QueryExpressionCompilationInfo HandleLetClause(LetClauseSyntax clause, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetQueryClauseInfo(clause).OperationInfo.Symbol;
			var delegateType = (INamedTypeSymbol)method.Parameters[0].Type;

			if (delegateType.IsExpressionOfT()) {
				delegateType = (INamedTypeSymbol)delegateType.TypeArguments[0];
				#warning TODO Quote
			}

			var oldType = delegateType.DelegateInvokeMethod.Parameters[0].Type;
			var newTransparentType = delegateType.DelegateInvokeMethod.ReturnType;
			var newMemberType = _semanticModel.GetTypeInfo(clause.Expression).ConvertedType;
			var newMemberSymbol = _semanticModel.GetDeclaredSymbol(clause);
			var newMemberName = _allVariables[newMemberSymbol].Name;

			var p1 = _createTemporaryVariable();
			RangeVariableSubstitutionBuilder.Process(current.CurrentContext, JsExpression.Identifier(p1), this);
			_additionalStatements.Add(JsStatement.Var(p1, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(oldType), JsExpression.String(current.CurrentContext.Name) })));

			var jsNewTransparentType = _instantiateTransparentType(newTransparentType, new[] { Tuple.Create(_instantiateType(oldType), current.CurrentContext.Name), Tuple.Create(_instantiateType(newMemberType), newMemberName) });
			var body = CompileFactoryCall("New", new[] { typeof(ConstructorInfo), typeof(Expression[]), typeof(MemberInfo[]) }, new[] { GetConstructor(jsNewTransparentType), JsExpression.ArrayLiteral(JsExpression.Identifier(p1), Visit(clause.Expression)), JsExpression.ArrayLiteral(GetProperty(_instantiateType(newTransparentType), current.CurrentContext.Name), GetProperty(_instantiateType(newTransparentType), newMemberName)) });
			var lambda = CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { body, JsExpression.ArrayLiteral(JsExpression.Identifier(p1)) });

			return new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, lambda), method.ReturnType, current.CurrentContext.WrapInTransparentType(p1, newTransparentType, oldType, newMemberSymbol, newMemberType, newMemberName));
		}

		private ExpressionCompiler.QueryExpressionCompilationInfo HandleJoinClause(JoinClauseSyntax clause, SelectClauseSyntax followingSelect, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			var clauseInfo = _semanticModel.GetQueryClauseInfo(clause);
			var newVariable = _semanticModel.GetDeclaredSymbol(clause);
			var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
			var other = Visit(clause.InExpression);

			if (clauseInfo.CastInfo.Symbol != null) {
				other = CompileMethodInvocation((IMethodSymbol)clauseInfo.CastInfo.Symbol, null, other);
			}
			var leftSelector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, clause.LeftExpression, current, null, null);
			var rightSelector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[2].Type, clause.RightExpression, new ExpressionCompiler.QueryExpressionCompilationInfo(JsExpression.Null, _semanticModel.GetTypeInfo(clause.RightExpression).ConvertedType, new ExpressionCompiler.RangeQueryContext(newVariable, _allVariables[newVariable].Name)), null, null);

			var secondArgToProjector = (clause.Into != null ? _semanticModel.GetDeclaredSymbol(clause.Into) : newVariable);

			if (followingSelect != null) {
				var projection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[3].Type, followingSelect.Expression, current, Tuple.Create(secondArgToProjector, ((INamedTypeSymbol)method.Parameters[3].Type.UnpackExpression()).DelegateInvokeMethod.Parameters[1].Type, _allVariables[secondArgToProjector].Name), null);
				return new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, other, leftSelector, rightSelector, projection), method.ReturnType, null);
			}
			else {
				var newInfo = AddMemberToTransparentType(current, (INamedTypeSymbol)method.Parameters[3].Type, method.ReturnType, secondArgToProjector);
				return new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, other, leftSelector, rightSelector, newInfo.Result), method.ReturnType, newInfo.CurrentContext);
			}
		}

		private ExpressionCompiler.QueryExpressionCompilationInfo HandleWhereClause(WhereClauseSyntax clause, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetQueryClauseInfo(clause).OperationInfo.Symbol;
			var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, clause.Condition, current, null, null);
			return new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, lambda), method.ReturnType, current.CurrentContext);
		}

		private ExpressionCompiler.QueryExpressionCompilationInfo HandleOrderByClause(OrderByClauseSyntax clause, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			foreach (var ordering in clause.Orderings) {
				var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(ordering).Symbol;
				var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, ordering.Expression, current, null, null);
				current = new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, lambda), method.ReturnType, current.CurrentContext);
			}
			return current;
		}

		#warning TODO return only JsExpression
		private ExpressionCompiler.QueryExpressionCompilationInfo AddMemberToTransparentType(ExpressionCompiler.QueryExpressionCompilationInfo current, INamedTypeSymbol delegateType, ITypeSymbol callReturnType, IRangeVariableSymbol newMember) {
			if (delegateType.IsExpressionOfT()) {
				#warning TODO Quote
				delegateType = (INamedTypeSymbol)delegateType.TypeArguments[0];
			}

			var oldType = delegateType.DelegateInvokeMethod.Parameters[0].Type;
			var newMemberType = delegateType.DelegateInvokeMethod.Parameters[1].Type;
			var newTransparentType = delegateType.DelegateInvokeMethod.ReturnType;

			var jsNewTransparentType = _instantiateTransparentType(newTransparentType, new[] { Tuple.Create(_instantiateType(oldType), current.CurrentContext.Name), Tuple.Create(_instantiateType(newMemberType), _allVariables[newMember].Name) });
			var info = AddMemberToTransparentType(current.CurrentContext, _allVariables[newMember].Name, newMemberType, oldType, newTransparentType);
			return new ExpressionCompiler.QueryExpressionCompilationInfo(info.Expression, callReturnType, current.CurrentContext.WrapInTransparentType(jsNewTransparentType.Name, newTransparentType, oldType, newMember, newMemberType, _allVariables[newMember].Name));
		}

		private ExpressionCompiler.QueryExpressionCompilationInfo HandleAdditionalFromClause(FromClauseSyntax clause, SelectClauseSyntax followingSelect, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			var clauseInfo = _semanticModel.GetQueryClauseInfo(clause);
			var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
			var delegateType = (INamedTypeSymbol)method.Parameters[1].Type;
			var innerSelection = CompileQueryLambda(delegateType, clause.Expression, current, null, (IMethodSymbol)clauseInfo.CastInfo.Symbol);

			if (followingSelect != null) {
				var rv = (IRangeVariableSymbol)_semanticModel.GetDeclaredSymbol(clause);
				var projection = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, followingSelect.Expression, current, Tuple.Create(rv, ((INamedTypeSymbol)delegateType.UnpackExpression()).DelegateInvokeMethod.Parameters[1].Type, _allVariables[rv].Name), null);
				return new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, innerSelection, projection), method.ReturnType, null);
			}
			else {
				var newInfo = AddMemberToTransparentType(current, delegateType, method.ReturnType, _semanticModel.GetDeclaredSymbol(clause));
				return new ExpressionCompiler.QueryExpressionCompilationInfo(CompileMethodInvocation(method, current.CurrentType, current.Result, innerSelection, newInfo.Result), method.ReturnType, newInfo.CurrentContext);
			}
		}

		private Tuple<ITypeSymbol, JsExpression> HandleSelectOrGroupClause(SelectOrGroupClauseSyntax node, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			if (node is SelectClauseSyntax) {
				return HandleSelectClause((SelectClauseSyntax)node, current);
			}
			else if (node is GroupClauseSyntax) {
				return HandleGroupClause((GroupClauseSyntax)node, current);
			}
			else {
				throw new Exception("Invalid node " + node);
			}
		}

		private Tuple<ITypeSymbol, JsExpression> HandleSelectClause(SelectClauseSyntax node, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			if (method != null) {
				var lambda = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, node.Expression, current, null, null);
				return Tuple.Create(method.ReturnType, CompileMethodInvocation(method, current.CurrentType, current.Result, lambda));
			}
			else {
				return Tuple.Create(current.CurrentType, current.Result);
			}
		}

		private Tuple<ITypeSymbol, JsExpression> HandleGroupClause(GroupClauseSyntax node, ExpressionCompiler.QueryExpressionCompilationInfo current) {
			var method = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
			var grouping = CompileQueryLambda((INamedTypeSymbol)method.Parameters[0].Type, node.ByExpression, current, null, null);

			switch (method.Parameters.Length) {
				case 1: {
					return Tuple.Create(method.ReturnType, CompileMethodInvocation(method, current.CurrentType, current.Result, grouping));
				}

				case 2: {
					var selector = CompileQueryLambda((INamedTypeSymbol)method.Parameters[1].Type, node.GroupExpression, current, null, null);
					return Tuple.Create(method.ReturnType, CompileMethodInvocation(method, current.CurrentType, current.Result, grouping, selector));
				}

				default: {
					throw new Exception("Invalid GroupBy call");
				}
			}
		}

		private JsExpression CompileQueryLambda(INamedTypeSymbol delegateType, ExpressionSyntax expression, ExpressionCompiler.QueryExpressionCompilationInfo info, Tuple<IRangeVariableSymbol, ITypeSymbol, string> additionalParameter, IMethodSymbol methodToInvokeOnBody) {
			if (delegateType.IsExpressionOfT()) {
				#warning TODO quote
				delegateType = (INamedTypeSymbol)delegateType.TypeArguments[0];
			}

			var oldParameters = _allParameters;
			_allParameters = new Dictionary<ISymbol, JsExpression>(_allParameters);
			try {
				var jsparams = new JsExpression[additionalParameter != null ? 2 : 1];
				var p1 = _createTemporaryVariable();
				jsparams[0] = JsExpression.Identifier(p1);
				RangeVariableSubstitutionBuilder.Process(info.CurrentContext, JsExpression.Identifier(p1), this);
				_additionalStatements.Add(JsStatement.Var(p1, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(delegateType.DelegateInvokeMethod.Parameters[0].Type), JsExpression.String(info.CurrentContext.Name) })));

				if (additionalParameter != null) {
					var p2 = _createTemporaryVariable();
					jsparams[1] = _allParameters[additionalParameter.Item1] = JsExpression.Identifier(p2);
					_additionalStatements.Add(JsStatement.Var(p2, CompileFactoryCall("Parameter", new[] { typeof(Type), typeof(string) }, new[] { _instantiateType(additionalParameter.Item2), JsExpression.String(additionalParameter.Item3) })));
				}

				var body = Visit(expression);
				if (methodToInvokeOnBody != null)
					body = CompileMethodInvocation(methodToInvokeOnBody, null, body);

				return CompileFactoryCall("Lambda", new[] { typeof(Expression), typeof(ParameterExpression[]) }, new[] { body, JsExpression.ArrayLiteral(jsparams) });
			}
			finally {
				_allParameters = oldParameters;
			}
		}

		private JsExpression CompileMethodInvocation(IMethodSymbol method, ITypeSymbol targetType, JsExpression target, params JsExpression[] args) {
			if (method.ContainingType.TypeKind == TypeKind.Delegate && method.Name == "Invoke") {
				throw new Exception("delegate invocation in query pattern");
			}
			else if (method.ReducedFrom != null) {
				method = method.UnReduceIfExtensionMethod();
				if (targetType != null)
					target = PerformConversion(target, _semanticModel.Compilation.ClassifyConversion(targetType, method.Parameters[0].Type), targetType, method.Parameters[0].Type, null);
				return CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { JsExpression.Null, _getMember(method), JsExpression.ArrayLiteral(new[] { target }.Concat(args)) });
			}
			else {
				if (targetType != null)
					target = PerformConversion(target, _semanticModel.Compilation.ClassifyConversion(targetType, method.ContainingType), targetType, method.ContainingType, null);
				return CompileFactoryCall("Call", new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }, new[] { target, _getMember(method), JsExpression.ArrayLiteral(args) });
			}
		}
	}
}
