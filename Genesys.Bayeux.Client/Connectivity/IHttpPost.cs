using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.Bayeux.Client.Connectivity
{
    public interface IHttpPost
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, string jsonContent, CancellationToken cancellationToken);
    }
}