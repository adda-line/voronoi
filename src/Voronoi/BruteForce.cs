using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class BruteForce : TextureRect
{
    private readonly object _diagramLock = new { };
    private CancellationTokenSource _taskCanceler = new CancellationTokenSource();

    private PointPlacer _siteContainer;

    private Image _diagram;
    private Task _diagramTask;

    private ImageTexture _tex = new();

    [Export]
    public PointPlacer SiteContainer
    {
        get => _siteContainer;
        set
        {
            DetachFromSiteContainer();
            AttachToSiteContainer(value);
            if (_siteContainer != null)
                StartRefresh();
        }
    }

    public override void _Ready()
    {
        Texture = _tex;
    }

    public override void _Draw()
    {
        GD.Print("Redrawing Diagram");
        if (_diagram == null)
            return;

        lock (_diagramLock)
        {
            _tex.SetImage(_diagram);
            DrawTexture(_tex, Vector2.Zero);
        }
    }

    private void AttachToSiteContainer(PointPlacer container)
    {
        _siteContainer = container;
        container.PointMoved += SiteContainer_PointMoved;
        container.ChildEnteredTree += SiteContainer_ChildEnteredTree;
        container.ChildExitingTree += SiteContainer_ChildExitingTree;
    }

    private void DetachFromSiteContainer()
    {
        if (_siteContainer == null)
            return;

        _siteContainer.PointMoved -= SiteContainer_PointMoved;
        _siteContainer.ChildEnteredTree -= SiteContainer_ChildEnteredTree;
        _siteContainer.ChildExitingTree -= SiteContainer_ChildExitingTree;
        _siteContainer = null;
    }

    private void SiteContainer_PointMoved(Point p)
    {
        GD.Print($"Point ({p.Name}) Moved");
        StartRefresh();
    }

    private void SiteContainer_ChildEnteredTree(Node n)
    {
        if (n is Draggable<Point> dp)
        {
            GD.Print($"Point ({n.Name}) Made at {dp.Name}");
            StartRefresh();
        }
    }

    private void SiteContainer_ChildExitingTree(Node _)
    {
        GD.Print("Point Deleted");
        StartRefresh();
    }

    private void StartRefresh()
    {
        if (_diagramTask?.Status == TaskStatus.Running)
        {
            _taskCanceler.Cancel();
            _diagramTask.Wait();
        }

        _taskCanceler = new CancellationTokenSource();

        var sites = new List<Vector2>();
        for (int i = 0; i < _siteContainer.GetChildCount(true); i++)
        {
            var child = _siteContainer.GetChild(i, true);
            if (child is Draggable<Point> dp)
                sites.Add(dp.GlobalPosition);
        }

        var size = GetViewportRect().Size;
        _diagramTask = Task.Run(() => RefreshDiagram(sites, size));
    }

    private void RefreshDiagram(List<Vector2> sites, Vector2 size)
    {
        Action<string> print = GD.Print;
        Callable.From(print).CallDeferred("Started Refresh");

        Image img = Image.Create((int)size.X, (int)size.Y, false, Image.Format.Rgba8);

        var colorStep = 1.0f / sites.Count;

        // Color every pixel
        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                if (_taskCanceler.Token.IsCancellationRequested)
                    return;
                int closestSiteIdx = FindIndexOfClosestSite(x, y, sites);
                img.SetPixel(x, y, Color.FromHsv(colorStep * closestSiteIdx, 1, 1));
            }
        }

        lock(_diagramLock)
            _diagram = img;
        Callable.From(QueueRedraw).CallDeferred();
    }

    private static int FindIndexOfClosestSite(int x, int y, List<Vector2> sites)
    {
        Vector2 pixelPos = new(x, y);

        int minIdx = 0;
        float minDst = float.PositiveInfinity;
        for (int idx = 0; idx < sites.Count; idx++)
        {
            var dst = (sites[idx] - pixelPos).LengthSquared();
            if (dst < minDst)
            {
                minIdx = idx;
                minDst = dst;
            }
        }
        return minIdx;
    }
}
