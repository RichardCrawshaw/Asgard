using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var cbusMessage = CbusMessage.Create(data);

            Assert.Multiple(() =>
            {
                Assert.That(cbusMessage, Is.Not.Null);
                Assert.That(Assert.Catch(() => OpCodeData.Create(cbusMessage), "Empty message"), Is.TypeOf<Exception>());
            });
        }
    }
}
