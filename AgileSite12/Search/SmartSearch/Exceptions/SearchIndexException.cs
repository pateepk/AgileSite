using System;
using System.Runtime.Serialization;

namespace CMS.Search
{
    /// <summary>
    /// Exception thrown during smart search operations on indexes.
    /// </summary>
    [Serializable]
    public class SearchIndexException : SearchException
    {
        /// <summary>
        /// Name of the <see cref="SearchIndexInfo"/> the exception is related to.
        /// </summary>
        public string SearchIndexName
        {
            get;
        }


        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error while processing index {0}: {1}", SearchIndexName, base.Message);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchIndexException"/> class for given <paramref name="indexName"/>.
        /// </summary>
        /// <param name="indexName">Codename of the <see cref="SearchIndexInfo"/> the exception is related to.</param>
        public SearchIndexException(string indexName)
            : this(indexName, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchIndexException"/> class for given <paramref name="indexName"/> with a specified error message.
        /// </summary>
        /// <param name="indexName">Codename of the <see cref="SearchIndexInfo"/> the exception is related to.</param>
        /// <param name="message">The message that describes the error.</param>
        public SearchIndexException(string indexName, string message)
            : this(indexName, message, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchIndexException"/> class for given <paramref name="indexName"/> with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="indexName">Codename of the <see cref="SearchIndexInfo"/> the exception is related to.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public SearchIndexException(string indexName, string message, Exception innerException)
            : base(message, innerException)
        {
            SearchIndexName = indexName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchIndexException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected SearchIndexException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            if (info != null)
            {
                SearchIndexName = info.GetString(nameof(SearchIndexName));
            }
        }


        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is a null reference.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue(nameof(SearchIndexName), SearchIndexName);
            }
        }
    }
}
