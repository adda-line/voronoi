internal class CircleEvent : IEvent
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Arc DisappearingArc { get; private set; }
}
