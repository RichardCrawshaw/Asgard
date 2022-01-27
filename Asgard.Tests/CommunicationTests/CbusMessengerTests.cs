using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Asgard.Tests.CommunicationTests
{
    [TestFixture]
    public class MessengerTests
    {
        [Test]
        public async Task CbusMessenger_ShouldRaiseCorrectParsedMessage_WhenTransportRaisesMessage()
        {
            var transport = new Mock<Communications.IGridConnectProcessor>();
            var connectionFactory = new Mock<ICbusConnectionFactory>();
            connectionFactory.Setup(cf => cf.GetConnection()).Returns(transport.Object);

            var frame = new Mock<ICbusCanFrame>();
            var frameFactory = new Mock<ICbusCanFrameFactory>();
            frameFactory.Setup(ff => ff.CreateFrame(null)).Returns(frame.Object);

            var cfp = new CbusCanFrameProcessor();
            var cm = new CbusMessenger(cfp, connectionFactory.Object, frameFactory.Object);
            await cm.OpenAsync();
            ICbusMessage? m = null;
            cm.MessageReceived += (sender, args) => m = args.Message;

            transport.Raise(t => 
                t.GridConnectMessage += null, 
                new MessageReceivedEventArgs(":SB020N9101000005;"));
            var opCode = m?.GetOpCode();
            opCode
                .Should().NotBeNull()
                .And.BeOfType<AccessoryOff>()
                .Which.Should().BeEquivalentTo(new { NodeNumber = 256, EventNumber = 5 });
        }

        [Test]
        [Ignore("Reinstate when opcode construction is available (rather than just parsing incoming messages)")]
        public async Task CbusMessenger_ShouldSendCorrectlyFormattedMessage_WhenSendMessageCalled()
        {
            var transport = new Mock<Communications.IGridConnectProcessor>();
            var connectionFactory = new Mock<ICbusConnectionFactory>();
            connectionFactory.Setup(cf => cf.GetConnection()).Returns(transport.Object);

            var frame = new Mock<ICbusCanFrame>();
            var frameFactory = new Mock<ICbusCanFrameFactory>();
            frameFactory.Setup(ff => ff.CreateFrame(null)).Returns(frame.Object);

            var cfp = new CbusCanFrameProcessor();
            var cm = new CbusMessenger(cfp, connectionFactory.Object, frameFactory.Object);
            await cm.OpenAsync();

            //TODO: need to consider how we want applications to be able to send messages to the bus
            var opc = new AccessoryOn() { NodeNumber = 1, EventNumber = 2 };
            if (opc.Message is null) return;

            await cm.SendMessage(opc.Message);

            transport.Verify(t => t.SendMessage(It.Is<string>(m => m == ":SAFA0N9000010002;")));
        }
        
    }
}
