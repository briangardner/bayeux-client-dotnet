using System;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Builders
{
    public interface IReplayIdExtensionBuilder
    {
        IServiceCollection Services { get; }
    }
    public class ReplayIdExtensionBuilder : IReplayIdExtensionBuilder
    {
        public ReplayIdExtensionBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
        public IServiceCollection Services { get; }
    }
}
