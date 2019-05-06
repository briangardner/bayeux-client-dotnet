using System;
using System.Collections.Generic;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Extensions.TimesyncClient;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Extensions.Timesync
{
    public class TimeSyncClientExtensionSendMeta
    {
        [Fact]
        public void Should_Set_Message_TimeSync_Tc()
        {
            var message = TestMessageWithTimesync;
            var ext = new TimesyncClientExtension();
            var channel = new BayeuxChannel(MockClientContext.Object, new ChannelId("/test"));
            ext.ReceiveMeta( message);
            var sendMessage = TestMessageNoTimesync;
            ext.SendMeta(sendMessage);
            
            var timeSync = JObject.FromObject(sendMessage.GetExt(false));
            var properties = JObject.Parse(timeSync["timesync"].ToString());
            Assert.NotNull(properties["tc"]);
        }

        [Fact]
        public void Should_Set_Message_TimeSync_Lag()
        {
            var message = TestMessageWithTimesync;
            var ext = new TimesyncClientExtension();
            var channel = new BayeuxChannel(MockClientContext.Object, new ChannelId("/test"));
            ext.ReceiveMeta(message);
            var sendMessage = TestMessageNoTimesync;
            ext.SendMeta( sendMessage);

            var timeSync = JObject.FromObject(sendMessage.GetExt(false));
            var properties = JObject.Parse(timeSync["timesync"].ToString());
            Assert.NotNull(properties["l"]);
        }

        [Fact]
        public void Should_Set_Message_TimeSync_Offset()
        {
            var message = TestMessageWithTimesync;
            var ext = new TimesyncClientExtension();
            var channel = new BayeuxChannel(MockClientContext.Object, new ChannelId("/test"));
            ext.ReceiveMeta( message);
            var sendMessage = TestMessageNoTimesync;
            ext.SendMeta( sendMessage);
            var timeSync = JObject.FromObject(sendMessage.GetExt(false));
            var properties = JObject.Parse(timeSync["timesync"].ToString());
            Assert.NotNull(properties["o"]);
        }

        [Fact]
        public void Should_Return_True_On_SendMeta()
        {
            var message = TestMessageWithTimesync;
            var ext = new TimesyncClientExtension();
            var channel = new BayeuxChannel(MockClientContext.Object, new ChannelId("/test"));
            ext.ReceiveMeta( message);
            var sendMessage = TestMessageNoTimesync;
            var result = ext.SendMeta( sendMessage);
            Assert.True(result);
        }

        private static long Now => (DateTime.Now.Ticks - 621355968000000000) / 10000;


        private readonly Mock<IBayeuxClientContext> MockClientContext = new Mock<IBayeuxClientContext>();


        private BayeuxMessage TestMessageWithTimesync
        {
            get
            {
                var msg = new BayeuxMessage(new Dictionary<string, object>());
                var messageExt = msg.GetExt(true);
                messageExt["timesync"] = new Dictionary<string, object>()
                {
                    { "tc", Now},
                    { "ts", Now - 10},
                    { "p", 100},

                };
                return msg;
            }
        }

        private BayeuxMessage TestMessageNoTimesync
        {
            get
            {
                var msg = new BayeuxMessage(new Dictionary<string, object>());
                var messageExt = msg.GetExt(true);
                return msg;
            }
        }
    }


}
