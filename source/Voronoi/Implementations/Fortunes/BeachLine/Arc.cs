using System;

internal class Arc
{
    internal Arc Parent;

    #region Internal Node Members - Breakpoint
    private Arc _left, _right;

    // When the arc has been closed it becomes an internal node
    // with the following members populated.
    // TODO: Formalize this with types
    internal Arc Left
    {
        get => _left;
        set
        {
            _left = value;
            _left.Parent = this;
        }
    }

    internal Arc Right
    {
        get => _right;
        set
        {
            _right = value;
            _right.Parent = this;
        }
    }
    #endregion

    #region Leaf Members - Arc
    /// <summary>
    /// The event that will close this arc.
    /// Null if it hasn't been detected or there is none.
    /// </summary>
    internal CircleEvent ClosingEvent;

    /// <summary>
    /// Site this arc is formed from.
    /// </summary>
    internal SiteEvent Site;
    #endregion

    internal bool IsLeaf => Left == null && Right == null;

    public Arc() : this(null)
    { }

    public Arc(SiteEvent site)
    {
        Parent = null;
        Left = null;
        Right = null;

        Site = site;
        ClosingEvent = null;
    }

    /// <summary>
    /// Retrieves the right-most leaf of the left subtree.
    /// </summary>
    /// <returns>Right-most leaf of left subtree.</returns>
    internal Arc GetInnerLeafLeft()
    {
        if (Left == null) return null;
        Arc leftLeaf;
        for (leftLeaf = Left;
             !leftLeaf.IsLeaf;
             leftLeaf = leftLeaf.Right);
        return leftLeaf;
    }

    /// <summary>
    /// Retrieves the left-most leaf of the right subtree.
    /// </summary>
    /// <returns>Left-most leaf of right subtree.</returns>
    internal Arc GetInnerLeafRight()
    {
        if (Right == null) return null;
        Arc rightLeaf;
        for (rightLeaf = Right;
             !rightLeaf.IsLeaf;
             rightLeaf = rightLeaf.Left);
        return rightLeaf;
    }

    internal float GetBreakpointX(float directrixY)
    {
        SiteEvent p = GetInnerLeafLeft()?.Site,
                  q = GetInnerLeafRight()?.Site;

        // If the points are at the same Y, the X intercept is in between them.
        float dY = p.Y - q.Y;
        if (dY < float.Epsilon) return (p.X + q.X) / 2.0f;

        // Adjusting the Ys by subtracting the directrix makes the math easier.
        float adjPY = p.Y - directrixY;
        float adjQY = q.Y - directrixY;

        // The equation for a parabola with focus f and directrix l looks like:
        //     (x-f_x)^2/2(f_y-l) + (f_y+l)/2
        // We set the equations for the parabolae defined by the left and right
        // site events and solve for X. This boils down to getting the equation into
        // the form ax^2 + bx + c = 0 and using the quadratic formula to solve it.
        float a = -dY;
        float b = 2 * (adjPY * q.X - adjQY * p.X);
        float c = adjQY * p.X * p.X - adjPY * q.X * q.X + adjPY * adjQY * dY;
        (float? xPos, float? xNeg) = QuadraticFormula(a, b, c);

        // It should always be xPos provided p.X < q.X
        // but to be safe, we'll choose based on relative Ys.
        // If p.Y < q.Y, the intercept will be the greater of the 2.
        // If q.y < p.Y, the intercept will be the lesser of the 2.
        // TODO: Log if it happens to NOT be xPos.
        float intercept = xPos.Value;
        if (p.Y < q.Y) intercept = Math.Max(xPos.Value, xNeg.Value);
        if (q.Y < p.Y) intercept = Math.Min(xPos.Value, xNeg.Value);
        return intercept;
    }

    private static (float?, float?) QuadraticFormula(float a, float b, float c)
    {
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
            return (null, null);
        return (
            (-b + MathF.Sqrt(discriminant)) / (2 * a),
            (-b - MathF.Sqrt(discriminant)) / (2 * a)
        );
    }
}