using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Security;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.Internal;
using CMS.IO;

using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

[assembly: RegisterImplementation(typeof(IFileUploaderComponentService), typeof(FileUploaderComponentService), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Services <see cref="FileUploaderComponent"/> and is used to protect the communication with
    /// <see cref="KenticoFormFileUploaderController"/> representing endpoint used for asynchronous upload of files for the <see cref="FileUploaderComponent"/>.
    /// </summary>
    /// <seealso cref="FileUploaderComponent"/>
    /// <seealso cref="KenticoFormFileUploaderController"/>
    internal class FileUploaderComponentService : IFileUploaderComponentService
    {
        /// <summary>
        /// Returns true, if <paramref name="fileName"/> contains allowed extension for given <paramref name="siteId"/>.
        /// </summary>
        /// <param name="fileName">Name of the file that should be validated.</param>
        /// <param name="siteId">Identifies site into which the file is to be uploaded.</param>
        /// <param name="errorMessage">Error message if the validation fails.</param>
        public bool IsExtensionValid(string fileName, int siteId, out string errorMessage)
        {
            var extension = Path.GetExtension(fileName).TrimStart('.');
            var allowedExtensionsSettings = SettingsKeyInfoProvider.GetValue("CMSUploadExtensions", siteId);
            var allowedExtensions = new HashSet<string>(allowedExtensionsSettings.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            bool isAllowed = allowedExtensions.Contains(extension);
            errorMessage = !isAllowed ? ResHelper.GetStringFormat("multifileuploader.extensionnotallowed", $".{extension}", allowedExtensions.Select(ext => $"'.{ext}'").Join(", ")) : String.Empty;

            return isAllowed;
        }


        /// <summary>
        /// Returns true, if the <paramref name="hash"/> was computed for the given <paramref name="formId"/> and <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="formId">Identifies form for which the file is being uploaded.</param>
        /// <param name="fieldName">Identifies a form's field for which the file is being uploaded.</param>
        /// <param name="hash">Hash to be validated.</param>
        public bool IsHashValid(int formId, string fieldName, string hash)
        {
            return hash.Equals(ComputeHash(formId, fieldName), StringComparison.Ordinal);
        }


        /// <summary>
        /// Returns true, if temporary file can be uploaded to <paramref name="tempFilesFolderPath"/> folder for given <paramref name="siteId"/>.
        /// </summary>
        /// <param name="tempFilesFolderPath">Temporary files folder path.</param>
        /// <param name="siteId">Identifies site into which the file is to be uploaded.</param>
        /// <param name="errorMessage">Error message if the validation fails.</param>
        public bool CanUploadTempFile(string tempFilesFolderPath, int siteId, out string errorMessage)
        {
            bool isAllowed = StorageHelper.IsExternalStorage(tempFilesFolderPath) || IsFolderSizeWithinLimits(tempFilesFolderPath);

            errorMessage = !isAllowed ? ResHelper.GetString("formfileuploader.temporaryunavailable.errormessage") : String.Empty;

            return isAllowed;
        }


        /// <summary>
        /// Checks if current folder size for temporary uploaded files storage is within quota.
        /// The return values is cached for performance reasons.
        /// </summary>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <returns>True if size is within quota and new upload should be possible.</returns>
        private bool IsFolderSizeWithinLimits(string tempFilesFolderPath)
        {
            var isWithinLimits = CacheHelper.Cache(() => IsFolderSizeWithinLimitsInternal(tempFilesFolderPath),
                new CacheSettings(1, "FormBuilder", "FolderSizeWithinLimits", tempFilesFolderPath));

            return isWithinLimits;
        }


        /// <summary>
        /// Checks if current folder size for temporary uploaded files storage is within quota.
        /// </summary>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <returns>True if size is within quota and new upload should be possible.</returns>
        private bool IsFolderSizeWithinLimitsInternal(string tempFilesFolderPath)
        {
            var directory = StorageHelper.GetDirectoryInfo(tempFilesFolderPath);

            // Temporary folder does not exist yet, so no files size check needed.
            if (!directory.Exists)
            {
                return true;
            }

            var files = directory.EnumerateFiles();

            var totalSize = 0L;
            foreach (var file in files)
            {
                totalSize += file.Length;
            }

            var totalSizeInMB = (int)(totalSize / 1048576);

            var sizeLimit = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSTemporaryUploadFilesFolderSizeLimit"], 500);

            return totalSizeInMB < sizeLimit;
        }
        
        
        /// <summary>
        /// Creates parameters for <see cref="KenticoFormFileUploaderController.PostFile(int, string, string)"/> endpoint
        /// to which the file will be uploaded for given <paramref name="formId"/> and its <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="formId">Identifies form for which the file will be uploaded.</param>
        /// <param name="fieldName">Identifies a form's field for which the file will be uploaded.</param>
        /// <returns>
        /// <see cref="RouteValueDictionary"/> containing <paramref name="formId"/> and <paramref name="fieldName"/> 
        /// parameters as well as their hash so that the uploaded file will not be associated with different form or field.
        /// </returns>
        public RouteValueDictionary CreateUploadEndpointParameters(int formId, string fieldName)
        {
            return new RouteValueDictionary
            {
                { "formId", formId },
                { "fieldName", fieldName},
                { "hash",  ComputeHash(formId, fieldName)}
            };
        }


        /// <summary>
        /// Protects the value of a <see cref="FileUploaderComponent"/>.
        /// </summary>
        /// <param name="value">Value to protect.</param>
        /// <param name="formId">Identifies form to which the <paramref name="value"/> is associated with.</param>
        /// <param name="fieldName">Identifies a form's field for which the <paramref name="value"/> is associated with.</param>
        /// <param name="purposes">Different purposes used for protecting the <paramref name="value"/>.</param>
        /// <returns>Protected <paramref name="value"/> in hexa decimal string format.</returns>
        public string GetProtectedValue(string value, int formId, string fieldName, params string[] purposes)
        {
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }

            var bytes = Encoding.Unicode.GetBytes(value);
            var protectedBytes = MachineKey.Protect(bytes, CreatePurposes(purposes));

            return CryptoHelper.BytesToHexa(protectedBytes);
        }


        /// <summary>
        /// Unprotects the value of a <see cref="FileUploaderComponent"/>.
        /// </summary>
        /// <param name="protectedHexaValue">Protected value in hexa decimal format.</param>
        /// <param name="formId">Identifies form to which the <paramref name="protectedHexaValue"/> is associated with.</param>
        /// <param name="fieldName">Identifies a form's field for which the <paramref name="protectedHexaValue"/> is associated with.</param>
        /// <param name="purposes">Different purposes used for unprotecting the <paramref name="protectedHexaValue"/>.</param>
        /// <returns>Unprotected value in plain text.</returns>
        public string GetUnprotectedValue(string protectedHexaValue, int formId, string fieldName, params string[] purposes)
        {
            if (String.IsNullOrEmpty(protectedHexaValue))
            {
                return null;
            }

            var protectedBytes = CryptoHelper.HexaToBytes(protectedHexaValue);
            var bytes = MachineKey.Unprotect(protectedBytes, CreatePurposes(purposes));

            return Encoding.Unicode.GetString(bytes);
        }


        private string[] CreatePurposes(string[] purposes)
        {
            return new string[] { nameof(FileUploaderComponent) }.Union(purposes ?? Enumerable.Empty<string>()).ToArray();
        }


        private string ComputeHash(int formId, string fieldName)
        {
            return ValidationHelper.GetHashString($"{formId}{fieldName}", new HashSettings(nameof(FileUploaderComponent)));
        }
    }
}
