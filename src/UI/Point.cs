using Godot;

public partial class Point : Area2D
{
    private const float POINT_RADIUS = 3.0f;

    /// <inheritdoc/>
    public override void _Draw()
    {
        // We're drawing in local coords, NOT the containing canvas' coords.
        DrawCircle(Vector2.Zero, POINT_RADIUS, Colors.Red);
        this.DrawCenteredString(ThemeDB.FallbackFont, new Vector2(0, -20), $"({(int)GlobalPosition.X}, {(int)GlobalPosition.Y})");
    }
}
