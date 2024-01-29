using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides the lists of document columns (versioned, system, etc.)
    /// </summary>
    public class DocumentColumnLists
    {
        #region "Variables"

        /// <summary>
        /// Columns required for the select nodes operation to perform successfully in any case.
        /// </summary>
        public const string SELECTNODES_REQUIRED_COLUMNS = "NodeID, NodeSiteID, DocumentID, DocumentCulture, NodeACLID, NodeLinkedNodeID, ClassName";


        /// <summary>
        /// Columns required for the select tree operation to perform successfully in any case.
        /// </summary>
        public const string SELECTTREE_REQUIRED_COLUMNS = "NodeID, NodeAliasPath, NodeName, NodeParentID, NodeClassID, NodeLevel, NodeHasChildren, NodeLinkedNodeID, DocumentID, DocumentName, DocumentCulture, DocumentModifiedWhen, DocumentMenuRedirectUrl, ClassName, DocumentType, DocumentIsArchived, DocumentPublishedVersionHistoryID, DocumentWorkflowStepID, DocumentCheckedOutByUserID, DocumentCheckedOutVersionHistoryID, DocumentCanBePublished, DocumentPublishFrom, DocumentPublishTo";


        /// <summary>
        /// Columns required for the security check operation to perform successfully in any case.
        /// </summary>
        public const string SECURITYCHECK_REQUIRED_COLUMNS = "ClassName, NodeACLID, NodeSiteID, NodeOwner, DocumentCulture, NodeParentID, DocumentID, NodeID";


        /// <summary>
        /// Columns required for the get documents operation to perform successfully in any case.
        /// </summary>
        public const string GETDOCUMENTS_REQUIRED_COLUMNS = SELECTNODES_REQUIRED_COLUMNS + ", DocumentCheckedOutVersionHistoryID";


        /// <summary>
        /// Columns required for getting the published information
        /// </summary>
        public const string GETPUBLISHED_REQUIRED_COLUMNS = "DocumentCanBePublished, DocumentPublishFrom, DocumentPublishTo";


        /// <summary>
        /// List of columns which must be present in order to evaluate if the document can be published
        /// </summary>
        public static readonly string[] CANBEPUBLISHED_REQUIRED_COLUMNS = { "DocumentWorkflowStepID", "DocumentIsArchived", "DocumentCheckedOutVersionHistoryID", "DocumentPublishedVersionHistoryID" };


        /// <summary>
        /// Non-versioned coupled columns list.
        /// </summary>
        private static readonly Lazy<ISet<string>> mNonVersionedCoupledColumns = new Lazy<ISet<string>>(() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "ECommerce.SKU.SKUAvailableItems",
                "ECommerce.SKU.SKUTrackInventory",
                "ECommerce.SKU.SKUGUID"
            });


        /// <summary>
        /// Set of the non-versioned document (CMS_Document) columns
        /// </summary>
        private static readonly Lazy<ISet<string>> mNonVersionedDocumentColumns = new Lazy<ISet<string>>(() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "DocumentURLPath",
                "DocumentUseNamePathForURLPath",
                "DocumentUseCustomExtensions",
                "DocumentPageTemplateID",
                "DocumentStylesheetID",
                "DocumentInheritsStylesheet",
                "DocumentCustomData",
                "DocumentExtensions",
                "DocumentTrackConversionName",
                "DocumentConversionValue",
                "DocumentTags",
                "DocumentTagGroupID",
                "DocumentCanBePublished",
                "DocumentLogVisitActivity"
            });


        /// <summary>
        /// Set of the system node (CMS_Tree) columns
        /// </summary>
        private static readonly Lazy<ISet<string>> mSystemNodeColumns = new Lazy<ISet<string>>(() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "NodeID",
                "NodeSiteID",
                "NodeParentID",
                "NodeACLID",
                "NodeIsACLOwner",
                "NodeAliasPath",
                "NodeClassID",
                "NodeLevel",
                "NodeGUID",
                "NodeOrder",
                "NodeSKUID",
                "NodeHasChildren",
                "NodeHasLinks",
                "NodeLinkedNodeID",
                "NodeOwner",
                "NodeGroupID"
            });


        /// <summary>
        /// Set of the system document (CMS_Document) columns
        /// </summary>
        private static readonly Lazy<ISet<string>> mSystemDocumentColumns = new Lazy<ISet<string>>(() => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "DocumentID",
                "DocumentGUID",
                "DocumentModifiedWhen",
                "DocumentModifiedByUserID",
                "DocumentForeignKeyValue",
                "DocumentCreatedByUserID",
                "DocumentCreatedWhen",
                "DocumentCheckedOutByUserID",
                "DocumentCheckedOutWhen",
                "DocumentCheckedOutversionHistoryID",
                "DocumentPublishedVersionHistoryID",
                "DocumentWorkflowStepID",
                "DocumentCulture",
                "DocumentNodeID",
                "DocumentNamePath",
                "DocumentWildcardRule",
                "DocumentRatingValue",
                "DocumentRatings",
                "DocumentPriority",
                "DocumentGroupWebParts",
                "DocumentCheckedOutAutomatically",
                "DocumentIsArchived",
                "DocumentLastVersionNumber",
                "DocumentHash",
                "DocumentWorkflowCycleGUID",
                "DocumentWorkflowActionStatus",
                "DocumentIsWaitingForTranslation",
                "DocumentLastPublished"
            });

        #endregion


        #region "Methods"

        /// <summary>
        /// List of the non-versioned coupled document columns.
        /// </summary>
        internal static ISet<string> NonVersionedCoupledColumns
        {
            get
            {
                return mNonVersionedCoupledColumns.Value;
            }
        }


        /// <summary>
        /// Set of the system node (CMS_Tree) columns
        /// </summary>
        internal static ISet<string> SystemNodeColumns
        {
            get
            {
                return mSystemNodeColumns.Value;
            }
        }


        /// <summary>
        /// Set of the system document (CMS_Document) columns
        /// </summary>
        internal static ISet<string> SystemDocumentColumns
        {
            get
            {
                return mSystemDocumentColumns.Value;
            }
        }


        /// <summary>
        /// Set of the non-versioned document (CMS_Document) columns
        /// </summary>
        internal static ISet<string> NonVersionedDocumentColumns
        {
            get
            {
                return mNonVersionedDocumentColumns.Value;
            }
        }

        #endregion
    }
}
