using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests
{
    public class MethodCompilerTestBase : CompilerTestBase {
        protected IMethod Method { get; private set; }
        protected MethodCompiler MethodCompiler { get; private set; }
        protected JsFunctionDefinitionExpression CompiledMethod { get; private set; }

        protected void CompileMethod(string source, INamingConventionResolver namingConvention = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, string methodName = "M") {
            Compile(new[] { "using System; class C { " + source + "}" }, namingConvention, runtimeLibrary, errorReporter, (m, res, mc) => {
				if (m.Name == methodName) {
					Method = m;
					MethodCompiler = mc;
					CompiledMethod = res;
				}
            });

			Assert.That(Method, Is.Not.Null, "Method " + methodName + " was not compiled");
        }

		protected void AssertCorrect(string csharp, string expected) {
			CompileMethod(csharp);
			string actual = OutputFormatter.Format(CompiledMethod.Body, true);

			int begin = actual.IndexOf("// BEGIN");
			if (begin > -1) {
				while (begin < (actual.Length - 1) && actual[begin - 1] != '\n')
					begin++;
				actual = actual.Substring(begin);
			}

			int end = actual.IndexOf("// END");
			if (end >= 0) {
				while (end >= 0 && actual[end] != '\n')
					end--;
				actual = actual.Substring(0, end + 1);
			}
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		[SetUp]
		public void Setup() {
			Method = null;
			MethodCompiler = null;
			CompiledMethod = null;
		}

    }
}
