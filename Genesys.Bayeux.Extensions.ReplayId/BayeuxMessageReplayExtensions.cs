using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Extensions.ReplayId
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

        public static void SetReplayId(this BayeuxMessage message, long replayId)
        {
            message[MessageFields.ReplayIdField] = replayId;
        }

    }
}
