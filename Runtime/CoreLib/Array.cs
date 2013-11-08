// Array.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {

	/// <summary>
	/// Equivalent to the Array type in Javascript.
	/// </summary>
	[IgnoreNamespace]
	[Imported(ObeysTypeSystem = true)]
	public sealed class Array : IEnumerable {

		[IntrinsicProperty]
		public int Length {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public object this[int index] {
			get {
				return null;
			}
			set {
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}

		[InlineCode("new {$System.ArrayEnumerator}({this})")]
		public ArrayEnumerator GetEnumerator() {
			return null;
		}

		public string Join() {
			return null;
		}

		public string Join(string delimiter) {
			return null;
		}

		public void Reverse() {
		}

		[InlineCode("{$System.Script}.arrayGet2({this}, {indices})")]
		public object GetValue(params int[] indices) {
			return null;
		}

		[InlineCode("{$System.Script}.arraySet2({this}, {value}, {indices})")]
		public void SetValue(object value, params int[] indices) {
		}

		[InlineCode("{$System.Script}.arrayLength({this}, {dimension})")]
		public int GetLength(int dimension) {
			return 0;
		}

		public int Rank { [InlineCode("{$System.Script}.arrayRank({this})")] get { return 0; } }

		[InlineCode("0")]
		public int GetLowerBound(int dimension) {
			return 0;
		}

		[InlineCode("{$System.Script}.arrayLength({this}, {dimension}) - 1")]
		public int GetUpperBound(int dimension) {
			return 0;
		}

		[InlineCode("{$System.Script}.repeat({value}, {count})")]
		public static T[] Repeat<T>(T value, int count) {
			return null;
		}
	}
}
