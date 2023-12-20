using System;

using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// X509 Security assertion.
    /// </summary>
    public class X509ClientAssertion : SecurityPolicyAssertion
    {
        private string mClientKeyId = null;
        private string mServerKeyId = null;


        /// <summary>
        /// Client certificate key ID.
        /// </summary>
        public string ClientKeyId
        {
            get
            {
                return mClientKeyId;
            }
        }


        /// <summary>
        /// Server certificate key ID.
        /// </summary>
        public string ServerKeyId
        {
            get
            {
                return mServerKeyId;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clientKeyId">Client certificate key ID</param>
        /// <param name="serverKeyId">Server certificate key ID</param>
        public X509ClientAssertion(string clientKeyId, string serverKeyId)
        {
            mClientKeyId = clientKeyId;
            mServerKeyId = serverKeyId;
        }


        /// <summary>
        /// Client output filter.
        /// </summary>
        public override SoapFilter CreateClientOutputFilter(FilterCreationContext context)
        {
            return new X509ClientOutputFilter(this, context);
        }


        /// <summary>
        /// No client input filter.
        /// </summary>
        public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
        {
            return null;
        }


        /// <summary>
        /// No service input filter.
        /// </summary>
        public override SoapFilter CreateServiceInputFilter(FilterCreationContext context)
        {
            return null;
        }


        /// <summary>
        /// No service output filter.
        /// </summary>
        public override SoapFilter CreateServiceOutputFilter(FilterCreationContext context)
        {
            return null;
        }
    }


    /// <summary>
    /// Client output filter.
    /// </summary>
    internal class X509ClientOutputFilter : SendSecurityFilter
    {
        private X509ClientAssertion mParentAssertion = null;
        private FilterCreationContext mFilterContext = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentAssertion">Parent assertion</param>
        /// <param name="filterContext">Filter context</param>
        public X509ClientOutputFilter(X509ClientAssertion parentAssertion, FilterCreationContext filterContext)
            : base(parentAssertion.ServiceActor, false, parentAssertion.ClientActor)
        {
            mParentAssertion = parentAssertion;
            mFilterContext = filterContext;
        }


        /// <summary>
        /// Secures the message context.
        /// </summary>
        /// <param name="envelope">Message envelope</param>
        /// <param name="security">Security context</param>
        public override void SecureMessage(SoapEnvelope envelope, Security security)
        {
            X509SecurityToken serverToken = X509Helper.GetX509Token(mParentAssertion.ServerKeyId);
            if (serverToken == null)
            {
                throw new Exception("[X509ClientOutputFilter.SecureMessage]: Unable to obtain server security token ID '" + mParentAssertion.ServerKeyId + "'.");
            }

            // Sign the message
            security.Tokens.Add(serverToken);
            security.Elements.Add(new MessageSignature(serverToken));
        }
    }
}