using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents collection of field values for indexing.
    /// </summary>
    public class SearchDocument
    {
        private readonly Dictionary<string, SearchDocumentField> data = new Dictionary<string, SearchDocumentField>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Name of the field in the search document whose value defines if the search document should be included in an index.
        /// </summary>
        public const string DOCUMENT_EXCLUDED_FROM_SEARCH_FIELD = "documentsearchexcluded";


        /// <summary>
        /// Gets enumeration of field names contained in the search document.
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return data.Keys;
            }
        }


        /// <summary>
        /// Adds new field data to the search document.
        /// </summary>
        /// <param name="name">Name of field.</param>
        /// <param name="value">Field data.</param>
        public void Add(string name, object value)
        {
            Add(name, value, true, false);
        }


        /// <summary>
        /// Adds new field data to the search document.
        /// </summary>
        /// <param name="name">Name of field.</param>
        /// <param name="value">Field data.</param>
        /// <param name="store">Indicates whether value should be stored in a retrievable representation. Considered only if index does not accept this information upon index definition.</param>
        /// <param name="tokenize">Indicates whether value should be tokenized. Considered only if index does not accept this information upon index definition.</param>
        /// <exception cref="ArgumentException">Thrown when filed of given <paramref name="name"/> is already present in the document.</exception>
        public void Add(string name, object value, bool store, bool tokenize)
        {
            var field = new SearchDocumentField(value, store, tokenize);

            try
            {
                data.Add(name, field);
            }
            catch(ArgumentException ex)
            {
                throw new ArgumentException($"Field of name '{name}' is already present in the search document.", nameof(name), ex);
            }
        }


        /// <summary>
        /// Returns <c>true</c> if document contains column <paramref name="name"/>. <c>false</c> otherwise.
        /// </summary>
        public bool Contains(string name)
        {
            return data.ContainsKey(name);
        }


        /// <summary>
        /// Gets value of a field.
        /// </summary>
        /// <param name="name">Name of the field to retrieve value for.</param>
        /// <returns>Value associated with field.</returns>
        /// <exception cref="ArgumentException">Thrown when field of given name does not exist.</exception>
        public object GetValue(string name)
        {
            return GetField(name).Value;
        }


        /// <summary>
        /// Gets store flag of a field.
        /// </summary>
        /// <param name="name">Name of the field to retrieve flag value for.</param>
        /// <returns>Store flag associated with field.</returns>
        /// <exception cref="ArgumentException">Thrown when field of given name does not exist.</exception>
        public bool GetStore(string name)
        {
            return GetField(name).Store;
        }


        /// <summary>
        /// Gets tokenize flag of a field.
        /// </summary>
        /// <param name="name">Name of the field to retrieve flag value for.</param>
        /// <returns>Tokenize flag associated with field.</returns>
        /// <exception cref="ArgumentException">Thrown when field of given name does not exist.</exception>
        public bool GetTokenize(string name)
        {
            return GetField(name).Tokenize;
        }


        /// <summary>
        /// Removes the field with specified name from the search document.
        /// </summary>
        /// <param name="name">Name of the field to remove.</param>
        public void Remove(string name)
        {
            data.Remove(name);
        }


        /// <summary>
        /// Gets field by name.
        /// </summary>
        private SearchDocumentField GetField(string name)
        {
            SearchDocumentField result;
            if (!data.TryGetValue(name, out result))
            {
                throw new ArgumentException($"Field '{name}' does not exist in the search document.", nameof(name));
            }
            return result;
        }


        /// <summary>
        /// Class holding search document data.
        /// </summary>
        private class SearchDocumentField
        {
            public object Value
            {
                get;
            }


            public bool Store
            {
                get;
            }


            public bool Tokenize
            {
                get;
            }


            public SearchDocumentField(object value, bool store, bool tokenize)
            {
                Value = value;
                Store = store;
                Tokenize = tokenize;
            }
        }
    }
}
