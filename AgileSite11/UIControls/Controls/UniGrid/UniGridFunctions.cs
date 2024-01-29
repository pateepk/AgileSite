using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.UIControls
{
    /// <summary>
    /// Unigrid functions.
    /// </summary>
    public static class UniGridFunctions
    {
        #region "Data methods"

        /// <summary>
        /// Gets the data container from the given parameter (grid view row or data row view)
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public static IDataContainer GetDataContainer(object parameter)
        {
            var drv = GetDataRowView(parameter);
            if (drv != null)
            {
                return new DataRowContainer(drv.Row);
            }

            return null;
        }


        /// <summary>
        /// Gets the data row view from the given parameter (grid view row or data row view)
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public static DataRowView GetDataRowView(object parameter)
        {
            DataRowView drv;

            // Expect grid view row or data row view
            var gvr = parameter as GridViewRow;
            if (gvr != null)
            {
                drv = gvr.DataItem as DataRowView;
            }
            else
            {
                drv = parameter as DataRowView;
            }

            return drv;
        }


        /// <summary>
        /// Gets whole row for given cell.
        /// </summary>
        /// <param name="dcf">Row cell</param>
        /// <returns>Appropriate row</returns>
        public static DataRowView GetDataRowView(DataControlFieldCell dcf)
        {
            if ((dcf != null) && (dcf.Parent != null))
            {
                var gvr = dcf.Parent as GridViewRow;
                if (gvr != null)
                {
                    return gvr.DataItem as DataRowView;
                }
            }
            return null;
        }

        #endregion


        #region "General methods"

        /// <summary>
        /// Gets formatted user name by user ID
        /// </summary>
        /// <param name="parameter">User ID</param>
        public static object FormattedUserName(object parameter)
        {
            int userId = ValidationHelper.GetInteger(parameter, 0);
            UserInfo userInfo = UserInfoProvider.GetUserInfo(userId);
            if (userInfo != null)
            {
                return HTMLHelper.HTMLEncode(UserInfoProvider.GetFormattedUserName(userInfo.UserName, userInfo.FullName));
            }

            return parameter;
        }


        /// <summary>
        /// Gets user name by user ID
        /// </summary>
        /// <param name="parameter">User ID</param>
        public static object UserName(object parameter)
        {
            int userId = ValidationHelper.GetInteger(parameter, 0);
            UserInfo userInfo = UserInfoProvider.GetUserInfo(userId);
            if (userInfo != null)
            {
                return HTMLHelper.HTMLEncode(userInfo.UserName);
            }

            return parameter;
        }


        /// <summary>
        /// Gets country name by country ID
        /// </summary>
        /// <param name="parameter">Country ID</param>
        public static object CountryName(object parameter)
        {
            // CountryID => CountryDisplayName
            int countryId = ValidationHelper.GetInteger(parameter, 0);

            CountryInfo country = CountryInfoProvider.GetCountryInfo(countryId);
            if (country != null)
            {
                return HTMLHelper.HTMLEncode(country.CountryDisplayName);
            }

            return parameter;
        }


        /// <summary>
        /// Gets class display name by class ID
        /// </summary>
        /// <param name="parameter">Class ID</param>
        public static object ClassName(object parameter)
        {
            // ClassID => ClassDisplayName
            int classID = ValidationHelper.GetInteger(parameter, 0);

            DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(classID);
            if (dataClass != null)
            {
                return HTMLHelper.HTMLEncode(dataClass.ClassDisplayName);
            }

            return parameter;
        }


        /// <summary>
        /// Gets document name with document type icon.
        /// </summary>
        /// <param name="documentName">Document name</param>
        /// <param name="classID">Document type identifier</param>
        /// <param name="page">Page object</param>
        public static object DocumentNameWithIcon(string documentName, int classID, Page page)
        {
            string name = HTMLHelper.HTMLEncode(DataHelper.GetNotEmpty(documentName, "/"));
            var dataClass = DataClassInfoProvider.GetDataClassInfo(classID);

            if (dataClass != null)
            {
                return UIHelper.GetDocumentTypeIcon(page, dataClass.ClassName, ValidationHelper.GetString(dataClass.GetValue("ClassIconClass"), String.Empty)) + " " + name;
            }
            return null;
        }


        /// <summary>
        /// Gets culture short name by culture code
        /// </summary>
        /// <param name="parameter">Country ID</param>
        public static object CultureShortName(object parameter)
        {
            // CultureCode => CultureShortName
            string cultureCode = ValidationHelper.GetString(parameter, "");

            if (!string.IsNullOrEmpty(cultureCode))
            {
                CultureInfo culture = CultureInfoProvider.GetCultureInfo(cultureCode);
                if (culture != null)
                {
                    return culture.CultureShortName;
                }
            }

            return parameter;
        }


        /// <summary>
        /// Gets culture names by culture codes
        /// </summary>
        /// <param name="parameter">Culture codes separated by semicolon</param>
        public static object CultureName(object parameter)
        {
            string[] cultureCodes = ValidationHelper.GetString(parameter, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> result = new List<string>();

            foreach (var cultureCode in cultureCodes)
            {
                CultureInfo culture = CultureInfoProvider.GetCultureInfo(cultureCode);
                if (culture != null)
                {
                    result.Add(culture.CultureName);
                }
            }

            return (result.Count > 0) ? result.Join(", ") : parameter;
        }


        /// <summary>
        /// Gets culture short names by culture codes with flag and tooltip.
        /// </summary>
        /// <param name="parameter">Culture codes separated by semicolon</param>
        public static object CultureNameWithFlag(object parameter)
        {
            string[] cultureCodes = ValidationHelper.GetString(parameter, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();

            foreach (var cultureCode in cultureCodes)
            {
                CultureInfo culture = CultureInfoProvider.GetCultureInfo(cultureCode);
                if (culture != null)
                {
                    sb.AppendFormat("<span title=\"{0}\"><img class=\"cms-icon-80\" src=\"{1}\" alt=\"{2}\" />&nbsp;{3}</span>&nbsp;", HTMLHelper.EncodeForHtmlAttribute(culture.CultureName), UIHelper.GetFlagIconUrl(null, culture.CultureCode, "16x16"), HTMLHelper.EncodeForHtmlAttribute(culture.CultureName), HTMLHelper.HTMLEncode(culture.CultureShortName));
                }
            }

            return (sb.Length > 0) ? sb.ToString() : parameter;
        }


        /// <summary>
        /// Gets site name or (global) by site ID ID
        /// </summary>
        /// <param name="parameter">Site ID</param>
        public static object SiteNameOrGlobal(object parameter)
        {
            // Special case for setting global value or site name for objects. Source(=parameter) must be set to SiteID.
            int siteId = ValidationHelper.GetInteger(parameter, 0);

            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            if (site != null)
            {
                return HTMLHelper.HTMLEncode(site.DisplayName);
            }

            return ResHelper.GetString("general.global");
        }


        /// <summary>
        /// Gets site name by Site ID
        /// </summary>
        /// <param name="parameter">Site ID</param>
        public static object SiteName(object parameter)
        {
            // Special case for site name option
            int siteId = ValidationHelper.GetInteger(parameter, 0);

            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            if (site != null)
            {
                return HTMLHelper.HTMLEncode(site.DisplayName);
            }

            return parameter;
        }


        /// <summary>
        /// Returns colored 'yes'/'no' span (default CSS classes) according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColoredSpanYesNo(object value)
        {
            return ColoredSpanYesNo(value, false);
        }


        /// <summary>
        /// Returns colored 'yes'/'no' span (default CSS classes) according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColoredSpanYesNo(object value, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(value, nullValue))
            {
                return "<span class=\"StatusEnabled\">" + ResHelper.GetString("general.yes") + "</span>";
            }
            else
            {
                return "<span class=\"StatusDisabled\">" + ResHelper.GetString("general.no") + "</span>";
            }
        }


        /// <summary>
        /// Returns colored 'yes' span (default CSS classes) if value is null or DBNull. Returns 'no' in other cases.
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColoredSpanIsNullYesNo(object value)
        {
            return ColoredSpanYesNo((value == DBNull.Value) || value == null);
        }


        /// <summary>
        /// Returns colored div with background color specified by value parameter.
        /// </summary>
        /// <param name="value">Hexa color code</param>
        public static object ColorBar(object value)
        {
            if (ValidationHelper.IsColor(value))
            {
                return "<div class='StatusColorBar' style='background-color: " + HTMLHelper.EncodeForHtmlAttribute(value.ToString()) + ";'></div>";
            }

            return value;
        }


        /// <summary>
        /// Returns colorless 'yes'/'no' span according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColorLessSpanYesNo(object value)
        {
            return ColorLessSpanYesNo(value, false);
        }


        /// <summary>
        /// Returns colorless 'yes'/'no' span according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColorLessSpanYesNo(object value, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(value, nullValue))
            {
                return ResHelper.GetString("general.yes");
            }
            else
            {
                return ResHelper.GetString("general.no");
            }
        }


        /// <summary>
        /// Returns colored 'yes' span only (default CSS classes) according to boolean value.
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColoredSpanYes(object value)
        {
            return ColoredSpanYes(value, false);
        }


        /// <summary>
        /// Returns colored 'yes' span only (default CSS classes) according to boolean value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColoredSpanYes(object value, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(value, nullValue))
            {
                return "<span class=\"StatusEnabled\">" + ResHelper.GetString("general.yes") + "</span>";
            }
            return "";
        }


        /// <summary>
        /// Returns colored 'enabled'/'disabled' span (default CSS classes) according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColoredSpanEnabledDisabled(object value)
        {
            return ColoredSpanEnabledDisabled(value, false);
        }


        /// <summary>
        /// Returns colored 'enabled'/'disabled' span (default CSS classes) according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColoredSpanEnabledDisabled(object value, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(value, nullValue))
            {
                return "<span class=\"StatusEnabled\">" + ResHelper.GetString("general.enabled") + "</span>";
            }
            else
            {
                return "<span class=\"StatusDisabled\">" + ResHelper.GetString("general.disabled") + "</span>";
            }
        }


        /// <summary>
        /// Returns colored 'allowed'/'excluded' span (default CSS classes) according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColoredSpanAllowedExcluded(object value)
        {
            return ColoredSpanAllowedExcluded(value, false);
        }


        /// <summary>
        /// Returns colored 'allowed'/'excluded' span (default CSS classes) according to boolean value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColoredSpanAllowedExcluded(object value, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(value, nullValue))
            {
                return "<span class=\"StatusEnabled\">" + ResHelper.GetString("general.allowed") + "</span>";
            }
            else
            {
                return "<span class=\"StatusDisabled\">" + ResHelper.GetString("general.excluded") + "</span>";
            }
        }


        /// <summary>
        /// Returns colored 'yes'/'no' span (default CSS classes) according to boolean value reversed
        /// </summary>
        /// <param name="value">Value</param>
        public static string ColoredSpanYesNoReversed(object value)
        {
            return ColoredSpanYesNoReversed(value, false);
        }


        /// <summary>
        /// Returns colored 'yes'/'no' span (default CSS classes) according to boolean value reversed
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColoredSpanYesNoReversed(object value, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(value, nullValue))
            {
                return "<span class=\"StatusDisabled\">" + ResHelper.GetString("general.yes") + "</span>";
            }
            else
            {
                return "<span class=\"StatusEnabled\">" + ResHelper.GetString("general.no") + "</span>";
            }
        }


        /// <summary>
        /// Returns colored message (default CSS classes) according to boolean value (success/fail).
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="success">Value</param>
        public static string ColoredSpanMsg(string msg, bool success)
        {
            return ColoredSpanMsg(msg, success, false);
        }


        /// <summary>
        /// Returns colored message (default CSS classes) according to boolean value (success/fail).
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="success">Value</param>
        /// <param name="nullValue">Allows to specify boolean value for null</param>
        public static string ColoredSpanMsg(string msg, bool success, bool nullValue)
        {
            if (ValidationHelper.GetBoolean(success, nullValue))
            {
                return "<span class=\"OperationSucceeded\">" + msg + "</span>";
            }
            else
            {
                return "<span class=\"OperationFailed\">" + msg + "</span>";
            }
        }


        /// <summary>
        /// Returns span (of specified CSS class) containing message.
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="className">CSS class</param>
        public static string SpanMsg(string msg, string className)
        {
            return HTMLHelper.SpanMsg(msg, className);
        }


        /// <summary>
        /// Gets the culture display name.
        /// </summary>
        /// <param name="value">Culture code</param>
        public static string CultureDisplayName(object value)
        {
            string cultureCode = ValidationHelper.GetString(value, "");
            if (!string.IsNullOrEmpty(cultureCode))
            {
                CultureInfo ci = CultureInfoProvider.GetCultureInfo(ValidationHelper.GetString(value, ""));
                if (ci != null)
                {
                    return ResHelper.LocalizeString(ci.CultureName);
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the document name tooltip (Document name (Document name path)).
        /// </summary>
        /// <param name="data">Data row view with document data</param>
        public static string DocumentNameTooltip(DataRowView data)
        {
            string docName = DataHelper.GetNotEmpty(ValidationHelper.GetString(data["DocumentName"], ""), ResHelper.GetString("general.notspecified"));
            string docNamePath = ValidationHelper.GetString(data["DocumentNamePath"], String.Empty);
            bool isLink = ValidationHelper.GetInteger(DataHelper.GetDataRowViewValue(data, "NodeLinkedNodeID"), 0) != 0;
            string aliasPath = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(data, "NodeAliasPath"), docNamePath);
            string text = isLink ? aliasPath : docNamePath;

            docName = HTMLHelper.HTMLEncode(docName);
            var pathToDisplay = String.IsNullOrEmpty(text) ? String.Empty : String.Format("<div>{0}: {1}</div>", ResHelper.GetString("general.path"), HTMLHelper.HTMLEncode(text));

            string className = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(data, "ClassName"), string.Empty);
            if (className.EqualsCSafe(SystemDocumentTypes.File, true))
            {
                // Check if document is file with image attachment
                string extension = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(data, "DocumentType"), null);
                if (ImageHelper.IsImage(extension))
                {
                    int tooltipWidget = 0;
                    var url = DocumentURLProvider.GetUrl(aliasPath);

                    return UIHelper.GetTooltip(url, 0, 0, String.Empty, docName, pathToDisplay, "", ref tooltipWidget);
                }
            }

            return String.Format("<strong>{0}</strong>{1}", HTMLHelper.HTMLEncode(docName), pathToDisplay);
        }


        /// <summary>
        /// Gets the document culture name  with flag icon.
        /// </summary>
        /// <param name="data">Data row view with document data</param>
        /// <param name="page">Page</param>
        public static string DocumentCultureFlag(DataRowView data, Page page)
        {
            string cultureCode = ValidationHelper.GetString(data["DocumentCulture"], "");
            string cultureName = null;
            if (data.DataView.Table.Columns["CultureName"] != null)
            {
                cultureName = ValidationHelper.GetString(data["CultureName"], "");
            }
            return DocumentCultureFlag(cultureCode, cultureName, page);
        }


        /// <summary>
        /// Gets the document culture name  with flag icon.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="cultureName">Culture name</param>
        /// <param name="page">Page</param>
        public static string DocumentCultureFlag(string cultureCode, string cultureName, Page page)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                CultureInfo ci = CultureInfoProvider.GetCultureInfo(cultureCode);
                if (ci != null)
                {
                    cultureName = ci.CultureName;
                }
            }
            cultureName = ResHelper.LocalizeString(cultureName);

            return "<img class=\"cms-icon-80\" src=\"" + UIHelper.GetFlagIconUrl(page, cultureCode, "16x16") + "\" alt=\"" + cultureName + "\" />&nbsp;" + cultureName;
        }


        /// <summary>
        /// Indicates if object context menu should be displayed in UniGrid.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="siteName">Site name to check</param>
        public static bool ShowUniGridObjectContextMenu(GeneralizedInfo obj, string siteName)
        {
            if (obj == null)
            {
                return false;
            }

            CurrentUserInfo curUser = MembershipContext.AuthenticatedUser;
            var typeInfo = obj.TypeInfo;

            return 
                   // Export
                   (typeInfo.ImportExportSettings.AllowSingleExport && (curUser.IsAuthorizedPerResource("cms.globalpermissions", "ExportObjects", siteName)
                                                                           || curUser.IsAuthorizedPerResource("cms.globalpermissions", "BackupObjects", siteName)
                                                                           || curUser.IsAuthorizedPerResource("cms.globalpermissions", "RestoreObjects", siteName)))
                   // Destroy  
                   || (ObjectVersionManager.AllowObjectRestore(obj) && ObjectSupportsDestroy(obj) && curUser.IsAuthorizedPerObject(PermissionsEnum.Destroy, obj, siteName))
                   // Clone
                   || obj.AllowClone
                   // Move up/down
                   || (typeInfo.OrderColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && curUser.IsAuthorizedPerObject(PermissionsEnum.Modify, obj, siteName);
        }


        /// <summary>
        /// Indicates if object supports destroy operation
        /// </summary>
        /// <param name="obj">Object to check</param>
        public static bool ObjectSupportsDestroy(GeneralizedInfo obj)
        {
            switch (obj.TypeInfo.OriginalObjectType)
            {
                // True for explicitly allowed low level objects
                case DataClassInfo.OBJECT_TYPE:
                case QueryInfo.OBJECT_TYPE:
                case PredefinedObjectType.TRANSFORMATION:
                    return true;

                default:
                    return obj.TypeInfo.ProviderIsCustomizable;
            }
        }


        /// <summary>
        /// Gets type info nice name from it's object name. (E.g. cms.users -> Users)
        /// </summary>
        /// <param name="parameter">Type info name</param>
        /// <returns>Type info nice name</returns>
        public static object ObjectTypeName(object parameter)
        {
            string objectTypeName = ValidationHelper.GetString(parameter, "");
            if (!String.IsNullOrEmpty(objectTypeName))
            {
                ObjectTypeInfo objectTypeInfo = ObjectTypeManager.GetTypeInfo(objectTypeName);
                return objectTypeInfo != null ? objectTypeInfo.GetNiceObjectTypeName() : LocalizationHelper.GetString("general.objecttypenotfound");
            }
            else
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// HTML  encodes text.
        /// </summary>
        /// <param name="text">Text object to be HTML encoded</param>
        /// <returns>Encoded result text</returns>
        public static object HTMLEncode(object text)
        {
            return HTMLHelper.HTMLEncode(ValidationHelper.GetString(text, null));
        }


        /// <summary>
        /// Transforms URL to link. Link text is equal to input parameter.
        /// </summary>
        /// <param name="parameter">URL address.</param>
        public static object Link(object parameter)
        {
            string output = String.Empty;

            string url = ValidationHelper.GetString(parameter, null);
            if (url != null)
            {
                output = "<a target=\"_blank\" href=\"" + HTMLHelper.EncodeForHtmlAttribute(url) + "\">" + HTMLHelper.HTMLEncode(url) + "</a>";
            }

            return output;
        }


        /// <summary>
        /// Transforms content size in bytes to human readable form.
        /// </summary>
        /// <param name="parameter">Size in bytes.</param>
        public static object FileSize(object parameter)
        {
            string output = String.Empty;

            long size = ValidationHelper.GetLong(parameter, -1);
            if (size >= 0)
            {
                output = DataHelper.GetSizeString(size);
            }

            return output;
        }


        /// <summary>
        /// Transforms email address to mailto: link. Link text is equal to input parameter. 
        /// Return null for parameters not containing valid email address.
        /// </summary>
        /// <param name="parameter">Email address.</param>
        public static object MailTo(object parameter)
        {
            if (ValidationHelper.IsEmail(parameter))
            {
                return string.Format("<a class=\"mailto\" href=\"mailto:{0}\">{0}</a>", HTMLEncode(parameter));
            }

            return null;
        }


        /// <summary>
        /// Returns colored yes / no based on the published state of the document
        /// </summary>
        /// <param name="parameter">Document row with the data</param>
        public static object IsPublished(object parameter)
        {
            var dc = GetDataContainer(parameter);
            var published = DocumentHelper.GetPublished(dc);

            return ColoredSpanYesNo(published);
        }

        #endregion


        #region "Date and time"

        /// <summary>
        /// Gets date time with user time zone applied with GMT shift information.
        /// </summary>
        /// <param name="parameter">DateTime</param>
        public static object UserDateTimeGMT(object parameter)
        {
            var time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);

            if (time != DateTimeHelper.ZERO_TIME)
            {
                parameter = TimeZoneHelper.ConvertToUserTimeZone(time, true, MembershipContext.AuthenticatedUser, SiteContext.CurrentSite);
            }
            else
            {
                parameter = null;
            }

            return parameter;
        }


        /// <summary>
        /// Gets date time with user time zone applied without GMT shift information.
        /// </summary>
        /// <param name="parameter">DateTime</param>
        public static object UserDateTime(object parameter)
        {
            var time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);

            if (time != DateTimeHelper.ZERO_TIME)
            {
                parameter = TimeZoneHelper.ConvertToUserTimeZone(time, false, MembershipContext.AuthenticatedUser, SiteContext.CurrentSite);
            }
            else
            {
                parameter = null;
            }

            return parameter;
        }


        /// <summary>
        /// Gets time from the DateTime value
        /// </summary>
        /// <param name="parameter">DateTime</param>
        public static object UserTime(object parameter)
        {
            var time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);

            if (time != DateTimeHelper.ZERO_TIME)
            {
                parameter = TimeZoneHelper.ConvertToUserTimeZone(time, false, MembershipContext.AuthenticatedUser, SiteContext.CurrentSite, "T");
            }
            else
            {
                parameter = null;
            }

            return parameter;
        }


        /// <summary>
        /// Gets "long" string representation of used time zone (user, site or server) in form '(UTC+0:00) TimeZoneDisplayName'.
        /// </summary>
        /// <param name="parameter">DateTime</param>
        public static object UserTimeZoneName(object parameter)
        {
            var time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);

            if (time != DateTimeHelper.ZERO_TIME)
            {
                parameter = TimeZoneHelper.GetUTCLongStringOffset(MembershipContext.AuthenticatedUser, SiteContext.CurrentSite);
            }
            else
            {
                parameter = null;
            }

            return parameter;
        }


        /// <summary>
        /// Gets date from the DateTime value
        /// </summary>
        /// <param name="parameter">DateTime</param>
        public static object Date(object parameter)
        {
            var time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);
            if (time != DateTimeHelper.ZERO_TIME)
            {
                parameter = time.ToShortDateString();
            }
            else
            {
                parameter = null;
            }

            return parameter;
        }


        /// <summary>
        /// Gets time from the DateTime value
        /// </summary>
        /// <param name="parameter">DateTime</param>
        public static object Time(object parameter)
        {
            var time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);
            if (time != DateTimeHelper.ZERO_TIME)
            {
                parameter = time.ToString("T");
            }
            else
            {
                parameter = null;
            }

            return parameter;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets empty object with specified site ID. Usable to correct check permissions.
        /// Site ID is set from UIContext or use current site from SiteContext.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="siteId">Site ID</param>
        public static BaseInfo GetEmptyObjectWithSiteID(string objectType, int siteId)
        {
            var emptyObject = ModuleManager.GetObject(objectType);

            // Check if object has SiteID column
            if ((emptyObject == null) || (emptyObject.TypeInfo.SiteIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                return emptyObject;
            }
            
            // If no SiteID set via context and object does not support global objects - use current site ID
            if ((siteId == 0) && !emptyObject.TypeInfo.SupportsGlobalObjects)
            {
                siteId = SiteContext.CurrentSiteID;
            }

            emptyObject.Generalized.ObjectSiteID = siteId;

            return emptyObject;
        }

        #endregion
    }
}
