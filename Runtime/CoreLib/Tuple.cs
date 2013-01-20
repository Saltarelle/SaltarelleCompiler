// Tuple.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1> {
		[InlineCode("{{ item1: {item1} }}")]
		public Tuple(T1 item1) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2> {
		[InlineCode("{{ item1: {item1}, item2: {item2} }}")]
		public Tuple(T1 item1, T2 item2) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2, T3> {
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3} }}")]
		public Tuple(T1 item1, T2 item2, T3 item3) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
		[IntrinsicProperty] public T3 Item3 { get { return default(T3); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2, T3, T4> {
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4} }}")]
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
		[IntrinsicProperty] public T3 Item3 { get { return default(T3); } }
		[IntrinsicProperty] public T4 Item4 { get { return default(T4); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2, T3, T4, T5> {
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5} }}")]
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
		[IntrinsicProperty] public T3 Item3 { get { return default(T3); } }
		[IntrinsicProperty] public T4 Item4 { get { return default(T4); } }
		[IntrinsicProperty] public T5 Item5 { get { return default(T5); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2, T3, T4, T5, T6> {
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5}, item6: {item6} }}")]
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
		[IntrinsicProperty] public T3 Item3 { get { return default(T3); } }
		[IntrinsicProperty] public T4 Item4 { get { return default(T4); } }
		[IntrinsicProperty] public T5 Item5 { get { return default(T5); } }
		[IntrinsicProperty] public T6 Item6 { get { return default(T6); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7> {
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5}, item6: {item6}, item7: {item7} }}")]
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
		[IntrinsicProperty] public T3 Item3 { get { return default(T3); } }
		[IntrinsicProperty] public T4 Item4 { get { return default(T4); } }
		[IntrinsicProperty] public T5 Item5 { get { return default(T5); } }
		[IntrinsicProperty] public T6 Item6 { get { return default(T6); } }
		[IntrinsicProperty] public T7 Item7 { get { return default(T7); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> {
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5}, item6: {item6}, item7: {item7}, rest: {rest} }}")]
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest) {}

		[IntrinsicProperty] public T1 Item1 { get { return default(T1); } }
		[IntrinsicProperty] public T2 Item2 { get { return default(T2); } }
		[IntrinsicProperty] public T3 Item3 { get { return default(T3); } }
		[IntrinsicProperty] public T4 Item4 { get { return default(T4); } }
		[IntrinsicProperty] public T5 Item5 { get { return default(T5); } }
		[IntrinsicProperty] public T6 Item6 { get { return default(T6); } }
		[IntrinsicProperty] public T7 Item7 { get { return default(T7); } }
		[IntrinsicProperty] public TRest Rest { get { return default(TRest); } }
	}

	[Imported, IgnoreNamespace, ScriptName("Object")]
	public static class Tuple {
		[InlineCode("{{ item1: {item1} }}")]
		public static Tuple<T1> Create<T1>(T1 item1) { return null; }

		[InlineCode("{{ item1: {item1}, item2: {item2} }}")]
		public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) { return null; }

		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3} }}")]
		public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) { return null; }

		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4} }}")]
		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) { return null; }

		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5} }}")]
		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) { return null; }

		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5}, item6: {item6} }}")]
		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) { return null; }

		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5}, item6: {item6}, item7: {item7} }}")]
		public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) { return null; }
		
		[InlineCode("{{ item1: {item1}, item2: {item2}, item3: {item3}, item4: {item4}, item5: {item5}, item6: {item6}, item7: {item7}, rest: {rest} }}")]
		public static Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> Create<T1, T2, T3, T4, T5, T6, T7, TRest>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest) { return null; }
	}
}
