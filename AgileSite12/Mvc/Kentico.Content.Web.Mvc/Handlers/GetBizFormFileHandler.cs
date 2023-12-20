using System;
using System.Web;

using CMS.Helpers;
using CMS.OnlineForms.Internal;
using CMS.Routing.Web;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterHttpHandler(FormBuilderRoutes.GET_FILE_ROUTE_TEMPLATE, typeof(GetBizFormFileHandler), Order = 1)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Handler for accessing biz form files in an MVC application.
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
            string originalfilename = QueryHelper.GetString("originalfilename", String.Empty);
            string siteName = QueryHelper.GetString("sitename", CurrentSiteName);

            ProcessRequestBase(context, fileName, originalfilename, siteName);
        }
    }
}
