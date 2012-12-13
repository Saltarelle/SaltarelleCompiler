using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulatorTests {
	public class ScriptSharpOOPEmulatorTestBase {
		protected ITypeDefinition CreateMockTypeDefinition(string name, Accessibility accessibility = Accessibility.Public, ITypeDefinition declaringType = null) {
			var typeDef = Common.CreateTypeMock(name);
			typeDef.SetupGet(_ => _.Attributes).Returns(new IAttribute[0]);
			typeDef.SetupGet(_ => _.DirectBaseTypes).Returns(new IType[0]);
			typeDef.SetupGet(_ => _.Accessibility).Returns(accessibility);
			typeDef.SetupGet(_ => _.DeclaringTypeDefinition).Returns(declaringType);
			typeDef.Setup(_ => _.GetConstructors(It.IsAny<Predicate<IUnresolvedMethod>>(), It.IsAny<GetMemberOptions>())).Returns(new IMethod[0]);
			return typeDef.Object;
		}

		protected IMethod CreateMockMethod(string name) {
			var mock = new Mock<IMethod>();
			mock.Setup(m => m.Name).Returns(name);
			return mock.Object;
		}

		protected string Process(IEnumerable<JsType> types, IScriptSharpMetadataImporter metadataImporter = null, IErrorReporter errorReporter = null, IMethod entryPoint = null) {
			metadataImporter = metadataImporter ?? new MockScriptSharpMetadataImporter();

			IProjectContent proj = new CSharpProjectContent();
			proj = proj.AddAssemblyReferences(new[] { Common.Mscorlib });
			var comp = proj.CreateCompilation();
			bool verifyNoErrors = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter();
			var obj = new OOPEmulator.ScriptSharpOOPEmulator(comp, metadataImporter, new MockRuntimeLibrary(), new MockNamer(), errorReporter);
			if (verifyNoErrors)
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Should not have errors");

			var rewritten = obj.Process(types, entryPoint);
			return string.Join("", rewritten.Select(s => OutputFormatter.Format(s, allowIntermediates: true)));
		}

		protected void AssertCorrect(string expected, IEnumerable<JsType> types, IScriptSharpMetadataImporter metadataImporter = null, IMethod entryPoint = null) {
			var actual = Process(types, metadataImporter: metadataImporter, entryPoint: entryPoint);

			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		protected void AssertCorrect(string expected, params JsType[] types) {
			AssertCorrect(expected, (IEnumerable<JsType>)types);
		}

		protected void AssertCorrect(string expected, IScriptSharpMetadataImporter metadataImporter, params JsType[] types) {
			AssertCorrect(expected, (IEnumerable<JsType>)types, metadataImporter);
		}

		protected void AssertCorrect(string expected, IScriptSharpMetadataImporter metadataImporter, IMethod entryPoint, params JsType[] types) {
			AssertCorrect(expected, (IEnumerable<JsType>)types, metadataImporter, entryPoint);
		}
	}
}
