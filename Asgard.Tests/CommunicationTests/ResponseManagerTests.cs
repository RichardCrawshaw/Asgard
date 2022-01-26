using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
using Moq;
using NUnit.Framework;

namespace Asgard.Tests.CommunicationTests
{
    [TestFixture]
    public class ResponseManagerTests
    {
        [Test]
        public void RespondsToIncomingMessage()
        {
            ICbusMessage? response = null;

            var messenger = new Mock<ICbusMessenger>();

            // Create the ResponseManager and register the callback(s).
            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryNodeNumber>((messenger, message) =>
            {
                response = message;
                return Task.CompletedTask;
            });

            // Generate a received QNN message for the ResponseManager to handle.
            messenger
                .Raise(
                    m => m.MessageReceived += null,
                    new CbusMessageEventArgs(new QueryNodeNumber().Message, received: true));

            // and that it is the right type with the expected values.
            Assert.That(response, Is.Not.Null);
            Assert.That(response?.GetOpCode(), Is.TypeOf<QueryNodeNumber>());
        }

        [Test]
        public void RespondsToOnlyExpectedIncomingMessage()
        {
            List<ICbusMessage?> responses = new();

            var messenger = new Mock<ICbusMessenger>();

            // Create the ResponseManager and register the callback(s).
            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryNodeNumber>((messenger, message) =>
            {
                responses.Add(message);
                return Task.CompletedTask;
            });

            // Generate a received QNN message and some other messages for the ResponseManager to
            // handle.
            var messages = new List<ICbusMessage>
            {
                new QueryNodeNumber().Message,
                new GeneralAcknowledgement().Message,
                new GeneralNoAcknowledgement().Message,
                new DebugWithOneDataByte() { DebugStatus = 0x88, }.Message
            };

            // and receive them.
            foreach (var message in messages)
                messenger
                    .Raise(
                        m => m.MessageReceived += null,
                        new CbusMessageEventArgs(message, received: true));

            // Make sure that it only responded to the expected message.
            Assert.That(responses.Count, Is.EqualTo(1));
            Assert.That(responses.FirstOrDefault()?.GetOpCode(), Is.TypeOf<QueryNodeNumber>());
        }

        [Test]
        public void RespondsToMultipleExpectedIncomingMessage()
        {
            var responses1 = new List<ICbusMessage?>();
            var responses2 = new List<ICbusMessage?>();
            var responses3 = new List<ICbusMessage?>();

            var messenger = new Mock<ICbusMessenger>();

            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryNodeNumber>((messenger, message) =>
            {
                responses1.Add(message);
                return Task.CompletedTask;
            });
            rm.Register<QueryEngine>((messenger, message) =>
            {
                responses2.Add(message);
                return Task.CompletedTask;
            });
            rm.Register<RequestCommandStationStatus>((messenger, message) =>
            {
                responses3.Add(message);
                return Task.CompletedTask;
            });

            // Generate some messages for the ResponseManager to handle.
            var messages = new List<ICbusMessage>
            {
                new QueryNodeNumber().Message,
                new GeneralAcknowledgement().Message,
                new GeneralNoAcknowledgement().Message,
                new DebugWithOneDataByte() { DebugStatus = 0x88, }.Message,
                new QueryNodeNumber().Message,
                new QueryEngine().Message,
                new QueryEngine().Message,
                new RequestCommandStationStatus().Message,
                new QueryEngine().Message,
            };

            foreach (var message in messages)
                messenger
                    .Raise(
                        m => m.MessageReceived += null,
                        new CbusMessageEventArgs(message, received: true));

            // Make sure that it only responded to the expected message.
            Assert.That(responses1.Count, Is.EqualTo(messages.Count(m=> m.GetOpCode() is QueryNodeNumber)));
            Assert.That(responses1.FirstOrDefault()?.GetOpCode(), Is.TypeOf<QueryNodeNumber>());

            Assert.That(responses2.Count, Is.EqualTo(messages.Count(m => m.GetOpCode() is QueryEngine)));
            Assert.That(responses2.FirstOrDefault()?.GetOpCode(), Is.TypeOf<QueryEngine>());

            Assert.That(responses3.Count, Is.EqualTo(messages.Count(m=>m.GetOpCode() is RequestCommandStationStatus)));
            Assert.That(responses3.FirstOrDefault()?.GetOpCode(), Is.TypeOf<RequestCommandStationStatus>());
        }
    }
}
