using CMS.DataEngine;
using CMS.Helpers;
using System;
using System.Data;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Collection of the attachments
    /// </summary>
    public class DocumentAttachmentCollection : InfoObjectCollection<DocumentAttachment>
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


        #region "Methods"

        /// <summary>
        /// Constructor. Creates a static collection populated from DataSet
        /// </summary>
        public DocumentAttachmentCollection(DataSet sourceData = null)
            : base(sourceData)
        {
            AutomaticNameColumn = false;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentDocument">Parent document node</param>
        public DocumentAttachmentCollection(TreeNode parentDocument)
        {
            ParentDocument = parentDocument;
            AutomaticNameColumn = false;
        }


        /// <summary>
        /// Creates new instance of the encapsulated object.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        public override DocumentAttachment CreateNewObject(DataRow dr)
        {
            return new DocumentAttachment(dr);
        }


        /// <summary>
        /// Gets the where condition by object name
        /// </summary>
        /// <param name="name">Object name</param>
        public override IWhereCondition GetNameWhereCondition(string name)
        {
            // Convert name to guid
            var guid = ValidationHelper.GetGuid(name, Guid.Empty);
            if (guid != Guid.Empty)
            {
                return new WhereCondition().WhereEquals("AttachmentGUID", guid);
            }

            return new WhereCondition().WhereEquals("AttachmentName", name);
        }


        /// <summary>
        /// Gets the complete where condition for the collection
        /// </summary>
        public override WhereCondition GetCompleteWhereCondition()
        {
            var where = new WhereCondition(Where);

            // Add dynamic where condition if set
            if (DynamicWhereCondition != null)
            {
                where.Where(DynamicWhereCondition());
            }

            return where;
        }


        /// <summary>
        /// Gets the data for the collection.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="offset">Starting offset for the data</param>
        /// <param name="maxRecords">Maximum number of records to get</param>
        /// <param name="totalRecords">Returning total number of records</param>
        protected override DataSet GetData(IWhereCondition where, int offset, int maxRecords, ref int totalRecords)
        { 
            // No results if disconnected
            if (IsDisconnected)
            {
                return null;
            }

            IObjectQuery query;

            if (ParentDocument.IsLastVersion)
            {
                query = DocumentHelper.GetAttachments(ParentDocument, LoadBinaryData);
            }
            else
            {
                query = AttachmentInfoProvider.GetAttachments(ParentDocument.DocumentID, LoadBinaryData).ExceptVariants();
            }

            var completeWhere = GetCompleteWhereCondition().Where(where);

            query.ApplySettings(w => w.Where(completeWhere));
            query.Offset = offset;
            query.MaxRecords = maxRecords;

            var result = query.Result;
            totalRecords = query.TotalRecords;

            return result;
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<DocumentAttachment> Clone()
        {
            // Create new instance and copy over the properties
            var result = new DocumentAttachmentCollection(ParentDocument);
            CopyPropertiesTo(result);

            return result;
        }


        /// <summary>
        /// Copies the properties of this collection to the other collection
        /// </summary>
        /// <param name="col">Target collection</param>
        protected override void CopyPropertiesTo(IInfoObjectCollection col)
        {
            base.CopyPropertiesTo(col);

            var docAttachmentCollection = col as DocumentAttachmentCollection;
            if (docAttachmentCollection != null)
            {
                docAttachmentCollection.ParentDocument = ParentDocument;
            }
        }

        #endregion
    }


    /// <summary>
    /// Collection of the attachments which only covers unsorted attachments
    /// </summary>
    internal class UnsortedDocumentAttachmentCollection : DocumentAttachmentCollection
    {
        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentDocument">Parent document node</param>
        public UnsortedDocumentAttachmentCollection(TreeNode parentDocument)
            : base(parentDocument)
        {
        }


        /// <summary>
        /// Ensures the proper values for the given object
        /// </summary>
        protected override void EnsureObjectValues(BaseInfo item)
        {
            base.EnsureObjectValues(item);

            // Ensure the attachment values
            var attachment = (DocumentAttachment)item;

            attachment.AttachmentIsUnsorted = true;
        }


        /// <summary>
        /// Gets the complete where condition for the collection
        /// </summary>
        public override WhereCondition GetCompleteWhereCondition()
        {
            var where = base.GetCompleteWhereCondition();

            where.WhereTrue("AttachmentIsUnsorted");

            return where;
        }

        #endregion
    }
}
