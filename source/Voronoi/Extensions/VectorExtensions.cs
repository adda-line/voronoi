using System;
using Godot;

namespace Voronoi.Extensions;

public static class VectorExtensions
{
    public static Vector2 Hadamard(this Vector2 a, Vector2 b) =>
        new(a.X * b.X, a.Y * b.Y);
}
