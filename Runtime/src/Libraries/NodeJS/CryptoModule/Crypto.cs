using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;

namespace NodeJS.CryptoModule{
	[Imported]
	[GlobalMethods]
	[ModuleName("crypto")]
	public static class Crypto {
		public static Credentials CreateCredentials() { return null; }
		public static Credentials CreateCredentials(CredentialsDetails details) { return null; }

		public static Hash CreateHash(string algorithm) { return null; }

		public static Hmac CreateHmac(string algorithm, string key) { return null; }

		public static Cipher CreateCipher(string algorithm, TypeOption<string, Buffer> password) { return null; }

		[ScriptName("createCipheriv")]
		public static Cipher CreateCipherIV(string algorithm, TypeOption<string, Buffer> password, TypeOption<string, Buffer> iv) { return null; }

		public static Decipher CreateDecipher(string algorithm, TypeOption<string, Buffer> password) { return null; }

		[ScriptName("createDecipheriv")]
		public static Decipher CreateDecipherIV(string algorithm, TypeOption<string, Buffer> password, TypeOption<string, Buffer> iv) { return null; }

		public static Signer CreateSign(string algorithm) { return null; }

		public static Verifier CreateVerify(string algorithm) { return null; }

		public static DiffieHellman CreateDiffieHellman(int primeLength) { return null; }

		public static DiffieHellman CreateDiffieHellman(object key, Encoding encoding) { return null; }

		public static DiffieHellman GetDiffieHellman(string groupName) { return null; }

		public static void Pbkdf2(string password, string salt, int iterations, int keylen, Action<Error, string> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.CryptoModule.Crypto}, 'pbkdf2', {password}, {salt}, {iterations}, {keylen})")]
		public static Task<string> Pbkdf2Task(string password, string salt, int iterations, int keylen) { return null; }

		public static Buffer RandomBytes(int size) { return null; }
		public static void RandomBytes(int size, Action<Error, Buffer> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.CryptoModule.Crypto}, 'randomBytes', {size})")]
		public static Task<Buffer> RandomBytesTask(int size) { return null; }
	}
}
