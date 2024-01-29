using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Form control for view group avatar.
    /// </summary>
    [ToolboxData("<{0}:ViewGroupAvatar runat=server></{0}:ViewGroupAvatar>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewGroupAvatar : FormControl
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets max side size of avatar image.
        /// </summary>
        public int MaxSideSize
        {
            get
            {
                return GetValue("MaxSideSize", 200);
            }
            set
            {
                SetValue("MaxSideSize", value);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public ViewGroupAvatar()
        {
            FormControlName = "ViewGroupAvatar";
        }
    }
}