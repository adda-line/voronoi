using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BruteForce : TextureRect
{
    private PointPlacer _siteContainer;

    ImageTexture _tex = new();

    [Export]
    public PointPlacer SiteContainer
    {
        get => _siteContainer;
        set
        {
            DetachFromSiteContainer();
            AttachToSiteContainer(value);
            if (_siteContainer != null)
                QueueRedraw();
        }
    }

    public override void _Ready()
    {
        Texture = _tex;
    }

    public override void _Draw()
    {
        var sites = _siteContainer?.Points.ToList() ?? new();
        if (sites.Any())
        {
            var size = GetViewportRect().Size;
            Image img = Image.Create((int)size.X, (int)size.Y, false, Image.Format.Rgba8);

            var colorStep = 1.0f / sites.Count;

            // Color every pixel
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    int closestSiteIdx = FindIndexOfClosestPoint(x, y, sites);
                    img.SetPixel(x, y, Color.FromHsv(colorStep * closestSiteIdx, 1, 1));
                }
            }

            _tex.SetImage(img);
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

    private void SiteContainer_PointMoved(Point _)
    {
        QueueRedraw();
    }

    private void SiteContainer_ChildExitingTree(Node node)
    {
        QueueRedraw();
    }

    private void SiteContainer_ChildEnteredTree(Node node)
    {
        QueueRedraw();
    }

    private static int FindIndexOfClosestPoint(int x, int y, IEnumerable<Point> points)
    {
        int minIdx = 0;
        float minDst = float.PositiveInfinity;
        for (int idx = 0; idx < points.Count(); idx++)
        {
            var dst = (points.ElementAt(idx).GlobalPosition - new Vector2I(x, y)).LengthSquared();
            if (dst < minDst)
            {
                minIdx = idx;
                minDst = dst;
            }
        }
        return minIdx;
    }
}
