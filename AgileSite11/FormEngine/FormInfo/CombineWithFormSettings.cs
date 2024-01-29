namespace CMS.FormEngine
{
    /// <summary>
    /// Settings for combining form fields.
    /// </summary>
    public class CombineWithFormSettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CombineWithFormSettings()
        {
            PreserveCategory = true;
        }


        /// <summary>
        /// If true, existing field are overwritten. Default false.
        /// </summary>
        public bool OverwriteExisting
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns to exclude from the combining. Default none.
        /// </summary>
        public string ExcludeColumns
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the categories are included into combined form. Default false.
        /// </summary>
        public bool IncludeCategories
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the category is preserved for field even if not overwritten. Default true.
        /// </summary>
        public bool PreserveCategory
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the hidden fields are overwritten even in case the OverwriteExisting is false. Default false.
        /// </summary>
        public bool OverwriteHidden
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the empty categories are removed within the combine process to make sure the combined fields do not fall into wrong category. Default false.
        /// </summary>
        public bool RemoveEmptyCategories
        {
            get;
            set;
        }
    }
}