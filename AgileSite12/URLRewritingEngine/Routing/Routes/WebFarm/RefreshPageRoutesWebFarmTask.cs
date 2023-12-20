using CMS.Core;
using CMS.DocumentEngine;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Web farm task used to refresh the route table.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    internal class RefreshPageRoutesWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the page identifier which routes will be refreshed.
        /// </summary>
        public int DocumentID { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CMSDocumentRouteHelper.RefreshRoutes(TreeNode, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            var node = DocumentHelper.GetDocument(DocumentID, tree: null);

            if (node != null)
            {
                CMSDocumentRouteHelper.RefreshRoutes(node, logWebFarmTask: false);
            }
        }
    }
}
