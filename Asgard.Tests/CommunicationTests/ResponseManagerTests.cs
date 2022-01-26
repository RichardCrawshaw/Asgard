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
    }
}
