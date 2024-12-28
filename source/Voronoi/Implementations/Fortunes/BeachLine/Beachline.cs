using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Beachline
{
    internal Arc Root;

    public Arc GetArcAbove(SiteEvent e)
    {
        Arc node = Root;
        while (node != null && !node.IsLeaf)
        {
            float breakpointX = node.GetBreakpointX(e, e.Y);
            if (e.X < breakpointX)
                node = node.Left;
            else
                node = node.Right;
        }
        return node;
    }
}
