/// <summary>
/// Abstracts unit data loading away from Unity API
/// </summary>
public interface IUnit
{
    /// <summary>
    /// Tell IUnit to load data
    /// </summary>
    /// <param name="unit">which unit to use as reference</param>
    /// <param name="direction">which direction relative to reference to load</param>
    /// <returns>true once the operation completes, false if it failed</returns>
    bool Load(IUnit unit, Direction direction);

    /// <summary>
    /// Chain another unit such that when <see cref="Load"/> is used, the other unit's load
    /// will be called after the operation completes.
    /// </summary>
    /// <param name="unit">chained unit</param>
    /// <param name="direction">direction to load to</param>
    void Chain(IUnit unit, Direction direction);
}
