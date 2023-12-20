using System.Web;
using System.Web.Routing;

using Kentico.Forms.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines a service for <see cref="FileUploaderComponent"/> and for protecting the communication with
    /// <see cref="KenticoFormFileUploaderController"/> representing endpoint used for asynchronous upload of files for the <see cref="FileUploaderComponent"/>.
    /// </summary>
    /// <seealso cref="FileUploaderComponent"/>
    /// <seealso cref="KenticoFormFileUploaderController"/>
    internal interface IFileUploaderComponentService
    {
        /// <summary>
        /// Returns true, if <paramref name="fileName"/> contains allowed extension for given <paramref name="siteId"/>.
        /// </summary>
        /// <param name="fileName">Name of the file that should be validated.</param>
        /// <param name="siteId">Identifies site into which the file is to be uploaded.</param>
        /// <param name="errorMessage">Error message if the validation fails.</param>
        bool IsExtensionValid(string fileName, int siteId, out string errorMessage);


        /// <summary>
        /// Returns true, if the <paramref name="hash"/> was computed for the given <paramref name="formId"/> and <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="formId">Identifies form for which the file is being uploaded.</param>
        /// <param name="fieldName">Identifies a form's field for which the file is being uploaded.</param>
        /// <param name="hash">Hash to be validated.</param>
        bool IsHashValid(int formId, string fieldName, string hash);


        /// <summary>
        /// Returns true, if temporary file can be uploaded to <paramref name="tempFilesFolderPath"/> folder for given <paramref name="siteId"/>.
        /// </summary>
        /// <param name="tempFilesFolderPath">Temporary files folder path.</param>
        /// <param name="siteId">Identifies site into which the file is to be uploaded.</param>
        /// <param name="errorMessage">Error message if the validation fails.</param>
        bool CanUploadTempFile(string tempFilesFolderPath, int siteId, out string errorMessage);

        
        /// <summary>
        /// Creates parameters for <see cref="KenticoFormFileUploaderController.PostFile(int, string, string)"/> endpoint.
        /// </summary>
        /// <param name="formId">Identifies form for which the file will be uploaded.</param>
        /// <param name="fieldName">Identifies a form's field for which the file will be uploaded.</param>
        /// <returns>
        /// <see cref="RouteValueDictionary"/> containing <paramref name="formId"/> and <paramref name="fieldName"/> 
        /// parameters as well as their hash so that the uploaded file will not be associated with different form or field.
        /// </returns>
        RouteValueDictionary CreateUploadEndpointParameters(int formId, string fieldName);


        /// <summary>
        /// Protects the value of a <see cref="FileUploaderComponent"/>.
        /// </summary>
        /// <param name="value">Value to protect.</param>
        /// <param name="formId">Identifies form to which the <paramref name="value"/> is associated with.</param>
        /// <param name="fieldName">Identifies a form's field for which the <paramref name="value"/> is associated with.</param>
        /// <param name="purposes">Different purposes used for protecting the <paramref name="value"/>.</param>
        /// <returns>Protected <paramref name="value"/> in hexa decimal string format.</returns>
        /// <seealso cref="GetUnprotectedValue(string, int, string, string[])"/>
        string GetProtectedValue(string value, int formId, string fieldName, params string[] purposes);
        

        /// <summary>
        /// Unprotects the value of a <see cref="FileUploaderComponent"/>.
        /// </summary>
        /// <param name="protectedHexaValue">Protected value in hexa decimal format.</param>
        /// <param name="formId">Identifies form to which the <paramref name="protectedHexaValue"/> is associated with.</param>
        /// <param name="fieldName">Identifies a form's field for which the <paramref name="protectedHexaValue"/> is associated with.</param>
        /// <param name="purposes">Different purposes used for unprotecting the <paramref name="protectedHexaValue"/>.</param>
        /// <returns>Unprotected value in plain text.</returns>
        string GetUnprotectedValue(string protectedHexaValue, int formId, string fieldName, params string[] purposes);
    }
}