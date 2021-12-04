using Asgard.Communications;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Tests.CommunicationTests
{
    [TestFixture]
    public class MessengerTests
    {
        [Test]
        public void CbusMessenger_ShouldRaiseCorrectParsedMessage_WhenTransportRaisesMessage()
        {
            var transport = new Mock<IGridConnect>();
            var cm = new CbusMessenger(transport.Object);
            ICbusMessage m = null;
            cm.MessageReceived += (sender, args) => {
                m = args.Message;
            };

            transport.Raise(t => t.GridConnectMessage += null, new MessageReceivedEventArgs(":SB020N9101000005;"));
            var opCode = m.GetOpCode();
            opCode.Should().NotBeNull()
                .And.BeOfType<ACOF>()
                .Which.Should().BeEquivalentTo(new { NodeNumber = 256, EventNumber = 5 });
        }

        [Test]
        [Ignore("Reinstate when opcode construction is available (rather than just parsing incoming messages)")]
        public async Task CbusMessenger_ShouldSendCorrectlyFormattedMessage_WhenSendMessageCalled()
        {
            var transport = new Mock<IGridConnect>();
            var cm = new CbusMessenger(transport.Object);

            //TODO: need to consider how we want applications to be able to send messages to the bus
            var opc = new ACON(null) { NodeNumber = 1, EventNumber = 2 };

            await cm.SendMessage(opc.Message);

            transport.Verify(t => t.SendMessage(It.Is<string>(m => m == ":SAFA0N9000010002;")));
        }
        
    }
}
