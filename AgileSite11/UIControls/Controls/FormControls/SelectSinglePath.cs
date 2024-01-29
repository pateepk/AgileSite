using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the path selection.
    /// </summary>
    [ToolboxData("<{0}:SelectSinglePath runat=server></{0}:SelectSinglePath>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectSinglePath : FormControl
    {
        #region "Private variables"

        private CMSTextBox txtPath;
        private CMSTextBox txtNodeId;
        private UniButton btnSelectPath;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether control should use postback after selection
        /// </summary>
        public bool UpdateControlAfterSelection
        {
            get
            {
                return GetValue("UpdateControlAfterSelection", false);
            }
            set
            {
                SetValue("UpdateControlAfterSelection", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether text input is enabled
        /// </summary>
        public bool DisableTextInput
        {
            get
            {
                return GetValue("DisableTextInput", false);
            }
            set
            {
                SetValue("DisableTextInput", value);
            }
        }
                       

        /// <summary>
        /// Gets the configuration for Copy and Move dialog.
        /// </summary>
        public DialogConfiguration Config
        {
            get
            {
                return (DialogConfiguration)GetValue("DialogConfiguration");
            }
        }


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
        /// Gets the select path button.
        /// </summary>
        public UniButton SelectButton
        {
            get
            {
                return EnsureChildControl(ref btnSelectPath);
            }
        }
                      

        /// <summary>
        /// Gets selected node id.
        /// </summary>
        public int NodeId
        {
            get
            {
                return ValidationHelper.GetInteger(TxtNodeId.Text, 0);
            }
        }


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
        /// Gets or sets the ID of the site from which the path is selected.
        /// </summary>
        public int SiteID
        {
            get
            {
                return GetValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
                if (value > 0)
                {
                    Config.ContentSites = AvailableSitesEnum.OnlySingleSite;
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(value);
                    if (si != null)
                    {
                        Config.ContentSelectedSite = si.SiteName;
                    }
                }
                else
                {
                    Config.ContentSites = AvailableSitesEnum.All;
                }
            }
        }

               
        /// <summary>
        /// Determines whether to allow setting permissions for selected path.
        /// </summary>
        public bool AllowSetPermissions
        {
            get
            {
                return GetValue("AllowSetPermissions", false);
            }
            set
            {
                SetValue("AllowSetPermissions", value);
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets the path text box.
        /// </summary>
        private CMSTextBox TxtNodeId
        {
            get
            {
                return txtNodeId ?? (txtNodeId = (CMSTextBox)ControlsHelper.GetLastChildControl(EditingControl, typeof(CMSTextBox)));
            }
        }

        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectSinglePath()
        {
            FormControlName = "SelectSinglePath";
        }
    }
}