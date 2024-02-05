using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;

using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Modules;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing UserInfo management.
    /// </summary>
    public class UserInfoProvider : AbstractInfoProvider<UserInfo, UserInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Registered Windows Live users will have this prefix in username.
        /// </summary>
        public const string LIVEID_USERS_PREFIX = "liveid_";

        /// <summary>
        /// Registered OpenID users will have this prefix in username.
        /// </summary>
        public const string OPENID_USERS_PREFIX = "openid_";

        /// <summary>
        /// Registered OpenID users will have this prefix in fullname.
        /// </summary>
        public const string OPENID_FULLNAME_PREFIX = "OpenID - ";

        /// <summary>
        /// Registered Facebook users will have this prefix in username.
        /// </summary>
        public const string FACEBOOKID_USERS_PREFIX = "facebookid_";

        /// <summary>
        /// Registered Facebook users will have this prefix in username.
        /// </summary>
        public const string FACEBOOKID_FULLNAME_PREFIX = "Facebook ID - ";

        /// <summary>
        /// Registered LinkedIn users will have this prefix in username.
        /// </summary>
        public const string LINKEDIN_USERS_PREFIX = "linkedinid_";

        /// <summary>
        /// Registered LinkedIn users will have this prefix in fullname.
        /// </summary>
        public const string LINKEDIN_FULLNAME_PREFIX = "LinkedIn ID - ";

        /// <summary>
        /// Where condition indicating that user is enabled
        /// </summary>
        public static readonly string USER_ENABLED_WHERE_CONDITION = new WhereCondition(new WhereCondition()
                                                                            .WhereEquals("UserEnabled", 1)
                                                                            .Or()
                                                                            .WhereNotNull("UserAccountLockReason")
                                                                            .Or()
                                                                            .WhereNotEquals("UserAccountLockReason", 0))
                                                                            .ToString(true);

        // Constant for password format SHA2 with salt.
        private const string SHA2SALT = "sha2salt";

        // Constant for password format PBKDF2.
        private const string PBKDF2 = "pbkdf2";

        /// <summary>
        /// Administrator user name
        /// </summary>
        public const string DEFAULT_ADMIN_USERNAME = "administrator";

        // Constants used for IsAuthorized per resource methods. DO NOT CHANGE THE CASE
        private const string REPORTING_RESOURCE = "cms.reporting";
        private const string FORM_RESOURCE = "cms.form";
        private const string DESIGN_RESOURCE = "cms.design";
        private const string GLOBAL_PERMISSIONS_RESOURCE = "cms.globalpermissions";

        private const string EDIT_SQL_QUERIES_PERMISSION = "editsqlqueries";
        private const string EDIT_CODE_PERMISSION = "editcode";
        private const string EDIT_SQL_CODE_PERMISSION = "editsqlcode";

        private const string SITE_PREFIX_TEMPLATE = "site.{0}.";

        #endregion


        #region "Private fields"

        private static bool mLogUserCounts = true;
        private static string mPasswordSalt;

        private static readonly CMSStatic<int?> mLicGlobalAdmins = new CMSStatic<int?>();
        private static readonly CMSStatic<SafeDictionary<string, int?>> mLicSiteMembers = new CMSStatic<SafeDictionary<string, int?>>(() => new SafeDictionary<string, int?>());
        private static readonly CMSStatic<SafeDictionary<string, int?>> mLicEditors = new CMSStatic<SafeDictionary<string, int?>>(() => new SafeDictionary<string, int?>());

        private static Regex mSitePrefixRegex;
        private static readonly object mEnsureRolesLock = new object();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of <see cref="UserInfoProvider"/>.
        /// </summary>
        public UserInfoProvider()
            : base(UserInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Events & delegates"

        /// <summary>
        /// Delegate used for formatting user name
        /// </summary>
        /// <param name="username">Source user name</param>
        /// <param name="fullname">Source full name</param>
        /// <param name="nickname">Source nick name</param>
        /// <param name="isLiveSite">Indicates if returned username should be displayed on live site</param>
        public delegate String FormattedUserNameEventHandler(string username, string fullname, string nickname, bool isLiveSite);


        /// <summary>
        /// Event for formatting user name
        /// </summary>
        public static event FormattedUserNameEventHandler OnFormattedUserName;

        #endregion


        #region "Private properties"


        /// <summary>
        /// Represents <see cref="DateTime.Now"/> value.
        /// </summary>
        /// <remarks>This property should is used for testability purposes only.</remarks>
        internal static DateTime DateTimeNow
        {
            get
            {
                return Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
            }
        }


        /// <summary>
        /// Get regex for detect site prefix username
        /// </summary>
        private static Regex SitePrefixRegex
        {
            get
            {
                return mSitePrefixRegex ?? (mSitePrefixRegex = RegexHelper.GetRegex("site[.]([A-Z0-9a-z-]+)[.](.+)"));
            }
        }


        /// <summary>
        /// License limitations for global admins
        /// </summary>
        private static int? LicGlobalAdmins
        {
            get
            {
                return mLicGlobalAdmins;
            }
            set
            {
                mLicGlobalAdmins.Value = value;
            }
        }


        /// <summary>
        /// License limitations for site members
        /// </summary>
        private static SafeDictionary<string, int?> LicSiteMembers
        {
            get
            {
                return mLicSiteMembers;
            }
        }


        /// <summary>
        /// License limitations for editors
        /// </summary>
        private static SafeDictionary<string, int?> LicEditors
        {
            get
            {
                return mLicEditors;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether user counts settings should be checked.
        /// </summary>
        public static bool LogUserCounts
        {
            get
            {
                return mLogUserCounts && CMSActionContext.CurrentUpdateUserCounts;
            }
            set
            {
                mLogUserCounts = value;
            }
        }


        /// <summary>
        /// Gets or sets password salt.
        /// </summary>
        public static string PasswordSalt
        {
            get
            {
                return mPasswordSalt ?? (mPasswordSalt = SettingsHelper.AppSettings["CMSPasswordSalt"]);
            }
            set
            {
                mPasswordSalt = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether user counts are enabled.
        /// </summary>
        public static bool UserCountsEnabled
        {
            get
            {
                return LogUserCounts && SettingsKeyInfoProvider.GetBoolValue("CMSEnableUserCounts");
            }
            set
            {
                mLogUserCounts = value;
            }
        }


        /// <summary>
        /// Returns password format to be used while hashing new passwords.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For generating password use <see cref="SetPassword(string, string)"/>.
        /// </para>
        /// <para>
        /// For password verification use <see cref="IsUserPasswordDifferent(UserInfo, string)"/>.
        /// </para>
        /// </remarks>
        public static string NewPasswordFormat
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue("CMSPasswordFormat");
            }
        }


        /// <summary>
        /// Indicates whether to use safe or normal user names.
        /// </summary>
        public static bool UseSafeUserName
        {
            get
            {
                return ValidationHelper.UseSafeUserName;
            }
            set
            {
                ValidationHelper.UseSafeUserName = value;
            }
        }


        /// <summary>
        /// Indicates whether to use safe or normal role names.
        /// </summary>
        public static bool UseSafeRoleName
        {
            get
            {
                return ValidationHelper.UseSafeRoleName;
            }
            set
            {
                ValidationHelper.UseSafeRoleName = value;
            }
        }


        /// <summary>
        /// Gets the default system administrator user in the listed order of preferences:
        /// 1) User defined by settings key CMSDefaultUserID
        /// 2) User with username 'administrator'
        /// 3) Any other global admin
        /// </summary>
        public static UserInfo AdministratorUser
        {
            get
            {
                // Cache the admin user to avoid too many loads
                return CacheHelper.Cache(GetAdministratorUser,
                    new CacheSettings(CacheHelper.API_CACHE_MINUTES, "administratoruser")
                    {
                        GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { "cms.user|all" })
                    }
                );
            }
        }


        /// <summary>
        /// Gets the default system administrator user name in the listed order of preferences:
        /// 1) User defined by settings key CMSDefaultUserID
        /// 2) User with username 'administrator'
        /// 3) Any other global admin
        /// 4) Defaults to 'administrator' if no global admin is found
        /// </summary>
        public static string AdministratorUserName
        {
            get
            {
                var ui = AdministratorUser;
                if (ui == null)
                {
                    return DEFAULT_ADMIN_USERNAME;
                }

                return ui.UserName;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all roles for specified user.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="siteName">Site name.</param>
        public static string[] GetRolesForUser(string userName, string siteName)
        {
            // Get user record
            UserInfo ui = GetUserInfo(userName);
            if (ui != null)
            {
                string where = String.Empty;

                // Set site limit only for CMS Sites or global role, for non-kentico sites return all roles => possible security issue
                var siteId = SiteInfoProvider.GetSiteID(siteName);
                if (siteId > 0)
                {
                    where = "SiteID = " + siteId + " OR SiteID IS NULL";
                }

                // Get the roles
                DataTable rolesTable = GetUserRoles(ui, where, null, 0, "RoleName");
                // Check whether exists at least one role
                if (!DataHelper.DataSourceIsEmpty(rolesTable))
                {
                    // Initialize role list
                    List<string> roles = new List<string>(rolesTable.Rows.Count);
                    // Loop thru all roles and fill list of roles
                    foreach (DataRow dr in rolesTable.Rows)
                    {
                        roles.Add(Convert.ToString(dr["RoleName"]));
                    }
                    return roles.ToArray();
                }
            }
            return new string[0];
        }


        /// <summary>
        /// Returns true if the user is a member of the role in context of the specified site.
        /// </summary>
        /// <remarks>The check also accounts for membership roles and global roles.</remarks>
        /// <param name="userName">User name</param>
        /// <param name="roleName">Role name</param>
        /// <param name="siteName">Site name</param>
        public static bool IsUserInRole(string userName, string roleName, string siteName)
        {
            UserInfo ui = GetUserInfo(userName);
            if (ui != null)
            {
                return ui.IsInRole(roleName, siteName);
            }
            return false;
        }


        /// <summary>
        /// Gets the default system user with global administrator privilege level in the listed order of preferences:
        /// 1) User defined by settings key CMSDefaultUserID
        /// 2) User with username 'administrator'
        /// 3) Any other global admin
        /// </summary>
        private static UserInfo GetAdministratorUser()
        {
            // Try to get global administrator id from settings
            int defaultUserID = SettingsKeyInfoProvider.GetIntValue("CMSDefaultUserID");
            var defaultUser = GetUserInfo(defaultUserID);

            if (defaultUser == null || !defaultUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                // Fall back to global administrator lookup by default username
                defaultUser = GetUserInfo(DEFAULT_ADMIN_USERNAME);
            }

            if (defaultUser == null || !defaultUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                defaultUser = GetFirstGlobalAdmin();
            }

            return defaultUser;
        }


        private static UserInfo GetFirstGlobalAdmin()
        {
            return GetUsers()
                .WhereEquals("UserPrivilegeLevel", (int)UserPrivilegeLevelEnum.GlobalAdmin)
                .TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Invalidates the specified user
        /// </summary>
        /// <param name="userId">User ID</param>
        public static void InvalidateUser(int userId)
        {
            UserInfo ui = GetUserInfo(userId);
            ui?.Generalized.Invalidate(false);
        }


        /// <summary>
        /// Returns smart search where condition for indexing.
        /// </summary>
        /// <param name="srchInfo">Index info</param>
        public static string GetSearchWhereCondition(SearchIndexInfo srchInfo)
        {
            if (srchInfo != null)
            {
                bool hiddenUsers = false;
                bool onlyEnabled = true;
                bool allSites = false;
                string whereCondition = String.Empty;

                if ((srchInfo.IndexSettings.Items != null) && (srchInfo.IndexSettings.Items.Count > 0))
                {
                    SearchIndexSettingsInfo sisi = srchInfo.IndexSettings.Items[SearchHelper.SIMPLE_ITEM_ID];
                    if (sisi != null)
                    {
                        hiddenUsers = ValidationHelper.GetBoolean(sisi.GetValue("UserHidden"), false);
                        onlyEnabled = ValidationHelper.GetBoolean(sisi.GetValue("UserEnabled"), true);
                        allSites = ValidationHelper.GetBoolean(sisi.GetValue("UserAllSites"), false);
                        whereCondition = ValidationHelper.GetString(sisi.GetValue("WhereCondition"), String.Empty);
                    }
                }

                // Current scope where condition
                string localWhere = String.Empty;

                #region "Where condition"

                // Add site restrictions to the where condition
                if (!allSites)
                {
                    // Get index sites
                    var siteBindings = SearchIndexSiteInfoProvider.GetIndexSiteBindings(srchInfo.IndexID).Columns("IndexSiteID");

                    // Loop through all sites and generate part of SQL where condition
                    string siteIds = String.Join(",", siteBindings.Select(binding => binding.IndexSiteID));

                    // Check whether index is assigned to the at least one site
                    // if not, return nothing
                    if (!String.IsNullOrEmpty(siteIds))
                    {
                        localWhere = "UserID IN (SELECT UserID FROM CMS_UserSite WHERE SiteID IN (" + siteIds + "))";
                    }
                    else
                    {
                        return String.Empty;
                    }
                }

                // Complete where condition
                whereCondition = SqlHelper.AddWhereCondition(localWhere, whereCondition);

                #endregion

                if (!hiddenUsers)
                {
                    whereCondition = SqlHelper.AddWhereCondition("UserIsHidden = 0 OR UserIsHidden IS NULL", whereCondition);
                }

                if (onlyEnabled)
                {
                    whereCondition = SqlHelper.AddWhereCondition(USER_ENABLED_WHERE_CONDITION, whereCondition);
                }

                return whereCondition;
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns user's full name
        /// </summary>
        /// <param name="firstName">User's first name</param>
        /// <param name="middleName">User's middle name</param>
        /// <param name="lastName">User's last name</param>
        public static string GetFullName(string firstName, string middleName, string lastName)
        {
            // Concatenate all non empty name parts separated by a single space
            return String.Join(" ", new List<String>(3) { firstName, middleName, lastName }
                                                    .Where(n => !String.IsNullOrWhiteSpace(n))
                                                    .Select(n => n.Trim()));
        }


        /// <summary>
        /// Returns top 15 search documents for current scope.
        /// </summary>
        /// <param name="indexInfo">Search index info</param>
        /// <param name="whereCondition">Additional where condition</param>
        /// <param name="lastUserId">Last record user id</param>
        public static List<SearchDocument> GetSearchDocuments(SearchIndexInfo indexInfo, string whereCondition, int lastUserId)
        {
            // Check whether index info object exists
            if (indexInfo != null)
            {
                // Initialize where condition by hidden and enabled parts
                whereCondition = SqlHelper.AddWhereCondition($"UserID > {lastUserId}", whereCondition);

                // Get users dataset by specific conditions
                var users = GetUsersDataWithSettings().Where(new WhereCondition(whereCondition)).OrderBy("UserID").TopN(indexInfo.IndexBatchSize);

                // Check whether something was found
                if (users.HasResults())
                {
                    // Initialize result collection
                    var list = new List<SearchDocument>();

                    // Loop through all records
                    foreach (var user in users)
                    {
                        // Get search document
                        var iDoc = user.GetSearchDocument(indexInfo);
                        if (iDoc != null)
                        {
                            // Add document to the result collection if search document exists
                            list.Add(iDoc);
                        }
                    }

                    // Return collection of search documents
                    return list;
                }
            }

            // Return nothing by default
            return null;
        }


        /// <summary>
        /// Gets currently logged user name. The user name is safe if the UseSafeUserName is enabled.
        /// </summary>
        public static string GetUserName()
        {
            return GetUserName(null);
        }


        /// <summary>
        /// Gets currently logged user name. The user name is safe if the UseSafeUserName is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetUserName(string siteName)
        {
            // Get current user name
            string userName = RequestContext.UserName.ToLowerInvariant();
            userName = UseSafeUserName ? ValidationHelper.GetSafeUserName(userName, siteName) : userName;

            return userName;
        }


        /// <summary>
        /// Returns true if specified name is defined in reserved names.
        /// </summary>
        /// <param name="siteName">SiteName</param>
        /// <param name="username">User name or nickname</param>
        public static bool NameIsReserved(string siteName, string username)
        {
            if (String.IsNullOrEmpty(username))
            {
                return false;
            }

            string[] reservedNames = SettingsKeyInfoProvider.GetValue(siteName + ".CMSReservedUserNames").Split(';');
            return reservedNames.Where(name => name != null).Any(name => name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Returns true if email doesn't exist in users table. 
        /// Depends on 'Unique e-mails' and 'Shared user accounts' settings
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="user">Current user which should be checked</param>
        public static bool IsEmailUnique(string email, UserInfo user)
        {
            if (user == null)
            {
                return false;
            }

            var checkSites = new List<string>();

            // Email must be unique in all sites where user belongs
            var userSites = GetUserSites(user.UserID).Columns("SiteName");
            if (!DataHelper.DataSourceIsEmpty(userSites))
            {
                // Add all site names
                foreach (var site in userSites)
                {
                    checkSites.Add(site.SiteName);
                }
            }

            return IsEmailUnique(email, checkSites, user.UserID);
        }


        /// <summary>
        /// Test if there is any site prefix username (f.e. 'site.{GUID}.{Name}' and '{Name}' is not allowed at the same time).
        /// Should be checked if site prefixes are disabled but user names with site prefixes may exist in the system.
        /// </summary>
        /// <param name="userName">User name to test. Must be without 'site.GUID' prefix</param>
        /// <param name="userID">ID of tested user</param>
        public static bool IsUserNamePrefixUnique(String userName, int userID)
        {
            var prefixPattern = String.Format(SITE_PREFIX_TEMPLATE, "%");

            // Get all prefixed similar users
            var users = GetUsers()
#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                .WhereLike("UserName", prefixPattern + userName)
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                .WhereNotEquals("UserID", userID)
                .Columns("UserName");

            return !users.Any(user => IsSitePrefixedUser(user.UserName));
        }


        /// <summary>
        /// Returns true if email doesn't exist in users table. 
        /// Depends on 'Unique e-mails' and 'Shared user accounts' settings
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="siteName">One or more site names separated by semicolon</param>
        /// <param name="currentUserId">Current user id, if is 0 => new user</param>
        public static bool IsEmailUnique(string email, string siteName, int currentUserId)
        {
            string[] siteList = { };
            if (!String.IsNullOrEmpty(siteName))
            {
                siteList = siteName.Split(';');
            }

            return IsEmailUnique(email, siteList, currentUserId);
        }


        /// <summary>
        /// Returns true if email doesn't exist in users table. 
        /// Depends on 'Unique e-mails' and 'Shared user accounts' settings
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="siteNames">One or more site name</param>
        /// <param name="currentUserId">Current user id, if is 0 => new user</param>
        public static bool IsEmailUnique(string email, IEnumerable<string> siteNames, int currentUserId)
        {
            bool sharedAccounts = SettingsKeyInfoProvider.GetBoolValue("CMSSiteSharedAccounts");

            // If shared accounts - valid unique emails to all sites (no matter if user is assigned to)
            if (sharedAccounts)
            {
                var sitesBuilder = new List<string>();

                // Get all sites
                DataSet ds = SiteInfoProvider.GetSites();
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        sitesBuilder.Add(dr["SiteName"].ToString());
                    }
                }
                siteNames = sitesBuilder;
            }

            string[] sites = { };
            if (siteNames != null)
            {
                sites = siteNames as string[] ?? siteNames.ToArray();
            }

            // Empty email is unique by default because the
            // purpose of this method is not verification whether email is sets or if is valid!
            if (String.IsNullOrEmpty(email) || sites.Length == 0)
            {
                return true;
            }

            // Only one site => check whether unique emails are required
            if ((sites.Length == 1) && !SettingsKeyInfoProvider.GetBoolValue(sites[0] + ".CMSUserUniqueEmail"))
            {
                // Is unique because unique emails are not required
                return true;
            }

            // Get users with dependence on selected email
            var users = GetUsers()
                .WhereEquals("Email", email)
                .WhereNotEquals("UserID", currentUserId);

            if (users.Count == 0)
            {
                return true;
            }

            // Go trough all sites which should be checked
            foreach (string siteName in sites)
            {
                // Check whether unique emails are required for site
                if (!SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUserUniqueEmail"))
                {
                    continue;
                }

                // User with same email is in current site or site requires unique email across all sites
                if (sharedAccounts || users.Any(user => user.IsInSite(siteName)))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static UserInfo GetUserInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action</param>
        /// <param name="currentStatus">License status</param>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action, bool currentStatus)
        {
            // Return true by default for context free applications
            if (String.IsNullOrEmpty(domain) || action == ObjectActionEnum.Read)
            {
                return true;
            }

            // Define domains
            string[] domains = domain.Contains(";") ? domain.Split(';') : new[] { domain };

            // Set default version limitations
            int versionLimitations = 0;

            // Check every
            for (int i = 0; i < domains.Length; i++)
            {
                if (!String.IsNullOrEmpty(domains[i]))
                {
                    domains[i] = URLHelper.RemoveWWW(domains[i].ToLowerInvariant());
                }
                versionLimitations += LicenseKeyInfoProvider.VersionLimitations(domains[i], feature, action != ObjectActionEnum.Insert);
            }

            // Check if current license hasn't any limitation
            if (versionLimitations == 0)
            {
                return true;
            }

            int countNumber = 0;

            // Set count number for edit
            if (currentStatus)
            {
                countNumber = 2;
            }

            switch (feature)
            {
                // Global administrator
                case FeatureEnum.Administrators:

                    // First initialize of global administrators settings
                    if (LicGlobalAdmins == null)
                    {
                        // Check all licenses
                        // If CMS instance contains at least one non trial, 
                        // non free and unlimited license, disable global admin check
                        var licenses = LicenseKeyInfoProvider.GetLicenseKeys();
                        bool hasLicences = licenses.Any();
                        bool checkGlobal = !licenses.Any(lki => (lki.Edition != ProductEditionEnum.Free) && (!lki.IsTrial) && (lki.ExpirationDateReal == LicenseKeyInfo.TIME_UNLIMITED_LICENSE));

                        if (!checkGlobal || !hasLicences)
                        {
                            LicGlobalAdmins = -1;
                            return true;
                        }

                        LicGlobalAdmins = GetUsers().WhereIn("UserPrivilegeLevel", new[]
                                                        {
                                                            (int)UserPrivilegeLevelEnum.Admin,
                                                            (int)UserPrivilegeLevelEnum.GlobalAdmin
                                                        }).GetCount();
                    }


                    try
                    {
                        if ((LicGlobalAdmins != null) && (LicGlobalAdmins.Value == -1))
                        {
                            return true;
                        }

                        foreach (string d in domains)
                        {
                            if (versionLimitations != 0)
                            {
                                // Try add
                                if (action == ObjectActionEnum.Insert)
                                {
                                    versionLimitations = LicenseKeyInfoProvider.VersionLimitations(d, feature, false);
                                    if ((versionLimitations + countNumber) <= (ValidationHelper.GetInteger(LicGlobalAdmins, -1) + 1))
                                    {
                                        return false;
                                    }
                                }

                                // Get status
                                if (action == ObjectActionEnum.Edit)
                                {
                                    versionLimitations = LicenseKeyInfoProvider.VersionLimitations(d, feature);
                                    if (versionLimitations < ValidationHelper.GetInteger(LicGlobalAdmins, 0))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        ClearLicenseValues();
                        return false;
                    }

                    return true;

                // Editors
                case FeatureEnum.Editors:

                    // First initialization of editors for user
                    foreach (string d in domains)
                    {
                        if (LicEditors[d] == null)
                        {
                            LicEditors[d] = GetUsers().WhereEquals("UserPrivilegeLevel", (int)UserPrivilegeLevelEnum.Editor)
                                                          .WhereIn(
                                                              "UserID",
                                                              new IDQuery<UserSiteInfo>("UserID")
                                                                  .WhereEquals("SiteID", LicenseHelper.GetSiteIDbyDomain(d))
                                                          ).GetCount();
                        }
                    }

                    try
                    {
                        foreach (string d in domains)
                        {
                            // Check editors count for user
                            if (action == ObjectActionEnum.Insert)
                            {
                                versionLimitations = LicenseKeyInfoProvider.VersionLimitations(d, feature, false);
                                if (ValidationHelper.GetInteger(LicEditors[d], -1) + 1 > versionLimitations + countNumber)
                                {
                                    return false;
                                }
                            }

                            // Get status
                            if (action == ObjectActionEnum.Edit)
                            {
                                versionLimitations = LicenseKeyInfoProvider.VersionLimitations(d, feature);
                                if (ValidationHelper.GetInteger(LicEditors[d], 0) > versionLimitations)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    catch
                    {
                        ClearLicenseValues();
                        return false;
                    }
                    return true;

                // Site members
                case FeatureEnum.SiteMembers:
                    {
                        // Check all domains
                        foreach (string dom in domains)
                        {
                            if (LicSiteMembers[dom] == null)
                            {
                                string d = dom;

                                LicSiteMembers[d] = UserSiteInfoProvider.GetUserSites().Where("SiteID", QueryOperator.Equals, LicenseHelper.GetSiteIDbyDomain(d)).GetCount();
                            }
                        }

                        try
                        {
                            foreach (string d in domains)
                            {
                                // Check site users count for user
                                if (action == ObjectActionEnum.Insert)
                                {
                                    versionLimitations = LicenseKeyInfoProvider.VersionLimitations(d, feature, false);

                                    if (ValidationHelper.GetInteger(LicSiteMembers[d], -1) + 1 > versionLimitations)
                                    {
                                        return false;
                                    }
                                }

                                // Get status
                                if (action == ObjectActionEnum.Edit)
                                {
                                    versionLimitations = LicenseKeyInfoProvider.VersionLimitations(d, feature);

                                    if (ValidationHelper.GetInteger(LicSiteMembers[d.ToLowerInvariant()], 0) > versionLimitations)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            ClearLicenseValues();
                            return false;
                        }

                        return true;
                    }
            }

            return false;
        }


        /// <summary>
        /// Updates user count with dependence on selected type of activity.
        /// </summary>
        /// <param name="type">Activity type</param>
        /// <param name="userId">User ID</param>
        /// <param name="points">Points</param>
        public static void UpdateUserCounts(ActivityPointsEnum type, int userId, int points)
        {
            if (UserCountsEnabled)
            {
                var parameters = new QueryDataParameters();

                parameters.Add("@UserID", userId);
                parameters.Add("@Points", points);
                parameters.Add("@Type", Convert.ToInt32(type));
                parameters.Add("@Now", DateTimeNow);

                // Update user counts and add activity points
                ConnectionHelper.ExecuteQuery("cms.user.UpdateUserCounts", parameters);

                if (userId <= 0)
                {
                    UserInfo.TYPEINFO.InvalidateAllObjects();
                }
                else
                {
                    var ui = GetUserInfo(userId);
                    ui?.Generalized.Invalidate(false);
                }
            }
        }


        /// <summary>
        /// Returns the UserInfo structure for the specified user.
        /// </summary>
        /// <param name="userId">User id</param>
        public static UserInfo GetUserInfo(int userId)
        {
            return ProviderObject.GetInfoById(userId);
        }


        /// <summary>
        /// Returns the UserInfo structure for the specified user.
        /// </summary>
        /// <param name="userName">UserName</param>
        public static UserInfo GetUserInfo(string userName)
        {
            return ProviderObject.GetInfoByCodeName(userName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified user.
        /// </summary>
        /// <param name="user">User to set</param>
        public static void SetUserInfo(UserInfo user)
        {
            ProviderObject.SetUserInfoInternal(user);
        }


        /// <summary>
        /// Deletes specified user.
        /// </summary>
        /// <param name="userObj">User object</param>
        public static void DeleteUser(UserInfo userObj)
        {
            ProviderObject.DeleteInfo(userObj);
        }


        /// <summary>
        /// Deletes specified user.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static void DeleteUser(int userId)
        {
            UserInfo userObj = GetUserInfo(userId);
            DeleteUser(userObj);
        }


        /// <summary>
        /// Deletes specified user.
        /// </summary>
        /// <param name="userName">User name</param>
        public static void DeleteUser(string userName)
        {
            UserInfo userObj = GetUserInfo(userName);
            DeleteUser(userObj);
        }


        /// <summary>
        /// Returns the table of the user roles.
        /// </summary>
        /// <param name="userInfo">User info for retrieving the role table</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Get top N records</param>
        /// <param name="columns">Columns to get</param>
        public static DataTable GetUserRoles(UserInfo userInfo, string whereCondition, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetUserRolesInternal(userInfo, whereCondition, orderBy, topN, columns, true, true, true);
        }


        /// <summary>
        /// Returns the table of the user roles.
        /// </summary>
        /// <param name="userInfo">User info for retrieving the role table</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N records</param>
        /// <param name="columns">Columns to get</param>
        /// <param name="includeMembership">If true, membership roles are added</param>
        /// <param name="includeGlobal">If true, global roles are added</param>
        /// <param name="checkValidity">If true, only valid roles are selected</param>
        public static DataTable GetUserRoles(UserInfo userInfo, string whereCondition, string orderBy, int topN, string columns, bool includeMembership, bool includeGlobal, bool checkValidity)
        {
            return ProviderObject.GetUserRolesInternal(userInfo, whereCondition, orderBy, topN, columns, includeMembership, includeGlobal, checkValidity);
        }


        /// <summary>
        /// Returns the table of the user roles given by membership connection.
        /// </summary>
        /// <param name="userInfo">User info for retrieving the role table</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Get top N records</param>
        /// <param name="columns">Columns to get</param>
        public static DataTable GetUserMembershipRoles(UserInfo userInfo, string whereCondition, string orderBy, int topN, string columns)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, string.Empty) != string.Empty)
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetUserMembershipRolesInternal(userInfo, whereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns the table of the user roles.
        /// </summary>
        /// <param name="userInfo">User info for retrieving the role table</param>
        public static DataTable GetUserRoles(UserInfo userInfo)
        {
            return GetUserRoles(userInfo, null, null, -1, null);
        }


        /// <summary>
        /// Returns the table of the user sites.
        /// </summary>
        /// <param name="userId">User ID for site table</param>
        public static ObjectQuery<SiteInfo> GetUserSites(int userId)
        {
            return ProviderObject.GetUserSitesInternal(userId);
        }


        /// <summary>
        /// Returns the UserName by the specified user ID.
        /// </summary>
        /// <param name="id">User ID</param>
        public static string GetUserNameById(int id)
        {
            return ProviderObject.GetUserNameByIdInternal(id);
        }


        /// <summary>
        /// Returns true if user is granted with specified permission for particular class (document type).
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name for which check the permissions</param>
        /// <param name="userInfo">User info object</param>
        public static bool IsAuthorizedPerClass(string className, string permissionName, string siteName, UserInfo userInfo)
        {
            return ProviderObject.IsAuthorizedPerClassInternal(className, permissionName, siteName, userInfo, false);
        }


        /// <summary>
        /// Returns true if user is granted with specified permission for particular class (document type).
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name for which check the permissions</param>
        /// <param name="userInfo">User info object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsAuthorizedPerClass(string className, string permissionName, string siteName, UserInfo userInfo, bool exceptionOnFailure)
        {
            return ProviderObject.IsAuthorizedPerClassInternal(className, permissionName, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Gets the DataSet of the required users for the specified resource permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        public static InfoDataSet<UserInfo> GetRequiredResourceUsers(string resourceName, string permissionName, string siteName)
        {
            return ProviderObject.GetRequiredResourceUsersInternal(resourceName, permissionName, siteName, null, null, 0, null);
        }


        /// <summary>
        /// Gets the DataSet of the required users for the specified resource permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        public static InfoDataSet<UserInfo> GetRequiredResourceUsers(string resourceName, string permissionName, string siteName, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetRequiredResourceUsersInternal(resourceName, permissionName, siteName, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns the query for all users.
        /// </summary>   
        public static ObjectQuery<UserInfo> GetUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the UserInfo structure for the specified user also with UserSettings.
        /// </summary>
        /// <param name="userId">User id</param>
        public static UserInfo GetFullUserInfo(int userId)
        {
            // If no user id is given, return null
            if (userId <= 0)
            {
                return null;
            }

            // Get the user record
            UserInfo ui = GetUserInfo(userId);
            if (ui == null)
            {
                throw new Exception("[UserInfoProvider.GetFullUserInfo]: User with user ID '" + userId + "' doesn't exist!");
            }

            // Get the settings record
            ui.UserSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userId);
            return ui;
        }


        /// <summary>
        /// Returns the UserInfo structure for the specified user also with UserSettings.
        /// </summary>
        /// <param name="userName">UserName</param>
        public static UserInfo GetFullUserInfo(string userName)
        {
            UserInfo ui = GetUserInfo(userName);
            if (ui == null)
            {
                throw new Exception("[UserInfoProvider.GetFullUserInfo]: User with given username doesn't exist!");
            }

            ui.UserSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(ui.UserID);
            return ui;
        }


        /// <summary>
        /// Clear license values.
        /// </summary>
        public static void ClearLicenseValues()
        {
            LicEditors.Clear();
            LicSiteMembers.Clear();

            LicGlobalAdmins = null;
        }


        /// <summary>
        /// Trims site prefix from user name (if any prefix found)
        /// </summary>
        /// <param name="username">User name</param>
        public static String TrimSitePrefix(string username)
        {
            return ProviderObject.TrimSitePrefixInternal(username);
        }


        /// <summary>
        /// Check if user belongs to specified site, considering settings key CMSSiteSharedAccounts.
        /// </summary>
        /// <param name="user">UserInfo</param>
        /// <param name="siteName">SiteName</param>
        /// <returns>Returns same UserInfo if user belongs to site(if shared accounts disabled) or null if not</returns>
        public static UserInfo CheckUserBelongsToSite(UserInfo user, string siteName)
        {
            // If users account are not shared across sites, check if user belongs to site
            if ((user != null) && (SettingsKeyInfoProvider.GetBoolValue("cmssitesharedaccounts") == false))
            {
                // If user is global admin or is assigned to site
                if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || user.IsInSite(siteName))
                {
                    return user;
                }
                else
                {
                    return null;
                }
            }

            return user;
        }


        /// <summary>
        /// Sets the preferred culture codes from given user object.
        /// </summary>
        /// <param name="user">User object</param>
        public static void SetPreferredCultures(UserInfo user)
        {
            if (user == null)
            {
                return;
            }

            // Set user preferred culture
            string preferredCulture = user.PreferredCultureCode;
            if (preferredCulture != string.Empty)
            {
                CultureHelper.SetPreferredCulture(preferredCulture);
            }

            // Set user preferred UI culture
            string preferredUICulture = user.PreferredUICultureCode;
            if (string.IsNullOrEmpty(preferredUICulture))
            {
                // Set default culture
                preferredUICulture = ResourceStringInfoProvider.DefaultUICulture;
            }

            CultureHelper.SetPreferredUICultureCode(preferredUICulture);
        }


        /// <summary>
        /// Ensures that roles and sites for the given user are present within the database. Used in case of Windows authentication.
        /// </summary>
        /// <param name="uInfo">Source user info object</param>
        public static void EnsureRolesAndSitesForWindowsAuthentication(UserInfo uInfo)
        {
            EnsureRolesAndSitesInternal(uInfo, true);
        }


        /// <summary>
        /// Ensures that roles and sites for the given user are present within the database.
        /// </summary>
        /// <param name="uInfo">Source user info object</param>
        public static void EnsureRolesAndSites(UserInfo uInfo)
        {
            EnsureRolesAndSitesInternal(uInfo, false);
        }


        /// <summary>
        /// Adds the specified user to the role of site.
        /// </summary>
        /// <param name="userName">User name to add to role</param>
        /// <param name="roleName">Role name</param>
        /// <param name="siteName">Site name</param>
        public static void AddUserToRole(string userName, string roleName, string siteName)
        {
            // First check if the user is not in role yet and role exists
            UserInfo ui = GetUserInfo(userName);
            RoleInfo ri = RoleInfoProvider.GetRoleInfo(roleName, siteName);

            UserRoleInfoProvider.AddUserToRole(ui, ri);
        }


        /// <summary>
        /// Adds the specified user to the role of site.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        public static void AddUserToRole(int userId, int roleId)
        {
            UserRoleInfoProvider.AddUserToRole(userId, roleId);
        }


        /// <summary>
        /// Removes specified user from the role.
        /// </summary>
        /// <param name="userName">User name to remove</param>
        /// <param name="roleName">Role name</param>
        /// <param name="siteName">Site name</param>
        public static void RemoveUserFromRole(string userName, string roleName, string siteName)
        {
            UserInfo ui = GetUserInfo(userName);
            RoleInfo ri = RoleInfoProvider.GetRoleInfo(roleName, siteName);

            if ((ui != null) && (ri != null))
            {
                UserRoleInfoProvider.DeleteUserRoleInfo(ui, ri);
            }
        }


        /// <summary>
        /// Removes specified user from the role.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        public static void RemoveUserFromRole(int userId, int roleId)
        {
            var uri = UserRoleInfoProvider.GetUserRoleInfo(userId, roleId);
            UserRoleInfoProvider.DeleteUserRoleInfo(uri);
        }


        /// <summary>
        /// Adds the specified user to the site.
        /// </summary>
        /// <param name="userName">User name to add</param>
        /// <param name="siteName">Site name</param>
        public static void AddUserToSite(string userName, string siteName)
        {
            UserInfo ui = GetUserInfo(userName);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);

            UserSiteInfoProvider.AddUserToSite(ui, si);
        }


        /// <summary>
        /// Removes the user from the specified site.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="siteId">Site ID</param>
        public static void RemoveUserFromSite(int userId, int siteId)
        {
            UserSiteInfoProvider.RemoveUserFromSite(userId, siteId);
        }


        /// <summary>
        /// Removes the user from the specified site.
        /// </summary>
        /// <param name="userName">User name to remove</param>
        /// <param name="siteName">Site name</param>
        public static void RemoveUserFromSite(string userName, string siteName)
        {
            UserInfo ui = GetUserInfo(userName);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);

            if ((si != null) && (ui != null))
            {
                RemoveUserFromSite(ui.UserID, si.SiteID);
            }
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        public static bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName, UserInfo userInfo)
        {
            return IsAuthorizedPerResource(resourceName, permissionName, siteName, userInfo, false);
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <exception cref="CMS.DataEngine.PermissionCheckException">Thrown when user is not authorized per resource and <paramref name="exceptionOnFailure"/> is enabled</exception>
        public static bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName, UserInfo userInfo, bool exceptionOnFailure)
        {
            // If no userInfo given, not authorized
            if (userInfo == null)
            {
                return false;
            }

            // Global admin can always access resources
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return true;
            }

            // Backward compatibility: Fix resource name for global permission
            ChangeResourceNameToDesignForGlobalResourceAndSqlOrCodePermission(ref resourceName, permissionName);

            // Check whether current permission is for SQL coder or ASCX code edit
            // Admin can edit SQL code or ASCX code only with specific permission or with enabled code editing setting
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) &&
                ((SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableCodeEditSiteAdministrators") || !IsSqlOrCodePermission(ref resourceName, permissionName))))
            {
                return true;
            }

            bool result = CheckRequiredResourceRole(userInfo, resourceName, permissionName, siteName);

            if (exceptionOnFailure && !result)
            {
                throw new PermissionCheckException(resourceName, permissionName, siteName);
            }

            return result;
        }


        /// <summary>
        /// Returns true if user is assigned to the role with permission under specified resource
        /// </summary>
        /// <param name="userInfo">User info to check</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        private static bool CheckRequiredResourceRole(UserInfo userInfo, string resourceName, string permissionName, string siteName)
        {
            bool result = false;

            // Check the roles
            DataSet ds = RoleInfoProvider.GetRequiredResourceRoles(resourceName, permissionName, siteName);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    bool checkGlobal = false;
                    string site = siteName;
                    if (ValidationHelper.GetInteger(dr["SiteID"], 0) == 0)
                    {
                        checkGlobal = true;
                        site = String.Empty;
                    }

                    // If user is member of the specified role, authorize user
                    if (userInfo.IsInRole(Convert.ToString(dr["RoleName"]), site, checkGlobal, true))
                    {
                        result = true;
                        break;
                    }
                }
            }

            // Initiate the authorization
            SecurityEvents.AuthorizeResource.StartResourceEvent(userInfo, resourceName, permissionName, ref result);

            return result;
        }


        /// <summary>
        /// Changes resource name to "CMS.Design" for global permission resource with SQL or Code edit permission
        /// </summary>
        /// <remarks>
        /// This change is necessary due to backward compatibility with legacy part of the system with exploitable inputs but with no
        /// specific permission e.g Form layouts
        /// </remarks>
        /// <param name="resourceName">Reference to permission value</param>
        /// <param name="permissionName">Permission name</param>
        private static void ChangeResourceNameToDesignForGlobalResourceAndSqlOrCodePermission(ref string resourceName, string permissionName)
        {
            var comparer = StringComparison.InvariantCultureIgnoreCase;

            if (GLOBAL_PERMISSIONS_RESOURCE.Equals(resourceName, comparer) && (EDIT_CODE_PERMISSION.Equals(permissionName, comparer) || EDIT_SQL_CODE_PERMISSION.Equals(permissionName, comparer)))
            {
                resourceName = DESIGN_RESOURCE;
            }
        }


        /// <summary>
        /// Returns true if permission name under the resource is known as SQL/ASCX code permission allowed for exploitable action 
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        private static bool IsSqlOrCodePermission(ref string resourceName, string permissionName)
        {
            bool result = false;

            var comparer = StringComparison.InvariantCultureIgnoreCase;

            // Check resource name and permission name, do not authorize for code editing by default
            switch (resourceName.ToLowerInvariant())
            {
                // Editing of SQL queries in reporting
                case REPORTING_RESOURCE:
                    if (EDIT_SQL_QUERIES_PERMISSION.Equals(permissionName, comparer))
                    {
                        result = true;
                    }
                    break;

                // Editing of SQL or ASCX code in design mode
                case DESIGN_RESOURCE:
                    if (EDIT_CODE_PERMISSION.Equals(permissionName, comparer) || EDIT_SQL_CODE_PERMISSION.Equals(permissionName, comparer))
                    {
                        result = true;
                    }
                    break;

                // Editing of SQL queries in form fields
                case FORM_RESOURCE:
                    if (EDIT_SQL_QUERIES_PERMISSION.Equals(permissionName, comparer))
                    {
                        result = true;
                    }
                    break;
            }

            // Return positive value => continue with permission check
            return result;
        }


        /// <summary>
        /// Indicates if user is authorized to see the specified UI element.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementName">Name of the UIElement</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        public static bool IsAuthorizedPerUIElement(string resourceName, string elementName, string siteName, UserInfo userInfo)
        {
            return IsAuthorizedPerUIElement(resourceName, new[] { elementName }, siteName, userInfo);
        }


        /// <summary>
        /// Indicates if user is authorized to see the specified UI element.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementNames">Name of the UIElement</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        public static bool IsAuthorizedPerUIElement(string resourceName, IEnumerable<string> elementNames, string siteName, UserInfo userInfo)
        {
            return IsAuthorizedPerUIElement(resourceName, elementNames, siteName, userInfo, false);
        }


        /// <summary>
        /// Indicates if user is authorized to see the specified UI element.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementNames">Name of the UIElement</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="checkElementsOnly">Indicates if only UI elements will be checked, not settings or special role membership</param>
        public static bool IsAuthorizedPerUIElement(string resourceName, IEnumerable<string> elementNames, string siteName, UserInfo userInfo, bool checkElementsOnly)
        {
            return IsAuthorizedPerUIElement(resourceName, elementNames, siteName, userInfo, false, true);
        }


        /// <summary>
        /// Indicates if user is authorized to see the specified UI element.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementNames">Name of the UIElement</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="checkElementsOnly">Indicates if only UI elements will be checked, not settings or special role membership</param>
        /// <param name="checkGlobalRoles">Indicates if global roles should be checked</param>
        public static bool IsAuthorizedPerUIElement(string resourceName, IEnumerable<string> elementNames, string siteName, UserInfo userInfo, bool checkElementsOnly, bool checkGlobalRoles)
        {
            // If no userInfo given, not authorized
            if (userInfo == null)
            {
                return false;
            }

            // If no elements given, return false
            if ((elementNames == null) || !elementNames.Any())
            {
                return false;
            }

            // Check for membership in special roles and settings
            if (!checkElementsOnly)
            {
                // Global admin can always access elements
                if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    return true;
                }

                // If the UIPersonalization is not on, return true
                if (!IsUIPersonalizationEnabled(siteName))
                {
                    return true;
                }
            }

            // Get the table of elements for the site
            var elements = userInfo.ResourceUIElementsInternal[siteName.ToLowerInvariant()];
            bool result = IsAuthorizedPerUIElementInternal(elements, elementNames, userInfo, resourceName);

            // Check global roles
            if (checkGlobalRoles && !result)
            {
                elements = userInfo.ResourceUIElementsInternal[UserInfo.GLOBAL_ROLES_KEY];
                result = IsAuthorizedPerUIElementInternal(elements, elementNames, userInfo, resourceName);
            }

            return result;
        }


        /// <summary>
        /// Indicates if user is authorized to see the specified UI element.
        /// </summary>
        /// <param name="elements">Collection with elements rights and validity</param>
        /// <param name="elementNames">Names of elements</param>
        /// <param name="userInfo">User info</param>
        /// <param name="resourceName">Name of resource</param>
        private static bool IsAuthorizedPerUIElementInternal(SafeDictionary<string, DateTime?> elements, IEnumerable<string> elementNames, UserInfo userInfo, string resourceName)
        {
            bool result = true;
            if (elements != null)
            {
                // Check every element
                foreach (string elementName in elementNames)
                {
                    if (!String.IsNullOrEmpty(elementName))
                    {
                        bool authorized = false;
                        string key = $"{resourceName}.{elementName}".ToLowerInvariant();
                        if (elements.Contains(key))
                        {
                            DateTime validTo = ValidationHelper.GetDateTime(elements[key], DateTimeHelper.ZERO_TIME);
                            if ((validTo == DateTimeHelper.ZERO_TIME) || (validTo > DateTimeNow))
                            {
                                authorized = true;
                            }
                        }

                        // Initiate the authorization
                        SecurityEvents.AuthorizeUIElement.StartUIElementEvent(userInfo, resourceName, elementName, ref result);

                        if (!authorized)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }


        /// <summary>
        /// Determines whether UI personalization is enabled for the specified site.
        /// </summary>
        /// <param name="siteName">Code name of the site to check</param>
        private static bool IsUIPersonalizationEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSPersonalizeUserInterface");
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="permission">Permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        public static bool IsAuthorizedPerMetaFile(string objectType, PermissionsEnum permission, string siteName, UserInfo userInfo)
        {
            return IsAuthorizedPerObject(objectType, permission, siteName, userInfo);
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="info">InfoObject to check</param>
        /// <param name="permission">Permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        public static bool IsAuthorizedPerObject(BaseInfo info, PermissionsEnum permission, string siteName, UserInfo userInfo)
        {
            return IsAuthorizedPerObject(info, permission, siteName, userInfo, false);
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="info">InfoObject to check</param>
        /// <param name="permission">Permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsAuthorizedPerObject(BaseInfo info, PermissionsEnum permission, string siteName, UserInfo userInfo, bool exceptionOnFailure)
        {
            if (info != null)
            {
                return info.CheckPermissions(permission, siteName, userInfo, exceptionOnFailure);
            }

            return false;
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="permission">Permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsAuthorizedPerObject(string objectType, PermissionsEnum permission, string siteName, UserInfo userInfo, bool exceptionOnFailure)
        {
            return IsAuthorizedPerObject(ModuleManager.GetReadOnlyObject(objectType), permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="permission">Permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        public static bool IsAuthorizedPerObject(string objectType, PermissionsEnum permission, string siteName, UserInfo userInfo)
        {
            return IsAuthorizedPerObject(objectType, permission, siteName, userInfo, false);
        }


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="permission">Permission</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userInfo">User info object</param>
        public static bool IsAuthorizedPerObject(string objectType, int objectId, PermissionsEnum permission, string siteName, UserInfo userInfo)
        {
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            BaseInfo info = ProviderHelper.GetInfoById(objectType, objectId);
            if (info != null)
            {
                return IsAuthorizedPerObject(info, permission, siteName, userInfo);
            }

            return false;
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="privilegeLevel">Privilege level of user to be checked</param>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public static bool CheckLicense(UserPrivilegeLevelEnum privilegeLevel = UserPrivilegeLevelEnum.None, ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null)
        {
            domainName = domainName ?? RequestContext.CurrentDomain;
            var feature = FeatureEnum.SiteMembers;

            switch (privilegeLevel)
            {
                case UserPrivilegeLevelEnum.GlobalAdmin:
                case UserPrivilegeLevelEnum.Admin:
                    feature = FeatureEnum.Administrators;
                    break;
                case UserPrivilegeLevelEnum.Editor:
                    feature = FeatureEnum.Editors;
                    break;
            }

            // Check number of users
            if (!LicenseVersionCheck(domainName, feature, action, false))
            {
                LicenseHelper.GetAllAvailableKeys(feature);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns object query for users with settings data
        /// </summary>
        public static ObjectQuery<UserInfo> GetUsersDataWithSettings()
        {
            return GetUsers()
                .From("View_CMS_User");
        }


        /// <summary>
        /// Returns dataset with users with their settings according to WHERE condition sorted by ORDER BY expression.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY expression</param>        
        [Obsolete("Use method GetUsersDataWithSettings() instead")]
        public static InfoDataSet<UserInfo> GetFullUsers(string where, string orderBy)
        {
            return GetFullUsers(where, orderBy, 0, null);
        }


        /// <summary>
        /// Returns dataset with users with their settings according to WHERE condition sorted by ORDER BY expression.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY expression</param> 
        /// <param name="topN">Top N users</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        [Obsolete("Use method GetUsersDataWithSettings() instead")]
        public static InfoDataSet<UserInfo> GetFullUsers(string where, string orderBy, int topN, string columns)
        {
            var query = GetUsersDataWithSettings().Where(new WhereCondition(where)).TopN(topN).Columns(columns);

            if (!string.IsNullOrEmpty(orderBy))
            {
                string direction;
                var column = SqlHelper.GetOrderByColumnName(orderBy, out direction);

                query = SqlHelper.ORDERBY_DESC.Equals(direction, StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(column)
                    : query.OrderByAscending(column);
            }

            return query.TypedResult;
        }


        /// <summary>
        /// Returns the DataSet with user permissions for specified resource.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="resourceId">ID of the resource</param>
        /// <param name="displayedOnly">Indicates if only visible permissions should be get</param>
        /// <param name="columns">Columns to get</param>
        public static DataSet GetUserResourcePermissions(UserInfo user, int siteId, int resourceId, bool displayedOnly, string columns)
        {
            return ProviderObject.GetUserPermissions(user, siteId, resourceId, ResourceInfo.OBJECT_TYPE, displayedOnly, columns);
        }


        /// <summary>
        /// Returns the DataSet with user permissions for specified class.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="classId">ID of the class</param>
        /// <param name="displayedOnly">Indicates if only visible permissions should be get</param>
        /// <param name="columns">Columns to get</param>
        public static DataSet GetUserDataClassPermissions(UserInfo user, int siteId, int classId, bool displayedOnly, string columns)
        {
            return ProviderObject.GetUserPermissions(user, siteId, classId, DataClassInfo.OBJECT_TYPE, displayedOnly, columns);
        }


        /// <summary>
        /// Prepends a site specific prefix to the given user name.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="si">Site info object</param>
        public static String EnsureSitePrefixUserName(String userName, SiteInfo si)
        {
            return ProviderObject.EnsureSitePrefixUserNameInternal(userName, si);
        }


        /// <summary>
        /// Returns a site specific prefix.
        /// </summary>
        /// <param name="si">Site info object</param>
        public static String GetUserNameSitePrefix(SiteInfo si)
        {
            return ProviderObject.GetUserNameSitePrefixInternal(si);
        }


        /// <summary>
        /// Return user info by codename. If Site prefix switched on - test site prefix variant of user name first
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="si">Site info object</param>        
        public static UserInfo GetUserInfoForSitePrefix(String userName, SiteInfo si)
        {
            return ProviderObject.GetUserInfoForSitePrefixInternal(userName, si);
        }


        /// <summary>
        /// Returns true, is user name has site prefix
        /// </summary>
        /// <param name="userName">User name</param>
        public static bool IsSitePrefixedUser(String userName)
        {
            return ProviderObject.IsSitePrefixedUserInternal(userName);
        }


        /// <summary>
        /// Returns true, if site prefix for users is enabled
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UserNameSitePrefixEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseSitePrefixForUserName");
        }


        /// <summary>
        /// Gets user generic roles in format "'role1', 'role2'"
        /// </summary>
        /// <param name="userInfo">User info</param>
        public static string GetGenericRoles(UserInfo userInfo)
        {
            var genericRoles = userInfo.GetGenericRoles();

            return String.Join(",", genericRoles.Select(r => $"'{r}'"));
        }

        #endregion


        #region "Passwords"

        /// <summary>
        /// Sets the password for the specified user object, saves the user object.
        /// </summary>
        /// <param name="ui">User info</param>
        /// <param name="userPassword">User password</param>
        public static void SetPassword(UserInfo ui, string userPassword)
        {
            SetPassword(ui, userPassword, true);
        }


        /// <summary>
        /// Sets the password for the specified user object, does not save the user object.
        /// </summary>
        /// <param name="ui">User info</param>
        /// <param name="userPassword">User password</param>
        /// <param name="saveObject">Specifies whether object is saved into database.</param>
        public static void SetPassword(UserInfo ui, string userPassword, bool saveObject)
        {
            if (ui != null)
            {
                if (string.IsNullOrEmpty(userPassword))
                {
                    ui.SetValue("UserPassword", string.Empty);
                }
                else
                {
                    CheckUserForHashGeneration(ui);
                    ui.SetValue("UserPassword", GetPasswordHash(userPassword, NewPasswordFormat, ui.UserSalt));
                }

                // Log event
                EventLogProvider.LogEvent(EventType.INFORMATION, "Set password", "SETPASSWORD", $"New password was set to user {ui.UserName}");

                ui.PasswordFormat = NewPasswordFormat;
                ui.UserPasswordLastChanged = DateTimeNow;

                if (saveObject)
                {
                    SetUserInfo(ui);
                }
            }
        }


        /// <summary>
        /// Checks if password hash can be generated properly.
        /// </summary>
        /// <param name="ui">User info.</param>
        private static void CheckUserForHashGeneration(UserInfo ui)
        {
            if (SHA2SALT.Equals(ui.PasswordFormat, StringComparison.OrdinalIgnoreCase))
            {
                if ((ui.UserID == 0) && (ui.UserGUID == Guid.Empty))
                {
                    throw new InfoObjectException(ui, "Given info is not already existing user. Password in SHA2 with salt as user GUID could not be generated.");
                }
            }
        }


        /// <summary>
        /// Sets the password for the specified user. Saves object to database.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="userPassword">User password</param>
        public static void SetPassword(string userName, string userPassword)
        {
            SetPassword(userName, userPassword, true);
        }


        /// <summary>
        /// Sets the password for the specified user.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="userPassword">User password</param>
        /// <param name="saveObject">Specifies whether object is saved into database.</param>
        public static void SetPassword(string userName, string userPassword, bool saveObject)
        {
            UserInfo ui = GetUserInfo(userName);
            SetPassword(ui, userPassword, saveObject);
        }

        #endregion


        #region "Password helper methods"

        /// <summary>
        /// Returns the hashed password representation (is hashing on).
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <param name="passwordFormat">Format of the password</param>
        /// <param name="salt">Password salt</param>
        public static string GetPasswordHash(string password, string passwordFormat, string salt)
        {
            return ProviderObject.GetPasswordHashInternal(password, passwordFormat, salt);
        }


        /// <summary>
        /// Indicates if user password differs from specified password
        /// </summary>
        /// <param name="ui">User to check password difference</param>
        /// <param name="password">Password to check</param>
        /// <returns>True if specified password differs from user password, False if the passwords are the same</returns>
        public static bool IsUserPasswordDifferent(UserInfo ui, string password)
        {
            if (ui == null)
            {
                return true;
            }

            // Domain users must be validated against AD when using mixed mode
            if (ui.UserIsDomain && RequestHelper.IsMixedAuthentication())
            {
                MembershipProvider adProvider = System.Web.Security.Membership.Providers["CMSADProvider"];
                return (adProvider == null) || !adProvider.ValidateUser(ui.UserName, password);
            }

            return !ValidateUserPassword(ui, password);
        }


        /// <summary>
        /// Compares hash of given password with user's hash stored in database.  
        /// </summary>
        /// <param name="ui">User whose password hash should be checked</param>
        /// <param name="password">Password to check</param>
        /// <returns>True if the given password's hash and user's password hash are the same, false otherwise.</returns>
        public static bool ValidateUserPassword(UserInfo ui, string password)
        {
            // Do not validate password for public user
            if (ui.IsPublic())
            {
                return false;
            }

            string userhash = ValidationHelper.GetString(ui.GetValue("UserPassword"), string.Empty);

            // Special treatment for blank passwords
            if (String.IsNullOrEmpty(password) && String.IsNullOrEmpty(userhash))
            {
                return true;
            }

            if (PBKDF2.Equals(ui.PasswordFormat, StringComparison.OrdinalIgnoreCase))
            {
                return SecurityHelper.VerifyPBKDF2Hash(password, userhash);
            }

            CheckUserForHashGeneration(ui);
            var passhash = GetPasswordHash(password, ui.PasswordFormat, ui.UserSalt);

            // Ignore case in comparing passwords when password format is different than plain text
            var comparer = string.IsNullOrEmpty(ui.PasswordFormat) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            return string.Compare(userhash, passhash, comparer) == 0;
        }


        /// <summary>
        /// Returns new password according to password policy.
        /// Generates password with 8 characters (at least one special character) if no password policy is configured.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static string GenerateNewPassword(string siteName)
        {
            return ProviderObject.GenerateNewPasswordInternal(siteName);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Validates the user name. Returns true if the user name is valid and follows rules defined by <see cref="ValidationHelper.UsernameRegExp"/>.
        /// </summary>
        /// <remarks>
        /// User names of external users are only checked for emptiness as they can have any user name since different services have different rules.
        /// </remarks>
        /// <param name="info">User info object to check</param>
        public override bool ValidateCodeName(UserInfo info)
        {
            var username = info.UserName;

            // For external users check if the user name is empty
            if (info.IsExternal)
            {
                return !String.IsNullOrEmpty(username);
            }

            // Check user name specific rules (allowed @,\ etc.)
            return ValidationHelper.IsUserName(username);
        }


        /// <summary>
        /// Returns formatted username in format: fullname (nickname) if nicname specified otherwise fullname (username).
        /// Allows you to customize how the usernames will look like throughout the admin UI. 
        /// </summary>
        /// <param name="username">Source user name</param>
        /// <param name="fullname">Source full name</param>
        /// <param name="nickname">Source user nick name</param>
        /// <param name="isLiveSite">Indicates if returned username should be displayed on live site</param>
        public static string GetFormattedUserName(string username, string fullname, string nickname = null, bool isLiveSite = false)
        {
            // Try to use external handler
            if (OnFormattedUserName != null)
            {
                return OnFormattedUserName(username, fullname, nickname, isLiveSite);
            }

            // Prefer nick name over user name if provided
            var name = DataHelper.GetNotEmpty(nickname, string.Empty).Trim();

            if (String.IsNullOrEmpty(name))
            {
                name = DataHelper.GetNotEmpty(username, string.Empty).Trim();
            }

            // Trim the site prefix
            name = TrimSitePrefix(name);
            fullname = DataHelper.GetNotEmpty(fullname, "").Trim();

            if (isLiveSite)
            {
                return !String.IsNullOrEmpty(fullname) ? fullname : name;
            }

            // Complete name with full name if available or user name only
            return !String.IsNullOrEmpty(fullname) ? $"{fullname} ({name})" : name;
        }

        #endregion


        #region "Membership methods"

        /// <summary>
        /// Returns user with specified Facebook Connect ID.
        /// </summary>
        /// <param name="facebookUserId">Facebook Connect ID</param>
        public static UserInfo GetUserInfoByFacebookConnectID(string facebookUserId)
        {
            if (!string.IsNullOrEmpty(facebookUserId))
            {
                return GetUsersDataWithSettings()
                    .WhereEquals("UserFacebookID", facebookUserId)
                    .FirstObject;
            }

            return null;
        }


        /// <summary>
        /// Returns user with specified LinkedIn ID.
        /// </summary>
        /// <param name="linkedInUserId">LinkedIn profile Id</param>
        public static UserInfo GetUserInfoByLinkedInID(string linkedInUserId)
        {
            if (!string.IsNullOrEmpty(linkedInUserId))
            {
                return GetUsersDataWithSettings()
                    .WhereEquals("UserLinkedInID", linkedInUserId)
                    .FirstObject;
            }

            return null;
        }


        /// <summary>
        /// Returns UserInfo related to WindowsLive ID.
        /// </summary>
        /// <param name="userLiveId">Windows LiveID</param>
        /// <returns>Returns UserInfo if found, otherwise returns NULL</returns>
        public static UserInfo GetUserInfoByWindowsLiveID(string userLiveId)
        {
            if (!string.IsNullOrEmpty(userLiveId))
            {
                return GetUsersDataWithSettings()
                    .WhereEquals("WindowsLiveID", userLiveId)
                    .FirstObject;
            }

            return null;
        }


        /// <summary>
        /// Checks license limitation for provider UserInfo. Checks limitations for global administrator, editor and site members.
        /// </summary>
        /// <param name="ui">UserInfo to be checked</param>
        /// <param name="error">Will contain error message if license check failed</param>
        /// <returns>Returns TRUE if limitation check is passed. Returns FALSE otherwise.</returns>
        public static bool CheckLicenseLimitation(UserInfo ui, ref string error)
        {
            // Check limitations for Global administrator
            if (ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                if (!LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Administrators, ObjectActionEnum.Insert, false))
                {
                    error = ResHelper.GetString("License.MaxItemsReachedGlobal");
                    return false;
                }
            }

            // Check limitations for editors
            if (ui.SiteIndependentPrivilegeLevel == UserPrivilegeLevelEnum.Editor)
            {
                if (!LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Editors, ObjectActionEnum.Insert, false))
                {
                    error = ResHelper.GetString("License.MaxItemsReachedEditor");
                    return false;
                }
            }

            // Check limitations for site members
            if (!LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.SiteMembers, ObjectActionEnum.Insert, false))
            {
                error = ResHelper.GetString("License.MaxItemsReachedSiteMember");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns user info object with settings according to where condition.
        /// </summary>
        /// <param name="where">Where condition.</param>
        [Obsolete("Use method GetUsersDataWithSettings() instead")]
        public static UserInfo GetUserInfoWithSettings(string where)
        {
            return ProviderObject.GetUserInfoWithSettingsInternal(where);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets (updates or inserts) specified user.
        /// </summary>
        /// <param name="user">User to set</param>
        protected virtual void SetUserInfoInternal(UserInfo user)
        {
            if (user != null)
            {
                // For saving information if user is added or updated
                bool newUser = false;
                List<string> changedColumns;

                using (var tr = BeginTransaction())
                {
                    // Set user xml data
                    user.SetValue("UserLastLogonInfo", user.UserLastLogonInfo.GetData());

                    if (user.UserID == 0)
                    {
                        user.SetValue("UserCreated", DateTimeNow);

                        // Default values
                        if (ValidationHelper.GetString(user.GetValue("UserPassword"), "__empty__") == "__empty__")
                        {
                            user.SetValue("UserPassword", string.Empty);
                        }

                        newUser = true;
                    }

                    changedColumns = user.ChangedColumns().Union(user.UserSettings.ChangedColumns()).ToList();

                    SetInfo(user);

                    // Keep user settings
                    UserSettingsInfo usi = user.UserSettings;

                    // Save user settings
                    if (user.UserGUID != Guid.Empty)
                    {
                        usi.UserSettingsUserGUID = user.UserGUID;
                    }
                    usi.UserSettingsUserID = user.UserID;

                    if (newUser || (usi.UserSettingsID > 0))
                    {
                        using (CMSActionContext ctx = new CMSActionContext())
                        {
                            // Do not touch parent since it is being updated
                            ctx.TouchParent = false;

                            if (newUser)
                            {
                                // Ensure empty user settings user id for cloned object
                                usi.UserSettingsID = 0;

                                // Set badge to user
                                BadgeInfoProvider.UpdateUserBadge(user);
                            }

                            UserSettingsInfoProvider.SetUserSettingsInfo(usi);
                        }
                    }

                    user.Generalized.Invalidate(true);

                    // Commit transaction
                    tr.Commit();
                }

                // Update on-line users table
                OnlineUserHelper.UpdateSessions(user, changedColumns, OnlineUserHelper.SessionType.User);

                // Refresh the license information
                ClearLicenseValues();

                if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, UserInfo.OBJECT_TYPE, user.TypeInfo.IDColumn, user.UserID.ToString(), user.UserID);
                }
            }
            else
            {
                throw new Exception("[UserInfoProvider.SetUserInfo]: No UserInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(UserInfo info)
        {
            if (info != null)
            {
                // Check if not public
                if (info.IsPublic())
                {
                    throw new Exception("[Userinfo.DeleteUser]: Cannot delete public user.");
                }

                // Delete the data
                base.DeleteInfo(info);

                if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, UserInfo.OBJECT_TYPE, info.TypeInfo.IDColumn, info.UserID.ToString(), info.UserID);
                }
            }

            ClearLicenseValues();
        }


        /// <summary>
        /// Return user info by codename. If Site prefix switched on - test site prefix variant of user name first
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="si">Site info object</param>    
        protected virtual UserInfo GetUserInfoForSitePrefixInternal(String userName, SiteInfo si)
        {
            UserInfo result = null;

            // If Site name prefix enabled
            if ((si != null) && (UserNameSitePrefixEnabled(si.SiteName)))
            {
                // First try login user name prefix
                String userNameWithPrefix = EnsureSitePrefixUserName(userName, si);
                result = GetUserInfo(userNameWithPrefix);
            }

            // If site prefix user not found or function disabled - try log global user
            return result ?? GetUserInfo(userName);
        }


        /// <summary>
        /// Trimes site prefix from user name (if any prefix found)
        /// </summary>
        /// <param name="username">User name</param>
        protected virtual String TrimSitePrefixInternal(string username)
        {
            // Find site prefix
            Match m = SitePrefixRegex.Match(username);

            // If prefix found
            if (m.Groups.Count > 2)
            {
                // If GUID is present
                String guid = m.Groups[1].Value;
                if (ValidationHelper.IsGuid(guid))
                {
                    username = m.Groups[2].Value;
                }
            }

            return username;
        }


        /// <summary>
        /// Returns true, is user name has site prefix
        /// </summary>
        /// <param name="userName">User name</param>
        protected virtual bool IsSitePrefixedUserInternal(String userName)
        {
            // Test user name for 'site.{GUID}.name'
            Match m = SitePrefixRegex.Match(userName);

            // Found results
            if (m.Groups.Count > 1)
            {
                // Test if GUID is present
                String guid = m.Groups[1].Value;
                if (ValidationHelper.IsGuid(guid))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Prepends a site specific prefix to the given user name.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="si">Site info object</param>
        protected virtual String EnsureSitePrefixUserNameInternal(String userName, SiteInfo si)
        {
            var sitePrefix = GetUserNameSitePrefix(si);

            return $"{sitePrefix}{userName}";
        }


        /// <summary>
        /// Returns a site specific prefix.
        /// </summary>
        /// <param name="si">Site info object</param>
        protected virtual String GetUserNameSitePrefixInternal(SiteInfo si)
        {
            if (si != null)
            {
                return String.Format(SITE_PREFIX_TEMPLATE, si.SiteGUID);
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns the table of the user roles given by membership connection.
        /// </summary>
        /// <param name="userInfo">User info for retrieving the role table</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N records</param>
        /// <param name="columns">Columns to get</param>
        protected virtual DataTable GetUserMembershipRolesInternal(UserInfo userInfo, string where, string orderBy, int topN, string columns)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userInfo.UserID);
            parameters.Add("@Date", DateTimeNow);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("CMS.User.SelectRolesByMembership", parameters, where, orderBy, topN, columns);
            if ((ds == null) || (ds.Tables.Count == 0))
            {
                return null;
            }
            else
            {
                return ds.Tables[0];
            }
        }


        /// <summary>
        /// Returns the table of the user roles.
        /// </summary>
        /// <param name="userInfo">User info for retrieving the role table</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N records</param>
        /// <param name="columns">Columns to get</param>
        /// <param name="includeMembership">If true, membership roles are added</param>
        /// <param name="includeGlobal">If true, global roles are added</param>
        /// <param name="checkValidity">If true, only valid roles are selected</param>
        protected virtual DataTable GetUserRolesInternal(UserInfo userInfo, string where, string orderBy, int topN, string columns, bool includeMembership, bool includeGlobal, bool checkValidity)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userInfo.UserID);

            // Get user generic roles
            var genRoleWhere = GetGenericRoles(userInfo);

            parameters.AddMacro("##GENERICROLES##", genRoleWhere);

            if (checkValidity)
            {
                parameters.Add("@ValidTo", DateTimeNow);
                where = SqlHelper.AddWhereCondition(where, "(ValidTo IS NULL OR ValidTo > @ValidTo)");
            }

            if (!includeGlobal)
            {
                where = SqlHelper.AddWhereCondition(where, "SiteID IS NOT NULL");
            }

            // Get the data
            string queryName = includeMembership ? "CMS.User.SelectMembershipUserRoles" : "CMS.User.SelectRoles";
            DataSet ds = ConnectionHelper.ExecuteQuery(queryName, parameters, where, orderBy, topN, columns);

            if ((ds == null) || (ds.Tables.Count == 0))
            {
                return null;
            }
            return ds.Tables[0];
        }


        /// <summary>
        /// Returns the table of the user sites.
        /// </summary>
        /// <param name="userId">User ID for site table</param>
        protected virtual ObjectQuery<SiteInfo> GetUserSitesInternal(int userId)
        {
            return SiteInfoProvider.GetSites().WhereIn("SiteID",
                UserSiteInfoProvider.GetUserSites().WhereEquals("UserID", userId).Column("SiteID"));
        }


        /// <summary>
        /// Returns the UserName by the specified user ID.
        /// </summary>
        /// <param name="id">User ID</param>
        protected virtual string GetUserNameByIdInternal(int id)
        {
            var userNames = GetObjectQuery().WhereEquals("UserID", id).Column("UserName").GetListResult<string>();
            return userNames.Any() ? userNames.First() : string.Empty;
        }


        /// <summary>
        /// Returns true if user is granted with specified permission for particular class.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name for which check the permissions</param>
        /// <param name="userInfo">User info object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected virtual bool IsAuthorizedPerClassInternal(string className, string permissionName, string siteName, UserInfo userInfo, bool exceptionOnFailure)
        {
            // If no userInfo given, not authorized
            if (userInfo == null)
            {
                return false;
            }

            bool result = false;

            // Get Class info
            DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(className);
            if (ci == null)
            {
                throw new Exception("[UserInfoProvider.IsAuthorizedPerClassInternal]: Class '" + className + "' not found.");
            }

            // Get site info
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[UserInfoProvider.IsAuthorizedPerClassInternal]: Site name '" + siteName + "' not found.");
            }

            // Check the roles
            DataSet ds = RoleInfoProvider.GetRequiredClassRoles(className, permissionName, siteName);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    bool checkGlobal = false;
                    string site = siteName;
                    if (ValidationHelper.GetInteger(dr["SiteID"], 0) == 0)
                    {
                        checkGlobal = true;
                        site = String.Empty;
                    }

                    // If user is member of the specified role, authorize user
                    if (userInfo.IsInRole(Convert.ToString(dr["RoleName"]), site, checkGlobal, true))
                    {
                        result = true;
                        break;
                    }
                }
            }

            // Initiate the authorization
            SecurityEvents.AuthorizeClass.StartClassEvent(userInfo, className, permissionName, ref result);

            if (exceptionOnFailure && !result)
            {
                throw new PermissionCheckException(className, permissionName, siteName);
            }

            return result;
        }


        /// <summary>
        /// Gets the DataSet of the required users for the specified resource permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N items</param>
        /// <param name="columns">List of columns to return</param>
        protected virtual InfoDataSet<UserInfo> GetRequiredResourceUsersInternal(string resourceName, string permissionName, string siteName, string where, string orderBy, int topN, string columns)
        {
            // Get site info
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[UserInfoProvider.GetRequiredResourceUsersInternal]: Site with name '" + siteName + "' not found.");
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ResourceName", resourceName);
            parameters.Add("@PermissionName", permissionName);
            parameters.Add("@SiteID", si.SiteID);
            parameters.Add("@Now", DateTimeNow);
            parameters.EnsureDataSet<UserInfo>();

            // Get the list of roles required for the given permission
            return ConnectionHelper.ExecuteQuery("CMS.Resource.SelectRequiredUsers", parameters, where, orderBy, topN, columns).As<UserInfo>();
        }


        /// <summary>
        /// Returns user info object with settings according to where condition.
        /// </summary>
        /// <param name="where">Where condition.</param>
        [Obsolete("Use method GetUsersDataWithSettings() instead")]
        protected virtual UserInfo GetUserInfoWithSettingsInternal(string where)
        {
            DataSet ds = GetFullUsers(where, null, 1, null);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get user info
                return new UserInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns the DataSet of user permissions for permission type specified by ID.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="permissionTypeId">ID identifying the permission</param>
        /// <param name="permissionType">Type of the permission(class or resource)</param>
        /// <param name="displayedOnly">Indicates if only visible permissions should be get</param>
        /// <param name="columns">Columns to get</param>
        protected virtual DataSet GetUserPermissions(UserInfo user, int siteId, int permissionTypeId, string permissionType, bool displayedOnly, string columns)
        {
            // Get query name to use
            string queryName;
            switch (permissionType)
            {
                case DataClassInfo.OBJECT_TYPE:
                    queryName = "cms.permission.getClassPermissionMatrix";
                    break;

                case ResourceInfo.OBJECT_TYPE:
                    queryName = "cms.permission.getResourcePermissionMatrix";
                    break;

                default:
                    throw new Exception($"Object type '{permissionType}' is not supported");
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", permissionTypeId);
            parameters.Add("@SiteID", siteId);
            parameters.Add("@DisplayInMatrix", displayedOnly);

            string where = null;
            bool noRoles = false;

            string siteWhere = (siteId > 0) ? "SiteID=" + siteId : "SiteID IS NULL";

            // Get user roles            
            DataTable roleTable = GetUserRoles(user, siteWhere, String.Empty, 0, "RoleName");
            if ((roleTable != null) && (roleTable.Rows.Count > 0))
            {
                StringBuilder sbWhere = new StringBuilder();
                foreach (DataRow dr in roleTable.Rows)
                {
                    string roleName = ValidationHelper.GetString(dr["RoleName"], String.Empty);
                    sbWhere.Append(",'");
                    sbWhere.Append(SqlHelper.EscapeQuotes(roleName));
                    sbWhere.Append("'");
                }

                // Set role name where condition
                where = sbWhere.ToString();
                if (!String.IsNullOrEmpty(where))
                {
                    where = where.Remove(0, 1);
                    where = "RoleName IN (" + where + ")";
                }
            }
            else
            {
                noRoles = true;
            }

            // Ensure required PermissionID column
            if (!String.IsNullOrEmpty(columns) && !columns.ToLowerInvariant().Contains("permissionid"))
            {
                columns += ",Matrix.PermissionID";
            }

            // Get data from DB
            DataSet ds = ConnectionHelper.ExecuteQuery(queryName, parameters, where, "Matrix.PermissionOrder,Matrix.PermissionID", -1, columns);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ArrayList deleteRows = new ArrayList();
                int processedPermission = 0;
                bool hasResult = false;

                // Filter permission entries
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];

                    // Get data from DataRow
                    int permissiondId = ValidationHelper.GetInteger(dr["PermissionID"], 0);
                    bool allowed = ValidationHelper.GetBoolean(dr["Allowed"], false);

                    // If new permission processed, set result to unknow(false)
                    if (permissiondId != processedPermission)
                    {
                        // Set new permission as processed
                        processedPermission = permissiondId;

                        // If positive result wasn't found for last permission, restore last row so we have entry for permission
                        if (!hasResult && (deleteRows.Count > 0))
                        {
                            deleteRows.RemoveAt(deleteRows.Count - 1);
                        }

                        // Reset result marker 
                        hasResult = false;
                    }

                    // Preserve 1 positive result for every permission
                    if (!hasResult && ((!noRoles && allowed) || noRoles))
                    {
                        hasResult = true;

                        // If user in no role selected first item and set it as result with allowed set to no
                        if (noRoles && allowed)
                        {
                            dr["Allowed"] = 0;
                        }
                    }
                    else
                    {
                        deleteRows.Add(dr);
                    }
                }

                // Solve last item
                if (!hasResult && (deleteRows.Count > 0))
                {
                    deleteRows.RemoveAt(deleteRows.Count - 1);
                }

                // Remove unnecessary data
                foreach (DataRow dr in deleteRows)
                {
                    ds.Tables[0].Rows.Remove(dr);
                }
            }

            return ds;
        }


        /// <summary>
        /// Returns the hashed password representation (is hashing on).
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <param name="passwordFormat">Format of the password</param>
        /// <param name="salt">Password salt</param>
        protected virtual string GetPasswordHashInternal(string password, string passwordFormat, string salt)
        {
            passwordFormat = passwordFormat?.ToLowerInvariant().Trim();

            switch (passwordFormat)
            {
                // SHA1 hash of the password
                case "sha1":
                    return SecurityHelper.GetSHA1Hash(password);

                // SHA2 hash
                case "sha2":
                    return SecurityHelper.GetSHA2Hash(password);

                // SHA2 with salt
                case SHA2SALT:
                    return SecurityHelper.GetSHA2Hash(password + salt + PasswordSalt);

                // PBKDF2
                case PBKDF2:
                    return SecurityHelper.GetPBKDF2Hash(password);

                // Plain text
                default:
                    return password;
            }
        }


        /// <summary>
        /// Returns new password according to password policy.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        protected virtual string GenerateNewPasswordInternal(string siteName)
        {
            bool policyEnabled = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUsePasswordPolicy");

            if (!policyEnabled)
            {
                // Generate default password - 8 characters (at least 1 special character)
                return System.Web.Security.Membership.GeneratePassword(8, 1);
            }

            int length = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSPolicyMinimalLength");
            int numberOfNonAlphaNum = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSPolicyNumberOfNonAlphaNumChars");

            // Length must be positive number or zero, default length is 8
            if (length <= 0)
            {
                length = 8;
            }

            // Number of non alphanum chars must be positive number or zero
            if (numberOfNonAlphaNum <= 0)
            {
                numberOfNonAlphaNum = 0;
            }

            // Number of non-alphanumeric characters cannot be higher than whole length
            if (numberOfNonAlphaNum > length)
            {
                numberOfNonAlphaNum = length;
            }

            return System.Web.Security.Membership.GeneratePassword(length, numberOfNonAlphaNum);
        }


        /// <summary>
        /// Updates user object in hashtables
        /// </summary>
        /// <param name="user">User object</param>
        internal static void UpdateUserInHashtablesInternal(UserInfo user)
        {
            if (user != null)
            {
                user.Generalized.Invalidate(false);
                ProviderObject.DeleteObjectFromHashtables(user);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Ensures that roles and sites for the given user are present within the database.
        /// </summary>
        /// <param name="user">Source user info object</param>
        /// <param name="isWindowsAuth">Indicates if ensuring Windows authentication roles.</param>
        private static void EnsureRolesAndSitesInternal(UserInfo user, bool isWindowsAuth)
        {
            // Get current user info to get current sites and roles status
            var oldUser = new UserInfo(user, true, false);

            // Remove user from sites he has left
            foreach (DictionaryEntry siteRecord in oldUser.SitesRoles)
            {
                string siteName = siteRecord.Key.ToString();

                // Do not process for global roles
                if (!siteName.Equals(UserInfo.GLOBAL_ROLES_KEY, StringComparison.OrdinalIgnoreCase) && !user.IsInSite(siteName))
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                    if (si != null)
                    {
                        // Remove the binding
                        UserSiteInfoProvider.RemoveUserFromSite(user.UserID, si.SiteID);
                    }
                }
            }

            var userAlreadyInSites = UserSiteInfoProvider.GetUserSites()
                                                         .Columns("SiteID")
                                                         .WhereEquals("UserID", user.UserID)
                                                         .Select(userSite => userSite.SiteID);

            // Process user's new and existing sites
            foreach (DictionaryEntry siteRecord in user.SitesRoles)
            {
                string siteName = siteRecord.Key.ToString();

                // Do not process global roles
                if (!siteName.Equals(UserInfo.GLOBAL_ROLES_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);

                    if (si != null)
                    {
                        var currentRoles = oldUser.SitesRoles[si.SiteName.ToLowerInvariant()] ?? new SafeDictionary<string, int?>();
                        var newRoles = (SafeDictionary<string, int?>)siteRecord.Value ?? new SafeDictionary<string, int?>();

                        var toDelete = currentRoles.TypedKeys.Except(newRoles.TypedKeys).ToList();
                        var toAdd = newRoles.TypedKeys.Except(currentRoles.TypedKeys).ToList();

                        if (!userAlreadyInSites.Contains(si.SiteID))
                        {
                            UserSiteInfoProvider.AddUserToSite(user, si);
                        }

                        // Remove old roles
                        foreach (string role in toDelete)
                        {
                            RoleInfo ri = RoleInfoProvider.GetRoleInfo(role, si.SiteID);

                            if (ri != null)
                            {
                                // Remove windows role binding only in case that role is domain
                                if (!isWindowsAuth || ri.RoleIsDomain)
                                {
                                    // Remove the binding
                                    var uri = UserRoleInfoProvider.GetUserRoleInfo(user.UserID, ri.RoleID);
                                    UserRoleInfoProvider.DeleteUserRoleInfo(uri);
                                }
                            }
                        }

                        // Add new roles
                        foreach (string role in toAdd)
                        {
                            string roleName = role.Trim();

                            if (!String.IsNullOrEmpty(roleName))
                            {
                                RoleInfo ri = RoleInfoProvider.GetRoleInfo(roleName, si.SiteName);

                                if (ri == null)
                                {
                                    lock (mEnsureRolesLock)
                                    {
                                        ri = RoleInfoProvider.GetRoleInfo(roleName, si.SiteName);

                                        if (ri == null)
                                        {
                                            // Create new role
                                            ri = new RoleInfo
                                            {
                                                RoleName = roleName,
                                                RoleDisplayName = roleName,
                                                RoleIsDomain = isWindowsAuth,
                                                SiteID = si.SiteID
                                            };

                                            RoleInfoProvider.SetRoleInfo(ri);
                                        }
                                    }
                                }

                                // Add the binding
                                UserRoleInfoProvider.AddUserToRole(user, ri);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
