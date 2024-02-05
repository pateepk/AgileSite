using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine.Internal;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// SessionInfo data container class.
    /// </summary>
    public class SessionInfo : IDataTransferObject
    {
        #region "Variables"

        private string mSessionIdentifier;
        private int mSessionUserID;
        private string mSessionLocation;
        private DateTime mSessionLastActive;
        private DateTime mSessionLastLogon;
        private DateTime mSessionExpires;
        private bool mSessionExpired;
        private bool mUpdated;
        private int mSessionSiteID;
        private bool mSessionUserIsHidden;
        private string mSessionFullName;
        private string mSessionEmail;
        private string mSessionUserName;
        private string mSessionNickName;
        private DateTime mSessionUserCreated;
        private int mSessionContactID;

        #endregion


        #region "Properties"

        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionIdentifier
        {
            get
            {
                return mSessionIdentifier;
            }
            set
            {
                mUpdated = true;
                mSessionIdentifier = value;
            }
        }


        /// <summary>
        /// Session user id.
        /// </summary>
        public int SessionUserID
        {
            get
            {
                return mSessionUserID;
            }
            set
            {
                mUpdated = true;
                mSessionUserID = value;
            }
        }


        /// <summary>
        /// Session location.
        /// </summary>
        public string SessionLocation
        {
            get
            {
                return mSessionLocation;
            }
            set
            {
                mUpdated = true;
                mSessionLocation = value;
            }
        }


        /// <summary>
        /// Session last active.
        /// </summary>
        public DateTime SessionLastActive
        {
            get
            {
                return mSessionLastActive;
            }
            set
            {
                mUpdated = true;
                mSessionLastActive = value;
            }
        }


        /// <summary>
        /// Session last logon.
        /// </summary>
        public DateTime SessionLastLogon
        {
            get
            {
                return mSessionLastLogon;
            }
            set
            {
                mUpdated = true;
                mSessionLastLogon = value;
            }
        }


        /// <summary>
        /// Session expires.
        /// </summary>
        public DateTime SessionExpires
        {
            get
            {
                return mSessionExpires;
            }
            set
            {
                mUpdated = true;
                mSessionExpires = value;
            }
        }


        /// <summary>
        /// Session expired.
        /// </summary>
        public bool SessionExpired
        {
            get
            {
                return mSessionExpired;
            }
            set
            {
                mUpdated = true;
                mSessionExpired = value;
            }
        }


        /// <summary>
        /// Session site id.
        /// </summary>
        public int SessionSiteID
        {
            get
            {
                return mSessionSiteID;
            }
            set
            {
                mUpdated = true;
                mSessionSiteID = value;
            }
        }


        /// <summary>
        /// If User is hidden.
        /// </summary>
        public bool SessionUserIsHidden
        {
            get
            {
                return mSessionUserIsHidden;
            }
            set
            {
                mSessionUserIsHidden = value;
            }
        }


        /// <summary>
        /// On-line visitor's full name (user or contact).
        /// </summary>
        public string SessionFullName
        {
            get
            {
                return mSessionFullName;
            }
            set
            {
                mSessionFullName = value;
                mUpdated = true;
            }
        }


        /// <summary>
        /// On-line visitor's e-mail (user or contact).
        /// </summary>
        public string SessionEmail
        {
            get
            {
                return mSessionEmail;
            }
            set
            {
                mSessionEmail = value;
                mUpdated = true;
            }
        }


        /// <summary>
        /// On-line visitor's user name.
        /// </summary>
        public string SessionUserName
        {
            get
            {
                return mSessionUserName;
            }
            set
            {
                mSessionUserName = value;
                mUpdated = true;
            }
        }


        /// <summary>
        /// On-line visitor's user nick name
        /// </summary>
        public string SessionNickName
        {
            get
            {
                return mSessionNickName;
            }
            set
            {
                mSessionNickName = value;
                mUpdated = true;
            }
        }


        /// <summary>
        /// On-line visitor's created time (user or contact).
        /// </summary>
        public DateTime SessionUserCreated
        {
            get
            {
                return mSessionUserCreated;
            }
            set
            {
                mSessionUserCreated = value;
                mUpdated = true;
            }
        }



        /// <summary>
        /// On-line visitor's contact ID.
        /// </summary>
        public int SessionContactID
        {
            get
            {
                return mSessionContactID;
            }
            set
            {
                mSessionContactID = value;
                mUpdated = true;
            }
        }


        /// <summary>
        /// Indicates if record was updated.
        /// </summary>
        public bool Updated
        {
            get
            {
                return mUpdated;
            }
        }


        /// <summary>
        /// Returns true when object changes should be saved to database.
        /// </summary>
        internal bool IsUpdateRequired
        {
            get
            {
                return Updated && (SessionSiteID > 0);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Removes the <see cref="Updated"/> flag
        /// </summary>
        internal void UnsetUpdate()
        {
            mUpdated = false;
        }


        /// <summary>
        /// Updates the session object with current context values.
        /// </summary>
        public void LoadFromContext()
        {
            var currentUser = CMSActionContext.CurrentUser;
            
            SessionSiteID = SiteContext.CurrentSiteID;
            SessionUserID = GetUserID(currentUser);
            SessionUserName = GetSessionUserName(currentUser);
            SessionNickName = GetSessionNickName(currentUser);
            
            BaseInfo currentContact = GetExistingContact();
            
            SessionContactID = GetContactID(currentContact);
            SessionFullName = GetSessionFullName(currentUser, currentContact);
            SessionEmail = GetSessionEmail(currentUser, currentContact);
            SessionUserCreated = GetSessionUserCreated(currentUser, currentContact);
        }


        /// <summary>
        /// Returns current UserID.
        /// </summary>
        private int GetUserID(IUserInfo currentUser)
        {
            if (AuthenticationHelper.IsAuthenticated())
            {
                return currentUser.UserID;
            }
            return 0;
        }


        /// <summary>
        /// Returns current contact if such exists.
        /// </summary>
        private BaseInfo GetExistingContact()
        {
            return ModuleCommands.OnlineMarketingGetExistingContact();
        }


        /// <summary>
        /// Returns current ContactID.
        /// </summary>
        private int GetContactID(BaseInfo currentContact)
        {
            if (currentContact != null)
            {
                return ValidationHelper.GetInteger(currentContact["ContactID"], 0);
            }
            return 0;
        }


        /// <summary>
        /// Returns full name.
        /// </summary>
        private string GetSessionFullName(IUserInfo currentUser, BaseInfo currentContact)
        {
            string sessionFullName = null;
            if ((currentUser != null) && ((currentUser.UserCreated != DateTimeHelper.ZERO_TIME) || currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)))
            {
                sessionFullName = currentUser.FullName;
            }
            if ((currentContact != null) && String.IsNullOrEmpty(sessionFullName))
            {
                string fullName = (string)currentContact["ContactFirstName"];
                fullName += " " + currentContact["ContactMiddleName"];
                fullName = fullName.Trim();
                fullName += " " + currentContact["ContactLastName"];
                sessionFullName = fullName.Trim();
            }

            return String.IsNullOrEmpty(sessionFullName) ? SessionFullName : sessionFullName;
        }


        /// <summary>
        /// Returns email.
        /// </summary>
        private string GetSessionEmail(IUserInfo currentUser, BaseInfo currentContact)
        {
            string sessionEmail = null;
            if (!String.IsNullOrEmpty(currentUser?.Email))
            {
                sessionEmail = currentUser.Email;
            }

            if ((currentContact != null) && String.IsNullOrEmpty(sessionEmail))
            {
                string email = ValidationHelper.GetString(currentContact["ContactEmail"], null);
                if (!String.IsNullOrEmpty(email))
                {
                    sessionEmail = email;
                }
            }

            return String.IsNullOrEmpty(sessionEmail) ? SessionEmail : sessionEmail;
        }


        /// <summary>
        /// Returns user name.
        /// </summary>
        private string GetSessionUserName(IUserInfo currentUser)
        {
            string sessionUserName = null;
            if (!String.IsNullOrEmpty(currentUser?.UserName) && ((currentUser.UserCreated != DateTimeHelper.ZERO_TIME) || currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)))
            {
                sessionUserName = currentUser.UserName;
            }

            return String.IsNullOrEmpty(sessionUserName) ? SessionUserName : sessionUserName;
        }


        /// <summary>
        /// Returns user nickname.
        /// </summary>
        private string GetSessionNickName(IUserInfo currentUser)
        {
            string sessionNickName = null;
            if (!String.IsNullOrEmpty(currentUser?.UserNickName))
            {
                sessionNickName = currentUser.UserNickName;
            }

            return String.IsNullOrEmpty(sessionNickName) ? SessionNickName : sessionNickName;
        }


        /// <summary>
        /// Returns date and time of user's or contact's creation.
        /// </summary>
        private DateTime GetSessionUserCreated(IUserInfo currentUser, BaseInfo currentContact)
        {
            if ((currentUser != null) && (currentUser.UserCreated != DateTimeHelper.ZERO_TIME))
            {
                return currentUser.UserCreated;
            }

            if (currentContact != null)
            {
                DateTime date = ValidationHelper.GetDateTime(currentContact["ContactCreated"], DateTimeHelper.ZERO_TIME);
                if (date != DateTimeHelper.ZERO_TIME)
                {
                    return date;
                }
            }

            return DateTimeHelper.ZERO_TIME;
        }


        /// <summary>
        /// Fills given <paramref name="dataContainer"/> with values from current object.
        /// </summary>
        /// <param name="dataContainer">Datarow to be filled</param>
        public void FillDataContainer(IDataContainer dataContainer)
        {
            dataContainer["SessionContactID"] = SessionContactID;
            dataContainer["SessionEmail"] = SessionEmail;
            dataContainer["SessionExpired"] = SessionExpired;
            dataContainer["SessionExpires"] = SessionExpires;
            dataContainer["SessionFullName"] = SessionFullName;
            dataContainer["SessionIdentificator"] = SessionIdentifier;
            dataContainer["SessionLastActive"] = SessionLastActive;
            dataContainer["SessionLastLogon"] = SessionLastLogon;
            dataContainer["SessionLocation"] = SessionLocation;
            dataContainer["SessionNickName"] = SessionNickName;
            dataContainer["SessionSiteID"] = SessionSiteID;
            dataContainer["SessionUserCreated"] = SessionUserCreated;
            dataContainer["SessionUserID"] = SessionUserID > 0 ? SessionUserID : (int?)null;
            dataContainer["SessionUserIsHidden"] = SessionUserIsHidden;
            dataContainer["SessionUserName"] = SessionUserName;
        }

        #endregion
    }
}