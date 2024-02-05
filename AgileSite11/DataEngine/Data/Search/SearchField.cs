using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents search field.
    /// </summary>
    public class SearchField : ISearchField
    {
        private readonly Dictionary<string, bool> flags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);


        #region "Properties"

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }


        /// <summary>
        /// Explicit analyzer to process search field's value. 
        /// When null default analyzer is used based on search index configuration. 
        /// </summary>
        public SearchAnalyzerTypeEnum? Analyzer
        {
            get;
            set;
        }


        /// <summary>
        /// Field value. May be null when field is not initialized with it's value.
        /// </summary>
        public object Value
        {
            get;
            set;
        }
        

        /// <summary>
        /// When field is marked to be insert directly, it will not be processed before it's inserted into search document. 
        /// Will not be passed to SearchHelper.AddGeneralField method.
        /// </summary>
        public bool InsertDirectly
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the field type.
        /// </summary>
        public Type DataType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a list of flag names.
        /// </summary>
        public List<string> FlagNames
        {
            get
            {
                return flags.Keys.ToList();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets value of a flag associated with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the flag.</param>
        /// <returns>Value of flag <paramref name="name"/>. Returns false if flag does not exist.</returns>
        /// <seealso cref="SearchSettings.SEARCHABLE"/>
        /// <seealso cref="SearchSettings.TOKENIZED"/>
        public bool GetFlag(string name)
        {
            bool flagValue;
            flags.TryGetValue(name, out flagValue);

            return flagValue;
        }


        /// <summary>
        /// Sets value of a flag associated with <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the flag.</param>
        /// <param name="value">Value to associate with the flag.</param>
        /// <seealso cref="SearchSettings.SEARCHABLE"/>
        /// <seealso cref="SearchSettings.TOKENIZED"/>
        public void SetFlag(string name, bool value)
        {
            flags[name] = value;
        }


        /// <summary>
        /// Converts this piece of data into a string.
        /// </summary>
        public override string ToString()
        {
            return FieldName + ":" + Value;
        }

        #endregion
    }
}
