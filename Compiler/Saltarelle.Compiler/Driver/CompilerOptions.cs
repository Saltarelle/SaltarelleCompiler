using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.Driver {
	[Serializable]
	public class Reference {
		/// <summary>
		/// Alias for the reference (for use with "extern alias"). Null if no alias.
		/// </summary>
		public string Alias { get; private set; }

		/// <summary>
		/// Name of the file to reference.
		/// </summary>
		public string Filename { get; private set; }

		public Reference(string filename, string alias = null) {
			Filename = filename;
			Alias    = alias;
		}
	}

	[Serializable]
	public class CompilerOptions {
		public List<string> AdditionalLibPaths  { get; private set; }
		public bool MinimizeScript              { get; set; }
		public List<string> DefineConstants     { get; private set; }
		public List<int> DisabledWarnings       { get; private set; }
		public string DocumentationFile         { get; set; }
		public string OutputAssemblyPath        { get; set; }
		public string OutputScriptPath          { get; set; }
		public List<Reference> References       { get; private set; }
		public List<string> SourceFiles         { get; private set; }
		public bool TreatWarningsAsErrors       { get; set; }
		public int WarningLevel                 { get; set; }
		public List<int> WarningsAsErrors       { get; private set; }
		public List<int> WarningsNotAsErrors    { get; private set; }
		public string KeyContainer              { get; set; }
		public string KeyFile                   { get; set; }
		public bool AlreadyCompiled             { get; set; }
		public bool HasEntryPoint               { get; set; }
		public string EntryPointClass           { get; set; }

		public CompilerOptions() {
			AdditionalLibPaths  = new List<string>();
			DefineConstants     = new List<string>();
			DisabledWarnings    = new List<int>();
			References          = new List<Reference>();
			SourceFiles         = new List<string>();
			WarningsAsErrors    = new List<int>();
			WarningsNotAsErrors = new List<int>();

			WarningLevel = 4;
		}
	}
}
