using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents search field.
    /// </summary>
    public interface ISearchField
    {
        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        string FieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the field type.
        /// </summary>
        Type DataType
        {
            get;
            set;
        }


        /// <summary>
        /// Explicit analyzer to process search field's value. 
        /// When null, default analyzer is used based on search index configuration. 
        /// </summary>
        /// <remarks>
        /// The analyzer property is used in Lucene search only. For Azure Search, specify the analyzer programmatically upon
        /// index definition or querying.
        /// </remarks>
        SearchAnalyzerTypeEnum? Analyzer
        {
            get;
            set;
        }


        /// <summary>
        /// Field value. May be null when field is not initialized with it's value.
        /// </summary>
        object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a list of flag names.
        /// </summary>
        List<string> FlagNames
        {
            get;
        }


        /// <summary>
        /// Gets value of a flag associated with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the flag.</param>
        /// <returns>Value of flag <paramref name="name"/>. Returns false if flag does not exist.</returns>
        /// <seealso cref="SearchSettings.SEARCHABLE"/>
        /// <seealso cref="SearchSettings.TOKENIZED"/>
        bool GetFlag(string name);


        /// <summary>
        /// Sets value of a flag associated with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the flag.</param>
        /// <param name="value">Value to associate with the flag.</param>
        /// <seealso cref="SearchSettings.SEARCHABLE"/>
        /// <seealso cref="SearchSettings.TOKENIZED"/>
        void SetFlag(string name, bool value);
    }
}
