using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents a feature that provides information about Page builder for the current request.
    /// </summary>
    public interface IPageBuilderFeature
    {
        /// <summary>
        /// Gets page builder options.
        /// </summary>
        PageBuilderOptions Options { get; }


        /// <summary>
        /// Gets a value indicating whether edit mode is enabled.
        /// </summary>
        bool EditMode { get; }


        /// <summary>
        /// Gets the identifier of the page where the Page builder stores and loads data from.
        /// </summary>
        int PageIdentifier { get; }


        /// <summary>
        /// Initializes the Page builder.
        /// </summary>
        /// <param name="pageIdentifier">The identifier of the page where the Page builder stores and loads data from.</param>
        void Initialize(int pageIdentifier);
    }
}
