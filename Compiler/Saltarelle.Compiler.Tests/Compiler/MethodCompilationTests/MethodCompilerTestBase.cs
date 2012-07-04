using System;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests
{
    public class MethodCompilerTestBase : CompilerTestBase {
        protected IMethod Method { get; private set; }
        protected MethodCompiler MethodCompiler { get; private set; }
        protected JsFunctionDefinitionExpression CompiledMethod { get; private set; }

        protected void CompileMethod(string source, INamingConventionResolver namingConvention = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, string methodName = "M", bool addSkeleton = true) {
            Compile(new[] { addSkeleton ? "using System; class C { " + source + "}" : source }, namingConvention, runtimeLibrary, errorReporter, (m, res, mc) => {
				if (m.Name == methodName) {
					Method = m;
					MethodCompiler = mc;
					CompiledMethod = res;
				}
            });

			Assert.That(Method, Is.Not.Null, "Method " + methodName + " was not compiled");
        }

		protected void AssertCorrect(string csharp, string expected, INamingConventionResolver namingConvention = null, bool addSkeleton = true) {
			CompileMethod(csharp, namingConvention: namingConvention ?? new MockNamingConventionResolver {
				GetPropertySemantics = p => {
					if (p.DeclaringType.Kind == TypeKind.Anonymous || new Regex("^F[0-9]*$").IsMatch(p.Name) || (p.DeclaringType.FullName == "System.Array" && p.Name == "Length"))
						return PropertyScriptSemantics.Field("$" + p.Name);
					else
				        return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$" + p.Name), MethodScriptSemantics.NormalMethod("set_$" + p.Name));
				},
				GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name),
				GetEventSemantics  = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_$" + e.Name), MethodScriptSemantics.NormalMethod("remove_$" + e.Name)),
			}, addSkeleton: addSkeleton);
			string actual = OutputFormatter.Format(CompiledMethod, true);

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

		protected void DoForAllIntegerTypes(Action<string> a) {
			DoForAllSignedIntegerTypes(a);
			DoForAllUnsignedIntegerTypes(a);
		}

		protected void DoForAllFloatingPointTypes(Action<string> a) {
			foreach (var type in new[] { "float", "double", "decimal" })
				a(type);
		}

		protected void DoForAllSignedIntegerTypes(Action<string> a) {
			foreach (var type in new[] { "sbyte", "short", "int", "long" })
				a(type);
		}

		protected void DoForAllUnsignedIntegerTypes(Action<string> a) {
			foreach (var type in new[] { "byte", "ushort", "uint", "ulong"  })
				a(type);
		}

		[SetUp]
		public void Setup() {
			Method = null;
			MethodCompiler = null;
			CompiledMethod = null;
		}
    }
}
