using CMS.Base;
using CMS.Helpers;

namespace CMS.SalesForce
{
    internal static class SalesForceUrlHelper
    {
        public static string AuthorizePath
        {
            get
            {
                return UseSandbox
                    ? @"https://test.salesforce.com/services/oauth2/authorize"
                    : @"https://login.salesforce.com/services/oauth2/authorize";
            }
        }


        public static string TokenPath
        {
            get
            {
                return UseSandbox
                    ? @"https://test.salesforce.com/services/oauth2/token"
                    : @"https://login.salesforce.com/services/oauth2/token";
            }
        }


        private static bool UseSandbox
        {
            get => ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSalesForceUseSandbox"], false);
        }
    }
}
