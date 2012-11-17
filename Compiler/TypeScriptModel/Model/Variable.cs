namespace TypeScriptModel.Model {
	public class Variable : Member {
		public string Name { get; private set; }
		public TSType Type { get; private set; }
		public bool Optional { get; private set; }
		public bool ParamArray { get; private set; }

		public Variable(string name, TSType type, bool optional, bool paramArray) {
			Name = name;
			Type = type;
			Optional = optional;
			ParamArray = paramArray;
		}
	}
}