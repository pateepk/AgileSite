using System;
using System.Web;

using CMS.Helpers;
using CMS.OnlineForms.Internal;
using CMS.Routing.Web;
using CMS.UIControls;

[assembly: RegisterHttpHandler("CMSPages/GetBizFormFile.aspx", typeof(GetBizFormFileHandler), Order = 1)]

namespace CMS.UIControls
{
    /// <summary>
    /// Handler for accessing biz form files.
    /// </summary>
    internal class GetBizFormFileHandler : GetBizFormFileHandlerBase
    {
        /// <summary>
        /// Processes biz form file request.
        /// </summary>
        /// <param name="context">Handler context</param>
        protected override void ProcessRequestInternal(HttpContextBase context)
        {
            string fileName = QueryHelper.GetString("filename", String.Empty);
            string siteName = QueryHelper.GetString("sitename", CurrentSiteName);

            ProcessRequestBase(context, fileName, null, siteName);
        }
    }
}