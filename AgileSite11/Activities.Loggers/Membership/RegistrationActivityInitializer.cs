using CMS.Base;

namespace CMS.Activities.Loggers
{
    /// <summary>
    /// Activity registration.
    /// </summary>
    public class RegistrationActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly IUserInfo mUserInfo;
        private readonly ITreeNode mCurrentDocument;
        private readonly int mCurrentContactId;


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="userInfo">Info of current user</param>
        /// <param name="currentDocument">Current document in tree (can be null)</param>
        /// <param name="currentContactId">Current contact ID</param>
        public RegistrationActivityInitializer(IUserInfo userInfo, ITreeNode currentDocument, int currentContactId)
        {
            mUserInfo = userInfo;
            mCurrentDocument = currentDocument;
            mCurrentContactId = currentContactId;
        }


        /// <summary>
        /// Sets <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Builder for activity</param>
        public void Initialize(IActivityInfo activity)
        {
            var visitorData = new VisitorData
            {
                Email = mUserInfo.Email,
                FirstName = mUserInfo.FirstName,
                LastName = mUserInfo.LastName,
                MiddleName = mUserInfo.MiddleName,
                UserName = mUserInfo.UserName,
                ID = mUserInfo.UserID
            };
            
            activity.ActivityTitle = mTitleBuilder.CreateTitleWithUserName(ActivityType, visitorData);
            activity.ActivityItemID = mUserInfo.UserID;
            activity.ActivityType = PredefinedActivityType.REGISTRATION;
            activity.ActivityContactID = mCurrentContactId;

            if (mCurrentDocument != null)
            {
                activity.ActivityNodeID = mCurrentDocument.NodeID;
                activity.ActivityCulture = mCurrentDocument.DocumentCulture;
            }
        }


        /// <summary>
        /// Registration activity type
        /// </summary>
        public string ActivityType
        {
            get { return PredefinedActivityType.REGISTRATION; }
        }


        /// <summary>
        /// Registration activity settings key
        /// </summary>
        public string SettingsKeyName
        {
            get { return "CMSCMUserRegistration"; }
        }
    }
}