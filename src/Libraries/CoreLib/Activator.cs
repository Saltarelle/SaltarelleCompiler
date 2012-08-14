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
		public static T CreateInstance<T>(params object[] arguments) where T : class {
            return null;
        }
	}
}
