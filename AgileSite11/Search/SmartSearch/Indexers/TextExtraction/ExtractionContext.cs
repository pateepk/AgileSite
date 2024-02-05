using System;

namespace CMS.Search
{
    /// <summary>
    /// Extraction context passed to the extract method.
    /// </summary>
    public class ExtractionContext
    {
        /// <summary>
        /// Culture of the document the extracted file belongs to. This information can be used to transform DateTimes and numbers to correct string representations.
        /// If null, EN culture is used.
        /// </summary>
        public string Culture
        {
            get;
            set;
        }
    }
}
