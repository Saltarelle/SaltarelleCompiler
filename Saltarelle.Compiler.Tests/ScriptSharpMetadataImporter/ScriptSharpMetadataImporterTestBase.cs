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

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
    public class ScriptSharpMetadataImporterTestBase {
        private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader().LoadAssemblyFile(@"..\..\..\ScriptSharp\bin\Debug\mscorlib.dll"));
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
    }
}
