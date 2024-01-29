using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the site selection.
    /// </summary>
    [ToolboxData("<{0}:SelectSite runat=server></{0}:SelectSite>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectSite : FormControl
    {
        #region "Public properties"

        /// <summary>
        /// Enables or disables multiple site selection.
        /// </summary>
        public bool AllowMultipleSelection
        {
            get
            {
                SelectionModeEnum mode = (SelectionModeEnum)ValidationHelper.GetInteger(GetValue("SelectionMode"), 1);
                return (mode == SelectionModeEnum.MultipleTextBox);
            }
            set
            {
                SetValue("SelectionMode", value ? (int)SelectionModeEnum.MultipleTextBox : (int)SelectionModeEnum.SingleTextBox);
            }
        }


        /// <summary>
        /// Enables or disables (all) item in selector.
        /// </summary>
        public bool AllowAll
        {
            get
            {
                return GetValue("AllowAll", false);
            }
            set
            {
                SetValue("AllowAll", value);
            }
        }
                        

        /// <summary>
        /// Enables or disables (empty) item in selector.
        /// </summary>
        public bool AllowEmpty
        {
            get
            {
                return GetValue("AllowEmpty", false);
            }
            set
            {
                SetValue("AllowEmpty", value);
            }
        }


        /// <summary>
        /// Enables or disables (global) item in selector. Uses uniSelector's SpecialFields property.
        /// </summary>
        public bool AllowGlobal
        {
            get
            {
                return GetValue("AllowGlobal", false);
            }
            set
            {
                SetValue("AllowGlobal", value);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public SelectSite()
        {
            FormControlName = "SelectSite";
            AllowEmpty = false;
            AllowMultipleSelection = false;
        }


        #region "Methods"

        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set resource strings for current mode
            SetValue("ResourcePrefix", AllowMultipleSelection ? "sitesselect" : "siteselect");
            SetValue("UseCodeNameForSelection", true);
        }

        #endregion
    }
}