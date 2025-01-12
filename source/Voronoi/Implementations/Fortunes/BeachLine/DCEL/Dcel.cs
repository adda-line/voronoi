using System.Collections.Generic;

internal class Dcel
{
    internal List<Vertex> _vertices = new();
    internal List<HalfEdge> _edges = new();
    internal List<Face> _faces = new();
}
