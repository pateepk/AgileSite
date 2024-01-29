using System;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides SalesForce organization sessions using the Salesforce.com Connector settings.
    /// </summary>
    public sealed class ConfigurationSessionProvider : ISessionProvider
    {

        #region "Public members"

        /// <summary>
        /// Gets ot sets the name of the site to look the organization credentials up.
        /// </summary>
        public string SiteName { get; set; }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Creates a new SalesForce organization session, and returns it.
        /// </summary>
        /// <returns>A new SalesForce organization session.</returns>
        public Session CreateSession()
        {
            string content = SettingsKeyInfoProvider.GetValue("CMSSalesForceCredentials");
            if (String.IsNullOrEmpty(content))
            {
                throw new Exception("[ConfigurationSessionProvider.CreateSession]: SalesForce organization access is not enabled.");
            }
            content = EncryptionHelper.DecryptData(content).TrimEnd('\0');
            OrganizationCredentials credentials = OrganizationCredentials.Deserialize(content);
            RefreshTokenSessionProvider provider = new RefreshTokenSessionProvider
            {
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret,
                RefreshToken = credentials.RefreshToken
            };

            return provider.CreateSession();
        }

        #endregion

    }

}