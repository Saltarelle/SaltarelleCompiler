namespace TypeScriptModel {
	public class Indexer : Member {
		public TSType ReturnType { get; private set; }
		public Variable Parameter { get; private set; }

		public Indexer(TSType returnType, Variable parameter) {
			ReturnType = returnType;
			Parameter  = parameter;
		}
	}
}