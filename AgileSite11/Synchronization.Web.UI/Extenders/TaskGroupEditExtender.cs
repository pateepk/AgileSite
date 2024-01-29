using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMS.Synchronization.Web.UI
{
    /// <summary>
    /// Extends edit of StagingTaskGroupInfo
    /// </summary>
    public class TaskGroupEditExtender : ControlExtender<UIForm>
    {
        /// <summary>
        /// Init the extender, attach events that communicates with StagingTaskGroupMenu
        /// </summary>
        public override void OnInit()
        {
            // Checks whether task group was saved, it does not matter if in New or Edit mode, 
            // because from New tab we are redirected to Edit, then register js to update StagingTaskGroupMenu
            bool saved = QueryHelper.GetBoolean("saved", false);

            Control.PreRender += (s, e) =>
            {
                TaskGroupInfo groupInfo = (TaskGroupInfo)Control.EditedObject;

                if (groupInfo != null && saved)
                { 
                    // Register java script to reload StagingTaskGroupMenu
                    ScriptHelper.RegisterModule(Control, "CMS.Staging/StagingTaskGroupEditExtender", new
                    {
                        taskGroupID = groupInfo.TaskGroupID,
                        taskGroupName = groupInfo.TaskGroupCodeName
                    });
                }
            };
        }
    }
}
