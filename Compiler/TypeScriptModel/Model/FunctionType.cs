using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class FunctionType : TSType {
		public IReadOnlyCollection<Variable> Parameters { get; private set; }
		public TSType ReturnType { get; private set; }

		public FunctionType(IEnumerable<Variable> parameters, TSType returnType) {
			Parameters = new List<Variable>(parameters).AsReadOnly();
			ReturnType = returnType;
		}
	}
}