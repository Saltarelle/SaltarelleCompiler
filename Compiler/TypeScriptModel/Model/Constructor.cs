using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Constructor : Member {
		public TSType ReturnType { get; private set; }
		public IReadOnlyList<Parameter> Parameters { get; private set; }

		public Constructor(TSType returnType, IEnumerable<Parameter> parameters) {
			ReturnType = returnType;
			Parameters = new List<Parameter>(parameters).AsReadOnly();
		}
	}
}