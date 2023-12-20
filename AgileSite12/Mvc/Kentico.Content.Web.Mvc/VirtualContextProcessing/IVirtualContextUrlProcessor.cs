using System.Web;
using System.Collections.Specialized;

using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Defines methods for processing data from <see cref="VirtualContext"/> URL.
    /// </summary>
    /// <seealso cref="VirtualContextUrlProcessorsRegister"/>
    internal interface IVirtualContextUrlProcessor
    {
        /// <summary>
        /// Validates values obtained from URL and on success saves them into <see cref="VirtualContext"/>.
        /// Method is invoked after parsing URL containing <see cref="VirtualContext"/> and collection containing values parsed from the URL is passed to this method.
        /// </summary>
        /// <param name="relativeFilePath">Relative path of the request, stripped off the part containing <see cref="VirtualContext"/> data.</param>
        /// <param name="nameValueCollection">Collection containing values parsed from the Url address.</param>
        /// <returns>
        /// Returns true if processor validated and on success saved the data from <see cref="VirtualContext"/> URL, otherwise false.
        /// In case processor handled the data from <see cref="VirtualContext"/> URL, other processors registered in <see cref="VirtualContextUrlProcessorsRegister"/>
        /// will not have their <see cref="ValidateAndInitializeVirtualContext(string, NameValueCollection)"/> method invoked. 
        /// </returns>
        bool ValidateAndInitializeVirtualContext(string relativeFilePath, NameValueCollection nameValueCollection);


        /// <summary>
        /// Method is invoked after initializing <see cref="VirtualContext"/> via <see cref="ValidateAndInitializeVirtualContext(string, NameValueCollection)"/>.
        /// </summary>
        /// <remarks>
        /// Method is invoked even if different <see cref="IVirtualContextUrlProcessor"/> handled the <see cref="VirtualContext"/> URL via <see cref="ValidateAndInitializeVirtualContext(string, NameValueCollection)"/>.
        /// Method is not invoked if <see cref="ValidateAndInitializeVirtualContext(string, NameValueCollection)"/> redirected the request or threw an exception.
        /// </remarks>
        /// <param name="context">Context in which the request is being handled.</param>
        void PostVirtualContextInitialization(HttpContextBase context);


        /// <summary>
        /// Decorates the given <paramref name="absolutePath" /> with additional authentication information.
        /// </summary>
        /// <param name="absolutePath">Absolute URL path to decorate.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        string DecoratePath(string absolutePath, bool readonlyMode);
    }
}
