using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PointQueueTests")]
public class DefaultEventQueue : PriorityQueue<IEvent, IEvent>, IEventQueue
{
    private class EventComparer : IComparer<IEvent>
    {
        /// <inheritdoc/>
        /// <remarks>TODO: Use an epsilon value.</remarks>
        public int Compare(IEvent x, IEvent y)
        {
            // Sort by Y first
            if (x.Position.Y < y.Position.Y) return -1;
            if (x.Position.Y > y.Position.Y) return 1;

            // Then by X
            if (x.Position.X < y.Position.X) return -1;
            if (x.Position.X > y.Position.X) return 1;

            // Then they gotta be equal!
            // Which we don't like >:)
            throw new InvalidOperationException();
        }
    }

    public DefaultEventQueue() : base(new EventComparer())
    { }

    public void Enqueue(IEvent e) => Enqueue(e, e);

    public void EnqueueRange(IEnumerable<IEvent> events) =>
        EnqueueRange(events.Select(e => (e, e)));
}
