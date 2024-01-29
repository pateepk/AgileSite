using CMS;
using CMS.EventLog;
using CMS.Helpers;

[assembly: RegisterCustomHelper(typeof(CustomCacheHelper))]

/// <summary>
/// Sample custom cache helper, does log an event upon cache item removal.
/// </summary>
public class CustomCacheHelper : CacheHelper
{
    /// <summary>
    /// Removes the item from the cache.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="useFullKey">If true, full cache key is used</param>
    protected override object RemoveInternal(string key, bool useFullKey)
    {
        // Log the event that the user was updated
        EventLogProvider.LogEvent(EventType.INFORMATION, "MyCustomCacheHelper", "Remove", "The cache item was removed", null);

        return base.RemoveInternal(key, useFullKey);
    }
}