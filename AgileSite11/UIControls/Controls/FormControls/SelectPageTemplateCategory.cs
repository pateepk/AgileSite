using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the page template category selection.
    /// </summary>
    [ToolboxData("<{0}:SelectPageTemplateCategory runat=server></{0}:SelectPageTemplateCategory>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectPageTemplateCategory : FormControl
    {
        private CMSDropDownList mDropDown;

        #region "Public properties"

        /// <summary>
        /// Selected category ID
        /// </summary>
        public int SelectedCategoryID
        {
            get
            {
                return ValidationHelper.GetInteger(Value, 0);
            }
            set
            {
                Value = value;
            }
        }


        /// <summary>
        /// Enables or disables root category in drop down list.
        /// </summary>
        public bool ShowRoot
        {
            get
            {
                return GetValue("ShowRoot", false);
            }
            set
            {
                SetValue("ShowRoot", value);
            }
        }


        /// <summary>
        /// Root value
        /// </summary>
        public string RootValue
        {
            get
            {
                return GetValue("RootValue", String.Empty);
            }
            set
            {
                SetValue("RootValue", value);
            }
        }


        /// <summary>
        /// Gets or sets WHERE condition.
        /// </summary>
        private string WhereCondition
        {
            get
            {
                return GetValue("WhereCondition", String.Empty);
            }
            set
            {
                SetValue("WhereCondition", value);
            }
        }


        /// <summary>
        /// Path of root
        /// </summary>
        public string StartingPath
        {
            get
            {
                return GetValue("StartingPath", String.Empty);
            }
            set
            {
                SetValue("StartingPath", value);
            }
        }


        /// <summary>
        /// Enables or disables hiding of empty categories in drop down list.
        /// </summary>
        public bool ShowEmptyCategories
        {
            get
            {
                return GetValue("ShowEmptyCategories", false);
            }
            set
            {
                SetValue("ShowEmptyCategories", value);
            }
        }


        /// <summary>
        /// If enabled, page templates are shown in dropdown list.
        /// </summary>
        public bool ShowTemplates
        {
            get
            {
                return GetValue("ShowObjects", false);
            }
            set
            {
                SetValue("ShowObjects", value);
            }
        }


        /// <summary>
        /// Enables or disables ad-hoc category in dropdown.
        /// </summary>
        public bool ShowAdHocCategory
        {
            get;
            set;
        }


        /// <summary>
        /// If enabled, category can be selected, otherwise categories are disabled.
        /// </summary>
        public bool EnableCategorySelection
        {
            get;
            set;
        }


        /// <summary>
        /// Gets inner drop down list control.
        /// </summary>
        public CMSDropDownList CMSDropDownList
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref mDropDown);
            }
        }

        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectPageTemplateCategory()
        {
            EnableCategorySelection = false;
            FormControlName = "PageTemplateCategorySelector";
        }


        /// <summary>
        /// OnLoad event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!ShowAdHocCategory)
            {
                WhereCondition = SqlHelper.AddWhereCondition(WhereCondition, "[CategoryName] != 'AdHoc' AND [CategoryName] != 'AdhocUI'");
            }
        }
    }
}