﻿using Godot;

[GlobalClass]
public partial class Grid : Control
{
    private const int DefaultGridSize = 25;

    private Vector2 _stepSize = new(DefaultGridSize, DefaultGridSize);
    private bool _showGrid = true;

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
            int verticalLinesToPlace = (int)(viewportSize.X / _stepSize.X) + 1;
            for (int i = 0;  i < verticalLinesToPlace; i++)
            {
                var from = new Vector2(i * _stepSize.X, 0);
                var to = new Vector2(i * _stepSize.X, viewportSize.Y);
                DrawLine(from, to, Colors.DarkGray);
            }

            int horizontalLinesToPlace = (int)(viewportSize.Y / _stepSize.Y) + 1;
            for (int i = 0; i < horizontalLinesToPlace; i++)
            {
                var from = new Vector2(0, i * _stepSize.Y);
                var to = new Vector2(viewportSize.X, i * _stepSize.Y);
                DrawLine(from, to, Colors.DarkGray);
            }
        }
    }
}
