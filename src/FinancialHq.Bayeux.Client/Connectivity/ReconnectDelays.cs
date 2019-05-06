using System;
using System.Collections.Generic;

namespace FinancialHq.Bayeux.Client.Connectivity
{
    internal class ReconnectDelays
    {
        readonly IEnumerable<TimeSpan> delays;

        IEnumerator<TimeSpan> currentDelaysEnumerator;
        TimeSpan currentDelay;
        bool lastSucceeded = true;


        public ReconnectDelays(IEnumerable<TimeSpan> delays)
        {
            this.delays = delays ??
                          new List<TimeSpan> { TimeSpan.Zero, TimeSpan.FromSeconds(5) };
        }

        public void ResetIfLastSucceeded()
        {
            if (lastSucceeded)
                currentDelaysEnumerator = delays.GetEnumerator();

            lastSucceeded = true;
        }

        public TimeSpan GetNext()
        {
            lastSucceeded = false;

            if (currentDelaysEnumerator.MoveNext())
                currentDelay = currentDelaysEnumerator.Current;

            return currentDelay;
        }
    }
}
