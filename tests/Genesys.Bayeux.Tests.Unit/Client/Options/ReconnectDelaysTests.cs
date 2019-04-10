using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Options;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Options
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
