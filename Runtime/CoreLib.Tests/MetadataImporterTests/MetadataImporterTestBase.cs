using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.MetadataImporterTests {
    public class MetadataImporterTestBase {
		private IEnumerable<ITypeDefinition> SelfAndNested(ITypeDefinition def) {
			return new[] { def }.Concat(def.NestedTypes.SelectMany(SelfAndNested));
		}

		private MockErrorReporter _errorReporter;

		protected Dictionary<string, ITypeDefinition> AllTypes { get; private set; }
		protected IMetadataImporter Metadata { get; private set; }
		protected IList<string> AllErrorTexts { get; private set; }
		protected IList<Message> AllErrors { get; private set; }

		protected void Prepare(string source, bool minimizeNames = true, bool expectErrors = false) {
			IProjectContent project = new CSharpProjectContent();
			var parser = new CSharpParser();

			using (var rdr = new StringReader(source)) {
				var pf = new CSharpUnresolvedFile { FileName = "File.cs" };
				var syntaxTree = parser.Parse(rdr, pf.FileName);
				syntaxTree.AcceptVisitor(new TypeSystemConvertVisitor(pf));
				project = project.AddOrUpdateFiles(pf);
			}
			project = project.AddAssemblyReferences(new[] { Files.Mscorlib });

			var compilation = project.CreateCompilation();
			var s = new AttributeStore(compilation);

			_errorReporter = new MockErrorReporter(!expectErrors);
			Metadata = new MetadataImporter(_errorReporter, compilation, s, new CompilerOptions { MinimizeScript = minimizeNames });

			Metadata.Prepare(compilation.GetAllTypeDefinitions());

			AllErrors = _errorReporter.AllMessages.ToList().AsReadOnly();
			AllErrorTexts = _errorReporter.AllMessages.Select(m => m.FormattedMessage).ToList().AsReadOnly();
			if (expectErrors) {
				Assert.That(AllErrorTexts, Is.Not.Empty, "Compile should have generated errors");
			}
			else {
				Assert.That(AllErrorTexts, Is.Empty, "Compile should not generate errors");
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
