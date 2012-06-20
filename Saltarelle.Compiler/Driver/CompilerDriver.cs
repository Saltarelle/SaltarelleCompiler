using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;
using System.Text;
using Mono.CSharp;
using System.Linq;

namespace Saltarelle.Compiler.Driver {
	internal class CecilAssembly : Assembly {
		public override string ToString() {
			return base.ToString();
		}

		public override string CodeBase { get { throw new NotImplementedException(); } }
		public override string EscapedCodeBase { get { throw new NotImplementedException(); } }

		public override string FullName {
			get { return base.FullName; }
		}

		public override MethodInfo EntryPoint {
			get { return base.EntryPoint; }
		}

		public override string Location {
			get { return base.Location; }
		}

		public override string ImageRuntimeVersion {
			get { return base.ImageRuntimeVersion; }
		}

		public override bool GlobalAssemblyCache {
			get { return base.GlobalAssemblyCache; }
		}

		public override event ModuleResolveEventHandler ModuleResolve {
			add { base.ModuleResolve += value; }
			remove { base.ModuleResolve -= value; }
		}

		public override long HostContext {
			get { return base.HostContext; }
		}

		public override bool IsDynamic {
			get { return base.IsDynamic; }
		}

		public override Evidence Evidence {
			get { return base.Evidence; }
		}

		public override PermissionSet PermissionSet {
			get { return base.PermissionSet; }
		}

		public override SecurityRuleSet SecurityRuleSet {
			get { return base.SecurityRuleSet; }
		}

		public override Module ManifestModule {
			get { return base.ManifestModule; }
		}

		public override bool ReflectionOnly {
			get { return base.ReflectionOnly; }
		}

		public override bool Equals(object o) {
			return base.Equals(o);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override AssemblyName GetName() {
			return base.GetName();
		}

		public override AssemblyName GetName(bool copiedName) {
			return base.GetName(copiedName);
		}

		public override Type GetType(string name) {
			return base.GetType(name);
		}

		public override Type GetType(string name, bool throwOnError) {
			return base.GetType(name, throwOnError);
		}

		public override Type[] GetExportedTypes() {
			return base.GetExportedTypes();
		}

		public override Type[] GetTypes() {
			return base.GetTypes();
		}

		public override Stream GetManifestResourceStream(Type type, string name) {
			return base.GetManifestResourceStream(type, name);
		}

		public override Stream GetManifestResourceStream(string name) {
			return base.GetManifestResourceStream(name);
		}

		public override FileStream GetFile(string name) {
			return base.GetFile(name);
		}

		public override FileStream[] GetFiles() {
			return base.GetFiles();
		}

		public override FileStream[] GetFiles(bool getResourceModules) {
			return base.GetFiles(getResourceModules);
		}

		public override string[] GetManifestResourceNames() {
			return base.GetManifestResourceNames();
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName) {
			return base.GetManifestResourceInfo(resourceName);
		}

		public override AssemblyName[] GetReferencedAssemblies() {
			return base.GetReferencedAssemblies();
		}

		public override Assembly GetSatelliteAssembly(CultureInfo culture) {
			return base.GetSatelliteAssembly(culture);
		}

		public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version) {
			return base.GetSatelliteAssembly(culture, version);
		}

		public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore) {
			return base.LoadModule(moduleName, rawModule, rawSymbolStore);
		}

		public override object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes) {
			return base.CreateInstance(typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
		}

		public override Module[] GetLoadedModules(bool getResourceModules) {
			return base.GetLoadedModules(getResourceModules);
		}

		public override Module[] GetModules(bool getResourceModules) {
			return base.GetModules(getResourceModules);
		}

		public override Module GetModule(string name) {
			return base.GetModule(name);
		}

		public override bool IsDefined(Type attributeType, bool inherit) {
			return base.IsDefined(attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData() {
			return base.GetCustomAttributesData();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
		}

		public override object[] GetCustomAttributes(bool inherit) {
			return base.GetCustomAttributes(inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return base.GetCustomAttributes(attributeType, inherit);
		}

		public override Type GetType(string name, bool throwOnError, bool ignoreCase) {
			return base.GetType(name, throwOnError, ignoreCase);
		}
	}

	public class CecilReferencesLoader : AssemblyReferencesLoader<Assembly> {
		public CecilReferencesLoader(CompilerContext compiler) : base(compiler) {
		}

		public override bool HasObjectType(Assembly assembly) {
			throw new System.NotImplementedException();
		}

		protected override string[] GetDefaultReferences() {
			throw new System.NotImplementedException();
		}

		public override Assembly LoadAssemblyFile(string fileName, bool isImplicitReference) {
			throw new System.NotImplementedException();
		}

		public override void LoadReferences(ModuleContainer module) {
			throw new System.NotImplementedException();
		}
	}

	public class CompilerDriver {
		private CompilerSettings MapSettings(CompilerOptions options) {
			var result = new CompilerSettings();
			result.Target                    = Target.Library;
			result.Platform                  = Platform.AnyCPU;
			result.TargetExt                 = ".dll";
			result.VerifyClsCompliance       = false;
			result.Optimize                  = false;
			result.Version                   = LanguageVersion.V_5;
			result.EnhancedWarnings          = false;
			result.LoadDefaultReferences     = false;
			result.TabSize                   = 1;
			result.WarningsAreErrors         = options.TreatWarningsAsErrors;
			result.WarningLevel              = options.WarningLevel;
			result.AssemblyReferences        = options.References.Where(r => r.Alias == null).Select(r => r.File).ToList();
			result.AssemblyReferencesAliases = options.References.Where(r => r.Alias != null).Select(r => new Mono.CSharp.Tuple<string, string>(r.Alias, r.File)).ToList();
			result.ReferencesLookupPaths     = options.AdditionalLibPaths;
			result.Encoding                  = Encoding.UTF8;
			result.DocumentationFile         = options.DocumentationFile;
			result.OutputFile                = options.OutputAssemblyPath;
			result.StdLib                    = false;
			result.StdLibRuntimeVersion      = RuntimeVersion.v4;
			result.SourceFiles.AddRange(options.SourceFiles.Select((f, i) => new SourceFile(Path.GetFileName(f), f, i)));
			foreach (var c in options.DefineConstants)
				result.AddConditionalSymbol(c);
			foreach (var w in options.WarningsAsErrors)
				result.AddWarningAsError(w);
			foreach (var w in options.WarningsNotAsErrors)
				result.AddWarningOnly(w);
			foreach (var w in options.IgnoredWarnings)
				result.SetIgnoreWarning(w);

			return result;
		}

		public void Compile(CompilerOptions options) {
			var wr = new StringWriter();

			var rpt = new StreamReportPrinter(wr);
			var settings = MapSettings(options);
			var ctx = new CompilerContext(settings, rpt);
			var d = new Mono.CSharp.Driver(ctx);
			d.Compile();
		}
	}
}
