using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Relationships;
using CMS.Taxonomy;
using CMS.WorkflowEngine;
using CMS.DataEngine.Query;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides the repository of the objects connected to a specific document
    /// </summary>
    internal class DocumentConnectedObjects
    {
        #region "Variables"

        /// <summary>
        /// Connected objects.
        /// </summary>
        private InfoObjectRepository mConnectedObjects;

        /// <summary>
        /// Connected documents.
        /// </summary>
        private TreeNodeRepository mConnectedDocuments;

        /// <summary>
        /// Related documents.
        /// </summary>
        private TreeNodeRepository mRelatedDocuments;


        /// <summary>
        /// Collection of all attachments
        /// </summary>
        private DocumentAttachmentCollection mAllAttachments;


        /// <summary>
        /// Collection of unsorted attachments
        /// </summary>
        private DocumentAttachmentCollection mAttachments;


        // Object for locking this instance context
        private readonly object lockObject = new object();
               
        #endregion


        #region "Properties"

        /// <summary>
        /// Node data
        /// </summary>
        private TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Attachments of the document
        /// </summary>
        public DocumentAttachmentCollection AllAttachments
        {
            get
            {
                return mAllAttachments ?? (mAllAttachments = new DocumentAttachmentCollection(Node));
            }
        }

        
        /// <summary>
        /// Unsorted attachments of the document
        /// </summary>
        public DocumentAttachmentCollection Attachments
        {
            get
            {
                return mAttachments ?? (mAttachments = new UnsortedDocumentAttachmentCollection(Node));
            }
        }


        /// <summary>
        /// Related documents.
        /// </summary>
        public TreeNodeRepository RelatedDocuments
        {
            get
            {
                if (mRelatedDocuments == null)
                {
                    lock (lockObject)
                    {
                        if (mRelatedDocuments == null)
                        {
                            // Prepare the repository
                            TreeNodeRepository repository = GetDocumentRepository();

                            // Provide through dynamic handler
                            repository.OnLoadCollection += RelatedDocuments_OnLoadCollection;
                            repository.GetDynamicNames = GetRelationshipNames;

                            mRelatedDocuments = repository;
                        }
                    }
                }

                return mRelatedDocuments;
            }
        }


        /// <summary>
        /// Connected objects.
        /// </summary>
        public InfoObjectRepository ConnectedObjects
        {
            get
            {
                if (mConnectedObjects == null)
                {
                    lock (lockObject)
                    {
                        if (mConnectedObjects == null)
                        {
                            // Prepare the repository
                            InfoObjectRepository repository = new InfoObjectRepository((ICMSStorage)Node.Generalized);

                            // Register default connected objects
                            RegisterConnectedObjects(repository);

                            mConnectedObjects = repository;
                        }
                    }
                }

                return mConnectedObjects;
            }
        }


        /// <summary>
        /// Collection of document tags.
        /// </summary>
        public IInfoObjectCollection Tags
        {
            get
            {
                return ConnectedObjects["Tags"];
            }
        }


        /// <summary>
        /// Collection of document categories.
        /// </summary>
        public IInfoObjectCollection Categories
        {
            get
            {
                return ConnectedObjects["Categories"];
            }
        }


        /// <summary>
        /// Collection of the version history of the document
        /// </summary>
        public IInfoObjectCollection VersionHistory
        {
            get
            {
                return ConnectedObjects["VersionHistory"];
            }
        }


        /// <summary>
        /// Collection of the attachment history of the document
        /// </summary>
        public IInfoObjectCollection AttachmentHistory
        {
            get
            {
                return ConnectedObjects["AttachmentHistory"];
            }
        }


        /// <summary>
        /// Collection of the workflow history of the document
        /// </summary>
        public IInfoObjectCollection WorkflowHistory
        {
            get
            {
                return ConnectedObjects["WorkflowHistory"];
            }
        }


        /// <summary>
        /// Collection of the document aliases.
        /// </summary>
        public IInfoObjectCollection Aliases
        {
            get
            {
                return ConnectedObjects["Aliases"];
            }
        }


        /// <summary>
        /// Collection of the message boards for this document
        /// </summary>
        public IInfoObjectCollection MessageBoards
        {
            get
            {
                return ConnectedObjects["MessageBoards"];
            }
        }


        /// <summary>
        /// Collection of the ad-hoc forums for this document
        /// </summary>
        public IInfoObjectCollection Forums
        {
            get
            {
                return ConnectedObjects["Forums"];
            }
        }


        /// <summary>
        /// Collection of the personalizations (widget settings) for the document
        /// </summary>
        public IInfoObjectCollection Personalizations
        {
            get
            {
                return ConnectedObjects["Personalizations"];
            }
        }

        
        /// <summary>
        /// Connected documents.
        /// </summary>
        public TreeNodeRepository ConnectedDocuments
        {
            get
            {
                if (mConnectedDocuments == null)
                {
                    lock (lockObject)
                    {
                        if (mConnectedDocuments == null)
                        {
                            // Prepare the repository
                            TreeNodeRepository repository = GetDocumentRepository();

                            // Register the default connected documents
                            RegisterConnectedDocuments(repository);

                            mConnectedDocuments = repository;
                        }
                    }
                }

                return mConnectedDocuments;
            }
        }


        /// <summary>
        /// Collection of child nodes.
        /// </summary>
        public TreeNodeCollection Children
        {
            get
            {
                return ConnectedDocuments["Children"];
            }
        }


        /// <summary>
        /// Collection of all child nodes from all levels.
        /// </summary>
        public TreeNodeCollection AllChildren
        {
            get
            {
                return ConnectedDocuments["AllChildren"];
            }
        }


        /// <summary>
        /// Collection of linked nodes, including the original.
        /// </summary>
        public TreeNodeCollection Links
        {
            get
            {
                return ConnectedDocuments["Links"];
            }
        }


        /// <summary>
        /// Collection of all culture versions of this document.
        /// </summary>
        public TreeNodeCollection CultureVersions
        {
            get
            {
                return ConnectedDocuments["CultureVersions"];
            }
        }


        /// <summary>
        /// Collection of documents on the path to the current document
        /// </summary>
        public TreeNodeCollection DocumentsOnPath
        {
            get
            {
                return ConnectedDocuments["DocumentsOnPath"];
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Document node</param>
        public DocumentConnectedObjects(TreeNode node)
        {
            Node = node;
        }                      


        /// <summary>
        /// Registers the connected objects for this document.
        /// </summary>
        /// <param name="repository">Object repository</param>
        private void RegisterConnectedObjects(InfoObjectRepository repository)
        {
            var documentId = Node.DocumentID;
            var nodeId = Node.NodeID;
            var nodeSiteId = Node.NodeSiteID;
            var versionHistoryId = Node.DocumentCheckedOutVersionHistoryID;

            // Fill in the collections of connected documents
            var settings = new InfoCollectionSettings("Tags", TagInfo.OBJECT_TYPE)
            {
                WhereCondition = new WhereCondition("TagID IN (SELECT TagID FROM CMS_DocumentTag WHERE DocumentID = " + documentId + ")"),
                OrderBy = "TagName"
            };

            var tags = repository.AddCollection(settings);
            tags.NameColumn = "TagName";

            // Categories
            settings = new InfoCollectionSettings("Categories", CategoryInfo.OBJECT_TYPE)
            {
                WhereCondition = new WhereCondition("CategoryID IN (SELECT CategoryID FROM CMS_DocumentCategory WHERE DocumentID = " + documentId + ")"),
                OrderBy = "CategoryDisplayName"
            };

            repository.AddCollection(settings);

            // Version, workflow and attachment history
            if (versionHistoryId > 0)
            {
                settings = new InfoCollectionSettings("VersionHistory", VersionHistoryInfo.OBJECT_TYPE)
                {
                    WhereCondition = new WhereCondition().WhereEquals("DocumentID", documentId),
                    OrderBy = "VersionHistoryID DESC",
                    SiteID = nodeSiteId
                };

                repository.AddCollection(settings);

                settings = new InfoCollectionSettings("WorkflowHistory", WorkflowHistoryInfo.OBJECT_TYPE)
                {
                    WhereCondition = new WhereCondition("VersionHistoryID IN (SELECT VersionHistoryID FROM CMS_VersionHistory WHERE DocumentID = " + documentId + ")"),
                    OrderBy = "WorkflowHistoryID DESC"
                };

                repository.AddCollection(settings);

                settings = new InfoCollectionSettings("AttachmentHistory", AttachmentHistoryInfo.OBJECT_TYPE)
                {
                    WhereCondition = new WhereCondition().WhereEquals("AttachmentDocumentID", documentId),
                    OrderBy = "AttachmentHistoryID DESC",
                    SiteID = nodeSiteId,
                };

                var attHistory = repository.AddCollection(settings);
                attHistory.NameColumn = null;
            }

            // Document aliases
            settings = new InfoCollectionSettings("Aliases", DocumentAliasInfo.OBJECT_TYPE)
            {
                WhereCondition = new WhereCondition().WhereEquals("AliasNodeID", nodeId),
                SiteID = nodeSiteId
            };

            repository.AddCollection(settings);

            // Message boards
            settings = new InfoCollectionSettings("MessageBoards", PredefinedObjectType.BOARD)
            {
                WhereCondition = new WhereCondition().WhereEquals("BoardDocumentID", documentId),
                SiteID = nodeSiteId
            };

            repository.AddCollection(settings);

            settings = new InfoCollectionSettings("Forums", PredefinedObjectType.FORUM)
            {
                WhereCondition = new WhereCondition().WhereEquals("ForumDocumentID", documentId),
                SiteID = nodeSiteId
            };

            repository.AddCollection(settings);

            // Personalizations
            settings = new InfoCollectionSettings("Personalizations", PersonalizationInfo.OBJECT_TYPE)
            {
                WhereCondition = new WhereCondition().WhereEquals("PersonalizationDocumentID", documentId),
                SiteID = nodeSiteId
            };

            repository.AddCollection(settings);
        }
        

        /// <summary>
        /// Registers the connected documents for this document.
        /// </summary>
        /// <param name="repository">Object repository</param>
        private void RegisterConnectedDocuments(TreeNodeRepository repository)
        {
            var siteId = Node.NodeSiteID;
            var nodeId = Node.NodeID;
            var originalNodeId = Node.OriginalNodeID;
            var documentCulture = Node.DocumentCulture;
            var tree = Node.TreeProvider;
            var isLastVersion = Node.IsLastVersion;
            var nodeAliasPath = Node.NodeAliasPath;
            var nodeClassName = Node.NodeClassName;

            // Fill in the collections of connected documents
            var childWhere = new WhereCondition();
            if (nodeId > 0)
            {
                childWhere.WhereEquals("NodeParentID", nodeId);
            }
            else
            {
                childWhere.NoResults();
            }

            var settings = new TreeNodeCollectionSettings("Children")
            {
                SiteID = siteId,
                CultureCode = documentCulture,
                CombineWithDefaultCulture = tree.CombineWithDefaultCulture, 
                WhereCondition = new WhereCondition(childWhere),
                OrderBy = "NodeOrder, DocumentName",
                SelectOnlyPublished = !isLastVersion
            };
            
            var children = repository.AddCollection(settings);
            children.NameColumn = "NodeAlias";

            var allChildWhere = new WhereCondition();
            if (nodeId <= 0)
            {
                allChildWhere.NoResults();
            }
            
            settings = new TreeNodeCollectionSettings("AllChildren")
            {
                SiteID = siteId,
                AliasPath = nodeAliasPath.TrimEnd('/') + "/%",
                CultureCode = documentCulture,
                CombineWithDefaultCulture = tree.CombineWithDefaultCulture,
                WhereCondition = new WhereCondition(allChildWhere),
                OrderBy = "NodeLevel, NodeOrder, DocumentName",
                SelectOnlyPublished = !isLastVersion
            };

            var allChildren = repository.AddCollection(settings);
            allChildren.NameColumn = "NodeAliasPath";

            // Linked documents (all including original)
            settings = new TreeNodeCollectionSettings("Links")
            {
                CultureCode = documentCulture,
                CombineWithDefaultCulture = tree.CombineWithDefaultCulture,
                ClassName = nodeClassName,
                WhereCondition = new WhereCondition().WhereEquals("NodeLinkedNodeID", originalNodeId).Or().WhereEquals("NodeID", originalNodeId),
                OrderBy = "DocumentName",
                SelectOnlyPublished = !isLastVersion
            };

            var links = repository.AddCollection(settings);
            links.NameColumn = "NodeAliasPath";

            // All culture versions
            settings = new TreeNodeCollectionSettings("CultureVersions")
            {
                SiteID = siteId,
                ClassName = nodeClassName,
                WhereCondition = new WhereCondition().WhereEquals("NodeID", nodeId),
                SelectOnlyPublished = !isLastVersion
            };

            var versions = repository.AddCollection(settings);
            versions.SelectAllData = true;
            versions.NameColumn = "DocumentCulture";

            // Documents on path to the document
            settings = new TreeNodeCollectionSettings("DocumentsOnPath")
            {
                SiteID = siteId,
                CultureCode = documentCulture,
                CombineWithDefaultCulture = true,
                WhereCondition =
                    new WhereCondition()
                        .WhereEquals("NodeAliasPath", "/")
                        .Or()
                        .WhereEquals("NodeAliasPath", nodeAliasPath)
                        .Or()
#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                        .WhereLike(nodeAliasPath.AsValue(), "NodeAliasPath + '/%'".AsExpression()),
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                OrderBy = "NodeLevel",
                SelectOnlyPublished = false
            };

            var onPath = repository.AddCollection(settings);
            onPath.NameColumn = "NodeAliasPath";
        }
        

        /// <summary>
        /// Gets the new repository for the given document.
        /// </summary>
        private TreeNodeRepository GetDocumentRepository()
        {
            var rep = new TreeNodeRepository(Node)
            {
                TreeProvider = Node.TreeProvider,
                IsLastVersion = Node.IsLastVersion
            };

            return rep;
        }

        #endregion


        #region "Connected objects helper methods"

        /// <summary>
        /// Returns relationship names used by the document
        /// </summary>
        internal IEnumerable<string> GetRelationshipNames()
        {
            var nodeId = Node.NodeID;

            // Prepare condition
            var where = 
                new WhereCondition()
                    .WhereIn(
                        "RelationshipNameID", 
                        RelationshipInfoProvider.GetRelationships()
                                                .Column("RelationshipNameID")
                                                .WhereEquals("LeftNodeID", nodeId)
                                                .Or()
                                                .WhereEquals("RightNodeID", nodeId)
                                                .GetListResult<int>()
                    );

            // Get relationship names
            var names = 
                RelationshipNameInfoProvider.GetRelationshipNames()
                    .Column("RelationshipName")
                    .Where(where)
                    .OrderBy("RelationshipName")
                    .GetListResult<string>();

            var result = new List<string>();
            result.Add("Any");
            result.AddRange(names);

            return result;
        }


        /// <summary>
        /// Handler to provide the related document collection.
        /// </summary>
        /// <param name="repository">Parent repository</param>
        /// <param name="name">Collection name</param>
        private TreeNodeCollection RelatedDocuments_OnLoadCollection(IInfoObjectRepository<TreeNodeCollection> repository, string name)
        {
            return GetRelatedDocumentsCollection(repository, name, true, true);
        }


        /// <summary>
        /// Handler to provide the related document collection.
        /// </summary>
        /// <param name="repository">Parent repository</param>
        /// <param name="name">Collection name</param>
        /// <param name="documentIsOnLeftSide">If true, the document can be on left side of the relationship</param>
        /// <param name="documentIsOnRightSide">If true, the document can be on left side of the relationship</param>
        private TreeNodeCollection GetRelatedDocumentsCollection(IInfoObjectRepository<TreeNodeCollection> repository, string name, bool documentIsOnLeftSide, bool documentIsOnRightSide)
        {
            TreeNodeCollection result = null;

            var nodeId = Node.NodeID;
            var documentCulture = Node.DocumentCulture;
            var isLastVersion = Node.IsLastVersion;

            // Prepare the where condition
            var where = TreeProvider.GetRelationshipWhereCondition(nodeId, name, documentIsOnLeftSide, documentIsOnRightSide);
            if (where != null)
            {
                // Create the collection
                result = repository.NewCollection(null);
                result.Where = new WhereCondition(where);
            }

            // Set the default properties of collection
            if (result != null)
            {
                result.Name = name;
                result.SiteName = TreeProvider.ALL_SITES;
                result.AliasPath = TreeProvider.ALL_DOCUMENTS;
                result.CultureCode = documentCulture;
                result.CombineWithDefaultCulture = true;
                result.OrderByColumns = "NodeAliasPath";
                result.MaxRelativeLevel = -1;
                result.SelectOnlyPublished = !isLastVersion;
                result.TopN = 0;
                result.Columns = null;
            }

            // Ensure empty result if not found
            if (result == null)
            {
                result = repository.NewCollection(null);
                result.MakeEmpty();
            }

            return result;
        }

        #endregion
    }
}
