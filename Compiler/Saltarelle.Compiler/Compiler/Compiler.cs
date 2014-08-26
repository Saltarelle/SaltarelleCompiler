using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class Compiler : CSharpSyntaxWalker, ICompiler, IRuntimeContext {
		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private CSharpCompilation _compilation;
		private SemanticModel _semanticModel;
		private Dictionary<INamedTypeSymbol, JsClass> _types;
		private List<JsEnum> _enums;
		private HashSet<Tuple<ConstructorDeclarationSyntax, SemanticModel>> _constructorDeclarations;
		private Dictionary<JsClass, List<JsStatement>> _instanceInitStatements;
		private SyntaxNode _currentNode;

		public event Action<IMethodSymbol, JsFunctionDefinitionExpression, MethodCompiler> MethodCompiled;

		private void OnMethodCompiled(IMethodSymbol method, JsFunctionDefinitionExpression result, MethodCompiler mc) {
			if (MethodCompiled != null)
				MethodCompiled(method, result, mc);
		}

		public Compiler(IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter) {
			_metadataImporter        = metadataImporter;
			_namer                   = namer;
			_errorReporter           = errorReporter;
			_runtimeLibrary          = runtimeLibrary;
		}

		private JsClass GetJsClass(INamedTypeSymbol typeDefinition) {
			JsClass result;
			if (!_types.TryGetValue(typeDefinition, out result)) {
				var semantics = _metadataImporter.GetTypeSemantics(typeDefinition);
				if (semantics.GenerateCode) {
					var errors = Utils.FindTypeUsageErrors(typeDefinition.GetAllBaseTypes(), _metadataImporter);
					if (errors.HasErrors) {
						var oldLocation = _errorReporter.Location;
						try {
							_errorReporter.Location = typeDefinition.GetLocation();
							foreach (var ut in errors.UsedUnusableTypes)
								_errorReporter.Message(Messages._7500, ut.Name, typeDefinition.Name);
							foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
								_errorReporter.Message(Messages._7539, t.Name);
						}
						finally {
							_errorReporter.Location = oldLocation;
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

		private JsEnum ConvertEnum(INamedTypeSymbol type) {
			var semantics = _metadataImporter.GetTypeSemantics(type);
			if (!semantics.GenerateCode)
				return null;

			return new JsEnum(type);
		}

		private IEnumerable<ITypeSymbol> SelfAndNested(ITypeSymbol type) {
			yield return type;
			foreach (var x in type.GetTypeMembers().SelectMany(SelfAndNested)) {
				yield return x;
			}
		}

		public IEnumerable<JsType> Compile(CSharpCompilation compilation) {
			_compilation = compilation;

			_types = new Dictionary<INamedTypeSymbol, JsClass>();
			_enums = new List<JsEnum>();
			_constructorDeclarations = new HashSet<Tuple<ConstructorDeclarationSyntax, SemanticModel>>();
			_instanceInitStatements = new Dictionary<JsClass, List<JsStatement>>();

			foreach (var tree in compilation.SyntaxTrees) {
				try {
					_semanticModel = _compilation.GetSemanticModel(tree);
					Visit(tree.GetRoot());
				}
				catch (Exception ex) {
					_errorReporter.Location = _currentNode.GetLocation();
					_errorReporter.InternalError(ex);
				}
			}

			// Handle constructors. We must do this after we have visited all the compilation units because field initializer (which change the InstanceInitStatements and StaticInitStatements) might appear anywhere.
			foreach (var n in _constructorDeclarations) {
				try {
					_semanticModel = n.Item2;
					HandleConstructorDeclaration(n.Item1);
				}
				catch (Exception ex) {
					_errorReporter.Location = n.Item1.GetLocation();
					_errorReporter.InternalError(ex);
				}
			}

			// Add default constructors where needed.
			foreach (var toAdd in _types.Where(t => t.Value != null).SelectMany(kvp => kvp.Key.GetMembers().OfType<IMethodSymbol>().Where(c => c.MethodKind == MethodKind.Constructor && c.IsImplicitlyDeclared).Select(c => new { jsClass = kvp.Value, c }))) {
				try {
					MaybeAddDefaultConstructorToType(toAdd.jsClass, toAdd.c);
				}
				catch (Exception ex) {
					_errorReporter.Location = toAdd.c.ContainingType.GetLocation();
					_errorReporter.InternalError(ex, "Error adding default constructor to type");
				}
			}

			_types.Values.Where(t => t != null).ForEach(t => t.Freeze());

			return _types.Values.Concat((IEnumerable<JsType>)_enums).Where(t => t != null);
		}

		private MethodCompiler CreateMethodCompiler() {
			return new MethodCompiler(_metadataImporter, _namer, _errorReporter, _compilation, _semanticModel, _runtimeLibrary);
		}

		private void AddCompiledMethodToType(JsClass jsClass, IMethodSymbol method, MethodScriptSemantics options, JsMethod jsMethod) {
			if ((options.Type == MethodScriptSemantics.ImplType.NormalMethod && method.IsStatic) || options.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
				jsClass.StaticMethods.Add(jsMethod);
			}
			else {
				jsClass.InstanceMethods.Add(jsMethod);
			}
		}

		private void MaybeCompileAndAddMethodToType(JsClass jsClass, SyntaxNode node, BlockSyntax body, IMethodSymbol method, MethodScriptSemantics options) {
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

		private void AddCompiledConstructorToType(JsClass jsClass, IMethodSymbol constructor, ConstructorScriptSemantics options, JsFunctionDefinitionExpression jsConstructor) {
			switch (options.Type) {
				case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
					if (jsClass.UnnamedConstructor != null) {
						_errorReporter.Location = constructor.GetLocation();
						_errorReporter.Message(Messages._7501, constructor.ContainingType.Name);
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

		private void MaybeCompileAndAddConstructorToType(JsClass jsClass, ConstructorDeclarationSyntax node, IMethodSymbol constructor, ConstructorScriptSemantics options) {
			if (options.GenerateCode) {
				var mc = CreateMethodCompiler();
				var compiled = mc.CompileConstructor(node, constructor, TryGetInstanceInitStatements(jsClass), options);
				OnMethodCompiled(constructor, compiled, mc);
				AddCompiledConstructorToType(jsClass, constructor, options, compiled);
			}
		}

		private void MaybeAddDefaultConstructorToType(JsClass jsClass, IMethodSymbol constructor) {
			var options = _metadataImporter.GetConstructorSemantics(constructor);
			if (options.GenerateCode) {
				var mc = CreateMethodCompiler();
				var compiled = mc.CompileDefaultConstructor(constructor, TryGetInstanceInitStatements(jsClass), options);
				OnMethodCompiled(constructor, compiled, mc);
				AddCompiledConstructorToType(jsClass, constructor, options, compiled);
			}
		}

		private JsFunctionDefinitionExpression CompileMethod(SyntaxNode node, BlockSyntax body, IMethodSymbol method, MethodScriptSemantics options) {
			var mc = CreateMethodCompiler();
			var result = mc.CompileMethod(node, body, method, options);
			OnMethodCompiled(method, result, mc);
			return result;
		}

		private void CompileAndAddAutoPropertyMethodsToType(JsClass jsClass, IPropertySymbol property, PropertyScriptSemantics options, string backingFieldName) {
			if (options.GetMethod != null && options.GetMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoPropertyGetter(property, options, backingFieldName);
				AddCompiledMethodToType(jsClass, property.GetMethod, options.GetMethod, new JsMethod(property.GetMethod, options.GetMethod.GeneratedMethodName, new string[0], compiled));
			}
			if (options.SetMethod != null && options.SetMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoPropertySetter(property, options, backingFieldName);
				AddCompiledMethodToType(jsClass, property.SetMethod, options.SetMethod, new JsMethod(property.SetMethod, options.SetMethod.GeneratedMethodName, new string[0], compiled));
			}
		}

		private void CompileAndAddAutoEventMethodsToType(JsClass jsClass, EventDeclarationSyntax node, IEventSymbol evt, EventScriptSemantics options, string backingFieldName) {
			if (options.AddMethod != null && options.AddMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoEventAdder(evt, options, backingFieldName);
				AddCompiledMethodToType(jsClass, evt.AddMethod, options.AddMethod, new JsMethod(evt.AddMethod, options.AddMethod.GeneratedMethodName, new string[0], compiled));
			}
			if (options.RemoveMethod != null && options.RemoveMethod.GeneratedMethodName != null) {
				var compiled = CreateMethodCompiler().CompileAutoEventRemover(evt, options, backingFieldName);
				AddCompiledMethodToType(jsClass, evt.RemoveMethod, options.RemoveMethod, new JsMethod(evt.RemoveMethod, options.RemoveMethod.GeneratedMethodName, new string[0], compiled));
			}
		}

		private void AddDefaultFieldInitializerToType(JsClass jsClass, string fieldName, ISymbol member, bool isStatic) {
			if (isStatic) {
				jsClass.StaticInitStatements.AddRange(CreateMethodCompiler().CompileDefaultFieldInitializer(member.GetLocation(), _runtimeLibrary.InstantiateType(Utils.SelfParameterize(member.ContainingType), this), fieldName, member));
			}
			else {
				AddInstanceInitStatements(jsClass, CreateMethodCompiler().CompileDefaultFieldInitializer(member.GetLocation(), JsExpression.This, fieldName, member));
			}
		}

		private void CompileAndAddFieldInitializerToType(JsClass jsClass, string fieldName, ISymbol member, ExpressionSyntax initializer, bool isStatic) {
			if (isStatic) {
				jsClass.StaticInitStatements.AddRange(CreateMethodCompiler().CompileFieldInitializer(initializer.GetLocation(), _runtimeLibrary.InstantiateType(Utils.SelfParameterize(member.ContainingType), this), fieldName, member, initializer));
			}
			else {
				AddInstanceInitStatements(jsClass, CreateMethodCompiler().CompileFieldInitializer(initializer.GetLocation(), JsExpression.This, fieldName, member, initializer));
			}
		}

		public override void Visit(SyntaxNode node) {
			_currentNode = node;
			base.Visit(node);
		}

		private void VisitTypeDeclaration(TypeDeclarationSyntax typeDeclaration) {
			var type = _semanticModel.GetDeclaredSymbol(typeDeclaration);
			if (type == null) {
				_errorReporter.Location = typeDeclaration.GetLocation();
				_errorReporter.InternalError("Type declaration " + typeDeclaration.Identifier.Text + " does not resolve to a type.");
				return;
			}
			GetJsClass(type);
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax node) {
			VisitTypeDeclaration(node);
			base.VisitClassDeclaration(node);
		}

		public override void VisitStructDeclaration(StructDeclarationSyntax node) {
			VisitTypeDeclaration(node);
			base.VisitStructDeclaration(node);
		}

		public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
			VisitTypeDeclaration(node);
			base.VisitInterfaceDeclaration(node);
		}

		public override void VisitEnumDeclaration(EnumDeclarationSyntax node) {
			var type = _semanticModel.GetDeclaredSymbol(node);
			if (type == null) {
				_errorReporter.Location = node.GetLocation();
				_errorReporter.InternalError("Enum declaration " + node.Identifier.Text + " does not resolve to a type.");
				return;
			}

			_enums.Add(ConvertEnum(type));
		}

		public override void VisitMethodDeclaration(MethodDeclarationSyntax methodDeclaration) {
			var method = _semanticModel.GetDeclaredSymbol(methodDeclaration);
			if (method == null) {
				_errorReporter.Location = methodDeclaration.GetLocation();
				_errorReporter.InternalError("Method declaration " + methodDeclaration.Identifier.Text + " does not resolve to a member.");
				return;
			}

			var jsClass = GetJsClass(method.ContainingType);
			if (jsClass == null)
				return;

			if (method.IsAbstract || methodDeclaration.Body != null) {	// The second condition is used to ignore partial method parts without definitions.
				MaybeCompileAndAddMethodToType(jsClass, methodDeclaration, methodDeclaration.Body, (IMethodSymbol)method, _metadataImporter.GetMethodSemantics((IMethodSymbol)method));
			}
		}

		public override void VisitOperatorDeclaration(OperatorDeclarationSyntax operatorDeclaration) {
			var method = _semanticModel.GetDeclaredSymbol(operatorDeclaration);
			if (method == null) {
				_errorReporter.Location = operatorDeclaration.GetLocation();
				_errorReporter.InternalError("Operator declaration " + operatorDeclaration.OperatorToken.Text + " does not resolve to a method.");
				return;
			}

			var jsClass = GetJsClass(method.ContainingType);
			if (jsClass == null)
				return;

			MaybeCompileAndAddMethodToType(jsClass, operatorDeclaration, operatorDeclaration.Body, method, _metadataImporter.GetMethodSemantics(method));
		}

		private void HandleConstructorDeclaration(ConstructorDeclarationSyntax constructorDeclaration) {
			var method = _semanticModel.GetDeclaredSymbol(constructorDeclaration);
			if (method == null) {
				_errorReporter.Location = constructorDeclaration.GetLocation();
				_errorReporter.InternalError("Method declaration " + constructorDeclaration.Identifier.Text + " does not resolve to a method.");
				return;
			}

			var jsClass = GetJsClass(method.ContainingType);
			if (jsClass == null)
				return;

			if (method.IsStatic) {
				jsClass.StaticInitStatements.AddRange(CompileMethod(constructorDeclaration, constructorDeclaration.Body, method, MethodScriptSemantics.NormalMethod("X")).Body.Statements);
			}
			else {
				MaybeCompileAndAddConstructorToType(jsClass, constructorDeclaration, method, _metadataImporter.GetConstructorSemantics(method));
			}
		}

		public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax constructorDeclaration) {
			_constructorDeclarations.Add(Tuple.Create(constructorDeclaration, _semanticModel));
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration) {
			var property = _semanticModel.GetDeclaredSymbol(propertyDeclaration);
			if (property == null) {
				_errorReporter.Location = propertyDeclaration.GetLocation();
				_errorReporter.InternalError("Property declaration " + propertyDeclaration.Identifier + " does not resolve to a property.");
				return;
			}

			var jsClass = GetJsClass(property.ContainingType);
			if (jsClass == null)
				return;

			var impl = _metadataImporter.GetPropertySemantics(property);
			var getter = propertyDeclaration.AccessorList.Accessors.SingleOrDefault(a => a.Keyword.CSharpKind() == SyntaxKind.GetKeyword);
			var setter = propertyDeclaration.AccessorList.Accessors.SingleOrDefault(a => a.Keyword.CSharpKind() == SyntaxKind.SetKeyword);

			switch (impl.Type) {
				case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
					if (!property.IsAbstract && getter != null && getter.Body == null && setter != null && setter.Body == null) {
						// Auto-property
						var fieldName = _metadataImporter.GetAutoPropertyBackingFieldName(property);
						if (_metadataImporter.ShouldGenerateAutoPropertyBackingField(property)) {
							AddDefaultFieldInitializerToType(jsClass, fieldName, property, property.IsStatic);
						}
						CompileAndAddAutoPropertyMethodsToType(jsClass, property, impl, fieldName);
					}
					else {
						if (getter != null) {
							MaybeCompileAndAddMethodToType(jsClass, getter, getter.Body, property.GetMethod, impl.GetMethod);
						}

						if (setter != null) {
							MaybeCompileAndAddMethodToType(jsClass, setter, setter.Body, property.SetMethod, impl.SetMethod);
						}
					}
					break;
				}
				case PropertyScriptSemantics.ImplType.Field: {
					AddDefaultFieldInitializerToType(jsClass, impl.FieldName, property, property.IsStatic);
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

		public override void VisitEventDeclaration(EventDeclarationSyntax eventDeclaration) {
			#warning TODO
			//foreach (var singleEvt in eventDeclaration.Variables) {
			//	var resolveResult = _semanticModel.Resolve(singleEvt);
			//	if (!(resolveResult is MemberResolveResult)) {
			//		_errorReporter.Region = eventDeclaration.FullSpan;
			//		_errorReporter.InternalError("Event declaration " + singleEvt.Name + " does not resolve to a member.");
			//		return;
			//	}
			//
			//	var evt = ((MemberResolveResult)resolveResult).Member as IEventSymbol;
			//	if (evt == null) {
			//		_errorReporter.Region = eventDeclaration.FullSpan;
			//		_errorReporter.InternalError("Event declaration " + singleEvt.Name + " does not resolve to an event (resolves to " + resolveResult.ToString() + ")");
			//		return;
			//	}
			//
			//	var jsClass = GetJsClass(evt.ContainingType);
			//	if (jsClass == null)
			//		return;
			//
			//	var impl = _metadataImporter.GetEventSemantics(evt);
			//	switch (impl.Type) {
			//		case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
			//			if (evt.IsAbstract) {
			//				if (impl.AddMethod.GeneratedMethodName != null)
			//					AddCompiledMethodToType(jsClass, evt.AddAccessor, impl.AddMethod, new JsMethod(evt.AddAccessor, impl.AddMethod.GeneratedMethodName, null, null));
			//				if (impl.RemoveMethod.GeneratedMethodName != null)
			//					AddCompiledMethodToType(jsClass, evt.RemoveAccessor, impl.RemoveMethod, new JsMethod(evt.RemoveAccessor, impl.RemoveMethod.GeneratedMethodName, null, null));
			//			}
			//			else {
			//				var fieldName = _metadataImporter.GetAutoEventBackingFieldName(evt);
			//				if (_metadataImporter.ShouldGenerateAutoEventBackingField(evt)) {
			//					if (singleEvt.Initializer.IsNull) {
			//						AddDefaultFieldInitializerToType(jsClass, fieldName, evt, evt.IsStatic);
			//					}
			//					else {
			//						CompileAndAddFieldInitializerToType(jsClass, fieldName, evt, singleEvt.Initializer, evt.IsStatic);
			//					}
			//				}
			//
			//				CompileAndAddAutoEventMethodsToType(jsClass, eventDeclaration, evt, impl, fieldName);
			//			}
			//			break;
			//		}
			//
			//		case EventScriptSemantics.ImplType.NotUsableFromScript: {
			//			break;
			//		}
			//
			//		default: {
			//			throw new InvalidOperationException("Invalid event implementation type");
			//		}
			//	}
			//}

			// Custom
			//var resolveResult = _semanticModel.Resolve(eventDeclaration);
			//if (!(resolveResult is MemberResolveResult)) {
			//	_errorReporter.Region = eventDeclaration.FullSpan;
			//	_errorReporter.InternalError("Event declaration " + eventDeclaration.Name + " does not resolve to a member.");
			//	return;
			//}
			//
			//var evt = ((MemberResolveResult)resolveResult).Member as IEventSymbol;
			//if (evt == null) {
			//	_errorReporter.Region = eventDeclaration.FullSpan;
			//	_errorReporter.InternalError("Event declaration " + eventDeclaration.Name + " does not resolve to an event (resolves to " + resolveResult.ToString() + ")");
			//	return;
			//}
			//
			//var jsClass = GetJsClass(evt.ContainingType);
			//if (jsClass == null)
			//	return;
			//
			//var impl = _metadataImporter.GetEventSemantics(evt);
			//
			//switch (impl.Type) {
			//	case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
			//		if (!eventDeclaration.AddAccessor.IsNull) {
			//			MaybeCompileAndAddMethodToType(jsClass, eventDeclaration.AddAccessor, eventDeclaration.AddAccessor.Body, evt.AddAccessor, impl.AddMethod);
			//		}
			//
			//		if (!eventDeclaration.RemoveAccessor.IsNull) {
			//			MaybeCompileAndAddMethodToType(jsClass, eventDeclaration.RemoveAccessor, eventDeclaration.RemoveAccessor.Body, evt.RemoveAccessor, impl.RemoveMethod);
			//		}
			//		break;
			//	}
			//	case EventScriptSemantics.ImplType.NotUsableFromScript: {
			//		break;
			//	}
			//	default: {
			//		throw new InvalidOperationException("Invalid event implementation type");
			//	}
			//}
		}

		public override void VisitFieldDeclaration(FieldDeclarationSyntax fieldDeclaration) {
			foreach (var v in fieldDeclaration.Declaration.Variables) {
				var field = _semanticModel.GetDeclaredSymbol(v) as IFieldSymbol;
				if (field == null) {
					_errorReporter.Location = fieldDeclaration.GetLocation();
					_errorReporter.InternalError("Field declaration " + v.Identifier + " does not resolve to a field.");
					return;
				}

				var jsClass = GetJsClass(field.ContainingType);
				if (jsClass == null)
					return;

				var impl = _metadataImporter.GetFieldSemantics(field);
				if (impl.GenerateCode) {
					if (v.Initializer == null) {
						AddDefaultFieldInitializerToType(jsClass, impl.Name, field, field.IsStatic);
					}
					else {
						CompileAndAddFieldInitializerToType(jsClass, impl.Name, field, v.Initializer.Value, field.IsStatic);
					}
				}
			}
		}

		public override void VisitIndexerDeclaration(IndexerDeclarationSyntax indexerDeclaration) {
			var prop = _semanticModel.GetDeclaredSymbol(indexerDeclaration);
			if (prop == null) {
				_errorReporter.Location = indexerDeclaration.GetLocation();
				_errorReporter.InternalError("Indexer declaration does not resolve to a property.");
				return;
			}

			var jsClass = GetJsClass(prop.ContainingType);
			if (jsClass == null)
				return;

			var impl = _metadataImporter.GetPropertySemantics(prop);
			var getter = indexerDeclaration.AccessorList.Accessors.SingleOrDefault(a => a.Keyword.CSharpKind() == SyntaxKind.GetKeyword);
			var setter = indexerDeclaration.AccessorList.Accessors.SingleOrDefault(a => a.Keyword.CSharpKind() == SyntaxKind.SetKeyword);

			switch (impl.Type) {
				case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
					if (getter != null)
						MaybeCompileAndAddMethodToType(jsClass, getter, getter.Body, prop.GetMethod, impl.GetMethod);
					if (setter != null)
						MaybeCompileAndAddMethodToType(jsClass, setter, setter.Body, prop.SetMethod, impl.SetMethod);
					break;
				}
				case PropertyScriptSemantics.ImplType.NotUsableFromScript:
					break;
				default:
					throw new InvalidOperationException("Invalid indexer implementation type " + impl.Type);
			}
		}

		JsExpression IRuntimeContext.ResolveTypeParameter(ITypeParameterSymbol tp) {
			return JsExpression.Identifier(_namer.GetTypeParameterName(tp));
		}

		JsExpression IRuntimeContext.EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			throw new NotSupportedException();
		}
	}
}
