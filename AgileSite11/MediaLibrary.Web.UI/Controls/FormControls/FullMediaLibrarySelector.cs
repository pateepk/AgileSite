using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;
using CMS.UIControls;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Form control for the media library selection.
    /// </summary>
    [ToolboxData("<{0}:FullMediaLibrarySelector runat=server></{0}:FullMediaLibrarySelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FullMediaLibrarySelector : FormControl
    {
        #region "Private variables"

        private UniSelector mUniSelector = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the site libraries should belongs to.
        /// </summary>
        public int SiteID
        {
            get
            {
                return GetValue<int>("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// ID of the group libraries should belongs to.
        /// </summary>
        public int GroupID
        {
            get
            {
                return GetValue<int>("GroupID", 0);
            }
            set
            {
                SetValue("GroupID", value);
            }
        }


        /// <summary>
        /// Gets or sets the value which determines, whether to add none item record to the dropdownlist.
        /// </summary>
        public bool AddNoneItemsRecord
        {
            get
            {
                return CurrentSelector.AllowEmpty;
            }
            set
            {
                CurrentSelector.AllowEmpty = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines, whether to add current media library record to the dropdownlist.
        /// </summary>
        public bool AddCurrentLibraryRecord
        {
            get
            {
                return GetValue<bool>("AddCurrentLibraryRecord", true);
            }
            set
            {
                SetValue("AddCurrentLibraryRecord", value);
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

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public FullMediaLibrarySelector()
        {
            FormControlName = "FullMediaLibrarySelector";
        }
    }
}