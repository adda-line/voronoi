using Godot;

public partial class Point : Area2D
{
    private readonly CircleCollisionShape2D _collider;

    private Color _color;
    private bool _showCoords;

    public float Radius
    {
        get => _collider.Radius;
        set
        {
            _collider.Radius = Mathf.Abs(value);
            QueueRedraw();
        }
    }

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            QueueRedraw();
        }
    }

    public bool ShowCoords
    {
        get => _showCoords;
        set
        {
            _showCoords = value;
            QueueRedraw();
        }
    }

    public Point(float radius, Color color, bool showCoords)
    {
        _color = color;
        _showCoords = showCoords;

        _collider = new CircleCollisionShape2D(Mathf.Abs(radius));
        AddChild(_collider, @internal: InternalMode.Front);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        // We're drawing in local coords, NOT the containing canvas' coords.
        DrawCircle(Vector2.Zero, _collider.Radius, _color);
        if (_showCoords)
        {
            this.DrawCenteredString(ThemeDB.FallbackFont, new Vector2(0, -(20 + _collider.Radius)), $"({(int)GlobalPosition.X}, {(int)GlobalPosition.Y})");
        }
    }
}
