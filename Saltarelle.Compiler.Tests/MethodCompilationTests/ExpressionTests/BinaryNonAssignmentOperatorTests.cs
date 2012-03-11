using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class BinaryNonAssignmentOperatorTests : MethodCompilerTestBase {
		protected new void AssertCorrect(string csharp, string expected, INamingConventionResolver namingConvention = null) {
			// Division, shift right and coalesce need dedicated tests.
			foreach (var op in new[] { "*", "%", "+", "-", "<<", "<", ">", "<=", ">=", "==", "!=", "&", "^", "|", "&&", "||" }) {
				var jsOp = (op == "==" || op == "!=" ? op + "=" : op);	// Script should use strict equals (===) rather than normal equals (==)
				base.AssertCorrect(csharp.Replace("+", op), expected.Replace("+", jsOp), namingConvention);
			}
		}

		[Test]
		public void TODO() {
			Assert.Fail("TODO: Bulk, division, right shift, coalesce, all as lifting or non-lifting versions.");
		}
	}
}
