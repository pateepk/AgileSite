using System;

namespace CMS.Core.Internal
{
    /// <summary>
    /// Provides method for getting current <see cref="DateTime"/>.
    /// </summary>
    internal class DefaultDateTimeNowService : IDateTimeNowService
    {
        /// <summary>
        /// Gets the current <see cref="DateTime"/> by calling <see cref="DateTime.Now"/> method.
        /// </summary>
        /// <returns>Current <see cref="DateTime"/></returns>
        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}