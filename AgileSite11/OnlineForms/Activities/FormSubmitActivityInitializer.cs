using System;

using CMS.Activities;
using CMS.Base;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Represents implementation of <see cref="IActivityInitializer"/> for form submission activity.
    /// </summary>
    public class FormSubmitActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mActivityTitleBuilder = new ActivityTitleBuilder();
        private readonly BizFormItem mRecordItem;
        private readonly ITreeNode mCurrentDocument;


        /// <summary>
        /// Instantiate new instance of <see cref="FormSubmitActivityInitializer"/>.
        /// </summary>
        /// <param name="recordItem">Specifies the inserted record the activity is related to</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <exception cref="ArgumentNullException"><paramref name="recordItem"/> is <c>null</c></exception>
        public FormSubmitActivityInitializer(BizFormItem recordItem, ITreeNode currentDocument = null)
        {
            if (recordItem == null)
            {
                throw new ArgumentNullException("recordItem");
            }

            mRecordItem = recordItem;
            mCurrentDocument = currentDocument;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c></exception>
        public void Initialize(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            var form = mRecordItem.BizFormInfo;

            activity.ActivityItemID = form.FormID;
            activity.ActivityItemDetailID = mRecordItem.ItemID;
            activity.ActivityTitle = mActivityTitleBuilder.CreateTitle(ActivityType, form.FormDisplayName);

            if (mCurrentDocument != null)
            {
                activity.ActivityNodeID = mCurrentDocument.NodeID;
                activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            }
        }


        /// <summary>
        /// Gets string type of the activity (<see cref="PredefinedActivityType.BIZFORM_SUBMIT "/>).
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.BIZFORM_SUBMIT;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMBizFormSubmission";
            }
        }
    }
}
