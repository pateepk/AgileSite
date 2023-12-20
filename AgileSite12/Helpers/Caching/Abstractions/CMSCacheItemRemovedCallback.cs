namespace CMS.Helpers
{
    /// <summary>Defines a callback method for notifying applications when a cached item is removed from the cache.</summary>
	/// <param name="key">The key that is removed from the cache. </param>
	/// <param name="value">The item associated with the key removed from the cache. </param>
	/// <param name="reason">The reason the item was removed from the cache.</param>
    public delegate void CMSCacheItemRemovedCallback(string key, object value, CMSCacheItemRemovedReason reason);
}
