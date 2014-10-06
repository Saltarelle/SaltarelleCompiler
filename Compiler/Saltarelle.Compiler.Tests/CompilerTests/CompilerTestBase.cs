using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Tests.CompilerTests {
	public class CompilerTestBase {
		protected ReadOnlyCollection<JsType> CompiledTypes { get; private set; }

		protected void AssertCorrect(JsStatement stmt, string expected) {
			var actual = OutputFormatter.Format(stmt, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		protected void AssertCorrect(JsExpression expr, string expected) {
			var actual = OutputFormatter.Format(expr, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		protected void Compile(IEnumerable<string> sources, IMetadataImporter metadataImporter = null, INamer namer = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, Action<IMethodSymbol, JsFunctionDefinitionExpression, MethodCompiler> methodCompiled = null, IList<string> defineConstants = null, IEnumerable<MetadataReference> references = null) {
			bool defaultErrorHandling = false;
			if (errorReporter == null) {
				defaultErrorHandling = true;
				errorReporter = new MockErrorReporter(true);
			}

			var compiler = new Saltarelle.Compiler.Compiler.Compiler(metadataImporter ?? new MockMetadataImporter(), namer ?? new MockNamer(), runtimeLibrary ?? new MockRuntimeLibrary(), errorReporter);
			if (methodCompiled != null)
				compiler.MethodCompiled += methodCompiled;

			var c = Common.CreateCompilation(sources, references, defineConstants);

			CompiledTypes = compiler.Compile(c).AsReadOnly();
			if (defaultErrorHandling) {
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Compile should not generate errors");
			}
		}

		protected void Compile(params string[] sources) {
			Compile((IEnumerable<string>)sources);
		}

		protected string Stringify(JsExpression expression) {
			return OutputFormatter.Format(expression, allowIntermediates: true);
		}

		protected JsClass FindClass(string name) {
			var result = CompiledTypes.SingleOrDefault(t => t.CSharpTypeDefinition.Name == name);
			if (result == null) Assert.Fail("Could not find type " + name);
			if (!(result is JsClass)) Assert.Fail("Found type is not a JsClass, it is a " + result.GetType().Name);
			return (JsClass)result;
		}

		protected JsEnum FindEnum(string name) {
			var result = CompiledTypes.SingleOrDefault(t => t.CSharpTypeDefinition.Name == name);
			if (result == null) Assert.Fail("Could not find type " + name);
			if (!(result is JsEnum)) Assert.Fail("Found type is not a JsEnum, it is a " + result.GetType().Name);
			return (JsEnum)result;
		}

		protected JsMethod FindInstanceMethod(string name) {
			var lastDot = name.LastIndexOf('.');
			var cls = FindClass(name.Substring(0, lastDot));
			return cls.InstanceMethods.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
		}

		protected string FindInstanceFieldInitializer(string name) {
			var lastDot = name.LastIndexOf('.');
			var cls = FindClass(name.Substring(0, lastDot));
			return cls.UnnamedConstructor.Body.Statements
			                                  .OfType<JsExpressionStatement>()
			                                  .Select(s => s.Expression)
			                                  .OfType<JsInvocationExpression>()
			                                  .Where(call =>    call.Method is JsIdentifierExpression
			                                                 && ((JsIdentifierExpression)call.Method).Name == "$Init"
			                                                 && call.Arguments[0] is JsThisExpression
			                                                 && call.Arguments[1] is JsConstantExpression && ((JsConstantExpression)call.Arguments[1]).StringValue == name.Substring(lastDot + 1))
			                                  .Select(call => OutputFormatter.Format(call.Arguments[2], allowIntermediates: true))
			                                  .SingleOrDefault();
		}

		protected string FindStaticFieldInitializer(string name) {
			var lastDot = name.LastIndexOf('.');
			var cls = FindClass(name.Substring(0, lastDot));
			return cls.StaticInitStatements.OfType<JsExpressionStatement>()
			                               .Select(s => s.Expression)
			                               .OfType<JsInvocationExpression>()
			                                  .Where(call =>    call.Method is JsIdentifierExpression
			                                                 && ((JsIdentifierExpression)call.Method).Name == "$Init"
			                                                 && call.Arguments[1] is JsConstantExpression && ((JsConstantExpression)call.Arguments[1]).StringValue == name.Substring(lastDot + 1))
			                               .Select(call => OutputFormatter.Format(call.Arguments[2], allowIntermediates: true))
			                               .SingleOrDefault();
		}

		protected JsMethod FindStaticMethod(string name) {
			var lastDot = name.LastIndexOf('.');
			var cls = FindClass(name.Substring(0, lastDot));
			return cls.StaticMethods.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
		}

		protected JsNamedConstructor FindNamedConstructor(string name) {
			var lastDot = name.LastIndexOf('.');
			var cls = FindClass(name.Substring(0, lastDot));
			return cls.NamedConstructors.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
		}
	}
}
