using System.Threading;

namespace FinancialHq.Bayeux.Client.Util
{
    class BooleanLatch
    {
        int closed = 0;

        public bool AlreadyRun()
        {
            var oldClosed = Interlocked.Exchange(ref closed, 1);
            return oldClosed == 1;
        }
    }
}
