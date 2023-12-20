using CMS.Helpers;
using CMS.UIControls;

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Class with blog comment data.
    /// </summary>
    public class BlogCommentDetail : CMSUserControl
    {
        /// <summary>
        /// Indicates whether the EDIT button would be displayed.
        /// </summary>
        protected bool mShowEditButton;

        /// <summary>
        /// Indicates whether the DELETE button would be displayed.
        /// </summary>
        protected bool mShowDeleteButton;

        /// <summary>
        /// Indicates whether the APPROVE button would be displayed.
        /// </summary>
        protected bool mShowApproveButton;

        /// <summary>
        /// Indicates whether the REJECT button would be displayed.
        /// </summary>
        protected bool mShowRejectButton;

        /// <summary>
        /// Roles possible to report abuse.
        /// </summary>
        protected string mAbuseReportRoles;

        /// <summary>
        /// Specifes what kind of users are able to report abuse.
        /// </summary>
        protected SecurityAccessEnum mAbuseReportSecurityAccess = SecurityAccessEnum.AllUsers;

        /// <summary>
        /// Default ID of the abuse report owner.
        /// </summary>
        protected int mAbuseReportOwnerID;

        /// <summary>
        /// Object holding information on current blog properties.
        /// </summary>
        public BlogProperties BlogProperties = new BlogProperties();


        /// <summary>
        /// Event fired when some kind of action related to the blog comment occurs.
        /// </summary>
        public event OnCommentActionEventHandler OnCommentAction;


        /// <summary>
        /// Comment data.
        /// </summary>
        public BlogCommentInfo Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether 'edit' button should be displayed.
        /// </summary>
        public bool ShowEditButton
        {
            get
            {
                return mShowEditButton;
            }
            set
            {
                mShowEditButton = value;
            }
        }


        /// <summary>
        /// Indicates whether 'delete' button should be displayed.
        /// </summary>
        public bool ShowDeleteButton
        {
            get
            {
                return mShowDeleteButton;
            }
            set
            {
                mShowDeleteButton = value;
            }
        }


        /// <summary>
        /// Indicates whether 'approve' button should be displayed.
        /// </summary>
        public bool ShowApproveButton
        {
            get
            {
                return mShowApproveButton;
            }
            set
            {
                mShowApproveButton = value;
            }
        }


        /// <summary>
        /// Indicates whether 'reject' button should be displayed.
        /// </summary>
        public bool ShowRejectButton
        {
            get
            {
                return mShowRejectButton;
            }
            set
            {
                mShowRejectButton = value;
            }
        }


        /// <summary>
        /// Gets or sets list of roles (separated by ';') which are allowed to report abuse (in combination with SecurityAccess.AuthorizedRoles).
        /// </summary>
        public string AbuseReportRoles
        {
            get
            {
                return mAbuseReportRoles;
            }
            set
            {
                mAbuseReportRoles = value;
            }
        }


        /// <summary>
        /// Gets or sets the security access for report abuse link.
        /// </summary>
        public SecurityAccessEnum AbuseReportSecurityAccess
        {
            get
            {
                return mAbuseReportSecurityAccess;
            }
            set
            {
                mAbuseReportSecurityAccess = value;
            }
        }


        /// <summary>
        /// Gets or sets the owner ID (in combination with SecurityAccess.Owner).
        /// </summary>
        public int AbuseReportOwnerID
        {
            get
            {
                return mAbuseReportOwnerID;
            }
            set
            {
                mAbuseReportOwnerID = value;
            }
        }


        /// <summary>
        /// Fires event when some blog comment action occurs.
        /// </summary>
        /// <param name="actionName">Name of the action that takes place</param>
        /// <param name="actionArgument">Argument of the action</param>
        protected void FireOnCommentAction(string actionName, object actionArgument)
        {
            if (OnCommentAction != null)
            {
                OnCommentAction(actionName, actionArgument);
            }
        }
    }
}