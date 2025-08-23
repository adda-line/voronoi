using Godot;

internal class SiteEvent : IEvent
{
    internal Vector2 Position;
    internal Face Face;

    public float X => Position.X;
    public float Y => Position.Y;

    public SiteEvent(Vector2 p)
    {
        Position = p;
    }
}
