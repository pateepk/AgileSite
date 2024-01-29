using System;
using System.Linq;
using System.Net;
using System.Text;

using Microsoft.SharePoint.Client;

namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to information about SharePoint 2010 site. 
    /// </summary>
    internal class SharePoint2010SiteService : SharePointAbstractService, ISharePointSiteService
    {
        /// <summary>
        /// Creates a new SharePoint 2010 ISiteService 
        /// </summary>
        /// <param name="connectionData"></param>
        public SharePoint2010SiteService(SharePointConnectionData connectionData) : base(connectionData)
        {

        }


        /// <summary>
        /// Gets SharePoint site URL.
        /// </summary>
        /// <returns>SharePoint site URL.</returns>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public string GetSiteUrl()
        {
            var context = CreateClientContext();
            Site site = context.Site;
            context.Load(site, s => s.Url);
            ExecuteQuery(context);

            return site.Url;
        }
    }
}
