using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace System
{
    [Imported(ObeysTypeSystem = true)]
    [ScriptNamespace("ss")]
    public class ContractException : Exception
    {
        [ScriptName("")]
        public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innterException) { }
        
        public ContractFailureKind Kind { get { return default(ContractFailureKind); } }
        public string UserMessage { get { return null;  } }
        public string Condition { get { return null; } }
    }
}
