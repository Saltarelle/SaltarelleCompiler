using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported, Serializable]
	public class Assembly {
		private Assembly() {
		}

		public string FullName { [InlineCode("{this}.toString()")] get { return null; } }

		[InlineCode("{typeName} + ', ' + {assemblyName}")]
		public static string CreateQualifiedName(string assemblyName, string typeName) {
			return null;
		}

		[InlineCode("{$System.Script}.getTypeAssembly({type})")]
		public static Assembly GetAssembly(Type type) {
			return null;
		}

		[InlineCode("{$System.Script}.load({assemblyString})")]
		public static Assembly Load(string assemblyString) {
			return null;
		}

		[InlineCode("{$System.Script}.getType({name}, {this})")]
		public Type GetType(string name) {
			return null;
		}

		[InlineCode("{$System.Script}.getAssemblyTypes({this})")]
		public Type[] GetTypes() {
			return null;
		}

		[InlineCode("{$System.Script}.createAssemblyInstance({this}, {typeName})")]
		public object CreateInstance(string typeName) {
			return null;
		}

		[InlineCode("__current_assembly_417c2c52e265424297fcbcb4fa402581__")]	// If this is ever changed, it needs to be synced with Linker.CurrentAssemblyIdentifier.
		public static Assembly GetExecutingAssembly() {
			return null;
		}

		[InlineCode("{this}.attr")]
		public object[] GetCustomAttributes() {
			return null;
		}

		[InlineCode("{this}.attr.filter(function(a) {{ return {$System.Script}.isInstanceOfType(a, {attributeType}); }})")]
		public object[] GetCustomAttributes(Type attributeType) {
			return null;
		}

		[InlineCode("{this}.attr")]
		public object[] GetCustomAttributes(bool inherit) {
			return null;
		}

		[InlineCode("{this}.attr.filter(function(a) {{ return {$System.Script}.isInstanceOfType(a, {attributeType}); }})")]
		public object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}

		[InlineCode("{this}.getResourceNames()")]
		public string[] GetManifestResourceNames() {
			return null;
		}

		[InlineCode("{this}.getResourceDataBase64({name})")]
		public string GetManifestResourceDataAsBase64(string name) {
			return null;
		}

		[InlineCode("{this}.getResourceDataBase64({$System.Script}.getTypeNamespace({type}) + '.' + {name})")]
		public string GetManifestResourceDataAsBase64(Type type, string name) {
			return null;
		}

		[InlineCode("{this}.getResourceData({name})")]
		public byte[] GetManifestResourceData(string name) {
			return null;
		}

		[InlineCode("{this}.getResourceData({$System.Script}.getTypeNamespace({type}) + '.' + {name})")]
		public byte[] GetManifestResourceData(Type type, string name) {
			return null;
		}
	}
}
