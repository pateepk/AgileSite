using System;

using CMS.Helpers;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Summary description for BoardProperties.
    /// </summary>
    public class BoardProperties
    {
        #region "Private fields"

        // Fields
        private bool mShowNameField = true;
        private bool mShowURLField = true;
        private bool mShowEmailField = true;        
        
        // Board properties
        private string mBoardModerators = "";
        private string mBoardRoles = "";
        private string mBoardName = "";
        private string mBoardDisplayName = "";
        private string mBoardDescription = "";
        private string mBoardOwner = "";
        private DateTime mBoardOpenedFrom = DateTimeHelper.ZERO_TIME;
        private DateTime mBoardOpenedTo = DateTimeHelper.ZERO_TIME;
        private SecurityAccessEnum mBoardAccess = SecurityAccessEnum.AllUsers;
        private string mBoardUnsubscriptionUrl = "";
        private string mBoardBaseUrl = "";

        // Rating
        private int mMaxRatingValue = 10;
        private string mRatingType = "Stars";
        private bool mAllowEmptyRating = true;

        #endregion


        #region "Public properties - New message form fields"

        /// <summary>
        /// Indicates if input field for entering user's name should be displayed.
        /// </summary>
        public bool ShowNameField
        {
            get
            {
                return mShowNameField;
            }
            set
            {
                mShowNameField = value;
            }
        }


        /// <summary>
        /// Indicates if input field for entering user's URL should be displayed.
        /// </summary>
        public bool ShowURLField
        {
            get
            {
                return mShowURLField;
            }
            set
            {
                mShowURLField = value;
            }
        }


        /// <summary>
        /// Indicates if input field for entering user's e-mail address should be displayed.
        /// </summary>
        public bool ShowEmailField
        {
            get
            {
                return mShowEmailField;
            }
            set
            {
                mShowEmailField = value;
            }
        }

        #endregion


        #region "Public properties - Rating"

        /// <summary>
        /// Enables content rating scale for each message.
        /// </summary>
        public bool EnableContentRating
        {
            get;
            set;
        }


        /// <summary>
        /// Max value (length) of content rating scale.
        /// </summary>
        public int MaxRatingValue
        {
            get
            {
                return mMaxRatingValue;
            }
            set
            {
                mMaxRatingValue = value;
            }
        }


        /// <summary>
        /// Type of content rating scale.
        /// </summary>
        public string RatingType
        {
            get
            {
                return mRatingType;
            }
            set
            {
                mRatingType = value;
            }
        }


        /// <summary>
        /// Indicates if it is allowed to add message without rating.
        /// </summary>
        public bool AllowEmptyRating
        {
            get
            {
                return mAllowEmptyRating;
            }
            set
            {
                mAllowEmptyRating = value;
            }
        }


        /// <summary>
        /// If checked, users can rate only once here.
        /// </summary>
        public bool CheckIfUserRated
        {
            get;
            set;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current board name.
        /// </summary>
        public string BoardName
        {
            get
            {
                return mBoardName;
            }
            set
            {
                mBoardName = value;
            }
        }


        /// <summary>
        /// Current board display name.
        /// </summary>
        public string BoardDisplayName
        {
            get
            {
                return mBoardDisplayName;
            }
            set
            {
                mBoardDisplayName = value;
            }
        }


        /// <summary>
        /// Default board authorized roles.
        /// </summary>
        public string BoardRoles
        {
            get
            {
                return mBoardRoles;
            }
            set
            {
                mBoardRoles = value;
            }
        }


        /// <summary>
        /// Current board description.
        /// </summary>
        public string BoardDescription
        {
            get
            {
                return mBoardDescription;
            }
            set
            {
                mBoardDescription = value;
            }
        }


        /// <summary>
        /// Owner of the board.
        /// </summary>
        public string BoardOwner
        {
            get
            {
                return mBoardOwner;
            }
            set
            {
                mBoardOwner = value;
            }
        }


        /// <summary>
        /// Indicates whether board is opened.
        /// </summary>
        public bool BoardOpened
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates type of board access.
        /// </summary>
        public SecurityAccessEnum BoardAccess
        {
            get
            {
                return mBoardAccess;
            }
            set
            {
                mBoardAccess = value;
            }
        }


        /// <summary>
        /// Indicates the board message post requires e-mail.
        /// </summary>
        public bool BoardRequireEmails
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the e-mails should be displayed with messages.
        /// </summary>
        public bool BoardShowEmails
        {
            get;
            set;
        }


        /// <summary>
        /// Board opened from.
        /// </summary>
        public DateTime BoardOpenedFrom
        {
            get
            {
                return mBoardOpenedFrom;
            }
            set
            {
                mBoardOpenedFrom = value;
            }
        }


        /// <summary>
        /// Board opened to.
        /// </summary>
        public DateTime BoardOpenedTo
        {
            get
            {
                return mBoardOpenedTo;
            }
            set
            {
                mBoardOpenedTo = value;
            }
        }


        /// <summary>
        /// Indicates whether the subscriptions are allowed for the current board.
        /// </summary>
        public bool BoardEnableSubscriptions
        {
            get;
            set;
        }


        /// <summary>
        /// Board unsubscription URL.
        /// </summary>
        public string BoardUnsubscriptionUrl
        {
            get
            {
                return mBoardUnsubscriptionUrl;
            }
            set
            {
                mBoardUnsubscriptionUrl = value;
            }
        }


        /// <summary>
        /// Board base URL.
        /// </summary>
        public string BoardBaseUrl
        {
            get
            {
                return mBoardBaseUrl;
            }
            set
            {
                mBoardBaseUrl = value;
            }
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
        /// Indicates whether the EDIT button should be displayed.
        /// </summary>
        public bool ShowEditButton
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the DELETE button should be displayed.
        /// </summary>
        public bool ShowDeleteButton
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the APPROVE button should be displayed.
        /// </summary>
        public bool ShowApproveButton
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the REJECT button should be displayed.
        /// </summary>
        public bool ShowRejectButton
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the board is moderated.
        /// </summary>
        public bool BoardModerated
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the permissions should be checked.
        /// </summary>
        public bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the CAPTCHA should be used.
        /// </summary>
        public bool BoardUseCaptcha
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the existing messages should be displayed for the anonymous users.
        /// </summary>
        public bool BoardEnableAnonymousRead
        {
            get;
            set;
        }


        /// <summary>
        /// Default board moderators.
        /// </summary>
        public string BoardModerators
        {
            get
            {
                return mBoardModerators;
            }
            set
            {
                mBoardModerators = value;
            }
        }


        /// <summary>
        /// Indicates whether logging activity is performed.
        /// </summary>
        public bool BoardLogActivity
        {
            get;
            set;
        }

        #endregion
    }
}