using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	public class ConstructorTests : CompilerTestBase {
        protected IMethod Constructor { get; private set; }
        protected MethodCompiler MethodCompiler { get; private set; }
        protected JsFunctionDefinitionExpression CompiledConstructor { get; private set; }

        protected void Compile(string source, INamingConventionResolver namingConvention = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null) {
            Compile(new[] { source }, namingConvention, runtimeLibrary, errorReporter, (m, res, mc) => {
				if (m.IsConstructor && m.Attributes.Any()) {
					Constructor = m;
					MethodCompiler = mc;
					CompiledConstructor = res;
				}
            });

			Assert.That(Constructor, Is.Not.Null, "No constructors with attributes were compiled.");
        }

		protected void AssertCorrect(string csharp, string expected, INamingConventionResolver namingConvention = null) {
			Compile(csharp, namingConvention);
			string actual = OutputFormatter.Format(CompiledConstructor);
			Assert.That(actual, Is.EqualTo(expected));
		}


		[Test]
		public void SimpleUnnamedConstructorWorks() {
			AssertCorrect(
@"class C {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		int i = 0;
	}
}",
@"function() {
	var $i = 0;
}");
		}

		[Test]
		public void ConstructorChainingWithoutArgumentsWorks() {
			Assert.Fail("TODO");
			AssertCorrect(
@"class C {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		int i = 0;
	}
}",
@"function() {
	var $i = 0;
}");
		}

		[Test]
		public void ConstructorChainingWithReorderedAndDefaultArgumentsWorks() {
			Assert.Fail("TODO");
			AssertCorrect(
@"class C {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		int i = 0;
	}
}",
@"function() {
	var $i = 0;
}");
		}

		[Test]
		public void ChainingToNamedConstructorWorks() {
			Assert.Fail("TODO");
			AssertCorrect(
@"class C {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		int i = 0;
	}
}",
@"function() {
	var $i = 0;
}");
		}

		[Test]
		public void ChainingToStaticMethodConstructorWorks() {
			Assert.Fail("TODO");
			AssertCorrect(
@"class C {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		int i = 0;
	}
}",
@"function() {
	var $i = 0;
}");
		}

		[Test]
		public void ConstructorWithoutExplicitBaseInvokerInvokesBaseClassDefaultConstructor() {
			Assert.Fail("TODO");
		}

		[Test]
		public void InvokingBaseConstructorWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void InvokingBaseConstructorWithReorderedAndDefaultArgumentsWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void FieldsAreInitialized() {
			Assert.Fail("TODO");
		}

		[Test]
		public void StaticMethodConstructorWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void FieldsAreNotInitializedWhenChainingConstructors() {
			Assert.Fail("TODO");
		}

		[Test]
		public void FieldsAreInitializedBeforeCallingBaseWhenBaseCallIsImplicit() {
			Assert.Fail("TODO");
		}

		[Test]
		public void FieldsAreInitializedBeforeCallingBaseWhenBaseCallIsExplicit() {
			Assert.Fail("TODO");
		}
	}
}
