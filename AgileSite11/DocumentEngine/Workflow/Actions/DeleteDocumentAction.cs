using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for document deletion action
    /// </summary>
    public class DeleteDocumentAction : BaseDocumentAction
    {
        #region "Variables"

        private TreeNode mAlternatingNode;

        #endregion


        #region "Parameters"

        /// <summary>
        /// Alternating alias path
        /// </summary>
        protected virtual string AlternatingAliasPath
        {
            get
            {
                return GetResolvedParameter<string>("AlternatingAliasPath", null);
            }
        }


        /// <summary>
        /// Alternating node
        /// </summary>
        protected virtual TreeNode AlternatingNode
        {
            get
            {
                if (mAlternatingNode == null)
                {
                    if (!string.IsNullOrEmpty(AlternatingAliasPath))
                    {
                        mAlternatingNode = Node.TreeProvider.SelectNodes()
                            .OnSite(SourceNode.NodeSiteName)
                            .Path(AlternatingAliasPath)
                            .CombineWithAnyCulture()
                            .TopN(1)
                            .WithCoupledColumns()
                            .FirstObject;
                    }
                }

                return mAlternatingNode;
            }
        }


        /// <summary>
        /// Indicates if aliases should be copied.
        /// </summary>
        protected virtual bool AlternatingDocumentCopyAllPaths
        {
            get
            {
                return GetResolvedParameter("AlternatingDocumentCopyAllPaths", false);
            }
        }


        /// <summary>
        /// Indicates if child documents should be alternated.
        /// </summary>
        protected virtual bool AlternatingDocumentChildren
        {
            get
            {
                return GetResolvedParameter("AlternatingDocumentChildren", false);
            }
        }


        /// <summary>
        /// Indicates if all cultures should be deleted.
        /// </summary>
        protected virtual bool DeleteAllCultures
        {
            get
            {
                return GetResolvedParameter("DeleteAllCultures", false);
            }
        }


        /// <summary>
        /// Indicates if the document should be destroyed.
        /// </summary>
        protected virtual bool DestroyDocument
        {
            get
            {
                return GetResolvedParameter("DestroyDocument", false);
            }
        }
        
        #endregion


        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            if (DeleteAllCultures)
            {
                SourceCulture = TreeProvider.ALL_CULTURES;
                SourceCombineWithDefaultCulture = true;
            }
            
            // Delete document
            if (SourceNode != null)
            {
                if (!String.IsNullOrEmpty(AlternatingAliasPath) && (AlternatingNode == null))
                {
                    // Cancel Delete action if specified alternating node doesn't exist
                    throw new Exception("[DeleteDocumentAction.Execute]: The page selected as an alternative for the deleted page doesn't exist on the current site.");
                }

                // Prepare settings for delete
                DeleteDocumentSettings settings = new DeleteDocumentSettings(SourceNode, DeleteAllCultures, DestroyDocument, Node.TreeProvider);
                
                // Add additional settings if alternating document is specified
                if (AlternatingNode != null)
                {
                    settings.AlternatingDocument = AlternatingNode;
                    settings.AlternatingDocumentCopyAllPaths = AlternatingDocumentCopyAllPaths;
                    settings.AlternatingDocumentMaxLevel = AlternatingDocumentChildren ? -1 : SourceNode.NodeLevel;
                }
                
                DocumentHelper.DeleteDocument(settings);

                // Refresh node to ensure that if current document deleted, instance is reloaded
                RefreshNode();
            }
        }
    }
}
