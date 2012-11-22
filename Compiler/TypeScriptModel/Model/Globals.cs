using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Globals {
		public IReadOnlyList<Module> Modules { get; private set; }
		public IReadOnlyList<Interface> Interfaces { get; private set; }
		public IReadOnlyList<Member> Members { get; private set; }

		public Globals(IEnumerable<Module> modules, IEnumerable<Interface> interfaces, IEnumerable<Member> members) {
			Modules    = new List<Module>(modules).AsReadOnly();
			Interfaces = new List<Interface>(interfaces).AsReadOnly();
			Members    = new List<Member>(members).AsReadOnly();
		}
	}
}