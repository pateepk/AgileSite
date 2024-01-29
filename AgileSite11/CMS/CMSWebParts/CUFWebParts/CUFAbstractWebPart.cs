//using System;
//using System.Web;
//using CMS.DataEngine;
//using CMS.Helpers;
//using CMS.Membership;
//using CMS.PortalControls;
//using CMS.PortalEngine;
//using CMS.SiteProvider;

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public abstract partial class CUFAbstractWebPart : CMSAbstractWebPart
    {
        #region Properties

        /// <summary>
        /// Get the current site site ID
        /// </summary>
        protected static int CurrentSiteID
        {
            get
            {
                int currentSiteID = SiteContext.CurrentSiteID;
                if (currentSiteID <= 0)
                {
                    SiteInfo test = SiteInfoProvider.GetRunningSiteInfo(RequestContext.CurrentDomain, "");
                    if (test == null)
                    {
                        //test
                        test = SiteInfoProvider.GetRunningSiteInfo("firstcitizens.cufs.ccesvc.com", "");
                    }
                    currentSiteID = test.SiteID;
                }

                return currentSiteID;
            }
        }

        #endregion Properties

        #region helpers

        #region redirection

        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        public static void RedirectToAccessDenied(string resourceName, string permissionName)
        {
            RedirectToAccessDeniedPage(GetAccessDeniedUrl(UIHelper.ADMIN_ACCESSDENIED_PAGE, resourceName, permissionName, null, null));
        }

        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="pageUrl">Access denied page URL</param>
        protected static void RedirectToAccessDeniedPage(string pageUrl)
        {
            SecurityDebug.LogSecurityOperation(MembershipContext.AuthenticatedUser.UserName, "RedirectToAccessDenied", null, null, null, SiteContext.CurrentSiteName);

            if (PortalContext.UIContext.IsDialog)
            {
                pageUrl = URLHelper.AddParameterToUrl(pageUrl, "dialog", "1");
            }

            URLHelper.Redirect(pageUrl);
        }

        /// <summary>
        /// Gets the URL to Access denied page
        /// </summary>
        /// <param name="pageUrl">Access denied page URL</param>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        /// <param name="uiElementName">UI element name that failed</param>
        /// <param name="message">Custom message</param>
        public static string GetAccessDeniedUrl(string pageUrl, string resourceName, string permissionName, string uiElementName, string message)
        {
            return UIHelper.GetAccessDeniedUrl(resourceName, permissionName, uiElementName, message, pageUrl);
        }

        /// <summary>
        /// Redirects the user to Access denied page
        /// </summary>
        /// <param name="message">Displayed access denied message.</param>
        public static void RedirectToAccessDenied(string message)
        {
            if (HttpContext.Current != null)
            {
                RedirectToAccessDeniedPage(GetAccessDeniedUrl(UIHelper.ADMIN_ACCESSDENIED_PAGE, null, null, null, message));
            }
        }

        /// <summary>
        /// Redirects the user to the info page which displays specified message.
        /// </summary>
        /// <param name="message">Message which should be displayed.</param>
        public static void RedirectToInformation(string message)
        {
            URLHelper.SeeOther(GetInformationUrl(message));
        }

        /// <summary>
        /// Gets URL for the info page which displays specified message.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <returns>URL of info page</returns>
        private static string GetInformationUrl(string message)
        {
            return UIHelper.GetInformationUrl(message);
        }

        #endregion redirection

        /// <summary>
        /// Get string property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected string GetSafeStringValue(string p_key, string p_default)
        {
            string value = p_default;

            object obj = GetValue(p_key);

            if (obj != null)
            {
                value = obj.ToString();
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = p_default;
            }

            return value;
        }

        /// <summary>
        /// Get Bool property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected bool GetSafeBoolValue(string p_key, bool p_default)
        {
            bool value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!bool.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        /// <summary>
        /// Get int value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected int GetSafeIntValue(string p_key, int p_default)
        {
            int value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!int.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        /// <summary>
        /// Redirect to a page
        /// </summary>
        /// <param name="p_url"></param>
        protected void RedirectToURL(string p_url)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                URLHelper.Redirect(ResolveUrl(p_url));
            }
        }

        /// <summary>
        /// Log CU membership changes
        /// </summary>
        /// <param name="p_logTypeID"></param>
        /// <param name="p_siteID"></param>
        /// <param name="p_shortDescription"></param>
        /// <param name="p_description"></param>
        /// <param name="p_userName"></param>
        /// <param name="p_updatedUser"></param>
        /// <param name="p_CUFMemberID"></param>
        /// <param name="p_IPAddress"></param>
        /// <param name="p_machineName"></param>
        /// <param name="p_urlReferrer"></param>
        /// <param name="p_eventDate"></param>
        /// <param name="p_message"></param>
        /// <param name="p_extraInfo"></param>
        /// <param name="p_errorMessage"></param>
        /// <returns></returns>
        protected bool LogCUMembershipChanges(int? p_logTypeID, int? p_siteID, string p_shortDescription, string p_description, string p_userName, string p_updatedUser, int? p_CUFMemberID, string p_IPAddress, string p_machineName, string p_urlReferrer, DateTime p_eventDate, string p_message, string p_extraInfo, out string p_errorMessage)
        {
            bool success = false;
            p_errorMessage = String.Empty;
            try
            {
                //populate drop down with available list of statements
                String dataDB = SettingsKeyInfoProvider.GetStringValue("StatementDatabase", SiteContext.CurrentSiteID);
                QueryDataParameters qdp = new QueryDataParameters();
                qdp.Add("LogTypeID", p_logTypeID);
                qdp.Add("SiteID", p_siteID);
                qdp.Add("ShortDescription", p_shortDescription);
                qdp.Add("Description", p_description);
                qdp.Add("UserName", p_userName);
                qdp.Add("UpdatedUser", p_updatedUser);
                qdp.Add("CUFMemberID", p_CUFMemberID);
                qdp.Add("IPAddress", p_IPAddress);
                qdp.Add("MachineName", p_machineName);
                qdp.Add("UrlReferrer", p_urlReferrer);
                qdp.Add("EventDate", p_eventDate);
                qdp.Add("Message", p_message);
                qdp.Add("ExtraInfo", p_extraInfo);
                ConnectionHelper.ExecuteNonQuery("dbo.CU_Proc_LogMemberChanges", qdp, QueryTypeEnum.StoredProcedure);
                //ConnectionHelper.ExecuteNonQuery(String.Format("{0}.dbo.sproc_LogMemberChanges", dataDB), qdp, QueryTypeEnum.StoredProcedure);
            }
            catch (Exception ex)
            {
                success = false;
                p_errorMessage = String.Format("Unable to log membership changes: {0}", ex.Message);
            }

            return success;
        }

        /// <summary>
        /// Get formatted short description for log
        /// </summary>
        /// <param name="p_logTypeID"></param>
        /// <returns></returns>
        protected string GetShortFormatedShortDescription(int p_logTypeID)
        {
            string desc = String.Empty;
            switch (p_logTypeID)
            {
                case 200: // member created
                case 300:
                    desc = "Member Created";
                    break;

                case 201: // member deleted
                case 301:
                    desc = "Member Deleted";
                    break;

                case 202: // member role added
                case 302:
                    desc = "Member Role Added";
                    break;

                case 203: // member role reomved
                case 303:
                    desc = "Member Role Removed";
                    break;

                case 204: // member updated
                case 304:
                    desc = "Member updated";
                    break;

                case 205: // member locked
                case 305:
                    desc = "Account locked";
                    break;

                case 206: // member unlocked
                case 306:
                    desc = "Account Unlocked";
                    break;

                case 207: // password reset
                case 307:
                    desc = "Password reset";
                    break;

                case 208: // password changed
                case 308:
                    desc = "Password changed";
                    break;

                case 209: // replicated user
                case 309:
                    desc = "Replicated User";
                    break;

                case 0: // message
                case 100: // general error
                case 1000: // debug message
                    break;
            }
            return desc;
        }

        /// <summary>
        /// Get formatted description for changed item
        /// </summary>
        /// <param name="p_fieldPrefix"></param>
        /// <param name="p_old"></param>
        /// <param name="p_new"></param>
        /// <returns></returns>
        protected string GetFormattedMembershipChanged(string p_fieldPrefix, string p_old, string p_new)
        {
            return String.Format(p_fieldPrefix + " changed from {0} to {1}.", p_old, p_new);
        }

        #endregion helpers
    }
}