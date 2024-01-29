using System;
using System.Collections.Generic;
using System.Xml;

using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Synchronization;

using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// X509 service assertion.
    /// </summary>
    public class ServiceAssertion : SecurityPolicyAssertion
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ServiceAssertion()
        {
        }


        /// <summary>
        /// No client output filter.
        /// </summary>
        public override SoapFilter CreateClientOutputFilter(FilterCreationContext context)
        {
            return null;
        }


        /// <summary>
        /// No client input filter.
        /// </summary>
        public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
        {
            return null;
        }


        /// <summary>
        /// Service input filter.
        /// </summary>
        public override SoapFilter CreateServiceInputFilter(FilterCreationContext context)
        {
            return new ServiceInputFilter(this, context);
        }


        /// <summary>
        /// No service output filter.
        /// </summary>
        public override SoapFilter CreateServiceOutputFilter(FilterCreationContext context)
        {
            return null;
        }


        /// <summary>
        /// Reading the request.
        /// </summary>
        public override void ReadXml(XmlReader reader, IDictionary<string, Type> extensions)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

            // Determine the name of the extension
            string tagName = null;
            foreach (string extName in extensions.Keys)
            {
                if (extensions[extName] == typeof(ServiceAssertion))
                {
                    tagName = extName;
                    break;
                }
            }

            // Read the first element (maybe empty)
            reader.ReadStartElement(tagName);
        }


        /// <summary>
        /// Writes the XML.
        /// </summary>
        public override void WriteXml(XmlWriter writer)
        {
        }
    }


    /// <summary>
    /// X509 Service input filter.
    /// </summary>
    public class ServiceInputFilter : ReceiveSecurityFilter
    {
        private ServiceAssertion mParentAssertion = null;
        private FilterCreationContext mFilterContext = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentAssertion">Parent assertion</param>
        /// <param name="filterContext">Filter context</param>
        public ServiceInputFilter(ServiceAssertion parentAssertion, FilterCreationContext filterContext)
            : base(parentAssertion.ServiceActor, false, parentAssertion.ClientActor)
        {
            mParentAssertion = parentAssertion;
            mFilterContext = filterContext;
        }


        /// <summary>
        /// Validates the message security.
        /// </summary>
        /// <param name="envelope">Message envelope</param>
        /// <param name="security">Security context</param>
        public override void ValidateMessageSecurity(SoapEnvelope envelope, Security security)
        {
            switch (StagingTaskRunner.ServerAuthenticationType(SiteContext.CurrentSiteName))
            {
                // X509 authentication
                case ServerAuthenticationEnum.X509:
                    // Validate security token
                    SecurityToken token = X509Helper.GetSigningX509Token(envelope.Context, security);
                    if (token == null)
                    {
                        throw new Exception(ResHelper.GetAPIString("SyncServer.ErrorMissingX509Token", "Missing X509 certificate token, please check authentication type"));
                    }
                    if (!X509Helper.CheckServerX509Token((X509SecurityToken)token, SiteContext.CurrentSiteName))
                    {
                        throw new Exception(ResHelper.GetAPIString("SyncServer.ErrorX509AuthenticationFailed", "X509 authentication failed."));
                    }
                    break;

                // Username authentication
                case ServerAuthenticationEnum.UserName:
#pragma warning disable CS0618 // Type or member is obsolete
                    if (envelope?.Context?.Security?.Tokens?.Count == 0)
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        throw new Exception(ResHelper.GetAPIString("syncserver.errormissingusernametoken", "UserName authentication failed."));
                    }
                    break;

                // Uknown authentication
                default:
                    throw new Exception(ResHelper.GetAPIString("SyncServer.ErrorInvalidAuthentication", "Unknown authentication type or wrong authentication settings"));
            }
        }
    }


    /// <summary>
    /// Service policy
    /// </summary>
    public class ServicePolicy : Policy
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ServicePolicy()
            : base()
        {
            // Add the policy assertion to the policy.
            this.Assertions.Add(new ServiceAssertion());
        }
    }
}