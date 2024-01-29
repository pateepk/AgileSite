using System;

namespace CMS.Core.Internal
{
    /// <summary>
    /// Provides method for getting current <see cref="DateTime"/>.
    /// </summary>
    public interface IDateTimeNowService
    {
        /// <summary>
        /// Gets the current <see cref="DateTime"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used only to improve code testability (allowing to mock the current <see cref="DateTime"/>). It does not handle time zones
        /// and should not be used for any other purposes.
        /// </remarks>
        /// <returns>Current <see cref="DateTime"/></returns>
        DateTime GetDateTimeNow();
    }
}
