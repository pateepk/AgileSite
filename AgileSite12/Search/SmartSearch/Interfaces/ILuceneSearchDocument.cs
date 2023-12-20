using System;
using System.Linq;
using System.Text;

namespace CMS.Search
{
    /// <summary>
    /// Interface for the search document
    /// </summary>
    public interface ILuceneSearchDocument
    {
        /// <summary>
        /// Adds a general-purpose field to the document, handles field conversion
        /// </summary>
        /// <param name="name">Name of new field</param>
        /// <param name="value">Value of field</param>
        /// <param name="store">Should be value stored</param>
        /// <param name="tokenize">Should be value tokenized</param>
        void AddGeneralField(string name, object value, bool store, bool tokenize);
        

        /// <summary>
        /// Adds the given field to the document
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="value">Field value</param>
        /// <param name="store">If true, the field value is stored</param>
        /// <param name="tokenize">If true, the field value is tokenized</param>
        void Add(string name, string value, bool store = true, bool tokenize = false);


        /// <summary>
        /// Gets the value of specified field
        /// </summary>
        /// <param name="name">Field name</param>
        string Get(string name);


        /// <summary>
        /// Removes field with the given name
        /// </summary>
        /// <param name="name">Field name</param>
        void RemoveField(string name);
    }
}
