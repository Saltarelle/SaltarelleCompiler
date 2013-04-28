using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ICSharpCode.NRefactory;
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
		private class ReflectionRuntimeContext : IRuntimeContext {
			private readonly bool _isGenericSpecialization;
			private readonly JsExpression _systemObject;
			private readonly INamer _namer;

			public ReflectionRuntimeContext(bool isGenericSpecialization, JsExpression systemObject, INamer namer) {
				_isGenericSpecialization = isGenericSpecialization;
				_systemObject = systemObject;
				_namer = namer;
			}

			public JsExpression ResolveTypeParameter(ITypeParameter tp) {
				if (_isGenericSpecialization && tp.OwnerType == EntityType.TypeDefinition)
					return JsExpression.Identifier(_namer.GetTypeParameterName(tp));
				else
					return _systemObject;
			}

			public JsExpression EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
				throw new NotSupportedException();
			}
		}

		private class DefaultRuntimeContext : IRuntimeContext {
			private readonly ITypeDefinition _currentType;
			private readonly IMetadataImporter _metadataImporter;
			private readonly IErrorReporter _errorReporter;
			private readonly INamer _namer;

			public DefaultRuntimeContext(ITypeDefinition currentType, IMetadataImporter metadataImporter, IErrorReporter errorReporter, INamer namer) {
				_currentType = currentType;
				_metadataImporter = metadataImporter;
				_errorReporter = errorReporter;
				_namer = namer;
			}

			public JsExpression ResolveTypeParameter(ITypeParameter tp) {
				if (_metadataImporter.GetTypeSemantics(_currentType).IgnoreGenericArguments) {
					_errorReporter.Message(Saltarelle.Compiler.Messages._7536, tp.Name, "type", _currentType.FullName);
					return JsExpression.Null;
				}
				else {
					return JsExpression.Identifier(_namer.GetTypeParameterName(tp));
				}
			}

			public JsExpression EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
				throw new NotSupportedException();
			}
		}

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
		private const string TypeName = "__typeName";
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
		private readonly JsTypeReferenceExpression _systemObject;
		private readonly IMetadataImporter _metadataImporter;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;
		private readonly IRuntimeContext _defaultReflectionRuntimeContext;
		private readonly IRuntimeContext _genericSpecializationReflectionRuntimeContext;

		public OOPEmulator(ICompilation compilation, IMetadataImporter metadataImporter, IRuntimeLibrary runtimeLibrary, INamer namer, IErrorReporter errorReporter) {
			_compilation = compilation;
			_systemScript = new JsTypeReferenceExpression(compilation.FindType(new FullTypeName("System.Script")).GetDefinition());
			_systemObject = new JsTypeReferenceExpression(compilation.FindType(KnownTypeCode.Object).GetDefinition());

			_metadataImporter = metadataImporter;
			_runtimeLibrary = runtimeLibrary;
			_namer = namer;
			_errorReporter = errorReporter;
			_defaultReflectionRuntimeContext = new ReflectionRuntimeContext(false, _systemObject, _namer);
			_genericSpecializationReflectionRuntimeContext = new ReflectionRuntimeContext(true, _systemObject, _namer);
		}

		private JsExpression RewriteMethod(JsMethod method) {
			return method.TypeParameterNames.Count == 0 ? method.Definition : JsExpression.FunctionDefinition(method.TypeParameterNames, new JsReturnStatement(method.Definition));
		}

		private JsExpression GetMetadataDescriptor(ITypeDefinition type, bool isGenericSpecialization) {
			var properties = new List<JsObjectLiteralProperty>();
			var scriptableAttributes = type.Attributes.Where(a => !a.IsConditionallyRemoved && _metadataImporter.GetTypeSemantics(a.AttributeType.GetDefinition()).Type == TypeScriptSemantics.ImplType.NormalType).ToList();
			if (scriptableAttributes.Count != 0) {
				properties.Add(new JsObjectLiteralProperty("attr", JsExpression.ArrayLiteral(scriptableAttributes.Select(a => MetadataUtils.ConstructAttribute(a, type, _compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter)))));
			}
			if (type.Kind == TypeKind.Class) {
				var members = type.Members.Where(m => MetadataUtils.IsReflectable(m, _metadataImporter))
				                          .OrderBy(m => m, MemberOrderer.Instance)
				                          .Select(m => {
				                                           _errorReporter.Region = m.Region;
				                                           return MetadataUtils.ConstructMemberInfo(m, _compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, t => _runtimeLibrary.InstantiateType(t, isGenericSpecialization ? _genericSpecializationReflectionRuntimeContext : _defaultReflectionRuntimeContext), includeDeclaringType: false);
				                                       })
				                          .ToList();
				if (members.Count > 0)
					properties.Add(new JsObjectLiteralProperty("members", JsExpression.ArrayLiteral(members)));

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
			var args = new List<JsExpression> { GetRoot(type), JsExpression.String(name), ctor };
			var metadata = GetMetadataDescriptor(type, false);
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
			var metadata = GetMetadataDescriptor(type, false);
			if (interfaces.Count > 0 || metadata != null)
				args.Add(JsExpression.ArrayLiteral(interfaces));
			if (metadata != null)
				args.Add(metadata);
			return JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterInterface), args);
		}

		private JsExpression CreateRegisterEnumCall(ITypeDefinition type, string name, JsExpression ctor) {
			var args = new List<JsExpression> { GetRoot(type), JsExpression.String(name), ctor };
			var metadata = GetMetadataDescriptor(type, false);
			if (metadata != null)
				args.Add(metadata);
			return JsExpression.Invocation(JsExpression.Member(_systemScript, RegisterEnum), args);
		}

		private IEnumerable<JsExpression> GetImplementedInterfaces(ITypeDefinition type) {
			return type.GetAllBaseTypes().Where(t => t.Kind == TypeKind.Interface && !t.Equals(type) && MetadataUtils.DoesTypeObeyTypeSystem(t.GetDefinition())).Select(t => _runtimeLibrary.InstantiateType(t, new DefaultRuntimeContext(type, _metadataImporter, _errorReporter, _namer)));
		}

		private JsExpression GetBaseClass(ITypeDefinition type) {
			var csBase = type.DirectBaseTypes.SingleOrDefault(b => b.Kind == TypeKind.Class);
			if (csBase == null || csBase.IsKnownType(KnownTypeCode.Object) || MetadataUtils.IsImported(csBase.GetDefinition()) && MetadataUtils.IsSerializable(csBase.GetDefinition()))
				return null;
			return _runtimeLibrary.InstantiateType(csBase, new DefaultRuntimeContext(type, _metadataImporter, _errorReporter, _namer));
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
					var createInstance = MetadataUtils.CompileConstructorInvocation(defaultConstructor, null, c.CSharpTypeDefinition, null, EmptyList<ResolveResult>.Instance, _compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, null, null);
					stmts.Add(new JsExpressionStatement(
						            JsExpression.Assign(
						                JsExpression.Member(typeRef, "createInstance"),
						                    JsExpression.FunctionDefinition(new string[0], new JsBlockStatement(createInstance.AdditionalStatements.Concat(new[] { new JsReturnStatement(createInstance.Expression) }))))));
				}
			}

			stmts.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, m.Name), RewriteMethod(m)))));

			if (MetadataUtils.IsSerializable(c.CSharpTypeDefinition)) {
				string typeCheckCode = MetadataUtils.GetSerializableTypeCheckCode(c.CSharpTypeDefinition);
				if (!string.IsNullOrEmpty(typeCheckCode)) {
					var oldReg = _errorReporter.Region;
					_errorReporter.Region = c.CSharpTypeDefinition.Attributes.Single(a => a.AttributeType.FullName == typeof(SerializableAttribute).FullName).Region;
					var method = MetadataUtils.CreateTypeCheckMethod(Saltarelle.Compiler.Utils.SelfParameterize(c.CSharpTypeDefinition), _compilation);

					var errors = new List<string>();
					var tokens = InlineCodeMethodCompiler.Tokenize(method, typeCheckCode, errors.Add);
					if (errors.Count == 0) {
						var context = new DefaultRuntimeContext(c.CSharpTypeDefinition, _metadataImporter, _errorReporter, _namer);
						var result = InlineCodeMethodCompiler.CompileInlineCodeMethodInvocation(method, tokens, JsExpression.Identifier("obj"), new JsExpression[0],
						                 n => {
						                     var type = ReflectionHelper.ParseReflectionName(n).Resolve(_compilation);
						                     if (type.Kind == TypeKind.Unknown) {
						                         errors.Add("Unknown type '" + n + "' specified in inline implementation");
						                         return JsExpression.Null;
						                     }
						                     return _runtimeLibrary.InstantiateType(type, context);
						                 },
						                 t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, context),
						                 errors.Add);

						stmts.Add(new JsExpressionStatement(
						              JsExpression.Assign(
						                  JsExpression.Member(typeRef, "isInstanceOfType"),
						                  JsExpression.FunctionDefinition(new[] { "obj" }, new JsReturnStatement(result)))));

						foreach (var e in errors) {
							_errorReporter.Message(Messages._7157, c.CSharpTypeDefinition.FullName, e);
						}
					}
					_errorReporter.Region = oldReg;
				}
			}

			if (MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter)) {
				var args = new List<JsExpression> { typeRef, new JsTypeReferenceExpression(c.CSharpTypeDefinition), JsExpression.ArrayLiteral(c.CSharpTypeDefinition.TypeParameters.Select(tp => JsExpression.Identifier(_namer.GetTypeParameterName(tp)))) };
				if (c.CSharpTypeDefinition.Kind == TypeKind.Class)
					args.Add(JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(GetBaseClass(c.CSharpTypeDefinition) ?? JsExpression.Null)));
				args.Add(JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.ArrayLiteral(GetImplementedInterfaces(c.CSharpTypeDefinition)))));
				var metadata = GetMetadataDescriptor(c.CSharpTypeDefinition, true);
				if (metadata != null)
					args.Add(metadata);
				stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemScript, c.CSharpTypeDefinition.Kind == TypeKind.Class ? RegisterGenericClassInstance : RegisterGenericInterfaceInstance), args)));
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

							if (MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter)) {
								var typeParameterNames = c.CSharpTypeDefinition.TypeParameters.Select(tp => _namer.GetTypeParameterName(tp)).ToList();
								var stmts = new List<JsStatement> { new JsVariableDeclarationStatement(InstantiatedGenericTypeVariableName, unnamedCtor) };
								AddClassMembers(c, JsExpression.Identifier(InstantiatedGenericTypeVariableName), stmts);
								stmts.AddRange(c.StaticInitStatements);
								stmts.Add(new JsReturnStatement(JsExpression.Identifier(InstantiatedGenericTypeVariableName)));
								var replacer = new GenericSimplifier(c.CSharpTypeDefinition, typeParameterNames, JsExpression.Identifier(InstantiatedGenericTypeVariableName));
								for (int i = 0; i < stmts.Count; i++)
									stmts[i] = replacer.Process(stmts[i]);
								result.Add(new JsVariableDeclarationStatement(typeRef.Name, JsExpression.FunctionDefinition(typeParameterNames, new JsBlockStatement(stmts))));
								result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, TypeName), JsExpression.String(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name))));
								var args = new List<JsExpression> { GetRoot(t.CSharpTypeDefinition), JsExpression.String(name), typeRef, JsExpression.Number(c.CSharpTypeDefinition.TypeParameterCount) };
								var metadata = GetMetadataDescriptor(t.CSharpTypeDefinition, false);
								if (metadata != null)
									args.Add(metadata);
								result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemScript, c.CSharpTypeDefinition.Kind == TypeKind.Interface ? RegisterGenericInterface : RegisterGenericClass), args)));
							}
							else {
								result.Add(new JsVariableDeclarationStatement(typeRef.Name, unnamedCtor));
								result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, TypeName), JsExpression.String(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name))));
								AddClassMembers(c, typeRef, result);
							}
						}
					}
					else if (t is JsEnum) {
						var e = (JsEnum)t;
						result.Add(new JsVariableDeclarationStatement(typeRef.Name, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)));
						result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, TypeName), JsExpression.String(_metadataImporter.GetTypeSemantics(e.CSharpTypeDefinition).Name))));
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
			                      .Where(c =>    !(c is JsClass && MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter))
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
			                .Where(c => !MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter) && !MetadataUtils.IsResources(c.CSharpTypeDefinition))
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
