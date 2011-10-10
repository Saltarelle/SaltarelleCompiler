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
            if (type.DeclaringType == null)
                return ScopedName.Global(!string.IsNullOrEmpty(type.Namespace) ? type.Namespace : null, _namingConvention.GetTypeName(type));
            else
                return ScopedName.Nested(ConvertName(type.DeclaringTypeDefinition), _namingConvention.GetTypeName(type));
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
            return new JsEnum(ConvertName(type), IsTypePublic(type));
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

        private JsClass ConvertClass(ITypeDefinition type) {
            var baseTypes    = type.GetAllBaseTypes(_typeResolveContext).ToList();
            var baseClass    = type.Kind != TypeKind.Interface ? ConvertPotentiallyGenericType(baseTypes.Last(t => t != type && t.Kind == TypeKind.Class)) : null;    // NRefactory bug/feature: Interfaces are reported as having System.Object as their base type.
            var interfaces   = baseTypes.Where(t => t != type && t.Kind == TypeKind.Interface).Select(ConvertPotentiallyGenericType).ToList();
            var typeArgNames = type.TypeParameters.Select(a => _namingConvention.GetTypeParameterName(a)).ToList();

            return new JsClass(ConvertName(type), IsTypePublic(type), ConvertClassType(type.Kind), typeArgNames, baseClass, interfaces);
        }

        private void CreateTypes() {
        var x = _project.GetAllTypes();
            foreach (var type in _project.GetAllTypes()) {
                _types[type] = (type.Kind == TypeKind.Enum ? (JsType)ConvertEnum(type) : (JsType)ConvertClass(type));
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
