using Godot;

public class OrderingTests
{
    [Fact]
    public void PointsSortedByY()
    {
        // Arrange
        DefaultEventQueue q = new();
        var first =  GetEvent(0, 1);
        var second = GetEvent(0, 2);
        var third =  GetEvent(0, 3);

        // Act
        q.Initialize(second, first, third);

        // Assert
        Assert.Equal(3, q.Count);

        var lowestPriority = q.Dequeue();
        Assert.Equal(first.X, lowestPriority.X);
        Assert.Equal(first.Y, lowestPriority.Y);

        lowestPriority = q.Dequeue();
        Assert.Equal(second.X, lowestPriority.X);
        Assert.Equal(second.Y, lowestPriority.Y);

        lowestPriority = q.Dequeue();
        Assert.Equal(third.X, lowestPriority.X);
        Assert.Equal(third.Y, lowestPriority.Y);
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
        var lowestPriority = q.Dequeue();
        Assert.Equal(first.X, lowestPriority.X);
        Assert.Equal(first.Y, lowestPriority.Y);

        lowestPriority = q.Dequeue();
        Assert.Equal(second.X, lowestPriority.X);
        Assert.Equal(second.Y, lowestPriority.Y);

        lowestPriority = q.Dequeue();
        Assert.Equal(third.X, lowestPriority.X);
        Assert.Equal(third.Y, lowestPriority.Y);
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