namespace CMS.Search
{
    /// <summary>
    /// Extraction context passed to the extract method. Can be used to transform DateTimes and numbers to correct string representations.
    /// </summary>
    public class ExtractionContext
    {
        /// <summary>
        /// Culture of the document the extracted file belongs to. This information can be used to transform DateTimes and numbers to correct string representations.
        /// </summary>
        public string Culture
        {
            get;
            set;
        }
    }
}
