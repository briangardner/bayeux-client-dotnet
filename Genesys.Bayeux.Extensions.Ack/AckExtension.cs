using System;
using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Extensions.Ack
{
    public class AckExtension : IExtension
    {
        private const string ExtensionField = "ack";

        private volatile bool _serverSupportsAcks;
        private volatile int _ackId = -1;
        public bool Receive(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(AbstractChannel channel, BayeuxMessage message)
        {
            if (ChannelFields.META_HANDSHAKE.Equals(message.Channel))
            {
                var ext = (Dictionary<string, object>)message.GetExt(false);
                _serverSupportsAcks = ext != null && true.Equals(ext[ExtensionField]);
            }
            else if (_serverSupportsAcks && true.Equals(message[MessageFields.SuccessfulField]) && ChannelFields.META_CONNECT.Equals(message.Channel))
            {
                var ext = (Dictionary<string, object>)message.GetExt(false);
                if (ext == null)
                {
                    return true;
                }

                ext.TryGetValue(ExtensionField, out var ack);
                try
                {
                    _ackId = Convert.ToInt32(ack);
                }
                catch (Exception)
                {
                    _ackId = default(int);
                }
            }

            return true;
        }

        public bool Send(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta(AbstractChannel channel, BayeuxMessage message)
        {
            if (ChannelFields.META_HANDSHAKE.Equals(message.Channel))
            {
                message.GetExt(true)[ExtensionField] = true;
                _ackId = -1;
            }
            else if (_serverSupportsAcks && ChannelFields.META_CONNECT.Equals(message.Channel))
            {
                message.GetExt(true)[ExtensionField] = _ackId;
            }

            return true;
        }
    }
}
