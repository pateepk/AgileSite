namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// When UserManager is modified and deploy to prod, please clear ASPStateTempSessions table
    /// because old UserManager might be cache for each individual user in it
    /// make sure you deploy at good time
    /// </summary>
    [Serializable]
    public class UserManager
    {
        public bool isNeedLogin { get; set; }
        public bool isLogin { get; set; }
        public int userID { get; set; }
        public string windowsUsername { get; set; }
        public string loginID { get; set; }
        public string FullName { get; set; }
        public string BackdoorGUID { get; set; }
        public string Email { get; set; }
        public string CustNum { get; set; } // Customer Number for ExcellaLite
        public roleIDs[] userRoles { get; set; }
        public string userRolesDescription { get; set; }
        public bool isimpersonating { get; set; }
        public string[] userSettingsName { get; set; }
        public string[] userSettingsValue { get; set; }
        private bool isusingtouchscreen { get; set; }
        private string visitorstate { get; set; } // State from current location of the browser

        /// <summary>
        /// For External User login. Password will be  encrypted then pass to SQL. Do not decrypt any password at all in the code
        /// </summary>
        public bool AuthorizeUser(string LoginID, string Password, string BackdoorGUID)
        {
            bool isAuthorized = false;
            DRspUser_AuthorizeExternalLogin ul = SQLData.spUser_AuthorizeExternalLogin(LoginID, EncryptionManager.EncryptExternalPassword(Password), BackdoorGUID);
            // authorized
            if ((ul != null) && (!ul.isError) && (ul.UserID > 0))
            {
                onLogin(ul.UserID, ul.LoginID, ul.FullName, ul.Email, ul.RoleIDs, ul.CustNum, ul.BackdoorGUID);
                isAuthorized = true;
            }
            return isAuthorized;
        }

        public bool AuthorizeUserDontLoginAndCache(string LoginID, string Password)
        {
            bool isAuthorized = false;
            if (LoginID.Length > 0)
            {
                string cacheName = String.Format(SV.Applications.LoginIDAndPassword, LoginID);

                object lp = AppManager.getApplicationObject(cacheName);
                if ((lp != null) && (lp.ToString() == Password))
                {
                    isAuthorized = true;
                }
                else
                {
                    DRspUser_AuthorizeExternalLogin ul = SQLData.spUser_AuthorizeExternalLogin(LoginID, EncryptionManager.EncryptExternalPassword(Password), string.Empty);
                    // authorized
                    if ((ul != null) && (!ul.isError) && (ul.UserID > 0))
                    {
                        AppManager.setApplicationObject(cacheName, Password);
                        isAuthorized = true;
                    }
                }
            }
            return isAuthorized;
        }

        public void onLogin(int UserID, string LoginID, string FullName, string Email, string RoleIDs, string CustNum, string BackdoorGUID)
        {
            this.isNeedLogin = false;
            this.isLogin = true;
            this.userID = UserID;
            this.windowsUsername = LoginID;
            this.loginID = this.windowsUsername;
            int t1 = this.loginID.IndexOf("\\");
            if (t1 > 0)
            {
                this.loginID = this.loginID.Substring(t1 + 1);
            }
            this.FullName = FullName;
            this.Email = Email;
            this.userRoles = this.addUserRolesByStringIDs(RoleIDs);
            this.CustNum = CustNum;
            this.BackdoorGUID = BackdoorGUID;

            DRspUserSettings_GetByUserID usersettings = SQLData.spUserSettings_GetByUserID(this.userID);
            userSettingsName = new string[usersettings.Count];
            userSettingsValue = new string[usersettings.Count];
            for (int i = 0; i < usersettings.Count; i++)
            {
                userSettingsName[i] = usersettings.Name(i);
                userSettingsValue[i] = usersettings.Value(i);
            }

            if (Utils.getAppSettings(SV.AppSettings.IsRecordActivityUserLoginLogout).ToBool())
            {
                DRspActivityHistory_Insert insert = SQLData.spActivityHistory_Insert(activityIDs.UserLoggedIn, this.userID, null);
            }
        }

        /// <summary>
        /// TODO: move this later somewhere else. need for all serialize only
        /// </summary>
        public void AuthorizeUser()
        {
            if (this.isNeedLogin)
            {
                string logonUser = Utils.getLogonUser();
                if (!String.IsNullOrEmpty(logonUser))
                {
                    DRspUsers_GetByLoginID ul = SQLData.spUsers_GetByLoginID(logonUser);
                    if (ul.Count > 0)
                    {
                        onLogin(ul.UserID, ul.LoginID, ul.FullName, ul.Email, ul.RoleIDs, ul.CustNum, ul.BackdoorGUID);
                    }
                }
            }
        }

        private int[] addTournamentIDsByStringIDs(string TournamentIDs)
        {
            string[] tds = TournamentIDs.Split(',');
            int[] tids = new int[tds.Length];
            for (int i = 0; i < tds.Length; i++)
            {
                tids[i] = 0;
                int.TryParse(tds[i], out tids[i]);
            }
            return tids;
        }

        private roleIDs[] addUserRolesByStringIDs(string roleids)
        {
            roleIDs[] ids = new roleIDs[0];
            if (!String.IsNullOrEmpty(roleids))
            {
                string[] strids = roleids.Split(',');
                if (strids.Length > 0)
                {
                    ids = new roleIDs[strids.Length];
                    for (int i = 0; i < strids.Length; i++)
                    {
                        ids[i] = (roleIDs)Enum.Parse(typeof(roleIDs), strids[i]);
                    }
                }
            }

            userRolesDescription = String.Empty;

            for (int i = 0; i < ids.Length; i++)
            {
                userRolesDescription += ids[i];
                if (i < ids.Length - 1)
                {
                    userRolesDescription += ", ";
                }
            }
            return ids;
        }

        public void Logout()
        {
            if (Utils.getAppSettings(SV.AppSettings.IsRecordActivityUserLoginLogout).ToBool())
            {
                DRspActivityHistory_Insert insert = SQLData.spActivityHistory_Insert(activityIDs.UserLoggedOut, this.userID, null);
            }
            this.isNeedLogin = false;
            reset();
        }

        private void reset()
        {
            // do not reset isNeedLogin (for logout)
            this.isLogin = false;
            this.userID = 0;
            this.windowsUsername = string.Empty;
            this.loginID = string.Empty;
            this.FullName = string.Empty;
            this.Email = string.Empty;
            this.userRoles = new roleIDs[0];
            this.userRolesDescription = String.Empty;
            this.userSettingsName = new string[0];
            this.userSettingsValue = new string[0];
        }

        public UserManager()
        {
            this.isNeedLogin = true;
            this.isimpersonating = false;
            this.isusingtouchscreen = false;
            reset();
        }

        public bool isImpersonating
        {
            get
            {
                return isimpersonating;
            }
        }

        public bool IsImpersonatingAllowed
        {
            get
            {
                return ((this.isUserAdministrator) || (this.isImpersonating));
            }
        }

        public void ImpersonateUser(int UserID)
        {
            if (IsImpersonatingAllowed)
            {
                if (UserID != Utils.getAppSettings(SV.Common.UserIDForSYSTEM).ToInt())
                {
                    DRspUser_GetByID us = SQLData.spUser_GetByID(UserID);
                    if (us.Count > 0)
                    {
                        DRspUsers_GetByLoginID ul = SQLData.spUsers_GetByLoginID(us.LoginID(0));
                        if (ul.Count > 0)
                        {
                            isimpersonating = true;
                            onLogin(ul.UserID, ul.LoginID, ul.FullName, ul.Email, ul.RoleIDs, ul.CustNum, ul.BackdoorGUID);
                        }
                    }
                }
            }
        }

        public bool isUserAdministrator
        {
            get
            {
                return isUserHasRole(roleIDs.Administrator);
            }
        }

        public string getUserSetting(string key)
        {
            // todo: make it better and faster
            string r = "";
            int i = 0;
            bool isFound = false;
            while ((i < userSettingsName.Length) && (!isFound))
            {
                if (userSettingsName[i] == key)
                {
                    r = userSettingsValue[i];
                    isFound = true;
                }
                i++;
            }
            if (!isFound)
            {
                r = Utils.getAppSettings(key).ToString();
            }
            return r;
        }

        public int getUserSettingInt(string key)
        {
            int r = 0;
            string s = getUserSetting(key);
            if (s.Length > 0)
            {
                int.TryParse(s, out r);
            }
            return r;
        }

        public bool getUserSettingBool(string key)
        {
            bool r = false;
            string s = getUserSetting(key).ToLower();
            if (s.Length > 0)
            {
                bool.TryParse(s, out r);
            }
            return r;
        }

        public bool isUserSystemAdministrator
        {
            get
            {
                return isUserHasRole(roleIDs.Administrator) && isUserHasRole(roleIDs.System);
            }
        }

        public bool IsUsingTouchScreen
        {
            get
            {
                return isusingtouchscreen;
            }
            set
            {
                isusingtouchscreen = value;
            }
        }

        public string VisitorState
        {
            get
            {
                return visitorstate;
            }
            set
            {
                visitorstate = value;
            }
        }

        public bool isUserSystem
        {
            get
            {
                return isUserHasRole(roleIDs.System);
            }
        }

        private bool isUserHasRole(roleIDs rid)
        {
            roleIDs foundid = Array.Find(this.userRoles, x => (x == rid));
            return (foundid != roleIDs.Unknown);
        }


    }

}
