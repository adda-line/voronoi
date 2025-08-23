internal class CircleEvent : IEvent
{
    public float X { get; internal set; }
    public float Y { get; internal set; }

    public Arc DisappearingArc { get; internal set; }
}
