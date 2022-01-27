using Asgard.Communications;
using FluentAssertions;
using NUnit.Framework;
using Asgard.Data;

namespace Asgard.Tests.CommunicationTests
{
    public class MessageParsingTests
    {
        [Test]
        public void CbusMessage_ParsesBasicMessage()
        {
            var cfp = new CbusCanFrameProcessor();
            var frame = cfp.ParseFrame(":SB020N9101000005;");
            var m = frame?.Message?.GetOpCode();

            m.Should().BeOfType<AccessoryOff>();
            var msg = (AccessoryOff?)m;
            Assert.IsNotNull(msg);
            msg?.NodeNumber.Should().Be(256);
            msg?.EventNumber.Should().Be(5);
        }
    }
}
