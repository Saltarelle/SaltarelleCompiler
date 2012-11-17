using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Globals {
		public IReadOnlyCollection<Member> Members { get; private set; }
		public IReadOnlyCollection<Interface> Interfaces { get; private set; }
		public IReadOnlyCollection<Module> Modules { get; private set; }

		public Globals(IEnumerable<Member> members, IEnumerable<Module> modules, IEnumerable<Interface> interfaces) {
			Members    = new List<Member>(members).AsReadOnly();
			Interfaces = new List<Interface>(interfaces).AsReadOnly();
			Modules    = new List<Module>(modules).AsReadOnly();
		}
	}
}