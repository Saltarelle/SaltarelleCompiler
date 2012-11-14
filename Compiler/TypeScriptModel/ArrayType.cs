namespace TypeScriptModel {
	public class ArrayType : TSType {
		public TSType ElementType { get; private set; }

		public ArrayType(TSType elementType) {
			ElementType = elementType;
		}
	}
}