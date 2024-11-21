using Godot;

public partial class Point : Area2D
{
    private float _radius;
    private Color _color;
    private bool _showCoords;

    public float Radius
    {
        get => _radius;
        set
        {
            // TODO: This feels hacky but I'm not sure what would be better....
            _radius = Mathf.Abs(value);
            ((CircleShape2D)GetChild<CollisionShape2D>(0, true).Shape).Radius = value;
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
        _radius = Mathf.Abs(radius);
        _color = color;
        _showCoords = showCoords;

        AddChild(new CollisionShape2D()
        {
            Shape = new CircleShape2D()
            {
                Radius = _radius
            }
        }, @internal: InternalMode.Front);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        // We're drawing in local coords, NOT the containing canvas' coords.
        DrawCircle(Vector2.Zero, _radius, _color);
        if (_showCoords)
        {
            this.DrawCenteredString(ThemeDB.FallbackFont, new Vector2(0, -(20 + _radius)), $"({(int)GlobalPosition.X}, {(int)GlobalPosition.Y})");
        }
    }
}
