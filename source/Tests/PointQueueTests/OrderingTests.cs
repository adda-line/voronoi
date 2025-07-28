using Godot;

public class OrderingTests
{
    [Fact]
    public void Test()
    {
        List<Vector2> sites = new()
        {
            GetEvent(1, 1),
            GetEvent(2, 2)
        };
        DiagramGenerator generator = new(sites);
        _ = generator.Generate();
    }

    [Fact]
    public void PointsSortedByY()
    {
        // Arrange
        DefaultEventQueue q = new();
        var first = GetEvent(0, 3);
        var second = GetEvent(0, 2);
        var third = GetEvent(0, 1);

        // Act
        q.Initialize(second, first, third);

        // Assert
        Assert.Equal(3, q.Count);

        var top = q.Dequeue();
        Assert.Equal(first.X, top.X);
        Assert.Equal(first.Y, top.Y);

        top = q.Dequeue();
        Assert.Equal(second.X, top.X);
        Assert.Equal(second.Y, top.Y);

        top = q.Dequeue();
        Assert.Equal(third.X, top.X);
        Assert.Equal(third.Y, top.Y);
    }

    [Fact]
    public void DeferToXWhenYsEqual()
    {
        // Arrange
        DefaultEventQueue q = new();
        var first = GetEvent(1, 0);
        var second = GetEvent(2, 0);
        var third = GetEvent(3, 0);

        // Act
        q.Initialize(second, first, third);

        // Assert
        Assert.Equal(3, q.Count);
        var top = q.Dequeue();
        Assert.Equal(first.X, top.X);
        Assert.Equal(first.Y, top.Y);

        top = q.Dequeue();
        Assert.Equal(second.X, top.X);
        Assert.Equal(second.Y, top.Y);

        top = q.Dequeue();
        Assert.Equal(third.X, top.X);
        Assert.Equal(third.Y, top.Y);
    }

    [Fact]
    public void ThrowOnDuplicatePoints()
    {
        // Arrange
        DefaultEventQueue q = new();
        var first = GetEvent(1, 1);
        var second = GetEvent(1, 1);

        // Assert on Act
        Assert.Throws<InvalidOperationException>(() =>
        {
            q.Initialize(first, second);
        });
    }

    private static Vector2 GetEvent(float x, float y) => new(x, y);
}