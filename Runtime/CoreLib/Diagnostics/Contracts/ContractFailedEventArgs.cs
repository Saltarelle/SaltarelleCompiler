namespace System.Diagnostics.Contracts {
	public sealed class ContractFailedEventArgs : EventArgs {
		private ContractFailureKind _failureKind;
		private String _message;
		private String _condition;
		private Exception _originalException;
		private bool _handled;
		private bool _unwind;

		public ContractFailedEventArgs(ContractFailureKind failureKind, String message, String condition, Exception originalException) {
			Contract.Requires(originalException == null || failureKind == ContractFailureKind.PostconditionOnException);
			_failureKind = failureKind;
			_message = message;
			_condition = condition;
			_originalException = originalException;
		}

		public String Message { get { return _message; } }
		public String Condition { get { return _condition; } }
		public ContractFailureKind FailureKind { get { return _failureKind; } }
		public Exception OriginalException { get { return _originalException; } }

		// Whether the event handler "handles" this contract failure, or to fail via escalation policy.
		public bool Handled {
			get { return _handled; }
		}

		public void SetHandled() {
			_handled = true;
		}

		public bool Unwind {
			get { return _unwind; }
		}

		public void SetUnwind() {
			_unwind = true;
		}
	}
}