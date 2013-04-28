using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler.SCTask {
	public static class Worker {
		private static bool HandleIntegerList(dynamic scTask, IList<int> targetCollection, string value, string itemName) {
			if (!string.IsNullOrEmpty(value)) {
				foreach (var s in value.Split(new[] { ';', ',' }).Select(s => s.Trim()).Where(s => s != "")) {
					int w;
					if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out w)) {
						scTask.Log.LogError("Invalid number " + s + " in " + itemName);
						return false;
					}
					if (!targetCollection.Contains(w))
						targetCollection.Add(w);
				}
			}
			return true;
		}

		private static CompilerOptions GetOptions(dynamic scTask) {
			var result = new CompilerOptions();

			result.KeyContainer          =  scTask.KeyContainer;
			result.KeyFile               =  scTask.KeyFile;
			result.MinimizeScript        = !scTask.EmitDebugInformation;
			result.DocumentationFile     =  scTask.DocumentationFile;
			result.OutputAssemblyPath    =  scTask.OutputAssembly;
			result.OutputScriptPath      =  scTask.OutputScript;
			result.TreatWarningsAsErrors =  scTask.TreatWarningsAsErrors;
			result.WarningLevel          =  scTask.WarningLevel;
			result.AlreadyCompiled       =  scTask.AlreadyCompiled;

			result.EntryPointClass = scTask.MainEntryPoint;
			if (!string.IsNullOrEmpty(scTask.TargetType)) {
				switch ((string)scTask.TargetType.ToLowerInvariant()) {
					case "exe":
					case "winexe":
						result.HasEntryPoint = true;
						break;
					case "library":
					case "module":
						result.HasEntryPoint = false;
						break;
					default:
						scTask.Log.LogError("Invalid target type (must be exe, winexe, library or module).");
						return null;
				}
			}
			else {
				result.HasEntryPoint = false;
			}

			if (scTask.WarningLevel < 0 || scTask.WarningLevel > 4) {
				scTask.Log.LogError("Warning level must be between 0 and 4.");
				return null;
			}

			if (scTask.AdditionalLibPaths != null)
				result.AdditionalLibPaths.AddRange(scTask.AdditionalLibPaths);

			if (scTask.DefineConstants != null)
				result.DefineConstants.AddRange(((string)scTask.DefineConstants).Split(';').Select(s => s.Trim()).Where(s => s != ""));

			if (!HandleIntegerList(scTask, result.DisabledWarnings, scTask.DisabledWarnings, "DisabledWarnings"))
				return null;
			if (!HandleIntegerList(scTask, result.WarningsAsErrors, scTask.WarningsAsErrors, "WarningsAsErrors"))
				return null;
			if (!HandleIntegerList(scTask, result.WarningsNotAsErrors, scTask.WarningsNotAsErrors, "WarningsNotAsErrors"))
				return null;

			if (scTask.References != null) {
				foreach (ITaskItem r in scTask.References) {
					string alias = r.GetMetadata("Aliases");
					result.References.Add(new Reference(r.ItemSpec, !string.IsNullOrWhiteSpace(alias) ? alias : null));
				}
			}

			if (scTask.Sources != null) {
				foreach (ITaskItem s in scTask.Sources) {
					result.SourceFiles.Add(s.ItemSpec);
				}
			}

			return result;
		}

		public static bool DoWork(dynamic taskOptions, TaskLoggingHelper log) {
			var options = GetOptions(taskOptions);
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
}
