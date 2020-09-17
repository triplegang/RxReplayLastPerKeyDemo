using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace RxReplayLastPerKeyDemo
{
    public class MockDataService
    {

        private readonly IConnectableObservable<Item> _changeStream;
        private IDisposable _changeSubscription;

        private readonly IObservable<Item> _replayChangeStream;

        public MockDataService()
        {

            // Simulate Http response pipeline.
            IObservable<List<Item>> responseStream = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(50)
                .LogConsoleWithThread("Interval")
                .Select(tick =>
                {
                    // New Item appears on the stream every 5 ticks.
                    // Items last for 10 ticks. Item Value has range (0-9)
                    int rangeStart = Math.Max(((int)tick / 5) -1, 0);
                    int rangeCount = Math.Min(((int)tick / 5) + 1, 2);
                    return Enumerable.Range(rangeStart, rangeCount).Select(id => new Item(id, (int)tick - (id * 5))).ToList();
                });

            // Flatten the list into IObservable<Item>
            var itemStream = responseStream
                .SelectMany(list => list)
                .LogConsoleWithThread("ItemStream");

            // Split into groups by Item.Id and process each group for changes
            // ChangeStream is an IObservable<Item> of changes.
            _changeStream = itemStream
                .GroupBy(item => item.Id)
                    .SelectMany(grp =>
                        grp
                        // Pipeline for each group.
                        .StartWith(new Item(grp.Key, -1))
                        .TakeUntil(item => item.Value >= 9)
                        .LogConsoleWithThread($"Group: {grp.Key}")
                        .Buffer(2, 1)
                        .Where(buffer => buffer.Count == 2 && buffer[0].HasChanges(buffer[1]))
                        .Select(buffer => buffer[1])
                        .LogConsoleWithThread($"Group.Change : {grp.Key}")
                        )
                .Publish();

            _replayChangeStream = _changeStream
                .Multicast(new ReplayLastPerKeySubject<Item, int>(x => x.Id))
                .RefCount();
        }

        public IObservable<Item> GetItemStream(int itemId)
        {
            return _replayChangeStream.Where(item => item.Id == itemId);
        }

        public void Start()
        {
            _changeSubscription = _changeStream.Connect();
        }

        public void Stop()
        {
            _changeSubscription.Dispose();
        }


    }
}
