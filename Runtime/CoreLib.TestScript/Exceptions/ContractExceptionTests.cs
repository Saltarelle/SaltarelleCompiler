using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using QUnit;

namespace CoreLib.TestScript.Exceptions
{
    [TestFixture]
    public class ContractExceptionTests
    {
        [Test]
        public void TypePropertiesAreCorrect()
        {
            Assert.AreEqual(typeof(ContractException).FullName, "ss.ContractException", "Name");
            Assert.IsTrue(typeof(ContractException).IsClass, "IsClass");
            Assert.AreEqual(typeof(ContractException).BaseType, typeof(Exception), "BaseType");
            object d = new ContractException(ContractFailureKind.Assert, "Contract failed", null, null, null);
            Assert.IsTrue(d is ContractException, "is ContractException");
            Assert.IsTrue(d is Exception, "is Exception");

            var interfaces = typeof(ContractException).GetInterfaces();
            Assert.AreEqual(interfaces.Length, 0, "Interfaces length");
        }

        [Test]
        public void DefaultConstructorWorks()
        {
            var ex = new ContractException(ContractFailureKind.Assert, "Contract failed", null, null, null);
            Assert.IsTrue((object)ex is ContractException, "is ContractException");
            Assert.IsTrue(ex.Kind == ContractFailureKind.Assert, "ContractFailureKind");
            Assert.IsTrue(ex.InnerException == null, "InnerException");
            Assert.IsTrue(ex.Condition == null, "Condition");
            Assert.IsTrue(ex.UserMessage == null, "UserMessage");
            Assert.AreEqual(ex.Message, "Contract failed");
        }
    }
}
