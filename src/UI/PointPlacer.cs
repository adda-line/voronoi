using Godot;

/// <summary>
/// Places points in the containing canvas layer.
/// </summary>
public partial class PointPlacer : Control
{
    private float _pointRadius = 2.5f;
    private Color _pointColor = Colors.Black;
    private bool _showCoords = true;

    [ExportCategory("Point Settings")]
    [Export(PropertyHint.Range, "1,10,0.1")]
    public float PointRadius
    {
        get => _pointRadius;
        set
        {
            _pointRadius = value;
            for (int i = 0; i < GetChildCount(true); i++)
            {
                GetChild<Draggable<Point>>(i, true).Value.Radius = _pointRadius;
            }
        }
    }

    [Export(PropertyHint.ColorNoAlpha)]
    public Color PointColor
    {
        get => _pointColor;
        set
        {
            _pointColor = value;
            for (int i = 0; i < GetChildCount(true); i++)
            {
                GetChild<Draggable<Point>>(i, true).Value.Color = _pointColor;
            }
        }
    }

    [Export]
    public bool ShowCoords
    {
        get => _showCoords;
        set
        {
            _showCoords = value;
            for (int i = 0; i < GetChildCount(true); i++)
            {
                GetChild<Draggable<Point>>(i, true).Value.ShowCoords = _showCoords;
            }
        }
    }

    /// <inheritdoc/>
    /// <remarks>Adds <see cref="Point"/> to be drawn to the containing canvas.</remarks>
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
            var site = new Draggable<Point>(new Point(PointRadius, PointColor, ShowCoords))
            {
                Position = canvasPosition
            };
            AddChild(site, @internal: InternalMode.Front);
            GetViewport().SetInputAsHandled();
        }
    }
}
