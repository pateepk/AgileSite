namespace CMS.Activities
{
    /// <summary>
    /// Can filter out logging of activities, logged through <see cref="IActivityLogService"/>.
    /// </summary>
    public interface IActivityLogFilter
    {
        /// <summary>
        /// Checks whether activity should be logged.
        /// </summary>
        /// <returns>Returns <c>true</c> if activity should not be logged, otherwise false.</returns>
        bool IsDenied();
    }
}