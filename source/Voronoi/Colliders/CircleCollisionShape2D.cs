using Godot;

/// <summary>
/// Just a <see cref="CollisionShape2D"/> node that has a <see cref="CircleShape2D"/> shape.
/// </summary>
public partial class CircleCollisionShape2D : CollisionShape2D
{
    private readonly CircleShape2D _circleShape;

    private float _radius;

    /// <summary>
    /// Radius of the internal <see cref="CircleShape2D"/>.
    /// </summary>
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            _circleShape.Radius = value;
        }
    }

    public CircleCollisionShape2D(float radius)
    {
        _radius = radius;
        Shape = _circleShape = new CircleShape2D()
        {
            Radius = _radius
        };
    }
}
