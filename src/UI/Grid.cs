using Godot;

[GlobalClass]
[Tool]
public partial class Grid : Control
{
    private const int DefaultGridSize = 25;

    private Vector2 _stepSize = new(DefaultGridSize, DefaultGridSize);
    private bool _showGrid = true;
    private Color _lineColor = Colors.DarkGray;
    private int _thickness = 2;

    [Export(PropertyHint.Link, "5,100,.5")]
    public Vector2 StepSize
    {
        get => _stepSize;
        set
        {
            _stepSize = value;
            QueueRedraw();
        }
    }

    [ExportSubgroup("Grid Lines")]
    [Export]
    public bool ShowGrid
    {
        get => _showGrid;
        set
        {
            _showGrid = value;
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
    public int LineThickness
    {
        get => _thickness;
        set
        {
            _thickness = value;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_showGrid)
        {
            int verticalLinesToPlace = (int)(Size.X / _stepSize.X) + 1;
            for (int i = 0;  i < verticalLinesToPlace; i++)
            {
                var from = new Vector2(i * _stepSize.X, 0);
                var to = new Vector2(i * _stepSize.X, Size.Y);
                DrawLine(from, to, Color, _thickness);
            }

            int horizontalLinesToPlace = (int)(Size.Y / _stepSize.Y) + 1;
            for (int i = 0; i < horizontalLinesToPlace; i++)
            {
                var from = new Vector2(0, i * _stepSize.Y);
                var to = new Vector2(Size.X, i * _stepSize.Y);
                DrawLine(from, to, Color, _thickness);
            }
        }
    }
}
