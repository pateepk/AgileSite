using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base class of the option category sku selector control.
    /// </summary>
    public class ProductOptionSelector : CMSUserControl
    {
        #region "Variables"

        private int mOptionCategoryId = 0;
        private bool mShowOptionCategoryName = true;
        private bool mShowOptionCategoryDescription = true;
        private Control mSelectionControl = null;

        private OptionCategoryInfo mOptionCategory = null;

        private Hashtable mProductOptions = null;
        private InfoDataSet<SKUInfo> mProductOptionsData = null;

        private int mTextOptionSKUID = 0;
        private String mSelectedProductOptions = null;

        private SKUInfo mSKU = null;
        private int mSKUID = 0;

        #endregion


        #region "Public properties"

        /// <summary>
        /// SKU ID
        /// </summary>
        public int SKUID
        {
            get
            {
                return mSKUID;
            }
            set
            {
                mSKU = null;

                mSKUID = value;
            }
        }


        /// <summary>
        /// Main SKU for options.
        /// </summary>        
        public SKUInfo SKU
        {
            get
            {
                return mSKU ?? (mSKU = SKUInfoProvider.GetSKUInfo(SKUID));
            }
            set
            {
                mSKUID = value?.SKUID ?? 0;

                mSKU = value;
            }
        }


        /// <summary>
        /// Gets or sets the CSS class to display options in selectors faded.
        /// </summary>
        public string CssClassFade
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CSS class to display options in selectors normally.
        /// </summary>
        public string CssClassNormal
        {
            get;
            set;
        }


        /// <summary>
        /// Option category ID
        /// </summary>
        public int OptionCategoryId
        {
            get
            {
                if ((mOptionCategoryId == 0) && (OptionCategory != null))
                {
                    mOptionCategoryId = OptionCategory.CategoryID;
                }

                return mOptionCategoryId;
            }
            set
            {
                // Force loading new selection control
                //mSelectionControl = null;

                // Force creating new option category object
                mOptionCategory = null;

                mOptionCategoryId = value;
            }
        }


        /// <summary>
        /// Indicates whether option category name should be displayed to the user.
        /// </summary>
        public bool ShowOptionCategoryName
        {
            get
            {
                return mShowOptionCategoryName;
            }
            set
            {
                mShowOptionCategoryName = value;
            }
        }


        /// <summary>
        /// Indicates whether option category description should be displayed to the user.
        /// </summary>
        public bool ShowOptionCategoryDescription
        {
            get
            {
                return mShowOptionCategoryDescription;
            }
            set
            {
                mShowOptionCategoryDescription = value;
            }
        }


        /// <summary>
        /// Selection control according to the current option category selection type.
        /// </summary>
        public Control SelectionControl
        {
            get
            {
                return mSelectionControl ?? (mSelectionControl = GetSelectionControl());
            }
        }


        /// <summary>
        /// Option category object. It is loaded from option category datarow by default, if it is not specified it is loaded by option category id.
        /// </summary>
        public OptionCategoryInfo OptionCategory
        {
            get
            {
                return mOptionCategory ?? (mOptionCategory = OptionCategoryInfoProvider.GetOptionCategoryInfo(mOptionCategoryId));
            }
            set
            {
                mOptionCategory = value;
            }
        }


        /// <summary>
        /// Shopping cart object required for price formatting. Use this property when in administration.
        /// </summary>
        public ShoppingCartInfo LocalShoppingCartObj
        {
            get;
            set;
        }


        /// <summary>
        /// TRUE - default currency is used for price formatting and no discounts and taxes are applied to price, by default it is set to FALSE.
        /// </summary>
        public bool UseDefaultCurrency
        {
            get;
            set;
        }


        /// <summary>
        /// SKU data of all product options of the current option category
        /// </summary>
        public Hashtable ProductOptions
        {
            get
            {
                return mProductOptions ?? (mProductOptions = GetProductOptions());
            }
        }

        /// <summary>
        /// Selected Product Options to mark in the form
        /// If its NULL - values from DB record of option category will be marked
        /// If its String.Empty - no Options will be marked
        /// </summary>
        public String SelectedProductOptions
        {
            get
            {
                return mSelectedProductOptions ?? GetSelectedSKUOptions();
            }
            set
            {
                mSelectedProductOptions = value;
            }
        }


        /// <summary>
        /// Allows to set IDs of product options which will be ignored during getting options for selector.
        /// Primary used for ignoring options of disabled variants.
        /// If its NULL - no action will be done.
        /// </summary>
        public IEnumerable<int> ProductOptionsInDisabledVariants
        {
            get;
            set;
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// SKUID of Text Option.
        /// </summary>
        public int TextOptionSKUID
        {
            get
            {
                if (mTextOptionSKUID <= 0)
                {
                    if (!DataHelper.DataSourceIsEmpty(ProductOptionsData))
                    {
                        mTextOptionSKUID = ValidationHelper.GetInteger(ProductOptionsData.Items[0].SKUID, 0);
                    }
                }

                return mTextOptionSKUID;
            }
            set
            {
                mTextOptionSKUID = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reloads selection control according to the option category selection type.
        /// </summary>
        public void LoadCategorySelectionControl()
        {
            mSelectionControl = GetSelectionControl();
        }


        /// <summary>
        /// Reloads selection control data according to the option category data.
        /// </summary>
        public void ReloadData()
        {
            DebugHelper.SetContext("ProductOptionSelector");

            // Load actual product options
            LoadSKUOptions();

            // Mark default product options
            SetDefaultSKUOptions();

            DebugHelper.ReleaseContext();
        }


        /// <summary>
        /// Gets selected product options from the selection control.
        /// </summary>
        public string GetSelectedSKUOptions()
        {
            if (SelectionControl != null)
            {
                // Dropdown list, radiobutton list - single selection
                if ((SelectionControl.GetType() == typeof(CMSDropDownList)) ||
                    (SelectionControl.GetType() == typeof(CMSRadioButtonList)))
                {
                    return ((ListControl)SelectionControl).SelectedValue;
                }

                // Checkbox list - multiple selection
                if (SelectionControl.GetType() == typeof(CMSCheckBoxList))
                {
                    var selectedItems = ((CMSCheckBoxList)SelectionControl).GetSelectedItems();
                    return selectedItems.Select(i => i.Value).Aggregate("", (i, j) => i + "," + j);
                }

                // TextBox
                if (SelectionControl is TextBox)
                {
                    return ((TextBox)(SelectionControl)).Text;
                }

                // Form control
                if (SelectionControl is FormEngineUserControl)
                {
                    FormEngineUserControl fc = SelectionControl as FormEngineUserControl;
                    return ValidationHelper.GetString(fc.Value, string.Empty);
                }
            }

            return null;
        }


        /// <summary>
        /// Gets selected product options parameters.
        /// </summary>
        /// <returns>List of Shopping Cart Item Parameters</returns>
        public List<ShoppingCartItemParameters> GetSelectedOptionsParameters()
        {
            List<ShoppingCartItemParameters> options = new List<ShoppingCartItemParameters>();

            if (SelectionControl != null)
            {
                ShoppingCartItemParameters param = null;

                // Dropdown list, Radiobutton list - single selection
                if ((SelectionControl is CMSDropDownList) || (SelectionControl is CMSRadioButtonList))
                {
                    param = new ShoppingCartItemParameters
                    {
                        SKUID = ValidationHelper.GetInteger(((ListControl)SelectionControl).SelectedValue, 0)
                    };

                    if (param.SKUID > 0)
                    {
                        options.Add(param);
                    }
                }
                // Checkbox list - multiple selection
                else if (SelectionControl is CMSCheckBoxList)
                {
                    var selectedItems = ((CMSCheckBoxList)SelectionControl).GetSelectedItems()
                        .Select(i => new ShoppingCartItemParameters
                        {
                            SKUID = ValidationHelper.GetInteger(i.Value, 0)
                        })
                        .Where(i => i.SKUID > 0);

                    options.AddRange(selectedItems);
                }
                else if (SelectionControl is TextBoxWithLabel)
                {
                    if (TextOptionSKUID > 0)
                    {
                        param = new ShoppingCartItemParameters
                        {
                            SKUID = TextOptionSKUID,
                            Text = ((TextBox)(SelectionControl)).Text
                        };

                        if (param.SKUID > 0)
                        {
                            options.Add(param);
                        }
                    }
                }
                else if (SelectionControl is FormEngineUserControl)
                {
                    FormEngineUserControl fc = SelectionControl as FormEngineUserControl;

                    string val = ValidationHelper.GetString(fc.Value, string.Empty);
                    int[] optionsIds = ValidationHelper.GetIntegers(val.Split(','), 0);

                    // Add selected options to parameters
                    foreach (int id in optionsIds)
                    {
                        if (id > 0)
                        {
                            param = new ShoppingCartItemParameters();
                            param.SKUID = id;
                            options.Add(param);
                        }
                    }

                }
            }

            return options;
        }


        /// <summary>
        /// Returns TRUE when selection control is empty or only '(none)' record is included, otherwise returns FALSE.
        /// </summary>
        public bool IsSelectionControlEmpty()
        {
            if (SelectionControl != null)
            {
                // Text type
                TextBox tb = SelectionControl as TextBox;
                if (tb != null)
                {
                    return string.IsNullOrEmpty(tb.Text);
                }

                // List control types
                ListControl list = SelectionControl as ListControl;
                if (list != null)
                {
                    bool noItems = (list.Items.Count == 0);
                    bool onlyNoneRecord = ((list.Items.Count == 1) && (list.Items.FindByValue("0") != null));

                    return (noItems || onlyNoneRecord);
                }

                // Form controls
                FormEngineUserControl formControl = SelectionControl as FormEngineUserControl;
                if (formControl != null)
                {
                    if (ValidationHelper.GetInteger(formControl.Value, 1) <= 0)
                    {
                        return true;
                    }

                    return (formControl.Value == null) || string.IsNullOrEmpty(formControl.Value.ToString());
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns true if there is a choice of values in selection control.
        /// </summary>
        public bool HasSelectableOptions()
        {
            if (SelectionControl != null)
            {
                // Text type or form control
                if ((SelectionControl is TextBox) || (SelectionControl is FormEngineUserControl))
                {
                    return true;
                }

                return !IsSelectionControlEmpty();
            }

            return true;
        }


        /// <summary>
        /// Validates selected/entered product option. If it is valid, returns true, otherwise returns false.
        /// </summary>        
        public virtual bool IsValid()
        {
            return true;
        }


        /// <summary>
        /// Sets the specified options as enabled and other as selectable but disabled/grey through CSS style.
        /// </summary>
        /// <param name="optionIds">The option ids.</param>
        public void SetEnabledOptions(List<int> optionIds)
        {
            ListControl selectionListControl = SelectionControl as ListControl;

            if (selectionListControl != null)
            {
                string selectedOptionId = GetSelectedSKUOptions();
                // If selected option is "disabled" -> style whole selector as faded, to ensure selected dropdownlist value would be faded
                if (!optionIds.Contains(ValidationHelper.GetInteger(selectedOptionId, 0)))
                {
                    selectionListControl.RemoveCssClass(CssClassNormal);
                    selectionListControl.AddCssClass(CssClassFade);
                }
                else
                {
                    selectionListControl.RemoveCssClass(CssClassFade);
                    selectionListControl.AddCssClass(CssClassNormal);
                }

                foreach (ListItem item in selectionListControl.Items)
                {
                    int skuId = ValidationHelper.GetInteger(item.Value, 0);

                    if (optionIds.Contains(skuId))
                    {
                        // Enabled options style as normal
                        item.Attributes.Add("class", CssClassNormal);
                    }
                    else
                    {
                        // Disabled options style as faded
                        item.Attributes.Add("class", CssClassFade);
                    }
                }
            }
        }


        /// <summary>
        /// Removes disabled option from selector.
        /// </summary>
        /// <param name="enabledOptionIds">The enabled option ids.</param>
        public void RemoveDisabledOptions(List<int> enabledOptionIds)
        {
            ListControl selectionListControl = SelectionControl as ListControl;
            List<ListItem> disabled = new List<ListItem>();

            if (selectionListControl != null)
            {
                // If selected option is "disabled" -> style whole selector as faded, to ensure selected dropdownlist value would be faded
                if (SelectionControl.GetType() == typeof(CMSDropDownList))
                {
                    // Dropdownlist -> only one item could be selected
                    string selectedOptionId = GetSelectedSKUOptions();
                    if (!enabledOptionIds.Contains(ValidationHelper.GetInteger(selectedOptionId, 0)))
                    {
                        selectionListControl.RemoveCssClass(CssClassNormal);
                        selectionListControl.AddCssClass(CssClassFade);
                    }
                    else
                    {
                        selectionListControl.RemoveCssClass(CssClassFade);
                        selectionListControl.AddCssClass(CssClassNormal);
                    }
                }

                foreach (ListItem item in selectionListControl.Items)
                {
                    int skuId = ValidationHelper.GetInteger(item.Value, 0);

                    if (!enabledOptionIds.Contains(skuId) && skuId != 0)
                    {
                        disabled.Add(item);
                    }
                    else if (skuId == 0)
                    {
                        item.Attributes.Add("class", CssClassFade);
                    }
                    else
                    {
                        // Enabled options style as normal, in case the whole dropdown list was faded
                        item.Attributes.Add("class", CssClassNormal);
                    }
                }

                foreach (var item in disabled)
                {
                    selectionListControl.Items.Remove(item);
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns initialized selection control according to the specified selection type.
        /// </summary>
        private Control GetSelectionControl()
        {
            if (OptionCategory != null)
            {
                switch (OptionCategory.CategorySelectionType)
                {
                    // Dropdownlist                     
                    case OptionCategorySelectionTypeEnum.Dropdownlist:

                        CMSDropDownList dropDown = new CMSDropDownList();
                        dropDown.ID = "dropdown";
                        dropDown.DataTextField = "SKUName";
                        dropDown.DataValueField = "SKUID";
                        dropDown.DataBound += SelectionControl_DataBound;
                        dropDown.CssClass = "form-control";

                        return dropDown;

                    // Checkbox list with horizontal direction
                    case OptionCategorySelectionTypeEnum.CheckBoxesHorizontal:

                        CMSCheckBoxList boxListHorizontal = new CMSCheckBoxList();
                        boxListHorizontal.ID = "chkHorizontal";
                        boxListHorizontal.RepeatDirection = RepeatDirection.Horizontal;
                        boxListHorizontal.DataTextField = "SKUName";
                        boxListHorizontal.DataValueField = "SKUID";
                        boxListHorizontal.DataBound += SelectionControl_DataBound;
                        boxListHorizontal.CssClass = "checkbox-list-horizontal";

                        return boxListHorizontal;

                    // Checkbox list with vertical direction
                    case OptionCategorySelectionTypeEnum.CheckBoxesVertical:

                        CMSCheckBoxList boxListVertical = new CMSCheckBoxList();
                        boxListVertical.ID = "chkVertical";
                        boxListVertical.RepeatDirection = RepeatDirection.Vertical;
                        boxListVertical.DataTextField = "SKUName";
                        boxListVertical.DataValueField = "SKUID";
                        boxListVertical.DataBound += SelectionControl_DataBound;
                        boxListVertical.CssClass = "checkbox-list-vertical";

                        return boxListVertical;

                    // Radiobutton list with horizontal direction
                    case OptionCategorySelectionTypeEnum.RadioButtonsHorizontal:

                        CMSRadioButtonList buttonListHorizontal = new CMSRadioButtonList();
                        buttonListHorizontal.ID = "radHorizontal";
                        buttonListHorizontal.RepeatDirection = RepeatDirection.Horizontal;
                        buttonListHorizontal.DataTextField = "SKUName";
                        buttonListHorizontal.DataValueField = "SKUID";
                        buttonListHorizontal.DataBound += SelectionControl_DataBound;
                        buttonListHorizontal.CssClass = "radio-list-horizontal";

                        return buttonListHorizontal;

                    // Radiobutton list with vertical direction
                    case OptionCategorySelectionTypeEnum.RadioButtonsVertical:

                        CMSRadioButtonList buttonListVertical = new CMSRadioButtonList();
                        buttonListVertical.ID = "radVertical";
                        buttonListVertical.RepeatDirection = RepeatDirection.Vertical;
                        buttonListVertical.DataTextField = "SKUName";
                        buttonListVertical.DataValueField = "SKUID";
                        buttonListVertical.DataBound += SelectionControl_DataBound;
                        buttonListVertical.CssClass = "radio-list-vertical";

                        return buttonListVertical;

                    // Text box
                    case OptionCategorySelectionTypeEnum.TextBox:

                        TextBoxWithLabel text = new TextBoxWithLabel();
                        text.ID = "txtText";
                        text.CssClass = "form-control";

                        return text;

                    // Text area
                    case OptionCategorySelectionTypeEnum.TextArea:

                        TextBoxWithLabel textArea = new TextBoxWithLabel();
                        textArea.ID = "txtArea";
                        textArea.TextMode = TextBoxMode.MultiLine;
                        textArea.CssClass = "form-control";

                        return textArea;
                }
            }

            return null;
        }


        private void SelectionControl_DataBound(object sender, EventArgs e)
        {
            // Check if datasource has data
            if (!DataHelper.DataSourceIsEmpty(((ListControl)sender).DataSource))
            {
                var isVariantAndDisplayPrice = OptionCategory.CategoryDisplayPrice && !VariantHelper.AreCategoriesUsedInVariants(SKUID, new[] { OptionCategoryId });

                foreach (DataRow row in ((DataSet)((ListControl)sender).DataSource).Tables[0].Rows)
                {
                    int skuId = ValidationHelper.GetInteger(row["SKUId"], 0);
                    ListItem item = ((ListControl)sender).Items.FindByValue(skuId.ToString());

                    if (item != null)
                    {
                        if (!(SelectionControl is CMSDropDownList))
                        {
                            item.Text = HTMLHelper.HTMLEncode(item.Text);
                        }

                        // Hide if price is defined in product variant
                        if (isVariantAndDisplayPrice)
                        {
                            // Append price to option's text
                            item.Text += GetPrice(row);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Recalculates and formats price.
        /// </summary>
        /// <param name="row">Data row to create price label for.</param>
        private string GetPrice(DataRow row)
        {
            SKUInfo option = new SKUInfo(row);
            CurrencyInfo currency;
            decimal price;

            // Use product option currency
            if (UseDefaultCurrency)
            {
                // Get site main currency
                currency = CurrencyInfoProvider.GetMainCurrency(option.SKUSiteID);

                // Get product price
                price = Service.Resolve<ISKUPriceSourceFactory>()
                    .GetSKUPriceSource(option.SKUSiteID)
                    .GetPrice(option, currency);
            }
            // Use cart currency
            else
            {
                // Get cart currency
                currency = LocalShoppingCartObj.Currency;

                price = Service.Resolve<ICatalogPriceCalculatorFactory>()
                    .GetCalculator(LocalShoppingCartObj.ShoppingCartSiteID)
                    .GetAdjustment(option, SKU, LocalShoppingCartObj);
            }

            if (price != 0)
            {
                // Prevent double encoding in DDL
                if (OptionCategory.CategorySelectionType == OptionCategorySelectionTypeEnum.Dropdownlist)
                {
                    return " (" + CurrencyInfoProvider.GetRelativelyFormattedPrice(price, currency, false) + ")";
                }

                return "<span class=\"price-adjustment-fade\"> (" + CurrencyInfoProvider.GetRelativelyFormattedPrice(price, currency) + ") </span>";
            }

            return string.Empty;
        }


        /// <summary>
        /// Sets category default options as 'Selected' in selection control.
        /// </summary>        
        public void SetDefaultSKUOptions()
        {
            String defaultOptions = OptionCategory.CategoryDefaultOptions;

            if ((SelectionControl != null))
            {
                // Dropdown list - single selection
                // Radiobutton list - single selection
                if ((SelectionControl is CMSDropDownList) ||
                    (SelectionControl is CMSRadioButtonList))
                {
                    try
                    {
                        ListControl listControl = (ListControl)SelectionControl;

                        // Selector has an option to be selected
                        if (listControl.Items.Count > 0)
                        {
                            // The first one option from selected values
                            string defaultValue = TrimMultipleChoices(defaultOptions);

                            // Try to find default items in list items
                            ListItem selectedItem = listControl.Items.FindByValue(defaultValue);

                            if (selectedItem != null)
                            {
                                listControl.SelectedValue = defaultValue;
                            }
                            else
                            {
                                // Select the first item in a radio button selector
                                listControl.SelectedIndex = 0;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                // Checkbox list - multiple selection
                else if ((SelectionControl is CMSCheckBoxList) && (!String.IsNullOrEmpty(defaultOptions)))
                {
                    try
                    {
                        if (defaultOptions != "")
                        {
                            foreach (string skuId in defaultOptions.Split(','))
                            {
                                // Ensure value is not empty
                                string value = (skuId != "") ? skuId : "0";

                                ListItem item = ((CMSCheckBoxList)SelectionControl).Items.FindByValue(value);
                                if (item != null)
                                {
                                    item.Selected = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                // Text type
                else if (SelectionControl is TextBoxWithLabel)
                {
                    TextBoxWithLabel tb = SelectionControl as TextBoxWithLabel;
                    tb.Text = defaultOptions;
                }
                // Form control
                else if (SelectionControl is FormEngineUserControl)
                {
                    ((FormEngineUserControl)SelectionControl).Value = defaultOptions;
                }
            }
        }


        /// <summary>
        /// Dataset with the product options data which should be loaded to the product options selector.
        /// </summary>
        public InfoDataSet<SKUInfo> ProductOptionsData
        {
            get
            {
                if (mProductOptionsData == null)
                {
                    if (IsLiveSite)
                    {
                        // Get cache minutes
                        int cacheMinutes = SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSCacheMinutes");

                        // Try to get data from cache
                        using (var cs = new CachedSection<InfoDataSet<SKUInfo>>(ref mProductOptionsData, cacheMinutes, true, null, "skuoptions", SKUID, OptionCategoryId))
                        {
                            if (cs.LoadData)
                            {
                                // Get the data
                                mProductOptionsData = VariantHelper.GetEnabledOptionsWithVariantOptions(SKUID, OptionCategoryId);

                                // Filter options from disabled variants
                                if (ProductOptionsInDisabledVariants != null)
                                {
                                    var skus = mProductOptionsData.OfType<SKUInfo>().Where(po => po.SKUEnabled && !ProductOptionsInDisabledVariants.Contains(po.SKUID));
                                    mProductOptionsData = new InfoDataSet<SKUInfo>(skus.ToArray());
                                }

                                // Save to the cache
                                if (cs.Cached)
                                {
                                    // Get dependencies
                                    List<string> dependencies = new List<string>();
                                    dependencies.Add("ecommerce.optioncategory|byid|" + OptionCategoryId);
                                    dependencies.Add("ecommerce.sku|all");

                                    // Set dependencies
                                    cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                                }

                                cs.Data = mProductOptionsData;
                            }
                        }
                    }
                    else
                    {
                        // Get the data
                        mProductOptionsData = VariantHelper.GetEnabledOptionsWithVariantOptions(SKUID, OptionCategoryId);
                    }
                }

                return mProductOptionsData;
            }
        }


        /// <summary>
        /// Loads data (SKU options) to the selection control.
        /// </summary>
        private void LoadSKUOptions()
        {
            // Only for none-text types
            if (SelectionControl != null)
            {
                if (SelectionControl is TextBoxWithLabel)
                {
                    // "No product options" label wont be shown
                    SetEmptyInfoVisibility(false);
                    if (!DataHelper.DataSourceIsEmpty(ProductOptionsData))
                    {
                        DataRow dr = ProductOptionsData.Tables[0].Rows[0];
                        TextOptionSKUID = ValidationHelper.GetInteger(ProductOptionsData.Tables[0].Rows[0]["SKUID"], 0);

                        var tb = (TextBoxWithLabel)SelectionControl;
                        tb.LabelText = OptionCategory.CategoryDisplayPrice ? GetPrice(dr) : "";
                    }
                }
                else if (SelectionControl is ListControl)
                {
                    // Set label visibility according to source emptiness
                    SetEmptyInfoVisibility(DataHelper.DataSourceIsEmpty(ProductOptionsData));

                    ((ListControl)SelectionControl).DataSource = ProductOptionsData;
                    SelectionControl.DataBind();

                    // Add '(none)' record when it is allowed
                    if ((OptionCategory != null) && (OptionCategory.CategoryDefaultRecord != "") && (OptionCategory.CategorySelectionType != OptionCategorySelectionTypeEnum.CheckBoxesHorizontal)
                                                                                                 && (OptionCategory.CategorySelectionType != OptionCategorySelectionTypeEnum.CheckBoxesVertical)
                                                                                                 && (OptionCategory.CategorySelectionType != OptionCategorySelectionTypeEnum.RadioButtonsHorizontal)
                                                                                                 && (OptionCategory.CategorySelectionType != OptionCategorySelectionTypeEnum.RadioButtonsVertical))
                    {
                        string emptyText = (SelectionControl is DropDownList) ? OptionCategory.CategoryDefaultRecord : HTMLHelper.HTMLEncode(OptionCategory.CategoryDefaultRecord);
                        ListItem noneRecord = new ListItem(emptyText, "0");
                        ((ListControl)SelectionControl).Items.Insert(0, noneRecord);
                    }
                }
            }
        }


        /// <summary>
        /// Returns hash table with product options.
        /// </summary>
        private Hashtable GetProductOptions()
        {
            Hashtable optionsTable = new Hashtable();

            // Get options and load them to the hashtable
            foreach (SKUInfo sku in ProductOptionsData)
            {
                optionsTable.Add(sku.SKUID, sku);
            }


            return optionsTable;
        }


        /// <summary>
        /// Override in ascx.cs to handle no product options label in form
        /// </summary>
        /// <param name="visible">Is label "no product options" visible</param>
        protected virtual void SetEmptyInfoVisibility(bool visible)
        {
        }


        /// <summary>
        /// Returns only first option from multiple options set
        /// </summary>
        /// <param name="selectedProductOptions">Selected product options</param>
        /// <returns>Only one option</returns>
        private String TrimMultipleChoices(String selectedProductOptions)
        {
            if (String.IsNullOrEmpty(selectedProductOptions))
            {
                return String.Empty;
            }

            return selectedProductOptions.Split(',')[0];
        }

        #endregion
    }
}