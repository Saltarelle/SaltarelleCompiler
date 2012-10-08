using System.Runtime.CompilerServices;

namespace NodeJS.DnsModule {
	[Imported]
	[NamedValues]
	public enum RecordType {
		[ScriptName("A")] IPv4,
		[ScriptName("AAAA")] IPv6,
		[ScriptName("MX")] MailExchange,
		[ScriptName("TXT")] Text,
		[ScriptName("SRV")] Srv,
		[ScriptName("PTR")] Reverse,
		[ScriptName("NS")] NameServer,
		[ScriptName("CNAME")] CName,
	}
}