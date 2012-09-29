using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
	[Imported]
	public static class Activator
	{
        [InlineCode("new {type}({*arguments})")]
		public static object CreateInstance(Type type, params object[] arguments) {
            return null;
        }

        [InlineCode("new {T}({*arguments})")]
		public static T CreateInstance<T>(params object[] arguments) {
            return default(T);
        }

        [InlineCode("{type}.createInstance()")]
		public static object CreateInstance(Type type) {
            return null;
        }

        [InlineCode("{T}.createInstance()")]
		public static T CreateInstance<T>() {
            return default(T);
        }
	}
}
