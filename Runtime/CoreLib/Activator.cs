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
		[InlineCode("new {type}({*arguments})", NonExpandedFormCode = "{$System.Script}.applyConstructor({type}, {arguments})")]
		public static object CreateInstance(Type type, params object[] arguments) {
			return null;
		}

		[InlineCode("new {T}({*arguments})", NonExpandedFormCode = "{$System.Script}.applyConstructor({T}, {arguments})")]
		public static T CreateInstance<T>(params object[] arguments) {
			return default(T);
		}

		[InlineCode("{$System.Script}.createInstance({type})")]
		public static object CreateInstance(Type type) {
			return null;
		}

		[InlineCode("{$System.Script}.createInstance({T})")]
		public static T CreateInstance<T>() {
			return default(T);
		}
	}
}
