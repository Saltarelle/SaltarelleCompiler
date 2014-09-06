using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saltarelle.Compiler {
	public class AssemblyResource {
		public string Name { get; private set; }
		public bool IsPublic { get; private set; }
		public Func<Stream> GetResourceStream { get; set; }

		public AssemblyResource(string name, bool isPublic, Func<Stream> getResourceStream) {
			Name = name;
			IsPublic = isPublic;
			GetResourceStream = getResourceStream;
		}
	}
}
