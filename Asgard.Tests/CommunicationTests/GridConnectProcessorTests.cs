﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Asgard.Tests.CommunicationTests
{
    [TestFixture]
    public class GridConnectProcessorTests
    {
        [Test]
        [TestCase(":somemsg;", ":somemsg;")]
        [TestCase(":;", ":;")]
        [TestCase(":s;", ":s;")]
        public async Task GridConnectProcessor_RaisesSingleEvent_ForEachSingleMessage(string input, string output)
        {
            var mre = new ManualResetEvent(false);
            var t = new Mock<ITransport>();
            t.Setup(r => r.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<Memory<byte>, CancellationToken>((mem, _) => {
                    var inp = Encoding.ASCII.GetBytes(input).AsSpan();
                    inp.CopyTo(mem[..inp.Length].Span);
                }).ReturnsAsync(input.Length);
            var s = new Asgard.Communications.GridConnectProcessor(t.Object);

            string? msg = null;
            s.GridConnectMessage += (o, e) => {
                msg = e.Message;
                mre.Set();
            };

            await s.OpenAsync();

            mre.WaitOne(TimeSpan.FromSeconds(2));
            msg.Should().Be(output);
        }


        [Test]
        [TestCase("abc:somemsg;", ":somemsg;")]
        [TestCase(":somemsg;123", ":somemsg;")]
        [TestCase("abc:somemsg;123", ":somemsg;")]
        [TestCase("a;bc:somemsg;123", ":somemsg;")]
        public async Task StreamTransport_RaisesSingleEvent_AndIgnoresPartialMessages(string input, string output)
        {
            var mre = new ManualResetEvent(false);

            var t = new Mock<ITransport>();
            t.Setup(r => r.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<Memory<byte>, CancellationToken>((mem, _) => {
                    var inp = Encoding.ASCII.GetBytes(input).AsSpan();
                    inp.CopyTo(mem[..inp.Length].Span);
                }).ReturnsAsync(input.Length);
            var s = new Communications.GridConnectProcessor(t.Object);

            string? msg = null;
            s.GridConnectMessage += (o, e) => {
                msg = e.Message;
                mre.Set();
            };

            await s.OpenAsync();

            mre.WaitOne(TimeSpan.FromSeconds(2));
            msg.Should().Be(output);
        }
    }
}
