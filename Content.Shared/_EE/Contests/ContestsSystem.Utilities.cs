namespace Content.Shared._EE.Contests;

public sealed partial class ContestsSystem
{
    /// <summary>
    ///     Clamp a contest to a Range of [Epsilon, 32bit integer limit]. This exists to make sure contests are always "Safe" to divide by.
    /// </summary>
    private static float ContestClamp(float input)
    {
        return Math.Clamp(input, float.Epsilon, float.MaxValue);
    }
}
