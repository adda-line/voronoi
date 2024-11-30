using NSubstitute;

public class OrderingTests
{
    [Fact]
    public void PointsSortedByY()
    {
        // Arrange
        EventQueue q = new();
        var first =  GetEvent(0, 1);
        var second = GetEvent(0, 2);
        var third =  GetEvent(0, 3);

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
        var first = GetEvent(1, 0);
        var second = GetEvent(2, 0);
        var third = GetEvent(3, 0);

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
        var first = GetEvent(1, 1);
        var second = GetEvent(1, 1);

        // Assert on Act
        q.Enqueue(first);
        Assert.Throws<InvalidOperationException>(() =>
        {
            q.Enqueue(second);
        });
    }

    private static IEvent GetEvent(float x, float y)
    {
        var @event = Substitute.For<IEvent>();
        @event.Position.Returns(new Godot.Vector2(x, y));
        return @event;
    }
}