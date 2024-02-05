using CMS.Activities;
using CMS.Base;

namespace CMS.WebAnalytics
{
    internal class InternalSearchActivityInitializer : IActivityInitializer
    {
        private readonly ITreeNode mCurrentDocument;
        private readonly string mKeyword;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Constructs <see cref="InternalSearchActivityInitializer"/> activity with provided parameters.
        /// </summary>
        /// <param name="searchKeyword">Search keyword</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        public InternalSearchActivityInitializer(string searchKeyword, ITreeNode currentDocument = null)
        {
            mKeyword = searchKeyword;
            mCurrentDocument = currentDocument;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mKeyword);
            activity.ActivityValue = mKeyword;

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
                return PredefinedActivityType.INTERNAL_SEARCH;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMSearch";
            }
        }
    }
}
