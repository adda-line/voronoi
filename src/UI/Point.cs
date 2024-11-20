using Godot;

public partial class Point : Area2D
{
    private const float POINT_RADIUS = 3.0f;

    /// <inheritdoc/>
    public override void _Draw()
    {
        // TODO: Looking at canvasPos while debugging, why is it (0,0)?
        var canvasPos = MakeCanvasPositionLocal(Position);
        DrawCircle(canvasPos, POINT_RADIUS, Colors.Red);
        DrawString(ThemeDB.FallbackFont, canvasPos, $"({(int)GlobalPosition.X},{(int)GlobalPosition.Y})");
    }
}
