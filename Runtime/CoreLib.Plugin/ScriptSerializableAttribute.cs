using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Plugin {
	public class ScriptSerializableAttribute : Attribute {
		/// <summary>
		/// Code used to check whether an object is of this type. Can use the placeholder {this} to reference the object being checked, as well as all type parameter for the type.
		/// </summary>
		public string TypeCheckCode { get; set; }
	}
}
