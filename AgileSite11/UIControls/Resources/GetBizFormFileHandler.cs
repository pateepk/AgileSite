using System;
using System.Web;

using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Routing.Web;
using CMS.UIControls;

[assembly: RegisterHttpHandler("CMSPages/GetBizFormFile.aspx", typeof(GetBizFormFileHandler), Order = 1)]

namespace CMS.UIControls
{
    /// <summary>
    /// Handler which returns biz form file.
    /// </summary>
    internal class GetBizFormFileHandler : AdvancedGetFileHandler
    {
        /// <summary>
        /// AdvancedGetFileHandler forces to implement AllowCache property.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                return false;
            }
            set
            {
            }
        }


        /// <summary>
        /// Provides biz form file.
        /// </summary>
        /// <param name="context">Handler context</param>
        protected override void ProcessRequestInternal(HttpContextBase context)
        {
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.form", "ReadData"))
            {
                RequestHelper.Respond403();
            }

            string fileName = QueryHelper.GetString("filename", String.Empty);
            string siteName = QueryHelper.GetString("sitename", CurrentSiteName);

            if ((!ValidationHelper.IsFileName(fileName)) || (siteName == null))
            {
                RequestHelper.Respond404();
            }

            // Check physical path to the file
            string filePath = FormHelper.GetFilePhysicalPath(siteName, fileName);
            if (!File.Exists(filePath))
            {
                RequestHelper.Respond404();
            }

            CookieHelper.ClearResponseCookies();
            Response.Clear();

            string extension = Path.GetExtension(filePath);
            Response.ContentType = MimeTypeHelper.GetMimetype(extension);

            SetDisposition(fileName, extension);

            WriteFile(filePath);

            CompleteRequest();
        }
    }
}