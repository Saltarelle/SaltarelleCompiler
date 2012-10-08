using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NodeJS.EventsModule;

namespace NodeJS.ReadLineModule {
	[Imported]
	[IgnoreNamespace]
	[ModuleName("readline")]
	[ScriptName("Interface")]
	public class ReadLineInterface : EventEmitter {
		private ReadLineInterface() {}

		public void SetPrompt(string prompt, int length) {}

		public string Prompt() { return null; }
		public string Prompt(bool preserveCursor) { return null; }

		public void Question(string query, Action<string> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'question', {query})")]
		public Task<string> QuestionTask(string query) { return null; }

		public void Pause() {}

		public void Resume() {}

		public void Close() {}

		public void Write(string data) {}

		public void Write(string data, JsDictionary<string, string> key) {}


		public event Action<string> OnLine {
			[InlineCode("{this}.addListener('line', {value})")] add {}
			[InlineCode("{this}.removeListener('line', {value})")] remove {}
		}

		[InlineCode("{this}.once('line', {callback})")]
		public void OnceLine(Action<string> callback) {}


		public event Action OnPause {
			[InlineCode("{this}.addListener('pause', {value})")] add {}
			[InlineCode("{this}.removeListener('pause', {value})")] remove {}
		}

		[InlineCode("{this}.once('pause', {callback})")]
		public void OncePause(Action callback) {}


		public event Action OnResume {
			[InlineCode("{this}.addListener('resume', {value})")] add {}
			[InlineCode("{this}.removeListener('resume', {value})")] remove {}
		}

		[InlineCode("{this}.once('resume', {callback})")]
		public void OnceResume(Action callback) {}


		public event Action OnClose {
			[InlineCode("{this}.addListener('close', {value})")] add {}
			[InlineCode("{this}.removeListener('close', {value})")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action callback) {}


		public event Action OnSigint {
			[InlineCode("{this}.addListener('SIGINT', {value})")] add {}
			[InlineCode("{this}.removeListener('SIGINT', {value})")] remove {}
		}

		[InlineCode("{this}.once('SIGINT', {callback})")]
		public void OnceSigint(Action callback) {}


		public event Action OnSigtstp {
			[InlineCode("{this}.addListener('SIGTSTP', {value})")] add {}
			[InlineCode("{this}.removeListener('SIGTSTP', {value})")] remove {}
		}

		[InlineCode("{this}.once('SIGTSTP', {callback})")]
		public void OnceSigtstp(Action callback) {}


		public event Action OnSigcont {
			[InlineCode("{this}.addListener('SIGCONT', {value})")] add {}
			[InlineCode("{this}.removeListener('SIGCONT', {value})")] remove {}
		}

		[InlineCode("{this}.once('SIGCONT', {callback})")]
		public void OnceSigcont(Action callback) {}
	}
}