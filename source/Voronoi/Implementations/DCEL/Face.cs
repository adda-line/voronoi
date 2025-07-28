internal class Face
{
    /// <summary>
    /// An arbitrary edge on the boundary of this face.
    /// Traversing this edge's next linkage defines this face.
    /// </summary>
    public HalfEdge OuterComponent { get; internal set; }
}