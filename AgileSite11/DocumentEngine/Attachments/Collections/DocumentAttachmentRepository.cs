using System;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Repository for document attachments
    /// </summary>
    public class DocumentAttachmentRepository : InfoObjectRepository<DocumentAttachmentCollection, DocumentAttachment, InfoCollectionSettings>
    {
        #region "Properties"
        
        /// <summary>
        /// Parent document.
        /// </summary>
        public TreeNode ParentDocument
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentDocument">Parent document</param>
        public DocumentAttachmentRepository(TreeNode parentDocument)
        {
            ParentDocument = parentDocument;
        }


        /// <summary>
        /// Loads the given collection.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        protected override DocumentAttachmentCollection LoadCollection(InfoCollectionSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            var collection = NewCollection(settings.ObjectType);

            SetupCollection(collection, settings);
            StoreCollection(collection, settings);

            return collection;
        }


        /// <summary>
        /// Creates new collection for the data.
        /// </summary>
        /// <param name="type">Object type of the collection</param>
        public override DocumentAttachmentCollection NewCollection(string type)
        {
            return new DocumentAttachmentCollection(ParentDocument);
        }


        /// <summary>
        /// Creates new combined collection for the data.
        /// </summary>
        public override CombinedInfoObjectCollection<DocumentAttachmentCollection, DocumentAttachment> NewCombinedCollection()
        {
            return new CombinedDocumentAttachmentCollection();
        }

        #endregion
    }
}