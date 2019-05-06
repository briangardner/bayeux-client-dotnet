using System;
using System.Collections.Generic;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Extensions.TimesyncClient;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Extensions.Timesync
{
    public class TimeSyncClientExtensionReceiveMeta
    {
        [Fact]
        public void Should_Return_True_If_Ext_Does_Not_Have_Timesync()
        {
            var message = new BayeuxMessage(new Dictionary<string, object>());
            var ext = new TimesyncClientExtension();
            var result = ext.ReceiveMeta( message);
            Assert.True(result);
        }

        [Fact]
        public void Should_Set_Offset_When_Timesync_Extension_Present()
        {
            var message = new BayeuxMessage(new Dictionary<string, object>());
            var messageExt = message.GetExt(true);
            messageExt["timesync"] = new Dictionary<string,object>()
            {
                { "tc", Now},
                { "ts", Now - 10},
                { "p", 100},

            };
            var ext = new TimesyncClientExtension();
            ext.ReceiveMeta( message);
            Assert.NotEqual(0, ext.Offset);
        }

        [Fact]
        public void Should_Set_Lag_When_Timesync_Extension_Present()
        {
            var message = new BayeuxMessage(new Dictionary<string, object>());
            var messageExt = message.GetExt(true);
            messageExt["timesync"] = new Dictionary<string, object>()
            {
                { "tc", Now},
                { "ts", Now - 10},
                { "p", 100},

            };
            var ext = new TimesyncClientExtension();
            ext.ReceiveMeta( message);
            Assert.NotEqual(0, ext.Lag);
        }

        [Fact]
        public void Should_Return_True_When_Timesync_Extension_Present()
        {
            var message = new BayeuxMessage(new Dictionary<string, object>());
            var messageExt = message.GetExt(true);
            messageExt["timesync"] = new Dictionary<string, object>()
            {
                { "tc", Now},
                { "ts", Now - 10},
                { "p", 100},

            };
            var ext = new TimesyncClientExtension();
            var result = ext.ReceiveMeta( message);
            Assert.True(result);
        }

        private static long Now => (DateTime.Now.Ticks - 621355968000000000) / 10000;




    }
}
