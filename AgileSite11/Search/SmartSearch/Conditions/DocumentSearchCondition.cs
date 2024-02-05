namespace CMS.Search
{
    /// <summary>
    /// Specifies parameters for document search condition
    /// </summary>
    /// <seealso cref="SearchCondition"/>
    public class DocumentSearchCondition
    {
        /// <summary>
        /// Class names to include in the search
        /// </summary>
        public string ClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Culture to search
        /// </summary>
        public string Culture
        {
            get;
            set;
        }


        /// <summary>
        /// Default culture
        /// </summary>
        public string DefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Combine with default culture. Default false
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentSearchCondition()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="classNames">Class names to include in the search</param>
        /// <param name="culture">Culture to search</param>
        /// <param name="defaultCulture">Default culture</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture</param>
        public DocumentSearchCondition(string classNames, string culture, string defaultCulture, bool combineWithDefaultCulture)
        {
            ClassNames = classNames;
            Culture = culture;
            DefaultCulture = defaultCulture;
            CombineWithDefaultCulture = combineWithDefaultCulture;
        }
    }
}