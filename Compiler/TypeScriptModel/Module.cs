using System.Collections.Generic;

namespace TypeScriptModel {
	public class Module {
		public string Name { get; private set; }
		public IReadOnlyCollection<ModuleImport> Imports { get; private set; }
		public IReadOnlyCollection<Member> Members { get; private set; }

		public Module(string name, IEnumerable<ModuleImport> imports, IEnumerable<Member> members) {
			Name = name;
			Imports = new List<ModuleImport>(imports).AsReadOnly();
			Members = new List<Member>(members).AsReadOnly();
		}
	}
}