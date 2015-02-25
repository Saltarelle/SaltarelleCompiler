namespace System.Diagnostics.Contracts
{
    /// <summary>
    /// Allows setting contract and tool options at assembly, type, or method granularity.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [Conditional("CONTRACTS_FULL")]
    public sealed class ContractOptionAttribute : Attribute
    {
        public ContractOptionAttribute(String category, String setting, bool enabled)
        {
            Category = category;
            Setting = setting;
            Enabled = enabled;
        }

        public ContractOptionAttribute(String category, String setting, String value)
        {
            Category = category;
            Setting = setting;
            Value = value;
        }

        public String Category { get; private set; }

        public String Setting { get; private set; }

        public bool Enabled { get; private set; }

        public String Value { get; private set; }
    }
}