using System;
using Asgard.Data;
using NUnit.Framework;

namespace Asgard.Tests
{
    [TestFixture]
    public class NonOpCodeDataTests
    {
        [Test]
        public void EmptyMessageTest()
        {
            var data = Array.Empty<byte>();

            var cbusMessage = CbusStandardMessage.Create(data);

            Assert.Multiple(() =>
            {
                Assert.That(cbusMessage, Is.Not.Null);
                Assert.That(
                    Assert.Catch(() => OpCodeData.Create(cbusMessage), "Empty message"),
                    Is.TypeOf<Exception>());
            });
        }
    }
}
