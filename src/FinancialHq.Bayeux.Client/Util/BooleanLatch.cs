using System.Threading;

namespace FinancialHq.Bayeux.Client.Util
{
    class BooleanLatch
    {
        private int _closed;

        public bool AlreadyRun()
        {
            var oldClosed = Interlocked.Exchange(ref _closed, 1);
            return oldClosed == 1;
        }
    }
}
