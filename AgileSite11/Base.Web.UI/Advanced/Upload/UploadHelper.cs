using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Helper class for uploading files.
    /// </summary>
    public static class UploadHelper
    {
        #region "Constants"

        /// <summary>
        /// Constant represents that no file extension is allowed to be uploaded.
        /// </summary>
        public const string NO_ALLOWED_EXTENSION = "NO_ALLOWED_EXTENSION";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks if specified file extension is allowed to be uploaded.
        /// </summary>
        /// <param name="extension">File extension, e.g. 'jpg' or '.jpg'</param>
        /// <param name="allowedExtensions">Allowed file extensions</param>
        public static bool IsExtensionAllowed(string extension, string allowedExtensions)
        {
            // If allowed extensions are not specified then all extensions are allowed
            if (string.IsNullOrEmpty(allowedExtensions))
            {
                return true;
            }

            if (allowedExtensions.EqualsCSafe(NO_ALLOWED_EXTENSION))
            {
                return false;
            }

            if (extension != null)
            {
                var extensions = allowedExtensions.ToLowerCSafe().Split(';');
                extension = extension.TrimStart('.').ToLowerCSafe();

                return extensions.Contains(extension);
            }

            return false;
        }


        /// <summary>
        /// Restricts allowed file extensions by file extensions defined by the CMSUploadExtensions setting (extensions not defined by this setting cannot be uploaded).
        /// Extensions are without dots and separated by semicolon (e.g. jpg;png;gif).
        /// If no file extension is allowed then "NO_ALLOWED_EXTENSION" is returned.
        /// </summary>
        /// <param name="allowedExtensions">File extensions to be restricted</param>
        /// <param name="siteName">Name of the site</param>
        public static string RestrictExtensions(string allowedExtensions, string siteName)
        {
            string globalExtensions = SettingsKeyInfoProvider.GetValue(siteName + ".CMSUploadExtensions");

            // If allowed extensions are not defined use extensions from settings
            if (string.IsNullOrEmpty(allowedExtensions))
            {
                return globalExtensions;
            }

            // If global extensions are not defined (which means that all extensions are allowed) use custom defined extensions
            if (string.IsNullOrEmpty(globalExtensions))
            {
                return allowedExtensions;
            }

            // Return extensions that are both in custom and global extensions
            string[] extensions = globalExtensions.ToLowerCSafe().Split(';');
            var extensionList = new List<string>();

            foreach (var extension in allowedExtensions.ToLowerCSafe().Split(';'))
            {
                if (extensions.Contains(extension))
                {
                    extensionList.Add(extension);
                }
            }

            if (extensionList.Count > 0)
            {
                return string.Join(";", extensionList.ToArray());
            }

            // None extension is allowed 
            return NO_ALLOWED_EXTENSION;
        }

        #endregion
    }
}
