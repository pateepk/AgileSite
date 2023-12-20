using System;
using System.Web;
using System.Collections.Specialized;

using CMS.Core;
using CMS.Core.Internal;
using CMS.Helpers;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Processes <see cref="VirtualContext"/> URLs for Form builder.
    /// </summary>
    internal class FormBuilderUrlProcessor : IVirtualContextUrlProcessor
    {
        private static readonly Lazy<FormBuilderUrlProcessor> instance = new Lazy<FormBuilderUrlProcessor>(() => new FormBuilderUrlProcessor());


        /// <summary>
        /// Returns instance of the <see cref="FormBuilderUrlProcessor"/>.
        /// </summary>
        public static FormBuilderUrlProcessor Instance => instance.Value;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormBuilderUrlProcessor"/>. 
        /// </summary>
        internal FormBuilderUrlProcessor()
        {
        }


        /// <summary>
        /// Validates values obtained from URL and in case they are valid save them into <see cref="VirtualContext"/>.
        /// </summary>
        /// <param name="relativeFilePath">Relative path of the request, stripped off the part containing <see cref="VirtualContext"/> data.</param>
        /// <param name="nameValueCollection">Collection containing values parsed from the URL address.</param>
        /// <returns>Returns true if processor validated and saved values related to form builder to <see cref="VirtualContext"/>, otherwise false.</returns>
        /// <exception cref="HttpException">In case that link expiration is exceeded.</exception>
        public bool ValidateAndInitializeVirtualContext(string relativeFilePath, NameValueCollection nameValueCollection)
        {
            var isFormBuilderLink = nameValueCollection.Get(VirtualContext.PARAM_FORM_BUILDER_URL) != null;
            if (!isFormBuilderLink)
            {
                return false;
            }

            if (IsLinkExpired(nameValueCollection))
            {
                throw new HttpException(403, "Form builder link is not valid");
            }

            VirtualContext.StoreVirtualContextValues(nameValueCollection);

            return true;
        }


        /// <summary>
        /// Method does nothing for form builder processor.
        /// </summary>
        /// <param name="context">Context in which the request is being handled.</param>
        public void PostVirtualContextInitialization(HttpContextBase context)
        {
        }


        /// <summary>
        /// Decorates the given <paramref name="absolutePath" /> with additional authentication information.
        /// </summary>
        /// <param name="absolutePath">Absolute URL path to decorate.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests. 
        /// Note: Readonly mode is not supported by the form builder.</param>
        public string DecoratePath(string absolutePath, bool readonlyMode)
        {
            if (VirtualContext.IsFormBuilderLinkInitialized && !VirtualContext.ContainsVirtualContextPrefix(absolutePath))
            {
                return GetPathDecorator().Decorate(absolutePath);
            }

            return absolutePath;
        }


        /// <summary>
        /// Gets the path decorator for the form builder URL processor.
        /// </summary>
        internal virtual IPathDecorator GetPathDecorator()
        {
            return new FormBuilderPathDecorator();
        }


        private static bool IsLinkExpired(NameValueCollection collection)
        {
            var expirationUtcTicks = ValidationHelper.GetLong(collection.Get(VirtualContext.PARAM_FORM_BUILDER_EXPIRATION), 0);

            return Service.Resolve<IDateTimeNowService>().GetDateTimeNow().ToUniversalTime().Ticks >= expirationUtcTicks;
        }
    }
}
