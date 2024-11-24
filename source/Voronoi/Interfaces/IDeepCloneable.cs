/// <summary>
/// Something that can be deep cloned.
/// </summary>
public interface IDeepCloneable
{
    /// <summary>
    /// Creates a deep clone of this object.
    /// </summary>
    /// <returns>A new instance of the object on which this was called containing the same data but no shared references.</returns>
    IDeepCloneable DeepClone();
}

/// <summary>
/// Something that can be deep cloned.
/// </summary>
/// <typeparam name="T">The type of the thing being deep cloned.</typeparam>
public interface IDeepCloneable<T> : IDeepCloneable
    where T : IDeepCloneable
{
    /// <summary>
    /// Creates a deep clone of this object.
    /// </summary>
    /// <returns>A new instance of the object on which this was called containing the same data but no shared references.</returns>
    new T DeepClone();
}
