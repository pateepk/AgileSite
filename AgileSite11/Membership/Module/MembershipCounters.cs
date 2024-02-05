using CMS.HealthMonitoring;

namespace CMS.Membership
{
    internal class MembershipCounters
    {
        /// <summary>
        /// Registers the performance counters
        /// </summary>
        public static void RegisterPerformanceCounters()
        {
            HealthMonitoringLogHelper.RegisterCounter(CounterName.ONLINE_USERS, UpdateUserCounter);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.AUTHENTICATED_USERS, UpdateUserCounter);
            HealthMonitoringLogHelper.RegisterCounter(CounterName.ANONYMOUS_VISITORS, UpdateUserCounter);
        }


        /// <summary>
        /// Updates the user counter
        /// </summary>
        /// <param name="counter">Counter to update</param>
        private static void UpdateUserCounter(Counter counter)
        {
            string counterKey = counter.Key;

            // Set global value
            counter.PerformanceCounter.SetValue(GetUserCount(counterKey, null), null);

            if (HealthMonitoringHelper.SiteCountersEnabled && (HealthMonitoringManager.Sites != null))
            {
                foreach (string siteName in HealthMonitoringManager.Sites)
                {
                    // Log site values
                    counter.PerformanceCounter.SetValue(GetUserCount(counterKey, siteName), siteName);
                }
            }
        }


        /// <summary>
        /// Gets count of users (online, authorized or anonymous).
        /// </summary>
        /// <param name="counterKey">Counter key</param>
        /// <param name="siteName">Site name</param>
        private static long GetUserCount(string counterKey, string siteName)
        {
            long value = 0;
            int publicUsers = 0;
            int authenticated = 0;

            // Get users number
            SessionManager.GetUsersNumber(siteName, null, true, false, out publicUsers, out authenticated);

            switch (counterKey)
            {
                // Online users
                case CounterName.ONLINE_USERS:
                    value = publicUsers + authenticated;
                    break;

                // Authenticated users
                case CounterName.AUTHENTICATED_USERS:
                    value = authenticated;
                    break;

                // Anonymous visitors
                case CounterName.ANONYMOUS_VISITORS:
                    value = publicUsers;
                    break;

                // Nothing
                default:
                    break;
            }

            return value;
        }
    }
}
