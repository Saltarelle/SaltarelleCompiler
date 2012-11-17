using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Module {
		public string Name { get; private set; }
		public IReadOnlyCollection<ModuleImport> Imports { get; private set; }
		public IReadOnlyCollection<Interface> Interfaces { get; private set; }
		public IReadOnlyCollection<Member> Members { get; private set; }

		public Module(string name, IEnumerable<ModuleImport> imports, IEnumerable<Member> members, IEnumerable<Interface> interfaces) {
			Name       = name;
			Imports    = new List<ModuleImport>(imports).AsReadOnly();
			Interfaces = new List<Interface>(interfaces).AsReadOnly();
			Members    = new List<Member>(members).AsReadOnly();
		}
	}
}