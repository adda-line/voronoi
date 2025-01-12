using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    }

    private void HandleCircleEvent(CircleEvent e)
    {
    }
}

internal class DiagramGenerator : DiagramGenerator<DefaultEventQueue>
{
    public DiagramGenerator(IEnumerable<Vector2> sites) : base(sites) { }
}
