namespace CMS.Newsletters
{
    /// <summary>
    /// Defines method to detect bounced subscriber.
    /// </summary>
    internal interface IBounceDetection
    {
        /// <summary>
        /// Returns true if given <paramref name="subscriber"/> is bounced.
        /// </summary>
        /// <param name="subscriber">Subscriber to check.</param>
        bool IsBounced(SubscriberInfo subscriber);
    }
}
