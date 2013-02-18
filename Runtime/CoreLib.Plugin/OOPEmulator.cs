using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace CoreLib.Plugin {
	public class OOPEmulator : IOOPEmulator {
		private class GenericSimplifier : RewriterVisitorBase<object> {
			private readonly ITypeDefinition _genericType;
			private readonly ReadOnlyCollection<string> _typeParameterNames;
			private readonly JsExpression _replaceWith;

			public GenericSimplifier(ITypeDefinition genericType, IEnumerable<string> typeParameterNames, JsExpression replaceWith) {
				_genericType = genericType;
				_typeParameterNames = typeParameterNames.AsReadOnly();
				_replaceWith = replaceWith;
			}

			public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
				if (expression.Arguments.Count != 2)
					return base.VisitInvocationExpression(expression, data);
				var access = expression.Method as JsMemberAccessExpression;
				if (access != null && access.MemberName == "makeGenericType") {
					var target = access.Target as JsTypeReferenceExpression;
					if (target != null && target.Type.FullName == "System.Script") {
						var genericType = expression.Arguments[0] as JsTypeReferenceExpression;
						if (genericType != null && genericType.Type.Equals(_genericType)) {
							var arr = expression.Arguments[1] as JsArrayLiteralExpression;
							if (arr != null && arr.Elements.Count == _typeParameterNames.Count && arr.Elements.All(e => e is JsIdentifierExpression)) {
								if (arr.Elements.Select(e => ((JsIdentifierExpression)e).Name).SequenceEqual(_typeParameterNames))
									return _replaceWith;
							}
						}
					}
				}
				return base.VisitInvocationExpression(expression, data);
			}

			public JsExpression Process(JsExpression expr) {
				return VisitExpression(expr, null);
			}

			public JsStatement Process(JsStatement stmt) {
				return VisitStatement(stmt, null);
			}
		}

		private const string Prototype = "prototype";
		private const string RegisterClass = "registerClass";
		private const string RegisterInterface = "registerInterface";
		private const string RegisterEnum = "registerEnum";
		private const string RegisterType = "registerType";
		private const string RegisterGenericClassInstance = "registerGenericClassInstance";
		private const string RegisterGenericInterfaceInstance = "registerGenericInterfaceInstance";
		private const string RegisterGenericClass = "registerGenericClass";
		private const string RegisterGenericInterface = "registerGenericInterface";
		private const string InstantiatedGenericTypeVariableName = "$type";

		private static Tuple<string, string> Split(string name) {
			int pos = name.LastIndexOf('.');
			if (pos == -1)
				return Tuple.Create("", name);
			else
				return Tuple.Create(name.Substring(0, pos), name.Substring(pos + 1));
		}

		internal static IEnumerable<T> OrderByNamespace<T>(IEnumerable<T> source, Func<T, string> nameSelector) {
			return    from s in source
			           let t = Split(nameSelector(s))
			       orderby t.Item1, t.Item2
			        select s;
		}

		private readonly ICompilation _compilation;
		private readonly JsTypeReferenceExpression _systemScript;
		private readonly IMetadataImporter _metadataImporter;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;

		public OOPEmulator(ICompilation compilation, IMetadataImporter metadataImporter, IRuntimeLibrary runtimeLibrary, INamer namer, IErrorReporter errorReporter) {
			_compilation = compilation;
			_systemScript = new JsTypeReferenceExpression(compilation.FindType(new FullTypeName("System.Script")).GetDefinition());

			_metadataImporter = metadataImporter;
			_runtimeLibrary = runtimeLibrary;
			_namer = namer;
			_errorReporter = errorReporter;
		}

		private JsExpression ResolveTypeParameter(ITypeParameter tp, ITypeDefinition currentType) {
			if (_metadataImporter.GetTypeSemantics(currentType).IgnoreGenericArguments) {
				_errorReporter.Message(Saltarelle.Compiler.Messages._7536, tp.Name, "type", currentType.FullName);
				return JsExpression.Null;
			}
			else {
				return JsExpression.Identifier(_namer.GetTypeParameterName(tp));
			}
		}

		private bool IsJsGeneric(JsClass c) {
			return c.CSharpTypeDefinition.TypeParameterCount > 0 && !_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).IgnoreGenericArguments;
		}

		private JsExpression RewriteMethod(JsMethod method) {
			return method.TypeParameterNames.Count == 0 ? method.Definition : JsExpression.FunctionDefinition(method.TypeParameterNames, new JsReturnStatement(method.Definition));
		}

		private JsExpression Compile(ResolveResult rr, ITypeDefinition currentType) {
			var result = new ExpressionCompiler(_compilation,
			                              _metadataImporter,
			                              _namer,
			                              _runtimeLibrary,
			                              _errorReporter,
			                              new Dictionary<IVariable, VariableData>(),
			                              new Dictionary<LambdaResolveResult, NestedFunctionData>(),
			                              _ => { throw new Exception("Cannot create temporary variables in OOP emulator"); },
			                              _ => { throw new Exception("Cannot compile nested functions in OOP emulator"); },
			                              null,
			                              new NestedFunctionContext(EmptyList<IVariable>.Instance),
			                              null,
			                              null,
			                              currentType
			                             ).Compile(rr, true);
			if (result.AdditionalStatements.Count > 0)
				throw new Exception("Did not expect extra statements to be generated by the compiler");
			return result.Expression;
		}

		private JsExpression ConstructAttribute(IAttribute attr, ITypeDefinition currentType) {
			JsExpression result = CompileConstructorInvocation(attr.Constructor, currentType, attr.PositionalArguments);
			if (attr.NamedArguments.Count > 0) {
				var initializers = new List<JsExpression>();
				foreach (var a in attr.NamedArguments) {
					if (a.Key is IProperty) {
						var sem = _metadataImporter.GetPropertySemantics((IProperty)a.Key);
						switch (sem.Type) {
							case PropertyScriptSemantics.ImplType.Field:
								initializers.Add(JsExpression.Assign(JsExpression.Member(JsExpression.Identifier("$a"), sem.FieldName), Compile(a.Value, currentType)));
								break;
							case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
								switch (sem.SetMethod.Type) {
									case MethodScriptSemantics.ImplType.NormalMethod:
										initializers.Add(JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier("$a"), sem.SetMethod.Name), Compile(a.Value, currentType)));
										break;
									case MethodScriptSemantics.ImplType.InlineCode: {
										var method = ((IProperty)a.Key).Setter;
										var tokens = InlineCodeMethodCompiler.Tokenize(method, sem.SetMethod.LiteralCode, s => _errorReporter.InternalError("Error in inline code during attribute generation: " + s));
										if (tokens != null) {
											initializers.Add(InlineCodeMethodCompiler.CompileInlineCodeMethodInvocation(method,
											                                                                            tokens,
											                                                                            JsExpression.Identifier("$a"),
											                                                                            new[] { Compile(a.Value, currentType) },
											                                                                            n => _runtimeLibrary.InstantiateType(ReflectionHelper.ParseReflectionName(n).Resolve(_compilation), t => { throw new InvalidOperationException("Cannot have type parameters in attribute constructor"); }),
											                                                                            t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, tp => { throw new InvalidOperationException("Cannot have type parameters in attribute constructor"); }),
											                                                                            s => _errorReporter.InternalError("Error in inline code during attribute generation: " + s)));
										}
										break;
									}
									default:
										throw new Exception("Invalid implementation type for set method of property: " + a.Key.Name);
								}
								break;
							}
							case PropertyScriptSemantics.ImplType.NotUsableFromScript:
								break;
							default:
								throw new Exception("Invalid implementation type for attribute property: " + sem.Type);
						}
					}
					else if (a.Key is IField) {
						var sem = _metadataImporter.GetFieldSemantics((IField)a.Key);
						switch (sem.Type) {
							case FieldScriptSemantics.ImplType.Field:
								initializers.Add(JsExpression.Assign(JsExpression.Member(JsExpression.Identifier("$a"), sem.Name), Compile(a.Value, currentType)));
								break;
							case FieldScriptSemantics.ImplType.NotUsableFromScript:
								break;
							default:
								throw new Exception("Invalid implementation type for attribute field: " + sem.Type);
						}
					}
					else
						throw new Exception("Invalid attribute named argument member: " + a.Key);
				}

				if (initializers.Count > 0) {
					result = JsExpression.Invocation(
					             JsExpression.FunctionDefinition(
					             new string[0],
					             new JsBlockStatement(
					                 new JsStatement[] { new JsVariableDeclarationStatement("$a", result) }
					                 .Concat(initializers.Select(x => new JsExpressionStatement(x)))
					                 .Concat(new[] { new JsReturnStatement(JsExpression.Identifier("$a")) }))));
				}
			}
			return result;
		}

		private JsExpression GetMetadataDescriptor(ITypeDefinition type) {
			var properties = new List<JsObjectLiteralProperty>();
			var scriptableAttributes = type.Attributes.Where(a => _metadataImporter.GetTypeSemantics(a.AttributeType.GetDefinition()).Type == TypeScriptSemantics.ImplType.NormalType).ToList();
			if (scriptableAttributes.Count != 0) {
				properties.Add(new JsObjectLiteralProperty("attr", JsExpression.ArrayLiteral(scriptableAttributes.Select(a => ConstructAttribute(a, type)))));
			}
			if (type.Kind == TypeKind.Class) {
				var aua = AttributeReader.ReadAttribute<AttributeUsageAttribute>(type);
				if (aua != null) {
					if (!aua.Inherited)
						properties.Add(new JsObjectLiteralProperty("attrNoInherit", JsExpression.True));
					if (aua.AllowMultiple)
						properties.Add(new JsObjectLiteralProperty("attrAllowMultiple", JsExpression.True));
				}
			}
			if (type.Kind == TypeKind.Enum && AttributeReader.HasAttribute<FlagsAttribute>(type))
				properties.Add(new JsObjectLiteralProperty("enumFlags", JsExpression.True));

			return properties.Count > 0 ? JsExpression.ObjectLiteral(properties) : null;
		}

		private JsExpression CreateRegisterClassCall(ITypeDefinition type, string name, JsExpression ctor, JsExpression baseClass, IList<JsExpression> interfaces) {
			if (baseClass is JsTypeReferenceExpression && ((JsTypeReferenceExpression)baseClass).Type.IsKnownType(KnownTypeCode.Object))
				baseClass = null;

			var args = new List<JsExpression> { GetRoot(type), JsExpression.String(name), ctor };
			var metadata = GetMetadataDescriptor(type);
			if (baseClass != null || interfaces.Count > 0 || metadata != null)
				args.Add(baseClass ?? JsExpression.Null);
			if (interfaces.Count > 0 || metadata != null)
				args.Add(JsExpression.ArrayLiteral(interfaces));
			if (metadata != null)
				args.Add(metadata);

			return JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterClass), args);
		}

		private JsExpression CreateRegisterInterfaceCall(ITypeDefinition type, string name, JsExpression ctor, IList<JsExpression> interfaces) {
			var args = new List<JsExpression> { GetRoot(type), JsExpression.String(name), ctor };
			var metadata = GetMetadataDescriptor(type);
			if (interfaces.Count > 0 || metadata != null)
				args.Add(JsExpression.ArrayLiteral(interfaces));
			if (metadata != null)
				args.Add(metadata);
			return JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterInterface), args);
		}

		private JsExpression CreateRegisterEnumCall(ITypeDefinition type, string name, JsExpression ctor) {
			var args = new List<JsExpression> { GetRoot(type), JsExpression.String(name), ctor };
			var metadata = GetMetadataDescriptor(type);
			if (metadata != null)
				args.Add(metadata);
			return JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterEnum), args);
		}

		private JsExpression CompileConstructorInvocation(IMethod constructor, ITypeDefinition currentType, IList<ResolveResult> arguments) {
			return Compile(new CSharpInvocationResolveResult(new TypeResolveResult(constructor.DeclaringType), constructor, arguments), currentType);
		}

		private IEnumerable<JsExpression> GetImplementedInterfaces(ITypeDefinition type) {
			return type.GetAllBaseTypes().Where(t => t.Kind == TypeKind.Interface && !t.Equals(type) && MetadataUtils.DoesTypeObeyTypeSystem(t.GetDefinition())).Select(t => _runtimeLibrary.InstantiateType(t, tp => ResolveTypeParameter(tp, type)));
		}

		private JsExpression GetBaseClass(ITypeDefinition type) {
			var csBase = type.DirectBaseTypes.SingleOrDefault(b => b.Kind == TypeKind.Class);
			return csBase != null ? _runtimeLibrary.InstantiateType(csBase, tp => ResolveTypeParameter(tp, type)) : JsExpression.Null;
		}

		private void AddClassMembers(JsClass c, JsExpression typeRef, List<JsStatement> stmts) {
			if (c.InstanceMethods.Count > 0) {
				stmts.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, Prototype), JsExpression.ObjectLiteral(c.InstanceMethods.Select(m => new JsObjectLiteralProperty(m.Name, m.Definition != null ? RewriteMethod(m) : JsExpression.Null))))));
			}

			if (c.NamedConstructors.Count > 0) {
				stmts.AddRange(c.NamedConstructors.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, m.Name), m.Definition))));
				stmts.Add(new JsExpressionStatement(c.NamedConstructors.Reverse().Aggregate((JsExpression)JsExpression.Member(typeRef, Prototype), (right, ctor) => JsExpression.Assign(JsExpression.Member(JsExpression.Member(typeRef, ctor.Name), Prototype), right))));	// This generates a statement like {C}.ctor1.prototype = {C}.ctor2.prototype = {C}.prototoype
			}

			var defaultConstructor = Saltarelle.Compiler.Utils.SelfParameterize(c.CSharpTypeDefinition).GetConstructors().SingleOrDefault(x => x.Parameters.Count == 0 && x.IsPublic);
			if (defaultConstructor != null) {
				var sem = _metadataImporter.GetConstructorSemantics(defaultConstructor);
				if (sem.Type != ConstructorScriptSemantics.ImplType.UnnamedConstructor && sem.Type != ConstructorScriptSemantics.ImplType.NotUsableFromScript) {
					JsExpression createInstance = CompileConstructorInvocation(defaultConstructor, c.CSharpTypeDefinition, EmptyList<ResolveResult>.Instance);
					if (createInstance != null) {
						stmts.Add(new JsExpressionStatement(
						              JsExpression.Assign(
						                  JsExpression.Member(typeRef, "createInstance"),
						                      JsExpression.FunctionDefinition(new string[0],
						                          new JsReturnStatement(createInstance)))));
					}
				}
			}

			stmts.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, m.Name), RewriteMethod(m)))));

			if (IsJsGeneric(c)) {
				var typeParameters = JsExpression.ArrayLiteral(c.CSharpTypeDefinition.TypeParameters.Select(tp => JsExpression.Identifier(_namer.GetTypeParameterName(tp))));
				var implementedInterfaces = GetImplementedInterfaces(c.CSharpTypeDefinition);
				if (c.CSharpTypeDefinition.Kind == TypeKind.Interface) {
					stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterGenericInterfaceInstance),
					                                                            typeRef,
					                                                            new JsTypeReferenceExpression(c.CSharpTypeDefinition),
					                                                            typeParameters,
					                                                            JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.ArrayLiteral(implementedInterfaces))))));
				}
				else {
					stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterGenericClassInstance),
					                                                            typeRef,
					                                                            new JsTypeReferenceExpression(c.CSharpTypeDefinition),
					                                                            typeParameters,
					                                                            JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(GetBaseClass(c.CSharpTypeDefinition))),
					                                                            JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.ArrayLiteral(implementedInterfaces))))));
				}
			}
		}

		private JsStatement GenerateResourcesClass(JsClass c) {
			var fields = c.StaticInitStatements
			              .OfType<JsExpressionStatement>()
			              .Select(s => s.Expression)
			              .OfType<JsBinaryExpression>()
			              .Where(expr => expr.NodeType == ExpressionNodeType.Assign && expr.Left is JsMemberAccessExpression)
			              .Select(expr => new { Name = ((JsMemberAccessExpression)expr.Left).MemberName, Value = expr.Right });

			return new JsVariableDeclarationStatement(_namer.GetTypeVariableName(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name), JsExpression.ObjectLiteral(fields.Select(f => new JsObjectLiteralProperty(f.Name, f.Value))));
		}

		private IEnumerable<JsType> TopologicalSortTypesByInheritance(IEnumerable<JsType> types) {
			var backref = types.ToDictionary(c => c.CSharpTypeDefinition);
			var edges = from s in backref.Keys from t in s.DirectBaseTypes.Select(x => x.GetDefinition()).Intersect(backref.Keys) select Tuple.Create(s, t);
			return TopologicalSorter.TopologicalSort(backref.Keys, edges).Select(t => backref[t]);
		}

		private JsExpression MakeNestedMemberAccess(string full) {
			var parts = full.Split('.');
			JsExpression result = JsExpression.Identifier(parts[0]);
			for (int i = 1; i < parts.Length; i++) {
				result = JsExpression.Member(result, parts[i]);
			}
			return result;
		}

		private JsExpression GetRoot(ITypeDefinition type, bool exportNonPublic = false) {
			if (!exportNonPublic && !type.IsExternallyVisible())
				return JsExpression.Null;
			else
				return JsExpression.Identifier(string.IsNullOrEmpty(MetadataUtils.GetModuleName(type)) ? "global" : "exports");
		}

		public IList<JsStatement> Process(IEnumerable<JsType> types, IMethod entryPoint) {
			var result = new List<JsStatement>();

			var orderedTypes = OrderByNamespace(types, t => _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name).ToList();
			foreach (var t in orderedTypes) {
				try {
					string name = _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name;
					bool isGlobal = string.IsNullOrEmpty(name);
					bool isMixin  = MetadataUtils.IsMixin(t.CSharpTypeDefinition);

					result.Add(new JsComment("//////////////////////////////////////////////////////////////////////////////" + Environment.NewLine + " " + t.CSharpTypeDefinition.FullName));

					var typeRef = JsExpression.Identifier(_namer.GetTypeVariableName(name));
					if (t is JsClass) {
						var c = (JsClass)t;
						if (isGlobal) {
							result.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Binary(ExpressionNodeType.Assign, JsExpression.Member(GetRoot(t.CSharpTypeDefinition, exportNonPublic: true), m.Name), m.Definition))));
						}
						else if (isMixin) {
							result.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Assign(MakeNestedMemberAccess(name + "." + m.Name), m.Definition))));
						}
						else if (MetadataUtils.IsResources(c.CSharpTypeDefinition)) {
							result.Add(GenerateResourcesClass(c));
						}
						else {
							var unnamedCtor = c.UnnamedConstructor ?? JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);

							if (IsJsGeneric(c)) {
								var typeParameterNames = c.CSharpTypeDefinition.TypeParameters.Select(tp => _namer.GetTypeParameterName(tp)).ToList();
								var stmts = new List<JsStatement> { new JsVariableDeclarationStatement(InstantiatedGenericTypeVariableName, unnamedCtor) };
								AddClassMembers(c, JsExpression.Identifier(InstantiatedGenericTypeVariableName), stmts);
								stmts.AddRange(c.StaticInitStatements);
								stmts.Add(new JsReturnStatement(JsExpression.Identifier(InstantiatedGenericTypeVariableName)));
								var replacer = new GenericSimplifier(c.CSharpTypeDefinition, typeParameterNames, JsExpression.Identifier(InstantiatedGenericTypeVariableName));
								for (int i = 0; i < stmts.Count; i++)
									stmts[i] = replacer.Process(stmts[i]);
								result.Add(new JsVariableDeclarationStatement(typeRef.Name, JsExpression.FunctionDefinition(typeParameterNames, new JsBlockStatement(stmts))));
								var args = new List<JsExpression> { GetRoot(t.CSharpTypeDefinition), JsExpression.String(name), typeRef, JsExpression.Number(c.CSharpTypeDefinition.TypeParameterCount) };
								var metadata = GetMetadataDescriptor(t.CSharpTypeDefinition);
								if (metadata != null)
									args.Add(metadata);
								result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemScript, c.CSharpTypeDefinition.Kind == TypeKind.Interface ? RegisterGenericInterface : RegisterGenericClass), args)));
							}
							else {
								result.Add(new JsVariableDeclarationStatement(typeRef.Name, unnamedCtor));
								AddClassMembers(c, typeRef, result);
							}
						}
					}
					else if (t is JsEnum) {
						var e = (JsEnum)t;
						result.Add(new JsVariableDeclarationStatement(typeRef.Name, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)));
						var values = new List<JsObjectLiteralProperty>();
						foreach (var v in e.CSharpTypeDefinition.Fields) {
							if (v.ConstantValue != null) {
								var sem = _metadataImporter.GetFieldSemantics(v);
								if (sem.Type == FieldScriptSemantics.ImplType.Field) {
									values.Add(new JsObjectLiteralProperty(sem.Name, JsExpression.Number(Convert.ToDouble(v.ConstantValue))));
								}
								else if (sem.Type == FieldScriptSemantics.ImplType.Constant && sem.Name != null) {
									values.Add(new JsObjectLiteralProperty(sem.Name, sem.Value is string ? JsExpression.String((string)sem.Value) : JsExpression.Number(Convert.ToDouble(sem.Value))));
								}
							}
							else {
								_errorReporter.Region = v.Region;
								_errorReporter.InternalError("Enum field " + v.FullName + " is not constant.");
							}
							
						}
						result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, Prototype), JsExpression.ObjectLiteral(values))));
					}
				}
				catch (Exception ex) {
					_errorReporter.Region = t.CSharpTypeDefinition.Region;
					_errorReporter.InternalError(ex, "Error formatting type " + t.CSharpTypeDefinition.FullName);
				}
			}

			var typesToRegister = orderedTypes
			                            .Where(c =>    !(c is JsClass && IsJsGeneric((JsClass)c))
			                                        && !string.IsNullOrEmpty(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name)
			                                        && (!MetadataUtils.IsResources(c.CSharpTypeDefinition) || c.CSharpTypeDefinition.IsExternallyVisible())	// Resources classes are only exported if they are public.
			                                        && !MetadataUtils.IsMixin(c.CSharpTypeDefinition))
			                            .ToList();

			result.AddRange(TopologicalSortTypesByInheritance(typesToRegister)
			                .Select(c => {
			                                 try {
			                                     string name = _metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name;
			                                     if (c.CSharpTypeDefinition.Kind == TypeKind.Enum) {
			                                         return CreateRegisterEnumCall(c.CSharpTypeDefinition, name, JsExpression.Identifier(_namer.GetTypeVariableName(name)));
			                                     }
			                                     if (MetadataUtils.IsResources(c.CSharpTypeDefinition)) {
			                                         return JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterType), GetRoot(c.CSharpTypeDefinition), JsExpression.String(name), JsExpression.Identifier(_namer.GetTypeVariableName(name)));
			                                     }
			                                     if (c.CSharpTypeDefinition.Kind == TypeKind.Interface) {
			                                         return CreateRegisterInterfaceCall(c.CSharpTypeDefinition,
			                                                                            name,
			                                                                            JsExpression.Identifier(_namer.GetTypeVariableName(name)),
			                                                                            GetImplementedInterfaces(c.CSharpTypeDefinition.GetDefinition()).ToList());
			                                     }
			                                     else {
			                                         return CreateRegisterClassCall(c.CSharpTypeDefinition,
			                                                                        name, 
			                                                                        JsExpression.Identifier(_namer.GetTypeVariableName(name)),
			                                                                        GetBaseClass(c.CSharpTypeDefinition),
			                                                                        GetImplementedInterfaces(c.CSharpTypeDefinition).ToList());
			                                     }
			                                 }
			                                 catch (Exception ex) {
			                                     _errorReporter.Region = c.CSharpTypeDefinition.Region;
			                                     _errorReporter.InternalError(ex, "Error formatting type " + c.CSharpTypeDefinition.FullName);
			                                     return JsExpression.Number(0);
			                                 }
			                             })
			                .Select(expr => new JsExpressionStatement(expr)));
			result.AddRange(GetStaticInitializationOrder(orderedTypes.OfType<JsClass>(), 1)
			                .Where(c => !IsJsGeneric(c) && !MetadataUtils.IsResources(c.CSharpTypeDefinition))
			                .SelectMany(c => c.StaticInitStatements));

			if (entryPoint != null) {
				if (entryPoint.Parameters.Count > 0) {
					_errorReporter.Region = entryPoint.Region;
					_errorReporter.Message(Messages._7800, entryPoint.FullName);
				}
				else {
					var sem = _metadataImporter.GetMethodSemantics(entryPoint);
					if (sem.Type != MethodScriptSemantics.ImplType.NormalMethod) {
						_errorReporter.Region = entryPoint.Region;
						_errorReporter.Message(Messages._7801, entryPoint.FullName);
					}
					else {
						result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(new JsTypeReferenceExpression(entryPoint.DeclaringTypeDefinition), sem.Name))));
					}
				}
			}

			return result;
		}

		private HashSet<ITypeDefinition> GetDependencies(JsClass c, int pass) {
			// Consider the following reference locations:
			// Pass 1: static init statements, static methods, instance methods, constructors
			// Pass 2: static init statements, static methods
			// Pass 3: static init statements only

			var result = new HashSet<ITypeDefinition>();
			switch (pass) {
				case 1:
					foreach (var r in c.InstanceMethods.Where(m => m.Definition != null).SelectMany(m => TypeReferenceFinder.Analyze(m.Definition)))
						result.Add(r);
					foreach (var r in c.NamedConstructors.Where(m => m.Definition != null).SelectMany(m => TypeReferenceFinder.Analyze(m.Definition)))
						result.Add(r);
					if (c.UnnamedConstructor != null) {
						foreach (var r in TypeReferenceFinder.Analyze(c.UnnamedConstructor))
							result.Add(r);
					}
					goto case 2;

				case 2:
					foreach (var r in c.StaticMethods.Where(m => m.Definition != null).SelectMany(m => TypeReferenceFinder.Analyze(m.Definition)))
						result.Add(r);
					goto case 3;

				case 3:
					foreach (var r in TypeReferenceFinder.Analyze(c.StaticInitStatements))
						result.Add(r);
					break;

				default:
					throw new ArgumentException("pass");
			}
			return result;
		}

		private IEnumerable<JsClass> GetStaticInitializationOrder(IEnumerable<JsClass> types, int pass) {
			if (pass > 3)
				return types;	// If we can't find a non-circular order after 3 passes, just use some random order.

			// We run the algorithm in 3 passes, each considering less types of references than the previous one.
			var dict = types.ToDictionary(t => t.CSharpTypeDefinition, t => new { deps = GetDependencies(t, pass), backref = t });
			var edges = from s in dict from t in s.Value.deps where dict.ContainsKey(t) select Tuple.Create(s.Key, t);

			var result = new List<JsClass>();
			foreach (var group in TopologicalSorter.FindAndTopologicallySortStronglyConnectedComponents(dict.Keys.ToList(), edges)) {
				var backrefed = group.Select(t => dict[t].backref);
				result.AddRange(group.Count > 1 ? GetStaticInitializationOrder(backrefed.ToList(), pass + 1) : backrefed);
			}

			return result;
		}
	}
}
