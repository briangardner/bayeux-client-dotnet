using System;
using System.Collections.Generic;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Extensions.TimesyncClient.Logging;

namespace FinancialHq.Bayeux.Extensions.TimesyncClient
{
    public class TimesyncClientExtension : IExtension
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        public int Offset { get; private set; }
        public int Lag { get; private set; }

        public bool Receive(BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(BayeuxMessage message)
        {
            Log.Debug("Timesync Extension - Receive Meta start");
            var ext = (Dictionary<string, object>)message.GetExt(false);

            var sync = (Dictionary<string, object>)ext?["timesync"];
            if (sync == null)
            {
                return true;
            }

            var now = (DateTime.Now.Ticks - 621355968000000000) / 10000;

            var tc = SafeConvertToLong(sync["tc"]);
            var ts = SafeConvertToLong(sync["ts"]);
            var p = SafeConvertToInt(sync["p"]);
            // final int a=((Number)sync.get("a")).intValue();

            var l2 = (int)((now - tc - p) / 2);
            var o2 = (int)(ts - tc - l2);

            Lag = Lag == 0 ? l2 : (Lag + l2) / 2;
            Offset = Offset == 0 ? o2 : (Offset + o2) / 2;

            Log.Debug("Timesync Extension - Receive Meta end");
            return true;
        }

        public bool Send(BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta(BayeuxMessage message)
        {
            Log.Debug("Timesync Extension - Send Meta start");
            var ext = (Dictionary<string, object>)message.GetExt(true);
            var now = (DateTime.Now.Ticks - 621355968000000000) / 10000;
            // Changed JSON.Literal to string
            var timesync = "{\"tc\":" + now + ",\"l\":" + Lag + ",\"o\":" + Offset + "}";
            ext["timesync"] = timesync;
            Log.Debug("Timesync Extension - Send Meta end");
            return true;
        }

        private long SafeConvertToLong(object str)
        {
            return long.TryParse(str.ToString(), out var number) ? number : default;
        }

        private int SafeConvertToInt(object str)
        {
            return int.TryParse(str.ToString(), out var number) ? number : default;
        }
    }
}
