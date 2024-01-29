using System;

using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.WebAnalytics;

public partial class CMSWebParts_BizForms_bizform_1 : CMSAbstractWebPart
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
    public string ItemID
    {
        get
        {
            //return ValidationHelper.GetInteger(Request.QueryString["ItemID"], 0);
            return ValidationHelper.GetString(GetValue("ItemID"), "");
        }
        set
        {
            SetValue("ItemID", value);
        }
    }


    /// <summary>
    /// Gets or sets the item ID
    /// </summary>
    public int ItemIDInt
    {
        get
        {
            //return ValidationHelper.GetInteger(Request.QueryString["ItemID"], 0);
            return ValidationHelper.GetInteger(ItemID, 0);
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
            return ValidationHelper.GetString(GetValue("FormRedirectURL", false), String.Empty);
            //viewBiz.GetFieldValue
            //return ValidationHelper.GetString(GetValue("FormRedirectURL"), "");
        }
        set
        {
            SetValue("FormRedirectURL", value);
        }
    }


    /// <summary>
    /// Returns the value of the given web part property property.
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <param name="resolveMacros">If true, macros are resolved</param>
    private object GetValue(string propertyName, bool resolveMacros)
    {
        object result;

        if (String.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        propertyName = propertyName.ToLowerInvariant();

        // If no part instance, get local value
        if (PartInstance == null)
        {
            result = mLocalProperties[propertyName];
        }
        // Get the bound value
        else
        {
            // Get local value first
            object localValue = mLocalProperties[propertyName];
            result = localValue ?? PartInstance.GetValue(propertyName);
        }

        //// Localize the value
        //if (result is string)
        //{
        //    // If widget and not resolve properties not loaded yet
        //    if ((PartInstance != null) && PartInstance.IsWidget)
        //    {
        //        if (!widgetNotResolvePropertiesLoaded)
        //        {
        //            WidgetInfo wi = WidgetInfoProvider.GetWidgetInfo(PartInstance.WebPartType);
        //            if (wi != null)
        //            {
        //                string notResolveProperties = String.Empty;

        //                // Get properties from default values 
        //                switch (ParentZone.WidgetZoneType)
        //                {
        //                    case WidgetZoneTypeEnum.Editor:
        //                        notResolveProperties = GetNotResolvingProperties(wi.WidgetFields, "Container", "ContainerTitle", "ContainerCSSClass", "ContainerCustomContent");
        //                        break;

        //                    case WidgetZoneTypeEnum.Group:
        //                    case WidgetZoneTypeEnum.User:
        //                    case WidgetZoneTypeEnum.Dashboard:
        //                        notResolveProperties = GetNotResolvingProperties(wi.WidgetFields, "WidgetTitle");
        //                        break;
        //                }

        //                NotResolveProperties += wi.WidgetPublicFileds + notResolveProperties;
        //            }
        //            widgetNotResolvePropertiesLoaded = true;
        //        }

        //        // Localize value
        //        result = ResHelper.LocalizeString(result as string);
        //    }

        //    string stringResult = (string)result;

        //    // If allowed to be resolved, resolve the macros
        //    string pname = ";" + propertyName.ToLowerInvariant() + ";";

        //    bool resolve = !((NotResolveProperties.IndexOfCSafe(pname) >= 0) || (propertyName.Contains("transformation") && stringResult.StartsWithCSafe("[") && stringResult.EndsWithCSafe("]")));

        //    bool isSqlProperty = (mSQLProperties.IndexOfCSafe(pname) >= 0);
        //    if (resolveMacros && resolve)
        //    {
        //        // Resolve the macros using web part resolver
        //        MacroSettings settings = new MacroSettings();
        //        settings.Culture = Thread.CurrentThread.CurrentCulture.ToString();
        //        settings.AvoidInjection = isSqlProperty;

        //        result = ResolveMacros(stringResult, settings);
        //    }

        //    // Check SQL properties for malicious code
        //    if (isSqlProperty)
        //    {
        //        QueryScopeEnum scope;
        //        switch (pname)
        //        {
        //            case ";wherecondition;":
        //                scope = QueryScopeEnum.Where;
        //                break;
        //            case ";columns;":
        //                scope = QueryScopeEnum.Columns;
        //                break;
        //            case ";orderby;":
        //                scope = QueryScopeEnum.OrderBy;
        //                break;
        //            default:
        //                scope = QueryScopeEnum.None;
        //                break;
        //        }

        //        if ((scope != QueryScopeEnum.None) && !SqlSecurityHelper.CheckQuery(result as string, scope))
        //        {
        //            throw new SecurityException("An invalid SQL query was used.");
        //        }
        //    }

        //}

        // Automatically handle data source name if data source is hosted
        //if ((WebPartType == WebPartTypeEnum.BasicViewer) && DataHelper.IsEmpty(result) && propertyName.EqualsCSafe("datasourcename") && (PartInstance != null))
        //{
        //    // Try parent data source
        //    if ((ParentWebPart != null) && (ParentWebPart.WebPartType == WebPartTypeEnum.DataSource))
        //    {
        //        result = ParentWebPart.WebPartID;
        //    }
        //    else
        //    {
        //        // Try nested data source 
        //        var dataSource = PartInstance.NestedWebParts[WebPartInstance.DATASOURCE];
        //        if (dataSource != null)
        //        {
        //            result = dataSource.ControlID;
        //        }
        //    }
        //}

        return result;
    }

    #endregion


    #region "Methods"

    protected override void OnLoad(EventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        {
            viewBiz.FormRedirectToUrl = FormRedirectURL;
        }

        viewBiz.OnAfterSave += viewBiz_OnAfterSave;
        if (ItemIDInt > 0)
        {
            viewBiz.ItemID = ItemIDInt;
        }
        base.OnLoad(e);
    }


    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        if (ItemIDInt > 0)
        {
            viewBiz.ItemID = ItemIDInt;
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

            if (ItemIDInt > 0)
            {
                viewBiz.ItemID = ItemIDInt;
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

        if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        {
            viewBiz.FormRedirectToUrl = viewBiz.ResolveMacros(FormRedirectURL);
            viewBiz.RedirectUrlAfterSave = "";

        }

        if (TrackConversionName != String.Empty)
        {
            string siteName = SiteContext.CurrentSiteName;

            //if (AnalyticsHelper.AnalyticsEnabled(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && !AnalyticsHelper.IsIPExcluded(siteName, RequestContext.UserHostAddress))
            //{
            //    HitLogProvider.LogConversions(SiteContext.CurrentSiteName, LocalizationContext.PreferredCultureCode, TrackConversionName, 0, ConversionValue);
            //}
        }
    }

    #endregion
}