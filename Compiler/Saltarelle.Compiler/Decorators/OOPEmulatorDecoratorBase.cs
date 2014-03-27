using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;

namespace Saltarelle.Compiler.Decorators {
	public abstract class OOPEmulatorDecoratorBase : IOOPEmulator {
		private readonly IOOPEmulator _prev;

		protected OOPEmulatorDecoratorBase(IOOPEmulator prev) {
			this._prev = prev;
		}

		public virtual IEnumerable<JsStatement> GetCodeBeforeFirstType(IEnumerable<JsType> types) {
			return _prev.GetCodeBeforeFirstType(types);
		}

		public virtual TypeOOPEmulation EmulateType(JsType type) {
			return _prev.EmulateType(type);
		}

		public virtual IEnumerable<JsStatement> GetCodeAfterLastType(IEnumerable<JsType> types) {
			return _prev.GetCodeAfterLastType(types);
		}

		public virtual IEnumerable<JsStatement> GetStaticInitStatements(JsClass type) {
			return _prev.GetStaticInitStatements(type);
		}
	}
}
