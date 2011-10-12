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
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
    public interface ICompiler {
        IEnumerable<JsType> Compile(IEnumerable<ISourceFile> sourceFiles, IEnumerable<ITypeResolveContext> references);
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
        private ITypeResolveContext _typeResolveContext;
        private SimpleProjectContent _project;
        private Dictionary<ITypeDefinition, JsType> _types;
        private ResolveVisitor _resolver;

        public Compiler(INamingConventionResolver namingConvention) {
            _namingConvention = namingConvention;
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

        private Tuple<IEnumerable<JsMember>, IEnumerable<JsMember>, IEnumerable<JsMember>> ConvertMembers(ITypeDefinition type) {
            var constructors = new List<JsMember>();
            var instanceMembers = new List<JsMember>();
            var staticMembers = new List<JsMember>();

            foreach (var c in type.GetConstructors(_typeResolveContext)) {
                var name = GetConstructorName(c);
            }

            foreach (var m in type.GetMethods(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
            }

            foreach (var m in type.GetProperties(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
            }

            foreach (var m in type.GetEvents(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
            }

            foreach (var m in type.GetFields(_typeResolveContext, options: GetMemberOptions.IgnoreInheritedMembers).Where(m => !m.IsConstructor)) {
            }

            return Tuple.Create(constructors, instanceMembers, staticMembers);
        }

        private JsClass ConvertClass(ITypeDefinition type) {
            var name = ConvertName(type);
            if (name == null)
                return null;

            var baseTypes    = type.GetAllBaseTypes(_typeResolveContext).ToList();
            var baseClass    = type.Kind != TypeKind.Interface ? ConvertPotentiallyGenericType(baseTypes.Last(t => t != type && t.Kind == TypeKind.Class)) : null;    // NRefactory bug/feature: Interfaces are reported as having System.Object as their base type.
            var interfaces   = baseTypes.Where(t => t != type && t.Kind == TypeKind.Interface).Select(ConvertPotentiallyGenericType).ToList();
            var typeArgNames = type.TypeParameters.Select(a => _namingConvention.GetTypeParameterName(a)).ToList();

            var members = ConvertMembers(type);

            return new JsClass(name, IsTypePublic(type), ConvertClassType(type.Kind), typeArgNames, baseClass, interfaces, members.Item1, members.Item2, members.Item3);
        }

        private void CreateTypes() {
            foreach (var type in _project.GetAllTypes()) {
                var t = (type.Kind == TypeKind.Enum ? (JsType)ConvertEnum(type) : (JsType)ConvertClass(type));
                if (t != null)
                    _types[type] = t;
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
                _typeResolveContext = syncContext;
                _types              = new Dictionary<ITypeDefinition, JsType>();

                CreateTypes();

                var res = new CSharpResolver(_typeResolveContext);
                foreach (var f in parsedFiles) {
                    _resolver = new ResolveVisitor(res, f.ParsedFile, new ResolveAllNavigator());
                    _resolver.Scan(f.CompilationUnit);
                    f.CompilationUnit.AcceptVisitor(this);
                }
            }

            foreach (var t in _types.Values)
                t.Freeze();

            return _types.Values;
        }

        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data) {
            var tp = _resolver.GetResolveResult(typeDeclaration);
            return base.VisitTypeDeclaration(typeDeclaration, data);
        }
    }
}
