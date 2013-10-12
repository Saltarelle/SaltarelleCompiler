using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
				if (_isGenericSpecialization && tp.OwnerType == SymbolKind.TypeDefinition)
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
		private const string InitClass = "initClass";
		private const string InitInterface = "initInterface";
		private const string InitEnum = "initEnum";
		private const string RegisterGenericClassInstance = "registerGenericClassInstance";
		private const string RegisterGenericInterfaceInstance = "registerGenericInterfaceInstance";
		private const string InitGenericClass = "initGenericClass";
		private const string InitGenericInterface = "initGenericInterface";
		private const string SetMetadata = "setMetadata";
		private const string InstantiatedGenericTypeVariableName = "$type";
		private const string InitAssembly = "initAssembly";

		private static Tuple<string, string> SplitIntoNamespaceAndName(string name) {
			int pos = name.LastIndexOf('.');
			if (pos == -1)
				return Tuple.Create("", name);
			else
				return Tuple.Create(name.Substring(0, pos), name.Substring(pos + 1));
		}

		internal static IEnumerable<T> OrderByNamespace<T>(IEnumerable<T> source, Func<T, string> nameSelector) {
			return    from s in source
			           let t = SplitIntoNamespaceAndName(nameSelector(s))
			       orderby t.Item1, t.Item2
			        select s;
		}

		private readonly ICompilation _compilation;
		private readonly JsTypeReferenceExpression _systemScript;
		private readonly JsTypeReferenceExpression _systemObject;
		private readonly IMetadataImporter _metadataImporter;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly INamer _namer;
		private readonly ILinker _linker;
		private readonly IErrorReporter _errorReporter;
		private readonly IRuntimeContext _defaultReflectionRuntimeContext;
		private readonly IRuntimeContext _genericSpecializationReflectionRuntimeContext;

		public OOPEmulator(ICompilation compilation, IMetadataImporter metadataImporter, IRuntimeLibrary runtimeLibrary, INamer namer, ILinker linker, IErrorReporter errorReporter) {
			_compilation = compilation;
			_systemScript = new JsTypeReferenceExpression(compilation.FindType(new FullTypeName("System.Script")).GetDefinition());
			_systemObject = new JsTypeReferenceExpression(compilation.FindType(KnownTypeCode.Object).GetDefinition());

			_metadataImporter = metadataImporter;
			_runtimeLibrary = runtimeLibrary;
			_namer = namer;
			_linker = linker;
			_errorReporter = errorReporter;
			_defaultReflectionRuntimeContext = new ReflectionRuntimeContext(false, _systemObject, _namer);
			_genericSpecializationReflectionRuntimeContext = new ReflectionRuntimeContext(true, _systemObject, _namer);
		}

		private JsExpression RewriteMethod(JsMethod method) {
			return method.TypeParameterNames.Count == 0 ? method.Definition : JsExpression.FunctionDefinition(method.TypeParameterNames, JsStatement.Return(method.Definition));
		}

		private static int ConvertVarianceToInt(VarianceModifier variance) {
			switch (variance) {
				case VarianceModifier.Covariant:
					return 1;
				case VarianceModifier.Contravariant:
					return 2;
				default:
					return 0;
			}
		}


		private JsExpression GetMetadataDescriptor(ITypeDefinition type, bool isGenericSpecialization) {
			var properties = new List<JsObjectLiteralProperty>();
			var scriptableAttributes = MetadataUtils.GetScriptableAttributes(type.Attributes, _metadataImporter).ToList();
			if (scriptableAttributes.Count != 0) {
				properties.Add(new JsObjectLiteralProperty("attr", JsExpression.ArrayLiteral(scriptableAttributes.Select(a => MetadataUtils.ConstructAttribute(a, type, _compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter)))));
			}
			if (type.Kind == TypeKind.Interface && MetadataUtils.IsJsGeneric(type, _metadataImporter) && type.TypeParameters != null && type.TypeParameters.Any(typeParameter => typeParameter.Variance != VarianceModifier.Invariant)) {
				properties.Add(new JsObjectLiteralProperty("variance", JsExpression.ArrayLiteral(type.TypeParameters.Select(typeParameter => JsExpression.Number(ConvertVarianceToInt(typeParameter.Variance))))));
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

		private JsExpression CreateInstanceMembers(JsClass c) {
			return JsExpression.ObjectLiteral(c.InstanceMethods.Select(m => new JsObjectLiteralProperty(m.Name, m.Definition != null ? RewriteMethod(m) : JsExpression.Null)));
		}

		private JsExpression CreateInitClassCall(JsClass type, string ctorName, JsExpression baseClass, IList<JsExpression> interfaces) {
			var args = new List<JsExpression> { JsExpression.Identifier(ctorName), _linker.CurrentAssemblyExpression, CreateInstanceMembers(type) };
			if (baseClass != null || interfaces.Count > 0)
				args.Add(baseClass ?? JsExpression.Null);
			if (interfaces.Count > 0)
				args.Add(JsExpression.ArrayLiteral(interfaces));

			return JsExpression.Invocation(JsExpression.Member(_systemScript, InitClass), args);
		}

		private JsExpression CreateInitInterfaceCall(JsClass type, string ctorName, IList<JsExpression> interfaces) {
			var args = new List<JsExpression> { JsExpression.Identifier(ctorName), _linker.CurrentAssemblyExpression, CreateInstanceMembers(type) };
			if (interfaces.Count > 0)
				args.Add(JsExpression.ArrayLiteral(interfaces));
			return JsExpression.Invocation(JsExpression.Member(_systemScript, InitInterface), args);
		}

		private JsExpression CreateInitEnumCall(JsEnum type, string ctorName) {
			var values = new List<JsObjectLiteralProperty>();
			foreach (var v in type.CSharpTypeDefinition.Fields) {
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

			var args = new List<JsExpression> { JsExpression.Identifier(ctorName), _linker.CurrentAssemblyExpression, JsExpression.ObjectLiteral(values) };
			if (MetadataUtils.IsNamedValues(type.CSharpTypeDefinition))
				args.Add(JsExpression.True);
			return JsExpression.Invocation(JsExpression.Member(_systemScript, InitEnum), args);
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

		private JsExpression AssignNamedConstructorPrototypes(JsClass c, JsExpression typeRef) {
			return c.NamedConstructors.Reverse().Aggregate((JsExpression)JsExpression.Member(typeRef, Prototype), (right, ctor) => JsExpression.Assign(JsExpression.Member(JsExpression.Member(typeRef, ctor.Name), Prototype), right));	// This generates a statement like {C}.ctor1.prototype = {C}.ctor2.prototype = {C}.prototoype
		}

		private void AddClassMembers(JsClass c, string typevarName, List<JsStatement> stmts) {
			if (c.NamedConstructors.Count > 0)
				stmts.AddRange(c.NamedConstructors.Select(m => (JsStatement)JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(typevarName), m.Name), m.Definition)));

			var defaultConstructor = Saltarelle.Compiler.Utils.SelfParameterize(c.CSharpTypeDefinition).GetConstructors().SingleOrDefault(x => x.Parameters.Count == 0 && x.IsPublic);
			if (defaultConstructor != null) {
				var sem = _metadataImporter.GetConstructorSemantics(defaultConstructor);
				if (sem.Type != ConstructorScriptSemantics.ImplType.UnnamedConstructor && sem.Type != ConstructorScriptSemantics.ImplType.NotUsableFromScript) {
					var createInstance = MetadataUtils.CompileConstructorInvocation(defaultConstructor, null, c.CSharpTypeDefinition, null, EmptyList<ResolveResult>.Instance, _compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, null, null);
					stmts.Add(JsExpression.Assign(
						          JsExpression.Member(JsExpression.Identifier(typevarName), "createInstance"),
						              JsExpression.FunctionDefinition(new string[0], JsStatement.Block(createInstance.AdditionalStatements.Concat(new[] { JsStatement.Return(createInstance.Expression) })))));
				}
			}

			stmts.AddRange(c.StaticMethods.Select(m => (JsStatement)JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(typevarName), m.Name), RewriteMethod(m))));

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
						var result = InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, JsExpression.Identifier("obj"), new JsExpression[0],
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

						stmts.Add(JsExpression.Assign(
						              JsExpression.Member(JsExpression.Identifier(typevarName), "isInstanceOfType"),
						              JsExpression.FunctionDefinition(new[] { "obj" }, JsStatement.Return(result))));

						foreach (var e in errors) {
							_errorReporter.Message(Messages._7157, c.CSharpTypeDefinition.FullName, e);
						}
					}
					_errorReporter.Region = oldReg;
				}
			}

			if (MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter)) {
				var args = new List<JsExpression> { JsExpression.Identifier(typevarName),
				                                    new JsTypeReferenceExpression(c.CSharpTypeDefinition), JsExpression.ArrayLiteral(c.CSharpTypeDefinition.TypeParameters.Select(tp => JsExpression.Identifier(_namer.GetTypeParameterName(tp)))),
				                                    CreateInstanceMembers(c),
				                                  };
				if (c.CSharpTypeDefinition.Kind == TypeKind.Class)
					args.Add(JsExpression.FunctionDefinition(new string[0], JsStatement.Return(GetBaseClass(c.CSharpTypeDefinition) ?? JsExpression.Null)));
				args.Add(JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.ArrayLiteral(GetImplementedInterfaces(c.CSharpTypeDefinition)))));
				stmts.Add(JsExpression.Invocation(JsExpression.Member(_systemScript, c.CSharpTypeDefinition.Kind == TypeKind.Class ? RegisterGenericClassInstance : RegisterGenericInterfaceInstance), args));
				if (c.CSharpTypeDefinition.Kind == TypeKind.Class && c.NamedConstructors.Count > 0) {
					stmts.Add(AssignNamedConstructorPrototypes(c, JsExpression.Identifier(typevarName)));
				}
				var metadata = GetMetadataDescriptor(c.CSharpTypeDefinition, true);
				if (metadata != null)
					stmts.Add(JsExpression.Invocation(JsExpression.Member(_systemScript, SetMetadata), JsExpression.Identifier(typevarName), metadata));
			}
		}

		private JsStatement GenerateResourcesClass(JsClass c) {
			var fields = c.StaticInitStatements
			              .OfType<JsExpressionStatement>()
			              .Select(s => s.Expression)
			              .OfType<JsBinaryExpression>()
			              .Where(expr => expr.NodeType == ExpressionNodeType.Assign && expr.Left is JsMemberAccessExpression)
			              .Select(expr => new { Name = ((JsMemberAccessExpression)expr.Left).MemberName, Value = expr.Right });

			return JsStatement.Var(_namer.GetTypeVariableName(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name), JsExpression.ObjectLiteral(fields.Select(f => new JsObjectLiteralProperty(f.Name, f.Value))));
		}

		private IEnumerable<JsType> TopologicalSortTypesByInheritance(IEnumerable<JsType> types) {
			var backref = types.ToDictionary(c => c.CSharpTypeDefinition);
			var edges = from s in backref.Keys from t in s.DirectBaseTypes.Select(x => x.GetDefinition()).Intersect(backref.Keys) select Tuple.Create(s, t);
			return TopologicalSorter.TopologicalSort(backref.Keys, edges).Select(t => backref[t]);
		}

		private JsExpression MakeNestedMemberAccess(string full, JsExpression root = null) {
			var parts = full.Split('.');
			JsExpression result = root ?? JsExpression.Identifier(parts[0]);
			for (int i = (root != null ? 0 : 1); i < parts.Length; i++) {
				result = JsExpression.Member(result, parts[i]);
			}
			return result;
		}

		private string GetRoot(ITypeDefinition type) {
			return string.IsNullOrEmpty(MetadataUtils.GetModuleName(type)) ? "global" : "exports";
		}

		private IEnumerable<string> EntireNamespaceHierarchy(string nmspace) {
			do {
				yield return nmspace;
				nmspace = SplitIntoNamespaceAndName(nmspace).Item1;
			} while (!string.IsNullOrEmpty(nmspace));
		}

		private IEnumerable<JsStatement> CreateNamespaces(JsExpression root, IEnumerable<string> requiredNamespaces) {
			return requiredNamespaces.SelectMany(EntireNamespaceHierarchy)
			                         .Distinct()
			                         .OrderBy(ns => ns)
			                         .Select(ns => {
			                                           var access = MakeNestedMemberAccess(ns, root);
			                                           return (JsStatement)JsExpression.Assign(access, JsExpression.LogicalOr(access, JsExpression.ObjectLiteral()));
			                                       });
		}

		private IEnumerable<IAssemblyResource> GetIncludedResources() {
			return _compilation.MainAssembly.Resources.Where(r => r.Type == AssemblyResourceType.Embedded && !r.Name.EndsWith("Plugin.dll"));
		}

		private static byte[] ReadResource(IAssemblyResource r) {
			using (var ms = new MemoryStream())
			using (var s = r.GetResourceStream()) {
				s.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public JsStatement MakeInitAssemblyCall() {
			var args = new List<JsExpression> { _linker.CurrentAssemblyExpression, JsExpression.String(_compilation.MainAssembly.AssemblyName) };
			var includedResources = GetIncludedResources().ToList();
			if (includedResources.Count > 0)
				args.Add(JsExpression.ObjectLiteral(includedResources.Select(r => new JsObjectLiteralProperty(r.Name, JsExpression.String(Convert.ToBase64String(ReadResource(r)))))));

			return JsExpression.Invocation(JsExpression.Member(_systemScript, InitAssembly), args);
		}

		public IList<JsStatement> Process(IEnumerable<JsType> types, IMethod entryPoint) {
			var result = new List<JsStatement> { MakeInitAssemblyCall() };

			var orderedTypes = OrderByNamespace(types, t => _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name).ToList();
			var exportedNamespacesByRoot = new Dictionary<string, HashSet<string>>();
			foreach (var t in orderedTypes) {
				try {
					string name = _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name;
					bool isGlobal = string.IsNullOrEmpty(name);
					bool isMixin  = MetadataUtils.IsMixin(t.CSharpTypeDefinition);
					bool export   = t.CSharpTypeDefinition.IsExternallyVisible();

					result.Add(JsStatement.Comment("//////////////////////////////////////////////////////////////////////////////" + Environment.NewLine + " " + t.CSharpTypeDefinition.FullName));

					string typevarName = _namer.GetTypeVariableName(name);
					if (t is JsClass) {
						var c = (JsClass)t;
						if (isGlobal) {
							result.AddRange(c.StaticMethods.Select(m => (JsStatement)JsExpression.Binary(ExpressionNodeType.Assign, JsExpression.Member(JsExpression.Identifier(GetRoot(t.CSharpTypeDefinition)), m.Name), m.Definition)));
							export = false;
						}
						else if (isMixin) {
							result.AddRange(c.StaticMethods.Select(m => (JsStatement)JsExpression.Assign(MakeNestedMemberAccess(name + "." + m.Name), m.Definition)));
							export = false;
						}
						else if (MetadataUtils.IsResources(c.CSharpTypeDefinition)) {
							result.Add(GenerateResourcesClass(c));
						}
						else {
							var unnamedCtor = c.UnnamedConstructor ?? JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock);

							if (MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter)) {
								var typeParameterNames = c.CSharpTypeDefinition.TypeParameters.Select(tp => _namer.GetTypeParameterName(tp)).ToList();
								var stmts = new List<JsStatement> { JsStatement.Var(InstantiatedGenericTypeVariableName, unnamedCtor) };
								AddClassMembers(c, InstantiatedGenericTypeVariableName, stmts);
								stmts.AddRange(c.StaticInitStatements);
								stmts.Add(JsStatement.Return(JsExpression.Identifier(InstantiatedGenericTypeVariableName)));
								var replacer = new GenericSimplifier(c.CSharpTypeDefinition, typeParameterNames, JsExpression.Identifier(InstantiatedGenericTypeVariableName));
								for (int i = 0; i < stmts.Count; i++)
									stmts[i] = replacer.Process(stmts[i]);
								result.Add(JsStatement.Var(typevarName, JsExpression.FunctionDefinition(typeParameterNames, JsStatement.Block(stmts))));
								result.Add(JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(typevarName), TypeName), JsExpression.String(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name)));
								var args = new List<JsExpression> { JsExpression.Identifier(typevarName), _linker.CurrentAssemblyExpression, JsExpression.Number(c.CSharpTypeDefinition.TypeParameterCount) };
								result.Add(JsExpression.Invocation(JsExpression.Member(_systemScript, c.CSharpTypeDefinition.Kind == TypeKind.Interface ? InitGenericInterface : InitGenericClass), args));
							}
							else {
								result.Add(JsStatement.Var(typevarName, unnamedCtor));
								result.Add(JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(typevarName), TypeName), JsExpression.String(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name)));
								AddClassMembers(c, typevarName, result);
							}
						}
					}
					else if (t is JsEnum) {
						var e = (JsEnum)t;
						result.Add(JsStatement.Var(typevarName, JsExpression.FunctionDefinition(new string[0], JsStatement.EmptyBlock)));
						result.Add(JsExpression.Assign(JsExpression.Member(JsExpression.Identifier(typevarName), TypeName), JsExpression.String(_metadataImporter.GetTypeSemantics(e.CSharpTypeDefinition).Name)));
					}

					if (export) {
						string root = GetRoot(t.CSharpTypeDefinition);
						var split = SplitIntoNamespaceAndName(name);
						if (!string.IsNullOrEmpty(split.Item1)) {
							HashSet<string> hs;
							if (!exportedNamespacesByRoot.TryGetValue(root, out hs))
								hs = exportedNamespacesByRoot[root] = new HashSet<string>();
							hs.Add(split.Item1);
						}

						result.Add(JsExpression.Assign(MakeNestedMemberAccess(name, JsExpression.Identifier(root)), JsExpression.Identifier(typevarName)));
					}
				}
				catch (Exception ex) {
					_errorReporter.Region = t.CSharpTypeDefinition.Region;
					_errorReporter.InternalError(ex, "Error formatting type " + t.CSharpTypeDefinition.FullName);
				}
			}

			result.InsertRange(0, exportedNamespacesByRoot.OrderBy(x => x.Key).SelectMany(x => CreateNamespaces(JsExpression.Identifier(x.Key), x.Value)).ToList());

			var typesToRegister = TopologicalSortTypesByInheritance(orderedTypes)
			                      .Where(c =>    !(c is JsClass && MetadataUtils.IsJsGeneric(c.CSharpTypeDefinition, _metadataImporter))
			                                  && !string.IsNullOrEmpty(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name)
			                                  && !MetadataUtils.IsResources(c.CSharpTypeDefinition)
			                                  && !MetadataUtils.IsMixin(c.CSharpTypeDefinition))
			                      .ToList();

			foreach (var t in typesToRegister) {
				try {
					string name = _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name;
					string typevarName = _namer.GetTypeVariableName(name);
					if (t.CSharpTypeDefinition.Kind == TypeKind.Enum) {
						result.Add(CreateInitEnumCall((JsEnum)t, typevarName));
					}
					else {
						var c = (JsClass)t;
						if (t.CSharpTypeDefinition.Kind == TypeKind.Interface) {
							result.Add(CreateInitInterfaceCall(c, typevarName, GetImplementedInterfaces(t.CSharpTypeDefinition.GetDefinition()).ToList()));
						}
						else {
							result.Add(CreateInitClassCall(c, typevarName, GetBaseClass(t.CSharpTypeDefinition), GetImplementedInterfaces(t.CSharpTypeDefinition).ToList()));
							if (c.NamedConstructors.Count > 0) {
								result.Add(AssignNamedConstructorPrototypes(c, JsExpression.Identifier(_namer.GetTypeVariableName(name))));
							}
						}
					}
				}
				catch (Exception ex) {
					_errorReporter.Region = t.CSharpTypeDefinition.Region;
					_errorReporter.InternalError(ex, "Error formatting type " + t.CSharpTypeDefinition.FullName);
				}
			}

			foreach (var t in orderedTypes) {
				var metadata = GetMetadataDescriptor(t.CSharpTypeDefinition, false);
				if (metadata != null)
					result.Add(JsExpression.Invocation(JsExpression.Member(_systemScript, SetMetadata), JsExpression.Identifier(_namer.GetTypeVariableName(_metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name)), metadata));
			}

			var scriptableAttributes = MetadataUtils.GetScriptableAttributes(_compilation.MainAssembly.AssemblyAttributes, _metadataImporter).ToList();
			if (scriptableAttributes.Count > 0)
				result.Add(JsExpression.Assign(JsExpression.Member(_linker.CurrentAssemblyExpression, "attr"), JsExpression.ArrayLiteral(scriptableAttributes.Select(a => MetadataUtils.ConstructAttribute(a, null, _compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter)))));

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
						result.Add(JsExpression.Invocation(JsExpression.Member(new JsTypeReferenceExpression(entryPoint.DeclaringTypeDefinition), sem.Name)));
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
