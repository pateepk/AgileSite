using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;

using CMS.Base;

using Microsoft.SharePoint.Client;

using IOExceptions = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides common methods suitable for all SharePoint services.
    /// </summary>
    internal abstract class SharePointAbstractService : ISharePointService
    {
        #region "Private constants"

        /// <summary>
        /// Identity client runtime library name. Needed for SharePoint Online connectivity if SharePointOnlineCredentials are being used.
        /// </summary>
        private const string SHAREPOINT_CCSDK_IDCRL_LIB = "msoidcliL.dll";


        /// <summary>
        /// Error code of <see cref="IdcrlException"/> thrown when the credentials are not valid.
        /// The corresponding exception message is: The sign-in name or password does not match one in the Microsoft account system.
        /// </summary>
        private const int ERROR_CODE_INVALID_CREDENTIALS = -2147186655;

        #endregion


        #region "Fields"

        private SharePointConnectionData mConnectionData;

        #endregion


        #region "Properties"

        /// <summary>
        /// Connection data used by service instance.
        /// </summary>
        protected SharePointConnectionData ConnectionData
        {
            get
            {
                return mConnectionData;
            }
            set
            {
                mConnectionData = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Tells you whether provided ConnectionData can be used (eg. the authentication mode is supported and site URL is in correct format).
        /// </summary>
        /// <returns>True if ConnectionData can be used by this implementation, false otherwise.</returns>
        public virtual bool IsConnectionSupported()
        {
            Uri siteUri;
            if (!Uri.TryCreate(ConnectionData.SharePointConnectionSiteUrl, UriKind.Absolute, out siteUri))
            {
                return false;
            }

            string authMode = ConnectionData.SharePointConnectionAuthMode;
            switch (authMode.ToLowerCSafe())
            {
                case "anonymous":
                case "default":
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Create a new ClientContext for SharePoint 2010 based on ConnectionData.
        /// </summary>
        /// <returns>ClientContext</returns>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <remarks>
        /// If you encounter problems regarding the missing library "msoidcliL.dll", make sure the SharePoint Client Components SDK is installed.
        /// </remarks>
        protected internal virtual ClientContext CreateClientContext()
        {
            ClientContext context;
            try
            {
                context = new ClientContext(ConnectionData.SharePointConnectionSiteUrl);
            }
            catch (ArgumentException)
            {
                throw new SharePointConnectionNotSupportedException(String.Format("[SharePoint2010AbstractService.CreateClientContext]: Invalid site URL '{0}'.", ConnectionData.SharePointConnectionSiteUrl));
            }

            string authMode = ConnectionData.SharePointConnectionAuthMode;

            switch (authMode.ToLowerCSafe())
            {
                case "anonymous":
                    context.AuthenticationMode = ClientAuthenticationMode.Anonymous;
                    break;

                case "default":
                    // default means Windows authentication
                    context.AuthenticationMode = ClientAuthenticationMode.Default;
                    if (ConnectionData.SharePointConnectionSharePointVersion != SharePointVersion.SHAREPOINT_ONLINE)
                    {
                        string domain = ConnectionData.SharePointConnectionDomain;
                        context.Credentials = (String.IsNullOrEmpty(domain)) ? new NetworkCredential(ConnectionData.SharePointConnectionUserName, ConnectionData.SharePointConnectionPassword) :
                            new NetworkCredential(ConnectionData.SharePointConnectionUserName, ConnectionData.SharePointConnectionPassword, ConnectionData.SharePointConnectionDomain);
                    }
                    else
                    {
                        List<char> pwd = (ConnectionData.SharePointConnectionPassword != null) ? ConnectionData.SharePointConnectionPassword.ToList() : new List<char>();
                        SecureString securedPwd = new SecureString();
                        pwd.ForEach(securedPwd.AppendChar);
                        try
                        {
                            context.Credentials = new SharePointOnlineCredentials(ConnectionData.SharePointConnectionUserName, securedPwd);
                        }
                        catch (ArgumentNullException ex)
                        {
                            // The SharePoint online throws different exception on empty user name than SharePoint 2010/2013
                            throw new WebException(ex.Message, ex, WebExceptionStatus.ProtocolError, null);
                        }
                        catch (ArgumentException ex)
                        {
                            // The SharePoint online throws different exception on invalid user name than SharePoint 2010/2013 (thrown when the format is invalid)
                            throw new WebException(ex.Message, ex, WebExceptionStatus.ProtocolError, null);
                        }
                        catch (IOExceptions.FileNotFoundException ex)
                        {
                            if (SHAREPOINT_CCSDK_IDCRL_LIB.EqualsCSafe(ex.Message, true))
                            {
                                // Occurs when using SharePoint Online server, authenticating via SharePointOnlineCredentials and the SDK is not installed.
                                throw new SharePointCCSDKException(
                                    String.Format("The identity client runtime library \"{0}\" seems to be missing. Make sure the SharePoint Client Components SDK is installed properly. See documentation for more details.", SHAREPOINT_CCSDK_IDCRL_LIB), ex);
                            }
                        }
                    }

                    break;

                default:
                    throw new SharePointConnectionNotSupportedException(String.Format("[SharePoint2010AbstractService.CreateClientContext]: Unsupported authentication mode '{0}'. Supported modes are: 'anonymous' and 'default'.", authMode));
            }

            return context;
        }


        /// <summary>
        /// Executes query on SharePoint server. Serves as an adapter for different versions.
        /// </summary>
        /// <param name="clientContext">Client context on which to perform query execution.</param>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected internal void ExecuteQuery(ClientContext clientContext)
        {
            if (ConnectionData.SharePointConnectionSharePointVersion == SharePointVersion.SHAREPOINT_ONLINE)
            {
                // SharePoint online has slightly different exceptions in case of invalid credentials
                try
                {
                    clientContext.ExecuteQuery();
                }
                catch (ArgumentNullException ex)
                {
                    if (ex.ParamName.EqualsCSafe("password", true))
                    {
                        // The SharePoint online throws different exception than version 2010/2013 in case of empty password
                        throw new WebException(ex.Message, ex, WebExceptionStatus.ProtocolError, null);
                    }
                }
                catch (IdcrlException ex)
                {
                    if (ex.ErrorCode == ERROR_CODE_INVALID_CREDENTIALS)
                    {
                        // The SharePoint online throws different exception than version 2010/2013 in case of invalid password
                        // Means: The sign-in name or password does not match one in the Microsoft account system.
                        throw new WebException(ex.Message, ex, WebExceptionStatus.ProtocolError, null);
                    }
                }
                catch (NotSupportedException ex)
                {
                    // The SharePoint online throws different exception than version 2010/2013 in case of invalid site
                    throw new WebException(ex.Message, ex, WebExceptionStatus.ProtocolError, null);
                }
            }
            else
            {
                clientContext.ExecuteQuery();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes SharePoint connection for any service built on this abstract service.
        /// </summary>
        /// <param name="connectionData">Connection data</param>
        protected SharePointAbstractService(SharePointConnectionData connectionData)
        {
            if (connectionData == null)
            {
                throw new ArgumentNullException("connectionData");
            }

            ConnectionData = connectionData;
        }

        #endregion
    }
}
