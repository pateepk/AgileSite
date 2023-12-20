using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Extension methods for working with <see cref="SearchDocument"/> instance.
    /// </summary>
    public static class SearchDocumentExtensions
    {
        /// <summary>
        /// Adds given search <paramref name="field"/> to search document.
        /// </summary>
        /// <param name="document">Search document to add field to.</param>
        /// <param name="field">Search field to be added.</param>
        public static void AddSearchField(this SearchDocument document, ISearchField field)
        {
            document.Add(field.FieldName, field.Value, field.GetFlag(SearchSettings.SEARCHABLE), field.GetFlag(SearchSettings.TOKENIZED));
        }


        /// <summary>
        /// Initializes search document from given parameters.
        /// </summary>
        /// <param name="document">Document to initialize.</param>
        /// <param name="index">Index the document belongs to</param>
        /// <param name="type">Type of document</param>   
        /// <param name="id">ID value</param>
        /// <param name="created">Document created</param>
        public static void Initialize(this SearchDocument document, ISearchIndexInfo index, string type, string id, DateTime created)
        {
            document.Initialize(
                new SearchDocumentParameters
                {
                    Index = index,
                    Type = type,
                    Id = id,
                    Created = created
                }
            );
        }


        /// <summary>
        /// Initializes search document from <paramref name="parameters"/>.
        /// </summary>
        /// <param name="document">Document to initialize.</param>
        /// <param name="parameters">Document initialization parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> does not have <see cref="SearchDocumentParameters.Type"/> or <see cref="SearchDocumentParameters.Id"/> specified.</exception>
        public static void Initialize(this SearchDocument document, SearchDocumentParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.Type) || string.IsNullOrEmpty(parameters.Id))
            {
                throw new ArgumentException("Type and Id in parameters must be specified.", nameof(parameters));
            }

            //Add default _type field
            document.Add(SearchFieldsConstants.TYPE, parameters.Type.ToLowerInvariant());

            //Add default _id field
            document.Add(SearchFieldsConstants.ID, parameters.Id);

            //Add default _site field
            document.Add(SearchFieldsConstants.SITE, parameters.SiteName);

            //Add default _created field
            document.Add(SearchFieldsConstants.CREATED, parameters.Created);

            //Add default _culture field
            document.Add(SearchFieldsConstants.CULTURE, parameters.Culture);

            //Add default _index field
            document.Add(SearchFieldsConstants.INDEX, parameters.Index?.IndexCodeName?.ToLowerInvariant() ?? "");
        }
    }
}
