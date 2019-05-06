using System;
using FinancialHq.Bayeux.Client.Options;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Options
{
    public class ReconnectDelaysTests
    {
        [Fact]
        public void Should_Throw_ArgumentNullException_If_Delays_Null()
        {
            Assert.Throws<ArgumentNullException>( () => new ReconnectDelayOptions(null));
        }
    }
}
