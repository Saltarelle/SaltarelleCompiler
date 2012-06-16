using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
    public interface ICompiler {
        IEnumerable<JsType> Compile(IEnumerable<ISourceFile> sourceFiles, IEnumerable<IAssemblyReference> references);
    }

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

        private readonly INamingConventionResolver _namingConvention;
		private readonly IRuntimeLibrary _runtimeLibrary;
        private readonly IErrorReporter _errorReporter;
        private ICompilation _compilation;
        private CSharpAstResolver _resolver;
        private Dictionary<ITypeDefinition, JsClass> _types;
        private HashSet<ConstructorDeclaration> _constructorDeclarations;
        private Dictionary<JsClass, List<JsStatement>> _instanceInitStatements;

        public event Action<IMethod, JsFunctionDefinitionExpression, MethodCompiler> MethodCompiled;

        private void OnMethodCompiled(IMethod method, JsFunctionDefinitionExpression result, MethodCompiler mc) {
            if (MethodCompiled != null)
                MethodCompiled(method, result, mc);
        }

        public Compiler(INamingConventionResolver namingConvention, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter) {
            _namingConvention = namingConvention;
            _errorReporter    = errorReporter;
        	_runtimeLibrary   = runtimeLibrary;
        }

        private JsClass.ClassTypeEnum ConvertClassType(TypeKind typeKind) {
            switch (typeKind) {
                case TypeKind.Class:     return JsClass.ClassTypeEnum.Class;
                case TypeKind.Interface: return JsClass.ClassTypeEnum.Interface;
                case TypeKind.Struct:    return JsClass.ClassTypeEnum.Struct;
                default: throw new ArgumentException("classType");
            }
        }

        private JsExpression GetJsType(IType type) {
			return _runtimeLibrary.GetScriptType(type, false, _namingConvention);
        }

        private JsClass GetJsClass(ITypeDefinition typeDefinition) {
            JsClass result;
            if (!_types.TryGetValue(typeDefinition, out result)) {
                var semantics = _namingConvention.GetTypeSemantics(typeDefinition);
                if (semantics.GenerateCode) {
                    var baseTypes    = typeDefinition.GetAllBaseTypes().ToList();
					var unusableTypes = Utils.FindUsedUnusableTypes(baseTypes, _namingConvention).ToList();
					if (unusableTypes.Count > 0) {
						foreach (var ut in unusableTypes)
							_errorReporter.Error("Cannot use the type " + ut.FullName + " in the inheritance list for type " + typeDefinition.FullName + " because it is marked as not usable from script.");

						result = new JsClass(typeDefinition, "X", ConvertClassType(typeDefinition.Kind), new string[0], null, null);
					}
					else {
						var baseClass    = typeDefinition.Kind != TypeKind.Interface ? GetJsType(baseTypes.Last(t => !t.GetDefinition().Equals(typeDefinition) && t.Kind == TypeKind.Class)) : null;    // NRefactory bug/feature: Interfaces are reported as having System.Object as their base type.
						var interfaces   = baseTypes.Where(t => !t.GetDefinition().Equals(typeDefinition) && t.Kind == TypeKind.Interface).Select(GetJsType).ToList();
						var typeArgNames = semantics.IgnoreGenericArguments ? null : typeDefinition.TypeParameters.Select(a => _namingConvention.GetTypeParameterName(a)).ToList();
						result = new JsClass(typeDefinition, semantics.Name, ConvertClassType(typeDefinition.Kind), typeArgNames, baseClass, interfaces);
					}
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
            var semantics = _namingConvention.GetTypeSemantics(type);
            var values = new List<JsEnumValue>();
            foreach (var f in type.Fields) {
                if (f.ConstantValue != null) {
					var sem = _namingConvention.GetFieldSemantics(f);
					if (sem.Type == FieldScriptSemantics.ImplType.Field) {
						values.Add(new JsEnumValue(sem.Name, Convert.ToInt64(f.ConstantValue)));
					}
                }
                else {
                    _errorReporter.Error("Enum field " + type.FullName + "." + f.Name + " is not a DefaultResolvedField");
                }
            }

            return semantics.GenerateCode ? new JsEnum(semantics.Name, values) : null;
        }

        private IEnumerable<IType> SelfAndNested(IType type) {
            yield return type;
            foreach (var x in type.GetNestedTypes(options: GetMemberOptions.IgnoreInheritedMembers).SelectMany(c => SelfAndNested(c))) {
                yield return x;
            }
        }

        public IEnumerable<JsType> Compile(IEnumerable<ISourceFile> sourceFiles, IEnumerable<IAssemblyReference> references) {
            IProjectContent project = new CSharpProjectContent();
            var parser = new CSharpParser();
            var files = sourceFiles.Select(f => { 
                                                    using (var rdr = f.Open()) {
                                                        return new { CompilationUnit = parser.Parse(rdr, f.FileName), ParsedFile = new CSharpParsedFile(f.FileName, new UsingScope()) };
                                                    }
                                                }).ToList();

            foreach (var f in files) {
                var tcv = new TypeSystemConvertVisitor(f.ParsedFile);
                f.CompilationUnit.AcceptVisitor(tcv);
                project = project.UpdateProjectContent(null, f.ParsedFile);
            }
            project = project.AddAssemblyReferences(references);

            _compilation = project.CreateCompilation();

			_namingConvention.Prepare(_compilation.GetAllTypeDefinitions(), _compilation.MainAssembly, _errorReporter);

            _types = new Dictionary<ITypeDefinition, JsClass>();
            _constructorDeclarations = new HashSet<ConstructorDeclaration>();
            _instanceInitStatements = new Dictionary<JsClass, List<JsStatement>>();

            foreach (var f in files) {
                _resolver = new CSharpAstResolver(_compilation, f.CompilationUnit, f.ParsedFile);
                _resolver.ApplyNavigator(new ResolveAllNavigator());
                f.CompilationUnit.AcceptVisitor(this);
            }

            // Handle constructors. We must do this after we have visited all the compilation units because field initializer (which change the InstanceInitStatements and StaticInitStatements) might appear anywhere.
            foreach (var n in _constructorDeclarations)
                HandleConstructorDeclaration(n);

            // Add default constructors where needed.
            foreach (var toAdd in _types.Where(t => t.Value != null).SelectMany(kvp => kvp.Key.GetConstructors().Where(c => c.IsSynthetic).Select(c => new { jsClass = kvp.Value, c })))
                MaybeAddDefaultConstructorToType(toAdd.jsClass, toAdd.c);

            _types.Values.Where(t => t != null).ForEach(t => t.Freeze());

            var enums = _compilation.MainAssembly.TopLevelTypeDefinitions.SelectMany(SelfAndNested).Where(t => t.Kind == TypeKind.Enum).Select(t => ConvertEnum(t.GetDefinition()));

            return _types.Values.Cast<JsType>().Concat(enums).Where(t => t != null);
        }

        private MethodCompiler CreateMethodCompiler() {
            return new MethodCompiler(_namingConvention, _errorReporter, _compilation, _resolver, _runtimeLibrary);
        }

        private void AddCompiledMethodToType(JsClass jsClass, IMethod method, MethodScriptSemantics options, JsMethod jsMethod) {
            if ((options.Type == MethodScriptSemantics.ImplType.NormalMethod && method.IsStatic) || options.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
                jsClass.StaticMethods.Add(jsMethod);
            }
            else {
                jsClass.InstanceMethods.Add(jsMethod);
            }
        }

        private void MaybeCompileAndAddMethodToType(JsClass jsClass, EntityDeclaration node, Statement body, IMethod method, MethodScriptSemantics options) {
            if (options.GenerateCode) {
                var typeParamNames = options.IgnoreGenericArguments ? (IEnumerable<string>)new string[0] : method.TypeParameters.Select(tp => _namingConvention.GetTypeParameterName(tp)).ToList();
                var compiled = CompileMethod(node, body, method, options);
                var jsMethod = new JsMethod(options.Name, typeParamNames, compiled);
                AddCompiledMethodToType(jsClass, method, options, jsMethod);
            }
        }

        private void AddCompiledConstructorToType(JsClass jsClass, IMethod constructor, ConstructorScriptSemantics options, JsFunctionDefinitionExpression jsConstructor) {
            switch (options.Type) {
                case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
                    if (jsClass.UnnamedConstructor != null) {
                        _errorReporter.Error("More than one unnamed constructor for " + constructor.DeclaringType.FullName);
                    }
                    else {
                        jsClass.UnnamedConstructor = jsConstructor;
                    }
                    break;
                case ConstructorScriptSemantics.ImplType.NamedConstructor:
                    jsClass.NamedConstructors.Add(new JsNamedConstructor(options.Name, jsConstructor));
                    break;

                case ConstructorScriptSemantics.ImplType.StaticMethod:
                    jsClass.StaticMethods.Add(new JsMethod(options.Name, new string[0], jsConstructor));
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
            var options = _namingConvention.GetConstructorSemantics(constructor);
            if (options.GenerateCode) {
                var mc = CreateMethodCompiler();
                var compiled = mc.CompileDefaultConstructor(constructor, TryGetInstanceInitStatements(jsClass), options);
                OnMethodCompiled(constructor, compiled, mc);
                AddCompiledConstructorToType(jsClass, constructor, options, compiled);
            }
        }

        private JsFunctionDefinitionExpression CompileMethod(EntityDeclaration node, Statement body, IMethod method, MethodScriptSemantics options) {
            var mc = CreateMethodCompiler();
            var result = mc.CompileMethod(node, body, method, options);
            OnMethodCompiled(method, result, mc);
            return result;
        }

        private void CompileAndAddAutoPropertyMethodsToType(JsClass jsClass, IProperty property, PropertyScriptSemantics options, string backingFieldName) {
            if (options.GetMethod.GenerateCode) {
                var compiled = CreateMethodCompiler().CompileAutoPropertyGetter(property, options, backingFieldName);
                AddCompiledMethodToType(jsClass, property.Getter, options.GetMethod, new JsMethod(options.GetMethod.Name, new string[0], compiled));
            }
            if (options.SetMethod.GenerateCode) {
                var compiled = CreateMethodCompiler().CompileAutoPropertySetter(property, options, backingFieldName);
                AddCompiledMethodToType(jsClass, property.Setter, options.SetMethod, new JsMethod(options.SetMethod.Name, new string[0], compiled));
            }
        }

        private void CompileAndAddAutoEventMethodsToType(JsClass jsClass, EventDeclaration node, IEvent evt, EventScriptSemantics options, string backingFieldName) {
            if (options.AddMethod.GenerateCode) {
                var compiled = CreateMethodCompiler().CompileAutoEventAdder(evt, options, backingFieldName);
                AddCompiledMethodToType(jsClass, evt.AddAccessor, options.AddMethod, new JsMethod(options.AddMethod.Name, new string[0], compiled));
            }
            if (options.RemoveMethod.GenerateCode) {
                var compiled = CreateMethodCompiler().CompileAutoEventRemover(evt, options, backingFieldName);
                AddCompiledMethodToType(jsClass, evt.RemoveAccessor, options.RemoveMethod, new JsMethod(options.RemoveMethod.Name, new string[0], compiled));
            }
        }

        private void AddDefaultFieldInitializerToType(JsClass jsClass, string fieldName, IType fieldType, ITypeDefinition owningType, bool isStatic) {
            if (isStatic) {
                jsClass.StaticInitStatements.AddRange(CreateMethodCompiler().CompileDefaultFieldInitializer(JsExpression.MemberAccess(GetJsType(owningType), fieldName), fieldType));
            }
            else {
                AddInstanceInitStatements(jsClass, CreateMethodCompiler().CompileDefaultFieldInitializer(JsExpression.MemberAccess(JsExpression.This, fieldName), fieldType));
            }
        }

        private void CompileAndAddFieldInitializerToType(JsClass jsClass, string fieldName, ITypeDefinition owningType, Expression initializer, bool isStatic) {
            if (isStatic) {
                jsClass.StaticInitStatements.AddRange(CreateMethodCompiler().CompileFieldInitializer(JsExpression.MemberAccess(GetJsType(owningType), fieldName), initializer));
            }
            else {
                AddInstanceInitStatements(jsClass, CreateMethodCompiler().CompileFieldInitializer(JsExpression.MemberAccess(JsExpression.This, fieldName), initializer));
            }
        }

        public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration) {
            if (typeDeclaration.ClassType == ClassType.Class || typeDeclaration.ClassType == ClassType.Interface || typeDeclaration.ClassType == ClassType.Struct) {
                var resolveResult = _resolver.Resolve(typeDeclaration);
                if (!(resolveResult is TypeResolveResult)) {
                    _errorReporter.Error("Type declaration " + typeDeclaration.Name + " does not resolve to a type.");
                    return;
                }
                GetJsClass(resolveResult.Type.GetDefinition());

                base.VisitTypeDeclaration(typeDeclaration);
            }
        }

        public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration) {
            var resolveResult = _resolver.Resolve(methodDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Method declaration " + methodDeclaration.Name + " does not resolve to a member.");
                return;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Method declaration " + methodDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return;
            }

            var jsClass = GetJsClass(method.DeclaringTypeDefinition);
            if (jsClass == null)
                return;

            if (!methodDeclaration.Body.IsNull) {
                MaybeCompileAndAddMethodToType(jsClass, methodDeclaration, methodDeclaration.Body, method, _namingConvention.GetMethodSemantics(method));
            }
        }

        public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration) {
            var resolveResult = _resolver.Resolve(operatorDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Operator declaration " + OperatorDeclaration.GetName(operatorDeclaration.OperatorType) + " does not resolve to a member.");
                return;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Operator declaration " + OperatorDeclaration.GetName(operatorDeclaration.OperatorType) + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return;
            }

            var jsClass = GetJsClass(method.DeclaringTypeDefinition);
            if (jsClass == null)
                return;

            MaybeCompileAndAddMethodToType(jsClass, operatorDeclaration, operatorDeclaration.Body, method, _namingConvention.GetMethodSemantics(method));
        }

        private void HandleConstructorDeclaration(ConstructorDeclaration constructorDeclaration) {
            var resolveResult = _resolver.Resolve(constructorDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Method declaration " + constructorDeclaration.Name + " does not resolve to a member.");
                return;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Method declaration " + constructorDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return;
            }

            var jsClass = GetJsClass(method.DeclaringTypeDefinition);
            if (jsClass == null)
                return;

            if (method.IsStatic) {
                jsClass.StaticInitStatements.AddRange(CompileMethod(constructorDeclaration, constructorDeclaration.Body, method, MethodScriptSemantics.NormalMethod("X")).Body.Statements);
            }
            else {
                MaybeCompileAndAddConstructorToType(jsClass, constructorDeclaration, method, _namingConvention.GetConstructorSemantics(method));
            }
        }


        public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration) {
            _constructorDeclarations.Add(constructorDeclaration);
        }

        public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration) {
            var resolveResult = _resolver.Resolve(propertyDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Property declaration " + propertyDeclaration.Name + " does not resolve to a member.");
                return;
            }

            var property = ((MemberResolveResult)resolveResult).Member as IProperty;
            if (property == null) {
                _errorReporter.Error("Property declaration " + propertyDeclaration.Name + " does not resolve to a property (resolves to " + resolveResult.ToString() + ")");
                return;
            }

            var jsClass = GetJsClass(property.DeclaringTypeDefinition);
            if (jsClass == null)
                return;

            var impl = _namingConvention.GetPropertySemantics(property);

            switch (impl.Type) {
                case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
                    if (propertyDeclaration.Getter.Body.IsNull && propertyDeclaration.Setter.Body.IsNull) {
                        // Auto-property.
                        if ((impl.GetMethod != null && impl.GetMethod.GenerateCode) || (impl.SetMethod != null && impl.SetMethod.GenerateCode)) {
                            var fieldName = _namingConvention.GetAutoPropertyBackingFieldName(property);
                            AddDefaultFieldInitializerToType(jsClass, fieldName, property.ReturnType, property.DeclaringTypeDefinition, property.IsStatic);
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
                    AddDefaultFieldInitializerToType(jsClass, impl.FieldName, property.ReturnType, property.DeclaringTypeDefinition, property.IsStatic);
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
                    _errorReporter.Error("Event declaration " + singleEvt.Name + " does not resolve to a member.");
                    return;
                }

                var evt = ((MemberResolveResult)resolveResult).Member as IEvent;
                if (evt == null) {
                    _errorReporter.Error("Event declaration " + singleEvt.Name + " does not resolve to an event (resolves to " + resolveResult.ToString() + ")");
                    return;
                }

                var jsClass = GetJsClass(evt.DeclaringTypeDefinition);
                if (jsClass == null)
                    return;

                var impl = _namingConvention.GetEventSemantics(evt);
                switch (impl.Type) {
                    case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
                        if ((impl.AddMethod != null && impl.AddMethod.GenerateCode) || (impl.RemoveMethod != null && impl.RemoveMethod.GenerateCode)) {
                            var fieldName = _namingConvention.GetAutoEventBackingFieldName(evt);
                            if (singleEvt.Initializer.IsNull) {
                                AddDefaultFieldInitializerToType(jsClass, fieldName, evt.ReturnType, evt.DeclaringTypeDefinition, evt.IsStatic);
                            }
                            else {
                                CompileAndAddFieldInitializerToType(jsClass, fieldName, evt.DeclaringTypeDefinition, singleEvt.Initializer, evt.IsStatic);
                            }

                            CompileAndAddAutoEventMethodsToType(jsClass, eventDeclaration, evt, impl, fieldName);
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
                _errorReporter.Error("Event declaration " + eventDeclaration.Name + " does not resolve to a member.");
                return;
            }

            var evt = ((MemberResolveResult)resolveResult).Member as IEvent;
            if (evt == null) {
                _errorReporter.Error("Event declaration " + eventDeclaration.Name + " does not resolve to an event (resolves to " + resolveResult.ToString() + ")");
                return;
            }

            var jsClass = GetJsClass(evt.DeclaringTypeDefinition);
            if (jsClass == null)
                return;

            var impl = _namingConvention.GetEventSemantics(evt);

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
                    _errorReporter.Error("Field declaration " + v.Name + " does not resolve to a member.");
                    return;
                }

                var field = ((MemberResolveResult)resolveResult).Member as IField;
                if (field == null) {
                    _errorReporter.Error("Field declaration " + v.Name + " does not resolve to a field (resolves to " + resolveResult.ToString() + ")");
                    return;
                }

                var jsClass = GetJsClass(field.DeclaringTypeDefinition);
                if (jsClass == null)
                    return;

                var impl = _namingConvention.GetFieldSemantics(field);

                switch (impl.Type) {
                    case FieldScriptSemantics.ImplType.Field:
                        if (v.Initializer.IsNull) {
                            AddDefaultFieldInitializerToType(jsClass, impl.Name, field.ReturnType, field.DeclaringTypeDefinition, field.IsStatic);
                        }
                        else {
                            CompileAndAddFieldInitializerToType(jsClass, impl.Name, field.DeclaringTypeDefinition, v.Initializer, field.IsStatic);
                        }
                        break;

                    case FieldScriptSemantics.ImplType.NotUsableFromScript:
                        break;

                    default:
                        throw new InvalidOperationException("Invalid field implementation type " + impl.Type);
                }
            }
        }

        public override void VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration) {
            var resolveResult = _resolver.Resolve(indexerDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Event declaration " + indexerDeclaration.Name + " does not resolve to a member.");
                return;
            }

            var prop = ((MemberResolveResult)resolveResult).Member as IProperty;
            if (prop == null) {
                _errorReporter.Error("Event declaration " + indexerDeclaration.Name + " does not resolve to a property (resolves to " + resolveResult.ToString() + ")");
                return;
            }

            var jsClass = GetJsClass(prop.DeclaringTypeDefinition);
            if (jsClass == null)
                return;

            var impl = _namingConvention.GetPropertySemantics(prop);

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
