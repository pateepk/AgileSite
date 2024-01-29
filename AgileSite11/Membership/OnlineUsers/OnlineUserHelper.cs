using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Serves for updating on-line users.
    /// </summary>
    public static class OnlineUserHelper
    {
        #region "Public enumerations"

        /// <summary>
        /// Type of session.
        /// </summary>
        public enum SessionType
        {
            /// <summary>
            /// Session based on user profile.
            /// </summary>
            User = 0,

            /// <summary>
            /// Session based on contact profile.
            /// </summary>
            Contact = 1
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Updates all CMS_Session records which have matching foreign key ID corresponding given object type.
        /// </summary>
        /// <param name="info">Info object containing data</param>
        /// <param name="changedColumns">Columns which were changed in the info object</param>
        /// <param name="type">Type of the object</param>
        public static void UpdateSessions(ISimpleDataContainer info, ICollection<string> changedColumns, SessionType type)
        {
            var importantColumns = GetImportantColumns(type);
            if (!CheckPrerequisites(type) || !importantColumns.Keys.Intersect(changedColumns).Any())
            {
                return;
            }
            
            var data = GetUpdataData(info, changedColumns, type);
            var where = GetWhereCondition(info, type);
            OnlineUserInfoProvider.UpdateData(where, data);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Check licenses and settings.
        /// </summary>
        private static bool CheckPrerequisites(SessionType type)
        {
            // Check if storing on-line users into DB is turned on.
            if (!SettingsKeyInfoProvider.GetBoolValue("CMSSessionUseDBRepository"))
            {
                return false;
            }

            // Check online users license
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.OnlineUsers))
            {
                return false;
            }

            // Check contact management license
            if ((type == SessionType.Contact) &&
                (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement)
                || !ResourceSiteInfoProvider.IsResourceOnSite("CMS.ContactManagement", SiteContext.CurrentSiteName)
                || !ModuleEntryManager.IsModuleLoaded(ModuleName.CONTACTMANAGEMENT)))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns where condition for bulk update.
        /// </summary>
        private static WhereCondition GetWhereCondition(ISimpleDataContainer info, SessionType type)
        {
            return type == SessionType.User ? 
                new WhereCondition("SessionUserID", QueryOperator.Equals, info["UserID"]) 
                : new WhereCondition("SessionContactID", QueryOperator.Equals, info["ContactID"]);
        }


        /// <summary>
        /// Returns data for bulk update.
        /// </summary>
        private static IDictionary<string, object> GetUpdataData(ISimpleDataContainer info, IEnumerable<string> changedColumns, SessionType type)
        {
            var importantColumns = GetImportantColumns(type);
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (string column in changedColumns)
            {
                if (importantColumns.ContainsKey(column))
                {
                    if ((column == "ContactFirstName") || (column == "ContactMiddleName") || (column == "ContactLastName"))
                    {
                        if (!result.ContainsKey("SessionFullName"))
                        {
                            result.Add("SessionFullName", GetContactFullName(info));
                        }
                    }
                    else if (column == "UserNickName")
                    {
                        var nickname = ValidationHelper.GetString(((UserInfo)info).UserSettings["UserNickName"], string.Empty);
                        result.Add("SessionNickName", nickname);
                    }
                    else if (info[column] is DateTime)
                    {
                        result.Add(importantColumns[column], info[column]);
                    }
                    else
                    {
                        var value = ValidationHelper.GetString(info[column], string.Empty);
                        result.Add(importantColumns[column], value);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns columns which are important when updating user or contact.
        /// </summary>
        private static Dictionary<string, string> GetImportantColumns(SessionType type)
        {
            // Important user columns
            if (type == SessionType.User)
            {
                return new Dictionary<string, string>
                {
                    {"FullName", "SessionFullName"},
                    {"Email", "SessionEmail"},
                    {"UserName","SessionUserName"},
                    {"UserNickName","SessionNickName"},
                    {"UserCreated","SessionUserCreated"},
                };
            }

            // Important contact columns
            return new Dictionary<string, string>
            {
                {"ContactFirstName", "SessionFullName"},
                {"ContactMiddleName", "SessionFullName"},
                {"ContactLastName", "SessionFullName"},
                {"ContactEmail", "SessionEmail"},
                {"ContactCreated","SessionUserCreated"},
            };
        }


        /// <summary>
        /// Returns contact full name.
        /// </summary>
        private static string GetContactFullName(ISimpleDataContainer info)
        {
            string contactFullName = String.Empty;
            if (!String.IsNullOrEmpty((string)info["ContactFirstName"]))
            {
                contactFullName = (string)info["ContactFirstName"];
            }
            if (!String.IsNullOrEmpty((string)info["ContactMiddleName"]))
            {
                contactFullName += " " + (string)info["ContactMiddleName"];
            }
            if (!String.IsNullOrEmpty((string)info["ContactLastName"]))
            {
                contactFullName = contactFullName.Trim();
                contactFullName += " " + (string)info["ContactLastName"];
            }
            return contactFullName.Trim();
        }

        #endregion
    }

}
