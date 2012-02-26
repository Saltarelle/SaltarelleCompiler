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

        protected void CompileMethod(string source, INamingConventionResolver namingConvention = null, IErrorReporter errorReporter = null, string methodName = "M") {
            Compile(new[] { "using System; class C { " + source + "}" }, namingConvention, errorReporter, (m, res, mc) => {
				if (m.Name == methodName) {
					Method = m;
					MethodCompiler = mc;
					CompiledMethod = res;
				}
            });

			Assert.That(Method, Is.Not.Null, "Method " + methodName + " was not compiled");
        }

		[SetUp]
		public void Setup() {
			Method = null;
			MethodCompiler = null;
			CompiledMethod = null;
		}

    }
}
