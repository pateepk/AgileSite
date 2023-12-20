using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document handler
    /// </summary>
    public class DocumentHandler : AdvancedHandler<DocumentHandler, DocumentEventArgs>, IRecursionControlHandler<DocumentEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Tree node (document)</param>
        /// <param name="tree">Tree provider</param>
        public DocumentHandler StartEvent(TreeNode node, TreeProvider tree)
        {
            return StartEvent(node, (TreeNode)null, tree);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Tree node (document)</param>
        /// <param name="attachment">Attachment</param>
        /// <param name="tree">Tree provider</param>
        public DocumentHandler StartEvent(TreeNode node, DocumentAttachment attachment, TreeProvider tree)
        {
            var e = new DocumentEventArgs
            {
                Node = node,
                Attachment = attachment,
                TreeProvider = tree
            };

            return StartEvent(e, tree.UseCustomHandlers);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Tree node (document)</param>
        /// <param name="target">Target parent node, initialized for operations where it makes sense: Copy, Move, Insert</param>
        /// <param name="tree">Tree provider</param>
        public DocumentHandler StartEvent(TreeNode node, TreeNode target, TreeProvider tree)
        {
            return StartEvent(node, target, false, tree);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="node">Tree node (document)</param>
        /// <param name="target">Target parent node, initialized for operations where it makes sense: Copy, Move, Insert</param>
        /// <param name="includeChildNodes">If true the children are included within the operation where it makes sense: Copy</param>
        /// <param name="tree">Tree provider</param>
        public DocumentHandler StartEvent(TreeNode node, TreeNode target, bool includeChildNodes, TreeProvider tree)
        {
            var e = new DocumentEventArgs
            {
                Node = node,
                TargetParentNode = target,
                IncludeChildren = includeChildNodes,
                TreeProvider = tree
            };

            return StartEvent(e, tree.UseCustomHandlers);
        }


        /// <summary>
        /// Gets the recursion key of the document to identify recursion
        /// </summary>
        public string GetRecursionKey(DocumentEventArgs e)
        {
            if (e != null)
            {
                var obj = e.Node;
                if (obj != null)
                {
                    return obj.Generalized.GetObjectKey();
                }
            }

            return null;
        }
    }
}