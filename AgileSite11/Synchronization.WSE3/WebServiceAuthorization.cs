using System;
using System.Security;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

using Microsoft.Web.Services3.Security.Tokens;

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Web service authorization interface for username authentication.
    /// </summary>
    public class WebServiceAuthorization : UsernameTokenManager
    {
        /// <summary>
        /// Verifies the incoming username token.
        /// </summary>
        /// <param name="token">Token to verify</param>
        public override void VerifyToken(SecurityToken token)
        {
            if (StagingTaskRunner.ServerAuthenticationType(SiteContext.CurrentSiteName) == ServerAuthenticationEnum.UserName)
            {
                base.VerifyToken(token);
            }
        }


        /// <summary>
        /// Authentication function, returns the password for specified username.
        /// </summary>
        /// <param name="token">Token to authorize against</param>
        /// <exception cref="SecurityException">Thrown when staging has no password set.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when username authentication token is missing.</exception>
        protected override string AuthenticateToken(UsernameToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("[WebServiceAuthorization.AuthenticateToken]: Missing username authentication token.");
            }

            // Authentication processed
            RequestStockHelper.Add(StagingTaskRunner.AUTHENTICATION_PROCESSED, true);

            // Get the required values
            string username = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSStagingServiceUsername");
            string password = EncryptionHelper.DecryptData(SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSStagingServicePassword"));
            if (String.IsNullOrEmpty(password))
            {
                throw new SecurityException("[WebServiceAuthorization.AuthenticateToken]: Staging does not work with blank password. Set a password on the target server.");
            }

            // Process authentication, return the password hash requested
            if (username == token.Username)
            {
                return StagingTaskRunner.GetSHA1Hash(password);
            }
            else
            {
                return "";
            }
        }
    }
}