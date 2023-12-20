using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(ISectionMarkupUrlRetriever), typeof(SectionMarkupUrlRetriever), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides interface for retrieval of a section markup URL.
    /// </summary>
    internal interface ISectionMarkupUrlRetriever
    {
        /// <summary>
        /// Gets URL providing markup of a section.
        /// </summary>
        /// <param name="sectionDefinition">Section to retrieve URL for.</param>
        /// <returns>Returns URL providing the markup.</returns>
        /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
        string GetUrl(SectionDefinition sectionDefinition);
    }
}