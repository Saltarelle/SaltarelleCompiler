using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Module {
		public string Name { get; private set; }
		public IReadOnlyList<ModuleImport> Imports { get; private set; }
		public IReadOnlyList<Member> ExportedMembers { get; private set; }
		public IReadOnlyList<Member> Members { get; private set; }
		public IReadOnlyList<Interface> ExportedInterfaces { get; private set; }
		public IReadOnlyList<Interface> Interfaces { get; private set; }

		public Module(string name, IEnumerable<ModuleImport> imports, IEnumerable<Member> exportedMembers, IEnumerable<Member> members, IEnumerable<Interface> exportedInterfaces, IEnumerable<Interface> interfaces) {
			Name               = name;
			Imports            = new List<ModuleImport>(imports).AsReadOnly();
			ExportedMembers    = new List<Member>(exportedMembers).AsReadOnly();
			Members            = new List<Member>(members).AsReadOnly();
			ExportedInterfaces = new List<Interface>(exportedInterfaces).AsReadOnly();
			Interfaces         = new List<Interface>(interfaces).AsReadOnly();
		}
	}
}