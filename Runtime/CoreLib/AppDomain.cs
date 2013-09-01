using System.Reflection;
using System.Runtime.CompilerServices;

namespace System {
	[Imported, Serializable]
	public sealed class AppDomain {
		private AppDomain() {
		}

		[InlineCode("{this}.getAssemblies()")]
		public Assembly[] GetAssemblies() {
			return null;
		}

		public static AppDomain CurrentDomain {
			[InlineCode("{$System.Script}")] get { return null; }
		}
	}
}
