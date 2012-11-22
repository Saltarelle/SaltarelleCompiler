using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Function : Member {
		public string Name { get; private set; }
		public TSType ReturnType { get; private set; }
		public IReadOnlyList<Parameter> Parameters { get; private set; }

		public Function(string name, TSType returnType, IEnumerable<Parameter> parameters) {
			Name = name;
			ReturnType = returnType;
			Parameters = new List<Parameter>(parameters).AsReadOnly();
		}
	}
}