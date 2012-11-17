namespace TypeScriptModel.Model {
	public class TypeReference : TSType {
		public string Name { get; private set; }

		public TypeReference(string name) {
			Name = name;
		}
	}
}
