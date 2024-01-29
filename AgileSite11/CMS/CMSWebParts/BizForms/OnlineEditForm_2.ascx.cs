using System;

using CMS.Core;
using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.Base.Web.UI;

public partial class CMSWebParts_BizForms_OnlineEditForm_2 : CMSAbstractWebPart
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
            return ValidationHelper.GetString(GetValue("FormRedirectURL", false), String.Empty);
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

        return result;
    }

    #endregion


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
            viewBiz.FormRedirectToUrl = viewBiz.ResolveMacros(FormRedirectURL);
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
		
		//if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        //{
        //    viewBiz.FormRedirectToUrl = viewBiz.ResolveMacros(FormRedirectURL);
        //}
		
		
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
			
			lbl1.Text = FormRedirectURL;
			
			//if (!String.IsNullOrWhiteSpace(FormRedirectURL))
			//{
			//	viewBiz.FormRedirectToUrl = viewBiz.ResolveMacros(FormRedirectURL);
			//	//lbl1.Text = FormRedirectURL;
			//}	

            // Set the live site context
            if (viewBiz != null)
            {
                viewBiz.ControlContext.ContextName = CMS.Base.Web.UI.ControlContext.LIVE_SITE;
            }
        }
    }


    private void viewBiz_OnAfterSave(object sender, EventArgs e)
    {

        if (TrackConversionName != String.Empty)
        {
            string siteName = SiteContext.CurrentSiteName;

            if (AnalyticsHelper.AnalyticsEnabled(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && !AnalyticsHelper.IsIPExcluded(siteName, RequestContext.UserHostAddress))
            {
                HitLogProvider.LogConversions(SiteContext.CurrentSiteName, LocalizationContext.PreferredCultureCode, TrackConversionName, 0, ConversionValue);
            }
        }
		
		lbl1.Visible = true;
		
		lbl1.Text = FormRedirectURL;
		
		if (!String.IsNullOrWhiteSpace(FormRedirectURL))
        {
			viewBiz.RedirectUrlAfterSave = "";
            //viewBiz.FormRedirectToUrl = viewBiz.ResolveMacros(FormRedirectURL);		
			string test1 = "/Building-Managers/Admin/Communication-Portal/Manage-Notification-Template/Edit-Notifcation-Template?templateid={?templateid?}&tid={%MOBLZ_Notification_TemplateID%}";
			lbl1.Text = viewBiz.FormRedirectToUrl + "|--|" + viewBiz.ResolveMacros(FormRedirectURL) + "|--|" + FormRedirectURL + "|--|" + viewBiz.ResolveMacros(test1) + viewBiz.ItemID;
			//URLHelper.Redirect(UrlResolver.ResolveUrl(viewBiz.ResolveMacros(FormRedirectURL)));
        }
    }

    #endregion
}