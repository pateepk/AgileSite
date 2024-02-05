using CMS.Activities;

namespace Tests.Activities
{ 
    /// <summary>
    /// Fake activity for test purposes.
    /// </summary>
    public class ActivityInitializerFake : IActivityInitializer
    {
        /// <summary>
        /// Specifies title used in <see cref="Initialize"/>.
        /// </summary>
        public const string ACTIVITY_TITLE = "Title";

        /// <summary>
        /// Specifies type returned by <see cref="ActivityType"/>.
        /// </summary>
        public const string ACTIVITY_TYPE = PredefinedActivityType.PAGE_VISIT;

        /// <summary>
        /// Specifies setting returned by <see cref="SettingsKeyName"/>.
        /// </summary>
        public const string SETTINGS_KEY_NAME = "CMSCMPageVisits";


        /// <summary>
        /// Sets <see cref="IActivityInfo.ActivityTitle"/> value to <see cref="ACTIVITY_TITLE"/> title for the given <paramref name="activity"/>.
        /// </summary>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = ACTIVITY_TITLE;
        }


        /// <summary>
        /// Value <see cref="PredefinedActivityType.PAGE_VISIT"/> is returned.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return ACTIVITY_TYPE;
            }
        }


        /// <summary>
        /// Value SettingsKeyName is returned.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return SETTINGS_KEY_NAME;
            }
        }
    }
}