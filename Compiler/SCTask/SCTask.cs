using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler.SCTask {
	public class SCTask : Task {
		public class Executor : MarshalByRefObject {
			public bool Execute(CompilerOptions options, TaskLoggingHelper log) {
				if (options == null)
					return false;
				var driver = new CompilerDriver(new TaskErrorReporter(log));
				try {
					return driver.Compile(options);
				}
				catch (Exception ex) {
					log.LogErrorFromException(ex);
					return false;
				}
			}
		}

		private bool HandleIntegerList(IList<int> targetCollection, string value, string itemName) {
			if (!string.IsNullOrEmpty(value)) {
				foreach (var s in value.Split(new[] { ';', ',' }).Select(s => s.Trim()).Where(s => s != "")) {
					int w;
					if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out w)) {
						Log.LogError("Invalid number " + s + " in " + itemName);
						return false;
					}
					if (!targetCollection.Contains(w))
						targetCollection.Add(w);
				}
			}
			return true;
		}

		private CompilerOptions GetOptions() {
			var result = new CompilerOptions();

			result.KeyContainer          =  this.KeyContainer;
			result.KeyFile               =  this.KeyFile;
			result.MinimizeScript        = !this.EmitDebugInformation;
			result.DocumentationFile     =  this.DocumentationFile;
			result.OutputAssemblyPath    =  this.OutputAssembly;
			result.OutputScriptPath      =  this.OutputScript;
			result.TreatWarningsAsErrors =  this.TreatWarningsAsErrors;
			result.WarningLevel          =  this.WarningLevel;

			if (this.WarningLevel < 0 || this.WarningLevel > 4) {
				Log.LogError("Warning level must be between 0 and 4.");
				return null;
			}

			if (this.AdditionalLibPaths != null)
				result.AdditionalLibPaths.AddRange(this.AdditionalLibPaths);

			if (this.DefineConstants != null)
				result.DefineConstants.AddRange(this.DefineConstants.Split(';').Select(s => s.Trim()).Where(s => s != ""));

			if (!HandleIntegerList(result.DisabledWarnings, this.DisabledWarnings, "DisabledWarnings"))
				return null;
			if (!HandleIntegerList(result.WarningsAsErrors, this.WarningsAsErrors, "WarningsAsErrors"))
				return null;
			if (!HandleIntegerList(result.WarningsNotAsErrors, this.WarningsNotAsErrors, "WarningsNotAsErrors"))
				return null;

			if (this.References != null) {
				foreach (var r in this.References) {
					string alias = r.GetMetadata("Aliases");
					result.References.Add(new Reference(r.ItemSpec, !string.IsNullOrWhiteSpace(alias) ? alias : null));
				}
			}

			if (this.Sources != null) {
				foreach (var s in this.Sources) {
					result.SourceFiles.Add(s.ItemSpec);
				}
			}

			return result;
		}

		public override bool Execute() {
			var options = GetOptions();
			if (options == null)
				return false;

			AppDomain ad = null;
			try {
				var setup = new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(typeof(Executor).Assembly.Location) };
				ad = AppDomain.CreateDomain("SCTask", null, setup);
				var executor = (Executor)ad.CreateInstanceAndUnwrap(typeof(Executor).Assembly.FullName, typeof(Executor).FullName);
				return executor.Execute(options, Log);
			}
			finally {
				if (ad != null) {
					AppDomain.Unload(ad);
				}
			}
		}

		public string KeyContainer { get; set; }

		public string KeyFile { get; set; }

		public string[] AdditionalLibPaths { get; set; }

		public string DefineConstants { get; set; }

		public string DisabledWarnings { get; set; }

		public string DocumentationFile { get; set; }

		public bool EmitDebugInformation { get; set; }

		public string OutputAssembly { get; set; }

		public string OutputScript { get; set; }

		public ITaskItem[] References { get; set; }

		public ITaskItem[] Sources { get; set; }

		public bool TreatWarningsAsErrors { get; set; }

		public int WarningLevel { get; set; }

		public string WarningsAsErrors { get; set; }

		public string WarningsNotAsErrors { get; set; }
	}
}
