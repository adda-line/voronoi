using Godot;

internal class SiteEvent : IEvent
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public SiteEvent(Vector2 p) : this((int)p.X, (int)p.Y) { }

    public SiteEvent(int x, int y)
    {
        X = x;
        Y = y;
    }
}
