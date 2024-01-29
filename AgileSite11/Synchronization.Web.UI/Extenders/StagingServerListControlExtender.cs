using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.Synchronization.Web.UI;
using CMS.UIControls;


[assembly: RegisterCustomClass("StagingServerListControlExtender", typeof(StagingServerListControlExtender))]

namespace CMS.Synchronization.Web.UI
{
    /// <summary>
    /// Permission edit control extender
    /// </summary>
    public class StagingServerListControlExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// OnInit event handler
        /// </summary>
        public override void OnInit()
        {
            Control.Load += (sender, args) =>
            {
                if (!String.IsNullOrEmpty(StagingTaskInfoProvider.ServerName))
                {
                    Control.ShowInformation(String.Format(Control.GetString("staging.currentserver"), StagingTaskInfoProvider.ServerName));
                }
            };
        }
    }
}