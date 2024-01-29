namespace CMS.Activities
{
    internal class ActivityCampaignInitializer : ActivityInitializerWrapperBase
    {
        private readonly string mCampaign;


        /// <summary>
        /// Initializes new instance of <see cref="ActivitySiteIdInitializer"/> with provided <paramref name="campaign"/>.
        /// Other settings that <see cref="IActivityInfo.ActivityCampaign"/> of <paramref name="originalInitializer"/> is not changed.
        /// </summary>
        /// <param name="originalInitializer">Original activity initializer</param>
        /// <param name="campaign">Campaign to insert in activity</param>
        public ActivityCampaignInitializer(IActivityInitializer originalInitializer, string campaign)
            : base(originalInitializer)
        {
            mCampaign = campaign;
        }


        /// <summary>
        /// Calls method on original initializer and sets campaign.
        /// </summary>
        /// <param name="activity"><see cref="IActivityInfo"/> to initialize</param>
        public override void Initialize(IActivityInfo activity)
        {
            OriginalInitializer.Initialize(activity);

            activity.ActivityCampaign = mCampaign;
        }
    }
}