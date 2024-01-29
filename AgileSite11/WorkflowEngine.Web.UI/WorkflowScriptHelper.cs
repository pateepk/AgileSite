using System;
using System.Web.UI;

using CMS.Base.Web.UI;

namespace CMS.WorkflowEngine.Web.UI
{
    /// <summary>
    /// Class for Workflow script helpers.
    /// </summary>
    public static class WorkflowScriptHelper
    {
        /// <summary>
        /// Refreshes designer from dialog edit
        /// </summary>
        public static void RefreshDesignerFromDialog(Page page, int workflowStepId, string graphName)
        {
            if (workflowStepId <= 0 || String.IsNullOrEmpty(graphName) || page == null)
            {
                return;
            }

            string refreshScript = @"
function refreshDesigner(graphName, stepId) {
    if(parent.wopener !== undefined) {
        var graph = parent.wopener[graphName];
        if(graph) {
            graph.refreshNode(stepId);
        }
    }
}
refreshDesigner('" + graphName + "'," + workflowStepId + ")";

            ScriptHelper.RegisterStartupScript(page, typeof(string), "WorkflowDesignerRefresh", ScriptHelper.GetScript(refreshScript));
        }
    }
}
