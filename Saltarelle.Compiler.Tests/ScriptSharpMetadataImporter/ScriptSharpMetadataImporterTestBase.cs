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

		private MockErrorReporter errorReporter;

		protected Dictionary<string, ITypeDefinition> AllTypes { get; private set; }
		protected INamingConventionResolver Metadata { get; private set; }
		protected IList<string> AllErrors { get; private set; }

        protected void Prepare(string source, bool minimizeNames = true, bool expectErrors = false) {
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

			errorReporter = new MockErrorReporter(!expectErrors);
			Metadata = new MetadataImporter.ScriptSharpMetadataImporter(minimizeNames);

			Metadata.Prepare(compilation.GetAllTypeDefinitions(), compilation.MainAssembly, errorReporter);

			AllErrors = errorReporter.AllMessages.ToList().AsReadOnly();
            if (expectErrors) {
                AllErrors.Should().NotBeEmpty("Compile should have generated errors");
            }
			else {
                AllErrors.Should().BeEmpty("Compile should not generate errors");
			}

			AllTypes = compilation.MainAssembly.TopLevelTypeDefinitions.SelectMany(SelfAndNested).ToDictionary(t => t.ReflectionName);
        }

		protected TypeScriptSemantics FindType(string name) {
			return Metadata.GetTypeSemantics(AllTypes[name]);
		}

		protected IEnumerable<IMember> FindMembers(string name) {
            var lastDot = name.LastIndexOf('.');
			return AllTypes[name.Substring(0, lastDot)].Members.Where(m => m.Name == name.Substring(lastDot + 1));
		}

		protected List<Tuple<IMethod, MethodScriptSemantics>> FindMethods(string name) {
			return FindMembers(name).Cast<IMethod>().Select(m => Tuple.Create(m, Metadata.GetMethodSemantics(m))).ToList();
		}

		protected MethodScriptSemantics FindMethod(string name) {
			return FindMethods(name).Single().Item2;
		}

		protected PropertyScriptSemantics FindProperty(string name) {
			return FindMembers(name).Cast<IProperty>().Where(p => !p.IsIndexer).Select(p => Metadata.GetPropertySemantics(p)).Single();
		}

		protected FieldScriptSemantics FindField(string name) {
			return FindMembers(name).Cast<IField>().Select(f => Metadata.GetFieldSemantics(f)).Single();
		}

		protected PropertyScriptSemantics FindIndexer(string typeName, int parameterCount) {
			return AllTypes[typeName].Members.OfType<IProperty>().Where(p => p.Parameters.Count == parameterCount).Select(p => Metadata.GetPropertySemantics(p)).Single();
		}

		protected EventScriptSemantics FindEvent(string name) {
			return FindMembers(name).Cast<IEvent>().Select(p => Metadata.GetEventSemantics(p)).Single();
		}
    }
}
