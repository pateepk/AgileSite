using CMS.Activities.Internal;

namespace CMS.Activities
{
    /// <summary>
    /// Provides possibility to modify activity data. Can be registered in <see cref="IActivityLogService"/>.
    /// </summary>
    public interface IActivityModifier
    {
        /// <summary>
        /// Mofifies activity properties. Should not rely on <see cref="IActivityInfo"/> properties, it should only update them.
        /// </summary>
        /// <param name="activity">Activity info</param>
        void Modify(IActivityInfo activity);
    }
}