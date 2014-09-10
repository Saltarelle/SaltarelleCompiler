using System;
using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;

namespace Saltarelle.Compiler.Tests.OOPEmulatorInvokerTests {
	public class MockOOPEmulator : IOOPEmulator {
		public Func<IEnumerable<JsType>, IReadOnlyList<AssemblyResource>, IEnumerable<JsStatement>> GetCodeBeforeFirstType { get; set; }
		public Func<IEnumerable<JsType>, IReadOnlyList<AssemblyResource>, IEnumerable<JsStatement>> GetCodeAfterLastType { get; set; }
		public Func<JsType, TypeOOPEmulation> EmulateType { get; set; }
		public Func<JsClass, IEnumerable<JsStatement>> GetStaticInitStatements { get; set; }

		public MockOOPEmulator() {
			GetCodeBeforeFirstType  = (_, __) => new JsStatement[0];
			GetCodeAfterLastType    = (_, __) => new JsStatement[0];
			EmulateType             = _ => new TypeOOPEmulation(null);
			GetStaticInitStatements = _ => new JsStatement[0];
		}
		
		IEnumerable<JsStatement> IOOPEmulator.GetCodeBeforeFirstType(IEnumerable<JsType> types, IReadOnlyList<AssemblyResource> resources) {
			return GetCodeBeforeFirstType(types, resources);
		}

		TypeOOPEmulation IOOPEmulator.EmulateType(JsType type) {
			return EmulateType(type);
		}

		IEnumerable<JsStatement> IOOPEmulator.GetCodeAfterLastType(IEnumerable<JsType> types, IReadOnlyList<AssemblyResource> resources) {
			return GetCodeAfterLastType(types, resources);
		}

		IEnumerable<JsStatement> IOOPEmulator.GetStaticInitStatements(JsClass type) {
			return GetStaticInitStatements(type);
		}
	}
}