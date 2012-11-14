using System.Collections.Generic;

namespace TypeScriptModel {
	public class Globals {
		public IReadOnlyCollection<Member> Members { get; private set; }
		public IReadOnlyCollection<Module> Modules { get; private set; }

		public Globals(IEnumerable<Member> members, IEnumerable<Module> modules) {
			Members = new List<Member>(members).AsReadOnly();
			Modules = new List<Module>(modules).AsReadOnly();
		}
	}
}