using Godot;

[GlobalClass]
[Tool]
public partial class Grid : Control
{
    private const int DefaultGridSize = 25;

    private Vector2 _basis = new(DefaultGridSize, DefaultGridSize);
    private bool _linesVisible = true;
    private Color _lineColor = Colors.DarkGray;
    private int _lineThickness = 2;

    [Export(PropertyHint.Link, "5,100,.5")]
    public Vector2 Basis
    {
        get => _basis;
        set
        {
            _basis = value;
            QueueRedraw();
        }
    }

    [ExportSubgroup("Grid Lines")]
    [Export]
    public bool IsVisible
    {
        get => _linesVisible;
        set
        {
            _linesVisible = value;
            QueueRedraw();
        }
    }

    [Export(PropertyHint.ColorNoAlpha)]
    public Color Color
    {
        get => _lineColor;
        set
        {
            _lineColor = value;
            QueueRedraw();
        }
    }

    [Export(PropertyHint.Range, "1,5,1")]
    public int Thickness
    {
        get => _lineThickness;
        set
        {
            _lineThickness = value;
            QueueRedraw();
        }
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_linesVisible)
        {
            int verticalLinesToPlace = (int)(Size.X / _basis.X) + 1;
            for (int i = 0;  i < verticalLinesToPlace; i++)
            {
                var from = new Vector2(i * _basis.X, 0);
                var to = new Vector2(i * _basis.X, Size.Y);
                DrawLine(from, to, Color, _lineThickness);
            }

            int horizontalLinesToPlace = (int)(Size.Y / _basis.Y) + 1;
            for (int i = 0; i < horizontalLinesToPlace; i++)
            {
                var from = new Vector2(0, i * _basis.Y);
                var to = new Vector2(Size.X, i * _basis.Y);
                DrawLine(from, to, Color, _lineThickness);
            }
        }
    }
}
