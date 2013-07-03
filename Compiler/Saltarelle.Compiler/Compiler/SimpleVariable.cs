using System.Diagnostics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
	public class SimpleVariable : IVariable {
		readonly DomRegion region;
		readonly IType type;
		readonly string name;
			
		public SimpleVariable(IType type, string name, DomRegion region) {
			Debug.Assert(type != null);
			Debug.Assert(name != null);
			Debug.Assert(region != null);
			this.type = type;
			this.name = name;
			this.region = region;
		}

		public SymbolKind SymbolKind { get { return SymbolKind.Variable; } }

		public string Name {
			get { return name; }
		}
			
		public DomRegion Region {
			get { return region; }
		}
			
		public IType Type {
			get { return type; }
		}
			
		public bool IsConst {
			get { return false; }
		}
			
		public object ConstantValue {
			get { return null; }
		}
			
		public override string ToString()
		{
			return type.ToString() + " " + name + ";";
		}
	}
}