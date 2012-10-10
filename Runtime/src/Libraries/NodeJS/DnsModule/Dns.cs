using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NodeJS.DnsModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("dns")]
	public static class Dns {
		public static void Lookup(string domain, Action<Error, string, int> callback) {}
		public static void Lookup(string domain, int family, Action<Error, string, int> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, function(a, f) {{ return {{ address: a, family: f }}; }}, 'lookup', {domain})")]
		public static Task<LookupResult> LookupTask(string domain) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, function(a, f) {{ return {{ address: a, family: f }}; }}, 'lookup', {domain}, {family})")]
		public static Task<LookupResult> LookupTask(string domain, int family) { return null; }

		public static void Resolve(string domain, Action<Error, string[]> callback) {}
		public static void Resolve(string domain, RecordType rrtype, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolve', {domain})")]
		public static Task<string[]> ResolveTask(string domain) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolve', {domain}, {rrtype})")]
		public static Task<string[]> ResolveTask(string domain, RecordType rrtype) { return null; }


		public static void Resolve4(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolve4', {domain})")]
		public static Task<string[]> Resolve4Task(string domain) { return null; }

		public static void Resolve6(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolve6', {domain})")]
		public static Task<string[]> Resolve6Task(string domain) { return null; }

		public static void ResolveMx(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolveMx', {domain})")]
		public static Task<string[]> ResolveMxTask(string domain) { return null; }

		public static void ResolveTxt(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolveTxt', {domain})")]
		public static Task<string[]> ResolveTxtTask(string domain) { return null; }

		public static void ResolveSrv(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolveSrv', {domain})")]
		public static Task<string[]> ResolveSrvTask(string domain) { return null; }

		public static void ResolveNs(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolveNs', {domain})")]
		public static Task<string[]> ResolveNsTask(string domain) { return null; }

		public static void ResolveCname(string domain, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'resolveCname', {domain})")]
		public static Task<string[]> ResolveCnameTask(string domain) { return null; }
	
		public static void Reverse(string ip, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.DnsModule.Dns}, 'reverse', {ip})")]
		public static Task<string[]> Reverse(string ip) { return null; }

		[IntrinsicProperty, ScriptName("NODATA")] public static int NoData { get; private set; }
		[IntrinsicProperty, ScriptName("FORMERR")] public static int FormErr { get; private set; }
		[IntrinsicProperty, ScriptName("SERVFAIL")] public static int ServFail { get; private set; }
		[IntrinsicProperty, ScriptName("NOTFOUND")] public static int NotFound { get; private set; }
		[IntrinsicProperty, ScriptName("NOTIMP")] public static int NotImp { get; private set; }
		[IntrinsicProperty, ScriptName("REFUSED")] public static int Refused { get; private set; }
		[IntrinsicProperty, ScriptName("BADQUERY")] public static int BadQuery { get; private set; }
		[IntrinsicProperty, ScriptName("BADNAME")] public static int BadName { get; private set; }
		[IntrinsicProperty, ScriptName("BADFAMILY")] public static int BadFamily { get; private set; }
		[IntrinsicProperty, ScriptName("BADRESP")] public static int BadResp { get; private set; }
		[IntrinsicProperty, ScriptName("CONNREFUSED")] public static int ConnRefused { get; private set; }
		[IntrinsicProperty, ScriptName("TIMEOUT")] public static int Timeout { get; private set; }
		[IntrinsicProperty, ScriptName("EOF")] public static int EOF { get; private set; }
		[IntrinsicProperty, ScriptName("FILE")] public static int File { get; private set; }
		[IntrinsicProperty, ScriptName("NOMEM")] public static int NoMem { get; private set; }
		[IntrinsicProperty, ScriptName("DESTRUCTION")] public static int Destruction { get; private set; }
		[IntrinsicProperty, ScriptName("BADSTR")] public static int BadStr { get; private set; }
		[IntrinsicProperty, ScriptName("BADFLAGS")] public static int BadFlags { get; private set; }
		[IntrinsicProperty, ScriptName("NONAME")] public static int NoName { get; private set; }
		[IntrinsicProperty, ScriptName("BADHINTS")] public static int BadHints { get; private set; }
		[IntrinsicProperty, ScriptName("NOTINITIALIZED")] public static int NotInitialized { get; private set; }
		[IntrinsicProperty, ScriptName("LOADIPHLPAPI")] public static int LoadIphlpapi { get; private set; }
		[IntrinsicProperty, ScriptName("ADDRGETNETWORKPARAMS")] public static int AddrGetNetworkParams { get; private set; }
		[IntrinsicProperty, ScriptName("CANCELLED")] public static int Cancelled { get; private set; }
	}
}
