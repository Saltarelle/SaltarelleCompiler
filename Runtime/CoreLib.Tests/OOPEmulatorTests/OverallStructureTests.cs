using System;
using System.IO;
using System.Linq;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class OverallStructureTests : OOPEmulatorTestBase {
		private class Resource : IAssemblyResource {
			public string Name { get; private set; }
			public AssemblyResourceType Type { get; private set; }
			public string LinkedFileName { get; private set; }
			public bool IsPublic { get; private set; }
			private readonly byte[] _data;

			public Resource(AssemblyResourceType type, string name, bool isPublic, string linkedFileName = null, byte[] data = null) {
				_data = data;
				Type = type;
				Name = name;
				IsPublic = isPublic;
				LinkedFileName = linkedFileName;
			}

			public Stream GetResourceStream() {
				return new MemoryStream(_data);
			}
		}

		[Test]
		public void CodeBeforeFirstTypeIncludesAssemblyAndNamespaceInitialization() {
			var compilation = Compile(
@"namespace OuterNamespace {
	namespace InnerNamespace {
		public class SomeType {}
		public class SomeType2 {}
		public enum SomeEnum {}
	}
	namespace InnerNamespace2 {
		public class OtherType : InnerNamespace.SomeType2 {}
		public interface OtherInterface {}
	}
}");

			var emulator = CreateEmulator(compilation.Item1);
			var actual = emulator.GetCodeBeforeFirstType(compilation.Item2).ToList();

			Assert.That(OutputFormatter.Format(actual, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"global.OuterNamespace = global.OuterNamespace || {};
global.OuterNamespace.InnerNamespace = global.OuterNamespace.InnerNamespace || {};
global.OuterNamespace.InnerNamespace2 = global.OuterNamespace.InnerNamespace2 || {};
{Script}.initAssembly($asm, 'x');
".Replace("\r\n", "\n")));
		}

		[Test]
		public void CodeBeforeFirstTypeExportsNamespacesToTheExportsObjectIfTheAssemblyHasAModuleName() {
			var compilation = Compile(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""m"")]
namespace OuterNamespace {
	namespace InnerNamespace {
		public class SomeType {}
		public class SomeType2 {}
		public enum SomeEnum {}
	}
	namespace InnerNamespace2 {
		public class OtherType : InnerNamespace.SomeType2 {}
		public interface OtherInterface {}
	}
}");

			var emulator = CreateEmulator(compilation.Item1);
			var actual = emulator.GetCodeBeforeFirstType(compilation.Item2).ToList();

			Assert.That(OutputFormatter.Format(actual, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"exports.OuterNamespace = exports.OuterNamespace || {};
exports.OuterNamespace.InnerNamespace = exports.OuterNamespace.InnerNamespace || {};
exports.OuterNamespace.InnerNamespace2 = exports.OuterNamespace.InnerNamespace2 || {};
{Script}.initAssembly($asm, 'x');
".Replace("\r\n", "\n")));
		}

		[Test]
		public void CodeBeforeFirstTypeDoesNotIncludeInitializationOfNamespaceForNonExportedTypes() {
			var compilation = Compile(
@"namespace OuterNamespace {
	namespace InnerNamespace {
		internal class SomeType {}
	}
}");

			var emulator = CreateEmulator(compilation.Item1);
			var actual = emulator.GetCodeBeforeFirstType(compilation.Item2).ToList();

			Assert.That(OutputFormatter.Format(actual, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"{Script}.initAssembly($asm, 'x');
".Replace("\r\n", "\n")));
		}

		[Test]
		public void CodeBeforeFirstTypeDoesNotIncludeInitializationOfNamespaceForGlobalOrMixinTypes() {
			var compilation = Compile(
@"namespace OuterNamespace {
	namespace InnerNamespace {
		[System.Runtime.CompilerServices.Mixin(""x"")] public static class GlobalType {}
		[System.Runtime.CompilerServices.GlobalMethods] public static class MixinType {}
	}
}");

			var emulator = CreateEmulator(compilation.Item1);
			var actual = emulator.GetCodeBeforeFirstType(compilation.Item2).ToList();

			Assert.That(OutputFormatter.Format(actual, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"{Script}.initAssembly($asm, 'x');
".Replace("\r\n", "\n")));
		}

		[Test]
		public void AssemblyAttributesAreAssignedInTheCodeAfterLastType() {
			var compilation = Compile(
@"[assembly: MyAttribute(42)]
public class MyAttribute : System.Attribute {
	public MyAttribute(int x) {}
	static MyAttribute() { int a = 0; }
}");

			var emulator = CreateEmulator(compilation.Item1);

			var actual = emulator.GetCodeAfterLastType(compilation.Item2).ToList();

			Assert.That(actual.Count, Is.EqualTo(1));
			Assert.That(OutputFormatter.Format(actual, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo("$asm.attr = [new {MyAttribute}(42)];\n"));
		}

		[Test]
		public void BothPublicAndPrivateEmbeddedResourcesAreIncludedInTheInitAssemblyCallButThisExcludesPluginDllsAndLinkedResources() {
			var compilation = Compile(@"", resources: new[] { new Resource(AssemblyResourceType.Embedded, "Resource.Name", true, data: new byte[] { 45, 6, 7, 4 }),
			                                                  new Resource(AssemblyResourceType.Linked, "Other.Resource", true, linkedFileName: "some-file.txt"),
			                                                  new Resource(AssemblyResourceType.Embedded, "Some.Private.Resource", false, data: new byte[] { 5, 3, 7 }),
			                                                  new Resource(AssemblyResourceType.Embedded, "Namespace.Plugin.dll", true, data: new byte[] { 5, 3, 7 }),
			                                                  new Resource(AssemblyResourceType.Embedded, "Plugin.dll", true, data: new byte[] { 5, 3, 7 }) });

			var emulator = CreateEmulator(compilation.Item1);

			var actual = emulator.GetCodeBeforeFirstType(compilation.Item2).Select(s => OutputFormatter.Format(s, allowIntermediates: true)).Single(s => s.StartsWith("{Script}.initAssembly"));

			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo("{Script}.initAssembly($asm, 'x', { 'Resource.Name': 'LQYHBA==', 'Some.Private.Resource': 'BQMH' });\n"));
		}
	}
}
