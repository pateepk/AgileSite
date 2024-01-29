using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Issuer name registry using settings.
    /// </summary>
    internal class IssuerNameRegistry : Microsoft.IdentityModel.Tokens.IssuerNameRegistry
    {
        #region "Variables"

        private Settings mSettings = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Settings of module.
        /// </summary>
        private Settings Settings
        {
            get
            {
                if (mSettings == null)
                {
                    mSettings = new Settings();
                }
                return mSettings;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns issuer name from certificate.
        /// </summary>
        /// <param name="securityToken">Received security token.</param>
        public override string GetIssuerName(SecurityToken securityToken)
        {
            X509SecurityToken x509Token = securityToken as X509SecurityToken;

            if ((x509Token != null) && x509Token.Certificate.Thumbprint.EqualsCSafe(Settings.TrustedCertificateThumbprint.Replace(" ","").Trim(), true))
            {
                return x509Token.Certificate.SubjectName.Name;
            }

            throw new SecurityTokenException("[IssuerNameRegistry.GetIssuerName]: Untrusted certificate.");
        }

        #endregion
    }
}