using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Saltarelle.Compiler.SCTask {
	public class AppendBlankLineToFile : Task {
		public ITaskItem File { get; set; }

		public override bool Execute() {
			System.IO.File.AppendAllText(File.ItemSpec, Environment.NewLine);
			return true;
		}
	}
}
