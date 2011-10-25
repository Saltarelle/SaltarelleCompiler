using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
    public interface ICompiler {
        IEnumerable<JsType> Compile(IEnumerable<ISourceFile> sourceFiles, IEnumerable<ITypeResolveContext> references);
    }

    public class Compiler : DepthFirstAstVisitor<object, object>, ICompiler {
        private class ResolveAllNavigator : IResolveVisitorNavigator {
            public ResolveVisitorNavigationMode Scan(AstNode node) {
                return ResolveVisitorNavigationMode.Scan;
            }

            public void Resolved(AstNode node, ResolveResult result) {
            }

            public void ProcessConversion(Expression expression, ResolveResult result, Conversion conversion, IType targetType) {
            }
        }

        private readonly INamingConventionResolver _namingConvention;
        private readonly IErrorReporter _errorReporter;
        private ITypeResolveContext _typeResolveContext;
        private SimpleProjectContent _project;
        private Dictionary<ITypeDefinition, JsType> _types;
        private ResolveVisitor _resolver;
        private Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>> _methodMap;
        private Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>> _defaultConstructors;

        public Compiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter) {
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
        }

        private ScopedName ConvertName(ITypeDefinition type) {
            var name = _namingConvention.GetTypeName(_typeResolveContext, type);
            if (name == null) {
                return null;
            }
            else if (type.DeclaringType == null) {
                return ScopedName.Global(!string.IsNullOrEmpty(type.Namespace) ? type.Namespace : null, name);
            }
            else {
                var declaringName = ConvertName(type.DeclaringTypeDefinition);
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
            return name != null ? new JsEnum(name, IsTypePublic(type)) : null;
        }

        private JsConstructedType ConvertPotentiallyGenericType(IType type) {
            if (type is ITypeParameter)
                return new JsConstructedType(new JsIdentifierExpression(_namingConvention.GetTypeParameterName(_typeResolveContext, (ITypeParameter)type)));

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
            var impl = _namingConvention.GetConstructorImplementation(_typeResolveContext, ctor);
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
                    var result = new JsMethod(impl.Name);
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
            var impl = _namingConvention.GetMethodImplementation(_typeResolveContext, method);
            if (!impl.GenerateCode)
                return null;

            switch (impl.Type) {
                case MethodImplOptions.ImplType.InstanceMethod: {
                    var result = new JsMethod(impl.Name);
                    instanceMethods.Add(result);
                    instanceMethods.AddRange(impl.AdditionalNames.Select(an => new JsMethod(an) {Definition = CompileDelegatingMethod(method)}));

                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.InstanceMethod);
                }
                case MethodImplOptions.ImplType.StaticMethod: {
                    var result = new JsMethod(impl.Name);
                    staticMethods.Add(result);
                    staticMethods.AddRange(impl.AdditionalNames.Select(an => new JsMethod(an) {Definition = CompileDelegatingMethod(method)}));
                    return Tuple.Create((IContainsJsFunctionDefinition)result, MethodCompilationOptions.StaticMethod);
                }
                default:
                    throw new ArgumentException("ctor");
            }
        }

        private Tuple<IEnumerable<JsConstructor>, IEnumerable<JsMethod>, IEnumerable<JsMethod>> ConvertMembers(ITypeDefinition type) {
            var constructors    = new List<JsConstructor>();
            var instanceMethods = new List<JsMethod>();
            var staticMethods   = new List<JsMethod>();

            foreach (var c in type.GetConstructors(_typeResolveContext)) {
                var def  = ConvertConstructor(c, constructors: constructors, staticMethods: staticMethods);
                if (def != null) {
                    _methodMap.Add(c, def);
                    if (type.Kind == TypeKind.Class && c.Parameters.Count == 0)
                        _defaultConstructors.Add(c, def);
                }
            }

            foreach (var m in type.GetMethods(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
                var def  = ConvertMethod(m, instanceMethods: instanceMethods, staticMethods: staticMethods);
                if (def != null)
                    _methodMap.Add(m, def);
            }

            foreach (var m in type.GetProperties(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers)) {
            }

            foreach (var m in type.GetEvents(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers)) {
            }

            foreach (var m in type.GetFields(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers)) {
            }

            return Tuple.Create((IEnumerable<JsConstructor>)constructors, (IEnumerable<JsMethod>)instanceMethods, (IEnumerable<JsMethod>)staticMethods);
        }

        private JsClass ConvertClass(ITypeDefinition type) {
            var name = ConvertName(type);
            if (name == null)
                return null;

            var baseTypes    = type.GetAllBaseTypes(_typeResolveContext).ToList();
            var baseClass    = type.Kind != TypeKind.Interface ? ConvertPotentiallyGenericType(baseTypes.Last(t => t != type && t.Kind == TypeKind.Class)) : null;    // NRefactory bug/feature: Interfaces are reported as having System.Object as their base type.
            var interfaces   = baseTypes.Where(t => t != type && t.Kind == TypeKind.Interface).Select(ConvertPotentiallyGenericType).ToList();
            var typeArgNames = type.TypeParameters.Select(a => _namingConvention.GetTypeParameterName(_typeResolveContext, a)).ToList();

            var members = ConvertMembers(type);

            return new JsClass(name, IsTypePublic(type), ConvertClassType(type.Kind), typeArgNames, baseClass, interfaces, members.Item1, members.Item2, members.Item3);
        }

        private void CreateTypes() {
            foreach (var type in _project.GetAllTypes()) {
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

        public IEnumerable<JsType> Compile(IEnumerable<ISourceFile> sourceFiles, IEnumerable<ITypeResolveContext> references) {
            _project = new SimpleProjectContent();
            var parsedFiles = sourceFiles.Select(f => {
                                                          using (var rdr = f.Open()) {
                                                              return new { ParsedFile = new CSharpParsedFile(f.FileName, new UsingScope(_project)), CompilationUnit = new CSharpParser().Parse(rdr) };
                                                          }
                                                      }
                                                ).ToList();

            foreach (var f in parsedFiles) {
                var tcv = new TypeSystemConvertVisitor(f.ParsedFile);
                f.CompilationUnit.AcceptVisitor(tcv);
                _project.UpdateProjectContent(null, f.ParsedFile);
            }

            using (var syncContext = new CompositeTypeResolveContext(new[] { _project }.Concat(references)).Synchronize()) { // docs recommend synchronizing.
                _typeResolveContext  = syncContext;
                _types               = new Dictionary<ITypeDefinition, JsType>();
                _methodMap           = new Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>>();
                _defaultConstructors = new Dictionary<IMethod, Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions>>();

                CreateTypes();

                var res = new CSharpResolver(_typeResolveContext);
                foreach (var f in parsedFiles) {
                    _resolver = new ResolveVisitor(res, f.ParsedFile, new ResolveAllNavigator());
                    _resolver.Scan(f.CompilationUnit);
                    f.CompilationUnit.AcceptVisitor(this);
                }
            }

            _types.Values.ForEach(t => t.Freeze());

            _methodMap.Where(kvp => kvp.Value.Item1.Definition == null)
                      .ForEach(kvp => _errorReporter.Error("Member " + kvp.Key.ToString() + " does not have an implementation."));

            return _types.Values;
        }

        public JsFunctionDefinitionExpression CompileDelegatingMethod(IMethod method) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        public JsFunctionDefinitionExpression CompileDefaultConstructorWithoutImplementation(IMethod method, MethodCompilationOptions options) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        public JsFunctionDefinitionExpression CompileMethod(AttributedNode method, MethodCompilationOptions options) {
            // BIG TODO.
            return new JsFunctionDefinitionExpression(new string[0], JsBlockStatement.Empty);
        }

        public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data) {
            var resolveResult = _resolver.GetResolveResult(methodDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Method declaration " + methodDeclaration.Name + " does not resolve to a member.");
                return null;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Method declaration " + methodDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString());
                return null;
            }

            Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(methodDeclaration, jsMethod.Item2);
            }

            return null;
        }

        public override object VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, object data) {
            var resolveResult = _resolver.GetResolveResult(constructorDeclaration);
            if (!(resolveResult is MemberResolveResult)) {
                _errorReporter.Error("Method declaration " + constructorDeclaration.Name + " does not resolve to a member.");
                return null;
            }
            var method = ((MemberResolveResult)resolveResult).Member as IMethod;
            if (method == null) {
                _errorReporter.Error("Method declaration " + constructorDeclaration.Name + " does not resolve to a method (resolves to " + resolveResult.ToString());
                return null;
            }

            Tuple<IContainsJsFunctionDefinition, MethodCompilationOptions> jsMethod;
            if (_methodMap.TryGetValue(method, out jsMethod)) {
                jsMethod.Item1.Definition = CompileMethod(constructorDeclaration, jsMethod.Item2);
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
