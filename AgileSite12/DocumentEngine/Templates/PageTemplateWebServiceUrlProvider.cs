using System;

using CMS.Base;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Encapsulates methods related to providing web service URLs of MVC page templates.  
    /// </summary>
    public sealed class PageTemplateWebServiceUrlProvider
    {
        internal const string GET_TEMPLATES_ENDPOINT_URL = "~/Kentico.PageBuilder/PageTemplates/GetFiltered";
        private readonly IUserInfo mUser;


        /// <summary>
        /// Creates a new instance of <see cref="PageTemplateWebServiceUrlProvider"/>.
        /// </summary>
        /// <param name="user">The user used to authenticate the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> is <c>null</c>.</exception>
        public PageTemplateWebServiceUrlProvider(IUserInfo user)
        {
            mUser = user ?? throw new ArgumentNullException(nameof(user));
        }


        /// <summary>
        /// Gets URL of an endpoint which provides definition of allowed page templates for the given parent node.
        /// </summary>
        /// <param name="parentNode">The parent node for which the endpoint retrieves filtered page templates.</param>
        /// <param name="pageType">The page type for which the endpoint retrieves filtered page templates.</param>
        /// <param name="culture">The culture for which the endpoint retrieves filtered page templates.</param>
        /// <returns>A string representation of the endpoint URL</returns>
        /// <exception cref="InvalidOperationException">Site's <see cref="SiteInfo.SitePresentationURL"/> is not set.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parentNode"/>, <paramref name="pageType"/> or <paramref name="culture"/> is <c>null</c>.</exception>
        public string GetTemplatesEndpointUrl(TreeNode parentNode, string pageType, string culture)
        {
            if (parentNode == null)
            {
                throw new ArgumentNullException(nameof(parentNode));
            }
            if (pageType == null)
            {
                throw new ArgumentNullException(nameof(pageType));
            }
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var presentationUrl = parentNode.Site?.SitePresentationURL;

            if (String.IsNullOrEmpty(presentationUrl))
            {
                throw new InvalidOperationException("A given site doesn't specify a PresentationURL.");
            }

            string url = VirtualContext.AddPathHash(GET_TEMPLATES_ENDPOINT_URL);
            url = DecorateWithVirtualContext(url, parentNode, mUser);
            url = URLHelper.CombinePath(url, '/', presentationUrl, null);
            url = URLHelper.AddParameterToUrl(url, "pagetype", pageType);
            url = URLHelper.AddParameterToUrl(url, "culture", culture);

            return url;
        }


        private static string DecorateWithVirtualContext(string url, TreeNode page, IUserInfo user)
        {
            var param = VirtualContext.GetPreviewParameters(user.UserName, page.DocumentCulture, page.DocumentWorkflowCycleGUID, readonlyMode: false, embededInAdministration: false);
            return VirtualContext.GetVirtualContextPath(url, param);
        }
    }
}