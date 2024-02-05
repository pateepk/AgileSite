using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.IO;

using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Protocols.WSTrust;

using AudienceRestriction = Microsoft.IdentityModel.Tokens.AudienceRestriction;
using SecurityTokenHandlerCollection = Microsoft.IdentityModel.Tokens.SecurityTokenHandlerCollection;
using SecurityTokenHandlerConfiguration = Microsoft.IdentityModel.Tokens.SecurityTokenHandlerConfiguration;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Class that handles the operations for WIF sign-in.
    /// </summary>
    internal class SignIn : AbstractWIFAuthentication
    {
        #region "Public methods"

        /// <summary>
        /// Handles WIF sign-in response from identity provider.
        /// </summary>
        public void ProcessSignInRequest()
        {
            // Check if module enabled
            if (!Settings.Enabled || (HttpContext.Current.Request.Form[WSFederationConstants.Parameters.Result] == null))
            {
                return;
            }

            // This is a response from the STS
            var message = WSFederationMessage.CreateFromNameValueCollection(
                WSFederationMessage.GetBaseUrl(RequestContext.URL),
                HttpContext.Current.Request.Form) as SignInResponseMessage;

            if (message == null)
            {
                LogErrorAndRedirect("WIF authentication failed when trying to get sign-in message.");
            }

            // Now read the token and convert it to claims principal           
            string tokenXml = GetTokenXml(message);
            SecurityTokenHandlerCollection tokenHandlers = CreateTokenHandlerCollection();
            ClaimsIdentityCollection claimsIdentity = ReadClaimsIdentityCollection(tokenXml, tokenHandlers);
            var principal = new ClaimsPrincipal(claimsIdentity);

            if (principal.Identity.IsAuthenticated)
            {
                AuthenticateUser(principal);
            }
            else
            {
                LogErrorAndRedirect("WIF authentication failed. User was not authenticated.");
            }
        }


        /// <summary>
        /// Handles WIF sign-in request to identity provider.
        /// </summary>
        /// <param name="returnUrl">Return URL.</param>
        public void RequestSignIn(string returnUrl)
        {
            // Check if module enabled
            if (!Settings.Enabled)
            {
                return;
            }

            // Check if user is authenticated
            if (AuthenticationHelper.IsAuthenticated())
            {
                return;
            }

            // Check settings
            if (Settings.IdentityProviderURL == null)
            {
                LogErrorAndRedirect("Identity provider URL setting not set.");
            }

            // Set HTTP Referrer header
            SetReferrer();

            // Redirect to identity provider
            var signinUrl = GetSignInUrl(Settings.IdentityProviderURL, Settings.Realm, returnUrl);
            URLHelper.ClientRedirect(signinUrl);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Authenticates user from claim.
        /// </summary>
        /// <param name="principal">Claims principal.</param>
        private void AuthenticateUser(ClaimsPrincipal principal)
        {
            // Get username and email claims
            string usernameClaim = principal.Identity.Name;
            string emailClaim = ExtractEmailClaim(principal);

            // If username is not provided try to use email
            if (String.IsNullOrEmpty(usernameClaim))
            {
                if (!String.IsNullOrEmpty(emailClaim))
                {
                    usernameClaim = emailClaim;
                    EventLogProvider.LogEvent(EventType.WARNING, "WIF Integration", "WIF_AUTH_WARNING", "Identity provider didn't provide username claim. Please configure your provider to do so.");
                }
                else
                {
                    // There is no username or email, so the user can't be identified
                    LogErrorAndRedirect("Identity provider didn't provide username or email claim. Please configure your provider to do so.");
                }
            }

            // Try to find this user
            var user = UserInfoProvider.GetUserInfo(usernameClaim);
            if (user == null)
            {
                CreateExternalUser(usernameClaim, emailClaim);
            }
            else
            {
                CheckUserNameConflict(user, usernameClaim);
            }

            // Authenticate user
            AuthenticationHelper.AuthenticateUser(usernameClaim, true);
            URLHelper.Redirect(RequestContext.CurrentURL);
        }


        /// <summary>
        /// In case of user name conflict logs error and redirects to error page.
        /// </summary>
        /// <param name="user">User to be checked.</param>
        /// <param name="usernameClaim">User name.</param>
        private void CheckUserNameConflict(UserInfo user, string usernameClaim)
        {
            // Check for username conflict
            if ((user.UserName == usernameClaim) && !user.IsExternal)
            {
                var msg = String.Format("WIF authentication failed. User with username {0} already exists.", usernameClaim);
                LogErrorAndRedirect(msg);
            }
        }


        /// <summary>
        /// Creates external user with given name and email.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="email">Email.</param>
        private void CreateExternalUser(string userName, string email)
        {
            // Create new user
            var ui = new UserInfo
            {
                UserName = userName,
                FullName = userName,
                Email = email,
                UserIsExternal = true,
                Enabled = true
            };
            AuthenticationHelper.EnsureExternalUser(ui);
        }


        /// <summary>
        /// Extracts email claim from the claims principal.
        /// </summary>
        /// <param name="principal">Claims principal.</param>
        private static string ExtractEmailClaim(ClaimsPrincipal principal)
        {
            var webpageClaims = ((ClaimsIdentity)principal.Identity).Claims.FindAll(x => x.ClaimType == ClaimTypes.Email);
            if (webpageClaims.Count >= 1)
            {
                return webpageClaims.ElementAt(0).Value;
            }
            return null;
        }


        /// <summary>
        /// Parses the sign-in message and extracts assertion XML string
        /// </summary>
        /// <param name="message">Sing-in message.</param>
        private string GetTokenXml(SignInResponseMessage message)
        {
            var serializer = GetTokenSerializer(message);

            if (!serializer.CanReadResponse(message.Result))
            {
                LogErrorAndRedirect("WIF authentication failed when trying to read sign-in message.");
            }

            RequestSecurityTokenResponse tokenResponse = serializer.CreateResponse(message, new WSTrustSerializationContext());
            return tokenResponse.RequestedSecurityToken.SecurityTokenXml.OwnerDocument.InnerXml;
        }


        /// <summary>
        /// Gets authentication token serializer to read the token.
        /// </summary>
        /// <param name="message">Authentication XML message representing the token.</param>
        private WSFederationSerializer GetTokenSerializer(SignInResponseMessage message)
        {
            // Process token namespace
            switch (GetTokenNamespace(message))
            {
                // Use WSTrust serializer for WS-Trust authentication message
                case WSTrust13Constants.NamespaceURI:
                    return new WSFederationSerializer(new WSTrust13RequestSerializer(), new WSTrust13ResponseSerializer());

                // Use WSFederation serializer for other messages
                default:
                    return new WSFederationSerializer();
            }
        }


        /// <summary>
        /// Gets authentication token namespace.
        /// </summary>
        /// <param name="message">Message containing claims information.</param>
        /// <returns>Namespace of the authentication token.</returns>
        private string GetTokenNamespace(SignInResponseMessage message)
        {
            // Load token XML
            XDocument doc = XDocument.Parse(message.Result);
            XPathNavigator navigator = doc.CreateNavigator();
            navigator.MoveToFollowing(XPathNodeType.Element);

            // Get token XML namespace
            return GetXMLFirstCustomNamespacePath(navigator.GetNamespacesInScope(XmlNamespaceScope.All));
        }


        /// <summary>
        /// Gets first custom namespace from set of XML namespaces.
        /// </summary>
        /// <param name="namespaces">XML namespaces to investigate.</param>
        private string GetXMLFirstCustomNamespacePath(IDictionary<string, string> namespaces)
        {
            // Process XML namespace name/URI pairs
            foreach (var ns in namespaces.Keys)
            {
                switch (ns.ToLowerCSafe())
                {
                    // Ignore default XML namespace
                    case "xml":
                        break;

                    // Return custom namespace URI path
                    default:
                        return namespaces[ns];
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns ClaimsIdentityCollection.
        /// </summary>
        /// <param name="tokenXml">Token XML.</param>
        /// <param name="hndlCol">Token handler collection.</param>
        private ClaimsIdentityCollection ReadClaimsIdentityCollection(string tokenXml, SecurityTokenHandlerCollection hndlCol)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(tokenXml)))
            {
                if (!hndlCol.CanReadToken(reader))
                {
                    LogErrorAndRedirect("WIF authentication failed when trying to read security token.");
                }

                try
                {
                    SecurityToken token = hndlCol.ReadToken(reader);
                    return hndlCol.ValidateToken(token);
                }
                catch (Exception e)
                {
                    LogErrorAndRedirect(e.Message);
                    return null;
                }
            }
        }


        /// <summary>
        /// Creates the collection of security handlers based on module settings.
        /// </summary>
        private SecurityTokenHandlerCollection CreateTokenHandlerCollection()
        {
            var config = new SecurityTokenHandlerConfiguration();
            ConfigureAudienceRestrictions(config.AudienceRestriction);
            config.CertificateValidator = GetSelectedCertificateValidator();
            config.IssuerNameRegistry = new IssuerNameRegistry();

            return SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection(config);
        }


        /// <summary>
        /// Method configuring audience restrictions from settings.
        /// </summary>
        /// <param name="restrictions">Audience restriction settings object.</param>
        private void ConfigureAudienceRestrictions(AudienceRestriction restrictions)
        {
            foreach (var uri in Settings.AllowedAudienceURIs)
            {
                restrictions.AllowedAudienceUris.Add(uri);
            }
        }


        /// <summary>
        /// Returns selected certificate validator.
        /// </summary>
        private X509CertificateValidator GetSelectedCertificateValidator()
        {
            switch (Settings.CertificateValidator)
            {
                case CertificateValidatorEnum.ChainTrust:
                    return X509CertificateValidator.ChainTrust;

                case CertificateValidatorEnum.PeerTrust:
                    return X509CertificateValidator.PeerTrust;

                case CertificateValidatorEnum.PeerOrChainTrust:
                    return X509CertificateValidator.PeerOrChainTrust;

                case CertificateValidatorEnum.None:
                    return X509CertificateValidator.None;
            }

            return X509CertificateValidator.ChainTrust;
        }

        #endregion
    }
}
