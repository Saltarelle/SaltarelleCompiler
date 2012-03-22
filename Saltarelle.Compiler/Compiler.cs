using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
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
        private Dictionary<IType, JsType> _types;
        private Dictionary<IMethod, Tuple<JsMethod, MethodImplOptions>> _methodMap;
        private Dictionary<IMethod, Tuple<JsConstructor, JsMethod, ConstructorImplOptions>> _constructorMap;
        private Dictionary<IField, JsField> _fieldMap;

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

        private ScopedName ConvertName(ITypeDefinition type) {
            var name = _namingConvention.GetTypeName(type);
            if (name == null) {
                return null;
            }
            else if (type.DeclaringType == null) {
                return ScopedName.Global(!string.IsNullOrEmpty(type.Namespace) ? type.Namespace : null, name);
            }
            else {
                var declaringName = ConvertName(type.DeclaringType.GetDefinition());
                if (declaringName == null)
                    return null;
                else
                    return ScopedName.Nested(declaringName, name);
            }
        }

        private JsEnum ConvertEnum(ITypeDefinition type) {
            var name = ConvertName(type);
            var values = new List<JsEnumValue>();
            foreach (var f in type.Fields) {
                if (f.ConstantValue != null) {
                    values.Add(new JsEnumValue(_namingConvention.GetEnumValueName(f), Convert.ToInt64(f.ConstantValue)));
                }
                else {
                    _errorReporter.Error("Enum field " + type.FullName + "." + f.Name + " is not a DefaultResolvedField");
                }
            }

            return name != null ? new JsEnum(name, values) : null;
        }

        private JsConstructedType ConvertPotentiallyGenericType(IType type) {
            if (type is ITypeParameter)
                return new JsConstructedType(JsExpression.Identifier(_namingConvention.GetTypeParameterName((ITypeParameter)type)));

            var unconstructed = new JsTypeReferenceExpression(type.GetDefinition());
            if (type is ParameterizedType)
                return new JsConstructedType(unconstructed, ((ParameterizedType)type).TypeArguments.Select(ConvertPotentiallyGenericType));
            else
                return new JsConstructedType(unconstructed);
        }

        private JsClass.ClassTypeEnum ConvertClassType(TypeKind typeKind) {
            switch (typeKind) {
                case TypeKind.Class:     return JsClass.ClassTypeEnum.Class;
                case TypeKind.Interface: return JsClass.ClassTypeEnum.Interface;
                case TypeKind.Struct:    return JsClass.ClassTypeEnum.Struct;
                default: throw new ArgumentException("classType");
            }
        }

        private Tuple<JsMethod, MethodImplOptions> ConvertMethod(MethodImplOptions impl, IMethod method, List<JsMethod> instanceMethods, List<JsMethod> staticMethods) {
            if (!impl.GenerateCode)
                return null;

            var typeParamNames = impl.IgnoreGenericArguments ? (IEnumerable<string>)new string[0] : method.TypeParameters.Select(tp => _namingConvention.GetTypeParameterName(tp)).ToList();

            switch (impl.Type) {
                case MethodImplOptions.ImplType.NormalMethod: {
                    var result = new JsMethod(impl.Name, typeParamNames);
					var list = (method.IsStatic ? staticMethods : instanceMethods);
                    list.Add(result);
                    list.AddRange(impl.AdditionalNames.Select(an => new JsMethod(an, typeParamNames) { Definition = CompileDelegatingMethod(method) }));

                    return Tuple.Create(result, impl);
                }
                case MethodImplOptions.ImplType.StaticMethodWithThisAsFirstArgument: {
                    var result = new JsMethod(impl.Name, typeParamNames);
					staticMethods.Add(result);
					return Tuple.Create(result, impl);
				}
                default:
                    throw new ArgumentException("method");
            }
        }

        private Tuple<IEnumerable<JsConstructor>, IEnumerable<JsMethod>, IEnumerable<JsMethod>, IEnumerable<JsField>, IEnumerable<JsField>> ConvertMembers(IType type) {
            var constructors    = new List<JsConstructor>();
            var instanceMethods = new List<JsMethod>();
            var staticMethods   = new List<JsMethod>();
            var instanceFields  = new List<JsField>();
            var staticFields    = new List<JsField>();

            foreach (var c in type.GetConstructors()) {
                var impl = _namingConvention.GetConstructorImplementation(c);
                if (!impl.GenerateCode)
                    continue;

                switch (impl.Type) {
                    case ConstructorImplOptions.ImplType.UnnamedConstructor:
                    case ConstructorImplOptions.ImplType.NamedConstructor: {
                        var def = new JsConstructor(impl.Type == ConstructorImplOptions.ImplType.NamedConstructor ? impl.Name : null);
                        if (c.IsSynthetic)
                            def.Definition = CompileDefaultConstructorWithoutImplementation(c, impl);
                        constructors.Add(def);
                        _constructorMap.Add(c, Tuple.Create(def, (JsMethod)null, impl));
                        break;
                    }
                    case ConstructorImplOptions.ImplType.StaticMethod: {
                        var def = new JsMethod(impl.Name, null);
                        if (c.IsSynthetic)
                            def.Definition = CompileDefaultConstructorWithoutImplementation(c, impl);
                        staticMethods.Add(def);
                        _constructorMap.Add(c, Tuple.Create((JsConstructor)null, def, impl));
                        break;
                    }
                    default:
                        throw new ArgumentException("ctor");
                }
            }

            foreach (var m in type.GetMethods(options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
                var def  = ConvertMethod(_namingConvention.GetMethodImplementation(m), m, instanceMethods: instanceMethods, staticMethods: staticMethods);
                if (def != null)
                    _methodMap.Add(m, def);
            }

            foreach (var p in type.GetProperties(options: GetMemberOptions.IgnoreInheritedMembers)) {
                var impl = _namingConvention.GetPropertyImplementation(p);
                switch (impl.Type) {
                    case PropertyImplOptions.ImplType.GetAndSetMethods: {
                        if (p.CanGet) {
                            var def = ConvertMethod(impl.GetMethod, p.Getter, instanceMethods: instanceMethods, staticMethods: staticMethods);
                            if (def != null)
                                _methodMap.Add(p.Getter, def);
                        }
                        if (p.CanSet) {
                            var def = ConvertMethod(impl.SetMethod, p.Setter, instanceMethods: instanceMethods, staticMethods: staticMethods);
                            if (def != null)
                                _methodMap.Add(p.Setter, def);
                        }
                        break;
                    }
                    case PropertyImplOptions.ImplType.Field: {
                        var field = new JsField(impl.FieldName, CreateDefaultInitializer(p.ReturnType));
                        (p.IsStatic ? staticFields : instanceFields).Add(field);
                        break;
                    }
                    case PropertyImplOptions.ImplType.NotUsableFromScript:
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Invalid property implementation type {0}", impl.Type));
                }
            }

            foreach (var f in type.GetFields(options: GetMemberOptions.IgnoreInheritedMembers)) {
                var impl = _namingConvention.GetFieldImplementation(f);
                switch (impl.Type) {
                    case FieldImplOptions.ImplType.Field: {
                        var jsf = new JsField(impl.Name);
                        (f.IsStatic ? staticFields : instanceFields).Add(jsf);
                        _fieldMap.Add(f, jsf);
                        break;
                    }
                    case FieldImplOptions.ImplType.NotUsableFromScript:
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Invalid field implementation type {0}", impl.Type));
                }
            }

            foreach (var e in type.GetEvents(options: GetMemberOptions.IgnoreInheritedMembers)) {
                var impl = _namingConvention.GetEventImplementation(e);
                switch (impl.Type) {
                    case EventImplOptions.ImplType.AddAndRemoveMethods: {
                        var add = ConvertMethod(impl.AddMethod, e.AddAccessor, instanceMethods: instanceMethods, staticMethods: staticMethods);
                        if (add != null)
                            _methodMap.Add(e.AddAccessor, add);
                        var remove = ConvertMethod(impl.RemoveMethod, e.RemoveAccessor, instanceMethods: instanceMethods, staticMethods: staticMethods);
                        if (add != null)
                            _methodMap.Add(e.RemoveAccessor, remove);
                        break;
                    }
                    case EventImplOptions.ImplType.NotUsableFromScript:
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Invalid event implementation type {0}", impl.Type));
                }
            }

            return Tuple.Create((IEnumerable<JsConstructor>)constructors, (IEnumerable<JsMethod>)instanceMethods, (IEnumerable<JsMethod>)staticMethods, (IEnumerable<JsField>)instanceFields, (IEnumerable<JsField>)staticFields);
        }

        private JsClass ConvertClass(ITypeDefinition type) {
            var name = ConvertName(type);
            if (name == null)
                return null;

            var baseTypes    = type.GetAllBaseTypes().ToList();
            var baseClass    = type.Kind != TypeKind.Interface ? ConvertPotentiallyGenericType(baseTypes.Last(t => t != type && t.Kind == TypeKind.Class)) : null;    // NRefactory bug/feature: Interfaces are reported as having System.Object as their base type.
            var interfaces   = baseTypes.Where(t => t != type && t.Kind == TypeKind.Interface).Select(ConvertPotentiallyGenericType).ToList();
            var typeArgNames = type.TypeParameters.Select(a => _namingConvention.GetTypeParameterName(a)).ToList();

            var members = ConvertMembers(type);

            return new JsClass(name, ConvertClassType(type.Kind), typeArgNames, baseClass, interfaces, constructors: members.Item1, instanceMethods: members.Item2, staticMethods: members.Item3, instanceFields: members.Item4, staticFields: members.Item5);
        }

        private IEnumerable<IType> SelfAndNested(IType type) {
            yield return type;
            foreach (var x in type.GetNestedTypes(options: GetMemberOptions.IgnoreInheritedMembers).SelectMany(c => SelfAndNested(c))) {
                yield return x;
            }
        }

        private void CreateTypes() {
            foreach (var type in _compilation.MainAssembly.TopLevelTypeDefinitions.SelectMany(SelfAndNested).Select(c => c.GetDefinition())) {
                switch (type.Kind) {
                    case TypeKind.Class:
                    case TypeKind.Struct:
                    case TypeKind.Interface: {
                        var t = ConvertClass(type);
                        if (t != null)
                            _types[type] = t;
                        break;
                    }
                    case TypeKind.Enum: {
                        var t = ConvertEnum(type);
                        if (t != null)
                            _types[type] = t;
                        break;
                    }
                }
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

            _types               = new Dictionary<IType, JsType>();
            _methodMap           = new Dictionary<IMethod, Tuple<JsMethod, MethodImplOptions>>();
            _constructorMap      = new Dictionary<IMethod, Tuple<JsConstructor, JsMethod, ConstructorImplOptions>>();
            _fieldMap            = new Dictionary<IField, JsField>();

            CreateTypes();

            foreach (var f in files) {
                _resolver = new CSharpAstResolver(_compilation, f.CompilationUnit, f.ParsedFile);
                _resolver.ApplyNavigator(new ResolveAllNavigator());
                f.CompilationUnit.AcceptVisitor(this);
            }

            _types.Values.ForEach(t => t.Freeze());

            _methodMap.Where(kvp => kvp.Value.Item1.Definition == null)
                      .ForEach(kvp => _errorReporter.Error("Member " + kvp.Key.ToString() + " does not have an implementation."));

            _fieldMap.Where(kvp => kvp.Value.Initializer == null)
                      .ForEach(kvp => _errorReporter.Error("Field " + kvp.Key.ToString() + " does not have an initializer."));

            return _types.Values;
        }

        private JsFunctionDefinitionExpression CompileDelegatingMethod(IMethod method) {
            // BIG TODO.
            return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
        }

        private JsFunctionDefinitionExpression CompileDefaultConstructorWithoutImplementation(IMethod method, ConstructorImplOptions options) {
            // BIG TODO.
            return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
        }

        private JsFunctionDefinitionExpression CompileMethod(EntityDeclaration node, Statement body, IMethod method, MethodImplOptions options) {
            var mc = new MethodCompiler(_namingConvention, _errorReporter, _compilation, _resolver, _runtimeLibrary);
            var result = mc.CompileMethod(node, body, method, options);
            OnMethodCompiled(method, result, mc);
            return result;
        }

        private JsFunctionDefinitionExpression CompileConstructor(ConstructorDeclaration node, IMethod method, ConstructorImplOptions options) {
            var mc = new MethodCompiler(_namingConvention, _errorReporter, _compilation, _resolver, _runtimeLibrary);
            var result = mc.CompileConstructor(node, method, options);
            OnMethodCompiled(method, result, mc);
            return result;
        }

        private JsFunctionDefinitionExpression CompileAutoPropertyGetter(IProperty property, FieldImplOptions backingField) {
            // BIG TODO.
            return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
        }

        private JsFunctionDefinitionExpression CompileAutoPropertySetter(IProperty property, FieldImplOptions backingField) {
            // BIG TODO.
            return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
        }

        private JsFunctionDefinitionExpression CompileAutoEventAdder(IEvent evt, FieldImplOptions backingField) {
            // BIG TODO.
            return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
        }

        private JsFunctionDefinitionExpression CompileAutoEventRemover(IEvent evt, FieldImplOptions backingField) {
            // BIG TODO.
            return JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);
        }

        private JsExpression CreateDefaultInitializer(IType type) {
            // TODO
            return JsExpression.Number(0);
        }

        private JsExpression CompileInitializer(Expression initializer) {
            // TODO
            return JsExpression.Number(0);
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

            Tuple<JsMethod, MethodImplOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(methodDeclaration, methodDeclaration.Body, method, jsMethod.Item2);
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

            Tuple<JsMethod, MethodImplOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(operatorDeclaration, operatorDeclaration.Body, method, jsMethod.Item2);
            }
        }

        public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration) {
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

            Tuple<JsConstructor, JsMethod, ConstructorImplOptions> jsConstructor;
            if (_constructorMap.TryGetValue(method, out jsConstructor)) {
                if (jsConstructor.Item1 != null)
                    jsConstructor.Item1.Definition = CompileConstructor(constructorDeclaration, method, jsConstructor.Item3);
                else
                    jsConstructor.Item2.Definition = CompileConstructor(constructorDeclaration, method, jsConstructor.Item3);
            }
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

            var impl = _namingConvention.GetPropertyImplementation(property);

            if (impl.Type == PropertyImplOptions.ImplType.GetAndSetMethods) {
                if (propertyDeclaration.Getter.Body.IsNull && propertyDeclaration.Setter.Body.IsNull) {
                    // Auto-property.
                    var fieldImpl = _namingConvention.GetAutoPropertyBackingFieldImplementation(property);
                    if (fieldImpl.Type != FieldImplOptions.ImplType.NotUsableFromScript) {
                        var field = new JsField(fieldImpl.Name, CreateDefaultInitializer(property.ReturnType));
                        if (fieldImpl.Type == FieldImplOptions.ImplType.Field) {
							if (property.IsStatic) {
	                            ((JsClass)_types[property.DeclaringTypeDefinition]).StaticFields.Add(field);
							}
							else {
								((JsClass)_types[property.DeclaringTypeDefinition]).InstanceFields.Add(field);
							}
                        }
                        else {
                            _errorReporter.Error("Invalid field type");
                        }
                    }

                    Tuple<JsMethod, MethodImplOptions> jsMethod;
                    if (property.Getter != null && _methodMap.TryGetValue(property.Getter, out jsMethod)) {
                        jsMethod.Item1.Definition = CompileAutoPropertyGetter(property, fieldImpl);
                    }
                    if (property.Setter != null && _methodMap.TryGetValue(property.Setter, out jsMethod)) {
                        jsMethod.Item1.Definition = CompileAutoPropertySetter(property, fieldImpl);
                    }
                }
                else {
                    if (!propertyDeclaration.Getter.IsNull) {
                        Tuple<JsMethod, MethodImplOptions> jsMethod;
                        if (_methodMap.TryGetValue(property.Getter, out jsMethod)) {
                            jsMethod.Item1.Definition = CompileMethod(propertyDeclaration.Getter, propertyDeclaration.Getter.Body, property.Getter, jsMethod.Item2);
                        }
                    }

                    if (!propertyDeclaration.Setter.IsNull) {
                        Tuple<JsMethod, MethodImplOptions> jsMethod;
                        if (_methodMap.TryGetValue(property.Setter, out jsMethod)) {
                            jsMethod.Item1.Definition = CompileMethod(propertyDeclaration.Setter, propertyDeclaration.Setter.Body, property.Setter, jsMethod.Item2);
                        }
                    }
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

                var impl = _namingConvention.GetEventImplementation(evt);
                switch (impl.Type) {
                    case EventImplOptions.ImplType.AddAndRemoveMethods:
                        var fieldImpl = _namingConvention.GetAutoEventBackingFieldImplementation(evt);
                        if (fieldImpl.Type != FieldImplOptions.ImplType.NotUsableFromScript) {
                            var field = new JsField(fieldImpl.Name, singleEvt.Initializer != null ? CompileInitializer(singleEvt.Initializer) : CreateDefaultInitializer(evt.ReturnType));
                            if (fieldImpl.Type == FieldImplOptions.ImplType.Field) {
								if (evt.IsStatic) {
									((JsClass)_types[evt.DeclaringTypeDefinition]).StaticFields.Add(field);
								}
								else {
	                                ((JsClass)_types[evt.DeclaringTypeDefinition]).InstanceFields.Add(field);
								}
                            }
                            else {
                                _errorReporter.Error("Invalid field type");
                            }
                        }
                        Tuple<JsMethod, MethodImplOptions> jsMethod;
                        if (_methodMap.TryGetValue(evt.AddAccessor, out jsMethod)) {
                            jsMethod.Item1.Definition = CompileAutoEventAdder(evt, fieldImpl);
                        }
                        if (_methodMap.TryGetValue(evt.RemoveAccessor, out jsMethod)) {
                            jsMethod.Item1.Definition = CompileAutoEventRemover(evt, fieldImpl);
                        }
                        break;

                    case EventImplOptions.ImplType.NotUsableFromScript:
                        break;

                    default:
                        throw new InvalidOperationException("Invalid event implementation type");
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

            Tuple<JsMethod, MethodImplOptions> jsMethod;
            if (_methodMap.TryGetValue(evt.AddAccessor, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(eventDeclaration.AddAccessor, eventDeclaration.AddAccessor.Body, evt.AddAccessor, jsMethod.Item2);
            }
            if (_methodMap.TryGetValue(evt.RemoveAccessor, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(eventDeclaration.RemoveAccessor, eventDeclaration.RemoveAccessor.Body, evt.RemoveAccessor, jsMethod.Item2);
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

                JsField jsField;
                if (_fieldMap.TryGetValue(field, out jsField))
                    jsField.Initializer = (v.Initializer != null ? CompileInitializer(v.Initializer) : CreateDefaultInitializer(field.Type));
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

            Tuple<JsMethod, MethodImplOptions> jsMethod;
            if (prop.Getter != null && _methodMap.TryGetValue(prop.Getter, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(indexerDeclaration.Getter, indexerDeclaration.Getter.Body, prop.Getter, jsMethod.Item2);
            }
            if (prop.Setter != null && _methodMap.TryGetValue(prop.Setter, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(indexerDeclaration.Setter, indexerDeclaration.Setter.Body, prop.Setter, jsMethod.Item2);
            }
        }
    }
}
