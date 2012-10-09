using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Collections.Generic;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.OOPEmulator {
	public class ScriptSharpOOPEmulator : IOOPEmulator {
		private const string Prototype = "prototype";
		private const string RegisterClass = "registerClass";
		private const string RegisterInterface = "registerInterface";
		private const string RegisterEnum = "registerEnum";
		private const string RegisterType = "registerType";
		private const string RegisterGenericClassInstance = "registerGenericClassInstance";
		private const string RegisterGenericInterfaceInstance = "registerGenericInterfaceInstance";
		private const string RegisterGenericClass = "registerGenericClass";
		private const string RegisterGenericInterface = "registerGenericInterface";
		private const string FlagsAttribute = "FlagsAttribute";
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

		private readonly IScriptSharpMetadataImporter _metadataImporter;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;
		private readonly ICompilation _compilation;
		private readonly JsTypeReferenceExpression _systemType;

		public ScriptSharpOOPEmulator(ICompilation compilation, IScriptSharpMetadataImporter metadataImporter, IRuntimeLibrary runtimeLibrary, INamer namer, IErrorReporter errorReporter) {
			_metadataImporter = metadataImporter;
			_runtimeLibrary = runtimeLibrary;
			_namer = namer;
			_errorReporter = errorReporter;
			_compilation = compilation;
			_systemType = new JsTypeReferenceExpression(_compilation.FindType(KnownTypeCode.Type).GetDefinition());
		}

		private IList<object> GetAttributePositionalArgs(IEntity entity, string attributeName, string nmspace = "System.Runtime.CompilerServices") {
			attributeName = nmspace + "." + attributeName;
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == attributeName);
			return attr != null ? attr.PositionalArguments.Select(arg => arg.ConstantValue).ToList() : null;
		}

		private JsExpression RewriteMethod(JsMethod method) {
			return method.TypeParameterNames.Count == 0 ? method.Definition : JsExpression.FunctionDefinition(method.TypeParameterNames, new JsReturnStatement(method.Definition));
		}

		private JsExpression CreateRegisterClassCall(JsExpression root, string name, JsExpression ctor, JsExpression baseClass, IList<JsExpression> interfaces) {
			var args = new List<JsExpression> { root, JsExpression.String(name), ctor };
			if (baseClass != null)
				args.Add(baseClass);
			else if (interfaces.Count > 0)
				args.Add(JsExpression.Null);
			if (interfaces.Count > 0)
				args.AddRange(interfaces);
			return JsExpression.Invocation(JsExpression.Member(_systemType, RegisterClass), args);
		}

		private JsExpression CreateDefaultConstructorInvocation(IMethod defaultConstructor, JsExpression typeRef) {
			var sem = _metadataImporter.GetConstructorSemantics(defaultConstructor);
			switch (sem.Type) {
				case ConstructorScriptSemantics.ImplType.UnnamedConstructor:  // default behavior is good enough.
				case ConstructorScriptSemantics.ImplType.NotUsableFromScript: // Can't be invoked so we don't need to create it.
					return null;

				case ConstructorScriptSemantics.ImplType.NamedConstructor:
					return JsExpression.New(JsExpression.Member(typeRef, sem.Name));

				case ConstructorScriptSemantics.ImplType.StaticMethod:
					return JsExpression.Invocation(JsExpression.Member(typeRef, sem.Name));

				case ConstructorScriptSemantics.ImplType.InlineCode:
					var prevRegion = _errorReporter.Region;
					try {
						_errorReporter.Region = defaultConstructor.Region;
						return InlineCodeMethodCompiler.CompileInlineCodeMethodInvocation(defaultConstructor, sem.LiteralCode, null, EmptyList<JsExpression>.Instance, r => r.Resolve(_compilation), _runtimeLibrary.GetScriptType, false, s => _errorReporter.Message(7525, s));
					}
					finally {
						_errorReporter.Region = prevRegion;
					}

				case ConstructorScriptSemantics.ImplType.Json:
					return JsExpression.ObjectLiteral();

				default:
					throw new Exception("Invalid constructor implementation type: " + sem.Type);
			}
		}

		private void AddClassMembers(JsClass c, JsExpression typeRef, ICompilation compilation, List<JsStatement> stmts) {
			ICollection<JsMethod> instanceMethods;
			if (_metadataImporter.IsTestFixture(c.CSharpTypeDefinition)) {
				var tests = new List<Tuple<string, string, bool, int?, JsFunctionDefinitionExpression>>();
				var instanceMethodList = new List<JsMethod>();
				foreach (var m in c.InstanceMethods) {
					var td = (m.CSharpMember is IMethod ? _metadataImporter.GetTestData((IMethod)m.CSharpMember) : null);
					if (td != null) {
						tests.Add(Tuple.Create(td.Description, td.Category, td.IsAsync, td.ExpectedAssertionCount, m.Definition));
					}
					else {
						instanceMethodList.Add(m);
					}
				}
				var testInvocations = new List<JsExpression>();
				foreach (var category in tests.GroupBy(t => t.Item2).Select(g => new { Category = g.Key, Tests = g.Select(x => new { Description = x.Item1, IsAsync = x.Item3, ExpectedAssertionCount = x.Item4, Function = x.Item5 }) }).OrderBy(x => x.Category)) {
					if (category.Category != null)
						testInvocations.Add(JsExpression.Invocation(JsExpression.Identifier("module"), JsExpression.String(category.Category)));
					testInvocations.AddRange(category.Tests.Select(t => JsExpression.Invocation(JsExpression.Identifier(t.IsAsync ? "asyncTest" : "test"), t.ExpectedAssertionCount != null ? new JsExpression[] { JsExpression.String(t.Description), JsExpression.Number(t.ExpectedAssertionCount.Value), _runtimeLibrary.Bind(t.Function, JsExpression.This) } : new JsExpression[] { JsExpression.String(t.Description), _runtimeLibrary.Bind(t.Function, JsExpression.This) })));
				}

				instanceMethodList.Add(new JsMethod(null, "runTests", null, JsExpression.FunctionDefinition(new string[0], new JsBlockStatement(testInvocations.Select(t => new JsExpressionStatement(t))))));

				instanceMethods = instanceMethodList;
			}
			else {
				instanceMethods = c.InstanceMethods;
			}

			if (instanceMethods.Count > 0) {
				stmts.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, Prototype), JsExpression.ObjectLiteral(instanceMethods.Select(m => new JsObjectLiteralProperty(m.Name, m.Definition != null ? RewriteMethod(m) : JsExpression.Null))))));
			}

			if (c.NamedConstructors.Count > 0) {
				stmts.AddRange(c.NamedConstructors.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, m.Name), m.Definition))));
				stmts.Add(new JsExpressionStatement(c.NamedConstructors.Reverse().Aggregate((JsExpression)JsExpression.Member(typeRef, Prototype), (right, ctor) => JsExpression.Assign(JsExpression.Member(JsExpression.Member(typeRef, ctor.Name), Prototype), right))));	// This generates a statement like {C}.ctor1.prototype = {C}.ctor2.prototype = {C}.prototoype
			}

			var defaultConstructor = c.CSharpTypeDefinition.GetConstructors().SingleOrDefault(x => x.Parameters.Count == 0 && x.IsPublic);
			if (defaultConstructor != null) {
				JsExpression createInstance = CreateDefaultConstructorInvocation(defaultConstructor, typeRef);
				if (createInstance != null) {
					stmts.Add(new JsExpressionStatement(
					              JsExpression.Assign(
					                  JsExpression.Member(typeRef, "createInstance"),
					                      JsExpression.FunctionDefinition(new string[0],
					                          new JsReturnStatement(createInstance)))));
				}
			}

			stmts.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, m.Name), RewriteMethod(m)))));

			if (c.TypeArgumentNames.Count > 0) {
				if (c.ClassType == JsClass.ClassTypeEnum.Interface) {
					stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemType, RegisterGenericInterfaceInstance),
					                                                            typeRef,
					                                                            new JsTypeReferenceExpression(c.CSharpTypeDefinition),
					                                                            JsExpression.ArrayLiteral(c.TypeArgumentNames.Select(JsExpression.Identifier)),
																				JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.ArrayLiteral(c.ImplementedInterfaces))))));
				}
				else {
					stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemType, RegisterGenericClassInstance),
					                                                            typeRef,
					                                                            new JsTypeReferenceExpression(c.CSharpTypeDefinition),
					                                                            JsExpression.ArrayLiteral(c.TypeArgumentNames.Select(JsExpression.Identifier)),
					                                                            JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(c.BaseClass)),
																				JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.ArrayLiteral(c.ImplementedInterfaces))))));
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

		private IList<JsClass> TopologicalSortTypesByInheritance(IList<JsClass> types) {
			types = new List<JsClass>(types);
			var result = new List<JsClass>();
			int iterationsLeft = types.Count;
			while (types.Count > 0) {
				if (iterationsLeft <= 0)
					throw new Exception("Circular inheritance chain involving types " + string.Join(", ", types.Select(t => t.CSharpTypeDefinition.FullName)));

				for (int i = 0; i < types.Count; i++) {
					var type = types[i];
					if (!type.CSharpTypeDefinition.DirectBaseTypes.Select(x => x.GetDefinition()).Intersect(types.Select(c => c.CSharpTypeDefinition)).Any()) {
						result.Add(type);
						types.RemoveAt(i);
						i--;
					}
				}
				iterationsLeft--;
			}
			return result;
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
			if (!exportNonPublic && !Utils.IsPublic(type))
				return JsExpression.Null;
			else
				return JsExpression.Identifier(string.IsNullOrEmpty(_metadataImporter.GetModuleName(type)) ? "global" : "exports");
		}

		public IList<JsStatement> Process(IEnumerable<JsType> types, ICompilation compilation) {
			var result = new List<JsStatement>();

			var orderedTypes = OrderByNamespace(types, t => _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name).ToList();
			foreach (var t in orderedTypes) {
				try {
					string name = _metadataImporter.GetTypeSemantics(t.CSharpTypeDefinition).Name;
					bool isGlobal = string.IsNullOrEmpty(name);
					bool isMixin  = _metadataImporter.IsMixin(t.CSharpTypeDefinition);

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
						else if (_metadataImporter.IsResources(t.CSharpTypeDefinition)) {
							result.Add(GenerateResourcesClass(c));
						}
						else {
							var unnamedCtor = c.UnnamedConstructor ?? JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);

							if (c.TypeArgumentNames.Count == 0) {
								result.Add(new JsVariableDeclarationStatement(typeRef.Name, unnamedCtor));
								AddClassMembers(c, typeRef, compilation, result);
							}
							else {
								var stmts = new List<JsStatement> { new JsVariableDeclarationStatement(InstantiatedGenericTypeVariableName, unnamedCtor) };
								AddClassMembers(c, JsExpression.Identifier(InstantiatedGenericTypeVariableName), compilation, stmts);
								stmts.AddRange(c.StaticInitStatements);
								stmts.Add(new JsReturnStatement(JsExpression.Identifier(InstantiatedGenericTypeVariableName)));
								result.Add(new JsVariableDeclarationStatement(typeRef.Name, JsExpression.FunctionDefinition(c.TypeArgumentNames, new JsBlockStatement(stmts))));
								result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemType, c.ClassType == JsClass.ClassTypeEnum.Interface ? RegisterGenericInterface : RegisterGenericClass), GetRoot(t.CSharpTypeDefinition), JsExpression.String(name), typeRef, JsExpression.Number(c.TypeArgumentNames.Count))));
							}
						}
					}
					else if (t is JsEnum) {
						var e = (JsEnum)t;
						bool flags = GetAttributePositionalArgs(t.CSharpTypeDefinition, FlagsAttribute, "System") != null;
						result.Add(new JsVariableDeclarationStatement(typeRef.Name, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)));
						result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Member(typeRef, Prototype), JsExpression.ObjectLiteral(e.Values.Select(v => new JsObjectLiteralProperty(v.Name, (_metadataImporter.IsNamedValues(t.CSharpTypeDefinition) ? JsExpression.String(v.Name) : JsExpression.Number(v.Value))))))));
						result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(_systemType, RegisterEnum), GetRoot(t.CSharpTypeDefinition), JsExpression.String(name), typeRef, JsExpression.Boolean(flags))));
					}
				}
				catch (Exception ex) {
					_errorReporter.Region = t.CSharpTypeDefinition.Region;
					_errorReporter.InternalError(ex, "Error formatting type " + t.CSharpTypeDefinition.FullName);
				}
			}

			var typesToRegister = orderedTypes.OfType<JsClass>()
			                            .Where(c =>    c.TypeArgumentNames.Count == 0
			                                        && !string.IsNullOrEmpty(_metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name)
			                                        && (!_metadataImporter.IsResources(c.CSharpTypeDefinition) || Utils.IsPublic(c.CSharpTypeDefinition))	// Resources classes are only exported if they are public.
			                                        && !_metadataImporter.IsMixin(c.CSharpTypeDefinition))
			                            .ToList();

			result.AddRange(TopologicalSortTypesByInheritance(typesToRegister)
			                .Select(c => {
			                                 try {
			                                     string name = _metadataImporter.GetTypeSemantics(c.CSharpTypeDefinition).Name;
			                                     if (_metadataImporter.IsResources(c.CSharpTypeDefinition)) {
			                                         return JsExpression.Invocation(JsExpression.Member(_systemType, RegisterType), GetRoot(c.CSharpTypeDefinition), JsExpression.String(name), JsExpression.Identifier(_namer.GetTypeVariableName(name)));
			                                     }
			                                     if (c.ClassType == JsClass.ClassTypeEnum.Interface) {
			                                         return JsExpression.Invocation(JsExpression.Member(_systemType, RegisterInterface), GetRoot(c.CSharpTypeDefinition), JsExpression.String(name), JsExpression.Identifier(_namer.GetTypeVariableName(name)), JsExpression.ArrayLiteral(c.ImplementedInterfaces));
			                                     }
			                                     else {
			                                         return CreateRegisterClassCall(GetRoot(c.CSharpTypeDefinition), name, JsExpression.Identifier(_namer.GetTypeVariableName(name)), c.BaseClass, c.ImplementedInterfaces);
			                                     }
			                                 }
			                                 catch (Exception ex) {
			                                     _errorReporter.Region = c.CSharpTypeDefinition.Region;
			                                     _errorReporter.InternalError(ex, "Error formatting type " + c.CSharpTypeDefinition.FullName);
			                                     return JsExpression.Number(0);
			                                 }
			                             })
			                .Select(expr => new JsExpressionStatement(expr)));
			result.AddRange(orderedTypes.OfType<JsClass>().Where(c => c.TypeArgumentNames.Count == 0 && !_metadataImporter.IsResources(c.CSharpTypeDefinition)).SelectMany(t => t.StaticInitStatements));

			return result;
		}
	}
}
