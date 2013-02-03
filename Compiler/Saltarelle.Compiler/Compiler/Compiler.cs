using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class Compiler : DepthFirstAstVisitor, ICompiler {
		private class ResolveAllNavigator : IResolveVisitorNavigator {
			public ResolveVisitorNavigationMode Scan(AstNode node) {
				return ResolveVisitorNavigationMode.Resolve;
			}

			public void Resolved(AstNode node, ResolveResult result) {
			}

			public void ProcessConversion(Expression expression, ResolveResult result, Conversion conversion, IType targetType) {
			}
		}

		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private bool _allowUserDefinedStructs;
		private ICompilation _compilation;
		private CSharpAstResolver _resolver;
		private Dictionary<ITypeDefinition, JsClass> _types;
		private HashSet<Tuple<ConstructorDeclaration, CSharpAstResolver>> _constructorDeclarations;
		private Dictionary<JsClass, List<JsStatement>> _instanceInitStatements;
		private AstNode _currentNode;
		private ISet<string> _definedSymbols;

		public event Action<IMethod, JsFunctionDefinitionExpression, MethodCompiler> MethodCompiled;

		private void OnMethodCompiled(IMethod method, JsFunctionDefinitionExpression result, MethodCompiler mc) {
			if (MethodCompiled != null)
				MethodCompiled(method, result, mc);
		}

		public Compiler(IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter) {
			_metadataImporter        = metadataImporter;
			_namer                   = namer;
			_errorReporter           = errorReporter;
			_runtimeLibrary          = runtimeLibrary;
		}

		internal bool? AllowUserDefinedStructs { get; set; }

		private JsClass GetJsClass(ITypeDefinition typeDefinition) {
			JsClass result;
			if (!_types.TryGetValue(typeDefinition, out result)) {
				if (typeDefinition.Kind == TypeKind.Struct && !_allowUserDefinedStructs) {
					var oldRegion = _errorReporter.Region;
					_errorReporter.Region = typeDefinition.Region;
					_errorReporter.Message(Messages._7998, "user-defined value type (struct)");
					_errorReporter.Region = oldRegion;
				}

				var semantics = _metadataImporter.GetTypeSemantics(typeDefinition);
				if (semantics.GenerateCode) {
					var unusableTypes = Utils.FindUsedUnusableTypes(typeDefinition.GetAllBaseTypes(), _metadataImporter).ToList();
					if (unusableTypes.Count > 0) {
						foreach (var ut in unusableTypes) {
							var oldRegion = _errorReporter.Region;
							_errorReporter.Region = typeDefinition.Region;
							_errorReporter.Message(Messages._7500, ut.FullName, typeDefinition.FullName);
							_errorReporter.Region = oldRegion;
						}
					}
					result = new JsClass(typeDefinition);
				}
				else {
					result = null;
				}
				_types[typeDefinition] = result;
			}
			return result;
		}

		private void AddInstanceInitStatements(JsClass jsClass, IEnumerable<JsStatement> statements) {
			List<JsStatement> l;
			if (!_instanceInitStatements.TryGetValue(jsClass, out l))
				_instanceInitStatements[jsClass] = l = new List<JsStatement>();
			l.AddRange(statements);
		}

		private List<JsStatement> TryGetInstanceInitStatements(JsClass jsClass) {
			List<JsStatement> l;
			if (_instanceInitStatements.TryGetValue(jsClass, out l))
				return l;
			else
				return new List<JsStatement>();
		}

		private JsEnum ConvertEnum(ITypeDefinition type) {
			var semantics = _metadataImporter.GetTypeSemantics(type);
			if (!semantics.GenerateCode)
				return null;

			return new JsEnum(type);
		}

		private IEnumerable<IType> SelfAndNested(IType type) {
			yield return type;
			foreach (var x in type.GetNestedTypes(options: GetMemberOptions.IgnoreInheritedMembers).SelectMany(c => SelfAndNested(c))) {
				yield return x;
			}
		}

		public IEnumerable<JsType> Compile(PreparedCompilation compilation) {
			_allowUserDefinedStructs = AllowUserDefinedStructs ?? compilation.Compilation.ReferencedAssemblies.Count == 0;	// mscorlib only.
			_compilation = compilation.Compilation;

			_types = new Dictionary<ITypeDefinition, JsClass>();
			_constructorDeclarations = new HashSet<Tuple<ConstructorDeclaration, CSharpAstResolver>>();
			_instanceInitStatements = new Dictionary<JsClass, List<JsStatement>>();

			foreach (var f in compilation.SourceFiles) {
				try {
					_definedSymbols = f.DefinedSymbols;

					_resolver = new CSharpAstResolver(_compilation, f.SyntaxTree, f.ParsedFile);
					_resolver.ApplyNavigator(new ResolveAllNavigator());
					f.SyntaxTree.AcceptVisitor(this);
				}
				catch (Exception ex) {
					_errorReporter.Region = _currentNode.GetRegion();
					_errorReporter.InternalError(ex);
				}
			}

			// Handle constructors. We must do this after we have visited all the compilation units because field initializer (which change the InstanceInitStatements and StaticInitStatements) might appear anywhere.
			foreach (var n in _constructorDeclarations) {
				try {
					_resolver = n.Item2;
					HandleConstructorDeclaration(n.Item1);
				}
				catch (Exception ex) {
					_errorReporter.Region = n.Item1.GetRegion();
					_errorReporter.InternalError(ex);
				}
			}

			// Add default constructors where needed.
			foreach (var toAdd in _types.Where(t => t.Value != null).SelectMany(kvp => kvp.Key.GetConstructors().Where(c => c.IsSynthetic).Select(c => new { jsClass = kvp.Value, c }))) {
				try {
					MaybeAddDefaultConstructorToType(toAdd.jsClass, toAdd.c);
				}
				catch (Exception ex) {
					_errorReporter.Region = toAdd.c.Region;
					_errorReporter.InternalError(ex, "Error adding default constructor to type");
				}
			}

			_types.Values.Where(t => t != null).ForEach(t => t.Freeze());

			var enums = new List<JsType>();
			foreach (var e in _compilation.MainAssembly.TopLevelTypeDefinitions.SelectMany(SelfAndNested).Where(t => t.Kind == TypeKind.Enum)) {
				try {
					enums.Add(ConvertEnum(e.GetDefinition()));
				}
				catch (Exception ex) {
					_errorReporter.Region = e.GetDefinition().Region;
					_errorReporter.InternalError(ex);
				}
			}

			return _types.Values.Concat(enums).Where(t => t != null);
		}

		private MethodCompiler CreateMethodCompiler() {
			return new MethodCompiler(_metadataImporter, _namer, _errorReporter, _compilation, _resolver, _runtimeLibrary, _definedSymbols);
		}

		private void AddCompiledMethodToType(JsClass jsClass, IMethod method, MethodScriptSemantics options, JsMethod jsMethod) {
			if ((options.Type == MethodScriptSemantics.ImplType.NormalMethod && method.IsStatic) || options.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
				jsClass.StaticMethods.Add(jsMethod);
			}
			else {
				jsClass.InstanceMethods.Add(jsMethod);
			}
		}

		private void MaybeCompileAndAddMethodToType(JsClass jsClass, EntityDeclaration node, BlockStatement body, IMethod method, MethodScriptSemantics options) {
			if (options.GeneratedMethodName != null) {
				var typeParamNames = options.IgnoreGenericArguments ? (IEnumerable<string>)new string[0] : method.TypeParameters.Select(tp => _namer.GetTypeParameterName(tp)).ToList();
				JsMethod jsMethod;
				if (method.IsAbstract) {
					jsMethod = new JsMethod(method, options.GeneratedMethodName, typeParamNames, null);
				}
				else {
					var compiled = CompileMethod(node, body, method, options);
					jsMethod = new JsMethod(method, options.GeneratedMethodName, typeParamNames, compiled);
				}
				AddCompiledMethodToType(jsClass, method, options, jsMethod);
			}
		}

		private void AddCompiledConstructorToType(JsClass jsClass, IMethod constructor, ConstructorScriptSemantics options, JsFunctionDefinitionExpression jsConstructor) {
			switch (options.Type) {
				case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
					if (jsClass.UnnamedConstructor != null) {
						_errorReporter.Region = constructor.Region;
						_errorReporter.Message(Messages._7501, constructor.DeclaringType.FullName);
					}
					else {
						jsClass.UnnamedConstructor = jsConstructor;
					}
					break;
				case ConstructorScriptSemantics.ImplType.NamedConstructor:
					jsClass.NamedConstructors.Add(new JsNamedConstructor(options.Name, jsConstructor));
					break;

				case ConstructorScriptSemantics.ImplType.StaticMethod:
					jsClass.StaticMethods.Add(new JsMethod(constructor, options.Name, new string[0], jsConstructor));
					break;
			}
		}

		private void MaybeCompileAndAddConstructorToType(JsClass jsClass, ConstructorDeclaration node, IMethod constructor, ConstructorScriptSemantics options) {
			if (options.GenerateCode) {
				var mc = CreateMethodCompiler();
				var compiled = mc.CompileConstructor(node, constructor, TryGetInstanceInitStatements(jsClass), options);
				OnMethodCompiled(constructor, compiled, mc);
				AddCompiledConstructorToType(jsClass, constructor, options, compiled);
			}
		}

		private void MaybeAddDefaultConstructorToType(JsClass jsClass, IMethod constructor) {
			var options = _metadataImporter.GetConstructorSemantics(constructor);
			if (options.GenerateCode) {
				var mc = CreateMethodCompiler();
				var compiled = mc.CompileDefaultConstructor(constructor, TryGetInstanceInitStatements(jsClass), options);
				OnMethodCompiled(constructor, compiled, mc);
				AddCompiledConstructorToType(jsClass, constructor, options, compiled);
			}
		}

		private JsFunctionDefinitionExpression CompileMethod(EntityDeclaration node, BlockStatement body, IMethod method, MethodScriptSemantics options) {
			var mc = CreateMethodCompiler();
			var result = mc.CompileMethod(node, body, method, options);
			OnMethodCompiled(method, result, mc);
			return result;
		}

		private void CompileAndAddAutoPropertyMethodsToType(JsClass jsClass, IProperty property, PropertyScriptSemantics options, string backingFieldName) {
			if (options.GetMethod != null && options.GetMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoPropertyGetter(property, options, backingFieldName);
				AddCompiledMethodToType(jsClass, property.Getter, options.GetMethod, new JsMethod(property.Getter, options.GetMethod.GeneratedMethodName, new string[0], compiled));
			}
			if (options.SetMethod != null && options.SetMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoPropertySetter(property, options, backingFieldName);
				AddCompiledMethodToType(jsClass, property.Setter, options.SetMethod, new JsMethod(property.Setter, options.SetMethod.GeneratedMethodName, new string[0], compiled));
			}
		}

		private void CompileAndAddAutoEventMethodsToType(JsClass jsClass, EventDeclaration node, IEvent evt, EventScriptSemantics options, string backingFieldName) {
			if (options.AddMethod != null && options.AddMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoEventAdder(evt, options, backingFieldName);
				AddCompiledMethodToType(jsClass, evt.AddAccessor, options.AddMethod, new JsMethod(evt.AddAccessor, options.AddMethod.GeneratedMethodName, new string[0], compiled));
			}
			if (options.RemoveMethod != null && options.RemoveMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoEventRemover(evt, options, backingFieldName);
				AddCompiledMethodToType(jsClass, evt.RemoveAccessor, options.RemoveMethod, new JsMethod(evt.RemoveAccessor, options.RemoveMethod.GeneratedMethodName, new string[0], compiled));
			}
		}

		private void AddDefaultFieldInitializerToType(JsClass jsClass, string fieldName, IMember member, IType fieldType, ITypeDefinition owningType, bool isStatic) {
			if (isStatic) {
				jsClass.StaticInitStatements.AddRange(CreateMethodCompiler().CompileDefaultFieldInitializer(member.Region, JsExpression.Member(_runtimeLibrary.GetScriptType(Utils.SelfParameterize(owningType), TypeContext.UseStaticMember, tp => JsExpression.Identifier(_namer.GetTypeParameterName(tp))), fieldName), fieldType, member.DeclaringTypeDefinition));
			}
			else {
				AddInstanceInitStatements(jsClass, CreateMethodCompiler().CompileDefaultFieldInitializer(member.Region, JsExpression.Member(JsExpression.This, fieldName), fieldType, member.DeclaringTypeDefinition));
			}
		}

		private void CompileAndAddFieldInitializerToType(JsClass jsClass, string fieldName, ITypeDefinition owningType, Expression initializer, bool isStatic) {
			if (isStatic) {
				jsClass.StaticInitStatements.AddRange(CreateMethodCompiler().CompileFieldInitializer(initializer.GetRegion(), JsExpression.Member(_runtimeLibrary.GetScriptType(Utils.SelfParameterize(owningType), TypeContext.UseStaticMember, tp => JsExpression.Identifier(_namer.GetTypeParameterName(tp))), fieldName), initializer, owningType));
			}
			else {
				AddInstanceInitStatements(jsClass, CreateMethodCompiler().CompileFieldInitializer(initializer.GetRegion(), JsExpression.Member(JsExpression.This, fieldName), initializer, owningType));
			}
		}

		protected override void VisitChildren(AstNode node) {
			AstNode next;
			for (var child = node.FirstChild; child != null; child = next) {
				// Store next to allow the loop to continue
				// if the visitor removes/replaces child.
				next = child.NextSibling;
				_currentNode = child;
				child.AcceptVisitor (this);
			}
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration) {
			if (typeDeclaration.ClassType == ClassType.Class || typeDeclaration.ClassType == ClassType.Interface || typeDeclaration.ClassType == ClassType.Struct) {
				var resolveResult = _resolver.Resolve(typeDeclaration);
				if (!(resolveResult is TypeResolveResult)) {
					_errorReporter.Region = typeDeclaration.GetRegion();
					_errorReporter.InternalError("Type declaration " + typeDeclaration.Name + " does not resolve to a type.");
					return;
				}
				GetJsClass(resolveResult.Type.GetDefinition());

				base.VisitTypeDeclaration(typeDeclaration);
			}
		}

		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration) {
			var resolveResult = _resolver.Resolve(methodDeclaration);
			if (!(resolveResult is MemberResolveResult)) {
				_errorReporter.Region = methodDeclaration.GetRegion();
				_errorReporter.InternalError("Method declaration " + methodDeclaration.Name + " does not resolve to a member.");
				return;
			}
			var method = ((MemberResolveResult)resolveResult).Member as IMethod;
			if (method == null) {
				_errorReporter.Region = methodDeclaration.GetRegion();
				_errorReporter.InternalError("Method declaration " + methodDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
				return;
			}

			var jsClass = GetJsClass(method.DeclaringTypeDefinition);
			if (jsClass == null)
				return;

			if (method.IsAbstract || !methodDeclaration.Body.IsNull) {	// The second condition is used to ignore partial method parts without definitions.
				MaybeCompileAndAddMethodToType(jsClass, methodDeclaration, methodDeclaration.Body, method, _metadataImporter.GetMethodSemantics(method));
			}
		}

		public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration) {
			var resolveResult = _resolver.Resolve(operatorDeclaration);
			if (!(resolveResult is MemberResolveResult)) {
				_errorReporter.Region = operatorDeclaration.GetRegion();
				_errorReporter.InternalError("Operator declaration " + OperatorDeclaration.GetName(operatorDeclaration.OperatorType) + " does not resolve to a member.");
				return;
			}
			var method = ((MemberResolveResult)resolveResult).Member as IMethod;
			if (method == null) {
				_errorReporter.Region = operatorDeclaration.GetRegion();
				_errorReporter.InternalError("Operator declaration " + OperatorDeclaration.GetName(operatorDeclaration.OperatorType) + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
				return;
			}

			var jsClass = GetJsClass(method.DeclaringTypeDefinition);
			if (jsClass == null)
				return;

			MaybeCompileAndAddMethodToType(jsClass, operatorDeclaration, operatorDeclaration.Body, method, _metadataImporter.GetMethodSemantics(method));
		}

		private void HandleConstructorDeclaration(ConstructorDeclaration constructorDeclaration) {
			var resolveResult = _resolver.Resolve(constructorDeclaration);
			if (!(resolveResult is MemberResolveResult)) {
				_errorReporter.Region = constructorDeclaration.GetRegion();
				_errorReporter.InternalError("Method declaration " + constructorDeclaration.Name + " does not resolve to a member.");
				return;
			}
			var method = ((MemberResolveResult)resolveResult).Member as IMethod;
			if (method == null) {
				_errorReporter.Region = constructorDeclaration.GetRegion();
				_errorReporter.InternalError("Method declaration " + constructorDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
				return;
			}

			var jsClass = GetJsClass(method.DeclaringTypeDefinition);
			if (jsClass == null)
				return;

			if (method.IsStatic) {
				jsClass.StaticInitStatements.AddRange(CompileMethod(constructorDeclaration, constructorDeclaration.Body, method, MethodScriptSemantics.NormalMethod("X")).Body.Statements);
			}
			else {
				MaybeCompileAndAddConstructorToType(jsClass, constructorDeclaration, method, _metadataImporter.GetConstructorSemantics(method));
			}
		}


		public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration) {
			_constructorDeclarations.Add(Tuple.Create(constructorDeclaration, _resolver));
		}

		public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration) {
			var resolveResult = _resolver.Resolve(propertyDeclaration);
			if (!(resolveResult is MemberResolveResult)) {
				_errorReporter.Region = propertyDeclaration.GetRegion();
				_errorReporter.InternalError("Property declaration " + propertyDeclaration.Name + " does not resolve to a member.");
				return;
			}

			var property = ((MemberResolveResult)resolveResult).Member as IProperty;
			if (property == null) {
				_errorReporter.Region = propertyDeclaration.GetRegion();
				_errorReporter.InternalError("Property declaration " + propertyDeclaration.Name + " does not resolve to a property (resolves to " + resolveResult.ToString() + ")");
				return;
			}

			var jsClass = GetJsClass(property.DeclaringTypeDefinition);
			if (jsClass == null)
				return;

			var impl = _metadataImporter.GetPropertySemantics(property);

			switch (impl.Type) {
				case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
					if (!property.IsAbstract && propertyDeclaration.Getter.Body.IsNull && propertyDeclaration.Setter.Body.IsNull) {
						// Auto-property
						if ((impl.GetMethod != null && impl.GetMethod.GeneratedMethodName != null) || (impl.SetMethod != null && impl.SetMethod.GeneratedMethodName != null)) {
							var fieldName = _metadataImporter.GetAutoPropertyBackingFieldName(property);
							AddDefaultFieldInitializerToType(jsClass, fieldName, property, property.ReturnType, property.DeclaringTypeDefinition, property.IsStatic);
							CompileAndAddAutoPropertyMethodsToType(jsClass, property, impl, fieldName);
						}
					}
					else {
						if (!propertyDeclaration.Getter.IsNull) {
							MaybeCompileAndAddMethodToType(jsClass, propertyDeclaration.Getter, propertyDeclaration.Getter.Body, property.Getter, impl.GetMethod);
						}

						if (!propertyDeclaration.Setter.IsNull) {
							MaybeCompileAndAddMethodToType(jsClass, propertyDeclaration.Setter, propertyDeclaration.Setter.Body, property.Setter, impl.SetMethod);
						}
					}
					break;
				}
				case PropertyScriptSemantics.ImplType.Field: {
					AddDefaultFieldInitializerToType(jsClass, impl.FieldName, property, property.ReturnType, property.DeclaringTypeDefinition, property.IsStatic);
					break;
				}
				case PropertyScriptSemantics.ImplType.NotUsableFromScript: {
					break;
				}
				default: {
					throw new InvalidOperationException("Invalid property implementation " + impl.Type);
				}
			}
		}

		public override void VisitEventDeclaration(EventDeclaration eventDeclaration) {
			foreach (var singleEvt in eventDeclaration.Variables) {
				var resolveResult = _resolver.Resolve(singleEvt);
				if (!(resolveResult is MemberResolveResult)) {
					_errorReporter.Region = eventDeclaration.GetRegion();
					_errorReporter.InternalError("Event declaration " + singleEvt.Name + " does not resolve to a member.");
					return;
				}

				var evt = ((MemberResolveResult)resolveResult).Member as IEvent;
				if (evt == null) {
					_errorReporter.Region = eventDeclaration.GetRegion();
					_errorReporter.InternalError("Event declaration " + singleEvt.Name + " does not resolve to an event (resolves to " + resolveResult.ToString() + ")");
					return;
				}

				var jsClass = GetJsClass(evt.DeclaringTypeDefinition);
				if (jsClass == null)
					return;

				var impl = _metadataImporter.GetEventSemantics(evt);
				switch (impl.Type) {
					case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
						if ((impl.AddMethod != null && impl.AddMethod.GeneratedMethodName != null) || (impl.RemoveMethod != null && impl.RemoveMethod.GeneratedMethodName != null)) {
							if (evt.IsAbstract) {
								if (impl.AddMethod.GeneratedMethodName != null)
									AddCompiledMethodToType(jsClass, evt.AddAccessor, impl.AddMethod, new JsMethod(evt.AddAccessor, impl.AddMethod.GeneratedMethodName, null, null));
								if (impl.RemoveMethod.GeneratedMethodName != null)
									AddCompiledMethodToType(jsClass, evt.RemoveAccessor, impl.RemoveMethod, new JsMethod(evt.RemoveAccessor, impl.RemoveMethod.GeneratedMethodName, null, null));
							}
							else {
								var fieldName = _metadataImporter.GetAutoEventBackingFieldName(evt);
								if (singleEvt.Initializer.IsNull) {
									AddDefaultFieldInitializerToType(jsClass, fieldName, evt, evt.ReturnType, evt.DeclaringTypeDefinition, evt.IsStatic);
								}
								else {
									CompileAndAddFieldInitializerToType(jsClass, fieldName, evt.DeclaringTypeDefinition, singleEvt.Initializer, evt.IsStatic);
								}

								CompileAndAddAutoEventMethodsToType(jsClass, eventDeclaration, evt, impl, fieldName);
							}
						}
						break;
					}

					case EventScriptSemantics.ImplType.NotUsableFromScript: {
						break;
					}

					default: {
						throw new InvalidOperationException("Invalid event implementation type");
					}
				}
			}
		}

		public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration) {
			var resolveResult = _resolver.Resolve(eventDeclaration);
			if (!(resolveResult is MemberResolveResult)) {
				_errorReporter.Region = eventDeclaration.GetRegion();
				_errorReporter.InternalError("Event declaration " + eventDeclaration.Name + " does not resolve to a member.");
				return;
			}

			var evt = ((MemberResolveResult)resolveResult).Member as IEvent;
			if (evt == null) {
				_errorReporter.Region = eventDeclaration.GetRegion();
				_errorReporter.InternalError("Event declaration " + eventDeclaration.Name + " does not resolve to an event (resolves to " + resolveResult.ToString() + ")");
				return;
			}

			var jsClass = GetJsClass(evt.DeclaringTypeDefinition);
			if (jsClass == null)
				return;

			var impl = _metadataImporter.GetEventSemantics(evt);

			switch (impl.Type) {
				case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
					if (!eventDeclaration.AddAccessor.IsNull) {
						MaybeCompileAndAddMethodToType(jsClass, eventDeclaration.AddAccessor, eventDeclaration.AddAccessor.Body, evt.AddAccessor, impl.AddMethod);
					}

					if (!eventDeclaration.RemoveAccessor.IsNull) {
						MaybeCompileAndAddMethodToType(jsClass, eventDeclaration.RemoveAccessor, eventDeclaration.RemoveAccessor.Body, evt.RemoveAccessor, impl.RemoveMethod);
					}
					break;
				}
				case EventScriptSemantics.ImplType.NotUsableFromScript: {
					break;
				}
				default: {
					throw new InvalidOperationException("Invalid event implementation type");
				}
			}
		}

		public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration) {
			foreach (var v in fieldDeclaration.Variables) {
				var resolveResult = _resolver.Resolve(v);
				if (!(resolveResult is MemberResolveResult)) {
					_errorReporter.Region = fieldDeclaration.GetRegion();
					_errorReporter.InternalError("Field declaration " + v.Name + " does not resolve to a member.");
					return;
				}

				var field = ((MemberResolveResult)resolveResult).Member as IField;
				if (field == null) {
					_errorReporter.Region = fieldDeclaration.GetRegion();
					_errorReporter.InternalError("Field declaration " + v.Name + " does not resolve to a field (resolves to " + resolveResult.ToString() + ")");
					return;
				}

				var jsClass = GetJsClass(field.DeclaringTypeDefinition);
				if (jsClass == null)
					return;

				var impl = _metadataImporter.GetFieldSemantics(field);
				if (impl.GenerateCode) {
					if (v.Initializer.IsNull) {
						AddDefaultFieldInitializerToType(jsClass, impl.Name, field, field.ReturnType, field.DeclaringTypeDefinition, field.IsStatic);
					}
					else {
						CompileAndAddFieldInitializerToType(jsClass, impl.Name, field.DeclaringTypeDefinition, v.Initializer, field.IsStatic);
					}
				}
			}
		}

		public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration) {
			var resolveResult = _resolver.Resolve(indexerDeclaration);
			if (!(resolveResult is MemberResolveResult)) {
				_errorReporter.Region = indexerDeclaration.GetRegion();
				_errorReporter.InternalError("Event declaration " + indexerDeclaration.Name + " does not resolve to a member.");
				return;
			}

			var prop = ((MemberResolveResult)resolveResult).Member as IProperty;
			if (prop == null) {
				_errorReporter.Region = indexerDeclaration.GetRegion();
				_errorReporter.InternalError("Event declaration " + indexerDeclaration.Name + " does not resolve to a property (resolves to " + resolveResult.ToString() + ")");
				return;
			}

			var jsClass = GetJsClass(prop.DeclaringTypeDefinition);
			if (jsClass == null)
				return;

			var impl = _metadataImporter.GetPropertySemantics(prop);

			switch (impl.Type) {
				case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
					if (!indexerDeclaration.Getter.IsNull)
						MaybeCompileAndAddMethodToType(jsClass, indexerDeclaration.Getter, indexerDeclaration.Getter.Body, prop.Getter, impl.GetMethod);
					if (!indexerDeclaration.Setter.IsNull)
						MaybeCompileAndAddMethodToType(jsClass, indexerDeclaration.Setter, indexerDeclaration.Setter.Body, prop.Setter, impl.SetMethod);
					break;
				}
				case PropertyScriptSemantics.ImplType.NotUsableFromScript:
					break;
				default:
					throw new InvalidOperationException("Invalid indexer implementation type " + impl.Type);
			}
		}
	}
}
