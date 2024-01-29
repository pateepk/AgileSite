using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the roles selection.
    /// </summary>
    [ToolboxData("<{0}:SelectRoles runat=server></{0}:SelectRoles>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectRoles : FormControl
    {
        #region "Private variables"

        private UniSelector mUniSelector = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Determines whether use display names instead of codenames.
        /// </summary>
        public bool UseFriendlyNames
        {
            get
            {
                return !UniSelector.AllowEditTextBox;
            }
            set
            {
                UniSelector.AllowEditTextBox = !value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string ValueDisplayName
        {
            get
            {
                return UniSelector.ValueDisplayName;
            }
        }


        /// <summary>
        /// Indicates if role selector allow empty selection.
        /// </summary>
        public bool AllowEmpty
        {
            get
            {
                return UniSelector.AllowEmpty;
            }
            set
            {
                UniSelector.AllowEmpty = value;
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets the current UniSelector instance.
        /// </summary>
        private UniSelector UniSelector
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
        public SelectRoles()
        {
            FormControlName = "RoleCheckboxSelector";
        }
    }
}