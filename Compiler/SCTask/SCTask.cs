using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Saltarelle.Compiler.SCTask {
	public class SCTask : Task {
		[Serializable]
		public class Options {
			public string KeyContainer { get; set; }
			public string KeyFile { get; set; }
			public string[] AdditionalLibPaths { get; set; }
			public string DefineConstants { get; set; }
			public string DisabledWarnings { get; set; }
			public string DocumentationFile { get; set; }
			public bool EmitDebugInformation { get; set; }
			public string OutputAssembly { get; set; }
			public string OutputScript { get; set; }
			public string OutputSourceMap { get; set; }
			public ITaskItem[] References { get; set; }
			public ITaskItem[] Sources { get; set; }
			public bool TreatWarningsAsErrors { get; set; }
			public int WarningLevel { get; set; }
			public string WarningsAsErrors { get; set; }
			public string WarningsNotAsErrors { get; set; }
			public string TargetType { get; set; }
			public string MainEntryPoint { get; set; }
			public bool AlreadyCompiled { get; set; }
			public string SourceMapSourceRoot { get; set; }
			public ITaskItem[] Resources { get; set; }
			public string[] Plugins { get; set; }
		}

		private class Executor : MarshalByRefObject {
			public bool Execute(Options options, TaskLoggingHelper log) {
				// The module initializer seems to not work in all cases on mono.
				var t = typeof(SCTask).Assembly.GetType("EmbedAssemblies.EmbeddedAssemblyLoader");
				var m = t.GetMethod("Register", BindingFlags.Static | BindingFlags.Public);
				m.Invoke(null, new object[0]);

				var asm = Assembly.Load("SCTaskWorker");
				var worker = asm.GetType("Saltarelle.Compiler.SCTask.Worker");
				return (bool)worker.GetMethod("DoWork").Invoke(null, new object[] { options, log });
			}
		}

		private readonly Options _options = new Options();

		public override bool Execute() {
			AppDomain ad = null;
			try {
				var setup = new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(typeof(SCTask).Assembly.Location) };
				ad = AppDomain.CreateDomain("SCTask", null, setup);
				var executor = (Executor)ad.CreateInstanceAndUnwrap(typeof(Executor).Assembly.FullName, typeof(Executor).FullName);
				return executor.Execute(_options, Log);
			}
			finally {
				if (ad != null)
					AppDomain.Unload(ad);
			}
		}

		public string KeyContainer {
			get { return _options.KeyContainer; }
			set { _options.KeyContainer = value; }
		}

		public string KeyFile {
			get { return _options.KeyFile; }
			set { _options.KeyFile = value; }
		}

		public string[] AdditionalLibPaths {
			get { return _options.AdditionalLibPaths; }
			set { _options.AdditionalLibPaths = value; }
		}

		public string DefineConstants {
			get { return _options.DefineConstants; }
			set { _options.DefineConstants = value; }
		}

		public string DisabledWarnings {
			get { return _options.DisabledWarnings; }
			set { _options.DisabledWarnings = value; }
		}

		public string DocumentationFile {
			get { return _options.DocumentationFile; }
			set { _options.DocumentationFile = value; }
		}

		public bool EmitDebugInformation {
			get { return _options.EmitDebugInformation; }
			set { _options.EmitDebugInformation = value; }
		}

		public string OutputAssembly {
			get { return _options.OutputAssembly; }
			set { _options.OutputAssembly = value; }
		}

		public string OutputScript {
			get { return _options.OutputScript; }
			set { _options.OutputScript = value; }
		}

		public string OutputSourceMap {
			get { return _options.OutputSourceMap; }
			set { _options.OutputSourceMap = value; }
		}

		public ITaskItem[] References {
			get { return _options.References; }
			set { _options.References = value; }
		}

		public ITaskItem[] Sources {
			get { return _options.Sources; }
			set { _options.Sources = value; }
		}

		public bool TreatWarningsAsErrors {
			get { return _options.TreatWarningsAsErrors; }
			set { _options.TreatWarningsAsErrors = value; }
		}

		public int WarningLevel {
			get { return _options.WarningLevel; }
			set { _options.WarningLevel = value; }
		}

		public string WarningsAsErrors {
			get { return _options.WarningsAsErrors; }
			set { _options.WarningsAsErrors = value; }
		}

		public string WarningsNotAsErrors {
			get { return _options.WarningsNotAsErrors; }
			set { _options.WarningsNotAsErrors = value; }
		}

		public string TargetType {
			get { return _options.TargetType; }
			set { _options.TargetType = value; }
		}

		public string MainEntryPoint {
			get { return _options.MainEntryPoint; }
			set { _options.MainEntryPoint = value; }
		}

		public bool AlreadyCompiled {
			get { return _options.AlreadyCompiled; }
			set { _options.AlreadyCompiled = value; }
		}

		public string SourceMapSourceRoot {
			get { return _options.SourceMapSourceRoot; }
			set { _options.SourceMapSourceRoot = value; }
		}

		public ITaskItem[] Resources {
			get { return _options.Resources; }
			set { _options.Resources = value; }
		}

		public string[] Plugins {
			get { return _options.Plugins; }
			set { _options.Plugins = value; }
		}
	}
}
