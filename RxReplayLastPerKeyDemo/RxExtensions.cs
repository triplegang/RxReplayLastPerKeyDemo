using System;
using System.Reactive.Linq;
using System.Threading;

namespace RxReplayLastPerKeyDemo
{
    public static class RxExtensions
    {
        public static IDisposable SubscribeConsole<T>(this IObservable<T> observable, string name = "")
        {
            return observable.Subscribe(new ConsoleObserver<T>(name));
        }


        /// <summary>
        /// Logs to the Console the subscriptions and emissions done on/by the observable
        /// each log message also includes the thread it happens on
        /// </summary>
        /// <typeparam name="T">The Observable Type</typeparam>
        /// <param name="observable">The Observable to log.</param>
        /// <param name="name">An optional name prefix that will be added before each notification</param>
        /// <returns></returns>
        public static IObservable<T> LogConsoleWithThread<T>(this IObservable<T> observable, string name = "")
        {
            return Observable.Defer(() =>
            {
                Console.WriteLine("{0} Subscription happened on Thread: {1}", name, Thread.CurrentThread.ManagedThreadId);

                return observable.Do(
                    x => Console.WriteLine("{0} - OnNext({1}) Thread: {2}", name, x, Thread.CurrentThread.ManagedThreadId),
                    ex =>
                    {
                        Console.WriteLine("{0} - OnError Thread:{1}", name, Thread.CurrentThread.ManagedThreadId);
                        Console.WriteLine("\t {0}", ex);
                    },
                    () => Console.WriteLine("{0} - OnCompleted() Thread {1}", name, Thread.CurrentThread.ManagedThreadId));
            });
        }
    }
}
