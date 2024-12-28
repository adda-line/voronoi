﻿using Godot;
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

        // 3. Replace a with a new subtree containing 3 leaves. The middle leaf
        //    will be the arc defined by e, the other 2 leaves will be a, and
        //    the two new internal nodes will store the breakpoints.
        // TODO: Rebalance I guess?
        // Internal Node
        // TODO: Wait what should the site be for this node??
        Arc subtreeRoot = new(e)
        {
            // Left Leaf
            Left = new(a.Site),

            // Internal Node
            // TODO: Wait what should the site be for this node??
            Right = new(e)
            {
                // Middle Leaf
                Left = new(e),

                // Right Leaf
                Right = new(a.Site)
            }
        };
        a.ReplaceWith(subtreeRoot);
    }

    private void HandleCircleEvent(CircleEvent e)
    {
    }
}

internal class DiagramGenerator : DiagramGenerator<DefaultEventQueue>
{
    public DiagramGenerator(IEnumerable<Vector2> sites) : base(sites) { }
}
