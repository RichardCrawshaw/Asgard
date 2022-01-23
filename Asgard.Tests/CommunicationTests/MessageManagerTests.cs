using Asgard.Communications;
using Asgard.Data;
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
    public class MessageManagerTests
    {
        [Test]
        public async Task ManagerReturnsCorrectMessages_WhenWaitingForMultipleReplies()
        {
            var messenger = new Mock<ICbusMessenger>();
            messenger.Setup(m => m.SendMessage(It.IsAny<ICbusMessage>())).ReturnsAsync(true).Callback(() => {
                messenger.Raise(m => 
                    m.MessageReceived += null, 
                    new CbusMessageEventArgs(new ResponseToQueryNode().Message, true));
                messenger.Raise(m => 
                    m.MessageReceived += null, 
                    new CbusMessageEventArgs(new ResponseToQueryNode().Message, true));
            });

            var mm = new MessageManager(messenger.Object);
            var response = await mm.SendMessageWaitForReplies<ResponseToQueryNode>(new QueryNodeNumber());

            response.Count().Should().Be(2);
        }

        [Test]
        public async Task ManagerReturnsCorrectMessages_WhenWaitingForASingleReply()
        {
            var messenger = new Mock<ICbusMessenger>();
            messenger.Setup(m => m.SendMessage(It.IsAny<ICbusMessage>())).ReturnsAsync(true).Callback(() => {
                messenger.Raise(m => 
                    m.MessageReceived += null, 
                    new CbusMessageEventArgs(
                        new ResponseToQueryNode() { NodeNumber = 1 }.Message, true));
                messenger.Raise(m => 
                    m.MessageReceived += null, 
                    new CbusMessageEventArgs(
                        new ResponseToQueryNode() { NodeNumber = 2 }.Message, true));
            });

            var mm = new MessageManager(messenger.Object);
            var response = await mm.SendMessageWaitForReply<ResponseToQueryNode>(new QueryNodeNumber());

            response.NodeNumber.Should().Be(1);
        }


        [Test]
        public async Task ManagerReturnsCorrectMessages_WhenWaitingForASingleReply_WithAFilter()
        {
            var messenger = new Mock<ICbusMessenger>();
            messenger
                .Setup(m => m.SendMessage(It.IsAny<CbusMessage>())).ReturnsAsync(true)
                .Callback(() =>
                {
                    messenger.Raise(m => 
                        m.MessageReceived += null, 
                        new CbusMessageEventArgs(
                            new ResponseToQueryNode() { NodeNumber = 1 }.Message, true));
                    messenger.Raise(m => 
                        m.MessageReceived += null, 
                        new CbusMessageEventArgs(
                            new ResponseToQueryNode() { NodeNumber = 2 }.Message, true));
                });

            var mm = new MessageManager(messenger.Object);
            var response = await mm.SendMessageWaitForReply<ResponseToQueryNode>(new QueryNodeNumber(), m => m.NodeNumber == 2);

            response.NodeNumber.Should().Be(2);
        }

        [Test]
        public void ManagerShouldTimeout_WhenNotAllExpectedArrive()
        {
            var messenger = new Mock<ICbusMessenger>();
            messenger
                .Setup(m => m.SendMessage(It.IsAny<CbusMessage>())).ReturnsAsync(true)
                .Callback(() => 
                    messenger.Raise(m => 
                        m.MessageReceived += null, 
                        new CbusMessageEventArgs(
                            new ResponseToQueryNode() { NodeNumber = 1 }.Message, true)));

            var mm = new MessageManager(messenger.Object);
            Assert.ThrowsAsync<TimeoutException>(async () => await 
                mm.SendMessageWaitForReplies<ResponseToQueryNode>(
                    new QueryNodeNumber(), 2));
        }

        [Test]
        public async Task Manager_ReturnsCorrectMessages_WhenMultipleRequestsAreInFlight()
        {
            var messenger = new Mock<ICbusMessenger>();
            messenger.Setup(m => m.SendMessage(It.IsAny<CbusMessage>())).ReturnsAsync(true);
            var mm = new MessageManager(messenger.Object);

            var responseTask1 = mm.SendMessageWaitForReply(new RequestEngineSession() { Address = 10 });
            var responseTask2 = mm.SendMessageWaitForReply(new RequestEngineSession() { Address = 20 });
            var responseTask3 = mm.SendMessageWaitForReply(new RequestEngineSession() { Address = 30 });


            messenger.Raise(m => 
                m.MessageReceived += null, 
                new CbusMessageEventArgs(
                    new EngineReport() { Address = 20 }.Message, received: true));
            messenger.Raise(m => 
                m.MessageReceived += null, 
                new CbusMessageEventArgs(
                    new EngineReport() { Address = 10 }.Message, received: true));
            messenger.Raise(m => 
                m.MessageReceived += null, 
                new CbusMessageEventArgs(
                    new CommandStationErrorReport() { Data1 = 0, Data2 = 30 }.Message, received: true));

            var response1 = await responseTask1;
            var response2 = await responseTask2;
            var response3 = await responseTask3;

            response1.Should().BeOfType<EngineReport>();
            response2.Should().BeOfType<EngineReport>();
            response3.Should().BeOfType<CommandStationErrorReport>();

            var r1 = (EngineReport)response1;
            var r2 = (EngineReport)response2;
            var r3 = (CommandStationErrorReport)response3;

            r1.Address.Should().Be(10);
            r2.Address.Should().Be(20);
            r3.Data2.Should().Be(30);
        }


    }
}
