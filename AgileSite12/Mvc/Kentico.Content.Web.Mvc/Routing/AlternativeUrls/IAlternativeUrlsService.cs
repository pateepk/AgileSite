namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Describes service for using alternative URLs.
    /// </summary>
    public interface IAlternativeUrlsService
    {
        /// <summary>
        /// Returns data of main document for given <paramref name="alternativeUrl"/>. <paramref name="alternativeUrl"/> is normalized and checked whether matches any excluded URLs and should not be processed.
        /// </summary>
        /// <param name="alternativeUrl">Alternative URL for which main document data is retrieved.</param>
        /// <returns>Data of main document if such is found, otherwise <c>null</c>.</returns>
        MainDocumentData GetMainDocumentData(string alternativeUrl);
    }
}
