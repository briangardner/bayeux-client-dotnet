using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client
{
    // On defining .NET events
    // https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/event
    // https://stackoverflow.com/questions/3880789/why-should-we-use-eventhandler
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines
    public class EventReceivedArgs : EventArgs
    {
        readonly JObject ev;

        public EventReceivedArgs(JObject ev)
        {
            this.ev = ev;
        }

        // https://docs.cometd.org/current/reference/#_code_data_code
        // The data message field is an arbitrary JSON encoded *object*
        public JObject Data { get => (JObject)ev["data"]; }

        public string Channel { get => (string)ev["channel"]; }

        public JObject Message { get => ev; }

        public override string ToString() => ev.ToString();
    }
}
