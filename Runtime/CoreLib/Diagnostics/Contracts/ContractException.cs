namespace System.Diagnostics.Contracts
{
    internal sealed class ContractException : Exception
    {
        readonly ContractFailureKind _Kind;
        readonly string _UserMessage;
        readonly string _Condition;
 
        public ContractFailureKind Kind { get { return _Kind; } }
        public string Failure { get { return this.Message; } }
        public string UserMessage { get { return _UserMessage; } }
        public string Condition { get { return _Condition; } }
 
        public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException)
            : base(failure, innerException)
        {
            this._Kind = kind;
            this._UserMessage = userMessage;
            this._Condition = condition;
        }
    }
}