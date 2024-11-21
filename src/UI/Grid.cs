using Godot;

[GlobalClass]
public partial class Grid : Control
{
    private float _stepSize = 5.0f;
    private bool _showGrid = true;

    [Export(PropertyHint.Range, "5,20,.5")]
    public float StepSize
    {
        get => _stepSize;
        set
        {
            _stepSize = value;
            QueueRedraw();
        }
    }

    [Export]
    public bool ShowGridLines
    {
        get => _showGrid;
        set
        {
            _showGrid = value;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_showGrid)
        {
            var viewportSize = GetViewportRect().Size;
            int verticalLinesToPlace = (int)(viewportSize.X / _stepSize) + 1;
            for (int i = 0;  i < verticalLinesToPlace; i++)
            {
                var from = new Vector2(i * _stepSize, 0);
                var to = new Vector2(i * _stepSize, viewportSize.Y);
                DrawLine(from, to, Colors.DarkGray);
            }

            int horizontalLinesToPlace = (int)(viewportSize.Y / _stepSize) + 1;
            for (int i = 0; i < horizontalLinesToPlace; i++)
            {
                var from = new Vector2(0, i * _stepSize);
                var to = new Vector2(viewportSize.X, i * _stepSize);
                DrawLine(from, to, Colors.DarkGray);
            }
        }
    }
}
