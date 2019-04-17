using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Extensions.ReplayId.Builders
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
