using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler {
    public interface ICompiler {
        IEnumerable<JsType> Compile(IEnumerable<ISourceFile> sourceFiles, IEnumerable<ITypeResolveContext> references);
    }

    public class Compiler : DepthFirstAstVisitor<object, object>, ICompiler {
        private ITypeResolveContext _typeResolveContext;
        private SimpleProjectContent _project;
        private Dictionary<ScopedName, JsType> _types;
        private string _currentNamespace;
        private JsType _currentType;

        public Compiler() {
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

            _typeResolveContext = new CompositeTypeResolveContext(new[] { _project }.Concat(references));
            _types              = new Dictionary<ScopedName, JsType>();
            _currentNamespace   = null;
            _currentType        = null;

            foreach (var f in parsedFiles) {
                f.CompilationUnit.AcceptVisitor(this);
            }

            foreach (var t in _types.Values)
                t.Freeze();

            return _types.Values;
        }

        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data) {
            var oldType = _currentType;
            try {
                ScopedName name = _currentType != null ? ScopedName.Nested(_currentType.Name, typeDeclaration.Name) : ScopedName.Global(_currentNamespace, typeDeclaration.Name);
                if (_types.TryGetValue(name, out _currentType)) {
                if ((typeDeclaration.ClassType == ClassType.Enum && !(_currentType is JsEnum)) || (typeDeclaration.ClassType != ClassType.Enum && !(_currentType is JsClass)))
                    throw new InvalidOperationException("Got type of the wrong kind. Does the code compile?");
                }
                else {
                    _types[name] = _currentType = (typeDeclaration.ClassType == ClassType.Enum ? (JsType)new JsEnum(name) : new JsClass(name, null));
                }

                return base.VisitTypeDeclaration(typeDeclaration, data);
            }
            finally {
                _currentType = oldType;
            }
        }

        public override object VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, object data) {
            var oldNamespace = _currentNamespace;
            try {
                _currentNamespace = (oldNamespace != null ? oldNamespace + "." : "") + string.Join(".", namespaceDeclaration.Identifiers.Select(i => i.Name));
                return base.VisitNamespaceDeclaration(namespaceDeclaration, data);
            }
            finally {
                _currentNamespace = oldNamespace;
            }
        }
    }
}
