using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
    public class ScriptSharpMetadataImporterTestBase {
        private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(@"..\..\..\ScriptSharp\bin\Debug\mscorlib.dll"));
        protected IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

		private IEnumerable<ITypeDefinition> SelfAndNested(ITypeDefinition def) {
			return new[] { def }.Concat(def.NestedTypes.SelectMany(SelfAndNested));
		}

        protected IDictionary<string, ITypeDefinition> Process(INamingConventionResolver namingConvention, string source, IErrorReporter errorReporter = null) {
            IProjectContent project = new CSharpProjectContent();
            var parser = new CSharpParser();

            using (var rdr = new StringReader(source)) {
				var pf = new CSharpParsedFile("File.cs");
				var cu = parser.Parse(rdr, pf.FileName);
				cu.AcceptVisitor(new TypeSystemConvertVisitor(pf));
				project = project.UpdateProjectContent(null, pf);
            }
            project = project.AddAssemblyReferences(new[] { Mscorlib });

			var compilation = project.CreateCompilation();

			bool defaultErrorHandling = (errorReporter == null);
			errorReporter = errorReporter ?? new MockErrorReporter(true);

			namingConvention.Prepare(compilation.GetAllTypeDefinitions(), compilation.MainAssembly, errorReporter);

            if (defaultErrorHandling) {
                ((MockErrorReporter)errorReporter).AllMessages.Should().BeEmpty("Compile should not generate errors");
            }

			return compilation.MainAssembly.TopLevelTypeDefinitions.SelectMany(SelfAndNested).ToDictionary(t => t.ReflectionName);
        }

		protected IEnumerable<IMember> FindMembers(IDictionary<string, ITypeDefinition> types, string name) {
            var lastDot = name.LastIndexOf('.');
			return types[name.Substring(0, lastDot)].Members.Where(m => m.Name == name.Substring(lastDot + 1));
		}

		protected List<Tuple<IMethod, MethodScriptSemantics>> FindMethods(IDictionary<string, ITypeDefinition> types, string name, INamingConventionResolver md) {
			return FindMembers(types, name).Cast<IMethod>().Select(m => Tuple.Create(m, md.GetMethodSemantics(m))).ToList();
		}

		protected MethodScriptSemantics FindMethod(IDictionary<string, ITypeDefinition> types, string name, INamingConventionResolver md) {
			return FindMethods(types, name, md).Single().Item2;
		}

		protected PropertyScriptSemantics FindProperty(IDictionary<string, ITypeDefinition> types, string name, INamingConventionResolver md) {
			return FindMembers(types, name).Cast<IProperty>().Where(p => !p.IsIndexer).Select(p => md.GetPropertySemantics(p)).Single();
		}

		protected PropertyScriptSemantics FindIndexer(IDictionary<string, ITypeDefinition> types, string typeName, int parameterCount, INamingConventionResolver md) {
			return types[typeName].Members.OfType<IProperty>().Where(p => p.Parameters.Count == parameterCount).Select(p => md.GetPropertySemantics(p)).Single();
		}
    }
}
