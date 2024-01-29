using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the settings category selection.
    /// </summary>
    [ToolboxData("<{0}:SelectSettingsCategory runat=server></{0}:SelectSettingsCategory>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectSettingsCategory : FormControl
    {
        #region "Variables"

        private bool mDisplayOnlyCategories = true;
        private int mCurrentCategoryId = 0;
        private bool mIsKeyEdit = false;
        private CMSDropDownList mDropDown = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets SubItemPrefix.
        /// </summary>
        public string SubItemPrefix
        {
            get
            {
                return GetValue<string>("SubItemPrefix", "--");
            }
            set
            {
                SetValue("SubItemPrefix", value);
            }
        }


        /// <summary>
        /// Gets or sets DisplayOnlyCategories property. 
        /// If set to <c>false</c> groups will be displayed as well.
        /// </summary>
        public bool DisplayOnlyCategories
        {
            get
            {
                return mDisplayOnlyCategories;
            }
            set
            {
                mDisplayOnlyCategories = value;
            }
        }


        /// <summary>
        /// If set to true selector is used in settings key edit form, so all items representing categories (not group) will be
        /// disabled and with different color.
        /// </summary>
        public bool IsKeyEdit
        {
            get
            {
                return mIsKeyEdit;
            }
            set
            {
                mIsKeyEdit = value;
            }
        }


        /// <summary>
        /// Gets the value of the selected Category in the list control, or selects the Category
        /// in the list control that contains the specified value.
        /// </summary>
        public int SelectedCategory
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
        /// Gets the collection of items in the list control.
        /// </summary>
        public ListItemCollection Items
        {
            get
            {
                return DropDownList.Items;
            }
        }


        /// <summary>
        /// Gets or sets ID of the Category which will be the root of the tree structure.
        /// </summary>
        public int RootCategoryId
        {
            get
            {
                return GetValue<int>("RootValue", 0);
            }
            set
            {
                SetValue("RootValue", value);
            }
        }


        /// <summary>
        /// Gets or sets enabled state of inclusion of RootCategory. Default false.
        /// </summary>
        public bool IncludeRootCategory
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
        /// Gets or sets ID of the Category which will be not included in the list control so its children.
        /// </summary>
        public int CurrentCategoryId
        {
            get
            {
                return mCurrentCategoryId;
            }
            set
            {
                mCurrentCategoryId = value;
            }
        }


        /// <summary>
        /// Gets or sets WHERE condition.
        /// </summary>
        public string WhereCondition
        {
            get
            {
                return GetValue<string>("WhereCondition", String.Empty);
            }
            set
            {
                SetValue("WhereCondition", value);
            }
        }


        /// <summary>
        /// Gets or sets Enabled condition.
        /// </summary>
        public string EnabledCondition
        {
            get
            {
                return GetValue<string>("EnabledCondition", String.Empty);
            }
            set
            {
                SetValue("EnabledCondition", value);
            }
        }


        /// <summary>
        /// Gets or sets Enabled condition.
        /// </summary>
        public string StartingPath
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
        /// Indexes of categories, which have to be disabled e.g. '0|1|5|9|'.
        /// </summary>
        protected string DisabledItems
        {
            get
            {
                return GetValue<string>("DisabledItems", string.Empty);
            }
            set
            {
                SetValue("DisabledItems", value);
            }
        }


        private string ExcludedItems
        {
            get
            {
                return GetValue<string>("ExcludedItems", string.Empty);
            }
            set
            {
                SetValue("ExcludedItems", value);
            }
        }


        /// <summary>
        /// Gets the inner CMSDropDownList control.
        /// </summary>
        private CMSDropDownList DropDownList
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref mDropDown);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectSettingsCategory()
        {
            FormControlName = "SelectSettingsCategory";
        }


        /// <summary>
        /// OnLoad event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DisplayOnlyCategories)
            {
                // Only categories that are not marked as groups will be displayed
                WhereCondition = SqlHelper.AddWhereCondition(WhereCondition, "ISNULL([CategoryIsGroup], 0) = 0");
            }

            if (RootCategoryId > 0)
            {
                SettingsCategoryInfo rootCat = SettingsCategoryInfoProvider.GetSettingsCategoryInfo(RootCategoryId);
                if (rootCat != null)
                {
                    StartingPath = rootCat.CategoryIDPath;
                }
            }

            if (CurrentCategoryId > 0)
            {
                ExcludedItems = CurrentCategoryId.ToString();
            }

            if (IsKeyEdit)
            {
                EnabledCondition = "{%CategoryIsGroup == True%}";
            }
        }

        #endregion
    }
}