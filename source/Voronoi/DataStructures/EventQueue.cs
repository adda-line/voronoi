using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PointQueueTests")]
internal class EventQueue
{
    public int Count => throw new NotImplementedException();

    public void Enqueue(IEvent _)
    {
        throw new NotImplementedException();
    }

    public IEvent Dequeue()
    {
        throw new NotImplementedException();
    }
}
