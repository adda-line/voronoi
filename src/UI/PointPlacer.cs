using Godot;

/// <summary>
/// Places points in the containing canvas layer.
/// </summary>
public partial class PointPlacer : Control
{
    /// <inheritdoc/>
    /// <remarks>Adds <see cref="Point"/> to bve drawn to the containing canvas.</remarks>
    public override void _Input(InputEvent @event)
    {
        // Shhh we're not here
        if (!Visible)
            return;

        // If we've been clicked let's plop a point!
        if (@event is InputEventMouseButton mb &&
            mb.Pressed &&
            mb.ButtonIndex == MouseButton.Left)
        {
            // Mouse position is in Viewport coords - move into canvas
            var canvasPosition = GetCanvasTransform() * mb.Position;
            var site = new Point
            {
                Position = canvasPosition
            };
            AddChild(site, @internal: InternalMode.Front);
            GetViewport().SetInputAsHandled();
        }
    }
}
