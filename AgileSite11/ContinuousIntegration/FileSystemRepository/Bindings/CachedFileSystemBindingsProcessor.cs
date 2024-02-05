using System;
using System.Xml;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Provides methods for working with bindings in the file system repository.
    /// Files are cached and written to the file system at processor disposal. This allows to aggregate writes to the shared binding files.
    /// </summary>
    internal class CachedFileSystemBindingsProcessor : FileSystemBindingsProcessor, IDisposable
    {
        /// <summary>
        /// Cache for binding documents indexed by file paths.
        /// </summary>
        private MemoryCache<XmlDocument> mDocumentsCache;

        private bool mDisposed;


        protected MemoryCache<XmlDocument> DocumentsCache
        {
            get
            {
                return mDocumentsCache ?? (mDocumentsCache = new MemoryCache<XmlDocument>());
            }
            set
            {
                mDocumentsCache = value;
            }
        }


        /// <summary>
        /// Constructor with required <paramref name="translationHelper"/> and <paramref name="writer"/> parameter.
        /// </summary>
        /// <param name="translationHelper"><see cref="TranslationHelper"/>Object that is used to optimize database calls when translating IDs to <see cref="TranslationReference"/>.</param>
        /// <param name="configuration">Object containing repository configuration.</param>
        /// <param name="writer">Object for writing to file system and storing hash of written file in hash manager.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="translationHelper"/> or <paramref name="writer"/> not provided.</exception>
        public CachedFileSystemBindingsProcessor(TranslationHelper translationHelper, FileSystemRepositoryConfiguration configuration, IFileSystemWriter writer)
            : base(translationHelper, configuration, writer)
        {
            DocumentsCache = new MemoryCache<XmlDocument>();
        }


        /// <summary>
        /// Loads <paramref name="relativePath"/>'s content into a new XmlDocument.
        /// </summary>
        /// <param name="relativePath">Path to an existing file containing an XML document to be loaded.</param>
        protected override XmlDocument LoadDocumentFromFile(string relativePath)
        {
            var document = DocumentsCache.FetchItem(
                relativePath,
                () => base.LoadDocumentFromFile(relativePath));

            return document;
        }


        /// <summary>
        /// Creates a new document with binding elements structure, stores it in the document cache and returns it.
        /// </summary>
        /// <param name="relativePath">Relative path to a file within the repository where the bindings should be stored.</param>
        /// <param name="parent">Parent common for all bindings stored within the document (including currently processed only right now).</param>
        /// <param name="rootElementName">Name of binding's object type (serving as root element of the document).</param>
        /// <returns>New element for currently processed binding.</returns>
        protected override XmlDocument PrepareNewDocument(string relativePath, BaseInfo parent, string rootElementName)
        {
            var document = base.PrepareNewDocument(relativePath, parent, rootElementName);

            // Newly created document always needs to be flushed to the file system.
            DocumentsCache.SetItem(relativePath, document, markAsDirty: true);

            return document;
        }


        /// <summary>
        /// Appends binding to the document retrieved from provided <paramref name="relativePath"/>.
        /// If binding record already exists within the file, it is rewritten in the resulting document.
        /// If the file does not exist, completely new document is created with single <paramref name="binding"/> (the provided one) inside.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the file in the repository that holds all bindings for the <paramref name="binding"/>'s parent.</param>
        /// <returns>
        /// An <see cref="XmlDocument"/> that contains all already written bindings and the new/edited one
        /// or <see cref="FileSystemBindingsProcessor.NO_BINDINGS"/> when there was an error in the document loaded from <paramref name="relativePath"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When no <paramref name="binding"/> provided.</exception>
        protected override XmlDocument AppendBindingInternal(BaseInfo binding, string relativePath)
        {
            var document = base.AppendBindingInternal(binding, relativePath);

            DocumentsCache.MarkDirty(relativePath);

            return document;
        }


        /// <summary>
        /// Removes the <paramref name="binding"/> from the document retrieved from provided <paramref name="relativePath"/>.
        /// </summary>
        /// <param name="binding"><see cref="BaseInfo"/> object representing a binding (its object type has <see cref="ObjectTypeInfo.IsBinding"/> set).</param>
        /// <param name="relativePath">Relative path to the file in the repository that holds all bindings for the <paramref name="binding"/>'s parent.</param>
        /// <returns>
        /// Document loaded from <paramref name="relativePath"/> without provided <paramref name="binding"/>'s record.
        /// <para>If binding record does not exist within the file, unchanged document is returned.</para>
        /// <para>If no binding record exist within the file after the <paramref name="binding"/> removal, <see cref="FileSystemBindingsProcessor.NO_BINDINGS"/> is returned.</para>
        /// <para>If the file does not exist, <see cref="FileSystemBindingsProcessor.NO_BINDINGS"/> is returned.</para>
        /// </returns>
        protected override XmlDocument RemoveBindingInternal(BaseInfo binding, string relativePath)
        {
            var document = base.RemoveBindingInternal(binding, relativePath);

            if (document == NO_BINDINGS)
            {
                // Last binding has been removed from the file and the file will be deleted.
                DocumentsCache.RemoveItem(relativePath);
            }
            else
            {
                DocumentsCache.MarkDirty(relativePath);
            }

            return document;
        }


        /// <summary>
        /// Does nothing since the documents are cached until this object is disposed.
        /// </summary>
        /// <param name="relativePath">Target file path</param>
        /// <param name="document">XML document containing bindings.</param>
        protected override void WriteBindingsInternal(string relativePath, XmlDocument document)
        {
            // Do not save the file every time, flush cache on disposal.
        }


        /// <summary>
        /// Writes all cached documents, that have been modified since the initial read, to the file system.
        /// </summary>
        /// <param name="disposing">Indicates whether the disposal has already started.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Flush the cache to the file system
                foreach (var item in DocumentsCache.GetItems(true))
                {
                    base.WriteBindingsInternal(item.Key, item.Value);
                }
            }

            mDisposed = true;
        }


        /// <summary>
        /// Writes all cached documents to the file system.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
