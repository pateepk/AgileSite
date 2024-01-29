using System;

using CMS.Activities;

using System.Globalization;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides initialization for RatingActivityInitializer subscribing activity.
    /// </summary>
    internal class RatingActivityInitializer : IActivityInitializer
    {
        private readonly TreeNode mCurrentDocument;
        private readonly double mValue;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Instantiates new instance of <see cref="RatingActivityInitializer"/>.
        /// </summary>
        /// <param name="value">Value of the rating to be logged</param>
        /// <param name="currentDocument">Specifies document the activity is logged for</param>
        /// <exception cref="ArgumentNullException"><paramref name="currentDocument"/> is <c>null</c></exception>
        public RatingActivityInitializer(double value, TreeNode currentDocument)
        {
            if (currentDocument == null)
            {
                throw new ArgumentNullException("currentDocument");
            }

            mCurrentDocument = currentDocument;
            mValue = value;
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

            activity.ActivityValue = mValue.ToString(CultureInfo.InvariantCulture);
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, string.Format("{0} ({1})", mValue, mCurrentDocument.GetDocumentName()));
            activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            activity.ActivityNodeID = mCurrentDocument.NodeID;
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.RATING;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMContentRating";
            }
        }
    }
}