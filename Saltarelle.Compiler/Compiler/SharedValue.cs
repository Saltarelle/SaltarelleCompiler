namespace Saltarelle.Compiler.Compiler {
	public class SharedValue<T> where T : struct {
		public T Value { get; set; }

		public SharedValue(T value) {
			Value = value;
		}
	}
}
