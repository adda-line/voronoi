internal class Beachline
{
    internal Arc Root;

    public Arc GetArcAbove(SiteEvent e)
    {
        Arc node = Root;
        while (!node.IsLeaf)
        {
            // Find X where child parabolae intersect using
            // the specified directrix.
            float breakpointX = node.GetBreakpointX(e.Y);
            if (e.X < breakpointX)  node = node.LeftChild;
            else                    node = node.RightChild;
        }
        return node;
    }
}
