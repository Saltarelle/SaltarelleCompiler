using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class FunctionType : TSType {
		public IReadOnlyList<Parameter> Parameters { get; private set; }
		public TSType ReturnType { get; private set; }

		public FunctionType(IEnumerable<Parameter> parameters, TSType returnType) {
			Parameters = new List<Parameter>(parameters).AsReadOnly();
			ReturnType = returnType;
		}
	}
}