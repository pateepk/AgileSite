namespace CMS.Activities
{
    internal class ActivityUrlInitializer : ActivityInitializerWrapperBase
    {
        private readonly string mUrl;


        /// <summary>
        /// Initializes new instance of <see cref="ActivitySiteIdInitializer"/> with provided <paramref name="url"/>.
        /// Other settings that <see cref="IActivityInfo.ActivityURL"/> of <paramref name="originalInitializer"/> is not changed.
        /// </summary>
        /// <param name="originalInitializer">Original activity initializer</param>
        /// <param name="url">URL to insert in activity</param>
        public ActivityUrlInitializer(IActivityInitializer originalInitializer, string url)
            : base(originalInitializer)
        {
            mUrl = url;
        }


        /// <summary>
        /// Calls method on original initializer and sets URL.
        /// </summary>
        /// <param name="activity"><see cref="IActivityInfo"/> to initialize</param>
        public override void Initialize(IActivityInfo activity)
        {
            OriginalInitializer.Initialize(activity);

            activity.ActivityURL = mUrl;
        }
    }
}