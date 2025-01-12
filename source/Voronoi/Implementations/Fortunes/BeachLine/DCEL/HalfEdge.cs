internal class HalfEdge
{
    /// <summary>
    /// Where the edge begins. We don't need to store where it ends
    /// since that can be located via <see cref="Twin.Origin"/>.
    /// </summary>
    public Vertex Origin { get; internal set; }

    /// <summary>
    /// Unique half-edge on the boundary of <see cref="IncidentFace"/>
    /// whose origin is our destination.
    /// </summary>
    public HalfEdge Next { get; internal set; }

    /// <summary>
    /// Unique half-edge on the boundary of <see cref="IncidentFace"/>
    /// whose destination is our origin and whose origin is our destination.
    /// </summary>
    public HalfEdge Prev { get; internal set; }

    /// <summary>
    /// This half-edge's twin. Together they form the mythical FULL EDGE.
    /// Its origin is this' destination and vice versa.
    /// </summary>
    public HalfEdge Twin { get; internal set; }

    /// <summary>
    /// The face this edge bounds; lies to the left of this edge.
    /// </summary>
    public Face IncidentFace { get; set; }
}
