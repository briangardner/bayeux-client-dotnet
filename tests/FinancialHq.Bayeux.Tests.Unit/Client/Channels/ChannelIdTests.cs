using System;
using FinancialHq.Bayeux.Client.Channels;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Channels
{
    public class ChannelIdTests
    {
        [Fact]
        public void Should_Throw_Exception_When_Name_Null()
        {
            Assert.Throws<ArgumentException>(() => new ChannelId(null as string));
        }

        [Fact]
        public void Should_Throw_Exception_When_Name_Empty()
        {
            Assert.Throws<ArgumentException>(() => new ChannelId(string.Empty));
        }

        [Fact]
        public void Should_Throw_Exception_When_Name_Whitespace()
        {
            Assert.Throws<ArgumentException>(() => new ChannelId(" "));
        }

        [Fact]
        public void Should_Throw_Exception_When_Name_Does_Not_Start_With_Slash()
        {
            Assert.Throws<ArgumentException>(() => new ChannelId("dummy"));
        }

        [Fact]
        public void Should_Throw_Exception_When_Name_Is_Only_Slash()
        {
            Assert.Throws<ArgumentException>(() => new ChannelId("/"));
        }
    }
}