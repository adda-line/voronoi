using Godot;
using System.ComponentModel;

public partial class Draggable<T> : Node2D
    where T : CollisionObject2D, IDeepCloneable<T>
{
    private const float MOVE_SPEED = 50f;

    private Node2D _ghost;

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
        Value.MouseEntered += Value_MouseEntered;
        Value.MouseExited += Value_MouseExited;

        // If we're operating on something that will notify us, let's match it's ghost real-time!
        if (Value is INotifyPropertyChanged changeNotifier)
            changeNotifier.PropertyChanged += Value_PropertyChanged;

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

        // Handle drag cancels
        if (_isHeld && @event.IsActionPressed("drag_cancel"))
        {
            _isHeld = false;
            Position = _ghost.Position;
            RemoveGhost();
        }

        if (@event is not InputEventMouseButton mb)
            return;

        // Now figure out if we're holding the thing.
        if (mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.IsPressed() && _mouseIsOver)
            {
                _isHeld = true;

                // This will be the "ghost" left behind when we start to drag something.
                _ghost = MakeGhostFrom(Value);
                _ghost.Position = Position;
                AddChild(_ghost);

                // Don't want this to propagate to overzealous controls
                // *cough* *cough* _PointPlacer_
                GetViewport().SetInputAsHandled();
            }
            else if (mb.IsReleased())
            {
                _isHeld = false;
                RemoveGhost();
            }
        }
    }

    /// <summary>
    /// Removes <see cref="_ghost"/> from the tree, frees it, and sets <see cref="_ghost"/> to null.
    /// </summary>
    private void RemoveGhost()
    {
        // Nothing to see here *whistling intensifies*
        if (_ghost == null)
            return;

        RemoveChild(_ghost);
        _ghost.QueueFree();
        _ghost = null;
    }

    /// <summary>
    /// Handler for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event on <see cref="Value"/>.
    /// Updates the original position <see cref="_ghost"/> if it exists.
    /// </summary>
    /// <param name="sender">The thing that updated - specifically <see cref="Value"/>.</param>
    /// <param name="e">What changed.</param>
    private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_ghost == null)
            return;

        // We have to make a new ghost that matches Value
        // but at the old position of _ghost.
        var updatedGhost = MakeGhostFrom(Value);
        updatedGhost.Position = _ghost.Position;

        // Flip it and replace it.
        _ghost.ReplaceBy(updatedGhost);
        _ghost.QueueFree();
        _ghost = updatedGhost;
    }

    /// <summary>
    /// Handler for the <see cref="CollisionObject2D.MouseEntered"/> event on <see cref="Value"/>.
    /// </summary>
    private void Value_MouseEntered()
    {
        _mouseIsOver = true;
    }

    /// <summary>
    /// Handler for the <see cref="CollisionObject2D.MouseExited"/> event on <see cref="Value"/>.
    /// </summary>
    private void Value_MouseExited()
    {
        _mouseIsOver = false;
    }

    /// <summary>
    /// Creates the ghost visage of <paramref name="cloneable"/>.
    /// </summary>
    /// <returns>The cloned node.</returns>
    private static Node2D MakeGhostFrom(IDeepCloneable<T> cloneable)
    {
        var ghost = cloneable.DeepClone();
        ghost.SelfModulate = new Color(1, 1, 1, 0.5f);
        ghost.Visible = true;
        ghost.TopLevel = true;
        return ghost;
    }
}
