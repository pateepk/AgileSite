using System;
using System.Collections.Generic;
using System.Threading;

using CMS.DataEngine;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Creates Azure Search field definitions.
    /// </summary>
    public class DocumentFieldCreator
    {
        private static DocumentFieldCreator mInstance;


        private DataMapper DataMapper
        {
            get
            {
                return DataMapper.Instance;
            }
        }


        /// <summary>
        /// Gets the <see cref="DocumentFieldCreator"/> instance.
        /// </summary>
        public static DocumentFieldCreator Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new DocumentFieldCreator(), null);
                }
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// An event raised upon <see cref="CreateFields"/> execution. The before phase allows for modification of processed <see cref="CreateFieldsEventArgs.SearchFields"/>,
        /// the after phase allows for modification of resulting <see cref="CreateFieldsEventArgs.Fields"/>.
        /// </summary>
        public readonly CreateFieldsHandler CreatingFields = new CreateFieldsHandler { Name = "DocumentFieldCreator.CreatingFields" };


        /// <summary>
        /// An event raised upon <see cref="CreateField"/> execution. The before phase allows for custom initialization of <see cref="CreateFieldEventArgs.Field"/>,
        /// the after phase allows for modification of resulting <see cref="CreateFieldEventArgs.Field"/> properties.
        /// </summary>
        public readonly CreateFieldHandler CreatingField = new CreateFieldHandler { Name = "DocumentFieldCreator.CreatingField" };


        /// <summary>
        /// Initializes a new document field creator.
        /// </summary>
        internal DocumentFieldCreator()
        {
        }


        /// <summary>
        /// Creates an Azure Search <see cref="Field"/> collection for given <paramref name="searchable"/>. The <paramref name="searchIndex"/> parameter
        /// is to provide contextual information for the creation.
        /// </summary>
        /// <param name="searchable">Searchable for which to create a collection of Azure Search fields.</param>
        /// <param name="searchIndex">Index for which the fields are being created.</param>
        /// <returns>Returns Azure Search fields for <paramref name="searchable"/>.</returns>
        public List<Field> CreateFields(ISearchable searchable, ISearchIndexInfo searchIndex)
        {
            ISearchFields searchFields = new SearchFields();
            searchFields.StoreValues = true;
            searchFields.AddSystemFields();
            searchFields = searchable.GetSearchFields(searchIndex, searchFields);

            var eventArgs = new CreateFieldsEventArgs
            {
                Fields = new List<Field>(),
                SearchFields = searchFields.Items,
                Searchable = searchable,
                SearchIndex = searchIndex
            };

            using (var h = CreatingFields.StartEvent(eventArgs))
            {
                if (!h.CanContinue())
                {
                    return eventArgs.Fields;
                }

                foreach (var searchField in eventArgs.SearchFields)
                {
                    var field = CreateField(searchField, eventArgs.Searchable, eventArgs.SearchIndex);

                    eventArgs.Fields.Add(field);
                }

                h.FinishEvent();
            }
            return eventArgs.Fields;
        }


        /// <summary>
        /// Creates an Azure Search <see cref="Field"/> for given <paramref name="searchField"/>. The <paramref name="searchIndex"/> and <paramref name="searchable"/> parameters
        /// are to provide contextual information for the creation.
        /// </summary>
        /// <param name="searchField">Search field for which to create an Azure Search counterpart.</param>
        /// <param name="searchable">Searchable for which the field is being created.</param>
        /// <param name="searchIndex">Index for which the field is being created.</param>
        /// <returns>Returns Azure Search field representation for <paramref name="searchField"/>.</returns>
        /// <remarks>
        /// The default implementation maps <see cref="AzureSearchFieldFlags.RETRIEVABLE"/>, <see cref="AzureSearchFieldFlags.SEARCHABLE"/>, <see cref="AzureSearchFieldFlags.FACETABLE"/>,
        /// <see cref="AzureSearchFieldFlags.FILTERABLE"/> and <see cref="AzureSearchFieldFlags.SORTABLE"/> flags  to their Azure Search counterparts.
        /// </remarks>
        public Field CreateField(ISearchField searchField, ISearchable searchable, ISearchIndexInfo searchIndex)
        {
            if (searchField == null)
            {
                throw new ArgumentNullException(nameof(searchField));
            }
            if (String.IsNullOrEmpty(searchField.FieldName))
            {
                throw new ArgumentException("Field name of search field must be provided.");
            }
            if (searchField.DataType == null)
            {
                throw new ArgumentException($"Data type of search field with name '{searchField.FieldName}' must be provided for object type '{searchable.SearchType}'.");
            }

            var eventArgs = new CreateFieldEventArgs
            {
                SearchField = searchField,
                Searchable = searchable,
                SearchIndex = searchIndex
            };

            using (var h = CreatingField.StartEvent(eventArgs))
            {
                if (!h.CanContinue())
                {
                    return eventArgs.Field;
                }

                if (eventArgs.Field == null)
                {
                    var isIdColumn = searchField.FieldName.Equals(SearchFieldsConstants.ID, StringComparison.OrdinalIgnoreCase);

                    var fieldName = NamingHelper.GetValidFieldName(searchField.FieldName);
                    var fieldType = DataMapper.MapType(searchField.DataType);

                    eventArgs.Field = new Field(fieldName, fieldType)
                    {
                        IsKey = isIdColumn,

                        IsFacetable = searchField.GetFlag(AzureSearchFieldFlags.FACETABLE),
                        IsFilterable = searchField.GetFlag(AzureSearchFieldFlags.FILTERABLE),
                        IsRetrievable = searchField.GetFlag(AzureSearchFieldFlags.RETRIEVABLE),
                        IsSearchable = searchField.GetFlag(AzureSearchFieldFlags.SEARCHABLE),
                        IsSortable = searchField.GetFlag(AzureSearchFieldFlags.SORTABLE)
                    };
                }

                h.FinishEvent();
            }

            return eventArgs.Field;
        }
    }
}
