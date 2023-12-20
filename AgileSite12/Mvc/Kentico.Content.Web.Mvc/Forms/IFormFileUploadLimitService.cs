using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormFileUploadLimitService), typeof(FormFileUploadLimitService), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains properties for retrieving limits for <see cref="FileUploaderComponent"/>.
    /// </summary>
    /// <remarks>
    /// The limits are determined from application's root web.config for configuration sections
    /// applying to the 'Kentico.FormBuilder/FormFileUploader' location or its parents.
    /// </remarks>
    public interface IFormFileUploadLimitService
    {
        /// <summary>
        /// Gets the 'maxRequestLength' attribute value in kilobytes from the application's root web.config file.
        /// Returns <see cref="FormFileUploadLimitService.MAX_REQUEST_LENGTH_DEFAULT_VALUE"/> if the value is undefined.
        /// </summary>
        long MaxRequestLength { get; }


        /// <summary>
        /// Gets the 'maxAllowedContentLength' attribute value in bytes from the application's root web.config file.
        /// Returns <see cref="FormFileUploadLimitService.MAX_ALLOWED_CONTENT_LENGHT_DEFAULT_VALUE"/> if the value is undefined.
        /// </summary>
        long MaxAllowedContentLength { get; }


        /// <summary>
        /// Gets the maximal allowed upload file size in kilobytes. 
        /// The limit is the smaller value between <see cref="MaxRequestLength"/> and <see cref="MaxAllowedContentLength"/> converted to kilobytes.
        /// </summary>
        long MaxAllowedFileSize { get; }
    }
}
