using CMS.Activities;
using CMS.Base;

namespace CMS.WebAnalytics
{
    internal class PageVisitActivityInitializer : IActivityInitializer
    {
        private readonly ITreeNode mCurrentDocument;
        private readonly string mAttachmentName;
        private readonly string mDocumentName;
        private readonly string mActivityUrl;
        private readonly string mReferrerUrl;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Constructor for activity initializer of <see cref="PredefinedActivityType.PAGE_VISIT"/> type.
        /// </summary>
        /// <param name="documentName">Name of the page</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="attachmentName">Attachment in the page; optional</param>
        /// <param name="activityUrl">Url where activity occurred</param>
        /// <param name="referrerUrl">Url referrer</param>
        public PageVisitActivityInitializer(string documentName, ITreeNode currentDocument = null, string attachmentName = null, string activityUrl = null, string referrerUrl = null)
        {
            mDocumentName = documentName;
            mCurrentDocument = currentDocument;
            mAttachmentName = attachmentName;
            mActivityUrl = activityUrl;
            mReferrerUrl = referrerUrl;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mDocumentName);
            activity.ActivityValue = mAttachmentName;
            activity.ActivityURL = mActivityUrl;
            activity.ActivityURLReferrer = mReferrerUrl;
            
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
                return PredefinedActivityType.PAGE_VISIT;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMPageVisits";
            }
        }
    }
}
