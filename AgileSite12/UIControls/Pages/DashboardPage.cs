using System;
using System.Web;

using CMS.DocumentEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Helpers;
using CMS.EventLog;
using CMS.PortalEngine;
using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class used for dashboard page.
    /// </summary>
    public class DashboardPage : CMSPage
    {
        /// <summary>
        /// If true, the page has been already redirected to the safe mode.
        /// </summary>
        private bool mRedirectedToSafeMode;

        private string mTemplateName;
        
        /// <summary>
        /// PreInit event handler.
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            // Ensure document manager
            EnsureDocumentManager = true;

            PortalContext.SetRequestViewMode(ViewModeEnum.DashboardWidgets);

            string dashboardName = QueryHelper.GetString("DashboardName", PersonalizationInfoProvider.UNDEFINEDDASHBOARD);
            PortalContext.DashboardName = dashboardName;

            // Set custom theme
            if (!String.IsNullOrEmpty(Theme))
            {
                Theme = URLHelper.CustomTheme;
            }

            mTemplateName = QueryHelper.GetString("templatename", String.Empty);

            InitPageTemplateAndVirtualPageInfo();

            base.OnPreInit(e);
        }


        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!CheckHashCode())
            {
                RedirectToAccessDenied(GetString("dashboard.invalidparameters"));
            }
        }


        /// <summary>
        /// Checks whether url parameters are valid.
        /// </summary>
        protected bool CheckHashCode()
        {
            // Get hashcode from querystring
            string hash = QueryHelper.GetString("hash", String.Empty);

            // Check whether url contains all reuired values
            if (QueryHelper.Contains("dashboardname") && !String.IsNullOrEmpty(hash))
            {
                // Try get custom hash values
                string hashValues = QueryHelper.GetString("hashvalues", String.Empty);

                string hashString = String.Empty;
                // Use default hash values
                if (String.IsNullOrEmpty(hashValues))
                {
                    hashString = QueryHelper.GetString("dashboardname", String.Empty) + "|" + mTemplateName;
                }
                // Use custom hash values
                else
                {
                    string[] values = hashValues.Split(';');
                    foreach (string value in values)
                    {
                        hashString += QueryHelper.GetString(value, String.Empty) + "|";
                    }

                    hashString = hashString.TrimEnd('|');
                }

                // Compare url hash with current hash
                return CMSString.Compare(hash, ValidationHelper.GetHashString(hashString, new HashSettings("")), false) == 0;
            }

            return false;
        }


        /// <summary>
        /// Error handler.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnError(EventArgs e)
        {
            Exception ex = HttpContext.Current.Server.GetLastError();
            
            // Check if the error should be processed
            if (mRedirectedToSafeMode)
            {
                return;
            }

            // Try to switch to the safe mode first
            if (!PortalHelper.SafeMode)
            {
                // Log the exception
                EventLogProvider.LogException("Dashboard", "Error", ex);

                mRedirectedToSafeMode = true;

                string url = RequestContext.CurrentURL;
                url = URLHelper.AddParameterToUrl(url, "safemode", "1");
                URLHelper.Redirect(url);
            }
            else
            {
                RequestContext.LogCurrentError = false;

                // Produce a nicer error message
                throw new Exception("There was an error processing the page. The error can be caused by the configuration of some component on the master page. Check the master page configuration or see event log for more details. Original message: " + ex.Message);
            }
        }


        /// <summary>
        /// Initializes page template info and virtual page info based on it 
        /// </summary>
        /// <remarks>Please make sure that this method is called in OnPreInit event of page because CMSPortalManager needs CurrentPageInfo in its Init phase</remarks>
        private void InitPageTemplateAndVirtualPageInfo()
        {
            var pageTemplate = PageTemplateInfoProvider.GetPageTemplateInfo(mTemplateName);
            if (pageTemplate != null)
            {
                if (pageTemplate.PageTemplateType != PageTemplateTypeEnum.Dashboard)
                {
                    RedirectToAccessDenied("dashboard.invalidpagetemplate");
                }

                // Prepare virtual page info
                PageInfo pi = PageInfoProvider.GetVirtualPageInfo(pageTemplate.PageTemplateId);
                pi.DocumentNamePath = "/" + mTemplateName;

                DocumentContext.CurrentPageInfo = pi;
            }
            else
            {
                RedirectToInformation(GetString("dashboard.notemplate"));
            }
        }
    }
}