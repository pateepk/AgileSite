using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CMS.Base
{
    /// <summary>
    /// Objects containing UserInfo properties.
    /// </summary>
    public interface IUserInfo : IDataContainer
    {
        #region "Public properties"

        /// <summary>
        /// Gets the HashTable of the user sites and roles.
        /// </summary>
        SafeDictionary<string, SafeDictionary<string, int?>> SitesRoles
        {
            get;
        }


        /// <summary>
        /// Indicates if the user is hidden.
        /// </summary>
        bool UserIsHidden
        {
            get;
            set;
        }


        /// <summary>
        /// Last name of the user.
        /// </summary>
        string LastName
        {
            get;
            set;
        }


        /// <summary>
        /// Full name of the user.
        /// </summary>
        string FullName
        {
            get;
            set;
        }


        /// <summary>
        /// Last logon date and time of the user.
        /// </summary>
        DateTime LastLogon
        {
            get;
            set;
        }


        /// <summary>
        /// Preferred culture code.
        /// </summary>
        string PreferredCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Middle name of the user.
        /// </summary>
        string MiddleName
        {
            get;
            set;
        }


        /// <summary>
        /// Preferred UI culture code.
        /// </summary>
        string PreferredUICultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the user is enabled.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool UserEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// First name of the user.
        /// </summary>
        string FirstName
        {
            get;
            set;
        }


        /// <summary>
        /// Date and time when was the user created.
        /// </summary>
        DateTime UserCreated
        {
            get;
            set;
        }


        /// <summary>
        /// User ID.
        /// </summary>
        int UserID
        {
            get;
            set;
        }


        /// <summary>
        /// Format of the user's password.
        /// </summary>
        string UserPasswordFormat
        {
            get;
            set;
        }


        /// <summary>
        /// User name.
        /// </summary>
        string UserName
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail address of the user.
        /// </summary>
        string Email
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the user is enabled.
        /// </summary>
        bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Format of the user password.
        /// </summary>
        string PasswordFormat
        {
            get;
            set;
        }


        /// <summary>
        ///  Starting alias path of the user.
        /// </summary>
        string UserStartingAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the user has allowed more than one culture.
        /// </summary>
        bool UserHasAllowedCultures
        {
            get;
            set;
        }


        /// <summary>
        /// User GUID.
        /// </summary>
        Guid UserGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Date and time of the user last modification.
        /// </summary>
        DateTime UserLastModified
        {
            get;
            set;
        }


        /// <summary>
        /// Contains XML with user's custom form field visibility settings.
        /// </summary>
        string UserVisibility
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether user is domain user.
        /// </summary>
        bool UserIsDomain
        {
            get;
            set;
        }


        /// <summary>
        /// Temporary GUID for user identification for automatic sign-in in the CMS Desk.
        /// </summary>
        Guid UserAuthenticationGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the user is external.
        /// </summary>
        bool UserIsExternal
        {
            get;
            set;
        }

        #endregion


        #region "UserSettings properties"

        /// <summary>
        /// User picture.
        /// </summary>
        string UserPicture
        {
            get;
            set;
        }


        /// <summary>
        /// User avatar ID.
        /// </summary>
        int UserAvatarID
        {
            get;
            set;
        }


        /// <summary>
        /// User signature.
        /// </summary>
        string UserSignature
        {
            get;
            set;
        }


        /// <summary>
        /// User description.
        /// </summary>
        string UserDescription
        {
            get;
            set;
        }


        /// <summary>
        /// User nick name.
        /// </summary>
        string UserNickName
        {
            get;
            set;
        }


        /// <summary>
        /// URL Referrer of user.
        /// </summary>
        string UserURLReferrer
        {
            get;
            set;
        }


        /// <summary>
        /// User campaign.
        /// </summary>
        string UserCampaign
        {
            get;
            set;
        }


        /// <summary>
        /// User time zone ID
        /// </summary>
        int UserTimeZoneID
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the user is public user record.
        /// </summary>
        bool IsPublic();


        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName, bool exceptionOnFailure);


        /// <summary>
        /// Checks whether the user is authorized for given class name and permission, returns true if so.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        bool IsAuthorizedPerClassName(string className, string permissionName, string siteName, bool exceptionOnFailure);

        
        /// <summary>
        /// Filters the user search results
        /// </summary>
        /// <param name="inRoles">List of roles in which the user should be</param>
        /// <param name="notInRoles">List of roles in which the user should not be</param>
        /// <param name="addToIndex">Flag if the results should be added to the index</param>
        void FilterSearchResults(List<string> inRoles, List<string> notInRoles, ref bool addToIndex);


        /// <summary>
        /// Returns true if user fulfils the required privilege level (the higher level contains all children: GlobalAdmin -> Admin -> Editor -> None)
        /// </summary>
        /// <param name="privilegeLevel">Required privilege level</param>
        /// <param name="siteName">Site name for editor assignment. If not set current site name is used</param>
        bool CheckPrivilegeLevel(UserPrivilegeLevelEnum privilegeLevel, string siteName = null);

        #endregion
    }
}