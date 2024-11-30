public class OrderingTests
{
    [Fact]
    public void PointsSortedByY()
    {
        // Arrange
        EventQueue q = new();
        var first = new Godot.Vector2(0, 1);
        var second = new Godot.Vector2(0, 2);
        var third = new Godot.Vector2(0, 3);

        // Act
        q.Enqueue(second);
        q.Enqueue(first);
        q.Enqueue(third);

        // Assert
        Assert.Equal(3, q.Count);
        Assert.Equal(first, q.Dequeue());
        Assert.Equal(second, q.Dequeue());
        Assert.Equal(third, q.Dequeue());
    }

    [Fact]
    public void DeferToXWhenYsEqual()
    {
        // Arrange
        EventQueue q = new();
        var first = new Godot.Vector2(1, 0);
        var second = new Godot.Vector2(2, 0);
        var third = new Godot.Vector2(3, 0);

        // Act
        q.Enqueue(second);
        q.Enqueue(first);
        q.Enqueue(third);

        // Assert
        Assert.Equal(3, q.Count);
        Assert.Equal(first, q.Dequeue());
        Assert.Equal(second, q.Dequeue());
        Assert.Equal(third, q.Dequeue());
    }

    [Fact]
    public void ThrowOnDuplicatePoints()
    {
        // Arrange
        EventQueue q = new();
        var first = new Godot.Vector2(1, 1);
        var second = new Godot.Vector2(1, 1);

        // Assert on Act
        q.Enqueue(first);
        Assert.Throws<Exception>(() =>
        {
            q.Enqueue(second);
        });
    }
}