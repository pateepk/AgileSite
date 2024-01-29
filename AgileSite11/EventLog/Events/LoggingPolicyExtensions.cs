using System;
using System.Runtime.Caching;

using CMS.Core;

namespace CMS.EventLog
{
    /// <summary>
    /// Class providing extension methods for logging policy.
    /// </summary>
    internal static class LoggingPolicyExtensions
    {
        /// <summary>
        /// Ensures that event is marked as logged in internal cache with dependence on <see cref="LoggingPolicy"/> for current event.
        /// </summary>
        /// <param name="eventObject">Event object.</param>
        /// <exception cref="NotSupportedException">Thrown when unknown policy type is used.</exception>
        /// <returns>False if event was already logged; otherwise true.</returns>
        internal static bool TryMarkEventAsLogged(this EventLogInfo eventObject)
        {
            CacheItem alreadyExistingItem = null;

            switch (eventObject.LoggingPolicy.Type)
            {
                case LoggingPolicyEnum.Default:
                    // Default policy is not blocking any logging
                    break;

                case LoggingPolicyEnum.OncePerPeriod:
                    alreadyExistingItem = MemoryCache.Default.AddOrGetExisting(GetCacheItem(eventObject), GetCacheItemPolicy(new DateTimeOffset(DateTime.UtcNow.Add(eventObject.LoggingPolicy.Period))));
                    break;

                case LoggingPolicyEnum.OnlyOnce:
                    alreadyExistingItem = MemoryCache.Default.AddOrGetExisting(GetCacheItem(eventObject), GetCacheItemPolicy(new DateTimeOffset(DateTime.MaxValue)));
                    break;

                default:
                    throw new NotSupportedException("Unknown policy type '" + eventObject.LoggingPolicy.Type + "'");
            }

            return
                // If item is not created the default policy was used
                (alreadyExistingItem == null) ||
                // Null value indicates that cache item was set to the memory cache, otherwise memory item already exists
                (alreadyExistingItem.Value == null);
        }


        /// <summary>
        /// Checks if the event was already logged.
        /// </summary>
        /// <param name="eventObject">Event object.</param>
        /// <returns>True if event was already logged; false otherwise.</returns>
        internal static bool IsLogged(this EventLogInfo eventObject)
        {
            return MemoryCache.Default.Contains(GetPolicyItemCacheKey(eventObject));
        }


        /// <summary>
        /// Creates default <see cref="CacheItemPolicy"/> object.
        /// </summary>
        /// <param name="absoluteExpiration">Absolute expiration <see cref="DateTimeOffset"/></param>
        private static CacheItemPolicy GetCacheItemPolicy(DateTimeOffset absoluteExpiration)
        {
            return new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = absoluteExpiration
            };
        }


        /// <summary>
        /// Creates <see cref="CacheItem"/> object for specified <paramref name="eventObject"/>.
        /// </summary>
        /// <param name="eventObject">Event object.</param>
        private static CacheItem GetCacheItem(EventLogInfo eventObject)
        {
            var key = GetPolicyItemCacheKey(eventObject);
            
            // This method is called due to bug in .NET 
            // see https://connect.microsoft.com/VisualStudio/feedbackdetail/view/937655/memorycache-addorgetexisting-checks-wrong-expiration-date
            MemoryCache.Default.Get(key);
            
            return new CacheItem(key, eventObject.LoggingPolicy.Type.ToString());
        }


        /// <summary>
        /// Returns key value used for log policy cache items.
        /// </summary>
        /// <param name="eventObject">Event object.</param>
        private static string GetPolicyItemCacheKey(EventLogInfo eventObject)
        {
            return String.Join("|", "CMSLogPolicy", eventObject.EventType, eventObject.Source, eventObject.EventCode);
        }
    }
}
