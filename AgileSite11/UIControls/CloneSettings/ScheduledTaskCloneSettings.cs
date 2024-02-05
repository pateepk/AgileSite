using CMS.Scheduler;
using CMS.UIControls;

[assembly: RegisterCustomCloneSettingsControl(TaskInfo.OBJECT_TYPE, typeof(ScheduledTaskCloneSettings))]

namespace CMS.UIControls
{
    internal class ScheduledTaskCloneSettings : CloneSettingsControl
    {
        public override string CloseScript
        {
            get { return "wopener.RefreshGrid(); CloseDialog();"; }
        }


        public override bool DisplayControl
        {
            get { return false; }
        }
    }
}
