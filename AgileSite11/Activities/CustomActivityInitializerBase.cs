namespace CMS.Activities
{
    /// <summary>
    /// Represents base class for custom activities.
    /// </summary>
    public abstract class CustomActivityInitializerBase : IActivityInitializer
    {
        /// <summary>
        /// Initializes properties of <see cref="IActivityInfo"/>.
        /// </summary>
        /// <param name="activity">Activity info that should be initialized.</param>
        public abstract void Initialize(IActivityInfo activity);


        /// <summary>
        /// Represents custom activity code name.
        /// </summary>
        public abstract string ActivityType
        {
            get;
        }


        /// <summary>
        /// Provides settings key for custom activities.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMCustomActivities";
            }
        }
    }
}