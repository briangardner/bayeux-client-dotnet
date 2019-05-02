using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Messaging
{
    public class BayeuxMessage : Dictionary<string, object>, IMessage
    {
        public BayeuxMessage(): base()
        {
            
        }
        public BayeuxMessage(IDictionary<string, object> message) : base(message)
        {
        }

        public IDictionary<string, object> Advice
        {
            get
            {
                TryGetValue(MessageFields.AdviceField, out var advice);
                if (!(advice is JObject))
                {
                    return (IDictionary<string, object>)advice;
                }

                advice = JsonConvert.DeserializeObject<IDictionary<string, object>>(advice.ToString());
                this[MessageFields.AdviceField] = advice;
                return (IDictionary<string, object>)advice;
            }
        }

        public string Channel
        {
            get
            {
                TryGetValue(MessageFields.ChannelField, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.ChannelField] = value;
        }

        public string Subscription
        {
            get
            {
                TryGetValue(MessageFields.SubscriptionField, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.SubscriptionField] = value;
        }
        public ChannelId ChannelId => new ChannelId(Channel);

        public string ClientId
        {
            get
            {
                TryGetValue(MessageFields.ClientIdField, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.ClientIdField] = value;
        }

        public object Data
        {
            get
            {
                TryGetValue(MessageFields.DataField, out var obj);
                return obj;
            }
            set => this[MessageFields.DataField] = value;
        }

        public IDictionary<string, object> DataAsDictionary
        {
            get
            {
                TryGetValue(MessageFields.DataField, out var data);
                if (!(data is string value))
                {
                    var obj = JObject.FromObject(data);
                    return obj.ToObject<Dictionary<string,object>>();
                }

                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
                this[MessageFields.DataField] = data;
                return (Dictionary<string, object>)data;
            }
        }

        public IDictionary<string, object> Ext
        {
            get
            {
                TryGetValue(MessageFields.ExtField, out var ext);
                if (ext is string value)
                {
                    ext = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
                    this[MessageFields.ExtField] = ext;
                }

                if (!(ext is JObject))
                {
                    return (Dictionary<string, object>)ext;
                }

                ext = JsonConvert.DeserializeObject<Dictionary<string, object>>(ext.ToString());
                this[MessageFields.ExtField] = ext;
                return (Dictionary<string, object>)ext;
            }
        }
        public string Id
        {
            get
            {
                TryGetValue(MessageFields.IdField, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.IdField] = value;
        }
        public string Json => JsonConvert.SerializeObject(this);

        public bool Meta => ChannelId.IsMeta(Channel);
        public bool Successful { get; set; }
        public IDictionary<string, object> GetAdvice(bool create)
        {
            var advice = Advice;
            if (!create || advice != null)
            {
                return advice;
            }

            advice = new Dictionary<string, object>();
            this[MessageFields.AdviceField] = advice;
            return advice;
        }

        public IDictionary<string, object> GetDataAsDictionary(bool create)
        {
            var data = DataAsDictionary;
            if (!create || data != null)
            {
                return data;
            }

            data = new Dictionary<string, object>();
            this[MessageFields.DataField] = data;
            return data;
        }

        public IDictionary<string, object> GetExt(bool create)
        {
            var ext = Ext;
            if (!create || ext != null)
            {
                return ext;
            }

            ext = new Dictionary<string, object>();
            this[MessageFields.ExtField] = ext;
            return ext;
        }

        public override string ToString()
        {
            return Json;
        }
    }
}
