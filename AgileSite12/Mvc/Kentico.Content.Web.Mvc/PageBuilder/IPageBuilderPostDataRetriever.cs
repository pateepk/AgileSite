using System;

using Kentico.Builder.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for retrieving data from Page builder POST request.
    /// </summary>
    internal interface IPageBuilderPostDataRetriever<out TPostData>
        where TPostData : class
    {
        /// <summary>
        /// Retrieves data from the page builder POST request.
        /// </summary>
        /// <returns>Returns deserialized POST request data into the type <typeparamref name="TPostData"/>.</returns>
        /// <exception cref="InvalidOperationException">Is thrown when incorrect data format is retrieved and cannot be deserialized into the type <typeparamref name="TPostData"/>.</exception>
        TPostData Retrieve();
    }
}