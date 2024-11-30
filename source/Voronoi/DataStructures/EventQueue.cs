using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PointQueueTests")]
internal class EventQueue
{
    private readonly PriorityQueue<IEvent, IEvent> _events = new(new EventComparer());

    public int Count => _events.Count;

    public void Enqueue(IEvent e)
    {
        _events.Enqueue(e, e);
    }

    public IEvent Dequeue()
    {
        return _events.Dequeue();
    }
}
