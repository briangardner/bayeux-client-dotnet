using System.Collections.Generic;

namespace Genesys.Bayeux.Client.Messaging
{
    public interface IMutableMessage : IMessage
    {
        new string Channel { get; set; }

        new string Subscription { get; set; }

        new string ClientId { get; set; }

        new object Data { get; set; }

        new string Id { get; set; }

        new bool Successful { get; set; }

        /// <summary> Convenience method to retrieve the {@link #ADVICE_FIELD} and create it if it does not exist</summary>
        /// <param name="create">whether to create the advice field if it does not exist
        /// </param>
        /// <returns> the advice of the message
        /// </returns>
        IDictionary<string, object> GetAdvice(bool create);

        /// <summary> Convenience method to retrieve the {@link #DATA_FIELD} and create it if it does not exist</summary>
        /// <param name="create">whether to create the data field if it does not exist
        /// </param>
        /// <returns> the data of the message
        /// </returns>
        IDictionary<string, object> GetDataAsDictionary(bool create);

        /// <summary> Convenience method to retrieve the {@link #EXT_FIELD} and create it if it does not exist</summary>
        /// <param name="create">whether to create the ext field if it does not exist
        /// </param>
        /// <returns> the ext of the message
        /// </returns>
        IDictionary<string, object> GetExt(bool create);
    }
}