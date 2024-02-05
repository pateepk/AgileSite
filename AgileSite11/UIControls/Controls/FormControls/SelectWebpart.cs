using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the web part category selection.
    /// </summary>
    [ToolboxData("<{0}:SelectWebpart runat=server></{0}:SelectWebpart>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectWebpart : FormControl
    {
        #region "Private variables"

        private CMSDropDownList mDropDown = null;
        private bool mShowInheritedWebparts = true;
        private bool mEnableCategorySelection = false;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Enables or disables root category in drop down list.
        /// </summary>
        public bool ShowRoot
        {
            get
            {
                return GetValue<bool>("ShowRoot", false);
            }
            set
            {
                SetValue("ShowRoot", value);
            }
        }
                

        /// <summary>
        /// Shows or hide inherited web parts.
        /// </summary>
        public bool ShowInheritedWebparts
        {
            get
            {
                return mShowInheritedWebparts;
            }
            set
            {
                mShowInheritedWebparts = value;
            }
        }


        /// <summary>
        /// Enables or disables hiding of empty categories in drop down list.
        /// </summary>
        public bool ShowEmptyCategories
        {
            get
            {
                return GetValue<bool>("ShowEmptyCategories", false);
            }
            set
            {
                SetValue("ShowEmptyCategories", value);
            }
        }


        /// <summary>
        /// If enabled, web parts are shown in drop down list.
        /// </summary>
        public bool ShowWebparts
        {
            get
            {
                return GetValue<bool>("ShowObjects", false);
            }
            set
            {
                SetValue("ShowObjects", value);
            }
        }
        

        /// <summary>
        /// If enabled, category can be selected, otherwise categories are disabled.
        /// </summary>
        public bool EnableCategorySelection
        {
            get
            {
                return mEnableCategorySelection;
            }
            set
            {
                mEnableCategorySelection = value;
            }
        }


        /// <summary>
        /// Gets the drop down list control.
        /// </summary>
        public CMSDropDownList DropDownListControl
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref mDropDown);
            }
        }


        /// <summary>
        /// Root category path
        /// </summary>
        public string RootPath
        {
            get
            {
                return GetValue<string>("StartingPath", String.Empty);
            }
            set
            {
                SetValue("StartingPath", value);
            }
        }


        /// <summary>
        /// Gets or sets WHERE condition.
        /// </summary>
        private string ObjectWhereCondition
        {
            get
            {
                return GetValue<string>("ObjectWhereCondition", String.Empty);
            }
            set
            {
                SetValue("ObjectWhereCondition", value);
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectWebpart()
        {
            FormControlName = "WebPartCategorySelector";
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnLoad event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ShowWebparts && !ShowInheritedWebparts)
            {
                ObjectWhereCondition = SqlHelper.AddWhereCondition(ObjectWhereCondition, "[WebPartParentID] IS NULL");
                ShowEmptyCategories = false;
            }
        }

        #endregion
    }
}