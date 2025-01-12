using Godot;

internal class SiteEvent : IEvent
{
    internal Vector2 Position;
    internal Face Face;

    public int X => (int)Position.X;
    public int Y => (int)Position.Y;

    public SiteEvent(Vector2 p)
    {
        Position = p;
    }
}
