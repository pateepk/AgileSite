using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{

    /// <summary>
    /// UniPager control.
    /// </summary>
    [ToolboxData("<{0}:UniPager runat=server></{0}:UniPager>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class UniPager : CMSWebControl, INamingContainer, IPostBackEventHandler
    {
        #region "Events"

        /// <summary>
        /// Page changed delegate method.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="pageNumber">Current page number</param>
        public delegate void PageChangedHandler(object sender, int pageNumber);


        /// <summary>
        /// Occurs when page is changed.
        /// </summary>
        public event PageChangedHandler OnPageChanged;


        /// <summary>
        /// Occurs before pager templates are loaded.
        /// </summary>
        public event EventHandler<EventArgs> OnBeforeTemplateLoading;

        #endregion


        #region "Variables"

        // ID of the control to page
        private string mPageControl;

        // ID of the filter control to page
        private string mFilterTypePageControl;

        // ID of the direct page control in template
        private string mDirectPageControlID = "directPageControl";

        // Array of supported direct page control types
        private readonly Type[] mDirectPageControlTypes = new Type[] { typeof(TextBox), typeof(DropDownList) };

        // Paged data source
        private PagedDataSource pd = new PagedDataSource();

        // Indicates whether pager was generated in current request
        private bool generated;

        // Indicates whether control is bounded
        private bool mTargetControlBounded;

        // Indicates whether page should be hidden for single page

        /// <summary>
        /// Related data is loaded.
        /// </summary>
        protected bool mRelatedDataLoaded;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData;

        /// <summary>
        /// Default envelope tag.
        /// </summary>
        protected HtmlTextWriterTag mEnvelopeTag = HtmlTextWriterTag.Div;

        /// <summary>
        /// Paged control
        /// </summary>
        private IUniPageable mPagedControl;

        /// <summary>
        /// Indicates if pager should be hidden if there is only single page.
        /// </summary>
        private bool mHidePagerForSinglePage = true;

        /// <summary>
        /// Default envelope rendering mode.
        /// </summary>
        protected HtmlEnvelopeRenderingMode mHTMLEnvelopeRenderingMode = HtmlEnvelopeRenderingMode.OnlyForUpdatePanel;

        private string mPreviousGroupText = "...";
        private string mNextGroupText = "...";
        private string mPreviousPageText = ResHelper.GetString("unigridpager.previouspage");
        private string mNextPageText = ResHelper.GetString("unigridpager.nextpage");
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the key used for values stored within current request
        /// </summary>
        private string RequestKey
        {
            get
            {
                return FilterTypePageControl + "_" + PageControl + "_currentPage";
            }
        }


        /// <summary>
        /// Reset scroll position on post back action
        /// </summary>
        public bool ResetScrollPositionOnPostBack
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether links to the first page should be generated with querystring parameter 
        /// </summary>
        public bool UseQueryParameterForFirstPage
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the HTML envelope rendering mode for current pager.
        /// </summary>
        public HtmlEnvelopeRenderingMode HTMLEnvelopeRenderingMode
        {
            get
            {
                return mHTMLEnvelopeRenderingMode;
            }
            set
            {
                mHTMLEnvelopeRenderingMode = value;
            }
        }


        /// <summary>
        /// Gets or sets the current envelope tag.
        /// </summary>
        public HtmlTextWriterTag EnvelopeTag
        {
            get
            {
                return mEnvelopeTag;
            }
            set
            {
                mEnvelopeTag = value;
            }
        }


        /// <summary>
        /// Gets or sets fake mode for unipager
        /// </summary>
        private bool FakeMode
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["FakeMode"], false);
            }
            set
            {
                ViewState["FakeMode"] = value;
            }
        }


        /// <summary>
        /// Custom data connected to the object, if not set, returns the Related data of the nearest IDataControl.
        /// </summary>
        public virtual object RelatedData
        {
            get
            {
                if ((mRelatedData == null) && !mRelatedDataLoaded)
                {
                    // Load the related data to the object
                    mRelatedDataLoaded = true;
                    IRelatedData dataControl = (IRelatedData)ControlsHelper.GetParentControl(this, typeof(IRelatedData));
                    if (dataControl != null)
                    {
                        mRelatedData = dataControl.RelatedData;
                    }
                }

                return mRelatedData;
            }
            set
            {
                mRelatedData = value;
            }
        }


        /// <summary>
        /// Paged control.
        /// </summary>
        public IUniPageable PagedControl
        {
            get
            {
                BoundTargetControl(false);
                return mPagedControl;
            }
            set
            {
                mPagedControl = value;
                BoundTargetControl(true);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the control to page.
        /// </summary>
        [DefaultValue(""), TypeConverter(typeof(ControlIDConverter))]
        public string PageControl
        {
            get
            {
                return mPageControl;
            }
            set
            {
                mPageControl = value;
                BoundTargetControl(true);
            }
        }


        /// <summary>
        /// Gets or sets the ID of direct page control in template.
        /// </summary>
        [DefaultValue(""), TypeConverter(typeof(ControlIDConverter))]
        public string DirectPageControlID
        {
            get
            {
                return mDirectPageControlID;
            }
            set
            {
                mDirectPageControlID = value;
            }
        }


        /// <summary>
        /// Gets or sets the ID of the control to page.
        /// </summary>
        [DefaultValue("")]
        public string FilterTypePageControl
        {
            get
            {
                return mFilterTypePageControl;
            }
            set
            {
                mFilterTypePageControl = value;
                BoundTargetControl(true);
            }
        }


        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["PageSize"], 10);
            }
            set
            {
                ViewState["PageSize"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the pager querystring key.
        /// </summary>
        public string QueryStringKey
        {
            get
            {
                return DataHelper.GetNotEmpty(ViewState["QueryStringKey"], "Page");
            }
            set
            {
                ViewState["QueryStringKey"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the pager mode.
        /// </summary>
        public UniPagerMode PagerMode
        {
            get
            {
                if (ViewState["PagerMode"] == null)
                {
                    ViewState["PagerMode"] = UniPagerMode.Querystring;
                }
                return (UniPagerMode)ViewState["PagerMode"];
            }
            set
            {
                ViewState["PagerMode"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the count of items in datasource.
        /// </summary>
        public int DataSourceItemsCount
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["DataSourceItemsCount"], 0);
            }
            set
            {
                ViewState["DataSourceItemsCount"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the current group item size.
        /// </summary>
        public int ItemsCount
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ItemsCount"], 1);
            }
            set
            {
                ViewState["ItemsCount"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the current number of pages.
        /// </summary>
        public int PageCount
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["PageCount"], 1);
            }
            set
            {
                ViewState["PageCount"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the current pages number.
        /// </summary>
        public int CurrentPage
        {
            get
            {
                int currentPage = ValidationHelper.GetInteger(ViewState["CurrentPage"], 0);
                if (currentPage <= 0)
                {
                    currentPage = GetCurrentPage();
                    ViewState["CurrentPage"] = currentPage;
                }

                return currentPage;
            }
            set
            {
                ViewState["CurrentPage"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the current pages number.
        /// </summary>
        public int GroupSize
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["GroupSize"], 5);
            }
            set
            {
                ViewState["GroupSize"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the max. processed pages (only for query string mode and fake number of results).
        /// </summary>
        public int MaxPages
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["MaxPages"], 0);
            }
            set
            {
                ViewState["MaxPages"] = value;
            }
        }


        /// <summary>
        /// Indicates whether control was generated and should be reloaded on postback.
        /// </summary>
        private bool WasGenerated
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["WasGenerated"], false);
            }
            set
            {
                ViewState["WasGenerated"] = value;
            }
        }

        #endregion


        #region "Public behavior properties"

        /// <summary>
        /// Gets or sets the value that indicates whether first and last items should be displayed only if first or last is not accessible by other way accessible.
        /// </summary>
        public bool DisplayFirstLastAutomatically
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DisplayFirstLastAutomatically"], false);
            }
            set
            {
                ViewState["DisplayFirstLastAutomatically"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether previous and next items should be displayed only if next or previous is not accessible by other way.
        /// </summary>
        public bool DisplayPreviousNextAutomatically
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["DisplayPreviousNextAutomatically"], false);
            }
            set
            {
                ViewState["DisplayPreviousNextAutomatically"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether pager should be hidden for single page.
        /// </summary>
        public bool HidePagerForSinglePage
        {
            get
            {
                return mHidePagerForSinglePage;
            }
            set
            {
                mHidePagerForSinglePage = value;
            }
        }


        /// <summary>
        /// Text displayed in first page link.
        /// </summary>
        public string FirstPageText
        {
            get;
            set;
        }


        /// <summary>
        /// Text displayed in previous page link.
        /// </summary>
        public string PreviousPageText
        {
            get
            {
                return mPreviousPageText;
            }
            set
            {
                mPreviousPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in previous group link.
        /// </summary>
        public string PreviousGroupText
        {
            get
            {
                return mPreviousGroupText;
            }
            set
            {
                mPreviousGroupText = value;
            }
        }


        /// <summary>
        /// Text displayed in next group link.
        /// </summary>
        public string NextGroupText
        {
            get
            {
                return mNextGroupText;
            }
            set
            {
                mNextGroupText = value;
            }
        }


        /// <summary>
        /// Text displayed in next page link.
        /// </summary>
        public string NextPageText
        {
            get
            {
                return mNextPageText;
            }
            set
            {
                mNextPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in last page link.
        /// </summary>
        public string LastPageText
        {
            get;
            set;
        }

        #endregion


        #region "Templates"

        private const string mPageNumbersTemplatePlc = "plcPageNumbers";
        private const string mNextGroupTemplatePlc = "plcNextGroup";
        private const string mPreviousGroupTemplatePlc = "plcPreviousGroup";
        private const string mFirstPageTemplatePlc = "plcFirstPage";
        private const string mLastPageTemplatePlc = "plcLastPage";
        private const string mNextPageTemplatePlc = "plcNextPage";
        private const string mPreviousPageTemplatePlc = "plcPreviousPage";
        private const string mDirectPageTemplatePlc = "plcDirectPage";


        /// <summary>
        /// Page numbers summary>
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PageNumbersTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Current page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate CurrentPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Next group template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate NextGroupTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Previous group template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PreviousGroupTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// First page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate FirstPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Last page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate LastPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Next page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate NextPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Previous page template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PreviousPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Layout template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate LayoutTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Page numbers separator template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate PageNumbersSeparatorTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Direct page control template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate DirectPageTemplate
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes web control default span tag.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[UniPager: " + ID + "]");
                return;
            }

            // Indicates whether HTML envelope should be rendered
            bool renderEnvelope = ((HTMLEnvelopeRenderingMode == HtmlEnvelopeRenderingMode.Always) || ((HTMLEnvelopeRenderingMode == HtmlEnvelopeRenderingMode.OnlyForUpdatePanel) && (ControlsHelper.IsInUpdatePanel(this))));

            // Render envelope start tag
            if (renderEnvelope)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
                writer.RenderBeginTag(EnvelopeTag);
            }

            RenderContents(writer);

            // Render envelope end tag
            if (renderEnvelope)
            {
                writer.RenderEndTag();
            }
        }


        /// <summary>
        /// Connects the pager to the given paged control
        /// </summary>
        /// <param name="pagedControl">Control to connect to</param>
        public void ConnectToPagedControl(IUniPageable pagedControl)
        {
            PagedControl = pagedControl;
            BoundTargetControl(true);
        }


        /// <summary>
        /// Loads data according to the current values of properties.
        /// </summary>
        /// <param name="forceReload">Force the reload</param>
        public void ReloadData(bool forceReload)
        {
            BoundTargetControl(forceReload);
        }


        /// <summary>
        /// Bound target control.
        /// </summary>
        /// <param name="forceBinding">Indicates whether control should be force bounded</param>
        protected void BoundTargetControl(bool forceBinding)
        {
            if (!forceBinding && mTargetControlBounded)
            {
                return;
            }

            IUniPageable ctrl = mPagedControl;

            // Remove event reference if exist
            if (ctrl != null)
            {
                ctrl.OnPageBinding -= dataPager_PageBinding;
            }

            // Try to find IUniPageable control
            if ((ctrl == null) && !String.IsNullOrEmpty(PageControl) && (Parent != null))
            {
                ctrl = Parent.FindControl(PageControl) as IUniPageable;
            }
            if ((ctrl == null) && !String.IsNullOrEmpty(FilterTypePageControl))
            {
                ctrl = CMSControlsHelper.GetFilter(FilterTypePageControl) as IUniPageable;
            }

            if (ctrl != null)
            {
                // Forward the container
                while (ctrl is IUniPageableContainer)
                {
                    IUniPageableContainer container = (IUniPageableContainer)ctrl;
                    if (container.PageableControl != null)
                    {
                        ctrl = container.PageableControl;
                    }
                    else
                    {
                        // There is no such a control
                        break;
                    }
                }

                // Setup the control
                ctrl.UniPagerControl = this;
                ctrl.OnPageBinding += dataPager_PageBinding;

                mTargetControlBounded = true;
            }

            mPagedControl = ctrl;
        }


        /// <summary>
        /// OnInit override.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageContext.InitComplete += PageHelper_InitComplete;
        }


        void PageHelper_InitComplete(object sender, EventArgs e)
        {
            BoundTargetControl(false);
        }


        /// <summary>
        /// OnLoad override.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Bound the target control
            BoundTargetControl(false);
        }


        /// <summary>
        /// OnPreRender.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if ((PagedControl != null) && (PagedControl.PagerDataItem != null))
            {
                // Check whether is postback and pager was generated in next request and control wasn't generated during actual request run
                if (RequestHelper.IsPostBack() && WasGenerated && !generated)
                {
                    // Call PageBinding
                    RebindPager();
                }
            }
        }


        /// <summary>
        /// Returns current page.
        /// </summary>
        protected int GetCurrentPage()
        {
            int currentPage = 1;

            // Querystring mode
            if (PagerMode == UniPagerMode.Querystring)
            {
                // Get current page from query string
                currentPage = QueryHelper.GetInteger(QueryStringKey, 1);
            }
            // Postback mode
            else if (RequestStockHelper.Contains(RequestKey))
            {
                currentPage = ValidationHelper.GetInteger(RequestStockHelper.GetItem(RequestKey), 1);
            }

            // If current page is higher than max. page set current page to max. page
            if ((MaxPages > 0) && (currentPage > MaxPages))
            {
                currentPage = MaxPages;
            }
			else if (currentPage < 1)
			{
				currentPage = 1;
			}

            return currentPage;
        }


        /// <summary>
        /// Calls page binding method for initializing pager control and ensures all necessary actions.
        /// </summary>
        public void RebindPager()
        {
            dataPager_PageBinding(null, null);
        }


        /// <summary>
        /// PageBinding method, initialize pager control and ensures all necessary actions.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">EventArgs</param>
        private void dataPager_PageBinding(object sender, EventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            
            // Clear control collection
            Controls.Clear();

            // Set visibility
            Visible = true;

            // Set page size
            pd.PageSize = PageSize;
            // Allow paging
            pd.AllowPaging = true;

            // Set force number of results if is higher than -1
            var pagerForceNumberOfResults = PagedControl.PagerForceNumberOfResults;
            if (pagerForceNumberOfResults > -1)
            {
                FakeMode = true;
                DataSourceItemsCount = pagerForceNumberOfResults;

                // Check if page size id not zero 
                if (PageSize != 0)
                {
                    PageCount = pagerForceNumberOfResults / PageSize;

                    if ((pagerForceNumberOfResults % PageSize) != 0)
                    {
                        PageCount++;
                    }
                }
            }

            // Check whether exists data
            var pagerDataItem = PagedControl.PagerDataItem;

            if (!FakeMode && DataHelper.DataSourceIsEmpty(pagerDataItem))
            {
                PageCount = 0;
                CurrentPage = 0;
            }
            else
            {
                // Dataview
                if (pagerDataItem is DataView)
                {
                    pd.DataSource = (DataView)pagerDataItem;
                }
                // DataSet
                else if (pagerDataItem is DataSet)
                {
                    pd.DataSource = ((DataSet)pagerDataItem).Tables[0].DefaultView;
                }
                // Data table
                else if (pagerDataItem is DataTable)
                {
                    pd.DataSource = ((DataTable)pagerDataItem).DefaultView;
                }
                // PagedDataSource
                else if (pagerDataItem is PagedDataSource)
                {
                    pd = (PagedDataSource)pagerDataItem;
                }
                // GroupedDataSource
                else if (pagerDataItem is GroupedDataSource)
                {
                    pd.DataSource = ((GroupedDataSource)pagerDataItem).DataSource as IEnumerable;
                }
                // IDataQuery
                else if (pagerDataItem is IDataQuery)
                {
                    pd.DataSource = ((IDataQuery)pagerDataItem).Result.Tables[0].DefaultView;
                }
                // ICollection
                else if (pagerDataItem is ICollection)
                {
                    pd.DataSource = (ICollection)pagerDataItem;
                }
                // IEnumerable
                else if (pagerDataItem is IEnumerable<object>)
                {
                    // Data source must be an object of type which implements ICollection. PagedDataSource then calls 'Count' property on the data source object.
                    pd.DataSource = new List<object>((IEnumerable<object>)pagerDataItem);
                }

                if (!FakeMode)
                {
                    PageCount = pd.PageCount;
                }
            }

            // Check whether current index is set in the request
            // Get current page from query string
            if (PagerMode == UniPagerMode.Querystring)
            {
                // Get current page value from query string
                CurrentPage = QueryHelper.GetInteger(QueryStringKey, 1);


                // Check whether selected page is in available interval
                if ((CurrentPage > 0) && (CurrentPage <= PageCount))
                {
                    if (!FakeMode)
                    {
                        // Set page to the paged datasource
                        pd.CurrentPageIndex = CurrentPage - 1;
                    }
                }
                // Set first page by default
                else
                {
                    if (FakeMode)
                    {
                        CurrentPage = PageCount;
                    }
                    else
                    {
                        CurrentPage = 1;
                    }

                    pd.CurrentPageIndex = 0;
                }

                // Check whether max. pages value is defined
                if (MaxPages > 0)
                {
                    // Check whether current page is higher than max. page value
                    if (CurrentPage > MaxPages)
                    {
                        // Set current page value to the
                        CurrentPage = MaxPages;

                        // Set paged datasource index
                        if (!FakeMode)
                        {
                            // Set page to the paged datasource
                            pd.CurrentPageIndex = CurrentPage - 1;
                        }
                    }
                }
            }
            else if (RequestStockHelper.Contains(RequestKey))
            {
                CurrentPage = ValidationHelper.GetInteger(RequestStockHelper.GetItem(RequestKey), 1);
                // Check whether selected page is in available interval
                if ((CurrentPage > 0) && (CurrentPage <= PageCount))
                {
                    if (!FakeMode)
                    {
                        // Set page to the paged datasource
                        pd.CurrentPageIndex = CurrentPage - 1;
                    }
                }
                // Set first page by default
                else
                {
                    CurrentPage = 1;
                    pd.CurrentPageIndex = 0;
                }

                // Check whether max. pages value is defined
                if (MaxPages > 0)
                {
                    // Check whether current page is higher than max. page value
                    if (CurrentPage > MaxPages)
                    {
                        // Set current page value to the
                        CurrentPage = MaxPages;

                        // Set paged datasource index
                        if (!FakeMode)
                        {
                            // Set page to the paged datasource
                            pd.CurrentPageIndex = CurrentPage - 1;
                        }
                    }
                }
            }
            // If is postback, set current page from viewstate
            else
            {
                if (CurrentPage > 0)
                {
                    if (!FakeMode)
                    {
                        if (CurrentPage > pd.PageCount)
                        {
                            CurrentPage = 1;
                            pd.CurrentPageIndex = 0;
                        }
                        else
                        {
                            pd.CurrentPageIndex = CurrentPage - 1;
                        }
                    }
                }
                else
                {
                    CurrentPage = 1;
                    pd.CurrentPageIndex = 0;
                }
            }

            if (!FakeMode && !DataHelper.DataSourceIsEmpty(pagerDataItem))
            {
                // Set paged datasource to the original control
                PagedControl.PagerDataItem = pd;
                PageCount = pd.PageCount;
                DataSourceItemsCount = pd.DataSourceCount;
            }
            
            // First, Last, Page, Next, Previous, NextGroup, PreviousGroup
            // FirstLink, LastLink, PageLink, NextLink, PreviousLink, NextGroupLink, PreviousGroupLink

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("First");
            dataTable.Columns.Add("Last");
            dataTable.Columns.Add("Page");
            dataTable.Columns.Add("Next");
            dataTable.Columns.Add("Previous");
            dataTable.Columns.Add("NextGroup");
            dataTable.Columns.Add("PreviousGroup");

            dataTable.Columns.Add("FirstURL");
            dataTable.Columns.Add("LastURL");
            dataTable.Columns.Add("PageURL");
            dataTable.Columns.Add("NextURL");
            dataTable.Columns.Add("PreviousURL");

            dataTable.Columns.Add("NextGroupURL");
            dataTable.Columns.Add("PreviousGroupURL");
            dataTable.Columns.Add("CurrentPage");
            dataTable.Columns.Add("Pages");
            dataTable.Columns.Add("Items");
            dataTable.Columns.Add("FirstOnPage");
            dataTable.Columns.Add("LastOnPage");

            string url;

            // Set url with dependence on current mode
            if (PagerMode == UniPagerMode.Querystring)
            {
                // querystring url
                url = URLHelper.UpdateParameterInUrl(RequestContext.CurrentURL, QueryStringKey, "##page##");
            }
            else
            {
                // postback url
                url = "javascript:" + ControlsHelper.GetPostBackEventReference(this, "##page##");
            }

            // Get items count
            ItemsCount = (GroupSize > PageCount) ? PageCount : GroupSize; //((this.Count - firstIndex) / this.GroupSize > 0) ? this.GroupSize : (this.Count - firstIndex);

            //previous group
            int previousGroup = 0;

            if (GroupSize != 0)
            {
                previousGroup = ((CurrentPage - 1) / GroupSize);
            }

            previousGroup = previousGroup * GroupSize;

            // Set items count
            if ((previousGroup + GroupSize > PageCount) && (previousGroup != 0))
            {
                int tmpCount = PageCount - previousGroup;
                if ((tmpCount == GroupSize) && (CurrentPage == PageCount))
                {
                    ItemsCount = 1;
                }
                else
                {
                    ItemsCount = tmpCount;
                }
            }

            // Hide pager if doesn't exist any item or hiding for single page is enabled
            if ((ItemsCount < 1) || (HidePagerForSinglePage && PageCount == 1))
            {
                Visible = false;
                return;
            }


            int firstIndex = previousGroup + 1;
            int previous = (CurrentPage == 1) ? 1 : CurrentPage - 1;
            int next = (CurrentPage == PageCount) ? CurrentPage : CurrentPage + 1;

            dataTable.Rows.Add(new object[19]
                            {
                                "1", // First
                                PageCount, // Last 
                                CurrentPage, // Page
                                next, // Next
                                previous, // Previous
                                previousGroup + GroupSize + 1, // Next group
                                previousGroup, // Previous group
                                EnsureQueryParameter(url, 1), // First page url
                                EnsureQueryParameter(url, PageCount), // Last page url
                                EnsureQueryParameter(url, CurrentPage), // Page url
                                EnsureQueryParameter(url, next), // Next page url
                                EnsureQueryParameter(url, previous), // Previous page url
                                EnsureQueryParameter(url, previousGroup + GroupSize + 1), // Next group url
                                EnsureQueryParameter(url, previousGroup), //previous group url
                                CurrentPage, // Current page
                                PageCount, // Pages
                                DataSourceItemsCount, // Items
                                (CurrentPage - 1) * PageSize + 1, // First in group
                                (CurrentPage == PageCount) ? ((CurrentPage - 1) * PageSize) + (DataSourceItemsCount - ((CurrentPage - 1) * PageSize)) : (CurrentPage - 1) * PageSize + PageSize // Last in group
                            });

            int index = 0;

            if (OnBeforeTemplateLoading != null)
            {
                OnBeforeTemplateLoading(this, null);
            }

            DataPagerItem layoutItem = null;

            if (LayoutTemplate != null)
            {
                layoutItem = new DataPagerItem(dataTable.DefaultView[0], index);
                index++;
                LayoutTemplate.InstantiateIn(layoutItem);
                Controls.Add(layoutItem);
                layoutItem.DataBind();

                // Direct page
                if (DirectPageTemplate != null)
                {
                    // Check whether DirectPageControlID is sets
                    if (!String.IsNullOrEmpty(DirectPageControlID))
                    {
                        ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mDirectPageTemplatePlc), DirectPageTemplate);
                        index++;

                        Control directPageControl;
                        if (layoutItem != null)
                        {
                            // Get Direct page control from template
                            directPageControl = ControlsHelper.GetChildControl(layoutItem, mDirectPageControlTypes, DirectPageControlID);
                        }
                        else
                        {
                            // Get direct page control from current controls set
                            directPageControl = ControlsHelper.GetChildControl(this, mDirectPageControlTypes, DirectPageControlID);
                        }
                        if (directPageControl != null)
                        {
                            // Store direct page control unique ID in viewstate
                            ViewState["DirectPageControlUniqueID"] = directPageControl.UniqueID;

                            string onchange;
                            // Set url with dependence on current mode
                            if (PagerMode == UniPagerMode.Querystring)
                            {
                                // Querystring url
                                var sanitizedCurrentUrl = ScriptHelper.GetString(RequestContext.CurrentURL, encapsulate: false);
                                onchange = "var pageIndex = parseInt(this.value,10);var url = '" + URLHelper.UpdateParameterInUrl(sanitizedCurrentUrl, QueryStringKey, "##PAGE##") + "'; window.location = url.replace('##PAGE##',encodeURIComponent(pageIndex));";
                            }
                            else
                            {
                                // Change the direct pager value of the primary pager. There can be multiple direct pagers (when pager is displayed on the top and bottom of the listing)
                                onchange = @"document.getElementById('" + directPageControl.ClientID + @"').value = this.value; "
                                           + ControlsHelper.GetPostBackEventReference(this, "direct") + ";";
                            }
                            bool isDirectPagerPostback = ((Page.Request.Form[Page.postEventSourceID] == UniqueID) && (Page.Request.Form[Page.postEventArgumentID] == "direct"));
                            if (directPageControl is TextBox)
                            {
                                TextBox txtPageControl = (TextBox)directPageControl;
                                txtPageControl.MaxLength = 9;
                                txtPageControl.AutoPostBack = false;
                                txtPageControl.EnableViewState = false;
                                txtPageControl.Attributes["onchange"] = onchange;
                                txtPageControl.Attributes["onkeydown"] = "var keyCode = event.which; if (keyCode == undefined) { keyCode = event.keyCode; } if (keyCode == 13){" + onchange + " return false;}";
                                txtPageControl.Text = GetPageValue(isDirectPagerPostback, txtPageControl.UniqueID);
                            }
                            else if (directPageControl is DropDownList)
                            {
                                DropDownList drpPageControl = (DropDownList)directPageControl;
                                drpPageControl.AutoPostBack = false;
                                drpPageControl.EnableViewState = false;
                                drpPageControl.Attributes["onchange"] = onchange;
                                string currentPage = GetPageValue(isDirectPagerPostback, drpPageControl.UniqueID);
                                for (int i = 1; i <= PageCount; i++)
                                {
                                    ListItem item = new ListItem(i.ToString());
                                    if (i.ToString() == currentPage)
                                    {
                                        item.Selected = true;
                                    }
                                    drpPageControl.Items.Add(item);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception(String.Format("[UniPager]: Direct page transformation must contain control with ID '{0}'.", DirectPageControlID));
                        }
                    }
                }
            }

            // First page
            if (FirstPageTemplate != null)
            {
                // Check whether first should be displayed automatically => exist page numbers template and first index is higher than 1
                if ((!DisplayFirstLastAutomatically) || (PageNumbersTemplate == null) || ((CurrentPage != 1) && (((firstIndex > GroupSize) && (GroupSize > 1)) || ((firstIndex > 2) && (GroupSize == 1) && (PreviousGroupTemplate != null)))))
                {
                    ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mFirstPageTemplatePlc), FirstPageTemplate);
                    index++;
                }
            }

            // Previous page
            if (PreviousPageTemplate != null)
            {
                // Check whether previous should be displayed automatically => exist page numbers template and current page is first in current group but not first in global
                if ((!DisplayPreviousNextAutomatically) || (PageNumbersTemplate == null) || ((CurrentPage != 1) && (PreviousGroupTemplate == null) && (firstIndex == CurrentPage)))
                {
                    ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mPreviousPageTemplatePlc), PreviousPageTemplate);
                    index++;
                }
            }

            // Previous group
            if ((PreviousGroupTemplate != null) && (previousGroup != 0))
            {
                ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mPreviousGroupTemplatePlc), PreviousGroupTemplate);
                index++;
            }

            Control ctrl = GetInnerControl(layoutItem, mPageNumbersTemplatePlc);

            // Pages
            for (int i = 0; i < ItemsCount; i++)
            {
                // Set current data
                dataTable.Rows[0]["Page"] = firstIndex + i;
                dataTable.Rows[0]["PageUrl"] = EnsureQueryParameter(url, firstIndex + i);

                // If current page template is defined use it
                if ((CurrentPageTemplate != null) && (CurrentPage == firstIndex + i))
                {
                    ProcessItem(dataTable.DefaultView[0], index, ctrl, CurrentPageTemplate);
                    index++;
                }
                // Process standard page template
                else if (PageNumbersTemplate != null)
                {
                    ProcessItem(dataTable.DefaultView[0], index, ctrl, PageNumbersTemplate);
                    index++;
                }

                // Number separator
                if ((PageNumbersSeparatorTemplate != null) && ((i + 1) < ItemsCount))
                {
                    ProcessItem(dataTable.DefaultView[0], index, ctrl, PageNumbersSeparatorTemplate);
                    index++;
                }
            }

            // Next group
            if ((NextGroupTemplate != null) && (previousGroup + GroupSize + 1 <= PageCount))
            {
                ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mNextGroupTemplatePlc), NextGroupTemplate);
                index++;
            }

            // Next page
            if (NextPageTemplate != null)
            {
                // Check whether previous should be displayed automatically => exist page numbers template and current page is last in current group but not last in global
                if ((!DisplayPreviousNextAutomatically) || (PageNumbersTemplate == null) || ((CurrentPage != PageCount) && (NextGroupTemplate == null) && (firstIndex + ItemsCount - 1 == CurrentPage)))
                {
                    dataTable.Rows[0]["Page"] = CurrentPage;
                    dataTable.Rows[0]["PageUrl"] = EnsureQueryParameter(url, CurrentPage);
                    ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mNextPageTemplatePlc), NextPageTemplate);
                    index++;
                }
            }

            // Last page
            if (LastPageTemplate != null)
            {
                // Check whether last should be displayed automatically => exist page numbers template and first index is lower than page count
                if ((!DisplayFirstLastAutomatically) || (PageNumbersTemplate == null) || (firstIndex - 1 + GroupSize < PageCount))
                {
                    ProcessItem(dataTable.DefaultView[0], index, GetInnerControl(layoutItem, mLastPageTemplatePlc), LastPageTemplate);
                    index++;
                }
            }

            WasGenerated = true;
            generated = true;
        }


        /// <summary>
        /// Returns inner control in layout template control collection.
        /// </summary>
        /// <param name="item">Data pager item</param>
        /// <param name="controlName">Control name</param>
        public Control GetInnerControl(DataPagerItem item, string controlName)
        {
            if (LayoutTemplate != null)
            {
                if (item != null)
                {
                    // Try to find placeholder directly
                    Control ctrl = item.FindControl(controlName);
                    // Try to find control in Transformation container
                    if ((ctrl == null) && (item.Controls.Count > 0))
                    {
                        ctrl = item.Controls[0].FindControl(controlName);
                    }

                    return ctrl;
                }
            }
            else
            {
                return this;
            }

            return null;
        }


        /// <summary>
        /// Process item.
        /// </summary>
        /// <param name="data">DataRowView object</param>
        /// <param name="index">Item index</param>
        /// <param name="ctrl">Target control</param>
        /// <param name="template">ITemplate object</param>
        public void ProcessItem(DataRowView data, int index, Control ctrl, ITemplate template)
        {
            if (ctrl != null)
            {
                // Create appropriate datapager item
                DataPagerItem pagerItem = new DataPagerItem(data, index);
                // InstantiateIn template
                template.InstantiateIn(pagerItem);

                // Add to the control collection
                ctrl.Controls.Add(pagerItem);

                // DataBind item
                pagerItem.DataBind();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Ensures that page parameter for first page is displayed only in required conditions
        /// </summary>
        /// <param name="url">URL with ##page## macro </param>
        /// <param name="page">Page number</param>
        private string EnsureQueryParameter(string url, int page)
        {
            if ((page == 1) && (PagerMode == UniPagerMode.Querystring) && (!UseQueryParameterForFirstPage))
            {
                url = URLHelper.RemoveParameterFromUrl(url, QueryStringKey);
            }
            else
            {
                url = url.Replace("##page##", page.ToString());
            }

            return url;
        }


        /// <summary>
        /// Gets current page number.
        /// </summary>
        /// <param name="isDirectPagerPostback">Indicates if it is direct page postback</param>
        /// <param name="uniqueID">Unique ID of direct page control</param>
        private string GetPageValue(bool isDirectPagerPostback, string uniqueID)
        {
            if (!isDirectPagerPostback)
            {
                return CurrentPage.ToString();
            }
            else
            {
                int currentPage = ValidationHelper.GetInteger(Page.Request.Form[uniqueID], CurrentPage);
                if (currentPage < 1)
                {
                    return "1";
                }
                else if (currentPage > PageCount)
                {
                    return CurrentPage.ToString();
                }
                else
                {
                    return currentPage.ToString();
                }
            }
        }

        #endregion


        #region "IPostBackEventHandler Members"

        /// <summary>
        /// Raise post back event - handle page change.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {

            int nextPage = 1;

            if (eventArgument.ToLowerCSafe() == "direct")
            {
                Control directPageControl = ControlsHelper.GetChildControl(this, mDirectPageControlTypes, DirectPageControlID);
                if (directPageControl == null)
                {
                    // Use viewstate and current request if control is not created yet
                    if (ViewState["DirectPageControlUniqueID"] != null)
                    {
                        string directPageControlUniqueID = ViewState["DirectPageControlUniqueID"].ToString();
                        nextPage = ValidationHelper.GetInteger(CMSHttpContext.Current.Request.Form[directPageControlUniqueID], CurrentPage);
                    }
                }
                else
                {
                    if (directPageControl is TextBox)
                    {
                        // Get next page from Text box
                        var txtPageControl = (TextBox)directPageControl;
                        nextPage = ValidationHelper.GetInteger(txtPageControl.Text, CurrentPage);
                    }
                    else if (directPageControl is DropDownList)
                    {
                        // Get next page from DropDown list
                        var drpPageControl = (DropDownList)directPageControl;
                        nextPage = ValidationHelper.GetInteger(drpPageControl.SelectedValue, CurrentPage);
                    }

                    // Ensure next page is from current page range
                    if ((nextPage < 1) || (nextPage > PageCount))
                    {
                        if (nextPage < 1)
                        {
                            nextPage = 1;
                        }
                        if (nextPage > PageCount)
                        {
                            nextPage = PageCount;
                        }
                    }
                }
            }
            else
            {
                // Get next page from callback argument
                nextPage = ValidationHelper.GetInteger(eventArgument, 1);
            }

            CurrentPage = nextPage;

            // Add to the request repository new page
            RequestStockHelper.Add(RequestKey, nextPage);

            // Call rebind method
            PagedControl.ReBind();

            if (ControlsHelper.IsInUpdatePanel(this))
            {
                ControlsHelper.UpdateCurrentPanel(this);
            }
            else if (ResetScrollPositionOnPostBack && (Page != null))
            {
                ScriptHelper.ResetScrollPosition(Page);
            }

            // Fire page changed event if is required
            if (OnPageChanged != null)
            {
                OnPageChanged(this, CurrentPage);
            }
        }

        #endregion
    }
}
