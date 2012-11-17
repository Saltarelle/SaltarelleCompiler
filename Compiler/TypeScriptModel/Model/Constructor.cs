using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Constructor : Member {
		public TSType ReturnType { get; private set; }
		public IReadOnlyCollection<Variable> Parameters { get; private set; }

		public Constructor(TSType returnType, IEnumerable<Variable> parameters) {
			ReturnType = returnType;
			Parameters = new List<Variable>(parameters).AsReadOnly();
		}
	}
}