using System.Data;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.Ecommerce
{
    internal class SKUSearchIndexer : DocumentSearchIndexer
    {
        /// <summary>
        /// Name of index type for processing SKU related documents.
        /// </summary>
        internal const string SKU_DOCUMENTS_INDEX = "_skudocuments";


        /// <summary>
        /// Update search index of nodes depending on the SKU.
        /// </summary>
        /// <param name="sti">Search task info object.</param>
        protected override void ExecuteUpdateTask(SearchTaskInfo sti)
        {
            // Prepare variables for searching for depending nodes
            TreeProvider tp = new TreeProvider();
            string where = "(SKUID = " + sti.SearchTaskRelatedObjectID + ")";

            // Get all nodes
            DataSet ds = tp.SelectNodes(TreeProvider.ALL_SITES, TreeProvider.ALL_DOCUMENTS, TreeProvider.ALL_CULTURES, false, null, where, null, -1, false, 0, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS + ", DocumentCheckedOutVersionHistoryID, DocumentPublishedVersionHistoryID");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get node
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    TreeNode node = TreeNode.New(dr["ClassName"].ToString(), dr);
                    if ((node != null) && node.PublishedVersionExists)
                    {
                        TreeNodeDocumentUpdate(node.DocumentID, sti);
                    }
                }
            }
        }
    }
}
