using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Tests.ReferenceMetadataImporterTests {
	[TestFixture]
	public class RoundtripTests {
		private void RoundtripTest(string source, Action<IAssemblySymbol, IMetadataImporter> asserter, IMetadataImporter orig = null) {
			orig = orig ?? new MockMetadataImporter();
			var compilation = Common.CreateCompilation(source);

			using (var stream = new MemoryStream()) {
				compilation.Emit(stream);
				stream.Seek(0, SeekOrigin.Begin);
				var asm = AssemblyDefinition.ReadAssembly(stream);
				ReferenceMetadataImporter.Write(compilation, asm, orig);
				stream.Seek(0, SeekOrigin.Begin);
				asm.Write(stream);

				stream.Seek(0, SeekOrigin.Begin);

				var references = new[] { Common.Mscorlib, new MetadataImageReference(stream) };
				var otherCompilation = Common.CreateCompilation("", references, assemblyName: "Test2");

				var er = new MockErrorReporter(true);
				var md = new ReferenceMetadataImporter(er);
				asserter((IAssemblySymbol)otherCompilation.GetAssemblyOrModuleSymbol(references[1]), md);
				if (er.AllMessages.Count > 0) {
					Assert.Fail("Errors:" + Environment.NewLine + string.Join(Environment.NewLine, er.AllMessages.Select(m => m.FormattedMessage)));
				}
			}
		}

		[Test]
		public void CanRoundtripTypeSemantics() {
			RoundtripTest(
				@"public class C1 {} public class C2 {} public class C3 {} public class C4 {} public class C5 {} public class C6 {} public class C7 {}",
				(assembly, importer) => {
					var c1 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C1"));
						Assert.That(c1.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
						Assert.That(c1.Name, Is.EqualTo("$C1"));
						Assert.That(c1.IgnoreGenericArguments, Is.False);
						Assert.That(c1.GenerateCode, Is.False);
					var c2 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C2"));
						Assert.That(c2.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
						Assert.That(c2.Name, Is.EqualTo("$C2"));
						Assert.That(c2.IgnoreGenericArguments, Is.True);
						Assert.That(c2.GenerateCode, Is.False);
					var c3 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C3"));
						Assert.That(c3.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
						Assert.That(c3.Name, Is.EqualTo("$C3"));
						Assert.That(c3.IgnoreGenericArguments, Is.False);
						Assert.That(c3.GenerateCode, Is.True);
					var c4 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C4"));
						Assert.That(c4.Type, Is.EqualTo(TypeScriptSemantics.ImplType.MutableValueType));
						Assert.That(c4.Name, Is.EqualTo("$C4"));
						Assert.That(c4.IgnoreGenericArguments, Is.False);
						Assert.That(c4.GenerateCode, Is.False);
					var c5 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C5"));
						Assert.That(c5.Type, Is.EqualTo(TypeScriptSemantics.ImplType.MutableValueType));
						Assert.That(c5.Name, Is.EqualTo("$C5"));
						Assert.That(c5.IgnoreGenericArguments, Is.True);
						Assert.That(c5.GenerateCode, Is.False);
					var c6 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C6"));
						Assert.That(c6.Type, Is.EqualTo(TypeScriptSemantics.ImplType.MutableValueType));
						Assert.That(c6.Name, Is.EqualTo("$C6"));
						Assert.That(c6.IgnoreGenericArguments, Is.False);
						Assert.That(c6.GenerateCode, Is.True);
					var c7 = importer.GetTypeSemantics(assembly.GetTypeByMetadataName("C7"));
						Assert.That(c7.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NotUsableFromScript));
				}, orig: new MockMetadataImporter {
					GetTypeSemantics = t => {
						switch (t.Name) {
							case "C1":
								return TypeScriptSemantics.NormalType("$C1", false, false);
							case "C2":
								return TypeScriptSemantics.NormalType("$C2", true, false);
							case "C3":
								return TypeScriptSemantics.NormalType("$C3", false, true);
							case "C4":
								return TypeScriptSemantics.MutableValueType("$C4", false, false);
							case "C5":
								return TypeScriptSemantics.MutableValueType("$C5", true, false);
							case "C6":
								return TypeScriptSemantics.MutableValueType("$C6", false, true);
							case "C7":
								return TypeScriptSemantics.NotUsableFromScript();
							default:
								throw new ArgumentException("t");
						}
					}
				}
			);
		}

		[Test]
		public void CanRoundtripUsedInstanceMemberNames() {
			RoundtripTest(
				@"public class C {}",
				(assembly, importer) => {
					var c = assembly.GetTypeByMetadataName("C");
					Assert.That(importer.GetUsedInstanceMemberNames(c), Is.EquivalentTo(new[] { "m1", "m2", "m3", "m4" }));
				},
				new MockMetadataImporter {
					GetUsedInstanceMemberNames = t => new[] { "m1", "m2", "m3", "m4" }
				}
			);
		}

		[Test]
		public void CanRoundtripConstructorSemantics() {
			RoundtripTest(
				string.Join(" ", Enumerable.Range(1, 69).Select(i => "public class C" + i.ToString(CultureInfo.InvariantCulture) + " { public int P1 { get; set; } public int P2 { get; set; } public int F1, F2; }")),
				(assembly, importer) => {
					Action<string, Action<ConstructorScriptSemantics>> assert = (n, a) => a(importer.GetConstructorSemantics(assembly.GetTypeByMetadataName(n).GetMembers().OfType<IMethodSymbol>().Single(m => m.MethodKind == MethodKind.Constructor)));

					assert("C11", c11 => {
						Assert.That(c11.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
						Assert.That(c11.GenerateCode, Is.False);
						Assert.That(c11.ExpandParams, Is.False);
						Assert.That(c11.SkipInInitializer, Is.False);
						Assert.That(c11.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C12", c12 => {
						Assert.That(c12.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
						Assert.That(c12.GenerateCode, Is.True);
						Assert.That(c12.ExpandParams, Is.False);
						Assert.That(c12.SkipInInitializer, Is.False);
						Assert.That(c12.OmitUnspecifiedArgumentsFrom, Is.EqualTo(3));
					});

					assert("C13", c13 => {
						Assert.That(c13.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
						Assert.That(c13.GenerateCode, Is.False);
						Assert.That(c13.ExpandParams, Is.True);
						Assert.That(c13.SkipInInitializer, Is.False);
						Assert.That(c13.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C14", c14 => {
						Assert.That(c14.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
						Assert.That(c14.GenerateCode, Is.False);
						Assert.That(c14.ExpandParams, Is.False);
						Assert.That(c14.SkipInInitializer, Is.True);
						Assert.That(c14.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C21", c21 => {
						Assert.That(c21.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
						Assert.That(c21.Name, Is.EqualTo("$C21"));
						Assert.That(c21.GenerateCode, Is.False);
						Assert.That(c21.ExpandParams, Is.False);
						Assert.That(c21.SkipInInitializer, Is.False);
						Assert.That(c21.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C22", c22 => {
						Assert.That(c22.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
						Assert.That(c22.Name, Is.EqualTo("$C22"));
						Assert.That(c22.GenerateCode, Is.True);
						Assert.That(c22.ExpandParams, Is.False);
						Assert.That(c22.SkipInInitializer, Is.False);
						Assert.That(c22.OmitUnspecifiedArgumentsFrom, Is.EqualTo(4));
					});

					assert("C23", c23 => {
						Assert.That(c23.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
						Assert.That(c23.Name, Is.EqualTo("$C23"));
						Assert.That(c23.GenerateCode, Is.False);
						Assert.That(c23.ExpandParams, Is.True);
						Assert.That(c23.SkipInInitializer, Is.False);
						Assert.That(c23.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C24", c24 => {
						Assert.That(c24.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
						Assert.That(c24.Name, Is.EqualTo("$C24"));
						Assert.That(c24.GenerateCode, Is.False);
						Assert.That(c24.ExpandParams, Is.False);
						Assert.That(c24.SkipInInitializer, Is.True);
						Assert.That(c24.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C31", c31 => {
						Assert.That(c31.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
						Assert.That(c31.Name, Is.EqualTo("$C31"));
						Assert.That(c31.GenerateCode, Is.False);
						Assert.That(c31.ExpandParams, Is.False);
						Assert.That(c31.SkipInInitializer, Is.False);
						Assert.That(c31.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C32", c32 => {
						Assert.That(c32.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
						Assert.That(c32.Name, Is.EqualTo("$C32"));
						Assert.That(c32.GenerateCode, Is.True);
						Assert.That(c32.ExpandParams, Is.False);
						Assert.That(c32.SkipInInitializer, Is.False);
						Assert.That(c32.OmitUnspecifiedArgumentsFrom, Is.EqualTo(5));
					});

					assert("C33", c33 => {
						Assert.That(c33.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
						Assert.That(c33.Name, Is.EqualTo("$C33"));
						Assert.That(c33.GenerateCode, Is.False);
						Assert.That(c33.ExpandParams, Is.True);
						Assert.That(c33.SkipInInitializer, Is.False);
						Assert.That(c33.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C34", c34 => {
						Assert.That(c34.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
						Assert.That(c34.Name, Is.EqualTo("$C34"));
						Assert.That(c34.GenerateCode, Is.False);
						Assert.That(c34.ExpandParams, Is.False);
						Assert.That(c34.SkipInInitializer, Is.True);
						Assert.That(c34.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("C41", c41 => {
						Assert.That(c41.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.InlineCode));
						Assert.That(c41.LiteralCode, Is.EqualTo("C41_Literal"));
						Assert.That(c41.SkipInInitializer, Is.False);
						Assert.That(c41.NonExpandedFormLiteralCode, Is.EqualTo("C41_Literal"));
					});

					assert("C42", c42 => {
						Assert.That(c42.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.InlineCode));
						Assert.That(c42.LiteralCode, Is.EqualTo("C42_Literal"));
						Assert.That(c42.SkipInInitializer, Is.True);
						Assert.That(c42.NonExpandedFormLiteralCode, Is.EqualTo("C42_NonExpanded"));
					});

					assert("C51", c51 => {
						Assert.That(c51.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.Json));
						Assert.That(c51.ParameterToMemberMap.Select(m => m.Name), Is.EqualTo(new[] { "P1", "F1", "P2", "F2" }));
						Assert.That(c51.SkipInInitializer, Is.False);
					});

					assert("C52", c52 => {
						Assert.That(c52.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.Json));
						Assert.That(c52.ParameterToMemberMap.Select(m => m.Name), Is.EqualTo(new[] { "F2", "P2", "F1", "P1" }));
						Assert.That(c52.SkipInInitializer, Is.True);
					});

					assert("C61", c61 => {
						Assert.That(c61.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NotUsableFromScript));
					});
				}, orig: new MockMetadataImporter {
					GetConstructorSemantics = c => {
						switch (c.ContainingType.Name) {
							case "C11":
								return ConstructorScriptSemantics.Unnamed(false, false, false, null);
							case "C12":
								return ConstructorScriptSemantics.Unnamed( true, false, false, 3);
							case "C13":
								return ConstructorScriptSemantics.Unnamed(false,  true, false, null);
							case "C14":
								return ConstructorScriptSemantics.Unnamed(false, false,  true, null);

							case "C21":
								return ConstructorScriptSemantics.Named("$C21", false, false, false, null);
							case "C22":
								return ConstructorScriptSemantics.Named("$C22",  true, false, false, 4);
							case "C23":
								return ConstructorScriptSemantics.Named("$C23", false,  true, false, null);
							case "C24":
								return ConstructorScriptSemantics.Named("$C24", false, false,  true, null);

							case "C31":
								return ConstructorScriptSemantics.StaticMethod("$C31", false, false, false, null);
							case "C32":
								return ConstructorScriptSemantics.StaticMethod("$C32",  true, false, false, 5);
							case "C33":
								return ConstructorScriptSemantics.StaticMethod("$C33", false,  true, false, null);
							case "C34":
								return ConstructorScriptSemantics.StaticMethod("$C34", false, false,  true, null);

							case "C41":
								return ConstructorScriptSemantics.InlineCode("C41_Literal", false);
							case "C42":
								return ConstructorScriptSemantics.InlineCode("C42_Literal", true, "C42_NonExpanded");

							case "C51": {
								var members = c.ContainingType.GetMembers().ToDictionary(p => p.Name);
								return ConstructorScriptSemantics.Json(new[] { members["P1"], members["F1"], members["P2"], members["F2"] }, false);
							}
							case "C52": {
								var members = c.ContainingType.GetMembers().ToDictionary(p => p.Name);
								return ConstructorScriptSemantics.Json(new[] { members["F2"], members["P2"], members["F1"], members["P1"] }, true);
							}

							case "C61":
								return ConstructorScriptSemantics.NotUsableFromScript();

							default:
								return ConstructorScriptSemantics.NotUsableFromScript();
						}
					},
					AllowGetSemanticsForAccessorMethods = true
				}
			);
		}

		[Test]
		public void CanRoundtripMethodSemantics() {
			RoundtripTest(@"public class C {
			                            public void M11() {} public void M12() {} public void M13() {} public void M14() {} public void M15() {}
			                            public void M21() {} public void M22() {} public void M23() {} public void M24() {} public void M25() {}
			                            public void M31() {} public void M32() {}
			                            public void M41() {}
			                            public int this[int i] { get { return 0; } }
			                            public static C operator+(C a, C b) { return null; }
			                        }",
				(assembly, importer) => {
					var members = assembly.GetTypeByMetadataName("C").GetMembers().ToDictionary(m => m.MetadataName);
					Action<string, Action<MethodScriptSemantics>> assert = (n, a) => a(importer.GetMethodSemantics((IMethodSymbol)members[n]));

					assert("M11", m11 => {
						Assert.That(m11.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(m11.Name, Is.EqualTo("$M11"));
						Assert.That(m11.IgnoreGenericArguments, Is.False);
						Assert.That(m11.GeneratedMethodName, Is.Null);
						Assert.That(m11.ExpandParams, Is.False);
						Assert.That(m11.EnumerateAsArray, Is.False);
						Assert.That(m11.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M12", m12 => {
						Assert.That(m12.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(m12.Name, Is.EqualTo("$M12"));
						Assert.That(m12.IgnoreGenericArguments, Is.True);
						Assert.That(m12.GeneratedMethodName, Is.Null);
						Assert.That(m12.ExpandParams, Is.False);
						Assert.That(m12.EnumerateAsArray, Is.False);
						Assert.That(m12.OmitUnspecifiedArgumentsFrom, Is.EqualTo(4));
					});

					assert("M13", m13 => {
						Assert.That(m13.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(m13.Name, Is.EqualTo("$M13"));
						Assert.That(m13.IgnoreGenericArguments, Is.False);
						Assert.That(m13.GeneratedMethodName, Is.EqualTo("$M13"));
						Assert.That(m13.ExpandParams, Is.False);
						Assert.That(m13.EnumerateAsArray, Is.False);
						Assert.That(m13.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M14", m14 => {
						Assert.That(m14.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(m14.Name, Is.EqualTo("$M14"));
						Assert.That(m14.IgnoreGenericArguments, Is.False);
						Assert.That(m14.GeneratedMethodName, Is.Null);
						Assert.That(m14.ExpandParams, Is.True);
						Assert.That(m14.EnumerateAsArray, Is.False);
						Assert.That(m14.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M15", m15 => {
						Assert.That(m15.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(m15.Name, Is.EqualTo("$M15"));
						Assert.That(m15.IgnoreGenericArguments, Is.False);
						Assert.That(m15.GeneratedMethodName, Is.Null);
						Assert.That(m15.ExpandParams, Is.False);
						Assert.That(m15.EnumerateAsArray, Is.True);
						Assert.That(m15.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M21", m21 => {
						Assert.That(m21.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
						Assert.That(m21.Name, Is.EqualTo("$M21"));
						Assert.That(m21.IgnoreGenericArguments, Is.False);
						Assert.That(m21.GeneratedMethodName, Is.Null);
						Assert.That(m21.ExpandParams, Is.False);
						Assert.That(m21.EnumerateAsArray, Is.False);
						Assert.That(m21.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M22", m22 => {
						Assert.That(m22.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
						Assert.That(m22.Name, Is.EqualTo("$M22"));
						Assert.That(m22.IgnoreGenericArguments, Is.True);
						Assert.That(m22.GeneratedMethodName, Is.Null);
						Assert.That(m22.ExpandParams, Is.False);
						Assert.That(m22.EnumerateAsArray, Is.False);
						Assert.That(m22.OmitUnspecifiedArgumentsFrom, Is.EqualTo(3));
					});

					assert("M23", m23 => {
						Assert.That(m23.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
						Assert.That(m23.Name, Is.EqualTo("$M23"));
						Assert.That(m23.IgnoreGenericArguments, Is.False);
						Assert.That(m23.GeneratedMethodName, Is.EqualTo("$M23"));
						Assert.That(m23.ExpandParams, Is.False);
						Assert.That(m23.EnumerateAsArray, Is.False);
						Assert.That(m23.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M24", m24 => {
						Assert.That(m24.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
						Assert.That(m24.Name, Is.EqualTo("$M24"));
						Assert.That(m24.IgnoreGenericArguments, Is.False);
						Assert.That(m24.GeneratedMethodName, Is.Null);
						Assert.That(m24.ExpandParams, Is.True);
						Assert.That(m24.EnumerateAsArray, Is.False);
						Assert.That(m24.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("M25", m25 => {
						Assert.That(m25.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
						Assert.That(m25.Name, Is.EqualTo("$M25"));
						Assert.That(m25.IgnoreGenericArguments, Is.False);
						Assert.That(m25.GeneratedMethodName, Is.Null);
						Assert.That(m25.ExpandParams, Is.False);
						Assert.That(m25.EnumerateAsArray, Is.True);
						Assert.That(m25.OmitUnspecifiedArgumentsFrom, Is.Null);
					});


					assert("M31", m31 => {
						Assert.That(m31.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
						Assert.That(m31.LiteralCode, Is.EqualTo("M31_Code"));
						Assert.That(m31.EnumerateAsArray, Is.False);
						Assert.That(m31.GeneratedMethodName, Is.Null);
						Assert.That(m31.NonVirtualInvocationLiteralCode, Is.EqualTo("M31_Code"));
						Assert.That(m31.NonExpandedFormLiteralCode, Is.EqualTo("M31_Code"));
					});

					assert("M32", m32 => {
						Assert.That(m32.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
						Assert.That(m32.LiteralCode, Is.EqualTo("M32_Code"));
						Assert.That(m32.EnumerateAsArray, Is.True);
						Assert.That(m32.GeneratedMethodName, Is.EqualTo("M32_Generated"));
						Assert.That(m32.NonVirtualInvocationLiteralCode, Is.EqualTo("M32_NonVirtual"));
						Assert.That(m32.NonExpandedFormLiteralCode, Is.EqualTo("M32_NonExpanded"));
					});


					assert("M41", m41 => {
						Assert.That(m41.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NotUsableFromScript));
					});


					assert("get_Item", indexer => {
						Assert.That(indexer.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
					});


					assert("op_Addition", oper => {
						Assert.That(oper.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeOperator));
					});
				},
				new MockMetadataImporter {
					GetMethodSemantics = m => {
						switch (m.Name) {
							case "M11":
								return MethodScriptSemantics.NormalMethod("$M11", false, false, false, false, null);
							case "M12":
								return MethodScriptSemantics.NormalMethod("$M12",  true, false, false, false, 4);
							case "M13":
								return MethodScriptSemantics.NormalMethod("$M13", false,  true, false, false, null);
							case "M14":
								return MethodScriptSemantics.NormalMethod("$M14", false, false,  true, false, null);
							case "M15":
								return MethodScriptSemantics.NormalMethod("$M15", false, false, false,  true, null);

							case "M21":
								return MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$M21", false, false, false, false, null);
							case "M22":
								return MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$M22",  true, false, false, false, 3);
							case "M23":
								return MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$M23", false,  true, false, false, null);
							case "M24":
								return MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$M24", false, false,  true, false, null);
							case "M25":
								return MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$M25", false, false, false,  true, null);

							case "M31":
								return MethodScriptSemantics.InlineCode("M31_Code", false, null, null, null);
							case "M32":
								return MethodScriptSemantics.InlineCode("M32_Code",  true, "M32_Generated", "M32_NonVirtual", "M32_NonExpanded");

							case "M41":
								return MethodScriptSemantics.NotUsableFromScript();

							case "get_Item":
								return MethodScriptSemantics.NativeIndexer();

							case "op_Addition":
								return MethodScriptSemantics.NativeOperator();

							default:
								return MethodScriptSemantics.NotUsableFromScript();
								throw new ArgumentException("m");
						}
					},
					AllowGetSemanticsForAccessorMethods = true
				}
			);
		}

		[Test]
		public void CanRoundtripPropertySemantics() {
			MockMetadataImporter orig = null;
			orig = new MockMetadataImporter {
					GetPropertySemantics = p => {
						switch (p.Name) {
							case "P1":
								return PropertyScriptSemantics.Field("$P1");
							case "P2":
							case "P3":
							case "P4":
								return PropertyScriptSemantics.GetAndSetMethods(p.GetMethod != null ? orig.GetMethodSemantics(p.GetMethod) : null, p.SetMethod != null ? orig.GetMethodSemantics(p.SetMethod) : null);
							case "P5":
								return PropertyScriptSemantics.NotUsableFromScript();
							default:
								throw new ArgumentException("p");
						}
					},
					GetMethodSemantics = m => {
						if (m.AssociatedSymbol == null)
							throw new ArgumentException("m");

						switch (m.AssociatedSymbol.Name) {
							case "P1":
							case "P5":
								return MethodScriptSemantics.NotUsableFromScript();
							case "P2":
							case "P3":
							case "P4":
								return MethodScriptSemantics.NormalMethod("$" + m.Name);
							default:
								throw new ArgumentException("m");
						}
					},
					AllowGetSemanticsForAccessorMethods = true
				};

			RoundtripTest(@"public class C { public int P1 { get; set; } public int P2 { get; set; } public int P3 { get { return 0; } } public int P4 { set {} } public int P5 { get; set; } }",
				(assembly, importer) => {
					var members = assembly.GetTypeByMetadataName("C").GetMembers().ToDictionary(m => m.MetadataName);
					Action<string, Action<PropertyScriptSemantics>> assert = (n, a) => a(importer.GetPropertySemantics((IPropertySymbol)members[n]));

					assert("P1", p1 => {
						Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
						Assert.That(p1.FieldName, Is.EqualTo("$P1"));
					});

					assert("P2", p2 => {
						Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
						Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(p2.GetMethod.Name, Is.EqualTo("$get_P2"));
						Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(p2.SetMethod.Name, Is.EqualTo("$set_P2"));
					});

					assert("P3", p3 => {
						Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
						Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(p3.GetMethod.Name, Is.EqualTo("$get_P3"));
						Assert.That(p3.SetMethod, Is.Null);
					});

					assert("P4", p2 => {
						Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
						Assert.That(p2.GetMethod, Is.Null);
						Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(p2.SetMethod.Name, Is.EqualTo("$set_P4"));
					});

					assert("P5", p5 => {
						Assert.That(p5.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
					});
				},
				orig
			);
		}

		[Test]
		public void CanRoundtripEventSemantics() {
			MockMetadataImporter orig = null;
			orig = new MockMetadataImporter {
					GetEventSemantics = e => {
						switch (e.Name) {
							case "E1":
								return EventScriptSemantics.AddAndRemoveMethods(orig.GetMethodSemantics(e.AddMethod), orig.GetMethodSemantics(e.RemoveMethod));
							case "E2":
								return EventScriptSemantics.NotUsableFromScript();
							default:
								throw new ArgumentException("e");
						}
					},
					GetMethodSemantics = m => {
						if (m.AssociatedSymbol == null)
							throw new ArgumentException("m");

						switch (m.AssociatedSymbol.Name) {
							case "E1":
								return MethodScriptSemantics.NormalMethod("$" + m.Name);
							case "E2":
								return MethodScriptSemantics.NotUsableFromScript();
							default:
								throw new ArgumentException("m");
						}
					},
					AllowGetSemanticsForAccessorMethods = true
				};

			RoundtripTest(@"public class C { public event System.Action E1, E2; }",
				(assembly, importer) => {
					var members = assembly.GetTypeByMetadataName("C").GetMembers().ToDictionary(m => m.MetadataName);
					Action<string, Action<EventScriptSemantics>> assert = (n, a) => a(importer.GetEventSemantics((IEventSymbol)members[n]));

					assert("E1", e1 => {
						Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
						Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(e1.AddMethod.Name, Is.EqualTo("$add_E1"));
						Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
						Assert.That(e1.RemoveMethod.Name, Is.EqualTo("$remove_E1"));
					});

					assert("E2", e2 => {
						Assert.That(e2.Type, Is.EqualTo(EventScriptSemantics.ImplType.NotUsableFromScript));
					});
				},
				orig
			);
		}

		[Test]
		public void CanRoundtripDelegateSemantics() {
			RoundtripTest("public delegate void D1(); public delegate void D2(); public delegate void D3();",
				(assembly, importer) => {
					Action<string, Action<DelegateScriptSemantics>> assert = (n, a) => a(importer.GetDelegateSemantics((assembly.GetTypeByMetadataName(n))));

					assert("D1", d1 => {
						Assert.That(d1.ExpandParams, Is.False);
						Assert.That(d1.BindThisToFirstParameter, Is.False);
						Assert.That(d1.OmitUnspecifiedArgumentsFrom, Is.Null);
					});

					assert("D2", d2 => {
						Assert.That(d2.ExpandParams, Is.True);
						Assert.That(d2.BindThisToFirstParameter, Is.False);
						Assert.That(d2.OmitUnspecifiedArgumentsFrom, Is.EqualTo(3));
					});

					assert("D3", d3 => {
						Assert.That(d3.ExpandParams, Is.False);
						Assert.That(d3.BindThisToFirstParameter, Is.True);
						Assert.That(d3.OmitUnspecifiedArgumentsFrom, Is.Null);
					});
				},
				new MockMetadataImporter {
					GetDelegateSemantics = d => {
						switch (d.Name) {
							case "D1":
								return new DelegateScriptSemantics(false, false, null);
							case "D2":
								return new DelegateScriptSemantics( true, false,    3);
							case "D3":
								return new DelegateScriptSemantics(false,  true, null);
							default:
								throw new ArgumentException("d");
						}
					}
				}
			);
		}

		[Test]
		public void CanRoundtripFieldSemantics() {
			RoundtripTest("public class C { public int F1, F2, F3, F4, F5, F6, F7, F8, F9, F10; }",
				(assembly, importer) => {
					var members = assembly.GetTypeByMetadataName("C").GetMembers().ToDictionary(m => m.MetadataName);
					Action<string, Action<FieldScriptSemantics>> assert = (n, a) => a(importer.GetFieldSemantics((IFieldSymbol)members[n]));

					assert("F1", f1 => {
						Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
						Assert.That(f1.Name, Is.EqualTo("$F1"));
					});

					assert("F2", f2 => {
						Assert.That(f2.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f2.Value, Is.True);
						Assert.That(f2.Name, Is.Null);
					});

					assert("F3", f3 => {
						Assert.That(f3.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f3.Value, Is.False);
						Assert.That(f3.Name, Is.EqualTo("$F3"));
					});

					assert("F4", f4 => {
						Assert.That(f4.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f4.Value, Is.Null);
						Assert.That(f4.Name, Is.Null);
					});

					assert("F5", f5 => {
						Assert.That(f5.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f5.Value, Is.Null);
						Assert.That(f5.Name, Is.EqualTo("$F5"));
					});

					assert("F6", f6 => {
						Assert.That(f6.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f6.Value, Is.EqualTo(1));
						Assert.That(f6.Name, Is.Null);
					});

					assert("F7", f7 => {
						Assert.That(f7.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f7.Value, Is.EqualTo(1.5));
						Assert.That(f7.Name, Is.EqualTo("$F7"));
					});

					assert("F8", f8 => {
						Assert.That(f8.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f8.Value, Is.EqualTo("Value 8"));
						Assert.That(f8.Name, Is.Null);
					});

					assert("F9", f9 => {
						Assert.That(f9.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
						Assert.That(f9.Value, Is.EqualTo("Value 9"));
						Assert.That(f9.Name, Is.EqualTo("$F9"));
					});

					assert("F10", f10 => {
						Assert.That(f10.Type, Is.EqualTo(FieldScriptSemantics.ImplType.NotUsableFromScript));
					});
				},
				new MockMetadataImporter {
					GetFieldSemantics = f => {
						switch (f.Name) {
							case "F1":
								return FieldScriptSemantics.Field("$F1");
							case "F2":
								return FieldScriptSemantics.BooleanConstant(true);
							case "F3":
								return FieldScriptSemantics.BooleanConstant(false, "$F3");
							case "F4":
								return FieldScriptSemantics.NullConstant();
							case "F5":
								return FieldScriptSemantics.NullConstant("$F5");
							case "F6":
								return FieldScriptSemantics.NumericConstant(1);
							case "F7":
								return FieldScriptSemantics.NumericConstant(1.5, "$F7");
							case "F8":
								return FieldScriptSemantics.StringConstant("Value 8");
							case "F9":
								return FieldScriptSemantics.StringConstant("Value 9", "$F9");
							case "F10":
								return FieldScriptSemantics.NotUsableFromScript();
							default:
								throw new ArgumentException("f");
						}
					}
				}
			);
		}

		private string TypeToString(ITypeSymbol type) {
			var at = type as IArrayTypeSymbol;
			if (at != null) {
				return TypeToString(at.ElementType) + at.Rank.ToString(CultureInfo.InvariantCulture);
			}

			var nt = type as INamedTypeSymbol;
			if (nt != null) {
				return (type.ContainingType != null ? TypeToString(type.ContainingType) : "") + type.Name + string.Join("", nt.TypeArguments.Select(TypeToString));
			}

			return type.MetadataName;
		}

		private string ParameterTypeToString(IParameterSymbol parameter) {
			return TypeToString(parameter.Type) + (parameter.RefKind != RefKind.None ? "R" : "");
		}

		[Test]
		public void CanMatchMethodsBySignature() {
			RoundtripTest(@"
				using System.Collections.Generic;
				public class X {}
				public class C<T1, T2> {
					public class X {}
					public class X<T3> {}

					public void M1() {}
					public int M2() { return 0; }
					public int M2(int x) { return 0; }
					public int M2(ref int x) { return 0; }
					public int M3(out int x) { x = 0; return 0; }

					public void M4(int[] a) {}
					public void M4(int[][] a) {}
					public void M4(int[,] a) {}
					public void M4(int[,,] a) {}
					public void M4(int[,,][] a) {}

					public void M5(T1 t1) {}
					public void M5(ref T1 t1) {}
					public void M5(T1[,][] t1) {}
					public void M5(T2 t2) {}
					public void M5(T1 t1, T2 t2) {}
					public void M5(T2 t2, T1 t1) {}
					public void M5<T3, T4>(T3 t3) {}
					public void M5<T3, T4>(T4 t4) {}
					public void M5<T3, T4>(T3 t3, T4 t4) {}
					public void M5<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) {}

					public void M6(List<int> l) {}
					public void M6(List<string> l) {}
					public void M6(List<T1> l) {}
					public void M6<T>(List<T> l) {}
					public void M6(Dictionary<string, int> l) {}
					public void M6(Dictionary<T1, int> l) {}

					public void M7(int a) {}
					public void M7(X x) {}
					public void M7(global::X x) {}
					public void M7(X<int> x) {}

					public static implicit operator int(C<T1, T2> c) { return 0; }
					public static implicit operator string(C<T1, T2> c) { return null; }
				}",
				(assembly, importer) => {
					var members = assembly.GetTypeByMetadataName("C`2").GetMembers().OfType<IMethodSymbol>().ToDictionary(m => m.Name + "_" + TypeToString(m.ReturnType) + m.Parameters.Aggregate("", (old, p) => old + "_" + ParameterTypeToString(p)));
					Action<string> assert = n => Assert.That(importer.GetMethodSemantics(members[n]).Name, Is.EqualTo(n));

					assert("M1_Void");

					assert("M2_Int32");
					assert("M2_Int32_Int32");
					assert("M2_Int32_Int32R");

					assert("M3_Int32_Int32R");

					assert("M4_Void_Int321");
					assert("M4_Void_Int3211");
					assert("M4_Void_Int322");
					assert("M4_Void_Int323");
					assert("M4_Void_Int3213");

					assert("M5_Void_T1");
					assert("M5_Void_T1R");
					assert("M5_Void_T112");
					assert("M5_Void_T2");
					assert("M5_Void_T1_T2");
					assert("M5_Void_T2_T1");
					assert("M5_Void_T3");
					assert("M5_Void_T4");
					assert("M5_Void_T3_T4");
					assert("M5_Void_T1_T2_T3_T4");

					assert("M6_Void_ListInt32");
					assert("M6_Void_ListString");
					assert("M6_Void_ListT1");
					assert("M6_Void_ListT");
					assert("M6_Void_DictionaryStringInt32");
					assert("M6_Void_DictionaryT1Int32");

					assert("M7_Void_Int32");
					assert("M7_Void_CT1T2X");
					assert("M7_Void_X");
					assert("M7_Void_CT1T2XInt32");

					assert("op_Implicit_Int32_CT1T2");
					assert("op_Implicit_String_CT1T2");
				},
				new MockMetadataImporter {
					GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Name + "_" + TypeToString(m.ReturnType) + m.Parameters.Aggregate("", (old, p) => old + "_" + ParameterTypeToString(p)))
				}
			);
		}

		[Test]
		public void CanMatchPropertiesBySignature() {
			MockMetadataImporter orig = null;
			orig = new MockMetadataImporter {
				GetPropertySemantics = p => p.Parameters.Length == 1 && p.Parameters[0].Type.MetadataName == "String" ? PropertyScriptSemantics.NotUsableFromScript() : PropertyScriptSemantics.GetAndSetMethods(orig.GetMethodSemantics(p.GetMethod), orig.GetMethodSemantics(p.SetMethod)),
				GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Name + m.Parameters.Aggregate("", (old, p) => old + "_" + ParameterTypeToString(p))),
				AllowGetSemanticsForAccessorMethods = true,
			};

			RoundtripTest(@"
				using System.Collections.Generic;
				public class C {
					public int this[int x] { get { return 0; } set {} }
					public int this[string x] { get { return 0; } set {} }
					public int this[string x, int y] { get { return 0; } set {} }
				}",
				(assembly, importer) => {
					var members = assembly.GetTypeByMetadataName("C").GetMembers().OfType<IPropertySymbol>().ToDictionary(m => string.Join("_", m.Parameters.Select(ParameterTypeToString)));
					Action<string> assert = n => {
					};

					var p1 = importer.GetPropertySemantics(members["Int32"]);
					Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
					Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
					Assert.That(p1.GetMethod.Name, Is.EqualTo("get_Item_Int32"));
					Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
					Assert.That(p1.SetMethod.Name, Is.EqualTo("set_Item_Int32_Int32"));

					var p2 = importer.GetPropertySemantics(members["String"]);
					Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));

					var p3 = importer.GetPropertySemantics(members["String_Int32"]);
					Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
					Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
					Assert.That(p3.GetMethod.Name, Is.EqualTo("get_Item_String_Int32"));
					Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
					Assert.That(p3.SetMethod.Name, Is.EqualTo("set_Item_String_Int32_Int32"));
				},
				orig);
		}

		[Test]
		public void NestedTypesWork() {
			RoundtripTest(
				@"public class C {
					public class D {
						public int F1;
						public class E {
							public int F2;
						}
					}
					public class F<T1> {
						public int F3;
						public class G<T2> {
							public int F4;
						}
					}
				}",
				(assembly, importer) => {
					var types = assembly.GetAllTypes().ToDictionary(t => t.Name);
					Action<string, string> asserter = (t, f) => { var sem = importer.GetFieldSemantics(types[t].GetMembers(f).OfType<IFieldSymbol>().Single()); Assert.That(sem.Name, Is.EqualTo("$" + f)); };

					asserter("D", "F1");
					asserter("E", "F2");
					asserter("F", "F3");
					asserter("G", "F4");
				}
			);
		}

		[Test]
		public void APublicDefaultConstructorIsCreatedForStructsIfNoneExists() {
			RoundtripTest(
				@"public struct S1 {} public struct S2 {}",
				(assembly, importer) => {
					var s1 = assembly.GetTypeByMetadataName("S1");
					var ctor1 = s1.GetConstructors().Single();
					var sem1 = importer.GetConstructorSemantics(ctor1);
					Assert.That(sem1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));

					var s2 = assembly.GetTypeByMetadataName("S2");
					var ctor2 = s2.GetConstructors().Single();
					var sem2 = importer.GetConstructorSemantics(ctor2);
					Assert.That(sem2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
					Assert.That(sem2.Name, Is.EqualTo("SomeCtor"));
				}, new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "S1" ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("SomeCtor") });
		}

		[Test]
		public void WritesScriptSerializableAttributeForSerializableTypes() {
			RoundtripTest(
				@"[System.Serializable] public class C1 {} [System.Serializable(TypeCheckCode = ""hello"")] public class C2 {}",
				(assembly, importer) => {
					var c1 = assembly.GetTypeByMetadataName("C1");
					var a1 = c1.GetAttributes().SingleOrDefault(a => a.AttributeClass.FullyQualifiedName() == "System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute");
					Assert.That(a1, Is.Not.Null);
					Assert.That(a1.ConstructorArguments, Has.Length.EqualTo(1));
					Assert.That(a1.ConstructorArguments[0].Value, Is.Null);
					Assert.That(a1.NamedArguments, Is.Empty);

					var c2 = assembly.GetTypeByMetadataName("C2");
					var a2 = c2.GetAttributes().SingleOrDefault(a => a.AttributeClass.FullyQualifiedName() == "System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute");
					Assert.That(a2, Is.Not.Null);
					Assert.That(a2.ConstructorArguments, Has.Length.EqualTo(1));
					Assert.That(a2.ConstructorArguments[0].Value, Is.EqualTo("hello"));
					Assert.That(a2.NamedArguments, Is.Empty);
				});
		}
	}
}
