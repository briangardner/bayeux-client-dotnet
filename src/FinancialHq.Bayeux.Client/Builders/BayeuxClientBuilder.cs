using System;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Client.Builders
{
    public interface IBayeuxClientBuilder
    {
        IServiceCollection Services { get; }
    }

    public class BayeuxClientBuilder : IBayeuxClientBuilder
    {

        public BayeuxClientBuilder(IServiceCollection collection)
        {
            Services = collection ?? throw new ArgumentNullException(nameof(collection));
        }
        public IServiceCollection Services { get; }
    }
}