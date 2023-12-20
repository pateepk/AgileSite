namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Data of main document that are used in <see cref="IAlternativeUrlsService"/>.
    /// </summary>
    public class MainDocumentData
    {
        /// <summary>
        /// URL of main document.
        /// </summary>
        public string Url { get; }


        /// <summary>
        /// Culture code of main document.
        /// </summary>
        public string Culture { get; }


        /// <summary>
        /// Initializes instance of <see cref="MainDocumentData"/>.
        /// </summary>
        /// <param name="url">URL of main document.</param>
        /// <param name="culture">Culture code of main document.</param>
        public MainDocumentData(string url, string culture)
        {
            Url = url;
            Culture = culture;
        }
    }
}
