﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

		private static CompilerOptions GetOptions(dynamic taskOptions) {
			var result = new CompilerOptions();

			result.KeyContainer          =  taskOptions.KeyContainer;
			result.KeyFile               =  taskOptions.KeyFile;
			result.MinimizeScript        = !taskOptions.EmitDebugInformation;
			result.DocumentationFile     =  taskOptions.DocumentationFile;
			result.OutputAssemblyPath    =  taskOptions.OutputAssembly;
			result.OutputScriptPath      =  taskOptions.OutputScript;
			result.TreatWarningsAsErrors =  taskOptions.TreatWarningsAsErrors;
			result.WarningLevel          =  taskOptions.WarningLevel;
			result.AlreadyCompiled       =  taskOptions.AlreadyCompiled;

			result.EntryPointClass = taskOptions.MainEntryPoint;
			if (!string.IsNullOrEmpty(taskOptions.TargetType)) {
				switch ((string)taskOptions.TargetType.ToLowerInvariant()) {
					case "exe":
					case "winexe":
						result.HasEntryPoint = true;
						break;
					case "library":
					case "module":
						result.HasEntryPoint = false;
						break;
					default:
						taskOptions.Log.LogError("Invalid target type (must be exe, winexe, library or module).");
						return null;
				}
			}
			else {
				result.HasEntryPoint = false;
			}

			if (taskOptions.WarningLevel < 0 || taskOptions.WarningLevel > 4) {
				taskOptions.Log.LogError("Warning level must be between 0 and 4.");
				return null;
			}

			if (taskOptions.AdditionalLibPaths != null)
				result.AdditionalLibPaths.AddRange(taskOptions.AdditionalLibPaths);

			if (taskOptions.DefineConstants != null)
				result.DefineConstants.AddRange(((string)taskOptions.DefineConstants).Split(';').Select(s => s.Trim()).Where(s => s != ""));

			if (!HandleIntegerList(taskOptions, result.DisabledWarnings, taskOptions.DisabledWarnings, "DisabledWarnings"))
				return null;
			if (!HandleIntegerList(taskOptions, result.WarningsAsErrors, taskOptions.WarningsAsErrors, "WarningsAsErrors"))
				return null;
			if (!HandleIntegerList(taskOptions, result.WarningsNotAsErrors, taskOptions.WarningsNotAsErrors, "WarningsNotAsErrors"))
				return null;

			if (taskOptions.References != null) {
				foreach (ITaskItem r in taskOptions.References) {
					string alias = r.GetMetadata("Aliases");
					result.References.Add(new Reference(r.ItemSpec, !string.IsNullOrWhiteSpace(alias) ? alias : null));
				}
			}

			if (taskOptions.Sources != null) {
				foreach (ITaskItem s in taskOptions.Sources) {
					result.SourceFiles.Add(s.ItemSpec);
				}
			}

			if (taskOptions.Resources != null) {
				foreach (ITaskItem r in taskOptions.Resources) {
					string name = r.GetMetadata("LogicalName");
					string access = r.GetMetadata("Access");
					result.EmbeddedResources.Add(new EmbeddedResource(r.ItemSpec, !string.IsNullOrWhiteSpace(name) ? name : Path.GetFileName(r.ItemSpec), !string.Equals(access, "private", StringComparison.OrdinalIgnoreCase)));
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
