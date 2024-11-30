using System.Collections.Generic;

public interface IEventQueue
{
    int Count { get; }

    void Enqueue(IEvent e);

    IEvent Dequeue();

    void EnqueueRange(IEnumerable<IEvent> events);
}