// StringBuilder.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Text {

	/// <summary>
	/// Provides an optimized mechanism to concatenate strings.
	/// </summary>
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public sealed class StringBuilder {

		/// <summary>
		/// Initializes a new instance of the <see cref="StringBuilder"/> class.
		/// </summary>
		[ScriptName("")]
		public StringBuilder() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringBuilder"/> class.
		/// </summary>
		/// <param name="initialText">
		/// The string that is used to initialize the value of the instance.
		/// </param>
		[ScriptName("")]
		public StringBuilder(string initialText) {
		}

		/// <summary>
		/// Gets whether the <see cref="StringBuilder"/> object has any content.
		/// </summary>
		/// <returns>true if the StringBuilder instance contains no text; otherwise, false.</returns>
		[IntrinsicProperty]
		public bool IsEmpty {
			get {
				return false;
			}
		}

		/// <summary>
		/// Gets the length of the StringBuilder content.
		/// </summary>
		/// <returns>length of the StringBuilder content.</returns>
		[IntrinsicProperty]
		public int Length {
			get {
				return 0;
			}
		}

		/// <summary>
		/// Appends a boolean value to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="b">The boolean value to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder Append(bool b) {
			return null;
		}

		/// <summary>
		/// Appends a character to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="c">The character to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		[ScriptName("appendChar")]
		public StringBuilder Append(char c) {
			return null;
		}

		/// <summary>
		/// Appends a number to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="i">The number to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder Append(int i) {
			return null;
		}

		/// <summary>
		/// Appends a number to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="d">The number to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder Append(double d) {
			return null;
		}

		/// <summary>
		/// Appends an object's string representation to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="o">The object to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder Append(object o) {
			return null;
		}

		/// <summary>
		/// Appends the specified string to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="s">The string to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder Append(string s) {
			return null;
		}

		/// <summary>
		/// Appends a string with a line terminator to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder AppendLine() {
			return null;
		}

		/// <summary>
		/// Appends a boolean with a line terminator to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="b">The boolean value to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder AppendLine(bool b) {
			return null;
		}

		/// <summary>
		/// Appends a character with a line terminator to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="c">The character to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		[ScriptName("appendLineChar")]
		public StringBuilder AppendLine(char c) {
			return null;
		}

		/// <summary>
		/// Appends a number with a line terminator to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="i">The number to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder AppendLine(int i) {
			return null;
		}

		/// <summary>
		/// Appends a number with a line terminator to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="d">The number to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder AppendLine(double d) {
			return null;
		}

		/// <summary>
		/// Appends an object's string representation with a line terminator to the end of the
		/// <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="o">The object to append to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder AppendLine(object o) {
			return null;
		}

		/// <summary>
		/// Appends a string with a line terminator to the end of the <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="s">The string to append with a line terminator to the end of the StringBuilder instance.</param>
		/// <returns>A reference to this instance after the append operation has completed.</returns>
		public StringBuilder AppendLine(string s) {
			return null;
		}

		/// <summary>
		/// Clears the contents of the <see cref="StringBuilder"/> instance.
		/// </summary>
		public void Clear() {
		}

		/// <summary>
		/// Creates a string from the contents of a <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <returns>A string representation of the StringBuilder instance.</returns>
		public override string ToString() {
			return null;
		}
	}
}
