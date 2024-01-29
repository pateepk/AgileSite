using System;
using System.Collections.Generic;
using System.Text;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ExternalAuthentication;
using CMS.Helpers;
using CMS.UIControls;


public partial class CMSModules_Membership_Pages_LinkedIn_LinkedInAccessTokenPage : CMSModalPage
{
    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        PageTitle.TitleText = ResHelper.GetString("socialnetworking.linkedin.accesstoken");
        PageTitle.ShowFullScreenButton = false;
        PageTitle.ShowCloseButton = false;

        string txtToken = QueryHelper.GetString("txtToken", String.Empty);
        string consumerKey = QueryHelper.GetString("apiKey", String.Empty);
        string consumerSecret = QueryHelper.GetString("apiSecret", String.Empty);
        string error = QueryHelper.GetString("error", String.Empty);
        string code = QueryHelper.GetString("code", String.Empty);

        // Check Social networking DLL and settings
        if (!SystemContext.IsFullTrustLevel)
        {
            lblStatus.Text = ResHelper.GetString("socialnetworking.fulltrustrequired");
        }
        else if ((String.IsNullOrEmpty(consumerKey) || String.IsNullOrEmpty(consumerSecret)) && String.IsNullOrEmpty(code) && String.IsNullOrEmpty(error))
        {
            lblStatus.Text = ResHelper.GetString("socialnetworking.linkedin.apisettingsmissing");
        }
        else
        {
            // If access denied
            if (error.EqualsCSafe("access_denied"))
            {
                // Close the window
                StringBuilder script = new StringBuilder("if(wopener.setAccessTokenToTextBox){ CloseDialog(); }");

                ScriptHelper.RegisterStartupScript(Page, typeof(string), "TokenScript", ScriptHelper.GetScript(script.ToString()));
            }
            else
            {
                try
                {
                    // Authenticate and retrieve tokens
                    Dictionary<string, string> tokens = LinkedInProvider.Authorize(txtToken);
                    if (tokens.Count != 0)
                    {
                        // Return access token values and close the window
                        StringBuilder script = new StringBuilder("if(wopener.setAccessTokenToTextBox){ wopener.setAccessTokenToTextBox('")
                            .AppendFormat("{0}', '{1}'); CloseDialog(); }}", txtToken, tokens["AccessToken"]);
                        ScriptHelper.RegisterStartupScript(Page, typeof (string), "TokenScript", ScriptHelper.GetScript(script.ToString()));
                    }
                    else
                    {
                        // Error occurred while communicating with LinkedIn
                        lblStatus.Text = ResHelper.GetString("socialnetworking.authorizationerror");
                    }
                }
                catch (Exception ex)
                {
                    LogAndShowError("SocialMedia", "LinkedInProvider", ex);
                }
            }
        }
    }

    #endregion
}
