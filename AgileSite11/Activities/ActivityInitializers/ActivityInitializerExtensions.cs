namespace CMS.Activities
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityInitializer"/> for specifying the info's fields.
    /// These methods are designed to be used as chained calls. 
    /// </summary>
    public static class ActivityInitializerExtensions
    {
        /// <summary>
        /// Extends the provided <paramref name="activityInitializer"/> with <paramref name="contactId"/>.
        /// </summary>
        /// <param name="activityInitializer">Initializer to extend</param>
        /// <param name="contactId">Contact id to be saved in <paramref name="activityInitializer"/></param>
        /// <returns>Extended <paramref name="activityInitializer"/></returns>
        public static IActivityInitializer WithContactId(this IActivityInitializer activityInitializer, int contactId)
        {
            return new ActivityContactIdInitializer(activityInitializer, contactId);
        }


        /// <summary>
        /// Extends the provided <paramref name="activityInitializer"/> with <paramref name="siteId"/>.
        /// </summary>
        /// <param name="activityInitializer">Initializer to extend</param>
        /// <param name="siteId">Site id to be saved in <paramref name="activityInitializer"/></param>
        /// <returns>Extended <paramref name="activityInitializer"/></returns>
        public static IActivityInitializer WithSiteId(this IActivityInitializer activityInitializer, int siteId)
        {
            return new ActivitySiteIdInitializer(activityInitializer, siteId);
        }


        /// <summary>
        /// Extends the provided <paramref name="activityInitializer"/> with <paramref name="campaign"/>.
        /// </summary>
        /// <param name="activityInitializer">Initializer to extend</param>
        /// <param name="campaign">Campaign to be saved in <paramref name="activityInitializer"/></param>
        /// <returns>Extended <paramref name="activityInitializer"/></returns>
        public static IActivityInitializer WithCampaign(this IActivityInitializer activityInitializer, string campaign)
        {
            return new ActivityCampaignInitializer(activityInitializer, campaign);
        }


        /// <summary>
        /// Extends the provided <paramref name="activityInitializer"/> with <paramref name="url"/>.
        /// </summary>
        /// <param name="activityInitializer">Initializer to extend</param>
        /// <param name="url">URL to be saved in <paramref name="activityInitializer"/></param>
        /// <returns>Extended <paramref name="activityInitializer"/></returns>
        public static IActivityInitializer WithUrl(this IActivityInitializer activityInitializer, string url)
        {
            return new ActivityUrlInitializer(activityInitializer, url);
        }
    }
}
