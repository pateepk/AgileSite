using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the banner type selection.
    /// </summary>
    [ToolboxData("<{0}:BannerTypeSelector runat=server></{0}:BannerTypeSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BannerTypeSelector : FormControl
    {
        #region "Private variables"

        private CMSDropDownList drpList = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Underlying drop down list.
        /// </summary>
        public CMSDropDownList DropDownList
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref drpList);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public BannerTypeSelector()
        {
            FormControlName = "BannerTypeSelector";
        }
    }
}