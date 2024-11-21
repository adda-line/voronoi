using Godot;

public partial class Draggable<T> : Node2D
    where T : Area2D
{
    private const float MOVE_SPEED = 50f;

    private bool _mouseIsOver = false;
    private bool _isHeld = false;
    private bool _snapToGrid = false;

    public T Value { get; private set; }

    public Grid Grid { get; set; }

    public Draggable(T item)
    {
        Value = item;
    }

    /// <inheritdoc/>
    public override void _Ready()
    {
        Value.MouseEntered += Item_MouseEntered;
        Value.MouseExited += Item_MouseExited;
        AddChild(Value);
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        if (_isHeld)
        {
            var mousePos = GetGlobalMousePosition();

            Vector2 newPosition;
            if (_snapToGrid && Grid != null)
            {
                // TODO: the grid should have configurable X and Y components
                var step = new Vector2(Grid.StepSize, Grid.StepSize);
                newPosition = mousePos.Snapped(step);
            }
            else
            {
                newPosition = GlobalPosition.Lerp(GetGlobalMousePosition(), (float)(delta * MOVE_SPEED));
            }
            if (newPosition.X > float.Epsilon || newPosition.Y > float.Epsilon)
            {
                GlobalPosition = newPosition;
                QueueRedraw();
            }
        }
    }

    /// <inheritdoc/>
    public override void _Input(InputEvent @event)
    {
        if (@event.IsEcho())
            return;

        // Handle grid snapping
        if (@event.IsActionPressed("drag_snap_to_grid"))
            _snapToGrid = true;
        if (@event.IsActionReleased("drag_snap_to_grid"))
            _snapToGrid = false;

        if (@event is not InputEventMouseButton mb)
            return;

        // Now figure out if we're holding the thing.
        if (mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.IsPressed() && _mouseIsOver)
            {
                _isHeld = true;

                // Don't want this to propagate to overzealous controls
                // *cough* *cough* _PointPlacer_
                GetViewport().SetInputAsHandled();
            }
            else if (mb.IsReleased())
                _isHeld = false;
        }
    }

    private void Item_MouseEntered()
    {
        _mouseIsOver = true;
    }

    private void Item_MouseExited()
    {
        _mouseIsOver = false;
    }
}
