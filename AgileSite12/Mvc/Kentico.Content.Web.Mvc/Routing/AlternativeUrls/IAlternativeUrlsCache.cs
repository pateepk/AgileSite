using CMS.DocumentEngine;

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Describes cache for alternative URLs feature.
    /// </summary>
    public interface IAlternativeUrlsCache
    {
        /// <summary>
        /// Returns data of document with given <paramref name="alternativeUrl"/>, if no such document exists returns <c>null</c>.
        /// </summary>
        /// <param name="alternativeUrl">Alternative relative URL.</param>
        MainDocumentData GetDocumentData(NormalizedAlternativeUrl alternativeUrl);
    }
}