using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Index info interface.
    /// </summary>
    public interface ISearchIndexInfo : IAdvancedDataContainer
    {
        /// <summary>
        /// Search index settings for current object.
        /// </summary>
        SearchIndexSettings IndexSettings
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the index type (documents, forums, etc).
        /// </summary>
        string IndexType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the batch size for data loading.
        /// </summary>
        int IndexBatchSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets index code name.
        /// </summary>
        string IndexCodeName
        {
            get;
        }


        /// <summary>
        /// Defines a provider which performs actions on search index.
        /// </summary>
        string IndexProvider
        {
            get;
            set;
        }
    }
}