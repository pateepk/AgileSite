using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;

[assembly: RegisterImplementation(typeof(ISearchFields), typeof(SearchFields), Priority = CMS.Core.RegistrationPriority.SystemDefault, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.Search
{
    /// <summary>
    /// Search fields collection and field constants
    /// </summary>
    public class SearchFields : ISearchFields
    {
        #region "Variables"

        private readonly Dictionary<string, ISearchField> mSearchFields = new Dictionary<string, ISearchField>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if field values will be retrieved and stored in collection.
        /// </summary>
        public bool StoreValues
        {
            get;
            set;
        }


        /// <summary>
        /// Search fields
        /// </summary>
        public IEnumerable<ISearchField> Items
        {
            get
            {
                return mSearchFields.Values;
            }
        }


        private string ContentFieldName 
        {
            get
            {
                return SearchFieldsConstants.CONTENT;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor
        /// </summary>
        public SearchFields() { }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storeValues">Indicates if field values will be stored in fields.</param>
        public SearchFields(bool storeValues)
        {
            StoreValues = storeValues;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Adds a new field to the collection. If the field already exists, it is updated (merged with existing one).
        /// When adding a content field and its value multiple times, the value is appended to the existing content field.
        /// </summary>
        /// <param name="searchField">Search field to be added to the collection.</param>
        /// <param name="getValueFunc">Function that returns value of the field.</param>
        /// <returns>The search field added.</returns>
        public virtual ISearchField Add(ISearchField searchField, Func<object> getValueFunc)
        {
            if (StoreValues && (getValueFunc != null))
            {
                searchField.Value = getValueFunc();
            }

            return AddInternal(searchField);
        }


        /// <summary>
        /// Returns requested field. Null when field doesn't exist.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public virtual ISearchField Get(string fieldName)
        {
            ISearchField field;

            if (mSearchFields.TryGetValue(fieldName, out field))
            {
                return field;
            }

            return null;
        }


        /// <summary>
        /// Prepares value to add to content field.
        /// </summary>
        /// <param name="value">Value to add</param> 
        /// <param name="stripTags">Indicates whether tags should be stripped</param>
        public virtual string PrepareContentValue(object value, bool stripTags)
        {
            return SearchHelper.PrepareContentValue(value, stripTags);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Adds new field to collection. When field already exists, it will be updated (merged with existing one). 
        /// When adding content field with it's value multiple times, the value is appended to existing content.
        /// </summary>
        /// <param name="field">Field to add</param>
        /// <returns>Newly added or existing field</returns>
        /// <exception cref="InvalidOperationException">When StoreValues is true and we try to add already existing field but with different value.</exception>
        protected virtual ISearchField AddInternal(ISearchField field)
        {
            // Field already exists, update field
            if (mSearchFields.ContainsKey(field.FieldName))
            {
                return UpdateExistingField(field);
            }

            return InsertNewField(field);
        }


        /// <summary>
        /// Inserts new search field into collection
        /// </summary>
        /// <param name="newField">Field to insert</param>
        /// <returns>Inserted field</returns>
        protected virtual ISearchField InsertNewField(ISearchField newField)
        {
            // Special case, content field appends value to string builder. 
            if (newField.FieldName == ContentFieldName)
            {
                var sb = new StringBuilder(1024);
                sb.Append(newField.Value ?? "");
                newField.Value = sb;
            }

            // Add new field to collection
            mSearchFields.Add(newField.FieldName, newField);

            return newField;
        }


        /// <summary>
        /// Updates existing search field in collection
        /// </summary>
        /// <param name="newField">Field with new data</param>
        /// <returns>Updated field</returns>
        /// <exception cref="InvalidOperationException">When StoreValues is true and we try to add already existing field but with different value.</exception>
        /// <exception cref="InvalidOperationException">When field with given field name doesn't exist.</exception>
        protected virtual ISearchField UpdateExistingField(ISearchField newField)
        {
            var existingField = mSearchFields[newField.FieldName];

            if (existingField == null)
            {
                throw new InvalidOperationException(String.Format("[CMS.Search.SearchFields]: Cannot update search field {0} because it doesn't exist.", newField.FieldName));
            }

            if (StoreValues && (newField.Value != null))
            {
                // Special case, content field appends value to existing content
                if ((existingField.FieldName == ContentFieldName) && (existingField.Value is StringBuilder))
                {
                    ((StringBuilder)existingField.Value).Append(" ", newField.Value);
                }
                // Standard case
                else
                {
                    // If value is already set and it's different from new value, throw exception. 
                    if ((existingField.Value != null) && !existingField.Value.Equals((newField.Value)))
                    {
                        throw new InvalidOperationException(String.Format("[CMS.Search.SearchFields]: Index field {0} cannot contain more then one value.", existingField.FieldName));
                    }

                    existingField.Value = newField.Value;
                }
            }

            foreach (var flagName in newField.FlagNames.Where(newField.GetFlag))
            {
                existingField.SetFlag(flagName, true);
            }

            existingField.Analyzer = existingField.Analyzer ?? newField.Analyzer;

            return existingField;
        }

        #endregion
    }
}
