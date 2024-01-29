using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;
using CMS.UIControls;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Form control for the newsletter selection.
    /// </summary>
    [ToolboxData("<{0}:NewsletterSelectorSimple runat=server></{0}:NewsletterSelectorSimple>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class NewsletterSelectorSimple : FormControl
    {
        private UniSelector mUniSelector = null;


        /// <summary>
        /// Gets Value display name.
        /// </summary>
        public string ValueDisplayName
        {
            get
            {
                return CurrentSelector.ValueDisplayName;
            }
        }

               
        /// <summary>
        /// Gets ClientID of the current selector.
        /// </summary>
        public string ValueElementID
        {
            get
            {
                return CurrentSelector.ClientID;
            }
        }


        /// <summary>
        /// Gets current uni selector
        /// </summary>
        private UniSelector CurrentSelector
        {
            get
            {
                return EnsureChildControl<UniSelector>(ref mUniSelector);
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public NewsletterSelectorSimple()
        {
            FormControlName = "NewsletterSelectorSimple";
        }
    }
}