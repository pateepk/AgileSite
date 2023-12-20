using System;
using System.Runtime.Serialization;

using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Exception that is thrown when <see cref="AlternativeUrlInfo"/> is in conflict with another URL or constraint.
    /// </summary>
    [Serializable]
    public class InvalidAlternativeUrlException : Exception
    {
        /// <summary>
        /// Alternative URL that is invalid.
        /// </summary>
        public AlternativeUrlInfo AlternativeUrl { get; }


        /// <summary>
        /// Excluded URL that is in conflict with <see cref="AlternativeUrl"/>. 
        /// </summary>
        public string ExcludedUrl { get; }


        /// <summary>
        /// <see cref="AlternativeUrlInfo"/> that is in conflict with <see cref="AlternativeUrl"/>.
        /// </summary>
        public AlternativeUrlInfo ConflictingAlternativeUrl { get; }


        /// <summary>
        /// Page that is in conflict with <see cref="AlternativeUrl"/>.
        /// </summary>
        public TreeNode ConflictingPage { get; }


        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                if (ConflictingPage != null)
                {
                    return $"Alternative URL '{AlternativeUrl.AlternativeUrlUrl}' is in conflict with the URL of a page {AlternativeUrlHelper.GetDocumentIdentification(ConflictingPage.DocumentNamePath, ConflictingPage.DocumentCulture)}.";
                }

                if (ConflictingAlternativeUrl != null)
                {
                    return $"Alternative URL '{AlternativeUrl.AlternativeUrlUrl}' of page {AlternativeUrlHelper.GetDocumentIdentification(AlternativeUrl)} is in conflict with another alternative URL of page {AlternativeUrlHelper.GetDocumentIdentification(ConflictingAlternativeUrl)}.";
                }

                if (!String.IsNullOrEmpty(ExcludedUrl))
                {
                    return $"Alternative URL '{AlternativeUrl.AlternativeUrlUrl}' is in conflict with '{ExcludedUrl}' excluded URL.";
                }

                if (AlternativeUrl != null)
                {
                    return $"Alternative URL '{AlternativeUrl.AlternativeUrlUrl}' does not satisfy pattern constraint.";
                }

                return base.Message;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        public InvalidAlternativeUrlException() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidAlternativeUrlException(string message)
            : base(message) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidAlternativeUrlException(string message, Exception innerException)
            : base(message, innerException) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that does not match pattern constraint.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl)
        {
            AlternativeUrl = invalidAlternativeUrl ?? throw new ArgumentNullException(nameof(invalidAlternativeUrl));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that does not match pattern constraint.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, Exception innerException)
            : base(null, innerException)
        {
            AlternativeUrl = invalidAlternativeUrl ?? throw new ArgumentNullException(nameof(invalidAlternativeUrl));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that is excluded URL.</param>
        /// <param name="excludedUrl">Excluded URL that is in conflict with <paramref name="invalidAlternativeUrl"/>.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, string excludedUrl)
            : this(invalidAlternativeUrl, excludedUrl, null) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class with a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that does not satisfy settings constraint.</param>
        /// <param name="excludedUrl">Excluded URL that is in conflict with <paramref name="invalidAlternativeUrl"/>.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, string excludedUrl, Exception innerException)
            : base(null, innerException)
        {
            if (String.IsNullOrEmpty(excludedUrl))
            {
                throw new ArgumentException(nameof(excludedUrl));
            }

            AlternativeUrl = invalidAlternativeUrl ?? throw new ArgumentNullException(nameof(invalidAlternativeUrl));
            ExcludedUrl = excludedUrl;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that is in conflict with another <see cref="AlternativeUrlInfo"/>.</param>
        /// <param name="conflictingAlternativeUrl">Existing <see cref="AlternativeUrlInfo"/> with the same <see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> as <paramref name="invalidAlternativeUrl"/>.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, AlternativeUrlInfo conflictingAlternativeUrl)
            : this(invalidAlternativeUrl, conflictingAlternativeUrl, null) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that is in conflict with another <see cref="AlternativeUrlInfo"/>.</param>
        /// <param name="conflictingAlternativeUrl">Existing <see cref="AlternativeUrlInfo"/> with the same <see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> as <paramref name="invalidAlternativeUrl"/>.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, AlternativeUrlInfo conflictingAlternativeUrl, Exception innerException)
            : base(null, innerException)
        {
            AlternativeUrl = invalidAlternativeUrl ?? throw new ArgumentNullException(nameof(invalidAlternativeUrl));
            ConflictingAlternativeUrl = conflictingAlternativeUrl ?? throw new ArgumentNullException(nameof(conflictingAlternativeUrl));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that is in conflict with another <see cref="AlternativeUrlInfo"/>.</param>
        /// <param name="conflictingPage">Existing page with the same URL as <paramref name="invalidAlternativeUrl"/>.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, TreeNode conflictingPage)
            : this(invalidAlternativeUrl, conflictingPage, null) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class.
        /// </summary>
        /// <param name="invalidAlternativeUrl">Alternative URL that is in conflict with another <see cref="AlternativeUrlInfo"/>.</param>
        /// <param name="conflictingPage">Existing page with the same URL as <paramref name="invalidAlternativeUrl"/>.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidAlternativeUrlException(AlternativeUrlInfo invalidAlternativeUrl, TreeNode conflictingPage, Exception innerException)
            : base(null, innerException)
        {
            AlternativeUrl = invalidAlternativeUrl ?? throw new ArgumentNullException(nameof(invalidAlternativeUrl));
            ConflictingPage = conflictingPage ?? throw new ArgumentNullException(nameof(conflictingPage));
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlternativeUrlException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidAlternativeUrlException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}