namespace CMS.Blogs
{
    /// <summary>
    /// Class providing blog properties.
    /// </summary>
    public class BlogProperties
    {
        #region "Variables"

        /// <summary>
        /// Indicates that comments are always opened after the blog post is published.
        /// </summary>
        public const int OPEN_COMMENTS_ALWAYS = -1;

        /// <summary>
        /// Indicates that comments are closed after the blog post is published.
        /// </summary>
        public const int OPEN_COMMENTS_DISABLE = 0;

        private int mOpenCommentsFor = OPEN_COMMENTS_ALWAYS;
        private string mSendCommentsToEmail = "";
        private bool mAllowAnonymousComments = true;
        private bool mUseCaptcha = true;
        private bool mShowDeleteButton = true;

        private int mUserPictureMaxWidth = 50;
        private int mUserPictureMaxHeight = 60;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates how long are comments opened.
        /// </summary>
        public int OpenCommentsFor
        {
            get
            {
                return mOpenCommentsFor;
            }
            set
            {
                mOpenCommentsFor = value;
            }
        }


        /// <summary>
        /// Indicates whether comments are moderated before publishing.
        /// </summary>
        public bool ModerateComments
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether 'Edit' button should be displayed in comment view while editing comments on the live site.
        /// </summary>
        public bool ShowEditButton
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether 'Delete' button should be displayed in comment view while editing comments on the live site.
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
        /// E-mail address the comments are sent to.
        /// </summary>
        public string SendCommentsToEmail
        {
            get
            {
                return mSendCommentsToEmail;
            }
            set
            {
                mSendCommentsToEmail = value;
            }
        }


        /// <summary>
        /// Indicates whether anonymous comments are allowed.
        /// </summary>
        public bool AllowAnonymousComments
        {
            get
            {
                return mAllowAnonymousComments;
            }
            set
            {
                mAllowAnonymousComments = value;
            }
        }


        /// <summary>
        /// Indicates whether security code should be entered when inserting new comment.
        /// </summary>
        public bool UseCaptcha
        {
            get
            {
                return mUseCaptcha;
            }
            set
            {
                mUseCaptcha = value;
            }
        }


        /// <summary>
        /// Indicates whether permissions should be checked.
        /// </summary>
        public bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether processing should be stopped.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether user pictures should be displayed in comment detail.
        /// </summary>
        public bool EnableUserPictures
        {
            get;
            set;
        }


        /// <summary>
        /// User picture max width.
        /// </summary>
        public int UserPictureMaxWidth
        {
            get
            {
                return mUserPictureMaxWidth;
            }
            set
            {
                mUserPictureMaxWidth = value;
            }
        }


        /// <summary>
        /// User picture max height.
        /// </summary>
        public int UserPictureMaxHeight
        {
            get
            {
                return mUserPictureMaxHeight;
            }
            set
            {
                mUserPictureMaxHeight = value;
            }
        }


        /// <summary>
        /// Indicates whether subscription is allowed.
        /// </summary>
        public bool EnableSubscriptions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether e-mail is required.
        /// </summary>
        public bool RequireEmails
        {
            get;
            set;
        }

        #endregion

    }
}