using System;
using System.Web;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;

using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Processes <see cref="VirtualContext"/> URLs for preview and Page builder.
    /// </summary>
    /// <remarks>Executes provided handlers to reflect the initialized virtual context and its properties in features.</remarks>
    internal class PreviewUrlProcessor : IVirtualContextUrlProcessor
    {
        /// <summary>
        /// Actions to be executed after validation of <see cref="VirtualContext"/> data.
        /// </summary>
        private readonly List<Action<HttpContextWrapper>> handlers = new List<Action<HttpContextWrapper>>();
        private static readonly Lazy<PreviewUrlProcessor> instance = new Lazy<PreviewUrlProcessor>(() => new PreviewUrlProcessor());


        /// <summary>
        /// Returns instance of the <see cref="PreviewUrlProcessor"/>.
        /// </summary>
        public static PreviewUrlProcessor Instance => instance.Value;


        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewUrlProcessor"/>. 
        /// </summary>
        internal PreviewUrlProcessor()
        {
        }


        /// <summary>
        /// Registers handler to handle application request to the preview link URL.
        /// </summary>
        /// <param name="handler">Handler to be executed after the preview link context initialization.</param>
        /// <seealso cref="ValidateAndInitializeVirtualContext(string, NameValueCollection)"/>
        /// <seealso cref="PostVirtualContextInitialization(HttpContextBase)"/>
        public void Register(Action<HttpContextWrapper> handler)
        {
            handlers.Add(handler);
        }


        /// <summary>
        /// Validates values obtained from URL in <paramref name="nameValueCollection"/> and in case they are valid saves them into <see cref="VirtualContext"/>.
        /// </summary>
        /// <param name="relativeFilePath">Relative path of the request, stripped off the part containing <see cref="VirtualContext"/> data.</param>
        /// <param name="nameValueCollection">Collection containing values parsed from the URL address.</param>
        /// <returns>Returns true if processor validated and saved values related to preview to <see cref="VirtualContext"/>, otherwise false.</returns>
        public bool ValidateAndInitializeVirtualContext(string relativeFilePath, NameValueCollection nameValueCollection)
        {
            var isPreviewLink = nameValueCollection.Get(VirtualContext.PARAM_PREVIEW_LINK) != null;
            if (!isPreviewLink)
            {
                return false;
            }

            VirtualContext.StoreVirtualContextValues(nameValueCollection);

            if (!ValidatePreviewGuid())
            {
                VirtualContext.Reset();
                throw new HttpException(404, "The preview link is not valid.");
            }

            return true;
        }


        /// <summary>
        /// Method invokes registered handlers via <see cref="Register(Action{HttpContextWrapper})"/>.
        /// </summary>
        /// <param name="context">Context in which the request is being handled.</param>
        /// <seealso cref="Register(Action{HttpContextWrapper})"/>
        public void PostVirtualContextInitialization(HttpContextBase context)
        {
            if (VirtualContext.IsPreviewLinkInitialized)
            {
                SetContentCulture(Thread.CurrentThread);
            }

            handlers.ForEach(h => h(new HttpContextWrapper(context.ApplicationInstance.Context)));
        }


        /// <summary>
        /// Decorates the given <paramref name="absolutePath" /> with additional authentication information.
        /// </summary>
        /// <param name="absolutePath">Absolute URL path to decorate.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        public string DecoratePath(string absolutePath, bool readonlyMode)
        {
            if (VirtualContext.IsPreviewLinkInitialized && !VirtualContext.ContainsVirtualContextPrefix(absolutePath))
            {
                return GetPathDecorator(readonlyMode).Decorate(absolutePath);
            }

            return absolutePath;
        }


        /// <summary>
        /// Gets the path decorator for Preview URL processor.
        /// </summary>
        internal virtual IPathDecorator GetPathDecorator(bool readonlyMode)
        {
            return new PreviewPathDecorator(readonlyMode);
        }


        private bool ValidatePreviewGuid()
        {
            var page = new VirtualContextPageRetriever().Retrieve();

            return page != null;
        }


        /// <summary>
        /// Sets the culture passed via <see cref="VirtualContext"/> to display content of the page inside Pages application in the given culture.
        /// </summary>
        private static void SetContentCulture(Thread thread)
        {
            var cultureName = CultureHelper.GetPreferredCulture();
            var culture = CultureInfo.GetCultureInfo(cultureName);

            thread.CurrentUICulture = culture;
            thread.CurrentCulture = culture;
        }
    }
}