using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptModel {
	public class TypeReference : TSType {
		public string Name { get; private set; }

		public TypeReference(string name) {
			Name = name;
		}
	}
}
