using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Plugin {
	public class RuntimeLibrary : IRuntimeLibrary {
		private readonly ITypeReference _systemScript = ReflectionHelper.ParseReflectionName("System.Script");

		private readonly IMetadataImporter _metadataImporter;
		private readonly IErrorReporter _errorReporter;
		private readonly INamer _namer;
		private readonly ICompilation _compilation;
		private readonly bool _omitDowncasts;
		private readonly bool _omitNullableChecks;

		public RuntimeLibrary(IMetadataImporter metadataImporter, IErrorReporter errorReporter, INamer namer, ICompilation compilation) {
			_metadataImporter = metadataImporter;
			_errorReporter = errorReporter;
			_namer = namer;
			_compilation = compilation;
			_omitDowncasts = MetadataUtils.OmitDowncasts(compilation);
			_omitNullableChecks = MetadataUtils.OmitNullableChecks(compilation);
		}

		private JsTypeReferenceExpression CreateTypeReferenceExpression(ITypeReference tr) {
			return new JsTypeReferenceExpression(tr.Resolve(_compilation).GetDefinition());
		}

		public JsExpression GetScriptType(IType type, TypeContext context) {
			if (type.Kind == TypeKind.Delegate) {
				return CreateTypeReferenceExpression(KnownTypeReference.Delegate);
			}
			else if (type.TypeParameterCount > 0 && !(type is ParameterizedType) && (context == TypeContext.TypeOf || context == TypeContext.InlineCode)) {
				// This handles open generic types ( typeof(C<,>) )
				return CreateTypeReferenceExpression(type.GetDefinition().ToTypeReference());
			}
			else if (type.Kind == TypeKind.Enum && (context == TypeContext.CastTarget || context == TypeContext.InvokeConstructor)) {
				var def = type.GetDefinition();
				return CreateTypeReferenceExpression(def.EnumUnderlyingType.ToTypeReference());
			}
			else if (type.Kind == TypeKind.Array) {
				return CreateTypeReferenceExpression(KnownTypeReference.Array);
			}
			else if (type is ITypeParameter) {
				return JsExpression.Identifier(_namer.GetTypeParameterName((ITypeParameter)type));
			}
			else if (type is ParameterizedType) {
				var pt = (ParameterizedType)type;
				var def = pt.GetDefinition();
				var sem = _metadataImporter.GetTypeSemantics(def);
				if (sem.Type == TypeScriptSemantics.ImplType.NormalType && !sem.IgnoreGenericArguments)
					return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "makeGenericType"), CreateTypeReferenceExpression(type.GetDefinition().ToTypeReference()), JsExpression.ArrayLiteral(pt.TypeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument))));
				else
					return GetScriptType(def, context);
			}
			else if (type is ITypeDefinition) {
				var td = (ITypeDefinition)type;
				if (MetadataUtils.IsSerializable(td) && (context == TypeContext.CastTarget || context == TypeContext.Inheritance)) {
					return null;
				}
				else if (context != TypeContext.UseStaticMember && context != TypeContext.InlineCode && context != TypeContext.InvokeConstructor && !MetadataUtils.DoesTypeObeyTypeSystem(td)) {
					if (context == TypeContext.CastTarget || context == TypeContext.Inheritance)
						return null;
					else
						return CreateTypeReferenceExpression(KnownTypeReference.Object);
				}
				else {
					var sem = _metadataImporter.GetTypeSemantics(td);
					var jsref = CreateTypeReferenceExpression(td.ToTypeReference());
					if (td.TypeParameterCount > 0 && !sem.IgnoreGenericArguments) {
						// This handles the case of resolving the current type, eg. to access a static member.
						return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "makeGenericType"), CreateTypeReferenceExpression(type.GetDefinition().ToTypeReference()), JsExpression.ArrayLiteral(td.TypeParameters.Select(a => GetScriptType(a, TypeContext.GenericArgument))));
					}
					else {
						return jsref;
					}
				}
			}
			else if (type.Kind == TypeKind.Anonymous || type.Kind == TypeKind.Null || type.Kind == TypeKind.Dynamic) {
				return CreateTypeReferenceExpression(KnownTypeReference.Object);
			}
			else {
				throw new InvalidOperationException("Could not determine the script type for " + type.ToString() + ", context " + context);
			}
		}

		private bool IsSystemObjectReference(JsExpression expr) {
			return expr is JsTypeReferenceExpression && ((JsTypeReferenceExpression)expr).Type.IsKnownType(KnownTypeCode.Object);
		}

		private JsExpression GetCastTarget(IType sourceType, IType targetType) {
			var ss = GetScriptType(sourceType, TypeContext.CastTarget);
			var st = GetScriptType(targetType, TypeContext.CastTarget);
			if (ss == null || st == null) {
				return null;	// Either the source or the target is not a real type.
			}
			else if (ss is JsTypeReferenceExpression && st is JsTypeReferenceExpression) {
				var ts = ((JsTypeReferenceExpression)ss).Type;
				var tt = ((JsTypeReferenceExpression)st).Type;
				if (_metadataImporter.GetTypeSemantics(ts).Name == _metadataImporter.GetTypeSemantics(tt).Name && Equals(ts.ParentAssembly, tt.ParentAssembly))
					return null;	// The types are the same in script, so no runtimeConversion is required.
			}

			return st;
		}

		public JsExpression TypeIs(JsExpression expression, IType sourceType, IType targetType) {
			var jsTarget = GetCastTarget(sourceType, targetType);
			if (jsTarget == null || IsSystemObjectReference(jsTarget))
				return ReferenceNotEquals(expression, JsExpression.Null);
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "isInstanceOfType"), expression, jsTarget);
		}

		public JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType) {
			var jsTarget = GetCastTarget(sourceType, targetType);
			if (jsTarget == null || IsSystemObjectReference(jsTarget))
				return expression;
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "safeCast"), expression, jsTarget);
		}

		public JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType) {
			if (_omitDowncasts)
				return expression;

			if (sourceType.Kind == TypeKind.Dynamic && targetType.IsKnownType(KnownTypeCode.Boolean))
				return JsExpression.LogicalNot(JsExpression.LogicalNot(expression));
			var jsTarget = GetCastTarget(sourceType, targetType);
			if (jsTarget == null || IsSystemObjectReference(jsTarget))
				return expression;
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "cast"), expression, jsTarget);
		}

		public JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType) {
			if (sourceType.IsKnownType(KnownTypeCode.Char))
				_errorReporter.Message(7700);
			return expression;
		}

		public JsExpression ReferenceEquals(JsExpression a, JsExpression b) {
			if (a.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "isNullOrUndefined"), b);
			else if (b.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "isNullOrUndefined"), a);
			else if (a.NodeType == ExpressionNodeType.String || b.NodeType == ExpressionNodeType.String)
				return JsExpression.Same(a, b);
			else
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "referenceEquals"), a, b);
		}

		public JsExpression ReferenceNotEquals(JsExpression a, JsExpression b) {
			if (a.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "isValue"), b);
			else if (b.NodeType == ExpressionNodeType.Null)
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "isValue"), a);
			else if (a.NodeType == ExpressionNodeType.String || b.NodeType == ExpressionNodeType.String)
				return JsExpression.NotSame(a, b);
			else
				return JsExpression.LogicalNot(JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "referenceEquals"), a, b));
		}

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments) {
			return JsExpression.Invocation(method, typeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument)));
		}

		public JsExpression MakeException(JsExpression operand) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.Exception), "wrap"), operand);
		}

		public JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.Int32), "div"), numerator, denominator);
		}

		public JsExpression FloatToInt(JsExpression operand) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.Int32), "trunc"), operand);
		}

		public JsExpression Coalesce(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "coalesce"), a, b);
		}

		public JsExpression Lift(JsExpression expression) {
			if (expression is JsInvocationExpression) {
				var ie = (JsInvocationExpression)expression;
				if (ie.Method is JsMemberAccessExpression) {
					var mae = (JsMemberAccessExpression)ie.Method;
					if (mae.Target is JsTypeReferenceExpression) {
						var t = ((JsTypeReferenceExpression)mae.Target).Type;
						bool isIntegerType = t.IsKnownType(KnownTypeCode.Byte) || t.IsKnownType(KnownTypeCode.SByte) || t.IsKnownType(KnownTypeCode.Int16) || t.IsKnownType(KnownTypeCode.UInt16) || t.IsKnownType(KnownTypeCode.Char) || t.IsKnownType(KnownTypeCode.Int32) || t.IsKnownType(KnownTypeCode.UInt32) || t.IsKnownType(KnownTypeCode.Int64) || t.IsKnownType(KnownTypeCode.UInt64);
						if (isIntegerType) {
							if (mae.MemberName == "div" || mae.MemberName == "trunc")
								return expression;
						}
					}
				}
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
							return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.NullableOfT), methodName), ((JsUnaryExpression)expression).Operand);
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
							return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.NullableOfT), methodName), ((JsBinaryExpression)expression).Left, ((JsBinaryExpression)expression).Right);
						break;
				}
			}

			throw new ArgumentException("Cannot lift expression " + OutputFormatter.Format(expression, true));
		}

		public JsExpression FromNullable(JsExpression expression) {
			if (_omitNullableChecks)
				return expression;

			if (expression.NodeType == ExpressionNodeType.LogicalNot)
				return expression;	// This is a little hacky. The problem we want to solve is that 'bool b = myDynamic' should compile to !!myDynamic, but the actual call is unbox(convert(myDynamic, bool)), where convert() will return the !!. Anyway, in JS, the !expression will never be null anyway.

			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.NullableOfT), "unbox"), expression);
		}

		public JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.NullableOfT), "and"), a, b);
		}

		public JsExpression LiftedBooleanOr(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(KnownTypeReference.NullableOfT), "or"), a, b);
		}

		public JsExpression Bind(JsExpression function, JsExpression target) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "mkdel"), target, function);
		}

		public JsExpression BindFirstParameterToThis(JsExpression function) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "thisFix"), function);
		}

		public JsExpression Default(IType type) {
			if (type.IsReferenceType == true || type.Kind == TypeKind.Dynamic) {
				return JsExpression.Null;
			}
			else if (type.IsReferenceType == null) {
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "getDefaultValue"), GetScriptType(type, TypeContext.GetDefaultValue));
			}
			else {
				switch (type.GetDefinition().KnownTypeCode) {
					case KnownTypeCode.Boolean:
						return JsExpression.False;
					case KnownTypeCode.NullableOfT:
						return JsExpression.Null;
					case KnownTypeCode.DateTime:
						return JsExpression.New(CreateTypeReferenceExpression(KnownTypeReference.DateTime), JsExpression.Number(0));
					case KnownTypeCode.Byte:
					case KnownTypeCode.SByte:
					case KnownTypeCode.Char:
					case KnownTypeCode.Int16:
					case KnownTypeCode.UInt16:
					case KnownTypeCode.Int32:
					case KnownTypeCode.UInt32:
					case KnownTypeCode.Int64:
					case KnownTypeCode.UInt64:
					case KnownTypeCode.Decimal:
					case KnownTypeCode.Single:
					case KnownTypeCode.Double:
						return JsExpression.Number(0);
					default:
						_errorReporter.InternalError("Cannot use default value for the type " + type);
						return JsExpression.Null;
				}
			}
		}

		public JsExpression CreateArray(IType elementType, IEnumerable<JsExpression> size) {
			var sizeList = (size is IList<JsExpression>) ? (IList<JsExpression>)size : size.ToList();
			if (sizeList.Count == 1) {
				return JsExpression.New(CreateTypeReferenceExpression(KnownTypeReference.Array), sizeList);
			}
			else {
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "multidimArray"), new[] { Default(elementType) }.Concat(sizeList));
			}
		}

		public JsExpression CloneDelegate(JsExpression source, IType sourceType, IType targetType) {
			if (Equals(sourceType, targetType)) {
				// The user does something like "D d1 = F(); var d2 = new D(d1)". Assume he does this for a reason and create a clone of the delegate.
				return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "delegateClone"), source);
			}
			else {
				return source;	// The clone is just to convert the delegate to a different type. The risk of anyone comparing the references is small, so just return the original as delegates are immutable anyway.
			}
		}

		public JsExpression CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			JsExpression method = JsExpression.Member(JsExpression.Member(GetScriptType(baseType, TypeContext.BindBaseCall), "prototype"), methodName);
			
			if (typeArguments != null && typeArguments.Count > 0)
				method = InstantiateGenericMethod(method, typeArguments);

			return JsExpression.Invocation(JsExpression.Member(method, "call"), thisAndArguments);
		}

		public JsExpression BindBaseCall(IType baseType, string methodName, IList<IType> typeArguments, JsExpression @this) {
			JsExpression method = JsExpression.Member(JsExpression.Member(GetScriptType(baseType, TypeContext.BindBaseCall), "prototype"), methodName);
			
			if (typeArguments != null && typeArguments.Count > 0)
				method = InstantiateGenericMethod(method, typeArguments);

			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "mkdel"), @this, method);
		}

		public JsExpression MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose) {
			return JsExpression.New(CreateTypeReferenceExpression(ReflectionHelper.ParseReflectionName("System.Collections.Generic.IteratorBlockEnumerator`1")), moveNext, getCurrent, dispose ?? (JsExpression)JsExpression.Null, JsExpression.This);
		}

		public JsExpression MakeEnumerable(IType yieldType, JsExpression getEnumerator) {
			return JsExpression.New(CreateTypeReferenceExpression(ReflectionHelper.ParseReflectionName("System.Collections.Generic.IteratorBlockEnumerable`1")), getEnumerator, JsExpression.This);
		}

		public JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "arrayGet"), new[] { array }.Concat(indices));
		}

		public JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value) {
			return JsExpression.Invocation(JsExpression.Member(CreateTypeReferenceExpression(_systemScript), "arraySet"), new[] { array }.Concat(indices).Concat(new[] { value }));
		}

		public JsExpression CreateTaskCompletionSource(IType taskGenericArgument) {
			return JsExpression.New(CreateTypeReferenceExpression(ReflectionHelper.ParseReflectionName("System.Threading.Tasks.TaskCompletionSource`1")));
		}

		public JsExpression SetAsyncResult(JsExpression taskCompletionSource, JsExpression value) {
			return JsExpression.Invocation(JsExpression.Member(taskCompletionSource, "setResult"), value ?? JsExpression.Null);
		}

		public JsExpression SetAsyncException(JsExpression taskCompletionSource, JsExpression exception) {
			return JsExpression.Invocation(JsExpression.Member(taskCompletionSource, "setException"), MakeException(exception));
		}

		public JsExpression GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource) {
			return JsExpression.Member(taskCompletionSource, "task");
		}
	}
}
