using Godot;

internal class Vertex
{
    public float X { get; }
    public float Y { get; }

    /// <summary>
    /// Edge whose origin is this vertex.
    /// </summary>
    public HalfEdge IncidentEdge { get; internal set; }

    public Vertex(float x, float y)
    {
        X = x;
        Y = y;
    }

    // TODO: This seems ugly, maybe consolidate the types?
    public static implicit operator Vertex(Vector2 vec) => new(vec.X, vec.Y);
}
