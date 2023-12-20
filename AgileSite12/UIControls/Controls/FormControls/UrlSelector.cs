using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the URL selection.
    /// </summary>
    [ToolboxData("<{0}:UrlSelector runat=server></{0}:UrlSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class UrlSelector : FormControl
    {
        #region "Private variables"

        private CMSTextBox txtPath;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets ClientID of the textbox with path.
        /// </summary>
        public string ValueElementID
        {
            get
            {
                return PathTextBox.ClientID;
            }
        }


        /// <summary>
        /// Determines whether to enable site selection or not.
        /// </summary>
        public bool EnableSiteSelection
        {
            get
            {
                return Config.ContentSites == AvailableSitesEnum.All;
            }
            set
            {
                Config.ContentSites = (value ? AvailableSitesEnum.All : AvailableSitesEnum.OnlyCurrentSite);
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets the path text box.
        /// </summary>
        public CMSTextBox PathTextBox
        {
            get
            {
                return EnsureChildControl(ref txtPath);
            }
        }


        /// <summary>
        /// Gets the configuration for Copy and Move dialog.
        /// </summary>
        private DialogConfiguration Config
        {
            get
            {
                return (DialogConfiguration)GetValue("DialogConfiguration");
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public UrlSelector()
        {
            FormControlName = "UrlSelector";
            SetValue("UseFieldInfoSettings", true);
        }
    }
}