using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
    public class ScriptSharpMetadataImporterTestBase {
		private IEnumerable<ITypeDefinition> SelfAndNested(ITypeDefinition def) {
			return new[] { def }.Concat(def.NestedTypes.SelectMany(SelfAndNested));
		}

		private MockErrorReporter errorReporter;

		protected Dictionary<string, ITypeDefinition> AllTypes { get; private set; }
		protected IScriptSharpMetadataImporter Metadata { get; private set; }
		protected IList<string> AllErrorTexts { get; private set; }
		protected IList<Message> AllErrors { get; private set; }

        protected void Prepare(string source, bool minimizeNames = true, bool expectErrors = false) {
            IProjectContent project = new CSharpProjectContent();
            var parser = new CSharpParser();

            using (var rdr = new StringReader(source)) {
				var pf = new CSharpUnresolvedFile("File.cs");
				var syntaxTree = parser.Parse(rdr, pf.FileName);
				syntaxTree.AcceptVisitor(new TypeSystemConvertVisitor(pf));
				project = project.AddOrUpdateFiles(pf);
            }
            project = project.AddAssemblyReferences(new[] { Common.Mscorlib });

			var compilation = project.CreateCompilation();

			errorReporter = new MockErrorReporter(!expectErrors);
			Metadata = new MetadataImporter.ScriptSharpMetadataImporter(errorReporter);

			Metadata.Prepare(compilation.GetAllTypeDefinitions(), minimizeNames, compilation.MainAssembly);

			AllErrors = errorReporter.AllMessages.ToList().AsReadOnly();
			AllErrorTexts = errorReporter.AllMessagesText.ToList().AsReadOnly();
            if (expectErrors) {
                AllErrorTexts.Should().NotBeEmpty("Compile should have generated errors");
            }
			else {
                AllErrorTexts.Should().BeEmpty("Compile should not generate errors");
			}

			AllTypes = compilation.MainAssembly.TopLevelTypeDefinitions.SelectMany(SelfAndNested).ToDictionary(t => t.ReflectionName);
        }

		protected TypeScriptSemantics FindType(string name) {
			return Metadata.GetTypeSemantics(AllTypes[name]);
		}

		protected DelegateScriptSemantics FindDelegate(string name) {
			return Metadata.GetDelegateSemantics(AllTypes[name]);
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

		protected MethodScriptSemantics FindMethod(string name, int parameterCount) {
			return FindMethods(name).Single(m => m.Item1.Parameters.Count == parameterCount).Item2;
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

		protected ConstructorScriptSemantics FindConstructor(string typeName, int parameterCount) {
			return Metadata.GetConstructorSemantics(AllTypes[typeName].Methods.Single(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == parameterCount));
		}
    }
}
