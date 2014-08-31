using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saltarelle.Compiler.Compiler {
	/// <summary>
	/// From https://github.com/ICSharpCode/NRefactory
	/// </summary>
	public static class ReflectionHelper {
		public static bool IsReflectionNameValid(string reflectionTypeName) {
			if (reflectionTypeName == null)
				throw new ArgumentNullException("reflectionTypeName");
			int pos = 0;
			if (!ParseReflectionName(reflectionTypeName, ref pos))
				return false;
			if (pos < reflectionTypeName.Length)
				return false;
			return true;
		}

		private static bool ParseReflectionName(string reflectionTypeName, ref int pos) {
			if (pos == reflectionTypeName.Length)
				return false;

			if (reflectionTypeName[pos] == '`') {
				// type parameter reference
				pos++;
				if (pos == reflectionTypeName.Length)
					return false;
				if (reflectionTypeName[pos] == '`') {
					// method type parameter reference
					pos++;
					if (ReadTypeParameterCount(reflectionTypeName, ref pos) < 0)
						return false;
				}
				else {
					// class type parameter reference
					if (ReadTypeParameterCount(reflectionTypeName, ref pos) < 0)
						return false;
				}
			}
			else {
				// not a type parameter reference: read the actual type name
				int tpc;
				if (ReadTypeName(reflectionTypeName, ref pos, out tpc) == null)
					return false;
			}
			// read type suffixes
			while (pos < reflectionTypeName.Length) {
				switch (reflectionTypeName[pos++]) {
					case '+':
						int tpc;
						if (ReadTypeName(reflectionTypeName, ref pos, out tpc) == null)
							return false;
						break;
					case '*':
						break;
					case '&':
						break;
					case '[':
						// this might be an array or a generic type
						if (pos == reflectionTypeName.Length)
							return false;
						if (reflectionTypeName[pos] == '[') {
							// it's a generic type
							pos++;
							if (!ParseReflectionName(reflectionTypeName, ref pos))
								return false;
							if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == ']')
								pos++;
							else
								return false;
							
							while (pos < reflectionTypeName.Length && reflectionTypeName[pos] == ',') {
								pos++;
								if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == '[')
									pos++;
								else
									return false;
								
								if (!ParseReflectionName(reflectionTypeName, ref pos))
									return false;
								
								if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == ']')
									pos++;
								else
									return false;
							}
							
							if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == ']') {
								pos++;
							}
							else {
								return false;
							}
						}
						else {
							// it's an array
							while (pos < reflectionTypeName.Length && reflectionTypeName[pos] == ',') {
								pos++;
							}
							if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == ']') {
								pos++; // end of array
							}
							else {
								return false;
							}
						}
						break;
					case ',':
						// assembly qualified name, ignore everything up to the end/next ']'
						while (pos < reflectionTypeName.Length && reflectionTypeName[pos] != ']')
							pos++;
						break;
					default:
						pos--; // reset pos to the character we couldn't read
						if (reflectionTypeName[pos] == ']')
							return true; // return from a nested generic
						else
							return false;
				}
			}
			return true;
		}

		private static int ReadTypeParameterCount(string reflectionTypeName, ref int pos) {
			int startPos = pos;
			while (pos < reflectionTypeName.Length) {
				char c = reflectionTypeName[pos];
				if (c < '0' || c > '9')
					break;
				pos++;
			}
			int tpc;
			if (!int.TryParse(reflectionTypeName.Substring(startPos, pos - startPos), out tpc))
				return -1;
			return tpc;
		}

		static bool IsReflectionNameSpecialCharacter(char c)
		{
			switch (c) {
				case '+':
				case '`':
				case '[':
				case ']':
				case ',':
				case '*':
				case '&':
					return true;
				default:
					return false;
			}
		}

		private static string ReadTypeName(string reflectionTypeName, ref int pos, out int tpc) {
			int startPos = pos;
			// skip the simple name portion:
			while (pos < reflectionTypeName.Length && !IsReflectionNameSpecialCharacter(reflectionTypeName[pos]))
				pos++;
			if (pos == startPos) {
				tpc = 0;
				return null;
			}
			string typeName = reflectionTypeName.Substring(startPos, pos - startPos);
			if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == '`') {
				pos++;
				tpc = ReadTypeParameterCount(reflectionTypeName, ref pos);
			}
			else {
				tpc = 0;
			}
			return typeName;
		}
	}
}
