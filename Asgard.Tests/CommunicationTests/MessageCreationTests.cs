using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Tests.CommunicationTests
{
    [TestFixture]
    public class MessageCreationTests
    {
        [Test]
        [Ignore("Reinstate when construction of opcodes is possible from an application, and when they've been migrated to use ushort rather than short")]
        [TestCase(1, 2, ":S0000N9000010002;")]
        [TestCase(260, 260, ":S0000N9001040104;")]
        [TestCase(20, 20, ":S0000N9000140014;")]
        [TestCase(196, 196, ":S0000N9000C400C4;")]
        [TestCase(47802, 47802, ":S0000N90BABABABA;")]
        public void AconShouldGenerateTheCorrectTransportString(ushort nodeNumber, ushort eventNumber, string expectedTransportString)
        {
            /*
            var m = new ACON(null)
            {
                NodeNumber = nodeNumber,
                EventNumber = eventNumber
            };

            m.TransportString.Should().Be(expectedTransportString);
            */
        }
    }
}
