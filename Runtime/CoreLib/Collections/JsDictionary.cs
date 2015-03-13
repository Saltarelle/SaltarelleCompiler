// Dictionary.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Collections {


	/// <summary>
	/// The JsDictionary data type which is mapped to the Object type in Javascript.
	/// </summary>
	[Imported]
	[IgnoreNamespace]
	[ScriptName("Object")]
	public sealed class JsDictionary: IEnumerable {
		[InlineCode("{{}}")]
		public JsDictionary() {
		}

		public JsDictionary(params object[] nameValuePairs) {
		}

		public int Count {
			[InlineCode("{$System.Script}.getKeyCount({this})")]
			get {
				return 0;
			}
		}

		public new ICollection<string> Keys {
			[InlineCode("{$System.Object}.keys({this})")]
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public object this[string key] {
			get {
				return null;
			}
			set {
			}
		}

		[InlineCode("{$System.Script}.clearKeys({this})")]
		public void Clear() {
		}

		[InlineCode("{$System.Script}.keyExists({this}, {key})")]
		public bool ContainsKey(string key) {
			return false;
		}

		[ScriptSkip]
		public static JsDictionary GetDictionary(object o) {
			return null;
		}

		[InlineCode("new {$System.ObjectEnumerator`2}({this})")]
		public ObjectEnumerator<string, object> GetEnumerator() {
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}

		[InlineCode("delete {this}[{key}]")]
		public void Remove(string key) {
		}

		[InlineCode("{this}[{key}] = {value}")]
		public void Add(string key, object value)
		{

		}

		[ScriptSkip]
		public static implicit operator JsDictionary<string, object>(JsDictionary value) {
			return null;
		}

		[ScriptSkip]
		public static implicit operator JsDictionary(JsDictionary<string, object> value) {
			return null;
		}
	}
}
