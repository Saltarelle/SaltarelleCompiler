using System;
using System.Runtime.CompilerServices;

namespace NodeJS.TlsModule {
	[Imported]
	[Serializable]
	public class CertificateSubjectOrIssuer {
		[PreserveCase] public string C { get; set; }
		[PreserveCase] public string ST { get; set; }
		[PreserveCase] public string L { get; set; }
		[PreserveCase] public string O { get; set; }
		[PreserveCase] public string OU { get; set; }
		[PreserveCase] public string CN { get; set; }
	}

	[Imported]
	[Serializable]
	public class Certificate {
		public CertificateSubjectOrIssuer Subject { get; set; }

		public CertificateSubjectOrIssuer Issuer { get; set; }

		[ScriptName("valid_from")]
		public DateTime ValidFrom { get; set; }

		[ScriptName("valid_to")]
		public DateTime ValidTo { get; set; }

		public string Fingerprint { get; set; }
	}
}