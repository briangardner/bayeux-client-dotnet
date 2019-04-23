using System;
using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Extensions.Ack.Logging;

namespace Genesys.Bayeux.Extensions.Ack
{
    public class AckExtension : IExtension
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private const string ExtensionField = "ack";
        private volatile bool _serverSupportsAcks;
        private volatile int _ackId = -1;
        public bool Receive(BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(BayeuxMessage message)
        {
            Log.Debug("Ack Extension - Receive Meta start");
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
            Log.Debug("Ack Extension - Receive Meta done");
            return true;
        }

        public bool Send(BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta( BayeuxMessage message)
        {
            Log.Debug("Ack Extension - Send Meta start");
            if (ChannelFields.META_HANDSHAKE.Equals(message.Channel))
            {
                message.GetExt(true)[ExtensionField] = true;
                _ackId = -1;
            }
            else if (_serverSupportsAcks && ChannelFields.META_CONNECT.Equals(message.Channel))
            {
                message.GetExt(true)[ExtensionField] = _ackId;
            }
            Log.Debug("Ack Extension - Send Meta end");
            return true;
        }
    }
}
