using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Tests {
    public class CompilerTestBase {
        private class MockSourceFile : ISourceFile {
            private readonly string _fileName;
            private readonly string _content;

            public MockSourceFile(string fileName, string content) {
                _fileName = fileName;
                _content  = content;
            }

            public string FileName {
                get { return _fileName; }
            }

            public TextReader Open() {
                return new StringReader(_content);
            }
        }

        private class MockNamingConventionResolver : INamingConventionResolver {
            public string GetTypeName(ITypeDefinition typeDefinition) {
                return typeDefinition.Name;
            }

            public string GetTypeParameterName(ITypeParameter typeDefinition) {
                return typeDefinition.Name;
            }

            public bool IsMemberStatic(IMember member) {
                throw new NotImplementedException();
            }

            public MethodImplOptions GetMethodImplementation(IMethod method) {
                return method.IsStatic ? MethodImplOptions.StaticMethod(method.Name) : MethodImplOptions.InstanceMethod(method.Name);
            }
        }

        private static readonly Lazy<IProjectContent> _mscorlibLazy = new Lazy<IProjectContent>(() => new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location));
        protected IProjectContent Mscorlib { get { return _mscorlibLazy.Value; } }

        protected ReadOnlyCollection<JsType> Compile(IEnumerable<string> sources, INamingConventionResolver namingConvention = null) {
            var sourceFiles = sources.Select((s, i) => new MockSourceFile("File" + i + ".cs", s)).ToList();
            return new Compiler(namingConvention ?? new MockNamingConventionResolver()).Compile(sourceFiles, new[] { Mscorlib }).AsReadOnly();
        }

        protected ReadOnlyCollection<JsType> Compile(params string[] sources) {
            return Compile((IEnumerable<string>)sources);
        }

        protected string Stringify(JsExpression expression) {
            switch (expression.NodeType) {
                case ExpressionNodeType.Identifier: return ((JsIdentifierExpression)expression).Name;
                case ExpressionNodeType.TypeReference: return "{" + ((JsTypeReferenceExpression)expression).TypeDefinition.ReflectionName + "}";
                default: throw new ArgumentException("expression");
            }
        }

        protected string Stringify(JsConstructedType tp) {
            return Stringify(tp.UnboundType) + (tp.TypeArguments.Count > 0 ? "<" + string.Join(",", tp.TypeArguments.Select(x => Stringify(x))) + ">" : "");
        }

        protected JsClass FindClass(IEnumerable<JsType> allTypes, string name) {
            var result = allTypes.SingleOrDefault(t => t.Name.ToString() == name);
            if (result == null) Assert.Fail("Could not find type " + name);
            if (!(result is JsClass)) Assert.Fail("Found type is not a JsClass, it is a " + result.GetType().Name);
            return (JsClass)result;
        }
    }

}
