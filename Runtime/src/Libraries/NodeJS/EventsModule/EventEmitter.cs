using System;
using System.Runtime.CompilerServices;

namespace NodeJS.EventsModule {
	[Imported]
	[ModuleName("events")]
	[IgnoreNamespace]
	public class EventEmitter {
		public void AddListener(string @event, Delegate listener) {}

		public void On(string @event, Delegate listener) {}

		public void Once(string @event, Delegate listener) {}

		public void RemoveListener(string @event, Delegate listener) {}

		public void RemoveAllListeners(string @event) {}

		public void SetMaxListeners(int n) {}

		public Delegate[] Listeners(string @event) { return null; }

		[ExpandParams]
		public void Emit(string @event, params object[] args) {}


		public event Action<string, Delegate> OnNewListener {
			[InlineCode("{this}.addListener('newListener', {value}')")] add {}
			[InlineCode("{this}.removeListener('newListener', {value}')")] remove {}
		}

		[InlineCode("{this}.once('newListener', {callback})")]
		public void OnceNewListener(Action<string, Delegate> callback) {}
	}
}
