namespace TypeScriptModel.Model {
	public class Indexer : Member {
		public TSType ReturnType { get; private set; }
		public string ParameterName { get; private set; }
		public TSType ParameterType { get; private set; }

		public Indexer(TSType returnType, string parameterName, TSType parameterType) {
			ReturnType    = returnType;
			ParameterName = parameterName;
			ParameterType = parameterType;
		}
	}
}