﻿using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Extensions
{
    public static class BayeuxMessageReplayExtensions 
    {

        public static long GetReplayId(this BayeuxMessage message)
        {
            message.TryGetValue(MessageFields.ReplayIdField, out var obj);
            if (obj != null)
            {
                return (long) obj;
            }

            return 0;
        }

        // ReSharper disable once UnusedMember.Global
        public static void SetReplayId(this BayeuxMessage message, long replayId)
        {
            message[MessageFields.ReplayIdField] = replayId;
        }

    }
}
