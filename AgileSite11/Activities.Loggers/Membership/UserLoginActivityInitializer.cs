using CMS.Base;

namespace CMS.Activities.Loggers
{
    /// <summary>
    /// Activity user login.
    /// </summary>
    public class UserLoginActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly ITreeNode mCurrentDocument;
        private readonly int mCurrentContactId;
        private readonly IUserInfo mUserInfo;


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="userInfo">Info of current user</param>
        /// <param name="currentDocument">Current document in tree (can be null)</param>
        /// <param name="currentContactId">Current contact ID</param>
        public UserLoginActivityInitializer(IUserInfo userInfo, ITreeNode currentDocument, int currentContactId)
        {
            mUserInfo = userInfo;
            mCurrentDocument = currentDocument;
            mCurrentContactId = currentContactId;
        }


        /// <summary>
        /// Sets <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Builder for activity</param>
        /// <returns>New <see cref="IActivityInfo"/></returns>
        public void Initialize(IActivityInfo activity)
        {
            var titleData = new VisitorData
            {
                Email = mUserInfo.Email,
                FirstName = mUserInfo.FirstName,
                LastName = mUserInfo.LastName,
                MiddleName = mUserInfo.MiddleName,
                UserName = mUserInfo.UserName,
                ID = mUserInfo.UserID
            };

            activity.ActivityTitle = mTitleBuilder.CreateTitleWithUserName(ActivityType, titleData);
            activity.ActivityItemID = mUserInfo.UserID;
            activity.ActivityType = PredefinedActivityType.USER_LOGIN;
            activity.ActivityContactID = mCurrentContactId;

            if (mCurrentDocument != null)
            {
                activity.ActivityNodeID = mCurrentDocument.NodeID;
                activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            }
        }


        /// <summary>
        /// User login activity type
        /// </summary>
        public string ActivityType
        {
            get { return PredefinedActivityType.USER_LOGIN; }
        }


        /// <summary>
        /// User login activity settings key
        /// </summary>
        public string SettingsKeyName
        {
            get { return "CMSCMUserLogin"; }
        }
    }
}