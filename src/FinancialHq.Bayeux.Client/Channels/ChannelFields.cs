﻿namespace FinancialHq.Bayeux.Client.Channels
{
    public static class ChannelFields
    {
        public const string Meta = "/meta";
        public const string MetaConnect = Meta + "/connect";
        // ReSharper disable once UnusedMember.Global
        public const string MetaDisconnect = Meta + "/disconnect";
        public const string MetaHandshake = Meta + "/handshake";
        public const string MetaSubscribe = Meta + "/subscribe";
        public const string MetaUnsubscribe = Meta + "/unsubscribe";
    }
}