using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Device layout block.
    /// </summary>
    [DebuggerDisplay("CMSDeviceLayout({ID})")]
    public class CMSDeviceLayout : CMSConditionalLayout
    {
        #region "Properties"

        /// <summary>
        /// List of device profile names separated by semicolon in which current device must belong in order to display the layout
        /// </summary>
        public override string VisibleForDeviceProfiles
        {
            get
            {
                return base.VisibleForDeviceProfiles;
            }
            set
            {
                base.VisibleForDeviceProfiles = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Device layout constructor.
        /// </summary>
        public CMSDeviceLayout()
        {
            GroupName = "DeviceProfile";
            ActiveInDesignMode = true;
        }

        #endregion
    }
}
