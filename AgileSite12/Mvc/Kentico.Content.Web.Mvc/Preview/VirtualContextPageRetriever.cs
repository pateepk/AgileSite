using System;
using System.Linq;

using CMS.DocumentEngine;
using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Retrieves page based on virtual context parameters.
    /// </summary>
    internal sealed class VirtualContextPageRetriever : IVirtualContextPageRetriever
    {
        /// <summary>
        /// Retrieves page by workflow cycle GUID stored in virtual context.
        /// </summary>
        /// <remarks>Retrieved page is stored into cache.</remarks>
        public TreeNode Retrieve()
        {
            var workflowCycleGuid = ValidationHelper.GetGuid(VirtualContext.GetItem(VirtualContext.PARAM_WF_GUID), Guid.Empty);

            return CacheHelper.Cache(
                cs =>
                {
                    var page = GetPageByWorkflowCycleGuid(workflowCycleGuid);
                    if (page == null)
                    {
                        return null;
                    }

                    cs.CacheDependency = CacheHelper.GetCacheDependency(new[]
                    {
                        CacheHelper.GetCacheItemName(null, "documentid", page.DocumentID)
                    });

                    return page;
                },
                new CacheSettings(10, "PageBuilder", "VirtualContextPage", workflowCycleGuid));
        }


        private static TreeNode GetPageByWorkflowCycleGuid(Guid workflowCycleGuid)
        {
            return new DocumentQuery()
                   .WhereEquals("DocumentWorkflowCycleGUID", workflowCycleGuid)
                   .FirstOrDefault();
        }
    }
}
