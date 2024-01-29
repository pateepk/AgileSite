using CMS.Activities;
using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Initializes new Landing page activity.
    /// </summary>
    internal class LandingPageActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly ITreeNode mCurrentDocument;
        private readonly string mDocumentname;
        private readonly string mActivityUrl;
        private readonly string mReferrerUrl;


        /// <summary>
        /// Initializes new instance of <see cref="LandingPageActivityInitializer"/>.
        /// </summary>
        /// <param name="documentname">Name of document where activity occurred</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="activityUrl">Url where activity occurred</param>
        /// <param name="referrerUrl">Url referrer</param>
        public LandingPageActivityInitializer(string documentname, ITreeNode currentDocument = null, string activityUrl = null, string referrerUrl = null)
        {
            mCurrentDocument = currentDocument;
            mDocumentname = documentname;
            mActivityUrl = activityUrl;
            mReferrerUrl = referrerUrl;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mDocumentname);
            activity.ActivityURL = mActivityUrl;
            activity.ActivityURLReferrer = mReferrerUrl;

            if (mCurrentDocument != null)
            {
                activity.ActivityNodeID = mCurrentDocument.NodeID;
                activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            }
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.LANDING_PAGE;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMLandingPage";
            }
        }
    }
}