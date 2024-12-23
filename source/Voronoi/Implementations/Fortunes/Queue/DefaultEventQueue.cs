using Godot;
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
        public int Compare(IEvent p, IEvent q)
        {
            // Sort by Y first
            if (p.Y < q.Y) return -1;
            if (p.Y > q.Y) return 1;

            // Then by X
            if (p.X < q.X) return -1;
            if (p.X > q.X) return 1;

            // Then they gotta be equal!
            // Which we don't like >:)
            throw new InvalidOperationException();
        }
    }

    public DefaultEventQueue() : base(new EventComparer())
    { }

    public void Initialize(params Vector2[] sites) =>
        EnqueueRange(sites.Select(e =>
        {
            IEvent siteEvent = new SiteEvent(e);
            return (siteEvent, siteEvent);
        }));

    public void Enqueue(IEvent @event) =>
        Enqueue(@event);
}
