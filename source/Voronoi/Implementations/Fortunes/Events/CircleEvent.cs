internal class CircleEvent : IEvent
{
    public int X { get; internal set; }
    public int Y { get; internal set; }

    public Arc DisappearingArc { get; internal set; }
}
