using System.Collections.Generic;

internal class DiagramGenerator<TQ>
    where TQ : IEventQueue, new()
{
    protected TQ _eventQueue;

    public DiagramGenerator(IEnumerable<IEvent> events)
    {
        _eventQueue = new();
        _eventQueue.EnqueueRange(events);
    }
}

internal class DiagramGenerator : DiagramGenerator<DefaultEventQueue>
{
    public DiagramGenerator(IEnumerable<IEvent> events) : base(events) { }
}
