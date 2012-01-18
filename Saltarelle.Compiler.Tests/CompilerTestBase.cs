using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
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

        protected class MockNamingConventionResolver : INamingConventionResolver {
            public MockNamingConventionResolver() {
                GetTypeName                               = (context, t) => t.Name;
                GetTypeParameterName                      = (context, t) => t.Name;
                GetMethodImplementation                   = (context, m) => m.IsStatic ? MethodImplOptions.StaticMethod(m.Name) : MethodImplOptions.InstanceMethod(m.Name);
                GetConstructorImplementation              = (context, c) => c.Parameters.Count == 0 ? ConstructorImplOptions.Unnamed() : ConstructorImplOptions.Named("ctor$" + string.Join("$", c.Parameters.Select(p => p.Type.Resolve(context).Name)));
                GetPropertyImplementation                 = (context, p) => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_" + p.Name), MethodImplOptions.InstanceMethod("set_" + p.Name));
                GetAutoPropertyBackingFieldImplementation = (context, p) => p.IsSealed ? FieldOptions.Static("$" + p.Name) : FieldOptions.Instance("$" + p.Name);
            }

            public Func<ITypeResolveContext, ITypeDefinition, string> GetTypeName { get; set; }
            public Func<ITypeResolveContext, ITypeParameter, string> GetTypeParameterName { get; set; }
            public Func<ITypeResolveContext, IMethod, MethodImplOptions> GetMethodImplementation { get; set; }
            public Func<ITypeResolveContext, IMethod, ConstructorImplOptions> GetConstructorImplementation { get; set; }
            public Func<ITypeResolveContext, IProperty, PropertyImplOptions> GetPropertyImplementation { get; set; }
            public Func<ITypeResolveContext, IProperty, FieldOptions> GetAutoPropertyBackingFieldImplementation { get; set; }

            string INamingConventionResolver.GetTypeName(ITypeResolveContext context, ITypeDefinition typeDefinition) {
                return GetTypeName(context, typeDefinition);
            }

            string INamingConventionResolver.GetTypeParameterName(ITypeResolveContext context, ITypeParameter typeDefinition) {
                return GetTypeParameterName(context, typeDefinition);
            }

            MethodImplOptions INamingConventionResolver.GetMethodImplementation(ITypeResolveContext context, IMethod method) {
                return GetMethodImplementation(context, method);
            }

            ConstructorImplOptions INamingConventionResolver.GetConstructorImplementation(ITypeResolveContext context, IMethod method) {
                return GetConstructorImplementation(context, method);
            }

            PropertyImplOptions INamingConventionResolver.GetPropertyImplementation(ITypeResolveContext context, IProperty property) {
                return GetPropertyImplementation(context, property);
            }

            FieldOptions INamingConventionResolver.GetAutoPropertyBackingFieldImplementation(ITypeResolveContext context, IProperty property) {
                return GetAutoPropertyBackingFieldImplementation(context, property);
            }
        }

        protected class MockErrorReporter : IErrorReporter {
            public List<string> AllMessages { get; set; }

            public MockErrorReporter() {
                AllMessages = new List<string>();
                Error   = s => { s = "Error: " + s; Console.WriteLine(s); AllMessages.Add(s); };
                Warning = s => { s = "Warning: " + s; Console.WriteLine(s); AllMessages.Add(s); };
            }

            public Action<string> Error { get; set; }
            public Action<string> Warning { get; set; }

            void IErrorReporter.Error(string message) {
                Error(message);
            }

            void IErrorReporter.Warning(string message) {
                Warning(message);
            }
        }

        private static readonly Lazy<IProjectContent> _mscorlibLazy = new Lazy<IProjectContent>(() => new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location));
        protected IProjectContent Mscorlib { get { return _mscorlibLazy.Value; } }

        protected ReadOnlyCollection<JsType> CompiledTypes { get; private set; }

        protected void Compile(IEnumerable<string> sources, INamingConventionResolver namingConvention = null, IErrorReporter errorReporter = null) {
            var sourceFiles = sources.Select((s, i) => new MockSourceFile("File" + i + ".cs", s)).ToList();
            bool defaultErrorHandling = false;
            if (errorReporter == null) {
                defaultErrorHandling = true;
                errorReporter = new MockErrorReporter();
            }
            CompiledTypes = new Compiler(namingConvention ?? new MockNamingConventionResolver(), errorReporter).Compile(sourceFiles, new[] { Mscorlib }).AsReadOnly();
            if (defaultErrorHandling) {
                ((MockErrorReporter)errorReporter).AllMessages.Should().BeEmpty("Compile should not generate errors");
            }
        }

        protected void Compile(params string[] sources) {
            Compile((IEnumerable<string>)sources);
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

        protected JsClass FindClass(string name) {
            var result = CompiledTypes.SingleOrDefault(t => t.Name.ToString() == name);
            if (result == null) Assert.Fail("Could not find type " + name);
            if (!(result is JsClass)) Assert.Fail("Found type is not a JsClass, it is a " + result.GetType().Name);
            return (JsClass)result;
        }

        protected JsMethod FindInstanceMethod(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.InstanceMethods.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
        }

        protected JsMethod FindStaticMethod(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.StaticMethods.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
        }

        protected JsConstructor FindConstructor(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.Constructors.SingleOrDefault(m => (m.Name ?? "<default>") == name.Substring(lastDot + 1));
        }

        protected JsField FindInstanceField(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.InstanceFields.SingleOrDefault(f => f.Name == name.Substring(lastDot + 1));
        }

        protected JsField FindStaticField(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.StaticFields.SingleOrDefault(f => f.Name == name.Substring(lastDot + 1));
        }
    }
}
