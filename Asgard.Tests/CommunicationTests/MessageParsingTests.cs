using Asgard.Communications;
using FluentAssertions;
using NUnit.Framework;
using Asgard.Data;

namespace Asgard.Tests.CommunicationTests
{
    public class MessageParsingTests
    {
        [Test]
        public void CbusMessage_ParseStandardFrame()
        {
            var cfp = new CbusCanFrameProcessor();
            var frame = cfp.ParseFrame(":SB020N9101000005;");

            Assert.That(frame, Is.Not.Null);
            Assert.That(frame, Is.TypeOf<CbusCanFrame>());
            Assert.False(frame?.IsExtended);
            Assert.That(frame?.Message, Is.Not.Null);
            Assert.False(frame?.Message.IsExtended);

            Assert.That(frame?.SidH, Is.EqualTo(0xB0));
            Assert.That(frame?.SidL, Is.EqualTo(0x20));

            Assert.That(frame?.Message[0], Is.EqualTo(0x91));
            Assert.That(frame?.Message[1], Is.EqualTo(0x01));
            Assert.That(frame?.Message[2], Is.EqualTo(0x00));
            Assert.That(frame?.Message[3], Is.EqualTo(0x00));
            Assert.That(frame?.Message[4], Is.EqualTo(0x05));
        }

        [Test]
        public void CbusMessage_ParseExtendedFrame()
        {
            var cfp = new CbusCanFrameProcessor();
            var frame = cfp.ParseFrame(":X00080004N000800000D020000;"); // Extended message with 8 data bytes

            Assert.That(frame, Is.Not.Null);
            Assert.That(frame, Is.TypeOf<CbusExtendedCanFrame>());
            Assert.IsTrue(frame?.IsExtended);
            Assert.That(frame?.Message, Is.Not.Null);
            Assert.IsTrue(frame?.Message.IsExtended);

            if (frame is CbusExtendedCanFrame extendedFrame)
            {
                Assert.That(extendedFrame.SidH, Is.EqualTo(0x00));
                Assert.That(extendedFrame.SidL, Is.EqualTo(0x08));

                Assert.That(extendedFrame.EidH, Is.EqualTo(0x00));
                Assert.That(extendedFrame.EidL, Is.EqualTo(0x04));

                Assert.That(extendedFrame.Message[0], Is.EqualTo(0x00));
                Assert.That(extendedFrame.Message[1], Is.EqualTo(0x08));
                Assert.That(extendedFrame.Message[2], Is.EqualTo(0x00));
                Assert.That(extendedFrame.Message[3], Is.EqualTo(0x00));
                Assert.That(extendedFrame.Message[4], Is.EqualTo(0x0D));
                Assert.That(extendedFrame.Message[5], Is.EqualTo(0x02));
                Assert.That(extendedFrame.Message[6], Is.EqualTo(0x00));
                Assert.That(extendedFrame.Message[7], Is.EqualTo(0x00));
            }
        }

        [Test]
        public void CbusMessage_ParsesBasicMessage()
        {
            var cfp = new CbusCanFrameProcessor();
            var frame = cfp.ParseFrame(":SB020N9101000005;");
            if (frame?.Message?.TryGetOpCode(out var opCode) ?? false)
            {
                opCode.Should().BeOfType<AccessoryOff>();
                var msg = (AccessoryOff?)opCode;
                Assert.IsNotNull(msg);
                msg?.NodeNumber.Should().Be(256);
                msg?.EventNumber.Should().Be(5);
            }
            else
                Assert.Fail("Failed to retrieve op-code.");
        }
    }
}
