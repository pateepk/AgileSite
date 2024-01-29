using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the site selection.
    /// </summary>
    [ToolboxData("<{0}:SiteOrGlobalSelector runat=server></{0}:SiteOrGlobalSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SiteOrGlobalSelector : FormControl
    {
        #region "Variables"

        private CMSDropDownList mSelector;
        private UniSelector mUniSelector;
        private CMSUpdatePanel mUpdatePanel;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns selector control internally used for selecting sites.
        /// </summary>
        public CMSDropDownList Selector
        {
            get
            {
                return EnsureChildControl(ref mSelector);
            }
        }


        /// <summary>
        /// Returns uni-selector control internally used for selecting sites.
        /// </summary>
        public UniSelector UniSelector
        {
            get
            {
                return EnsureChildControl(ref mUniSelector);
            }
        }


        /// <summary>
        /// Returns inner update panel.
        /// </summary>
        public CMSUpdatePanel UpdatePanel
        {
            get
            {
                return EnsureChildControl(ref mUpdatePanel);
            }
        }


        /// <summary>
        /// ID of the selected site.
        /// </summary>
        public int SiteID
        {
            get
            {
                // Make sure data is loaded
                UniSelector.Reload(false);
                return ValidationHelper.GetInteger(Value, SpecialFieldValue.GLOBAL_AND_SITE);
            }
            set
            {
                Value = value;
            }
        }


        /// <summary>
        /// Gets or sets visibility to 'global and this site' value.
        /// </summary>
        public bool ShowSiteAndGlobal
        {
            get
            {
                return GetValue("ShowSiteAndGlobal", true);
            }
            set
            {
                SetValue("ShowSiteAndGlobal", value);
            }
        }


        /// <summary>
        /// Object type to filter using this site selector.
        /// </summary>
        public string TargetObjectType
        {
            get
            {
                return GetValue<string>("TargetObjectType", null);
            }
            set
            {
                SetValue("TargetObjectType", value);
            }
        }


        /// <summary>
        /// If true, full post-back is called when site changed
        /// </summary>
        public bool PostbackOnDropDownChange
        {
            get
            {
                return GetValue("PostbackOnDropDownChange", false);
            }
            set
            {
                SetValue("PostbackOnDropDownChange", value);
            }
        }


        /// <summary>
        /// Dropdown CSS class.
        /// </summary>
        public string DropDownCSSClass
        {
            get
            {
                return CssClass;
            }
            set
            {
                CssClass = value;
            }
        }
        

        /// <summary>
        /// Control CSS class.
        /// </summary>
        public override string CssClass
        {
            get
            {
                return Selector.CssClass;
            }
            set
            {
                Selector.CssClass = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public SiteOrGlobalSelector()
        {
            FormControlName = "Site_or_global_selector";
        }


        /// <summary>
        /// Init event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            UpdatePanel.RenderMode = UpdatePanelRenderMode.Inline;
            AutoPostBack = true;
        }


        /// <summary>
        /// Gets control where condition
        /// </summary>
        /// <returns></returns>
        public string GetWhereCondition()
        {
            return EditingControl.GetWhereCondition();
        }

        #endregion
    }
}