using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view user avatar.
    /// </summary>
    [ToolboxData("<{0}:ViewUserAvatar runat=server></{0}:ViewUserAvatar>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewUserAvatar : FormControl
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
        public ViewUserAvatar()
        {
            FormControlName = "ViewUserAvatar";
        }
    }
}