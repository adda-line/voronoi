using Godot;
using System.Xml.Serialization;

public partial class Draggable<T> : Node2D
    where T : Area2D, IDeepCloneable<T>
{
    private const float MOVE_SPEED = 50f;

    private Area2D _ghost;

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

        // This will be the "ghost" left behind when we start to drag something.
        _ghost = Value.DeepClone();
        _ghost.SelfModulate = new Color(1, 1, 1, 0.5f);
        _ghost.Visible = false;
        _ghost.TopLevel = true;
        AddChild(_ghost);
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        if (_isHeld)
        {
            var mousePos = GetGlobalMousePosition();

            Vector2 newPosition;
            if (_snapToGrid && Grid != null)
                newPosition = mousePos.Snapped(Grid.Basis);
            else
                newPosition = GlobalPosition.Lerp(GetGlobalMousePosition(), (float)(delta * MOVE_SPEED));

            // Let's not queue a redraw if we can help it.
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

                // Show the ghost where we're dragging from
                _ghost.Position = Position;
                _ghost.Visible = true;

                // Don't want this to propagate to overzealous controls
                // *cough* *cough* _PointPlacer_
                GetViewport().SetInputAsHandled();
            }
            else if (mb.IsReleased())
            {
                _isHeld = false;

                // Hide the ghost, it is no longer needed.
                _ghost.Visible = false;
            }
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
