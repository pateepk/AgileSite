using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.UIControls;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CMSApp.CMSWebParts.WTE.Custom
{
    public partial class UniPager_WTE : CMSAbstractWebPart
    {
        #region "struct"

        /// <summary>
        /// Contains parsed page size options.
        /// </summary>
        protected struct PageSizeOptionsData
        {
            /// <summary>
            /// List of page size options.
            /// </summary>
            public List<int> Options
            {
                get;
                set;
            }

            /// <summary>
            /// Is set to true when given page size options string contains 'Select ALL' macro.
            /// </summary>
            public bool ContainsAll
            {
                get;
                set;
            }
        }

        #endregion "struct"

        #region "Constants"

        /// <summary>
        /// Constant for query-string key of page size value. Used with <see cref="PagerMode"/> set to 'QueryString'.
        /// </summary>
        protected const string PAGE_SIZE_QUERYSTRING_KEY = "pagesize";

        /// <summary>
        /// Contains default page size options.
        /// </summary>
        protected const string DEFAULT_PAGE_SIZE_OPTIONS = "5,10,25,50,100";

        #endregion "Constants"

        #region "members"

        private bool mDisplayPager = true;

        #endregion "members"

        #region "Properties"

        /// <summary>
        /// Indicates if whole pager should be displayed.
        /// </summary>
        public virtual bool DisplayPager
        {
            get
            {
                return mDisplayPager;
            }
            set
            {
                mDisplayPager = value;

                // Reset default page size
                if (!value)
                {
                    DefaultPageSize = 0;
                }
            }
        }

        /// <summary>
        /// Indicates if pager was already loaded.
        /// </summary>
        private bool PagerLoaded
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["PagerLoaded"], false);
            }
            set
            {
                ViewState["PagerLoaded"] = value;
            }
        }

        #region "Pager properties"

        /// <summary>
        /// Gets or sets the value that indicates whether scroll position should be cleared after post back paging
        /// </summary>
        public bool ResetScrollPositionOnPostBack
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ResetScrollPositionOnPostBack"), pagerElem.ResetScrollPositionOnPostBack);
            }
            set
            {
                SetValue("ResetScrollPositionOnPostBack", value);
                pagerElem.ResetScrollPositionOnPostBack = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID of web part which should be paged. Use this property only for web parts.
        /// </summary>
        public string TargetControlName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TargetControlName"), pagerElem.FilterTypePageControl);
            }
            set
            {
                SetValue("TargetControlName", value);
                pagerElem.FilterTypePageControl = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of records to display on a page.
        /// </summary>
        public int PageSize
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PageSize"), pagerElem.PageSize);
            }
            set
            {
                SetValue("PageSize", value);
                pagerElem.PageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of pages displayed for current page range.
        /// </summary>
        public int GroupSize
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("GroupSize"), pagerElem.GroupSize);
            }
            set
            {
                SetValue("GroupSize", value);
                pagerElem.GroupSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the pager mode ('querystring' or 'postback').
        /// </summary>
        public string PagingMode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PagingMode"), "querystring");
            }
            set
            {
                if (value != null)
                {
                    SetValue("PagingMode", value);
                    switch (value.ToLowerCSafe())
                    {
                        case "postback":
                            pagerElem.PagerMode = UniPagerMode.PostBack;
                            break;

                        default:
                            pagerElem.PagerMode = UniPagerMode.Querystring;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the pager mode.
        /// </summary>
        public UniPagerMode PagerMode
        {
            get
            {
                return pagerElem.PagerMode;
            }
            set
            {
                pagerElem.PagerMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the querystring parameter.
        /// </summary>
        public string QueryStringKey
        {
            get
            {
                return ValidationHelper.GetString(GetValue("QueryStringKey"), pagerElem.QueryStringKey);
            }
            set
            {
                SetValue("QueryStringKey", value);
                pagerElem.QueryStringKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether first and last item template are displayed dynamically based on current view.
        /// </summary>
        public bool DisplayFirstLastAutomatically
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DisplayFirstLastAutomatically"), pagerElem.DisplayFirstLastAutomatically);
            }
            set
            {
                SetValue("DisplayFirstLastAutomatically", value);
                pagerElem.DisplayFirstLastAutomatically = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether first and last item template are displayed dynamically based on current view.
        /// </summary>
        public bool DisplayPreviousNextAutomatically
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DisplayPreviousNextAutomatically"), pagerElem.DisplayPreviousNextAutomatically);
            }
            set
            {
                SetValue("DisplayPreviousNextAutomatically", value);
                pagerElem.DisplayPreviousNextAutomatically = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether pager should be hidden for single page.
        /// </summary>
        public bool HidePagerForSinglePage
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("HidePagerForSinglePage"), pagerElem.HidePagerForSinglePage);
            }
            set
            {
                SetValue("HidePagerForSinglePage", value);
                pagerElem.HidePagerForSinglePage = value;
            }
        }

        #endregion "Pager properties"

        #region "Additional Options"

        /// <summary>
        /// Page size label text
        /// </summary>
        public string PageSizeCaption
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageSizeCaption"), "Item per page");
            }
            set
            {
                SetValue("PageSizeCaption", value);
            }
        }

        /// <summary>
        /// Default page size at first load.
        /// </summary>
        public int DefaultPageSize
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("DefaultPageSize"), pagerElem.PageSize);
            }
            set
            {
                SetValue("DefaultPageSize", value);
                pagerElem.PageSize = value;
                SetupControl(true);
            }
        }

        /// <summary>
        /// Gets or sets current page size.
        /// </summary>
        public int CurrentPageSize
        {
            get
            {
                if (PagerMode == UniPagerMode.Querystring)
                {
                    return QueryHelper.GetInteger(PAGE_SIZE_QUERYSTRING_KEY, pagerElem.PageSize);
                }

                if (drpPageSize.Visible)
                {
                    return ValidationHelper.GetInteger(drpPageSize.SelectedValue, pagerElem.PageSize);
                }

                return pagerElem.PageSize;
            }
            set
            {
                pagerElem.PageSize = value;
                SetupControl(true);
            }
        }

        /// <summary>
        /// Show Page size selection
        /// </summary>
        public bool ShowPageSize
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowPageSize"), true);
                //return true;
            }
            set
            {
                SetValue("ShowPageSize", value);
            }
        }

        /// <summary>
        /// Pager CSS class (outter most container)
        /// </summary>
        public string PagerCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PagerCssClass"), "pager-control");
            }
            set
            {
                SetValue("PagerCssClass", value);
            }
        }

        /// <summary>
        /// Page size drop down container
        /// </summary>
        public string PageSizeWrapperCSSClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageSizeWrapperCSSClass"), "pager-control_size");
            }
            set
            {
                SetValue("PageSizeWrapperCSSClass", value);
            }
        }

        /// <summary>
        /// Page size label CSS Class
        /// </summary>
        public string PageSizeLabelCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageSizeLabelCssClass"), "");
            }
            set
            {
                SetValue("PageSizeLabelCssClass", value);
            }
        }

        /// <summary>
        /// Page size drop down CSS Class
        /// </summary>
        public string PageSizeDropDownCSSClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageSizeDropDownCSSClass"), "select-css");
            }
            set
            {
                SetValue("PageSizeDropDownCSSClass", value);
            }
        }

        /// <summary>
        /// page size options
        /// </summary>
        public string PageSizeOptions
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageSizeOptions"), "");
            }
            set
            {
                SetValue("PageSizeOptions", value);
                SetupControl(true);
            }
        }

        #endregion "Additional Options"

        #region "Template properties"

        /// <summary>
        /// Gets or sets the pages template.
        /// </summary>
        public string PagesTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Pages"), "");
            }
            set
            {
                SetValue("Pages", value);
            }
        }

        /// <summary>
        /// Gets or sets the current page template.
        /// </summary>
        public string CurrentPageTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CurrentPage"), "");
            }
            set
            {
                SetValue("CurrentPage", value);
            }
        }

        /// <summary>
        /// Gets or sets the separator template.
        /// </summary>
        public string SeparatorTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageSeparator"), "");
            }
            set
            {
                SetValue("PageSeparator", value);
            }
        }

        /// <summary>
        /// Gets or sets the first page template.
        /// </summary>
        public string FirstPageTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FirstPage"), "");
            }
            set
            {
                SetValue("FirstPage", value);
            }
        }

        /// <summary>
        /// Gets or sets the last page template.
        /// </summary>
        public string LastPageTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("LastPage"), "");
            }
            set
            {
                SetValue("LastPage", value);
            }
        }

        /// <summary>
        /// Gets or sets the previous page template.
        /// </summary>
        public string PreviousPageTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PreviousPage"), "");
            }
            set
            {
                SetValue("PreviousPage", value);
            }
        }

        /// <summary>
        /// Gets or sets the next page template.
        /// </summary>
        public string NextPageTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NextPage"), "");
            }
            set
            {
                SetValue("NextPage", value);
            }
        }

        /// <summary>
        /// Gets or sets the previous group template.
        /// </summary>
        public string PreviousGroupTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PreviousGroup"), "");
            }
            set
            {
                SetValue("PreviousGroup", value);
            }
        }

        /// <summary>
        /// Gets or sets the next group template.
        /// </summary>
        public string NextGroupTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NextGroup"), "");
            }
            set
            {
                SetValue("NextGroup", value);
            }
        }

        /// <summary>
        /// Gets or sets the layout template.
        /// </summary>
        public string LayoutTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PagerLayout"), "");
            }
            set
            {
                SetValue("PagerLayout", value);
            }
        }

        /// <summary>
        /// Gets or sets the direct page template.
        /// </summary>
        public string DirectPageTemplate
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DirectPage"), "");
            }
            set
            {
                SetValue("DirectPage", value);
            }
        }

        #endregion "Template properties"

        #endregion "Properties"

        #region "Methods"

        #region "Page events"

        protected override void OnLoad(EventArgs e)
        {
            SetupControl();

            drpPageSize.SelectedIndexChanged += drpPageSize_SelectedIndexChanged;

            base.OnLoad(e);
        }

        /// <summary>
        /// Content loaded event handler.
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }

        /// <summary>
        /// OnPreRender - check visibility.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            pagerElem.Visible = DisplayPager;
            divPageSize.Visible = (DisplayPager && ShowPageSize && (drpPageSize.Items.Count > 1) && (pagerElem.DataSourceItemsCount > 0));
            lblPageSize.Text = PageSizeCaption;

            // Handle pager only if visible
            if (pagerElem.Visible)
            {
                if (pagerElem.PageCount > pagerElem.GroupSize)
                {
                    LocalizedLabel lblPage = ControlsHelper.GetChildControl(pagerElem, typeof(LocalizedLabel), "lblPage") as LocalizedLabel;
                    using (Control drpPage = ControlsHelper.GetChildControl(pagerElem, typeof(DropDownList), "drpPage"))
                    {
                        using (Control txtPage = ControlsHelper.GetChildControl(pagerElem, typeof(TextBox), "txtPage"))
                        {
                            if ((lblPage != null) && (drpPage != null) && (txtPage != null))
                            {
                                if (pagerElem.PageCount > 20)
                                {
                                    drpPage.Visible = false;
                                    // Set labels associated control for US Section 508 validation
                                    lblPage.AssociatedControlClientID = txtPage.ClientID;
                                }
                                else
                                {
                                    txtPage.Visible = false;
                                    // Set labels associated control for US Section 508 validation
                                    lblPage.AssociatedControlClientID = drpPage.ClientID;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Remove direct page control if only one group of pages is  shown
                    using (Control plcDirectPage = ControlsHelper.GetChildControl(pagerElem, typeof(PlaceHolder), "plcDirectPage"))
                    {
                        if (plcDirectPage != null)
                        {
                            plcDirectPage.Controls.Clear();
                        }
                    }
                }
            }

            // Hide entire control if pager and page size drodown is hidden
            if (!divPageSize.Visible && !pagerElem.Visible)
            {
                Visible = false;
                divMain.Visible = false;
            }

            PagerLoaded = true;
        }

        #endregion "Page events"

        #region "binding"

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        public void SetupControl(bool forceReload = false)
        {
            if (StopProcessing)
            {
                // Do nothing
            }
            else
            {
                // If target control is not specified do nothing
                if (!String.IsNullOrEmpty(TargetControlName))
                {
                    Visible = true;

                    SetCSS(divMain, PagerCssClass);
                    SetCSS(divPageSize, PageSizeWrapperCSSClass);
                    SetCSS(lblPageSize, PageSizeLabelCssClass);
                    SetCSS(drpPageSize, PageSizeDropDownCSSClass);

                    #region "Pager properties"

                    // Set pager properties
                    pagerElem.FilterTypePageControl = TargetControlName;
                    pagerElem.PageSize = PageSize;
                    SetPageSize(forceReload);
                    pagerElem.PageSize = ValidationHelper.GetInteger(drpPageSize.SelectedValue, -1);
                    pagerElem.GroupSize = GroupSize;
                    pagerElem.QueryStringKey = QueryStringKey;
                    pagerElem.DisplayFirstLastAutomatically = DisplayFirstLastAutomatically;
                    pagerElem.DisplayPreviousNextAutomatically = DisplayPreviousNextAutomatically;
                    pagerElem.HidePagerForSinglePage = HidePagerForSinglePage;
                    pagerElem.ResetScrollPositionOnPostBack = ResetScrollPositionOnPostBack;

                    // Set pager mode
                    switch (PagingMode.ToLowerCSafe())
                    {
                        case "postback":
                            pagerElem.PagerMode = UniPagerMode.PostBack;
                            break;

                        default:
                            pagerElem.PagerMode = UniPagerMode.Querystring;
                            break;
                    }

                    #endregion "Pager properties"

                    #region "Pager templates"

                    // Pages
                    if (!String.IsNullOrEmpty(PagesTemplate))
                    {
                        pagerElem.PageNumbersTemplate = TransformationHelper.LoadTransformation(pagerElem, PagesTemplate);
                    }

                    // Current page
                    if (!String.IsNullOrEmpty(CurrentPageTemplate))
                    {
                        pagerElem.CurrentPageTemplate = TransformationHelper.LoadTransformation(pagerElem, CurrentPageTemplate);
                    }

                    // Separator
                    if (!String.IsNullOrEmpty(SeparatorTemplate))
                    {
                        pagerElem.PageNumbersSeparatorTemplate = TransformationHelper.LoadTransformation(pagerElem, SeparatorTemplate);
                    }

                    // First page
                    if (!String.IsNullOrEmpty(FirstPageTemplate))
                    {
                        pagerElem.FirstPageTemplate = TransformationHelper.LoadTransformation(pagerElem, FirstPageTemplate);
                    }

                    // Last page
                    if (!String.IsNullOrEmpty(LastPageTemplate))
                    {
                        pagerElem.LastPageTemplate = TransformationHelper.LoadTransformation(pagerElem, LastPageTemplate);
                    }

                    // Previous page
                    if (!String.IsNullOrEmpty(PreviousPageTemplate))
                    {
                        pagerElem.PreviousPageTemplate = TransformationHelper.LoadTransformation(pagerElem, PreviousPageTemplate);
                    }

                    // Next page
                    if (!String.IsNullOrEmpty(NextPageTemplate))
                    {
                        pagerElem.NextPageTemplate = TransformationHelper.LoadTransformation(pagerElem, NextPageTemplate);
                    }

                    // Previous group
                    if (!String.IsNullOrEmpty(PreviousGroupTemplate))
                    {
                        pagerElem.PreviousGroupTemplate = TransformationHelper.LoadTransformation(pagerElem, PreviousGroupTemplate);
                    }

                    // Next group
                    if (!String.IsNullOrEmpty(NextGroupTemplate))
                    {
                        pagerElem.NextGroupTemplate = TransformationHelper.LoadTransformation(pagerElem, NextGroupTemplate);
                    }

                    // Direct page
                    if (!String.IsNullOrEmpty(DirectPageTemplate))
                    {
                        pagerElem.DirectPageTemplate = TransformationHelper.LoadTransformation(pagerElem, DirectPageTemplate);
                    }

                    // Layout
                    if (!String.IsNullOrEmpty(LayoutTemplate))
                    {
                        pagerElem.LayoutTemplate = TransformationHelper.LoadTransformation(pagerElem, LayoutTemplate);
                    }

                    #endregion "Pager templates"
                }
            }
        }

        /// <summary>
        /// Reload data.
        /// </summary>
        public override void ReloadData()
        {
            SetupControl();
            pagerElem.ReloadData(true);
            base.ReloadData();
        }

        #endregion "binding"

        #region general events

        /// <summary>
        /// Checks whether current control should be visible.
        /// </summary>
        private void Page_SaveStateComplete(object sender, EventArgs e)
        {
            // Check visibility
            if ((HidePagerForSinglePage && pagerElem.PageCount < 2) || (pagerElem.DataSourceItemsCount == 0))
            {
                Visible = false;
            }
        }

        /// <summary>
        /// Size drop down index changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void drpPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            pagerElem.CurrentPage = 1;
            pagerElem.PageSize = ValidationHelper.GetInteger(drpPageSize.SelectedValue, -1);

            if (PagerMode == UniPagerMode.Querystring)
            {
                // Remove query string paging key to get to page 1
                string url = URLHelper.RemoveParameterFromUrl(RequestContext.CurrentURL, pagerElem.QueryStringKey);
                url = URLHelper.UpdateParameterInUrl(url, PAGE_SIZE_QUERYSTRING_KEY, pagerElem.PageSize.ToString());
                URLHelper.Redirect(url);
            }
            else if (pagerElem.PagedControl != null)
            {
                pagerElem.PagedControl.ReBind();
            }
        }

        #endregion general events

        #region "Page size control"

        /// <summary>
        /// Sets page size dropdown list according to PageSize property.
        /// </summary>
        private void SetPageSize(bool forceReload)
        {
            if ((drpPageSize.Items.Count == 0) || forceReload)
            {
                string currentPagesize = CurrentPageSize.ToString();

                if (!PagerLoaded && (PagerMode != UniPagerMode.Querystring))
                {
                    currentPagesize = DefaultPageSize.ToString();
                }

                drpPageSize.Items.Clear();

                PageSizeOptionsData pageSizeOptionsData;

                if (!TryParsePageSizeOptions(PageSizeOptions, out pageSizeOptionsData))
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "UIPager", "ParseCustomOptions", "Could not parse custom page size options: '" + PageSizeOptions + "'. Correct format is values separated by comma.");
                    TryParsePageSizeOptions(DEFAULT_PAGE_SIZE_OPTIONS, out pageSizeOptionsData);
                }

                // Add default page size if not present
                if ((DefaultPageSize > 0) && !pageSizeOptionsData.Options.Contains(DefaultPageSize))
                {
                    pageSizeOptionsData.Options.Add(DefaultPageSize);
                }

                // Sort list of page sizes
                pageSizeOptionsData.Options.Sort();

                FillPageSizeDropdown(pageSizeOptionsData, currentPagesize);
            }
        }

        private void FillPageSizeDropdown(PageSizeOptionsData pageSizeOptionsData, string currentPagesize)
        {
            ListItem item;
            foreach (int size in pageSizeOptionsData.Options)
            {
                item = new ListItem(size.ToString());
                if (item.Value == currentPagesize)
                {
                    item.Selected = true;
                }
                drpPageSize.Items.Add(item);
            }

            // Add 'Select ALL' macro at the end of list
            if (pageSizeOptionsData.ContainsAll)
            {
                item = new ListItem(GetString("general.selectall"), "-1");
                if (currentPagesize == "-1")
                {
                    item.Selected = true;
                }
                drpPageSize.Items.Add(item);
            }
        }

        /// <summary>
        /// Parses given string containing page size options.
        /// </summary>
        /// <param name="pageSizeOptions">String containing page size options separated by comma</param>
        /// <param name="pageSizeOptionsData">Class containing parsed page size options</param>
        /// <returns>True when parsing successfully parsed given string</returns>
        protected bool TryParsePageSizeOptions(string pageSizeOptions, out PageSizeOptionsData pageSizeOptionsData)
        {
            pageSizeOptionsData = new PageSizeOptionsData
            {
                Options = new List<int>()
            };

            bool containsZero = false;

            foreach (string size in pageSizeOptions.Split(','))
            {
                string trimmedSize = size.Trim();
                if (trimmedSize.ToUpperCSafe() == UniGrid.ALL)
                {
                    pageSizeOptionsData.ContainsAll = true;
                }
                else
                {
                    int parsedSize = ValidationHelper.GetInteger(trimmedSize, int.MinValue);
                    if (parsedSize == 0)
                    {
                        containsZero = true;
                        continue;
                    }
                    if ((parsedSize > 0) && !pageSizeOptionsData.Options.Contains(parsedSize))
                    {
                        pageSizeOptionsData.Options.Add(parsedSize);
                    }
                }
            }

            // Parsing was successful when something was parsed out of given string or when given string is empty
            return pageSizeOptionsData.ContainsAll || containsZero || (pageSizeOptionsData.Options.Count > 0) || string.IsNullOrEmpty(pageSizeOptions);
        }

        #endregion "Page size control"

        #region helpers

        /// <summary>
        /// Set CSS on control
        /// </summary>
        private void SetCSS(Control p_control, string p_cssclass, bool p_doset = true)
        {
            if (p_control != null)
            {
                if (p_doset || !String.IsNullOrWhiteSpace(p_cssclass))
                {
                    if (p_control.GetType() == typeof(CMSDropDownList))
                    {
                        ((CMSDropDownList)(p_control)).CssClass = p_cssclass;
                    }
                    else if (p_control.GetType() == typeof(HtmlGenericControl))
                    {
                        ((HtmlGenericControl)p_control).Attributes["class"] = p_cssclass;
                    }
                    else if (p_control.GetType() == typeof(Label))
                    {
                        ((Label)p_control).CssClass = p_cssclass;
                    }
                }
            }
        }

        #endregion helpers

        #endregion "Methods"
    }
}