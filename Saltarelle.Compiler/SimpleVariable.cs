using System.Diagnostics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	internal class SimpleVariable : IVariable {
		readonly DomRegion region;
		readonly IType type;
		readonly string name;
			
		public SimpleVariable(IType type, string name) {
			Debug.Assert(type != null);
			Debug.Assert(name != null);
			this.type = type;
			this.name = name;
		}
			
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