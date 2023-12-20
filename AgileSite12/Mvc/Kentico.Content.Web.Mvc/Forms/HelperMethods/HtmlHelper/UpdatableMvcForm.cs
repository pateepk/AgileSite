using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using Kentico.Forms.Web.Mvc.Internal;
using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents updatable MVC form.
    /// </summary>
    public sealed class UpdatableMvcForm : IDisposable
    {
        // Defines a key used in a view data to store an updatable MVC form instance between the beginning and the end of the form
        private const string FORM_CONTEXT_DATA = "kentico_end_form_context_data";

        private readonly MvcForm form;
        private readonly ViewContext viewContext;
        private readonly IViewDataContainer viewDataContainer;
        private readonly RouteCollection routeCollection;
        private readonly string formId;

        private bool disposed;


        /// <summary>
        /// Name of data attribute which marks an input as not being observed for changes.
        /// </summary>
        public const string NOT_OBSERVED_ELEMENT_ATTRIBUTE_NAME = "data-ktc-notobserved-element";


        /// <summary>
        /// Name of data attribute which marks an element that is used to be replaced by HTML received from the server.
        /// </summary>
        public const string UPDATE_TARGET_ID = "data-ktc-ajax-update";


        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="form">Represents an HTML form element in an MVC view.</param>
        /// <param name="viewContext">The name of the controller that will handle the request.</param>
        /// <param name="viewDataContainer">An object that contains the parameters for a route.</param>
        /// <param name="routeCollection">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>Returns an object representing updatable form.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        internal UpdatableMvcForm(string formId, MvcForm form, ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection = null)
        {
            if (String.IsNullOrEmpty(formId))
            {
                throw new ArgumentException(nameof(formId));
            }

            this.formId = formId;
            this.form = form ?? throw new ArgumentNullException(nameof(form));
            this.viewContext = viewContext ?? throw new ArgumentNullException(nameof(viewContext));
            this.viewDataContainer = viewDataContainer ?? throw new ArgumentNullException(nameof(viewDataContainer));
            this.routeCollection = routeCollection;
        }


        /// <summary>
        /// Release resources used by the current instance.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                viewContext.ViewData.Remove(FORM_CONTEXT_DATA);
                
                var htmlHelper = routeCollection == null ? new HtmlHelper(viewContext, viewDataContainer) : new HtmlHelper(viewContext, viewDataContainer, routeCollection);
                viewContext.Writer.Write(htmlHelper.Kentico().RegisterUpdatableFormScript(formId));

                form.Dispose();

                disposed = true;
            }
        }


        /// <summary>
        /// Stores this instance into the given <paramref name="viewDataDictionary"/>.
        /// </summary>
        /// <seealso cref="GetUpdatableMvcFormFromViewData(ViewDataDictionary)"/>
        internal void StoreInViewData(ViewDataDictionary viewDataDictionary)
        {
            if (viewDataDictionary.ContainsKey(FORM_CONTEXT_DATA))
            {
                throw new InvalidOperationException("Forms cannot be nested.");
            }

            viewDataDictionary.Add(FORM_CONTEXT_DATA, this);
        }


        /// <summary>
        /// Returns the last stored <see cref="UpdatableMvcForm"/> from view data. Null, when no form was stored.
        /// </summary>
        /// <seealso cref="StoreInViewData(ViewDataDictionary)"/>
        internal static UpdatableMvcForm GetUpdatableMvcFormFromViewData(ViewDataDictionary viewDataDictionary)
        {
            viewDataDictionary.TryGetValue(FORM_CONTEXT_DATA, out object form);
            return form as UpdatableMvcForm;
        }
    }
}
