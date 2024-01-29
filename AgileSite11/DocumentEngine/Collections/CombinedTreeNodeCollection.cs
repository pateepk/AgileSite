using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Combined collection of documents.
    /// </summary>
    public class CombinedTreeNodeCollection : CombinedInfoObjectCollection<TreeNodeCollection, TreeNode>
    {
        #region "Methods"

        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        public override void SubmitChanges()
        {
            // Submit all collections individually
            foreach (TreeNodeCollection collection in Collections)
            {
                collection.SubmitChanges();
            }
        }

        #endregion
    }
}