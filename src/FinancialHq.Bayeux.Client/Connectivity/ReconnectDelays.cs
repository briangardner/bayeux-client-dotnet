using System;
using System.Collections.Generic;

namespace FinancialHq.Bayeux.Client.Connectivity
{
    internal class ReconnectDelays
    {
        private readonly IEnumerable<TimeSpan> _delays;

        private IEnumerator<TimeSpan> _currentDelaysEnumerator;
        private TimeSpan _currentDelay;
        private bool _lastSucceeded = true;


        public ReconnectDelays(IEnumerable<TimeSpan> delays)
        {
            _delays = delays ??
                          new List<TimeSpan> { TimeSpan.Zero, TimeSpan.FromSeconds(5) };
        }

        public void ResetIfLastSucceeded()
        {
            if (_lastSucceeded)
                _currentDelaysEnumerator = _delays.GetEnumerator();

            _lastSucceeded = true;
        }

        public TimeSpan GetNext()
        {
            _lastSucceeded = false;

            if (_currentDelaysEnumerator.MoveNext())
                _currentDelay = _currentDelaysEnumerator.Current;

            return _currentDelay;
        }
    }
}
