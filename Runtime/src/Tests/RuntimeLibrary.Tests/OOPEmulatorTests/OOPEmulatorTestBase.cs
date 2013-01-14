using System.Collections.Generic;
using System.Linq;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.Tests;

namespace RuntimeLibrary.Tests.OOPEmulatorTests {
	public class OOPEmulatorTestBase {
		protected IMethod CreateMockMethod(string name) {
			var mock = new Mock<IMethod>();
			mock.Setup(m => m.Name).Returns(name);
			return mock.Object;
		}

		protected string Process(IEnumerable<JsType> types, IMetadataImporter metadataImporter = null, IErrorReporter errorReporter = null, IMethod entryPoint = null) {
			metadataImporter = metadataImporter ?? new MockMetadataImporter();

			IProjectContent proj = new CSharpProjectContent();
			proj = proj.AddAssemblyReferences(new[] { Files.Mscorlib });
			var comp = proj.CreateCompilation();
			bool verifyNoErrors = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter();
			var obj = new OOPEmulator(comp, metadataImporter, new MockRuntimeLibrary(), new MockNamer(), errorReporter);
			if (verifyNoErrors)
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Should not have errors");

			var rewritten = obj.Process(types, entryPoint);
			return string.Join("", rewritten.Select(s => OutputFormatter.Format(s, allowIntermediates: true)));
		}

		protected string Process(params JsType[] types) {
			return Process((IEnumerable<JsType>)types);
		}

		protected void AssertCorrect(string expected, IEnumerable<JsType> types, IMetadataImporter metadataImporter = null, IMethod entryPoint = null) {
			var actual = Process(types, metadataImporter: metadataImporter, entryPoint: entryPoint);

			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		protected void AssertCorrect(string expected, params JsType[] types) {
			AssertCorrect(expected, (IEnumerable<JsType>)types);
		}

		protected void AssertCorrect(string expected, IMetadataImporter metadataImporter, params JsType[] types) {
			AssertCorrect(expected, (IEnumerable<JsType>)types, metadataImporter);
		}

		protected void AssertCorrect(string expected, IMetadataImporter metadataImporter, IMethod entryPoint, params JsType[] types) {
			AssertCorrect(expected, (IEnumerable<JsType>)types, metadataImporter, entryPoint);
		}
	}
}
