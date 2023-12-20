using System;
using System.Collections.Specialized;

namespace CMS.Newsletters
{
    /// <summary>
    /// Describes configuration for double opt-in extension.
    /// </summary>
    public sealed class DoubleOptInExtensionDefinition
    {
        /// <summary>
        /// This function is invoked when double opt-in URL is generated.
        /// </summary>
        /// <remarks>Return query parameters which are used to extend double opt-in URL.</remarks>
        public Func<NameValueCollection> GetQueryParameters
        {
            get;
        }


        /// <summary>
        /// This action is invoked when the subscription is approved via double opt-in URL.
        /// </summary>
        /// <remarks>Gets collection of HTTP query-string values as parameter.</remarks>
        public Action<NameValueCollection> ApprovalAction
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="DoubleOptInExtensionDefinition"/>.
        /// </summary>
        /// <param name="getQueryParameters">Function which is invoked when double opt-in URL is generated</param>
        /// <param name="approvalAction">Action which is invoked when the subscription is approved via double opt-in URL</param>
        public DoubleOptInExtensionDefinition(Func<NameValueCollection> getQueryParameters, Action<NameValueCollection> approvalAction)
        {
            GetQueryParameters = getQueryParameters;
            ApprovalAction = approvalAction;
        }
    }
}
