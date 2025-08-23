using Godot;

namespace DiagramTests;

public class DiagramTests
{
    [Fact]
    public void SimpleDiagramTest()
    {
        // Arrange
        List<Vector2> sites =
        [
            new(-1, -1),
            new(1, 1)
        ];

        // Act
        DiagramGenerator generator = new(sites, 3, 3);
        Dcel diagram = generator.Generate();

        // Assert
        // Vertices in each corner to separate the sites.
        Assert.Equal(2, diagram.Vertices.Count);
        Assert.Single(diagram.Vertices, v => v.X == 3 && v.Y == -3);
        Assert.Single(diagram.Vertices, v => v.X == -3 && v.Y == 3);

        // Two-half edges to form the boundary between the site cells.
        Assert.Equal(2, diagram.Edges.Count);
        Assert.Single(diagram.Edges, e => e.Origin.X == -3 && e.Origin.Y ==  3 && e.Destination.X ==  3 && e.Destination.Y == -3);
        Assert.Single(diagram.Edges, e => e.Origin.X ==  3 && e.Origin.Y == -3 && e.Destination.X == -3 && e.Destination.Y ==  3);

        // Two faces, one for each cell.
        Assert.Equal(2, diagram.Faces.Count);
    }

    [Fact]
    public void SameYRootPoint()
    {
        // Arrange
        List<Vector2> sites =
        [
            new(-1, 0),
            new(1, 0)
        ];

        // Act
        DiagramGenerator generator = new(sites, 2, 2);
        Dcel diagram = generator.Generate();

        // Assert
        // Vertices at the top center and bottom to separate the sites.
        Assert.Equal(2, diagram.Vertices.Count);
        Assert.Single(diagram.Vertices, v => v.X == 0 && v.Y == 2);
        Assert.Single(diagram.Vertices, v => v.X == 0 && v.Y == -2);

        // Two-half edges to form the boundary between the site cells.
        Assert.Equal(2, diagram.Edges.Count);
        Assert.Single(diagram.Edges, e => e.Origin.X == 0 && e.Origin.Y == 2 && e.Destination.X == 0 && e.Destination.Y == -2);
        Assert.Single(diagram.Edges, e => e.Origin.X == 0 && e.Origin.Y == -2 && e.Destination.X == 0 && e.Destination.Y == 2);

        // Two faces, one for each cell.
        Assert.Equal(2, diagram.Faces.Count);
    }

    [Fact]
    public void ArcClosureTest()
    {
        // Arrange
        List<Vector2> sites =
        [
            new(0, 1),
            new(-1, -1),
            new(1, -1)
        ];

        // Act
        DiagramGenerator generator = new(sites, 2, 2);
        Dcel diagram = generator.Generate();

        // Assert
        // One in each top corner, one in the center of all 3 sites, and one at the center bottom.
        // Should form a 'Y' shape
        Assert.Equal(4, diagram.Vertices.Count);

        // Six-half edges to form the boundary between the site cells.
        Assert.Equal(6, diagram.Edges.Count);

        // Two faces, one for each cell.
        Assert.Equal(2, diagram.Faces.Count);
    }
}