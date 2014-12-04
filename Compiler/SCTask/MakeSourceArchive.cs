using System.IO;
using Ionic.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Saltarelle.Compiler.SCTask {
	public class MakeSourceArchive : Task {
		public ITaskItem[] SourceFiles { get; set; }
		public string MapFile { get; set; }
		public string Destination { get; set; }
		public string SourceRoot { get; set; }

		[Output]
		public ITaskItem OutputFile { get; set; }

		private string MakePath(string path) {
			return !string.IsNullOrEmpty(SourceRoot) ? Path.Combine(SourceRoot, path) : path;
		}

		public override bool Execute() {
			if (File.Exists(Destination))
				File.Delete(Destination);

			using (var archive = new ZipFile(Destination)) {
				archive.AddEntry(MakePath("no-source-location"), "No source location");
				archive.AddEntry(Path.GetFileName(MapFile), _ => File.OpenRead(MapFile), (_, s) => s.Dispose());
				foreach (var source in SourceFiles) {
					archive.AddEntry(MakePath(source.GetMetadata("RelativePath")), _ => File.OpenRead(source.ItemSpec), (_, s) => s.Dispose());
				}
				archive.Save();
			}

			OutputFile = new TaskItem(Destination);

			return true;
		}
	}
}
