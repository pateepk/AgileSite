namespace CMS.Activities
{
    /// <summary>
    /// Provides possibility to initialize activity.
    /// </summary>
    public interface IActivityInitializer
    {
        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        void Initialize(IActivityInfo activity);


        /// <summary>
        /// Activity type
        /// </summary>
        string ActivityType
        {
            get;
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        string SettingsKeyName
        {
            get;
        }
    }
}
