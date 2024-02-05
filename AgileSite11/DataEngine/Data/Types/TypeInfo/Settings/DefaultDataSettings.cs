using System.Collections.Generic;

using CMS.DataEngine.Internal;


namespace CMS.DataEngine
{
    /// <summary>
    /// Settings for export of the default database data and web template data
    /// </summary>
    /// <remarks>
    /// This class is for internal use only and should not be used in custom code.
    /// </remarks>
    public class DefaultDataSettings
    {
        #region "Variables"

        private List<string> mExcludedPrefixes = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Where condition
        /// </summary>
        public string Where 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Order by columns
        /// </summary>
        public string OrderBy 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// List of child dependencies for population of child count column(s) upon default data retrieval.
        /// The child dependencies must contain no loops except direct reference to object type itself.
        /// </summary>
        public List<DefaultDataChildDependency> ChildDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns that should be excluded from the exported data.
        /// </summary>
        public List<string> ExcludedColumns 
        { 
            get; 
            set;
        }


        /// <summary>
        /// Indicates if data should be exported only as part of the web template data
        /// </summary>
        public bool IncludeToWebTemplateDataOnly
        {
            get;
            set;
        }


        /// <summary>
        /// List of code display name prefixes that will be excluded from default data. By default everything starting with 'test' is excluded.
        /// </summary>
        public List<string> ExcludedPrefixes
        {
            get
            {
                return mExcludedPrefixes ?? (mExcludedPrefixes = new List<string>{ "test" });
            }
        }

        #endregion
    }
}
