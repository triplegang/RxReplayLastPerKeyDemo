using System;

namespace RxReplayLastPerKeyDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            var dataService = new MockDataService();

            Console.WriteLine("Press Any Key to Start");
            Console.ReadLine();

            // Demo to show the OnCompleted non-propagation Bug.
            // When the Item.Id=0 group terminates with an OnCompleted
            // neither the ReplayLastPerKeySubject or the subscribers below receive the OnCompleted.

            // Test 2 early subscribers
            var item0Sub1 = dataService.GetItemStream(0)
                .SubscribeConsole("Item 0 Subscriber 1:");
            var item0Sub2 = dataService.GetItemStream(0)
                .SubscribeConsole("Item 0 Subscriber 2:");

            dataService.Start();

            // Subscribe late to Item2 (appears on Stream after 10 seconds, wait 12 seconds)
            //Thread.Sleep(12000);
            //var item2Sub = dataService.GetItemStream(2)
            //    .SubscribeConsole("Item 2 Subscriber 1");

            Console.WriteLine("Press Any Key to Stop");
            Console.ReadLine();

            dataService.Stop();

            item0Sub1.Dispose();
            item0Sub2.Dispose();
            //item2Sub.Dispose();
        }
    }
}
