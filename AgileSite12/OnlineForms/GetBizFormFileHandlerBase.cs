using System.Web;

using CMS.Core;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Routing.Web;

namespace CMS.OnlineForms.Internal
{
    /// <summary>
    /// Base handler class for serving biz form files.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public abstract class GetBizFormFileHandlerBase : AdvancedGetFileHandler
    {
        /// <summary>
        /// Indicates whether caching is allowed. Returns false.
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
        /// Processes biz form file request.
        /// </summary>
        /// <param name="context">Handler context</param>
        /// <param name="fileName">File name (as originally provided by the <see cref="FormHelper.GetGuidFileName"/> method).</param>
        /// <param name="originalFileName">File name for download (as originally provided by the <see cref="FormHelper.GetOriginalFileName"/> method).
        /// If invalid or not provided, the content disposition will use <paramref name="fileName"/> value instead.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        protected void ProcessRequestBase(HttpContextBase context, string fileName, string originalFileName, string siteName)
        {
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.BIZFORM, "ReadData"))
            {
                RequestHelper.Respond403();
            }

            if ((!ValidationHelper.IsFileName(fileName)) || (siteName == null))
            {
                RequestHelper.Respond404();
            }

            if (string.IsNullOrEmpty(originalFileName) || ValidationHelper.FileNameForbiddenCharRegExp.IsMatch(originalFileName))
            {
                originalFileName = fileName;
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

            // If file name extension was messed up, stop here
            if (!extension.Equals(Path.GetExtension(originalFileName), System.StringComparison.OrdinalIgnoreCase))
            {
                RequestHelper.Respond403();
            }

            Response.ContentType = MimeTypeHelper.GetMimetype(extension);

            SetDisposition(originalFileName, extension);

            WriteFile(filePath);

            CompleteRequest();
        }
    }
}
