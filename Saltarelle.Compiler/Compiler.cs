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
    internal interface ITypeConvertVisitor {
        IEnumerable<JsType> ConvertTypes(IEnumerable<ISourceFile> sourceFiles, IEnumerable<ITypeResolveContext> references);
    }

    internal class Compiler : DepthFirstAstVisitor<object, object>, ITypeConvertVisitor {
        private ITypeResolveContext _typeResolveContext;
        private List<JsType> _result;

        public Compiler() {
        }

        public IEnumerable<JsType> ConvertTypes(IEnumerable<ISourceFile> sourceFiles, IEnumerable<ITypeResolveContext> references) {
            var project = new SimpleProjectContent();
            var parsedFiles = sourceFiles.Select(f => {
                                                          using (var rdr = f.Open()) {
                                                              return new { ParsedFile = new CSharpParsedFile(f.FileName, new UsingScope(project)), CompilationUnit = new CSharpParser().Parse(rdr) };
                                                          }
                                                      }
                                                ).ToList();

            foreach (var f in parsedFiles) {
                var tcv = new TypeSystemConvertVisitor(f.ParsedFile);
                f.CompilationUnit.AcceptVisitor(tcv);
                project.UpdateProjectContent(null, f.ParsedFile);
            }

            _typeResolveContext = new CompositeTypeResolveContext(new[] { project }.Concat(references));
            _result = new List<JsType>();

            foreach (var f in parsedFiles) {
                f.CompilationUnit.AcceptVisitor(this);
            }

            return _result;
        }
    }
}
