using System;
using System.Threading.Tasks;

namespace FinancialHq.Bayeux.Client
{
    public interface IUnsubscribe<out T> where T : class
    {
        Task UnsubscribeAsync(IObserver<T> observer);
    }
}