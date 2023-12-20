using System.Collections.Generic;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Generic interface for WebAPI controllers, which provide data for angular Smart Drop-down list directive.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// Selector view model. Must be subclass of <see cref="BaseSelectorViewModel"/> class.
    /// Derived models contains specific object type properties that can be displayed in the Smart Drop-down list using item templates.
    /// Controller can handle different object types with same view model.
    /// </typeparam>
    public interface ISelectorController<out TViewModel>
        where TViewModel : BaseSelectorViewModel, new()
    {
        /// <summary>
        /// Get view model according to <paramref name="ID"/> and <paramref name="objType"/>.
        /// </summary>
        /// <param name="objType">Object type used to identify different data types within one controller.</param>
        /// <param name="ID">Object identifier.</param>
        /// <returns>
        /// <c>HTTP status code 400 Bad Request</c>, if model is not found;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <typeparamref name="TViewModel"/> view model.
        /// </returns>
        TViewModel Get(string objType, int ID);


        /// <summary>
        /// Gets list of view models that are containing <paramref name="name"/>.
        /// </summary>
        /// <param name="objType">Object type used to identify different data types within one controller.</param>
        /// <param name="name">Part of models's name.</param>        
        /// <param name="pageIndex">Index of the page. Pages are indexed from 0 (first page).</param>
        /// <param name="pageSize">Number of results in the page.</param>
        /// <returns>
        /// List of view models with <c>id</c> and <c>text</c> properties.
        /// </returns>
        IEnumerable<TViewModel> Get(string objType, string name = "", int pageIndex = 0, int pageSize = 10);
    }
}
