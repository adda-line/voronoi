using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Voronoi.Extensions;

internal class DiagramGenerator<TQ>
    where TQ : IEventQueue, new()
{
    // FUTURE: Arbitrary bounding shape?
    private readonly float _boundingBoxWidth, _boundingBoxHeight;

    private readonly HashSet<IEvent> _falseAlarms = new();
    private readonly Beachline _beachline = new();

    protected TQ _eventQueue;

    private Dcel _diagram = new Dcel();


    // TODO: Bounding box is actualy 2x width and height centered at 0,0.
    //       Should allow the caller to pass in a proper rectangle struct.
    public DiagramGenerator(IEnumerable<Vector2> sites, float boundingBoxWidth, float boundingBoxHeight)
    {
        _boundingBoxWidth = boundingBoxWidth;
        _boundingBoxHeight = boundingBoxHeight;

        _eventQueue = new();
        _eventQueue.Initialize([.. sites]);
    }

    public Dcel Generate()
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

        AddBoundingBox();

        return _diagram;
    }

    private void HandleSiteEvent(SiteEvent e)
    {
        // 0. Add this site's face to the list
        _diagram.Faces.Add(e.Face);

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
        a.RightChild = rightLeaf;

        // New Internal Node
        a.LeftChild = new()
        {
            LeftChild = leftLeaf,
            RightChild = middleLeaf
        };

        // 4. Create new half-edge records in the Voronoi diagram structure for the
        //    edge separating V(pi) and V(pj), which will be traced out by the two new
        //    breakpoints.
        Vertex breakpoint = new(e.X, a.GetYAt(e.X, e.Y));
        _diagram.Vertices.Add(breakpoint);

        HalfEdge edgeWithEToLeft = MakeIncompleteHalfEdge(breakpoint, e, rightLeaf.Site);
        a.LeftChild._edge = edgeWithEToLeft;
        _diagram.Edges.Add(edgeWithEToLeft);

        HalfEdge edgeWithEToRight = MakeIncompleteHalfEdge(breakpoint, rightLeaf.Site, e);
        a._edge = edgeWithEToRight;
        _diagram.Edges.Add(edgeWithEToRight);

        edgeWithEToLeft.Twin = edgeWithEToRight;
        edgeWithEToRight.Twin = edgeWithEToLeft;

        // 5a. Check the triple of consecutive arcs where the new arc (middleLeaf) for e is the
        //     left arc to see if the breakpoints converge. If so, insert the circle event into Q
        //     and add pointers between the node in T and the node in Q.
        Arc nextArcToTheRight = rightLeaf.GetArcToRight(out _);
        if (nextArcToTheRight != null &&
            WillBeCircleEvent(middleLeaf, rightLeaf, nextArcToTheRight, out CircleEvent circleEvent))
        {
            _eventQueue.Enqueue(circleEvent);
        }

        // 5b. Do the same for the triple where the new arc (middleLeaf) is the right arc.
        Arc nextArcToTheLeft = leftLeaf.GetArcToLeft(out _);
        if (nextArcToTheLeft != null &&
            WillBeCircleEvent(nextArcToTheLeft, leftLeaf, middleLeaf, out circleEvent))
        {
            _eventQueue.Enqueue(circleEvent);
        }
    }

    private void HandleCircleEvent(CircleEvent e)
    {
        Arc middleArc = e.DisappearingArc;

        // TODO: This is already computed at time of circle event generation
        //       maybe store as additional info? Makes rebalance awkward.
        Arc leftArc = middleArc.GetArcToLeft(out Arc leftCommonAncestor);
        Arc rightArc = middleArc.GetArcToRight(out Arc rightCommonAncestor);

        // TODO: This is an error case, it should be treated as such
        if (leftArc == rightArc) Debug.Print("Single parabola predicted to close another.");

        // 1a. TODO: Delete the disappearing arc from the beachline

        // 1. Mark circle events as "false alarms" on sibling nodes because one of the
        //    nodes in the event (e.DisappearingArc) is no longer a valid candidate for
        //    closing other arcs.
        if (leftArc.ClosingEvent != null)
            _falseAlarms.Add(leftArc.ClosingEvent);

        if (rightArc.ClosingEvent != null)
            _falseAlarms.Add(rightArc.ClosingEvent);

        leftArc.ClosingEvent = rightArc.ClosingEvent = null;

        // 2a. Add the center of the circle causing the event as a vertex record to the
        //     doubly-connected edge list diagram under construction.
        Vertex circumcenter = new(e.X, middleArc.GetYAt(e.X, e.Y));
        _diagram.Vertices.Add(circumcenter);

        // 2b. Update the tuples representing the breakpoints at the internal nodes.
        leftCommonAncestor._edge.Destination =
        rightCommonAncestor._edge.Destination = circumcenter;

        // TODO: Understand
        // Let the ancestor encountered first in the plane (nearest root) represent the
        // edge that starts at the circumcenter.
        Arc earliestAncestor = null;
        for (Arc currentAncestor = middleArc.Parent;
             currentAncestor != _beachline.Root;
             currentAncestor = currentAncestor.Parent)
        {
            if (currentAncestor == leftCommonAncestor)
                earliestAncestor = leftCommonAncestor;
            if (currentAncestor == rightCommonAncestor)
                earliestAncestor = rightCommonAncestor;
        }

        // TODO: Proper error handling.
        Debug.Assert(earliestAncestor != null);

        // TODO: Do I need to delete the existing edge from _diagram?
        earliestAncestor._edge = MakeIncompleteHalfEdge(circumcenter, leftArc.Site, rightArc.Site);
        _diagram.Edges.Add(earliestAncestor._edge);

        // 3. Remove the middle arc. We do this by replacing the arc and its parent
        //    with the middle arc's sibling.
        //    TODO: Rebalance?
        Arc middleArcSibling =
            (middleArc.Parent.LeftChild == middleArc)
                ? middleArc.Parent.RightChild
                : middleArc.Parent.LeftChild;
        if (middleArc.Parent.Parent.LeftChild == middleArc.Parent)
        {
            middleArc.Parent.Parent.LeftChild = middleArcSibling;
        }
        else
        {
            middleArc.Parent.Parent.RightChild = middleArcSibling;
        }

        // 4. Check the new triple of consecutive arcs that has the former left neighbor
        //    of α as its middle arc to see if the two breakpoints of the triple converge.
        //    If so, insert the corresponding circle event into Q. and set pointers between
        //    the new circle event in Q and the corresponding leaf of T. Do the same for
        //    the triple where the former right neighbor is the middle arc.
        Arc nextLeftArc = leftArc.GetArcToLeft(out _);
        if (nextLeftArc != null &&
            WillBeCircleEvent(nextLeftArc, leftArc, rightArc, out CircleEvent circleEvent))
        {
            _eventQueue.Enqueue(circleEvent);
        }

        Arc nextRightArc = rightArc.GetArcToRight(out _);
        if (nextRightArc != null &&
            WillBeCircleEvent(leftArc, rightArc, nextRightArc, out circleEvent))
        {
            _eventQueue.Enqueue(circleEvent);
        }
    }

    private void AddBoundingBox()
    {
        Stack<Arc> edgesToProcess = new();
        edgesToProcess.Push(_beachline.Root);

        while (edgesToProcess.Count > 0)
        {
            Arc arc = edgesToProcess.Pop();
            if (arc.IsLeaf)
                continue;
            Vector2 endpoint = CompleteEdge(arc._edge, _boundingBoxWidth, _boundingBoxHeight);

            // Delete breakpoint vertex from diagram.
            _diagram.Vertices.Remove(arc._edge.Destination);
            arc._edge.Destination = new Vertex(endpoint.X, endpoint.Y);
            _diagram.Vertices.Add(arc._edge.Destination);

            if (arc.LeftChild != null)
                edgesToProcess.Push(arc.LeftChild);
            if (arc.RightChild != null)
                edgesToProcess.Push(arc.RightChild);
        }
    }

    /// <summary>
    /// Detects if the middle parabola, <paramref name="p2"/> will be closed out
    /// by <paramref name="p1"/> and <paramref name="p3"/>. Creates a circle event
    /// with the appropriate closure point if so.
    /// </summary>
    /// <param name="p1">Left parabola.</param>
    /// <param name="p2">Middle parabola.</param>
    /// <param name="p3">Right parabola.</param>
    /// <param name="circleEvent">Event that represents the closure of <paramref name="p2"/>.</param>
    /// <returns>True if a closure event is detected, false otherwise.</returns>
    /// TODO: Add check that middle arc site position is highest since that _probably_ precludes a circle event.
    private static bool WillBeCircleEvent(Arc p1, Arc p2, Arc p3, out CircleEvent circleEvent)
    {
        // Assert that the sites are actually left to right.
        Debug.Assert(p1.Site.X < p2.Site.X);
        Debug.Assert(p2.Site.X < p3.Site.X);

        circleEvent = null;

        // First we need to see if the points are collinear.
        // To do this we need to see if the area of the triangle they make
        // is roughly 0.
        // TODO: Figure out a good epsilon.
        Vector3 pointXs = new(p1.Site.X, p2.Site.X, p3.Site.X);
        Vector3 pointYs = new(p1.Site.Y, p2.Site.Y, p3.Site.Y);
        float triangleArea = new Basis(pointXs, pointYs, Vector3.One).Determinant();
        if (MathF.Abs(triangleArea) < float.Epsilon)
            return false;

        // Now to calculate the circumcenter of these 3 points.
        // The circumcenter is the point that is equidistant from all 3 sites.
        // The bottom of the circumcenter will be the circle event.
        // I stole the maths from wikipedia:
        //     https://en.wikipedia.org/wiki/Circumcircle#Circumcircle_equations
        Vector3 lengthsSqrd = new(
            p1.Site.Position.LengthSquared(),
            p2.Site.Position.LengthSquared(),
            p3.Site.Position.LengthSquared()
        );

        // Next to find the circumcenter X-Y coords.
        Vector2 circumcenter = new()
        {
            X = (0.5f / triangleArea) * new Basis(lengthsSqrd, pointYs, Vector3.One).Determinant(),
            Y = (0.5f / triangleArea) * new Basis(pointXs, lengthsSqrd, Vector3.One).Determinant()
        };

        // Finally, add the circumradius to the Y since we will process the circle event
        // when the sweep-line encounters the bottom of the circumcircle
        float b = new Basis(pointXs, pointYs, lengthsSqrd).Determinant();
        float circumradius = MathF.Sqrt((b / triangleArea) + circumcenter.LengthSquared());
        circleEvent = new()
        {
            X = circumcenter.X,
            Y = circumcenter.Y + circumradius,
            DisappearingArc = p2
        };
        return true;
    }

    /// <summary>
    /// Projecs the edge's end-point within the provided bounding-box.
    /// </summary>
    /// <remarks>Graph: https://www.desmos.com/calculator/lpavb1npa6</remarks>
    private static Vector2 CompleteEdge(HalfEdge edge, float boundingBoxWidth, float boundingBoxHeight)
    {
        Debug.Assert(edge._direction.IsNormalized());

        // TODO: Why aren't I using vectors everywhere?
        Vector2 originVector = new(edge.Origin.X, edge.Origin.Y);

        // By figuring out which edges are being pointed towards, we know
        // where to project the driection's end-point.
        Vector2 edgeSign = edge._direction.Sign();
        Vector2 boundingBox = new(boundingBoxWidth, boundingBoxHeight);
        Vector2 edgesBeingPointedTowards = edgeSign.Hadamard(boundingBox);

        // Now we figure out how far from the pointed-to horizontal and
        // vertical bounding box edges.
        // By dividing that distance by the direction vector components
        // we can estimate how far we'd travel along the direction vector
        // until an edge is hit.
        // The minimum "scaling factor" is the minimum distance either the
        // x or y component permits travel before collision.
        Vector2 offsetFromPointedEdges = edgesBeingPointedTowards - originVector;
        Vector2 scalingFactors = offsetFromPointedEdges.Hadamard(edge._direction.Inverse());
        float scalingFactor = scalingFactors[(int)scalingFactors.MinAxisIndex()];

        // Then we scale the direction vector by the minimum factor and
        // re-center on the ray's origin.
        Vector2 boundingBoxCollision = scalingFactor * edge._direction + originVector;
        return boundingBoxCollision;
    }

    /// <summary>
    /// Creates a <see cref="HalfEdge"/> that will be completed (have its destination set)
    /// when the algorithm completes.
    /// </summary>
    /// <param name="origin">Where the edge begins.</param>
    /// <param name="leftSite">
    /// Site to the left of this edge - the associated face will be treated as the <see cref="HalfEdge.IncidentFace"/>.
    /// </param>
    /// <param name="rightSite">Site to the right of this edge.</param>
    /// <returns>An incomplete edge to record within the beachline.</returns>
    private static HalfEdge MakeIncompleteHalfEdge(Vertex origin, SiteEvent leftSite, SiteEvent rightSite)
    {
        // Since the half-edge won't be completed until the end of the algorithm,
        // we need a way to "predict" the direction of the edge.
        // We can calculate the direction vector by rotating the direction vector
        // from the left site to the right site by -90 degress.
        // Rotation by 90 degrees ccw can be done easily with a simple swizzle i.e.
        //     (x,y) => (-y,x)
        Vector2 leftToRight = new(rightSite.X - leftSite.X, rightSite.Y - leftSite.Y);
        return new HalfEdge
        {
            _direction = new Vector2(-leftToRight.Y, leftToRight.X).Normalized(),
            Origin = origin,
            IncidentFace = leftSite.Face
        };
    }
}

internal class DiagramGenerator : DiagramGenerator<DefaultEventQueue>
{
    public DiagramGenerator(IEnumerable<Vector2> sites, float boundingBoxWidth, float boundingBoxHeight) : base(sites, boundingBoxWidth, boundingBoxHeight) { }
}