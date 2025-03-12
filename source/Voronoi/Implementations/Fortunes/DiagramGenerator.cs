using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Diagram { }

internal class DiagramGenerator<TQ>
    where TQ : IEventQueue, new()
{
    private HashSet<IEvent> _falseAlarms = new();

    protected TQ _eventQueue;

    private Beachline _beachline = new Beachline();
    private Dcel _diagram;

    public DiagramGenerator(IEnumerable<Vector2> sites)
    {
        _eventQueue = new();
        _eventQueue.Initialize(sites.ToArray());
    }

    public Diagram Generate()
    {
        while (_eventQueue.Count > 0)
        {
            IEvent @event = _eventQueue.Dequeue();
            if (_falseAlarms.Contains(@event))
                continue;

            switch (@event)
            {
                case SiteEvent site:
                    HandleSiteEvent(site);
                    break;
                case CircleEvent circle:
                    HandleCircleEvent(circle);
                    break;
            }
        }

        return new Diagram();
    }

    private void HandleSiteEvent(SiteEvent e)
    {
        // 0. Add this site's face to the list
        _diagram._faces.Add(e.Face);

        // 1. If the beachline is empty, insert it at the root and quit.
        if (_beachline.Root == null)
        {
            _beachline.Root = new() { Site = e };
            return;
        }

        // TODO: Handle degenerate case of 2 points at same root y

        // 2. Find arc directly above e in the beachline. If it has a circle
        //    event in our queue, the circle event is a false alarm and is deleted.
        Arc a = _beachline.GetArcAbove(e);
        if (a.ClosingEvent != null)
        {
            _falseAlarms.Add(a.ClosingEvent);
            a.ClosingEvent = null;
        }

        // 3. Replace a with new subtree containing 3 leaves. The middle leaf
        //    will be the arc defined by e, the other 2 leaves will be a, and
        //    the two new internal nodes will store the breakpoints.
        //    We will be using a as the subtree root.
        //    TODO: Rebalance here
        Arc leftLeaf = new(a.Site),
            middleLeaf = new(e),
            rightLeaf = new(a.Site);
        a.Left = leftLeaf;

        // New Internal Node
        a.Right = new()
        {
            Left = middleLeaf,
            Right = rightLeaf
        };

        // 4. Create new half-edge records in the Voronoi diagram structure for the
        //    edge separating V(pi) and V(pj), which will be traced out by the two new
        //    breakpoints.
        Vertex breakpoint = new(e.X, a.GetYAt(e.X, e.Y));
        _diagram._vertices.Add(breakpoint);

        HalfEdge edgeLeft = new()
        {
            Origin = breakpoint,
            IncidentFace = leftLeaf.Site.Face,
        };
        HalfEdge edgeRight = new()
        {
            Origin = breakpoint,
            IncidentFace = rightLeaf.Site.Face,
        };
        edgeLeft.Twin = edgeRight;
        edgeRight.Twin = edgeLeft;
        _diagram._edges.Add(edgeLeft);
        _diagram._edges.Add(edgeRight);

        // 5. Check the triple of consecutive arcs where the new arc for pi is the left arc
        //    to see if the breakpoints converge. If so, insert the circle event into Q and
        //    add pointers between the node in T and the node in Q.Do the same for the
        //    triple where the new arc is the right arc.
        // TODO: Real point values!
        Vector2 p1 = Vector2.Zero, p2 = Vector2.Zero, p3 = Vector2.Zero;
        if (WillBeCircleEvent(p1, p2, p3, out CircleEvent circleEvent))
        {
            _eventQueue.Enqueue(circleEvent);
        }
    }

    private void HandleCircleEvent(CircleEvent e)
    {
    }

    // TODO: Probably need to feed the site events in so we can have a closing arc.
    private static bool WillBeCircleEvent(Vector2 p1, Vector2 p2, Vector2 p3, out CircleEvent circleEvent)
    {
        circleEvent = null;

        // First we need to see if the points are collinear.
        // To do this we need to see if the area of the triangle they make
        // is roughly 0.
        Vector3 pointXs = new(p1.X, p2.X, p3.X);
        Vector3 pointYs = new(p1.Y, p2.Y, p3.Y);
        float triangleArea = new Basis(pointXs, pointYs, Vector3.One).Determinant();
        if (MathF.Abs(triangleArea) < float.Epsilon)
            return false;

        // Now to calculate the circumcenter of these 3 points.
        // The circumcenter is the point that is equidistant from all 3 - the location of my circle event.
        // I stole the maths from wikipedia:
        //     https://en.wikipedia.org/wiki/Circumcircle#Circumcircle_equations
        Vector3 lengthsSqrd = new(p1.LengthSquared(), p2.LengthSquared(), p3.LengthSquared());

        // next to find the circumcenter X-Y coords.
        Vector2 circumcenter = new()
        {
            X = (0.5f / triangleArea) * new Basis(lengthsSqrd, pointYs, Vector3.One).Determinant(),
            Y = (0.5f / triangleArea) * new Basis(pointXs, lengthsSqrd, Vector3.One).Determinant()
        };

        // Finally, add the radius to the Y since we will process the circle event when
        // the sweep-line encounters the bottom of the circumcircle
        float b = new Basis(pointXs, pointYs, lengthsSqrd).Determinant();
        float circumradius = MathF.Sqrt((b / triangleArea) + circumcenter.LengthSquared());
        circleEvent = new()
        {
            X = (int)circumcenter.X,
            Y = (int)(circumcenter.Y + circumradius)
        };
        return true;
    }
}

internal class DiagramGenerator : DiagramGenerator<DefaultEventQueue>
{
    public DiagramGenerator(IEnumerable<Vector2> sites) : base(sites) { }
}
