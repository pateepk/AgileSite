using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

namespace CMS.Synchronization.Web.UI
{
    /// <summary>
    /// Extends staging task group listing so it can communicate with StagingTaskGroupMenu
    /// </summary>
    public class TaskGroupListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Inits extender that will register javascript module
        /// </summary>
        public override void OnInit()
        {
            Control.OnAction += (actionName, actionArgument) =>
            {
                int deletedTaskGroupID = ValidationHelper.GetInteger(actionArgument, 0);
                
                // Register javascript to reload application
                ScriptHelper.RegisterModule(Control, "CMS.Staging/StagingTaskGroupListExtender", new
                {
                    TaskGroupDeleted = deletedTaskGroupID > 0,
                    TaskGroupDeletedID = deletedTaskGroupID
                });
            };
        }
    }
}
