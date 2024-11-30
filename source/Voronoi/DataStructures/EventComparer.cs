using System.Collections.Generic;
using System;

internal class EventComparer : IComparer<IEvent>
{
    /// <inheritdoc/>
    /// <remarks>TODO: Use an epsilon value.</remarks>
    public int Compare(IEvent x, IEvent y)
    {
        // Sort by Y first
        if (x.Position.Y < y.Position.Y) return -1;
        if (x.Position.Y > y.Position.Y) return 1;

        // Then by X
        if (x.Position.X < y.Position.X) return -1;
        if (x.Position.X > y.Position.X) return 1;

        // Then they gotta be equal!
        // Which we don't like >:)
        throw new InvalidOperationException();
    }
}