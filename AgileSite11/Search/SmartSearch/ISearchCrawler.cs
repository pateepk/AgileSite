namespace CMS.Search
{
    /// <summary>
    /// Contract for a search crawler which indexes HTML content of a web page.
    /// </summary>
    public interface ISearchCrawler
    {
        /// <summary>
        /// Gets the crawler user password.
        /// </summary>
        string CrawlerPassword
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the crawler user name which should be used for current search.
        /// </summary>
        string CrawlerUserName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the user name which should be used for crawler search.
        /// </summary>
        string CrawlerFormsUserName
        {
            get;
            set;
        }


        /// <summary>
        /// Downloads the HTML code for the specified URL.
        /// </summary>
        /// <param name="url">URL of the page to download.</param>
        string DownloadHtmlContent(string url);
    }
}
