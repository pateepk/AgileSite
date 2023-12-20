using System;

namespace CMS.Helpers
{
    internal static class CacheConstants
    {
        /// <summary>Cache item should never expire.</summary>
        public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;

        /// <summary>Disable sliding expirations.</summary>
        public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;
    }
}
