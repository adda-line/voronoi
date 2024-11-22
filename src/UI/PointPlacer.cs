using Godot;
using System.Collections.Generic;

/// <summary>
/// Places points in the containing canvas layer.
/// </summary>
[GlobalClass]
public partial class PointPlacer : Control
{
    private float _pointRadius = 2.5f;
    private Color _pointColor = Colors.Black;
    private bool _showCoords = true;

    private Grid _grid = null;

    [ExportCategory("Point Settings")]
    [Export(PropertyHint.Range, "1,10,0.1")]
    public float PointRadius
    {
        get => _pointRadius;
        set
        {
            _pointRadius = value;
            foreach (var point in GetPoints())
            {
                point.Radius = _pointRadius;
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
            foreach (var point in GetPoints())
            {
                point.Color = _pointColor;
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
            foreach (var point in GetPoints())
            {
                point.ShowCoords = _showCoords;
            }
        }
    }

    [Export]
    public Grid Grid
    {
        get => _grid;
        set
        {
            _grid = value;
            for (int i = 0; i < GetChildCount(true); i++)
            {
                GetChild<Draggable<Point>>(i, true).Grid = _grid;
            }
        }
    }

    public event CollisionObjectMovedEventHandler<Point> PointMoved = delegate { };

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
                Position = canvasPosition,
                Grid = _grid
            };
            site.Moved += obj => PointMoved(obj);
            AddChild(site, @internal: InternalMode.Front);
            GetViewport().SetInputAsHandled();
        }
    }

    private IEnumerable<Point> GetPoints()
    {
        for (int i = 0; i < GetChildCount(true); ++i)
        {
            var child = GetChild(i, true);
            if (child is Draggable<Point> dragPoint)
                yield return dragPoint.Value;
        }
    }
}
