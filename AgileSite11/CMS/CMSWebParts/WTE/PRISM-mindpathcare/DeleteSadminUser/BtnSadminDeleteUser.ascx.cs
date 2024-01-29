using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Activities.Loggers;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Protection;
using CMS.SiteProvider;
using CMS.WebAnalytics;

using CMS.DocumentEngine.Web.UI;
using CMS.CustomTables;

public partial class CMSWebParts_WTE_PRISM_mindpathcare_DeleteSadminUser_BtnSadminDeleteUser : CMSAbstractWebPart
{
    private const string urlRedirect = "https://referrals.mindpathcare.com/sadmin/Dashboard/Provider-Management/UserEdit/DeleteUser";

    #region "Properties"

    /// <summary>
    /// Link text
    /// </summary>
    public string LinkText
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("LinkText"), btnElem.LinkText);
        }
        set
        {
            this.SetValue("LinkText", value);
            btnElem.LinkText = value;
        }
    }

    /// <summary>
    /// Link CSS class
    /// </summary>
    public string LinkCssClass
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("LinkCssClass"), btnElem.CssClass);
        }
        set
        {
            this.SetValue("LinkCssClass", value);
            btnElem.CssClass = value;
        }
    }

    /// <summary>
    /// Show as button
    /// </summary>
    public bool ShowAsButton
    {
        get
        {
            return ValidationHelper.GetBoolean(this.GetValue("ShowAsButton"), btnElem.ShowAsButton);
        }
        set
        {
            this.SetValue("ShowAsButton", value);
            btnElem.ShowAsButton = value;
        }
    }

    /// <summary>
    /// Image URL
    /// </summary>
    public string ImageUrl
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ImageURL"), btnElem.ImageUrl);
        }
        set
        {
            this.SetValue("ImageURL", value);
            btnElem.ImageUrl = value;
        }
    }

    /// <summary>
    /// Image alternate text
    /// </summary>
    public string ImageAltText
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ImageAltText"), btnElem.ImageAltText);
        }
        set
        {
            this.SetValue("ImageAltText", value);
            btnElem.ImageAltText = value;
        }
    }

    /// <summary>
    /// Image CSS class
    /// </summary>
    public string ImageCssClass
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("ImageCssClass"), btnElem.ImageCssClass);
        }
        set
        {
            this.SetValue("ImageCssClass", value);
            btnElem.ImageCssClass = value;
        }
    }

    /// <summary>
    /// Link URL
    /// </summary>
    public string LinkUrl
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("LinkURL"), btnElem.LinkUrl);
        }
        set
        {
            this.SetValue("LinkURL", value);
            btnElem.LinkUrl = value;
        }
    }

    /// <summary>
    /// Link target
    /// </summary>
    public string LinkTarget
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("LinkTarget"), btnElem.LinkTarget);
        }
        set
        {
            this.SetValue("LinkTarget", value);
            btnElem.LinkTarget = value;
        }
    }

    /// <summary>
    /// Raise event
    /// </summary>
    public string LinkEvent
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("LinkEvent"), btnElem.LinkEvent);
        }
        set
        {
            this.SetValue("LinkEvent", value);
            btnElem.LinkEvent = value;
        }
    }

    /// <summary>
    /// Link javascript
    /// </summary>
    public string LinkJavascript
    {
        get
        {
            return ValidationHelper.GetString(this.GetValue("LinkJavascript"), btnElem.OnClientClick);
        }
        set
        {
            this.SetValue("LinkJavascript", value);
            btnElem.OnClientClick = value;
        }
    }

    /// <summary>
    /// Gets or sets CSS class to style the submit button.
    /// </summary>
    public string ButtonCSS
    {
        get
        {
            return ValidationHelper.GetString(GetValue("ButtonCSS"), string.Empty);
        }
        set
        {
            SetValue("ButtonCSS", value);
        }
    }

    /// <summary>
    /// Gets or sets submit button text.
    /// </summary>
    public string ButtonText
    {
        get
        {
            return DataHelper.GetNotEmpty(GetValue("ButtonText"), GetString("Webparts_Membership_RegistrationForm.Button"));
        }

        set
        {
            SetValue("ButtonText", value);
            btnRegister.Text = value;
        }
    }

    #endregion "Properties"

    #region "Methods"

    /// <summary>
    /// Content loaded event handler
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }

    /// <summary>
    /// Initializes the control properties
    /// </summary>
    protected void SetupControl()
    {
        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            // btnElem.LinkText = LinkText;
            // btnElem.CssClass = LinkCssClass;
            // btnElem.ShowAsButton = ShowAsButton;
            // btnElem.ImageUrl = ImageUrl;
            // btnElem.ImageAltText = ImageAltText;
            // btnElem.ImageCssClass = ImageCssClass;
            // btnElem.LinkUrl = LinkUrl;
            // btnElem.LinkTarget = LinkTarget;
            // btnElem.LinkEvent = LinkEvent;

            string linkJavascript = string.Empty;

            if (ShowAsButton)
            {
                // Ensure that the link will be opened in the specified target
                linkJavascript = ";openModal(); return false;";
            }

            btnRegister.Text = "Delete User";
            btnRegister.AddCssClass(ButtonCSS);
            btnRegister.Click += btnRegister_Click;
        }
    }

    /// <summary>
    /// Reloads the control data
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();

        SetupControl();
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        DeleteUser();
    }

    public void DeleteUser()
    {
        // Get current url then get the sAdmin id
        string urlString = HttpContext.Current.Request.Url.AbsoluteUri.ToString();
        urlString = urlString.Split('?')[1];
        string sAdminID = "";

        // GET ID
        if (!urlString.Contains('&'))
        { // OnlyContains one param
            if (urlString.Contains("refID"))
            {
                sAdminID = urlString.Replace("refID=", "");
            }
        }
        else
        {
            foreach (var x in urlString.Split('&'))
            {
                if (x.Contains("refID"))
                {
                    sAdminID = x.Replace("refID=", "");
                }
            }
        }

        // Get CMS_USER ID
        string userName = GetUserIDBySQL(int.Parse(sAdminID));

        if (userName != "")
        {
            UserInfo userInfo = UserInfoProvider.GetUserInfo(userName);
            UserInfoProvider.DeleteUser(userInfo);
            DeleteSAdminUser(int.Parse(sAdminID));
            // complete now redirect
            URLHelper.Redirect(urlRedirect);
        }
    }

    public string GetUserIDBySQL(int SAdminUserID)
    {
        string userName = "";

        try
        {
            DataSet ds2 = ConnectionHelper.ExecuteQuery(string.Format(@"SELECT [USR].[UserName]
FROM Form_MindPathCheckIn_SAdmin_Register [REG]
    LEFT JOIN CMS_User [USR] ON [REG].[UserName] = [USR].UserName AND [REG].[emailinput] = [USR].Email
WHERE SAdmin_RegisterID = {0}", SAdminUserID), null, QueryTypeEnum.SQLQuery);

            if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds2.Tables[0].Rows[0];
                userName = row["UserName"].ToString();
            }
            else
            {
                EventLogProvider.LogEvent(EventType.INFORMATION, "Registration Error", "CUSTOMREGESTRATION", eventDescription: "no id found");
            }
        }
        catch (Exception ex)
        {
            // Logs an information event into the event log
            EventLogProvider.LogEvent(EventType.INFORMATION, "Registration Error", "CUSTOMREGESTRATION", eventDescription: ex.Message.ToString());
        }

        return userName;
    }

    public void DeleteSAdminUser(int SAdminUserID)
    {
        try
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(string.Format(
                "DELETE FROM Form_MindPathCheckIn_SAdmin_Register WHERE SAdmin_RegisterID = {0}"
                , SAdminUserID)
                , null, QueryTypeEnum.SQLQuery);
        }
        catch (Exception ex)
        {
            // Logs an information event into the event log
            EventLogProvider.LogEvent(EventType.INFORMATION, "Registration Error", "CUSTOMREGESTRATION", eventDescription: ex.Message.ToString());
        }
    }

    #endregion "Methods"
}