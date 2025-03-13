using System;

internal class Arc
{
    internal Arc Parent;

    #region Internal Node Members - Breakpoint
    private Arc _left, _right;

    // When the arc has been closed it becomes an internal node
    // with the following members populated.
    // TODO: Formalize this with types
    internal Arc LeftChild
    {
        get => _left;
        set
        {
            _left = value;
            _left.Parent = this;
        }
    }

    internal Arc RightChild
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

    internal bool IsLeaf => LeftChild == null && RightChild == null;

    public Arc() : this(null)
    { }

    public Arc(SiteEvent site)
    {
        Parent = null;
        LeftChild = null;
        RightChild = null;

        Site = site;
        ClosingEvent = null;
    }

    /// <summary>
    /// Gets the Y value of the parabola defined by this arc's focus - <see cref="Site"/>
    /// and the provided directrix - <paramref name="directrixY"/>
    /// </summary>
    /// <remarks>
    /// This code is derived directly from Wikipedia's article on parabolae circa Jan. 7, 2025:
    /// https://en.wikipedia.org/wiki/Parabola#Axis_of_symmetry_parallel_to_the_y_axis
    /// </remarks>
    /// <param name="x">X value to plug into the focus-directrix relation.</param>
    /// <param name="directrixY">The heigh of the directrix so that a parabola may be defined with <see cref="Site"/> as the focus.</param>
    /// <returns>Y value for the parabola defined by this arc's focus (<see cref="Site"/>) and the provided directrix.</returns>
    public float GetYAt(float x, float directrixY)
    {
        float _2f = (Site.Y - directrixY);
        float _4f = 2 * _2f;
        float _f = _2f / 2;
        float _1Over2f = 1 / _2f;
        float _1Over4f = 1 / _4f;

        // Define vertex V it lie directly under Site and equidistant
        // from both the Site and the directrix.
        //     X = Site.X
        //     Y = (Site.Y + directrixY)/2 <- shortcut is Site.Y - _f
        float vx = Site.X;
        float vy = Site.Y - _f;

        // These will form the standard definition of a parabola
        // i.e. y = ax^2 + bx + c
        float a = _1Over4f;
        float b = -(vx * _1Over2f);
        float c = (vx * vx * _1Over4f) + vy;
        return (a * x * x) +
               (b * x) +
               (c);
    }

    /// <summary>
    /// Determines the arc directly to the left of this one on the beachline.
    /// </summary>
    /// <returns>The arc directly to the left of this on the beachline.</returns>
    /// TODO: Add handling for if this is the left-most arc.
    internal Arc GetArcToLeft()
    {
        // First we need to get the first ancestor such that _this_ is
        // in the right subtree - only then will there be a left sub-tree
        // to traverse.
        Arc ancestor = this;
        while (ancestor == ancestor.Parent.LeftChild)
        {
            ancestor = ancestor.Parent;
        }
        ancestor = ancestor.Parent;

        // Next we need to get the right-most leaf of the ancestor's
        // left-subtree his will be the arc directly to the left of
        // _this_ arc.
        return ancestor.GetInnerLeafLeft();
    }

    /// <summary>
    /// Determines the arc directly to the right of this one on the beachline.
    /// </summary>
    /// <returns>The arc directly to the right of this on the beachline.</returns>
    /// TODO: Add handling for if this is the right-most arc.
    internal Arc GetArcToRight()
    {
        // First we need to get the first ancestor such that _this_ is
        // in the left subtree - only then will there be a right sub-tree
        // to traverse.
        Arc ancestor = this;
        while (ancestor == ancestor.Parent.LeftChild)
        {
            ancestor = ancestor.Parent;
        }
        ancestor = ancestor.Parent;

        // Next we need to get the left-most leaf of the ancestor's
        // right-subtree his will be the arc directly to the left of
        // _this_ arc.
        return ancestor.GetInnerLeafRight();
    }

    /// <summary>
    /// Retrieves the right-most leaf of the left subtree.
    /// </summary>
    /// <returns>Right-most leaf of left subtree.</returns>
    internal Arc GetInnerLeafLeft()
    {
        if (LeftChild == null) return null;
        Arc leftLeaf;
        for (leftLeaf = LeftChild;
             !leftLeaf.IsLeaf;
             leftLeaf = leftLeaf.RightChild);
        return leftLeaf;
    }

    /// <summary>
    /// Retrieves the left-most leaf of the right subtree.
    /// </summary>
    /// <returns>Left-most leaf of right subtree.</returns>
    internal Arc GetInnerLeafRight()
    {
        if (RightChild == null) return null;
        Arc rightLeaf;
        for (rightLeaf = RightChild;
             !rightLeaf.IsLeaf;
             rightLeaf = rightLeaf.LeftChild);
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