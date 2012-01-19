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

    public class Compiler : DepthFirstAstVisitor<object, object>, ICompiler {
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
        private readonly IErrorReporter _errorReporter;
        private ICompilation _compilation;
        private CSharpAstResolver _resolver;
        private Dictionary<IType, JsType> _types;
        private Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>> _methodMap;
        private Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>> _defaultConstructors;

        public Compiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
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

        private bool IsTypePublic(ITypeDefinition type) {
            // A type is public if the type and all its declaring types are public or protected (or protected internal).
            while (type != null) {
                bool isPublic = (type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Protected || type.Accessibility == Accessibility.ProtectedOrInternal);
                if (!isPublic)
                    return false;
                type = type.DeclaringTypeDefinition;
            }
            return true;
        }

        private JsEnum ConvertEnum(ITypeDefinition type) {
            var name = ConvertName(type);
            return name != null ? new JsEnum(name) : null;
        }

        private JsConstructedType ConvertPotentiallyGenericType(IType type) {
            if (type is ITypeParameter)
                return new JsConstructedType(new JsIdentifierExpression(_namingConvention.GetTypeParameterName((ITypeParameter)type)));

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

        private Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> ConvertConstructor(IMethod ctor, List<JsConstructor> constructors, List<JsMethod> staticMethods) {
            var impl = _namingConvention.GetConstructorImplementation(ctor);
            if (!impl.GenerateCode)
                return null;

            switch (impl.Type) {
                case ConstructorImplOptions.ImplType.UnnamedConstructor: {
                    var result = new JsConstructor(null);
                    if (ctor.IsSynthetic)
                        result.Definition = CompileDefaultConstructorWithoutImplementation(ctor, MethodCompilationOptions.Constructor);
                    constructors.Add(result);
                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.Constructor);
                }
                case ConstructorImplOptions.ImplType.NamedConstructor: {
                    var result = new JsConstructor(impl.Name);
                    if (ctor.IsSynthetic)
                        result.Definition = CompileDefaultConstructorWithoutImplementation(ctor, MethodCompilationOptions.Constructor);
                    constructors.Add(result);
                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.Constructor);
                }
                case ConstructorImplOptions.ImplType.StaticMethod: {
                    var result = new JsMethod(impl.Name, null);
                    if (ctor.IsSynthetic)
                        result.Definition = CompileDefaultConstructorWithoutImplementation(ctor, MethodCompilationOptions.ConstructorAsStaticMethod);
                    staticMethods.Add(result);
                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.ConstructorAsStaticMethod);
                }
                default:
                    throw new ArgumentException("ctor");
            }
        }

        private Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> ConvertMethod(IMethod method, List<JsMethod> instanceMethods, List<JsMethod> staticMethods) {
            var impl = _namingConvention.GetMethodImplementation(method);
            if (!impl.GenerateCode)
                return null;

            var typeParamNames = impl.IgnoreGenericArguments ? (IEnumerable<string>)new string[0] : method.TypeParameters.Select(tp => _namingConvention.GetTypeParameterName(tp)).ToList();

            switch (impl.Type) {
                case MethodImplOptions.ImplType.InstanceMethod: {
                    var result = new JsMethod(impl.Name, typeParamNames);
                    instanceMethods.Add(result);
                    instanceMethods.AddRange(impl.AdditionalNames.Select(an => new JsMethod(an, typeParamNames) {Definition = CompileDelegatingMethod(method)}));

                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.InstanceMethod);
                }
                case MethodImplOptions.ImplType.StaticMethod: {
                    var result = new JsMethod(impl.Name, typeParamNames);
                    staticMethods.Add(result);
                    staticMethods.AddRange(impl.AdditionalNames.Select(an => new JsMethod(an, typeParamNames) {Definition = CompileDelegatingMethod(method)}));
                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.StaticMethod);
                }
                default:
                    throw new ArgumentException("ctor");
            }
        }

        private Tuple<IEnumerable<JsConstructor>, IEnumerable<JsMethod>, IEnumerable<JsMethod>> ConvertMembers(IType type) {
            var constructors    = new List<JsConstructor>();
            var instanceMethods = new List<JsMethod>();
            var staticMethods   = new List<JsMethod>();

            foreach (var c in type.GetConstructors()) {
                var def  = ConvertConstructor(c, constructors: constructors, staticMethods: staticMethods);
                if (def != null) {
                    _methodMap.Add(c, def);
                    if (type.Kind == TypeKind.Class && c.Parameters.Count == 0)
                        _defaultConstructors.Add(c, def);
                }
            }

            foreach (var m in type.GetMethods(options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
                var def  = ConvertMethod(m, instanceMethods: instanceMethods, staticMethods: staticMethods);
                if (def != null)
                    _methodMap.Add(m, def);
            }

            foreach (var m in type.GetProperties(options: GetMemberOptions.IgnoreInheritedMembers)) {
            }

            foreach (var m in type.GetEvents(options: GetMemberOptions.IgnoreInheritedMembers)) {
            }

            foreach (var m in type.GetFields(options: GetMemberOptions.IgnoreInheritedMembers)) {
            }

            return Tuple.Create((IEnumerable<JsConstructor>)constructors, (IEnumerable<JsMethod>)instanceMethods, (IEnumerable<JsMethod>)staticMethods);
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

            return new JsClass(name, ConvertClassType(type.Kind), typeArgNames, baseClass, interfaces, constructors: members.Item1, instanceMethods: members.Item2, staticMethods: members.Item3, instanceFields: null, staticFields: null);
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
            _methodMap           = new Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>>();
            _defaultConstructors = new Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>>();

            CreateTypes();

            foreach (var f in files) {
                _resolver = new CSharpAstResolver(_compilation, f.CompilationUnit, f.ParsedFile);
                _resolver.ApplyNavigator(new ResolveAllNavigator());
                f.CompilationUnit.AcceptVisitor(this);
            }

            _types.Values.ForEach(t => t.Freeze());

            _methodMap.Where(kvp => kvp.Value.Item1.Definition == null)
                      .ForEach(kvp => _errorReporter.Error("Member " + kvp.Key.ToString() + " does not have an implementation."));

            return _types.Values;
        }

        private JsFunctionDefinitionExpression CompileDelegatingMethod(IMethod method) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        private JsFunctionDefinitionExpression CompileDefaultConstructorWithoutImplementation(IMethod method, MethodCompilationOptions options) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        private JsFunctionDefinitionExpression CompileMethod(AttributedNode method, MethodCompilationOptions options) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        private JsFunctionDefinitionExpression CompileAutoPropertyGetter(IMethod method, FieldOptions backingField) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        private JsFunctionDefinitionExpression CompileAutoPropertySetter(IMethod method, FieldOptions backingField) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        private JsExpression CreateDefaultInitializer(IType type) {
            // TODO
            return JsExpression.Number(0);
        }

        public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data) {
            var resolveResult = _resolver.Resolve(methodDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Method declaration " + methodDeclaration.Name + " does not resolve to a member.");
                return null;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Method declaration " + methodDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return null;
            }

            Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(methodDeclaration, jsMethod.Item2);
            }

            return null;
        }

        public override object VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration, object data) {
            var resolveResult = _resolver.Resolve(operatorDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Operator declaration " + OperatorDeclaration.GetName(operatorDeclaration.OperatorType) + " does not resolve to a member.");
                return null;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Operator declaration " + OperatorDeclaration.GetName(operatorDeclaration.OperatorType) + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return null;
            }

            Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(operatorDeclaration, jsMethod.Item2);
            }

            return null;
        }

        public override object VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, object data) {
            var resolveResult = _resolver.Resolve(constructorDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Method declaration " + constructorDeclaration.Name + " does not resolve to a member.");
                return null;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Method declaration " + constructorDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return null;
            }

            Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(constructorDeclaration, jsMethod.Item2);
            }

            return null;
        }

        public override object VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, object data) {
            var resolveResult = _resolver.Resolve(propertyDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Property declaration " + propertyDeclaration.Name + " does not resolve to a member.");
                return null;
            }

            var property = ((MemberResolveResult)resolveResult).Member as IProperty;
            if (property == null) {
                _errorReporter.Error("Property declaration " + propertyDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString() + ")");
                return null;
            }

            var impl = _namingConvention.GetPropertyImplementation(property);

            if (impl.Type == PropertyImplOptions.ImplType.GetAndSetMethods) {
                if (propertyDeclaration.Getter.Body == null) {
                    // Auto-property.
                    var fieldImpl = _namingConvention.GetAutoPropertyBackingFieldImplementation(property);
                    if (fieldImpl.Type != FieldOptions.ImplType.NotUsableFromScript) {
                        var field = new JsField(fieldImpl.Name, CreateDefaultInitializer(property.ReturnType));
                        if (fieldImpl.Type == FieldOptions.ImplType.Instance) {
                            ((JsClass)_types[property.DeclaringTypeDefinition]).InstanceFields.Add(field);
                        }
                        else if (fieldImpl.Type == FieldOptions.ImplType.Static) {
                            ((JsClass)_types[property.DeclaringTypeDefinition]).StaticFields.Add(field);
                        }
                        else {
                            _errorReporter.Error("Invalid field type");
                        }
                    }

                    Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> jsMethod;
                    if (_methodMap.TryGetValue(property.Getter, out jsMethod)) {
                        jsMethod.Item1.Definition = CompileAutoPropertyGetter(property.Getter, fieldImpl);
                    }
                    if (_methodMap.TryGetValue(property.Getter, out jsMethod)) {
                        jsMethod.Item1.Definition = CompileAutoPropertySetter(property.Setter, fieldImpl);
                    }
                }
                else {
                    // Manual property: TODO
                }
            }

            return null;
        }
    }

    public enum MethodCompilationOptions {
        InstanceMethod,
        StaticMethod,
        Constructor,
        ConstructorAsStaticMethod
    }
}
