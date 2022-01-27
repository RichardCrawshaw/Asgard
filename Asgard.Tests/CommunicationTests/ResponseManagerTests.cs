using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using static Asgard.Communications.ResponseManager;

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
            rm.Register<QueryNodeNumber>((messenger, message, opc) =>
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
            rm.Register<QueryNodeNumber>((messenger, message, opc) =>
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
            rm.Register<QueryNodeNumber>((messenger, message, opc) =>
            {
                responses1.Add(message);
                return Task.CompletedTask;
            });
            rm.Register<QueryEngine>((messenger, message, opc) =>
            {
                responses2.Add(message);
                return Task.CompletedTask;
            });
            rm.Register<RequestCommandStationStatus>((messenger, message, opc) =>
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

        [Test]
        public void ResponseManager_CallsMultipleCallbacks_WhenRegisteredForSameType()
        {
            var messenger = new Mock<ICbusMessenger>();

            var rm = new ResponseManager(messenger.Object);
            var count1 = 0;
            rm.Register<QueryEngine>((a, b, c) =>
            {
                count1++;
                return Task.CompletedTask;
            });
            var count2 = 0;
            rm.Register<QueryEngine>((a, b, c) =>
            {
                count2++;
                return Task.CompletedTask;
            });


            messenger
                    .Raise(
                        m => m.MessageReceived += null,
                        new CbusMessageEventArgs(new QueryEngine().Message, received: true));
            messenger
                    .Raise(
                        m => m.MessageReceived += null,
                        new CbusMessageEventArgs(new QueryEngine().Message, received: true));

            count1.Should().Be(2);
            count2.Should().Be(2);
        }

        [Test]
        public void ResponseManager_AddsASingleEventHandler_WhenMultipleRegistersUsed()
        {
            var messenger = new Mock<ICbusMessenger>();

            MessageCallback<QueryEngine> cb1 = (_, _, _) => Task.CompletedTask;
            MessageCallback<QueryNodeNumber> cb2 = (_, _, _) => Task.CompletedTask;

            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryEngine>(cb1);
            rm.Register<QueryNodeNumber>(cb2);

            messenger.VerifyAdd(m => m.MessageReceived += It.IsAny<EventHandler<CbusMessageEventArgs>>(), Times.Exactly(1));
        }

        [Test]
        public void ResponseManager_RemovesEventHandler_WhenDeregistersCalled()
        {
            var messenger = new Mock<ICbusMessenger>();

            MessageCallback<QueryEngine> cb1 = (_, _, _) => Task.CompletedTask;
            MessageCallback<QueryNodeNumber> cb2 = (_, _, _) => Task.CompletedTask;

            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryEngine>(cb1);
            rm.Register<QueryNodeNumber>(cb2);

            rm.Deregister<QueryEngine>(cb1);
            rm.Deregister<QueryNodeNumber>(cb2);


            messenger.VerifyRemove(m => m.MessageReceived -= It.IsAny<EventHandler<CbusMessageEventArgs>>(), Times.Exactly(1));
        }

        [Test]
        public void ResponseManager_DoesNotRemoveHandler_WhenNotAllDeregistersForTypeCalled()
        {
            var messenger = new Mock<ICbusMessenger>();

            MessageCallback<QueryEngine> cb1 = (_, _, _) => Task.CompletedTask;
            MessageCallback<QueryEngine> cb2 = (_, _, _) => Task.CompletedTask;

            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryEngine>(cb1);
            rm.Register<QueryEngine>(cb2);

            rm.Deregister<QueryEngine>(cb1);


            messenger.VerifyRemove(m => m.MessageReceived -= It.IsAny<EventHandler<CbusMessageEventArgs>>(), Times.Exactly(0));
        }

        [Test]
        public void ResponseManager_RemovesHandler_WhenAllDeregistersForTypeCalled()
        {
            var messenger = new Mock<ICbusMessenger>();

            MessageCallback<QueryEngine> cb1 = (_, _, _) => Task.CompletedTask;
            MessageCallback<QueryEngine> cb2 = (_, _, _) => Task.CompletedTask;

            var rm = new ResponseManager(messenger.Object);
            rm.Register<QueryEngine>(cb1);
            rm.Register<QueryEngine>(cb2);

            rm.Deregister<QueryEngine>(cb1);
            rm.Deregister<QueryEngine>(cb2);

            messenger.VerifyRemove(m => m.MessageReceived -= It.IsAny<EventHandler<CbusMessageEventArgs>>(), Times.Exactly(1));
        }


        [Test]
        public void ResponseManager_FiltersMessages_WhenAFilterIsSpecified()
        {
            var messenger = new Mock<ICbusMessenger>();
            messenger
                .Setup(m => m.SendMessage(It.IsAny<CbusMessage>()))
                .ReturnsAsync(true);
            var rm = new ResponseManager(messenger.Object);

            var count1 = 0;
            rm.Register<EngineReport>((a, b, report) =>
            {
                count1++;
                return Task.CompletedTask;
            }, er => er.Address == 10);

            var count2 = 0;
            rm.Register<EngineReport>((a, b, report) =>
            {
                count2++;
                return Task.CompletedTask;
            }, er => er.Address == 20);



            messenger
                .Raise(
                    m => m.MessageReceived += null,
                    new CbusMessageEventArgs(
                        new EngineReport() { Address = 20 }.Message, received: true));
            messenger
                .Raise(
                    m => m.MessageReceived += null,
                    new CbusMessageEventArgs(
                        new EngineReport() { Address = 10 }.Message, received: true));

            messenger
                .Raise(
                    m => m.MessageReceived += null,
                    new CbusMessageEventArgs(
                        new EngineReport() { Address = 20 }.Message, received: true));

            count1.Should().Be(1);
            count2.Should().Be(2);
        }
    }
}
