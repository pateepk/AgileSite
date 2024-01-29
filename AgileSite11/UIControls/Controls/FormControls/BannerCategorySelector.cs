using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the banner category selection.
    /// </summary>
    [ToolboxData("<{0}:BannerCategorySelector runat=server></{0}:BannerCategorySelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BannerCategorySelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BannerCategorySelector()
        {
            FormControlName = "BannerCategorySelector";
        }
    }
}
