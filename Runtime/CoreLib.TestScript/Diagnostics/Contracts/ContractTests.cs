#define CONTRACTS_FULL

using System;
using System.Diagnostics.Contracts;
using QUnit;

namespace CoreLib.TestScript.Diagnostics.Contracts
{
    [TestFixture]
    public class ContractTests
    {
        private void AssertNoExceptions(Action block)
        {
            try
            {
                block();
                QUnit.Assert.IsTrue(true, "No Exception thrown.");
            }
            catch (Exception ex)
            {
                QUnit.Assert.Fail("Unexpected Exception");
            }
        }

        private void AssertException(Action block, ContractFailureKind expectedKind, string expectedMessage, string expectedUserMessage, Exception expectedInnerException)
        {
            try
            {
                block();
            }
            catch (Exception ex)
            {
                ContractException cex = ex as ContractException;
                if (cex == null)
                    QUnit.Assert.Fail("Unexpected Exception");

                QUnit.Assert.IsTrue(cex.Kind == expectedKind, "Kind");
                QUnit.Assert.IsTrue(cex.Message == expectedMessage, "Message");
                QUnit.Assert.IsTrue(cex.UserMessage == expectedUserMessage, "UserMessage");
                if (cex.InnerException != null)
                    QUnit.Assert.IsTrue(cex.InnerException.Equals(expectedInnerException), "InnerException");
                else if (cex.InnerException == null && expectedInnerException != null)
                    QUnit.Assert.Fail("InnerException");
            }
        }

        [Test]
        public void Assume()
        {
            int a = 0;
            QUnit.Assert.Throws<ContractException>(() => Contract.Assume(a != 0), "ContractException");
            AssertNoExceptions(() => Contract.Assume(a == 0));
            AssertException(() => Contract.Assume(a == 99), ContractFailureKind.Assume, "Contract 'a === 99' failed", null, null);
        }

        [Test]
        public void AssumeWithUserMessage()
        {
            int a = 0;
            QUnit.Assert.Throws<ContractException>(() => Contract.Assume(a != 0, "is not zero"), "ContractException");
            AssertNoExceptions(() => Contract.Assume(a == 0, "is zero"));
            AssertException(() => Contract.Assume(a == 99, "is 99"), ContractFailureKind.Assume, "Contract 'a === 99' failed: is 99", "is 99", null);
        }

        [Test]
        public void Assert()
        {
            int a = 0;
            QUnit.Assert.Throws<ContractException>(() => Contract.Assert(a != 0), "ContractException");
            AssertNoExceptions(() => Contract.Assert(a == 0));
            AssertException(() => Contract.Assert(a == 99), ContractFailureKind.Assert, "Contract 'a === 99' failed", null, null);
        }

        [Test]
        public void AssertWithUserMessage()
        {
            int a = 0;
            QUnit.Assert.Throws<ContractException>(() => Contract.Assert(a != 0, "is not zero"), "ContractException");
            AssertNoExceptions(() => Contract.Assert(a == 0, "is zero"));
            AssertException(() => Contract.Assert(a == 99, "is 99"), ContractFailureKind.Assert, "Contract 'a === 99' failed: is 99", "is 99", null);
        }

        [Test]
        public void Requires()
        {
            int a = 0;
            QUnit.Assert.Throws<ContractException>(() => Contract.Requires(a != 0), "ContractException");
            AssertNoExceptions(() => Contract.Requires(a == 0));
            AssertException(() => Contract.Requires(a == 99), ContractFailureKind.Precondition, "Contract 'a === 99' failed", null, null);
        }

        [Test]
        public void RequiresWithUserMessage()
        {
            int a = 0;
            QUnit.Assert.Throws<ContractException>(() => Contract.Requires(a != 0, "must not be zero"), "ContractException");
            AssertNoExceptions(() => Contract.Requires(a == 0, "can only be zero"));
            AssertException(() => Contract.Requires(a == 99, "can only be 99"), ContractFailureKind.Precondition, "Contract 'a === 99' failed: can only be 99", "can only be 99", null);
        }

        [Test]
        public void RequiresWithTypeException()
        {
            int a = 0;
            QUnit.Assert.Throws<Exception>(() => Contract.Requires<Exception>(a != 0), "Exception");
            AssertNoExceptions(() => Contract.Requires<Exception>(a == 0));
        }

        [Test]
        public void RequiredWithTypeExceptionAndUserMessage()
        {
            int a = 0;
            QUnit.Assert.Throws<Exception>(() => Contract.Requires<Exception>(a != 0, "must not be zero"), "Exception");
            AssertNoExceptions(() => Contract.Requires<Exception>(a == 0, "can only be zero"));
        }

        [Test]
        public void ForAll()
        {
            QUnit.Assert.Throws<ArgumentNullException>(() => Contract.ForAll(2, 5, null), "ArgumentNullException");
            AssertNoExceptions(() => Contract.ForAll(2, 5, s => s != 3));
            QUnit.Assert.IsFalse(Contract.ForAll(2, 5, s => s != 3));
            QUnit.Assert.IsTrue(Contract.ForAll(2, 5, s => s != 6));
        }

        [Test]
        public void ForAllWithCollection()
        {
            QUnit.Assert.Throws<ArgumentNullException>(() => Contract.ForAll(new [] { 1, 2, 3 }, null), "ArgumentNullException");
            AssertNoExceptions(() => Contract.ForAll(new[] { 1, 2, 3 }, s => s != 3));
            QUnit.Assert.IsFalse(Contract.ForAll(new[] { 1, 2, 3 }, s => s != 3));
            QUnit.Assert.IsTrue(Contract.ForAll(new[] { 1, 2, 3 }, s => s != 6));
        }

        [Test]
        public void Exists()
        {
            QUnit.Assert.Throws<ArgumentNullException>(() => Contract.Exists(1, 5, null), "ArgumentNullException");
            AssertNoExceptions(() => Contract.Exists(1, 5, s => s == 3));
            QUnit.Assert.IsTrue(Contract.Exists(1, 5, s => s == 3));
            QUnit.Assert.IsFalse(Contract.Exists(1, 5, s => s == 6));
        }

        [Test]
        public void ExistsWithCollection()
        {
            QUnit.Assert.Throws<ArgumentNullException>(() => Contract.Exists(new[] { 1, 2, 3 }, null), "ArgumentNullException");
            AssertNoExceptions(() => Contract.Exists(new[] { 1, 2, 3 }, s => s == 3));
            QUnit.Assert.IsTrue(Contract.Exists(new[] { 1, 2, 3 }, s => s == 3));
            QUnit.Assert.IsFalse(Contract.Exists(new[] { 1, 2, 3 }, s => s == 6));
        }
    }
}
