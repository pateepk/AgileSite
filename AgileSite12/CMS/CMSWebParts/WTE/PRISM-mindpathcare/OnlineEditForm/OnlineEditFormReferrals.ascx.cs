using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.CustomTables;
using CMS.OnlineForms;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.WebAnalytics;

using CMS.Base.Web.UI.ActionsConfig;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Globalization;
using CMS.HealthMonitoring;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.UIControls;
using CMS.URLRewritingEngine;
using CMS.WebFarmSync;
using CMS.WinServiceEngine;

using Newtonsoft.Json;
using System.Runtime.Serialization.Json;

public partial class CMSWebParts_WTE_PRISM_mindpathcare_OnlineEditForm_OnlineEditFormReferrals : CMSAbstractWebPart
{
    #region "Properties"

    /// <summary>
    /// Gets or sets the form name of BizForm.
    /// </summary>
    public string BizFormName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("BizFormName"), "");
        }
        set
        {
            SetValue("BizFormName", value);
        }
    }

    /// <summary>
    /// Gets or sets the alternative form full name (ClassName.AlternativeFormName).
    /// </summary>
    public string AlternativeFormName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AlternativeFormName"), "");
        }
        set
        {
            SetValue("AlternativeFormName", value);
        }
    }

    /// <summary>
    /// Gets or sets the site name.
    /// </summary>
    public string SiteName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("SiteName"), "");
        }
        set
        {
            SetValue("SiteName", value);
        }
    }

    /// <summary>
    /// Gets or sets the value that indicates whether the WebPart use colon behind label.
    /// </summary>
    public bool UseColonBehindLabel
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("UseColonBehindLabel"), true);
        }
        set
        {
            SetValue("UseColonBehindLabel", value);
        }
    }

    /// <summary>
    /// Gets or sets the message which is displayed after validation failed.
    /// </summary>
    public string ValidationErrorMessage
    {
        get
        {
            return ValidationHelper.GetString(GetValue("ValidationErrorMessage"), "");
        }
        set
        {
            SetValue("ValidationErrorMessage", value);
        }
    }

    /// <summary>
    /// Gets or sets the conversion track name used after successful registration.
    /// </summary>
    public string TrackConversionName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("TrackConversionName"), "");
        }
        set
        {
            if (value.Length > 400)
            {
                value = value.Substring(0, 400);
            }
            SetValue("TrackConversionName", value);
        }
    }

    /// <summary>
    /// Gets or sets the conversion value used after successful registration.
    /// </summary>
    public double ConversionValue
    {
        get
        {
            return ValidationHelper.GetDoubleSystem(GetValue("ConversionValue"), 0);
        }
        set
        {
            SetValue("ConversionValue", value);
        }
    }

    /// <summary>
    /// Gets or sets the item ID
    /// </summary>
    public int ItemID
    {
        get
        {
            //return ValidationHelper.GetInteger(Request.QueryString["ItemID"], 0);
            return ValidationHelper.GetInteger(GetValue("ItemID"), 0);
        }
        set
        {
            SetValue("ItemID", value);
        }
    }

    /// <summary>
    /// Redirect URL
    /// </summary>
    public string FormRedirectURL
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FormRedirectURL"), "");
        }
        set
        {
            SetValue("FormRedirectURL", value);
        }
    }

    #region "custom logging"

    /// <summary>
    /// Enable Custom Logging
    /// </summary>
    public bool EnableCustomLogging
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("EnableCustomLogging"), true);
        }
        set
        {
            SetValue("EnableCustomLogging", value);
        }
    }

    #endregion "custom logging"

    #endregion "Properties"

    #region "Methods"

    protected override void OnLoad(EventArgs e)
    {
        viewBiz.OnAfterSave += viewBiz_OnAfterSave;
        if (ItemID > 0)
        {
            viewBiz.ItemID = ItemID;
        }

        if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        {
            viewBiz.FormRedirectToUrl = FormRedirectURL;
        }

        base.OnLoad(e);
    }

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();

        if (ItemID > 0)
        {
            viewBiz.ItemID = ItemID;
        }

        if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        {
            viewBiz.FormRedirectToUrl = FormRedirectURL;
        }

        SetupControl();
    }

    /// <summary>
    /// Reloads data for partial caching.
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();
        SetupControl();
    }

    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (StopProcessing)
        {
            // Do nothing
            viewBiz.StopProcessing = true;
        }
        else
        {
            // Set BizForm properties
            viewBiz.FormName = BizFormName;
            viewBiz.SiteName = SiteName;
            viewBiz.UseColonBehindLabel = UseColonBehindLabel;
            viewBiz.AlternativeFormFullName = AlternativeFormName;
            viewBiz.ValidationErrorMessage = ValidationErrorMessage;

            if (ItemID > 0)
            {
                viewBiz.ItemID = ItemID;
            }

            if (!String.IsNullOrWhiteSpace(FormRedirectURL))
            {
                viewBiz.FormRedirectToUrl = FormRedirectURL;
            }

            // Set the live site context
            if (viewBiz != null)
            {
                viewBiz.ControlContext.ContextName = CMS.Base.Web.UI.ControlContext.LIVE_SITE;
            }
        }
    }

    private void viewBiz_OnAfterSave(object sender, EventArgs e)
    {
        string error = String.Empty;
        bool logsucess = DoCustomLogging(out error);

        if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        {
            viewBiz.FormRedirectToUrl = viewBiz.ResolveMacros(FormRedirectURL);
        }

        if (TrackConversionName != String.Empty)
        {
            string siteName = SiteContext.CurrentSiteName;

            if (AnalyticsHelper.AnalyticsEnabled(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && !AnalyticsHelper.IsIPExcluded(siteName, RequestContext.UserHostAddress))
            {
                HitLogProvider.LogConversions(SiteContext.CurrentSiteName, LocalizationContext.PreferredCultureCode, TrackConversionName, 0, ConversionValue);
            }
        }
    }

    /// <summary>
    /// Log
    /// </summary>
    private bool DoCustomLogging(out string p_message)
    {
        bool success = true;
        p_message = String.Empty;
        DataSet ds = null;

        try
        {
            bool doCustomLogging = EnableCustomLogging;
            string workFlowSprocName = "custom_customLogging_LogChange";
            //string workFlowItemIDKey = WorkFlowItemIDKey;
            //string workFlowIDParamName = WorkFlowIDParamName;
            //string workFlowID = WorkFlowID;

            if (doCustomLogging)
            {
                GeneralConnection conn = ConnectionHelper.GetConnection();
                QueryDataParameters parameters = new QueryDataParameters();
                CurrentUserInfo currentUser = MembershipContext.AuthenticatedUser;

                int userid = currentUser.UserID;

                //parameters.Add("@" + workFlowItemIDParamName, p_companyID);
                parameters.Add("@UserID", userid);
                parameters.Add("@UpdatedItemID", viewBiz.ItemID);
                parameters.Add("@FormName", viewBiz.FormName);
                parameters.Add("@AlternativeFormName", viewBiz.AlternativeFormFullName);
                parameters.Add("@ActionID", null);
                parameters.Add("@Action", viewBiz.Mode);

                QueryParameters qp = new QueryParameters(workFlowSprocName, parameters, QueryTypeEnum.StoredProcedure, false);
                ds = conn.ExecuteQuery(qp);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    if (row != null)
                    {
                        p_message += GetStringOrNull(row["Message"]);
                        success = GetBoolOrNull(row["Success"]).GetValueOrDefault(true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // for now.
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        return success;
    }

    #region "Helpers

    /// <summary>
    /// Clear all caching
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected void ClearCache(object sender = null, EventArgs args = null)
    {
        if (StopProcessing)
        {
            return;
        }

        // Clear the cache
        CacheHelper.ClearCache(null, true);
        Functions.ClearHashtables();

        // Drop the routes
        CMSDocumentRouteHelper.DropAllRoutes();

        // Disposes all zip files
        ZipStorageProvider.DisposeAll();

        // Collect the memory
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // no need to log
        // Log event
        //EventLogProvider.LogEvent(EventType.INFORMATION, "System", "CLEARCACHE", GetString("Administration-System.ClearCacheSuccess"));

        //ShowConfirmation(GetString("Administration-System.ClearCacheSuccess"));
    }

    /// <summary>
    /// Get Session data
    /// </summary>
    /// <param name="p_key"></param>
    /// <returns></returns>
    protected string GetSession(string p_key)
    {
        string ret = String.Empty;
        try
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    ret = (string)HttpContext.Current.Session[p_key];
                }
            }
        }
        catch (Exception ex)
        {
            // for now.
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        return ret;
    }

    /// <summary>
    /// Get request parameter
    /// </summary>
    /// <param name="p_key"></param>
    /// <returns></returns>
    protected void SetSession(string p_key, object p_value)
    {
        try
        {
            if (p_value == null)
            {
                ClearSession(p_key);
            }
            else
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session[p_key] = p_value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // for now.
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get request parameter
    /// </summary>
    /// <param name="p_key"></param>
    /// <returns></returns>
    protected void ClearSession(string p_key)
    {
        try
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session.Remove(p_key);
                }
            }
        }
        catch (Exception ex)
        {
            // for now.
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get Int value or null
    /// </summary>
    /// <param name="p_object"></param>
    /// <returns></returns>
    protected int? GetIntOrNull(object p_object)
    {
        int? ret = null;
        int val = 0;
        if (int.TryParse(GetStringOrNull(p_object), out val))
        {
            ret = val;
        }
        return ret;
    }

    /// <summary>
    /// Get string value or null
    /// </summary>
    /// <param name="p_object"></param>
    /// <returns></returns>
    protected string GetStringOrNull(object p_object)
    {
        string ret = String.Empty;
        try
        {
            if (p_object != null)
            {
                ret = p_object.ToString();
            }
        }
        catch (Exception)
        {
            // ignore it
        }
        return ret;
    }

    /// <summary>
    /// Get boolean value of Null
    /// </summary>
    /// <param name="p_object"></param>
    /// <returns></returns>
    protected bool? GetBoolOrNull(object p_object)
    {
        bool? ret = null;
        bool val = false;
        if (bool.TryParse(GetStringOrNull(p_object), out val))
        {
            ret = val;
        }
        return ret;
    }

    #endregion "Helpers

    #endregion "Methods"
}