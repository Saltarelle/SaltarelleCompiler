using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable CheckNamespace
namespace Saltarelle.Compiler.Tests.MetadataWriteBackEngineTests.MetadataWriteBackEngineTestCase {
// ReSharper restore CheckNamespace

	[Obsolete("This class has an attribute")]
	public class ClassWithAttribute {
	}

	public class ClassWithField {
		[Obsolete("This field has an attribute")] public int MyField;
	}
}
