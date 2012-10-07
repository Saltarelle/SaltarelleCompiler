using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS {
	[Imported]
	[Serializable]
	public class MemoryUsage {
		public int Rss { get; set; }
		public int HeapTotal { get; set; }
		public int HeapUsed { get; set; }
	}

	[Imported]
	[ModuleName(null)]
	[IgnoreNamespace]
	[ScriptName("process")]
	public static class Process {
		[IntrinsicProperty]
		public static WritableStream Stdout { get { return null; } }

		[IntrinsicProperty]
		public static WritableStream Stderr { get { return null; } }

		[IntrinsicProperty]
		public static ReadableStream Stdin { get { return null; } }

		[IntrinsicProperty]
		public static string[] Argv { get { return null; } }

		[IntrinsicProperty]
		public static string ExecPath { get { return null; } }

		public static void Abort() {}

		public static void Chdir(string directory) {}

		public static string Cwd { [ScriptName("cwd")] get { return null; } }

		[IntrinsicProperty]
		public static JsDictionary<string, string> Env { get { return null; } }

		public static void Exit() {}
		public static void Exit(int code) {}

		[ScriptName("getgid")]
		public static int GetGID() { return 0; }

		[ScriptName("setgid")]
		public static void SetGID(int id) {}

		[ScriptName("getuid")]
		public static int GetUID() { return 0; }

		[ScriptName("setuid")]
		public static void SetUID(int id) {}

		[IntrinsicProperty]
		public static string Version { get { return null; } }

		[IntrinsicProperty]
		public static JsDictionary<string, string> Versions { get { return null; } }

		[IntrinsicProperty]
		public static dynamic Config { get { return null; } }

		public static void Kill(int pid) {}
		public static void Kill(int pid, string signal) {}

		[IntrinsicProperty]
		public static int Pid { get { return 0; } }

		[IntrinsicProperty]
		public static string Title { get; set; }

		[IntrinsicProperty]
		public static string Arch { get { return null; } }

		[IntrinsicProperty]
		public static string Platform { get { return null; } }

		public static MemoryUsage MemoryUsage { [ScriptName("memoryUsage")] get { return null; } }

		public static void NextTick(Action callback) {}

		[ScriptName("umask")]
		public static int UMask() { return 0; }

		[ScriptName("umask")]
		public static int UMask(int mask) { return 0; }

		public static double Uptime() { return 0; }

		public static int[] Hrtime() { return null; }
		public static int[] Hrtime(int[] relativeTo) { return null; }

		// EventEmitter

		public static void AddListener(string @event, Delegate listener) {}

		public static void On(string @event, Delegate listener) {}

		public static void Once(string @event, Delegate listener) {}

		public static void RemoveListener(string @event, Delegate listener) {}

		public static void RemoveAllListeners(string @event) {}

		public static void SetMaxListeners(int n) {}

		public static Delegate[] Listeners(string @event) { return null; }


		public static event Action OnExit {
			[InlineCode("process.addListener('exit', {value})")] add {}
			[InlineCode("process.removeListener('exit', {value})")] remove {}
		}

		[InlineCode("process.once('exit', {callback})")]
		public static void OnceExit(Action callback) {}


		public static event Action<Error> OnUncaughtException {
			[InlineCode("process.addListener('uncaughtException', {value})")] add {}
			[InlineCode("process.removeListener('uncaughtException', {value})")] remove {}
		}

		[InlineCode("process.once('uncaughtException', {callback})")]
		public static void OnceUncaughtException(Action<Error> callback) {}
	}
}
