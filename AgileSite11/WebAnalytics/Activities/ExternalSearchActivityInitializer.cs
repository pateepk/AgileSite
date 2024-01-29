using CMS.Activities;
using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents initializer for external search activity.
    /// </summary>
    internal class ExternalSearchActivityInitializer : IActivityInitializer
    {
        private readonly string mSearchKeyword;
        private readonly ITreeNode mCurrentDocument;
        private readonly string mActivityUrl;
        private readonly string mReferrerUrl;
        private readonly ActivityTitleBuilder mActivityTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="ExternalSearchActivityInitializer"/>.
        /// </summary>
        /// <param name="searchKeyword">Search keyword</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        public ExternalSearchActivityInitializer(string searchKeyword, ITreeNode currentDocument = null, string activityUrl = null, string referrerUrl = null)
        {
            mSearchKeyword = searchKeyword;
            mCurrentDocument = currentDocument;
            mActivityUrl = activityUrl;
            mReferrerUrl = referrerUrl;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity object</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mActivityTitleBuilder.CreateTitle(ActivityType, mSearchKeyword);
            activity.ActivityURL = mActivityUrl;
            activity.ActivityURLReferrer = mReferrerUrl;
            activity.ActivityValue = mSearchKeyword;

            if (mCurrentDocument != null)
            {
                activity.ActivityCulture = mCurrentDocument.DocumentCulture;
                activity.ActivityNodeID = mCurrentDocument.NodeID;
            }
        }


        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.EXTERNAL_SEARCH;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMExternalSearch";
            }
        }
    }
}