using Godot;
using System.Collections.Generic;

public interface IEventQueue
{
    int Count { get; }

    void Enqueue(IEvent @event);

    IEvent Dequeue();

    void Initialize(params Vector2[] sites);
}