using System;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// URL provider for the attachments.
    /// </summary>
    public class AttachmentURLProvider : AbstractBaseProvider<AttachmentURLProvider>
    {
        #region "Methods"

        /// <summary>
        /// Gets the URL to the physical file.
        /// </summary>
        /// <param name="siteName">File site name.</param>
        /// <param name="guid">Attachment GUID.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        public static string GetFilePhysicalUrl(string siteName, string guid, string extension = null, int width = 0, int height = 0)
        {
            return ProviderObject.GetFilePhysicalUrlInternal(siteName, guid, extension, width, height);
        }


        /// <summary>
        /// Returns friendly attachment URL.
        /// </summary>
        /// <param name="safeFileName">Attachment safe file name. Use <see cref="ValidationHelper.GetSafeFileName"/> method to get file name in format suitable for URL.</param>
        /// <param name="aliasPath">Node alias path.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public static string GetAttachmentUrl(string safeFileName, string aliasPath, string extension = null, string variant = null)
        {
            return ProviderObject.GetAttachmentUrlInternal(safeFileName, aliasPath, extension, variant);
        }


        /// <summary>
        /// Returns attachment URL.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID.</param>
        /// <param name="safeFileName">Attachment safe file name. Use <see cref="ValidationHelper.GetSafeFileName"/> method to get file name in format suitable for URL.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="versionHistoryId">ID of the document version for which the attachment version should be displayed.</param>
        /// <param name="prefix">URL prefix, if null, "getattachment" is used, the prefix must be supported by the URL rewriting engine.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public static string GetAttachmentUrl(Guid attachmentGuid, string safeFileName, string extension = null, int versionHistoryId = 0, string prefix = null, string variant = null)
        {
            return ProviderObject.GetAttachmentUrlInternal(attachmentGuid, safeFileName, extension, versionHistoryId, prefix, variant);
        }


        /// <summary>
        /// Returns attachment URL.
        /// </summary>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="safeFileName">Attachment safe file name. Use <see cref="ValidationHelper.GetSafeFileName"/> method to get file name in format suitable for URL.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public static string GetPermanentAttachmentUrl(Guid nodeGuid, string safeFileName, string extension = null, string variant = null)
        {
            return ProviderObject.GetPermanentAttachmentUrlInternal(nodeGuid, safeFileName, extension, variant);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets the URL to the physical file.
        /// </summary>
        /// <param name="siteName">File site name.</param>
        /// <param name="guid">Attachment GUID.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        protected virtual string GetFilePhysicalUrlInternal(string siteName, string guid, string extension, int width, int height)
        {
            // Get files folder relative path
            string filesFolderPath = AttachmentBinaryHelper.GetFilesFolderRelativePath(siteName);
            if (filesFolderPath == null)
            {
                // Not relative, cannot return relative URL
                return null;
            }

            if (extension == null)
            {
                extension = TreePathUtils.GetFilesUrlExtension();
            }

            string fileName = AttachmentHelper.GetFullFileName(guid, extension, width, height);

            // Get the relative URL
            return string.Format("~/{0}/{1}/{2}", filesFolderPath.Trim('/'), AttachmentHelper.GetFileSubfolder(fileName), fileName);
        }


        /// <summary>
        /// Returns attachment permanent URL.
        /// </summary>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="safeFileName">Attachment safe file name. Use <see cref="ValidationHelper.GetSafeFileName"/> method to get file name in format suitable for URL.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="variant">Identifier of the attachment variant.</param>
        protected virtual string GetPermanentAttachmentUrlInternal(Guid nodeGuid, string safeFileName, string extension, string variant)
        {
            if (extension == null)
            {
                extension = TreePathUtils.GetFilesUrlExtension();
            }

            var baseUrl = string.Format("~/getfile/{0}/{1}{2}", nodeGuid, safeFileName, extension);

            if (!string.IsNullOrEmpty(variant))
            {
                return URLHelper.AddParameterToUrl(baseUrl, "variant", variant);
            }

            return baseUrl;
        }


        /// <summary>
        /// Returns friendly attachment URL.
        /// </summary>
        /// <param name="safeFileName">Attachment safe file name. Use <see cref="ValidationHelper.GetSafeFileName"/> method to get file name in format suitable for URL.</param>
        /// <param name="aliasPath">Node alias path.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="variant">Identifier of the attachment variant.</param>
        protected virtual string GetAttachmentUrlInternal(string safeFileName, string aliasPath, string extension, string variant)
        {
            if (aliasPath == null)
            {
                throw new Exception("Node alias path is missing.");
            }

            if (extension == null)
            {
                extension = TreePathUtils.GetFilesUrlExtension();
            }

            // Ensure correct node alias path
            aliasPath = aliasPath.Trim('/');
            if (!string.IsNullOrEmpty(aliasPath))
            {
                aliasPath += "/";
            }

            var baseUrl = string.Format("~/getattachment/{0}{1}{2}", aliasPath, safeFileName, extension);

            if (!string.IsNullOrEmpty(variant))
            {
                return URLHelper.AddParameterToUrl(baseUrl, "variant", variant);
            }

            return baseUrl;
        }


        /// <summary>
        /// Returns attachment URL.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID.</param>
        /// <param name="safeFileName">Attachment safe file name. Use <see cref="ValidationHelper.GetSafeFileName"/> method to get file name in format suitable for URL.</param>
        /// <param name="extension">File extension. If not provided, default friendly extension is used from global settings.</param>
        /// <param name="versionHistoryId">ID of the document version for which the attachment version should be displayed.</param>
        /// <param name="prefix">URL prefix, if null, "getattachment" is used, the prefix must be supported by the URL rewriting engine.</param>
        /// <param name="variant">Identifier of the attachment variant.</param>
        protected virtual string GetAttachmentUrlInternal(Guid attachmentGuid, string safeFileName, string extension, int versionHistoryId, string prefix, string variant)
        {
            if (extension == null)
            {
                extension = TreePathUtils.GetFilesUrlExtension();
            }

            // Ensure the default prefix
            prefix = prefix == null ? "getattachment" : prefix.Trim('/');

            if (String.IsNullOrEmpty(safeFileName))
            {
                safeFileName = "attachment";
            }

            // Build the result URL
            string result = string.Format("~/{0}/{1}/{2}{3}", prefix, attachmentGuid, safeFileName, extension);
            if (versionHistoryId > 0)
            {
                result = URLHelper.AddParameterToUrl(result, "versionhistoryid", versionHistoryId.ToString());
            }

            if (!string.IsNullOrEmpty(variant))
            {
                return URLHelper.AddParameterToUrl(result, "variant", variant);
            }

            return result;
        }

        #endregion
    }
}