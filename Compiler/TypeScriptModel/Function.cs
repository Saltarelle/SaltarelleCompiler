using System.Collections.Generic;

namespace TypeScriptModel {
	public class Function : Member {
		public string Name { get; private set; }
		public TSType ReturnType { get; private set; }
		public IReadOnlyCollection<Variable> Parameters { get; private set; }

		public Function(string name, TSType returnType, IEnumerable<Variable> parameters) {
			Name = name;
			ReturnType = returnType;
			Parameters = new List<Variable>(parameters).AsReadOnly();
		}
	}
}