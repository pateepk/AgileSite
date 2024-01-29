using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for the UI Pager control
    /// </summary>
    public abstract class UIPager : CMSUserControl
    {
        #region "Nested"

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

        #endregion


        #region "Constants"

        private const string FIRST_PAGE_TEMPLATE_PLC = "plcFirstPage";
        private const string LAST_PAGE_TEMPLATE_PLC = "plcLastPage";
        private const string PREVIOUS_PAGE_TEMPLATE_PLC = "plcPreviousPage";
        private const string NEXT_PAGE_TEMPLATE_PLC = "plcNextPage";
        private const string PREVIOUS_GROUP_TEMPLATE_PLC = "plcPreviousGroup";
        private const string NEXT_GROUP_TEMPLATE_PLC = "plcNextGroup";
        private const string DIRECT_PAGE_TEMPLATE_PLC = "plcDirectPage";


        /// <summary>
        /// Constant for query-string key of page size value. Used with <see cref="PagerMode"/> set to 'QueryString'.
        /// </summary>
        protected const string PAGE_SIZE_QUERYSTRING_KEY = "pagesize";


        /// <summary>
        /// Contains default page size options.
        /// </summary>
        protected const string DEFAULT_PAGE_SIZE_OPTIONS = "5,10,25,50,100";

        #endregion


        #region "Variables"

        private bool mDisplayPager = true;
        private bool? mShowFirstLastButtons;
        private bool mShowPreviousNextButtons = true;
        private bool mShowPreviousNextPageGroup = true;
        private bool? mShowDirectPageControl;
        private bool mShowPageSize = true;
        private string mPageSizeOptions;
        private int mDefaultPageSize;

        #endregion


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
        /// Indicates if first and last button should be visible.
        /// </summary>
        public bool ShowFirstLastButtons
        {
            get
            {
                if (mShowFirstLastButtons == null)
                {
                    mShowFirstLastButtons = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSListingShowFirstLastButtons"], false);
                }
                return (mShowFirstLastButtons != false);
            }
            set
            {
                mShowFirstLastButtons = value;
            }
        }


        /// <summary>
        /// Indicates if previous and next button should be visible.
        /// </summary>
        public bool ShowPreviousNextButtons
        {
            get
            {
                return mShowPreviousNextButtons;
            }
            set
            {
                mShowPreviousNextButtons = value;
            }
        }


        /// <summary>
        /// Indicates if previous and next page group button should be visible.
        /// </summary>
        public bool ShowPreviousNextPageGroup
        {
            get
            {
                return mShowPreviousNextPageGroup;
            }
            set
            {
                mShowPreviousNextPageGroup = value;
            }
        }


        /// <summary>
        /// Indicates if direct page control should be visible.
        /// </summary>
        public bool ShowDirectPageControl
        {
            get
            {
                if (mShowDirectPageControl == null)
                {
                    mShowDirectPageControl = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSListingShowDirectPageControl"], true);
                }
                return (mShowDirectPageControl != false);
            }
            set
            {
                mShowDirectPageControl = value;
            }
        }


        /// <summary>
        /// Indicates if page size dropdown list should be visible.
        /// </summary>
        public bool ShowPageSize
        {
            get
            {
                return mShowPageSize;
            }
            set
            {
                mShowPageSize = value;
            }
        }


        /// <summary>
        /// Page size values separates with comma. 
        /// Macro ##ALL## indicates that all the results will be displayed at one page.
        /// </summary>
        public virtual string PageSizeOptions
        {
            get
            {
                if (string.IsNullOrEmpty(mPageSizeOptions))
                {
                    mPageSizeOptions = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSDefaultPageSizeOptions"], DEFAULT_PAGE_SIZE_OPTIONS);
                }

                return mPageSizeOptions;
            }
            set
            {
                mPageSizeOptions = value;
            }
        }


        /// <summary>
        /// Default page size at first load.
        /// </summary>
        public virtual int DefaultPageSize
        {
            get
            {
                if ((mDefaultPageSize <= 0) && (mDefaultPageSize != -1))
                {
                    mDefaultPageSize = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDefaultListingPageSize"], 25);
                }
                return mDefaultPageSize;
            }
            set
            {
                mDefaultPageSize = value;
            }
        }


        /// <summary>
        /// Returns current page.
        /// </summary>
        public int CurrentPage
        {
            get
            {
                return UniPager.CurrentPage;
            }
            set
            {
                UniPager.CurrentPage = value;
        }
        }


        /// <summary>
        /// Gets or sets current page size.
        /// </summary>
        public virtual int CurrentPageSize
        {
            get
            {
                return UniPager.PageSize;
            }
            set
            {
                UniPager.PageSize = value;
            }
        }


        /// <summary>
        /// Number of pages visible in one group of pages.
        /// </summary>
        public int GroupSize
        {
            get
            {
                return UniPager.GroupSize;
            }
            set
            {
                UniPager.GroupSize = value;
            }
        }


        /// <summary>
        /// Paged control for UniPager.
        /// </summary>
        public IUniPageable PagedControl
        {
            get
            {
                return UniPager.PagedControl;
            }
            set
            {
                UniPager.PagedControl = value;
                UniPager.PageControl = UniqueID;
            }
        }


        /// <summary>
        /// Direct page controls ID for UniPager control..
        /// </summary>
        public string DirectPageControlID
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DirectPageControlID"], string.Empty);
            }
            set
            {
                ViewState["DirectPageControlID"] = value;
                UniPager.DirectPageControlID = value;
            }
        }


        /// <summary>
        /// UniPager control.
        /// </summary>
        public virtual UniPager UniPager
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// PageSize dropdown control.
        /// </summary>
        public virtual DropDownList PageSizeDropdown
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Text displayed in first page link.
        /// </summary>
        public string FirstPageText
        {
            get
            {
                return UniPager.FirstPageText;
            }
            set
            {
                UniPager.FirstPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in previous page link.
        /// </summary>
        public string PreviousPageText
        {
            get
            {
                return UniPager.PreviousPageText;
            }
            set
            {
                UniPager.PreviousPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in previous group link.
        /// </summary>
        public string PreviousGroupText
        {
            get
            {
                return UniPager.PreviousGroupText;
            }
            set
            {
                UniPager.PreviousGroupText = value;
            }
        }


        /// <summary>
        /// Text displayed in next group link.
        /// </summary>
        public string NextGroupText
        {
            get
            {
                return UniPager.NextGroupText;
            }
            set
            {
                UniPager.NextGroupText = value;
            }
        }


        /// <summary>
        /// Text displayed in next page link.
        /// </summary>
        public string NextPageText
        {
            get
            {
                return UniPager.NextPageText;
            }
            set
            {
                UniPager.NextPageText = value;
            }
        }


        /// <summary>
        /// Text displayed in last page link.
        /// </summary>
        public string LastPageText
        {
            get
            {
                return UniPager.LastPageText;
            }
            set
            {
                UniPager.LastPageText = value;
            }
        }


        /// <summary>
        /// String used as query string parameter for paging
        /// </summary>
        public string QueryStringKey
        {
            get
            {
                return UniPager.QueryStringKey;
            }
            set
            {
                UniPager.QueryStringKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether pager should be hidden for single page.
        /// </summary>
        public bool HidePagerForSinglePage
        {
            get
            {
                return UniPager.HidePagerForSinglePage;
            }
            set
            {
                UniPager.HidePagerForSinglePage = value;
            }
        }


        /// <summary>
        /// Gets or sets the pager mode.
        /// </summary>
        public UniPagerMode PagerMode
        {
            get
            {
                
                return UniPager.PagerMode;
            }
            set
            {
                UniPager.PagerMode = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnPreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Try to reload data if paged control contains more data than page size
            if ((UniPager.Controls.Count == 0) && (UniPager.PagedControl != null) && (UniPager.PageSize > 0) && (UniPager.PagedControl.PagerForceNumberOfResults > UniPager.PageSize))
            {
                UniPager.Enabled = true;
                UniPager.RebindPager();
            }

            base.OnPreRender(e);

            // First page placeholder
            Control plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), FIRST_PAGE_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowFirstLastButtons;
            }
            // Last page placeholder
            plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), LAST_PAGE_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowFirstLastButtons;
            }
            // Previous page placeholder
            plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), PREVIOUS_PAGE_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowPreviousNextButtons;
            }
            // Next page placeholder
            plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), NEXT_PAGE_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowPreviousNextButtons;
            }
            // Previous group placeholder
            plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), PREVIOUS_GROUP_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowPreviousNextPageGroup;
            }
            // Next group placeholder
            plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), NEXT_GROUP_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowPreviousNextPageGroup;
            }
            // Direct page placeholder
            plcHolder = ControlsHelper.GetChildControl(UniPager, typeof(PlaceHolder), DIRECT_PAGE_TEMPLATE_PLC);
            if (plcHolder != null)
            {
                plcHolder.Visible = ShowDirectPageControl;
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


        /// <summary>
        /// Resets the current paging.
        /// </summary>
        public void Reset()
        {
            UniPager.CurrentPage = 1;
        }

        #endregion
    }
}