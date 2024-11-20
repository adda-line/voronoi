using Godot;

/// <summary>
/// Places points in the containing canvas layer.
/// </summary>
public partial class PointPlacer : Node2D
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
            var site = new Point
            {
                Position = mb.Position
            };
            AddChild(site, @internal: InternalMode.Front);
            GetViewport().SetInputAsHandled();
        }
    }
}
