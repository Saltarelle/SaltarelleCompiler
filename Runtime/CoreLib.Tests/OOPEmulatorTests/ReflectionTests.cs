﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class ReflectionTests : OOPEmulatorTestBase {
		[Test]
		public void ReflectionOnUnusableConstructorIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[NonScriptable, Reflectable] public C1() {}
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7200 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnNativeOperatorMethodIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[IntrinsicOperator, Reflectable] public static C1 operator *(C1 x, C1 y) { return null; }
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7201 && m.FormattedMessage.Contains("C1.op_Multiply") && m.FormattedMessage.Contains("method") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnUnusableMethodIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[NonScriptable, Reflectable] public void M() {}
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7201 && m.FormattedMessage.Contains("C1.M") && m.FormattedMessage.Contains("method") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnInlineConstantFieldIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[InlineConstant, Reflectable] public const int F = 0;
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7201 && m.FormattedMessage.Contains("C1.F") && m.FormattedMessage.Contains("field") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnUnusableFieldIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[NonScriptable, Reflectable] public int F = 0;
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7201 && m.FormattedMessage.Contains("C1.F") && m.FormattedMessage.Contains("field") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnUnusablePropertyIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[NonScriptable, Reflectable] public int P { get; set; }
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7201 && m.FormattedMessage.Contains("C1.P") && m.FormattedMessage.Contains("property") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnPropertyWithUnusableGetterIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[Reflectable] public int P { [NonScriptable] get; set; }
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7202 && m.FormattedMessage.Contains("C1.P") && m.FormattedMessage.Contains("property") && m.FormattedMessage.Contains("getter") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnPropertyWithUnusableSetterIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[Reflectable] public int P { get; [NonScriptable] set; }
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7202 && m.FormattedMessage.Contains("C1.P") && m.FormattedMessage.Contains("property") && m.FormattedMessage.Contains("setter") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnUnusableEventIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[NonScriptable, Reflectable] public event System.Action E;
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7201 && m.FormattedMessage.Contains("C1.E") && m.FormattedMessage.Contains("event") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnEventWithUnusableAdderIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[Reflectable] public event System.Action E { [NonScriptable] add {} remove {} }
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7202 && m.FormattedMessage.Contains("C1.E") && m.FormattedMessage.Contains("event") && m.FormattedMessage.Contains("add accessor") && m.FormattedMessage.Contains("reflection")));
		}

		[Test]
		public void ReflectionOnEventWithUnusableRemoverIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public class C1 {
	[Reflectable] public event System.Action E { add {} [NonScriptable] remove {} }
}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7202 && m.FormattedMessage.Contains("C1.E") && m.FormattedMessage.Contains("event") && m.FormattedMessage.Contains("remove accessor") && m.FormattedMessage.Contains("reflection")));
		}

		private static readonly Regex _typerefRe = new Regex(@"{([a-zA-Z0-9]+)}");
		private string RemoveTypeReferences(string source) {
			return _typerefRe.Replace(source, "$1");
		}

		[Test]
		public void DefaultMemberReflectabilityAttributeCanBeSpecifiedOnAssemblyButCanBeOverriddenOnTypes() {
			string s = Process(@"
using System.Runtime.CompilerServices;
[assembly: DefaultMemberReflectability(MemberReflectability.PublicAndProtected)]
public class C1 {
	public int P1 { get; set; }
	[Reflectable]
	public int P2 { get; set; }
	[Reflectable(false)]
	public int P3 { get; set; }
	private int P4 { get; set; }
}
[DefaultMemberReflectability(MemberReflectability.None)]
public class C2 {
	public int P1 { get; set; }
	[Reflectable]
	public int P2 { get; set; }
}");
			var c1 = (JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)JavaScriptParser.Parser.ParseStatement(RemoveTypeReferences(s.Replace("\r\n", "\n").Split('\n').Single(l => l.StartsWith("{Script}.setMetadata($C1"))))).Expression).Arguments[1];
			var c2 = (JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)JavaScriptParser.Parser.ParseStatement(RemoveTypeReferences(s.Replace("\r\n", "\n").Split('\n').Single(l => l.StartsWith("{Script}.setMetadata($C2"))))).Expression).Arguments[1];

			var c1Members = ((JsArrayLiteralExpression)c1.Values.Single(p => p.Name == "members").Value).Elements.Cast<JsObjectLiteralExpression>().ToList();
			Assert.That( c1Members.Any(m => m.Values.Any(v => v.Name == "name" && ((JsConstantExpression)v.Value).StringValue == "P1")));
			Assert.That( c1Members.Any(m => m.Values.Any(v => v.Name == "name" && ((JsConstantExpression)v.Value).StringValue == "P2")));
			Assert.That(!c1Members.Any(m => m.Values.Any(v => v.Name == "name" && ((JsConstantExpression)v.Value).StringValue == "P3")));
			Assert.That(!c1Members.Any(m => m.Values.Any(v => v.Name == "name" && ((JsConstantExpression)v.Value).StringValue == "P4")));

			var c2Members = ((JsArrayLiteralExpression)c2.Values.Single(p => p.Name == "members").Value).Elements.Cast<JsObjectLiteralExpression>().ToList();
			Assert.That(!c2Members.Any(m => m.Values.Any(v => v.Name == "name" && ((JsConstantExpression)v.Value).StringValue == "P1")));
			Assert.That( c2Members.Any(m => m.Values.Any(v => v.Name == "name" && ((JsConstantExpression)v.Value).StringValue == "P2")));
		}
	}
}
