using Godot;

public partial class Draggable<T> : Node2D
    where T : Area2D
{
    private const float MOVE_SPEED = 50f;

    internal bool _mouseIsOver = false;
    internal bool _isHeld = false;

    public T Value { get; private set; }

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
            Vector2 newPosition = GlobalPosition.Lerp(GetGlobalMousePosition(), (float)(delta * MOVE_SPEED));
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
        if (@event is not InputEventMouseButton mb)
            return;

        if (mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.IsPressed() && _mouseIsOver)
                _isHeld = true;
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
