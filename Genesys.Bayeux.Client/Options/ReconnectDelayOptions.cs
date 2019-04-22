﻿using System;
using System.Collections.Generic;

namespace Genesys.Bayeux.Client.Options
{
    public class ReconnectDelayOptions
    {
        public ReconnectDelayOptions(IEnumerable<TimeSpan> delays)
        {
            ReconnectDelays = delays ?? throw new ArgumentNullException(nameof(delays));
        }
        public IEnumerable<TimeSpan> ReconnectDelays { get; set; }
    }
}
