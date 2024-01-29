using System;
using System.Threading;

using CMS.DataEngine;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Creates Azure Search <see cref="Document"/>s for indexing.
    /// </summary>
    public class DocumentCreator
    {
        private static DocumentCreator mInstance;


        private DataMapper DataMapper
        {
            get
            {
                return DataMapper.Instance;
            }
        }


        /// <summary>
        /// Gets the <see cref="DocumentCreator"/> instance.
        /// </summary>
        public static DocumentCreator Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new DocumentCreator(), null);
                }
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// An event raised upon <see cref="CreateDocument"/> execution. The before phase allows for modification of processed <see cref="CreateDocumentEventArgs.SearchDocument"/>,
        /// the after phase allows for modification of resulting <see cref="CreateDocumentEventArgs.Document"/>.
        /// </summary>
        public readonly CreateDocumentHandler CreatingDocument = new CreateDocumentHandler { Name = "DocumentCreator.CreatingDocument" };


        /// <summary>
        /// An event raised upon adding a new <see cref="Document"/> value. The event allows for modification of <see cref="AddDocumentValueEventArgs.AzureName"/> and <see cref="AddDocumentValueEventArgs.Value"/> before
        /// it is passed to <see cref="CMS.Search.Azure.DataMapper.ConvertValue"/>.
        /// </summary>
        public readonly AddDocumentValueHandler AddingDocumentValue = new AddDocumentValueHandler { Name = "DocumentCreator.AddingDocumentValue" };


        /// <summary>
        /// Initializes a new document creator.
        /// </summary>
        internal DocumentCreator()
        {
        }


        /// <summary>
        /// Creates an Azure Search <see cref="Document"/> for given <paramref name="searchDocument"/>. The <paramref name="searchable"/> and <paramref name="searchIndex"/> parameters
        /// are to provide contextual information for the creation.
        /// </summary>
        /// <param name="searchDocument">Search document for which to create an Azure Search document.</param>
        /// <param name="searchable">Searchable for which the document is being created.</param>
        /// <param name="searchIndex">Index for which the document is being created.</param>
        /// <returns>Returns Azure Search document for <paramref name="searchable"/>.</returns>
        public Document CreateDocument(SearchDocument searchDocument, ISearchable searchable, ISearchIndexInfo searchIndex)
        {
            if (searchDocument == null)
            {
                throw new ArgumentNullException(nameof(searchDocument));
            }

            var eventArgs = new CreateDocumentEventArgs
            {
                SearchDocument = searchDocument,
                Document = new Document(),
                Searchable = searchable,
                SearchIndex = searchIndex
            };

            using (var h = CreatingDocument.StartEvent(eventArgs))
            {
                if (!h.CanContinue())
                {
                    return eventArgs.Document;
                }

                foreach (var name in eventArgs.SearchDocument.Names)
                {
                    AddDocumentValue(eventArgs.Document, eventArgs.SearchDocument, name, searchable, searchIndex);
                }

                h.FinishEvent();
            }
            return eventArgs.Document;
        }


        /// <summary>
        /// Adds value of <paramref name="searchDocument"/> field <paramref name="name"/> to Azure Search <paramref name="document"/>.
        /// </summary>
        /// <param name="document">Azure Search document to which to add value.</param>
        /// <param name="searchDocument">Search document whose <paramref name="name"/> field is to be added.</param>
        /// <param name="name">Name of <paramref name="searchDocument"/> field being added.</param>
        /// <param name="searchable">Searchable to which the Azure Search document relates.</param>
        /// <param name="searchIndex">Index to which the Azure Search document relates.</param>
        private void AddDocumentValue(Document document, SearchDocument searchDocument, string name, ISearchable searchable, ISearchIndexInfo searchIndex)
        {
            var eventArgs = new AddDocumentValueEventArgs
            {
                Name = name,
                AzureName = NamingHelper.GetValidFieldName(name),
                Value = searchDocument.GetValue(name),
                Document = document,
                SearchDocument = searchDocument,
                Searchable = searchable,
                SearchIndex = searchIndex
            };

            using (AddingDocumentValue.StartEvent(eventArgs))
            {
                var convertedValue = ConvertValue(eventArgs.Value, name, searchable);

                ProcessSystemField(eventArgs.Name, ref convertedValue);

                document.Add(eventArgs.AzureName, convertedValue);
            }
        }


        /// <summary>
        /// Performs additional processing if <paramref name="name"/> is a system field.
        /// </summary>
        private void ProcessSystemField(string name, ref object convertedValue)
        {
            var isIdColumn = name.Equals(SearchFieldsConstants.ID, StringComparison.OrdinalIgnoreCase);
            if (isIdColumn)
            {
                ValidateDocumentKeyType(convertedValue);
                convertedValue = NamingHelper.GetValidDocumentKey((string)convertedValue);
            }

            var isSiteOrCultureColumn = name.Equals(SearchFieldsConstants.SITE, StringComparison.OrdinalIgnoreCase) || name.Equals(SearchFieldsConstants.CULTURE, StringComparison.OrdinalIgnoreCase);
            if (isSiteOrCultureColumn)
            {
                convertedValue = ((string)convertedValue)?.ToLowerInvariant();
            }
        }


        /// <summary>
        /// Converts search document value to Azure Document value while providing additional details in case of an error.
        /// </summary>
        private object ConvertValue(object value, string name, ISearchable searchable)
        {
            try
            {
                return DataMapper.ConvertValue(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while converting value '{value}' of search document field '{name}' of type '{searchable.SearchType}'.", ex);
            }
        }


        private void ValidateDocumentKeyType(object documentKey)
        {
            if (documentKey is string)
            {
                return;
            }

            throw new InvalidOperationException("The Azure Search document key column value must be of type string.");
        }
    }
}
