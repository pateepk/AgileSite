namespace CMS.WebAnalytics
{
    /// <summary>
    ///  Data class containing all fields required for asynchronous search hit logging.
    /// </summary>
    public class LogSearchHitParameters
    {
        /// <summary>
        /// Keyword representing the search query made by search engine.
        /// </summary>
        public string Keyword
        {
            get;
            set;
        }


        /// <summary>
        /// Alias path to the document from which was the request made.
        /// </summary>
        public string NodeAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Culture of the document from which was the request made.
        /// </summary>
        public string DocumentCultureCode
        {
            get;
            set;
        }
    }
}