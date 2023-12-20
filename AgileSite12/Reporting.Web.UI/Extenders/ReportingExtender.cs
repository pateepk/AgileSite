using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.PortalEngine.Web.UI;
using CMS.Helpers;
using CMS.Reporting.Web.UI;

[assembly: RegisterCustomClass("ReportNewControlExtender", typeof(ReportNewControlExtender))]

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// New report control extender
    /// </summary>
    public class ReportNewControlExtender : ControlExtender<UIForm>
    {
        /// <summary>
        /// Macro rules list extender
        /// </summary>
        public override void OnInit()
        {
            Control.OnBeforeValidate += new EventHandler(Control_OnBeforeValidate);
        }


        void Control_OnBeforeValidate(object sender, EventArgs e)
        {
            if (Control.EditedObject is ReportInfo)
            {
                ReportInfo ri = (ReportInfo)Control.EditedObject;

                // Get information from checkbox
                bool allowforall = ValidationHelper.GetBoolean(Control.FieldControls["ReportAccess"].Value, false);

                // Set proper integer value
                ri.ReportAccess = allowforall ? ReportAccessEnum.All : ReportAccessEnum.Authenticated;

                // Disable the item to prevent integer validation (value is boolean)
                Control.FieldControls["ReportAccess"].Enabled = false;
                Control.ProcessDisabledFields = false;
            }
        }
    }
}