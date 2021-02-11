using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace CSN.Common.Core
{
    public static class Events
    {
        public static async Task<TEvent> FirstEventOf<TSource, TEvent>(CancellationToken token, Action<TypedEventHandler<TSource, TEvent>> subscribe, Action<TypedEventHandler<TSource, TEvent>> unsubscribe, int msTimeout, Predicate<TEvent> eventFilter = null)
        {
            var timeoutCTS = new CancellationTokenSource(msTimeout);
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCTS.Token, token);

            var tcs = new TaskCompletionSource<TEvent>();
            TypedEventHandler<TSource, TEvent> handler = (_, result) =>
            {
                if (eventFilter != null)
                {
                    if (eventFilter(result))
                    {
                        tcs.TrySetResult(result);
                    }
                }
                else
                {
                    tcs.TrySetResult(result);
                }
            };

            timeoutCTS.Token.Register(() => tcs.TrySetException(new TimeoutException()));
            var task = tcs.Task;
            subscribe(handler);
            try
            {
                return await task;
            }
            finally
            {
                unsubscribe(handler);
            }
        }

    }
}
