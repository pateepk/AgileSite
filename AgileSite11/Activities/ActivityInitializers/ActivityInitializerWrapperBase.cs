namespace CMS.Activities
{
    /// <summary>
    /// Provides wrapper for activity type and activity setting key for <see cref="IActivityInitializer"/>.
    /// </summary>
    public abstract class ActivityInitializerWrapperBase : IActivityInitializer
    {
        /// <summary>
        /// Original <see cref="IActivityInitializer"/>.
        /// </summary>
        protected IActivityInitializer OriginalInitializer
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructs new instance of <see cref="ActivityInitializerWrapperBase"/>.
        /// </summary>
        /// <param name="originalInitializer">Original activity initializer</param>
        protected ActivityInitializerWrapperBase(IActivityInitializer originalInitializer)
        {
            OriginalInitializer = originalInitializer;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public abstract void Initialize(IActivityInfo activity);


        /// <summary>
        /// Original activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return OriginalInitializer.ActivityType;
            }
        }


        /// <summary>
        /// Original settings key.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return OriginalInitializer.SettingsKeyName;
            }
        }
    }
}