using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Messaging
{
    public abstract class MessageExtension : Dictionary<string, object>, IMessage
    {
        private IMutableMessage _message;

        protected MessageExtension(IMutableMessage message)
        {
            _message = message;
        }

        public IDictionary<string, object> Advice
        {
            get
            {
                TryGetValue(MessageFields.ADVICE_FIELD, out var advice);
                if (!(advice is JObject))
                {
                    return (IDictionary<string, object>)advice;
                }

                advice = JsonConvert.DeserializeObject<IDictionary<string, object>>(advice.ToString());
                this[MessageFields.ADVICE_FIELD] = advice;
                return (IDictionary<string, object>)advice;
            }
        }

        public string Channel
        {
            get
            {
                TryGetValue(MessageFields.CHANNEL_FIELD, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.CHANNEL_FIELD] = value;
        }

        public string Subscription
        {
            get
            {
                TryGetValue(MessageFields.SUBSCRIPTION_FIELD, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.SUBSCRIPTION_FIELD] = value;
        }
        public ChannelId ChannelId => new ChannelId(Channel);

        public string ClientId
        {
            get
            {
                TryGetValue(MessageFields.CLIENT_ID_FIELD, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.CLIENT_ID_FIELD] = value;
        }

        public object Data
        {
            get
            {
                TryGetValue(MessageFields.DATA_FIELD, out var obj);
                return obj;
            }
            set => this[MessageFields.DATA_FIELD] = value;
        }

        public IDictionary<string, object> DataAsDictionary
        {
            get
            {
                TryGetValue(MessageFields.DATA_FIELD, out var data);
                if (!(data is string value))
                {
                    return (Dictionary<string, object>)data;
                }

                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
                this[MessageFields.DATA_FIELD] = data;
                return (Dictionary<string, object>)data;
            }
        }

        public IDictionary<string, object> Ext
        {
            get
            {
                TryGetValue(MessageFields.EXT_FIELD, out var ext);
                if (ext is string value)
                {
                    ext = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
                    this[MessageFields.EXT_FIELD] = ext;
                }

                if (!(ext is JObject))
                {
                    return (Dictionary<string, object>)ext;
                }

                ext = JsonConvert.DeserializeObject<Dictionary<string, object>>(ext.ToString());
                this[MessageFields.EXT_FIELD] = ext;
                return (Dictionary<string, object>)ext;
            }
        }
        public string Id
        {
            get
            {
                TryGetValue(MessageFields.ID_FIELD, out var obj);
                return (string)obj;
            }
            set => this[MessageFields.ID_FIELD] = value;
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
            this[MessageFields.ADVICE_FIELD] = advice;
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
            this[MessageFields.DATA_FIELD] = data;
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
            this[MessageFields.EXT_FIELD] = ext;
            return ext;
        }

        public override string ToString()
        {
            return Json;
        }
    }
}
