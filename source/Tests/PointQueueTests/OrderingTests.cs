using Godot;

public class OrderingTests
{
    [Fact]
    public void PointsSortedByY()
    {
        // Arrange
        DefaultEventQueue q = new();
        Vector2 first = new(0, 3);
        Vector2 second = new(0, 2);
        Vector2 third = new(0, 1);

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
        Vector2 first = new(1, 0);
        Vector2 second = new(2, 0);
        Vector2 third = new(3, 0);

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
        Vector2 first = new(1, 1);
        Vector2 second = new(1, 1);

        // Assert on Act
        Assert.Throws<InvalidOperationException>(() =>
        {
            q.Initialize(first, second);
        });
    }

    private static Vector2 GetEvent(float x, float y) => new(x, y);
}