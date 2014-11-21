using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	public class MethodCompilerTestBase : CompilerTestBase {
		protected IMethodSymbol Method { get; private set; }
		protected MethodCompiler MethodCompiler { get; private set; }
		protected JsFunctionDefinitionExpression CompiledMethod { get; private set; }

		protected void CompileMethod(string source, IMetadataImporter metadataImporter = null, INamer namer = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, string methodName = "M", bool addSkeleton = true, IEnumerable<MetadataReference> references = null) {
			Compile(new[] { addSkeleton ? "using System; class C { " + source + "}" : source }, metadataImporter: metadataImporter, namer: namer, runtimeLibrary: runtimeLibrary, errorReporter: errorReporter, methodCompiled: (m, res, mc) => {
				if (m.Name == methodName) {
					Method = m;
					MethodCompiler = mc;
					CompiledMethod = res;
				}
			}, references: references);

			Assert.That(Method, Is.Not.Null, "Method " + methodName + " was not compiled");
		}

		protected void AssertCorrect(string csharp, string expected, IMetadataImporter metadataImporter = null, IRuntimeLibrary runtimeLibrary = null, bool addSkeleton = true, IEnumerable<MetadataReference> references = null, string methodName = "M", bool mutableValueTypes = false, bool collapseWhitespace = false, bool addSourceLocations = false) {
			CompileMethod(csharp, metadataImporter: metadataImporter ?? new MockMetadataImporter {
				GetPropertySemantics = p => {
					if (p.ContainingType.IsAnonymousType || new Regex("^F[0-9]*$").IsMatch(p.Name) || (p.ContainingType.SpecialType == SpecialType.System_Array && p.Name == "Length")) {
						return PropertyScriptSemantics.Field("$" + p.Name);
					}
					else {
						string name = p.IsIndexer ? "Item" : p.Name;
						return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$" + name), MethodScriptSemantics.NormalMethod("set_$" + name));
					}
				},
				GetMethodSemantics = m => {
					if (m.IsAccessor())
						throw new InvalidOperationException("Can't get semantics for accessor");
					return MethodScriptSemantics.NormalMethod("$" + m.Name);
				},
				GetEventSemantics  = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_$" + e.Name), MethodScriptSemantics.NormalMethod("remove_$" + e.Name)),
				GetTypeSemantics = t => {
					return mutableValueTypes && t.TypeKind == TypeKind.Struct ? TypeScriptSemantics.MutableValueType(t.Name) : TypeScriptSemantics.NormalType(t.Name);
				}
			}, runtimeLibrary: runtimeLibrary, methodName: methodName, addSkeleton: addSkeleton, references: references);

			if (addSourceLocations)
				CompiledMethod = (JsFunctionDefinitionExpression)SourceLocationsInserter.Process(CompiledMethod);

			string actual = OutputFormatter.Format(CompiledMethod, allowIntermediates: true);

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

			string expectedCompare, actualCompare;
			if (collapseWhitespace) {
				expectedCompare = Regex.Replace(expected, @"\s", "");
				actualCompare = Regex.Replace(actual, @"\s", "");
			}
			else {
				expectedCompare = expected.Replace("\r\n", "\n");
				actualCompare = actual.Replace("\r\n", "\n");
			}
			
			Assert.That(actualCompare, Is.EqualTo(expectedCompare), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		protected void DoForAllIntegerTypes(Action<string> a) {
			DoForAllSignedIntegerTypes(a);
			DoForAllUnsignedIntegerTypes(a);
		}

		protected void DoForAllNumericTypes(Action<string> a) {
			DoForAllIntegerTypes(a);
			DoForAllFloatingPointTypes(a);
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
