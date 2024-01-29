using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Search fields collection and field constants
    /// </summary>
    public interface ISearchFields
    {
        #region "Properties"

        /// <summary>
        /// Indicates if field values will be retrieved and stored in collection.
        /// </summary>
        bool StoreValues
        {
            get;
            set;
        }


        /// <summary>
        /// Search fields
        /// </summary>
        IEnumerable<ISearchField> Items
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds a new field to the collection. If the field already exists, it is updated (merged with existing one).
        /// When adding a content field and its value multiple times, the value is appended to the existing content field.
        /// </summary>
        /// <param name="searchField">Search field to be added to the collection.</param>
        /// <param name="getValueFunc">Function that returns value of the field.</param>
        /// <returns>The search field added.</returns>
        ISearchField Add(ISearchField searchField, Func<object> getValueFunc);


        /// <summary>
        /// Returns requested field. Null when field doesn't exist.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        ISearchField Get(string fieldName);


        /// <summary>
        /// Prepares value to add to content field.
        /// </summary>
        /// <param name="value">Value to add</param> 
        /// <param name="stripTags">Indicates whether tags should be stripped</param>
        string PrepareContentValue(object value, bool stripTags);

        #endregion
    }
}
