using System;
using System.Text;

using CMS.Base.Web.UI;
using CMS.Membership.Web.UI;

namespace CMS.ExternalAuthentication.Web.UI
{
    /// <summary>
    /// Facebook connect script helper
    /// </summary>
    public static class FacebookConnectScriptHelper
    {
        /// <summary>
        /// Returns script for Facebook Connect initialization.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFacebookInitScriptForSite(string siteName)
        {
            // SDK version must be specified in FB.init script. The version number doesn't matter for login purposes (as of now). 
            const string sdkVersion = "v3.2";

            var script = $@"window.fbAsyncInit = function() {{ FB.init({{ appId: '{FacebookConnectHelper.GetFacebookApiKey(siteName)}', status: true, cookie: false, xfbml: true, oauth: true, version: '{sdkVersion}' }}); }};

(function (d) {{
    var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
    js = d.createElement('script'); js.id = id; js.async = true;
    js.src = '//connect.facebook.net/en_US/sdk.js';
    d.getElementsByTagName('head')[0].appendChild(js);
  }} (document));";
            return ScriptHelper.GetScript(script);
        }


        /// <summary>
        /// Returns login handler script.
        /// </summary>
        public static string GetFacebookLoginHandlerScript()
        {
            string script =
@"function Facebook_login() {
FB.getLoginStatus(function (response) {
    if (response.status === 'connected') {
    var hr = window.location.href;
    if(window.location.hash){
      hr = hr.replace(window.location.hash,'');
    }
    if (hr.indexOf('confirmed=') < 0) {
      if (hr.indexOf('?', 0) >= 0) { hr = hr + '&'; }
      else { hr = hr + '?'; }
      hr = hr + 'confirmed=' + response.authResponse.accessToken;
      if (!window.fbRedirectedAfterLogon) {
        window.fbRedirectedAfterLogon = true;
        window.location.href = hr + window.location.hash; 
      }
    }
  }
});
}";
            return ScriptHelper.GetScript(script);
        }


        /// <summary>
        /// Returns Facebook Connect logout script including check whether user is logged
        /// to Facebook (if not additional script is executed).
        /// </summary>
        /// <param name="additionalScriptForLogout">Additional script called when logging out of facebook finishes</param>
        public static string GetFacebookLogoutScriptForSignOut(string additionalScriptForLogout)
        {
            string script =
                String.Format("FB.getLoginStatus(function(response) {{ if (response.status === 'connected') {{ FB.logout(function () {{ {0} }}); }} else {{ {0} }} }});", additionalScriptForLogout);

            return script;
        }


        /// <summary>
        /// Ensures Facebook connect is initialized. Call on module initialization
        /// </summary>
        internal static void InitializeFacebookConnect()
        {
            SignOutScriptHelper.RegisterCustomSignOutScriptProvider(new FacebookSignOutScriptProvider());
        }
    }
}
