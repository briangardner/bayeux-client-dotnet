using System;
using System.Threading.Tasks;
using Moq.Language;

namespace FinancialHq.Bayeux.Tests.Unit
{
    public static class MoqExtensions
    {
        public static ISetupSequentialResult<Task<T>> ReturnsIndefinitely<T>(this ISetupSequentialResult<Task<T>> setup, Func<Task<T>> valueFunction)
        {
            for (var i = 0; i < 100; i++)
                setup.Returns(valueFunction);

            return setup;
        }
    }
}
