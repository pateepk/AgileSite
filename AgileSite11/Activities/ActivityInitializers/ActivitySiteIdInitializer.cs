namespace CMS.Activities
{
    /// <summary>
    /// Initializes activity with site id. All other members are delegated to provided initializer.
    /// </summary>
    internal class ActivitySiteIdInitializer : ActivityInitializerWrapperBase
    {
        private readonly int mSiteId;


        /// <summary>
        /// Initializes new instance of <see cref="ActivitySiteIdInitializer"/> with provided <paramref name="siteId"/>.
        /// Other settings that <see cref="IActivityInfo.ActivitySiteID"/> of <paramref name="originalInitializer"/> is not changed.
        /// </summary>
        /// <param name="originalInitializer">Original activity initializer</param>
        /// <param name="siteId">Site id to insert in activity</param>
        public ActivitySiteIdInitializer(IActivityInitializer originalInitializer, int siteId)
            : base(originalInitializer)
        {
            mSiteId = siteId;
        }


        /// <summary>
        /// Calls method on original initializer and sets siteId.
        /// </summary>
        /// <param name="activity"><see cref="IActivityInfo"/> to initialize</param>
        public override void Initialize(IActivityInfo activity)
        {
            OriginalInitializer.Initialize(activity);

            activity.ActivitySiteID = mSiteId;
        }
    }
}