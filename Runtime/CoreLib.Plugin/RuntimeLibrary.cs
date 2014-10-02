using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Plugin {
	public class RuntimeLibrary : IRuntimeLibrary {
		private const string System_Script = "System.Script";

		private enum TypeContext {
			TypeOf,
			GetScriptType,
			GenericArgument,
		}

		private readonly IMetadataImporter _metadataImporter;
		private readonly IErrorReporter _errorReporter;
		private readonly Compilation _compilation;
		private readonly INamer _namer;
		private readonly IAttributeStore _attributeStore;
		private readonly bool _omitDowncasts;
		private readonly bool _omitNullableChecks;

		public RuntimeLibrary(IMetadataImporter metadataImporter, IErrorReporter errorReporter, Compilation compilation, INamer namer, IAttributeStore attributeStore) {
			_metadataImporter = metadataImporter;
			_errorReporter = errorReporter;
			_compilation = compilation;
			_namer = namer;
			_attributeStore = attributeStore;
			_omitDowncasts = MetadataUtils.OmitDowncasts(compilation, _attributeStore);
			_omitNullableChecks = MetadataUtils.OmitNullableChecks(compilation, _attributeStore);
		}

		private MethodScriptSemantics GetMethodSemantics(IMethodSymbol m) {
			if (m.IsAccessor()) {
				var prop = m.AssociatedSymbol as IPropertySymbol;
				if (prop != null) {
					var psem = _metadataImporter.GetPropertySemantics(prop);
					if (psem.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods)
						throw new InvalidOperationException("Property " + prop.Name + " should be implemented with get/set methods");
					if (m.Equals(prop.GetMethod))
						return psem.GetMethod;
					else if (m.Equals(prop.SetMethod))
						return psem.SetMethod;
					else
						throw new Exception(m + " is neither the GetMethod nor the SetMethod for " + prop);
				}

				var evt = m.AssociatedSymbol as IEventSymbol;
				if (evt != null) {
					var esem = _metadataImporter.GetEventSemantics(evt);
					if (esem.Type != EventScriptSemantics.ImplType.AddAndRemoveMethods)
						throw new InvalidOperationException("Event " + prop.Name + " should be implemented with add/remove methods");
					if (m.Equals(evt.AddMethod))
						return esem.AddMethod;
					else if (m.Equals(evt.RemoveMethod))
						return esem.RemoveMethod;
					else
						throw new Exception(m + " is neither the adder nor the remover for " + evt);
				}

				throw new ArgumentException("Invalid accessor owner " + m.ContainingSymbol + " on member " + m);
			}
			else
				return _metadataImporter.GetMethodSemantics(m);
		}

		private JsTypeReferenceExpression CreateTypeReferenceExpression(INamedTypeSymbol type) {
			return new JsTypeReferenceExpression(type);
		}

		private JsTypeReferenceExpression CreateTypeReferenceExpression(SpecialType type) {
			return new JsTypeReferenceExpression(_compilation.GetSpecialType(type));
		}

		private JsTypeReferenceExpression CreateTypeReferenceExpression(string typeName) {
			return new JsTypeReferenceExpression(_compilation.GetTypeByMetadataName(typeName));
		}

		private JsExpression GetTypeDefinitionScriptType(INamedTypeSymbol type, TypeContext context) {
			var sem = _metadataImporter.GetTypeSemantics(type);
			if (sem.Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_errorReporter.Message(Saltarelle.Compiler.Messages._7522, type.FullyQualifiedName());
				return JsExpression.Null;
			}

			if (context != TypeContext.GetScriptType && context != TypeContext.TypeOf && !MetadataUtils.DoesTypeObeyTypeSystem(type, _attributeStore)) {
				return CreateTypeReferenceExpression(SpecialType.System_Object);
			}
			else if (MetadataUtils.IsSerializable(type, _attributeStore) && !MetadataUtils.DoesTypeObeyTypeSystem(type, _attributeStore)) {
				return CreateTypeReferenceExpression(SpecialType.System_Object);
			}
			else {
				return CreateTypeReferenceExpression(type);
			}
		}

		private JsExpression GetScriptType(ITypeSymbol type, TypeContext typeContext, IRuntimeContext context) {
			if (type.TypeKind == TypeKind.Delegate) {
				return CreateTypeReferenceExpression(SpecialType.System_Delegate);
			}
			else if (type.TypeKind == TypeKind.ArrayType) {
				return CreateTypeReferenceExpression(SpecialType.System_Array);
			}
			else if (type is ITypeParameterSymbol) {
				return context.ResolveTypeParameter((ITypeParameterSymbol)type);
			}
			else if (type.IsAnonymousType || type.TypeKind == TypeKind.DynamicType) {
				return CreateTypeReferenceExpression(SpecialType.System_Object);
			}
			else if (type is INamedTypeSymbol) {
				var nt = (INamedTypeSymbol)type;
				if (nt.IsUnboundGenericType) {
					return CreateTypeReferenceExpression((INamedTypeSymbol)type.OriginalDefinition);
				}
				if (nt.TypeArguments.Length > 0) {
					var def = nt.OriginalDefinition;
					var sem = _metadataImporter.GetTypeSemantics(def);
					if (sem.Type != TypeScriptSemantics.ImplType.NotUsableFromScript && !sem.IgnoreGenericArguments)
						return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "makeGenericType"), CreateTypeReferenceExpression((INamedTypeSymbol)type.OriginalDefinition), JsExpression.ArrayLiteral(nt.TypeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument, context))));
					else
						return GetTypeDefinitionScriptType((INamedTypeSymbol)type.OriginalDefinition, typeContext);
				}
				else {
					return GetTypeDefinitionScriptType(nt, typeContext);
				}
			}
			else {
				throw new InvalidOperationException("Could not determine the script type for " + type + ", context " + typeContext);
			}
		}

		private bool IsSystemObjectReference(JsExpression expr) {
			return expr is JsTypeReferenceExpression && ((JsTypeReferenceExpression)expr).Type.SpecialType == SpecialType.System_Object;
		}

		private JsExpression GetCastTarget(ITypeSymbol type, IRuntimeContext context) {
			if (type == null)
				return null;

			var def = type.OriginalDefinition as INamedTypeSymbol;

			if (type.TypeKind == TypeKind.Enum) {
				var underlying = MetadataUtils.IsNamedValues(def, _attributeStore) ? _compilation.GetSpecialType(SpecialType.System_String) : def.EnumUnderlyingType;
				return CreateTypeReferenceExpression(underlying.OriginalDefinition);
			}

			if (def != null) {
				if (MetadataUtils.IsSerializable(def, _attributeStore) && string.IsNullOrEmpty(MetadataUtils.GetSerializableTypeCheckCode(def, _attributeStore)))
					return null;
				if (!MetadataUtils.DoesTypeObeyTypeSystem(def, _attributeStore))
					return null;
			}

			return GetScriptType(type, TypeContext.GetScriptType, context);
		}

		private JsExpression GetCastTarget(ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			var ss = GetCastTarget(sourceType, context);
			var st = GetCastTarget(targetType, context);
			if (st == null) {
				return null;	// The target is not a real type.
			}
			else if (ss is JsTypeReferenceExpression && st is JsTypeReferenceExpression) {
				var ts = ((JsTypeReferenceExpression)ss).Type;
				var tt = ((JsTypeReferenceExpression)st).Type;
				var sems = _metadataImporter.GetTypeSemantics(ts);
				var semt = _metadataImporter.GetTypeSemantics(tt);
				if (sems.Type != TypeScriptSemantics.ImplType.NotUsableFromScript && semt.Type != TypeScriptSemantics.ImplType.NotUsableFromScript && sems.Name == semt.Name && Equals(ts.ContainingAssembly, tt.ContainingAssembly))
					return null;	// The types are the same in script, so no runtime conversion is required.
			}

			return st;
		}

		public JsExpression TypeOf(ITypeSymbol type, IRuntimeContext context) {
			return GetScriptType(type, TypeContext.TypeOf, context);
		}

		public JsExpression InstantiateType(ITypeSymbol type, IRuntimeContext context) {
			return GetScriptType(type, TypeContext.GetScriptType, context);
		}

		public JsExpression InstantiateTypeForUseAsTypeArgumentInInlineCode(ITypeSymbol type, IRuntimeContext context) {
			return GetScriptType(type, TypeContext.GenericArgument, context);
		}

		private readonly Dictionary<INamedTypeSymbol, IMethodSymbol> _typeCheckMethods = new Dictionary<INamedTypeSymbol, IMethodSymbol>();
		private IMethodSymbol GetTypeCheckMethod(INamedTypeSymbol type) {
			IMethodSymbol result;
			if (!_typeCheckMethods.TryGetValue(type, out result))
				result = _typeCheckMethods[type] = MetadataUtils.CreateTypeCheckMethod((INamedTypeSymbol)type, _compilation);
			return result;
		}

		private JsExpression CompileImportedTypeCheckCode(ITypeSymbol type, ref JsExpression @this, IRuntimeContext context, bool isTypeIs) {
			var def = type.OriginalDefinition as INamedTypeSymbol;
			if (def == null)
				return null;
			var ia = _attributeStore.AttributesFor(def).GetAttribute<ImportedAttribute>();
			if (ia == null || string.IsNullOrEmpty(ia.TypeCheckCode))
				return null;

			// Can ignore errors here because they are caught by the metadata importer
			var method = GetTypeCheckMethod((INamedTypeSymbol)type);
			var tokens = InlineCodeMethodCompiler.Tokenize(method, ia.TypeCheckCode, _ => {});
			int thisCount = tokens.Count(t => t.Type == InlineCodeToken.TokenType.This);
			if (!isTypeIs || thisCount > 0)
				@this = context.EnsureCanBeEvaluatedMultipleTimes(@this, new JsExpression[0]);
			return JsExpression.LogicalAnd(
			           ReferenceNotEquals(@this, JsExpression.Null, context),
			           InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, @this, ImmutableArray<JsExpression>.Empty, n => { var t = _compilation.GetTypeByMetadataName(n); return t != null ? InstantiateType(t, context) : JsExpression.Null; }, t => InstantiateTypeForUseAsTypeArgumentInInlineCode(t, context), _ => {}));
		}

		public JsExpression TypeIs(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			var importedCheck = CompileImportedTypeCheckCode(targetType, ref expression, context, true);
			if (importedCheck != null)
				return importedCheck;

			var def = targetType.OriginalDefinition as INamedTypeSymbol;
			if (def != null && (!MetadataUtils.DoesTypeObeyTypeSystem(def, _attributeStore) || (MetadataUtils.IsSerializable(def, _attributeStore) && string.IsNullOrEmpty(MetadataUtils.GetSerializableTypeCheckCode(def, _attributeStore))))) {
				_errorReporter.Message(Messages._7701, targetType.FullyQualifiedName());
				return JsExpression.Null;
			}

			var jsTarget = GetCastTarget(sourceType, targetType, context);
			if (jsTarget == null || IsSystemObjectReference(jsTarget))
				return ReferenceNotEquals(expression, JsExpression.Null, context);
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "isInstanceOfType"), expression, jsTarget);
		}

		public JsExpression TryDowncast(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			JsExpression jsTarget = CompileImportedTypeCheckCode(targetType, ref expression, context, false);

			if (jsTarget == null) {
				var def = targetType.OriginalDefinition as INamedTypeSymbol;
				if (def != null && (!MetadataUtils.DoesTypeObeyTypeSystem(def, _attributeStore) || (MetadataUtils.IsSerializable(def, _attributeStore) && string.IsNullOrEmpty(MetadataUtils.GetSerializableTypeCheckCode(def, _attributeStore))))) {
					_errorReporter.Message(Messages._7702, targetType.FullyQualifiedName());
					return JsExpression.Null;
				}

				jsTarget = GetCastTarget(sourceType, targetType, context);
			}

			if (jsTarget == null || IsSystemObjectReference(jsTarget))
				return expression;

			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "safeCast"), expression, jsTarget);
		}

		public JsExpression Downcast(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			if (_omitDowncasts)
				return expression;

			if (sourceType.TypeKind == TypeKind.DynamicType && targetType.SpecialType == SpecialType.System_Boolean)
				return JsExpression.LogicalNot(JsExpression.LogicalNot(expression));

			JsExpression jsTarget = CompileImportedTypeCheckCode(targetType, ref expression, context, false);

			if (jsTarget == null)
				jsTarget = GetCastTarget(sourceType, targetType, context);

			if (jsTarget == null || IsSystemObjectReference(jsTarget))
				return expression;
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "cast"), expression, jsTarget);
		}

		public JsExpression Upcast(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			if (sourceType.SpecialType == SpecialType.System_Char)
				_errorReporter.Message(Messages._7700);
			return expression;
		}

		public JsExpression ReferenceEquals(JsExpression a, JsExpression b, IRuntimeContext context) {
			if (a.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "isNullOrUndefined"), b);
			else if (b.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "isNullOrUndefined"), a);
			else if (a.NodeType == ExpressionNodeType.String || b.NodeType == ExpressionNodeType.String)
				return JsExpression.Same(a, b);
			else
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "referenceEquals"), a, b);
		}

		public JsExpression ReferenceNotEquals(JsExpression a, JsExpression b, IRuntimeContext context) {
			if (a.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "isValue"), b);
			else if (b.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "isValue"), a);
			else if (a.NodeType == ExpressionNodeType.String || b.NodeType == ExpressionNodeType.String)
				return JsExpression.NotSame(a, b);
			else
				return JsExpression.LogicalNot(JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "referenceEquals"), a, b));
		}

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<ITypeSymbol> typeArguments, IRuntimeContext context) {
			return JsExpression.Invocation(method, typeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument, context)));
		}

		public JsExpression MakeException(JsExpression operand, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(typeof(System.Exception).FullName), "wrap"), operand);
		}

		public JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Int32), "div"), numerator, denominator);
		}

		public JsExpression FloatToInt(JsExpression operand, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Int32), "trunc"), operand);
		}

		public JsExpression Coalesce(JsExpression a, JsExpression b, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "coalesce"), a, b);
		}

		public JsExpression Lift(JsExpression expression, IRuntimeContext context) {
			if (expression is JsInvocationExpression) {
				var ie = (JsInvocationExpression)expression;
				if (ie.Method is JsMemberAccessExpression) {
					var mae = (JsMemberAccessExpression)ie.Method;
					if (mae.Target is JsTypeReferenceExpression) {
						var t = ((JsTypeReferenceExpression)mae.Target).Type;
						bool isIntegerType = t.SpecialType == SpecialType.System_Byte || t.SpecialType == SpecialType.System_SByte || t.SpecialType == SpecialType.System_Int16 || t.SpecialType == SpecialType.System_UInt16 || t.SpecialType == SpecialType.System_Char || t.SpecialType == SpecialType.System_Int32 || t.SpecialType == SpecialType.System_UInt32 || t.SpecialType == SpecialType.System_Int64 || t.SpecialType == SpecialType.System_UInt64;
						if (isIntegerType) {
							if (mae.MemberName == "div" || mae.MemberName == "trunc")
								return expression;
						}
					}
				}

				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Nullable_T), "lift"), new[] { ie.Method }.Concat(ie.Arguments));
			}
			if (expression is JsUnaryExpression) {
				string methodName = null;
				switch (expression.NodeType) {
					case ExpressionNodeType.LogicalNot: methodName = "not"; goto default;
					case ExpressionNodeType.Negate:     methodName = "neg"; goto default;
					case ExpressionNodeType.Positive:   methodName = "pos"; goto default;
					case ExpressionNodeType.BitwiseNot: methodName = "cpl"; goto default;

					default:
						if (methodName != null)
							return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Nullable_T), methodName), ((JsUnaryExpression)expression).Operand);
						break;
				}
			}
			else if (expression is JsBinaryExpression) {
				string methodName = null;
				switch (expression.NodeType) {
					case ExpressionNodeType.Equal:
					case ExpressionNodeType.Same:
						methodName = "eq";
						goto default;

					case ExpressionNodeType.NotEqual:
					case ExpressionNodeType.NotSame:
						methodName = "ne";
						goto default;

					case ExpressionNodeType.LesserOrEqual:      methodName = "le";   goto default;
					case ExpressionNodeType.GreaterOrEqual:     methodName = "ge";   goto default;
					case ExpressionNodeType.Lesser:             methodName = "lt";   goto default;
					case ExpressionNodeType.Greater:            methodName = "gt";   goto default;
					case ExpressionNodeType.Subtract:           methodName = "sub";  goto default;
					case ExpressionNodeType.Add:                methodName = "add";  goto default;
					case ExpressionNodeType.Modulo:             methodName = "mod";  goto default;
					case ExpressionNodeType.Divide:             methodName = "div";  goto default;
					case ExpressionNodeType.Multiply:           methodName = "mul";  goto default;
					case ExpressionNodeType.BitwiseAnd:         methodName = "band"; goto default;
					case ExpressionNodeType.BitwiseOr:          methodName = "bor";  goto default;
					case ExpressionNodeType.BitwiseXor:         methodName = "xor";  goto default;
					case ExpressionNodeType.LeftShift:          methodName = "shl";  goto default;
					case ExpressionNodeType.RightShiftSigned:   methodName = "srs";  goto default;
					case ExpressionNodeType.RightShiftUnsigned: methodName = "sru";  goto default;

					default:
						if (methodName != null)
							return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Nullable_T), methodName), ((JsBinaryExpression)expression).Left, ((JsBinaryExpression)expression).Right);
						break;
				}
			}

			throw new ArgumentException("Cannot lift expression " + OutputFormatter.Format(expression, true));
		}

		public JsExpression FromNullable(JsExpression expression, IRuntimeContext context) {
			if (_omitNullableChecks)
				return expression;

			if (expression.NodeType == ExpressionNodeType.LogicalNot)
				return expression;	// This is a little hacky. The problem we want to solve is that 'bool b = myDynamic' should compile to !!myDynamic, but the actual call is unbox(convert(myDynamic, bool)), where convert() will return the !!. Anyway, in JS, the !expression will never be null anyway.

			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "unbox"), expression);
		}

		public JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Nullable_T), "and"), a, b);
		}

		public JsExpression LiftedBooleanOr(JsExpression a, JsExpression b, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(SpecialType.System_Nullable_T), "or"), a, b);
		}

		public JsExpression Bind(JsExpression function, JsExpression target, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "mkdel"), target, function);
		}

		public JsExpression BindFirstParameterToThis(JsExpression function, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "thisFix"), function);
		}

		public JsExpression Default(ITypeSymbol type, IRuntimeContext context) {
			if (type.IsReferenceType || type.TypeKind == TypeKind.DynamicType) {
				return JsExpression.Null;
			}
			else if (type.TypeKind == TypeKind.Enum) {
				return MetadataUtils.IsNamedValues((INamedTypeSymbol)type, _attributeStore) ? JsExpression.Null : JsExpression.Number(0);
			}
			else if (type.IsNullable()) {
				return JsExpression.Null;
			}

			switch (type.SpecialType) {
				case SpecialType.System_Boolean:
					return JsExpression.False;
				case SpecialType.System_DateTime:
					return JsExpression.New(CreateTypeReferenceExpression(SpecialType.System_DateTime), JsExpression.Number(0));
				case SpecialType.System_Byte:
				case SpecialType.System_SByte:
				case SpecialType.System_Char:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Decimal:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
					return JsExpression.Number(0);
			}

			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "getDefaultValue"), GetScriptType(type, TypeContext.GetScriptType, context));
		}

		public JsExpression CreateArray(ITypeSymbol elementType, IEnumerable<JsExpression> size, IRuntimeContext context) {
			var sizeList = (size is IList<JsExpression>) ? (IList<JsExpression>)size : size.ToList();
			if (sizeList.Count == 1) {
				return JsExpression.New(CreateTypeReferenceExpression(SpecialType.System_Array), sizeList);
			}
			else {
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "multidimArray"), new[] { Default(elementType, context) }.Concat(sizeList));
			}
		}

		public JsExpression CloneDelegate(JsExpression source, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			if (Equals(sourceType, targetType)) {
				// The user does something like "D d1 = F(); var d2 = new D(d1)". Assume he does this for a reason and create a clone of the delegate.
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "delegateClone"), source);
			}
			else {
				return source;	// The clone is just to convert the delegate to a different type. The risk of anyone comparing the references is small, so just return the original as delegates are immutable anyway.
			}
		}

		public JsExpression CallBase(IMethodSymbol method, IEnumerable<JsExpression> thisAndArguments, IRuntimeContext context) {
			var impl = GetMethodSemantics(method);

			JsExpression jsMethod = JsExpression.Member(JsExpression.Member(GetScriptType(method.ContainingType, TypeContext.GetScriptType, context), "prototype"), impl.Name);
			
			if (method.TypeArguments.Length > 0 && !impl.IgnoreGenericArguments)
				jsMethod = InstantiateGenericMethod(jsMethod, method.TypeArguments, context);

			if (impl.ExpandParams) {
				var args = thisAndArguments.ToList();
				if (args[args.Count - 1] is JsArrayLiteralExpression) {
					return JsExpression.Invocation(JsExpression.Member(jsMethod, "call"), args.Take(args.Count - 1).Concat(((JsArrayLiteralExpression)args[args.Count - 1]).Elements));
				}
				else {
					return JsExpression.Invocation(JsExpression.Member(jsMethod, "apply"), args[0], args.Count == 2 ? args[1] : JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(args.Skip(1).Take(args.Count - 2)), "concat"), args[args.Count - 1]));
				}
			}
			else {
				return JsExpression.Invocation(JsExpression.Member(jsMethod, "call"), thisAndArguments);
			}
		}

		public JsExpression BindBaseCall(IMethodSymbol method, JsExpression @this, IRuntimeContext context) {
			var impl = GetMethodSemantics(method);

			JsExpression jsMethod = JsExpression.Member(JsExpression.Member(GetScriptType(method.ContainingType, TypeContext.GetScriptType, context), "prototype"), impl.Name);
			
			if (method.TypeArguments.Length > 0 && !impl.IgnoreGenericArguments)
				jsMethod = InstantiateGenericMethod(jsMethod, method.TypeArguments, context);

			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "mkdel"), @this, jsMethod);
		}

		public JsExpression MakeEnumerator(ITypeSymbol yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose, IRuntimeContext context) {
			return JsExpression.New(CreateTypeReferenceExpression("System.Collections.Generic.IteratorBlockEnumerator`1"), moveNext, getCurrent, dispose ?? (JsExpression)JsExpression.Null, JsExpression.This);
		}

		public JsExpression MakeEnumerable(ITypeSymbol yieldType, JsExpression getEnumerator, IRuntimeContext context) {
			return JsExpression.New(CreateTypeReferenceExpression("System.Collections.Generic.IteratorBlockEnumerable`1"), getEnumerator, JsExpression.This);
		}

		public JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "arrayGet"), new[] { array }.Concat(indices));
		}

		public JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "arraySet"), new[] { array }.Concat(indices).Concat(new[] { value }));
		}

		public JsExpression CreateTaskCompletionSource(ITypeSymbol taskGenericArgument, IRuntimeContext context) {
			return JsExpression.New(CreateTypeReferenceExpression("System.Threading.Tasks.TaskCompletionSource`1"));
		}

		public JsExpression SetAsyncResult(JsExpression taskCompletionSource, JsExpression value, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(taskCompletionSource, "setResult"), value ?? JsExpression.Null);
		}

		public JsExpression SetAsyncException(JsExpression taskCompletionSource, JsExpression exception, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(taskCompletionSource, "setException"), MakeException(exception, context));
		}

		public JsExpression GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource, IRuntimeContext context) {
			return JsExpression.Member(taskCompletionSource, "task");
		}

		public JsExpression ApplyConstructor(JsExpression constructor, JsExpression argumentsArray, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "applyConstructor"), constructor, argumentsArray);
		}

		public JsExpression ShallowCopy(JsExpression source, JsExpression target, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "shallowCopy"), source, target);
		}

		private int FindIndexInReflectableMembers(ISymbol member) {
			if (!MetadataUtils.IsReflectable(member, _attributeStore))
				return -1;

			int i = 0;
			foreach (var m in member.ContainingType.GetNonAccessorNonTypeMembers().Where(m => MetadataUtils.IsReflectable(m, _attributeStore))
			                                                                      .OrderBy(m => m, MemberOrderer.Instance)) {
				if (m.Equals(member))
					return i;
				i++;
			}
			throw new Exception("Member " + member + " not found even though it should be present");
		}

		public JsExpression GetMember(ISymbol member, IRuntimeContext context) {
			var owner = member is IMethodSymbol && ((IMethodSymbol)member).IsAccessor() ? ((IMethodSymbol)member).AssociatedSymbol : null;

			int index = FindIndexInReflectableMembers(owner ?? member);
			if (index >= 0) {
				JsExpression result = JsExpression.Index(
				                          JsExpression.Member(
				                              JsExpression.Member(
				                                  TypeOf(member.ContainingType, context),
				                                  "__metadata"),
				                              "members"),
				                          JsExpression.Number(index));
				if (owner != null) {
					if (owner is IPropertySymbol) {
						if (ReferenceEquals(member, ((IPropertySymbol)owner).GetMethod))
							result = JsExpression.MemberAccess(result, "getter");
						else if (ReferenceEquals(member, ((IPropertySymbol)owner).SetMethod))
							result = JsExpression.MemberAccess(result, "setter");
						else
							throw new ArgumentException("Invalid member " + member);
					}
					else if (owner is IEventSymbol) {
						if (ReferenceEquals(member, ((IEventSymbol)owner).AddMethod))
							result = JsExpression.MemberAccess(result, "adder");
						else if (ReferenceEquals(member, ((IEventSymbol)owner).RemoveMethod))
							result = JsExpression.MemberAccess(result, "remover");
						else
							throw new ArgumentException("Invalid member " + member);
					}
					else
						throw new ArgumentException("Invalid owner " + owner);
				}
				return result;
			}
			else {
				return MetadataUtils.ConstructMemberInfo(member, _compilation, _metadataImporter, _namer, this, _errorReporter, t => TypeOf(t, context), includeDeclaringType: true);
			}
		}

		public JsExpression GetAnonymousTypeInfo(INamedTypeSymbol anonymousType, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "anonymousType"), anonymousType.GetProperties().Select(p => JsExpression.ArrayLiteral(InstantiateType(p.Type, context), JsExpression.String(p.Name))));
		}

		public JsExpression GetTransparentTypeInfo(IEnumerable<Tuple<JsExpression, string>> members, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "anonymousType"), members.Select(m => JsExpression.ArrayLiteral(m.Item1, JsExpression.String(m.Item2))));
		}

		public JsExpression GetExpressionForLocal(string name, JsExpression accessor, ITypeSymbol type, IRuntimeContext context) {
			var scriptType = TypeOf(type, context);

			JsExpression getterDefinition = JsExpression.FunctionDefinition(new string[0], JsStatement.Return(accessor));
			JsExpression setterDefinition = JsExpression.FunctionDefinition(new[] { "$" }, JsExpression.Assign(accessor, JsExpression.Identifier("$")));
			if (UsesThisVisitor.Analyze(accessor)) {
				getterDefinition = JsExpression.Invocation(JsExpression.Member(getterDefinition, "bind"), JsExpression.This);
				setterDefinition = JsExpression.Invocation(JsExpression.Member(setterDefinition, "bind"), JsExpression.This);
			}

			return JsExpression.ObjectLiteral(
			           new JsObjectLiteralProperty("ntype", JsExpression.Number((int)ExpressionType.MemberAccess)),
			           new JsObjectLiteralProperty("type", scriptType),
			           new JsObjectLiteralProperty("expression", JsExpression.ObjectLiteral(
			               new JsObjectLiteralProperty("ntype", JsExpression.Number((int)ExpressionType.Constant)),
			               new JsObjectLiteralProperty("type", scriptType),
			               new JsObjectLiteralProperty("value", JsExpression.ObjectLiteral())
			           )),
			           new JsObjectLiteralProperty("member", JsExpression.ObjectLiteral(
			               new JsObjectLiteralProperty("typeDef", CreateTypeReferenceExpression(SpecialType.System_Object)),
			               new JsObjectLiteralProperty("name", JsExpression.String(name)),
			               new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Property)),
			               new JsObjectLiteralProperty("returnType", scriptType),
			               new JsObjectLiteralProperty("getter", JsExpression.ObjectLiteral(
			                   new JsObjectLiteralProperty("typeDef", CreateTypeReferenceExpression(SpecialType.System_Object)),
			                   new JsObjectLiteralProperty("name", JsExpression.String("get_" + name)),
			                   new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Method)),
			                   new JsObjectLiteralProperty("returnType", scriptType),
			                   new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral()),
			                   new JsObjectLiteralProperty("def", getterDefinition)
			               )),
			               new JsObjectLiteralProperty("setter", JsExpression.ObjectLiteral(
			                   new JsObjectLiteralProperty("typeDef", CreateTypeReferenceExpression(SpecialType.System_Object)),
			                   new JsObjectLiteralProperty("name", JsExpression.String("set_" + name)),
			                   new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Method)),
			                   new JsObjectLiteralProperty("returnType", CreateTypeReferenceExpression(SpecialType.System_Void)),
			                   new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(scriptType)),
			                   new JsObjectLiteralProperty("def", setterDefinition)
			               ))
			           ))
			       );
		}

		public JsExpression CloneValueType(JsExpression value, ITypeSymbol type, IRuntimeContext context) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(System_Script), "clone"), GetScriptType(type, TypeContext.GetScriptType, context), value);
		}

		public JsExpression InitializeField(JsExpression jsThis, string scriptName, ISymbol member, JsExpression initialValue, IRuntimeContext context) {
			var cia = _attributeStore.AttributesFor(member).GetAttribute<CustomInitializationAttribute>();
			if (cia != null) {
				if (string.IsNullOrEmpty(cia.Code))
					return null;
				var method = MetadataUtils.CreateDummyMethodForFieldInitialization(member, _compilation);
				// Can ignore errors because they are caught by the metadata importer
				var tokens = InlineCodeMethodCompiler.Tokenize(method, cia.Code, _ => {});
				initialValue = InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, jsThis, new[] { initialValue }, n => { var t = _compilation.GetTypeByMetadataName(n); return t != null ? InstantiateType(t, context) : JsExpression.Null; }, t => InstantiateTypeForUseAsTypeArgumentInInlineCode(t, context), _ => {});
			}

			return JsExpression.Assign(JsExpression.Member(jsThis, scriptName), initialValue);
		}
	}
}
