namespace TypeScriptModel {
	public class ModuleImport {
		public string Module { get; private set; }
		public string Alias { get; private set; }

		public ModuleImport(string module, string alias) {
			Module = module;
			Alias  = alias;
		}
	}
}