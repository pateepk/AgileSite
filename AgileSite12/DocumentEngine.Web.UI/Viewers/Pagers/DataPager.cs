using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control for supporting the data paging within the databound controls.
    /// </summary>
    [Serializable]
    [RefreshProperties(RefreshProperties.Repaint)]
    [ParseChildren(false)]
    [PersistChildren(true)]
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:DataPager runat=server></{0}:DataPager>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class DataPager : BasicDataPager, IPostBackEventHandler
    {
        #region "Variables"

        // Keys insert to Url
        private string[,] mInsertKeys;
        // When mInsert = true, Keys insert
        private bool mInsert;

        // Keys to remove from Url
        private string[] mRemoveKeys;

        // When mRemove = true, Keys remove
        private bool mRemove;

        /// <summary>
        /// Enable only 1st load from hidden field.
        /// </summary>
        private bool LoadedFromHiddenField;

        /// <summary>
        /// Render Html.
        /// </summary>
        protected string mRenderedHtml = "";


        /// <summary>
        /// Query string key.
        /// </summary>
        public string mQueryStringKey = "Page";

        /// <summary>
        /// Ignore query string.
        /// </summary>
        protected bool mIgnoreQueryString;

        /// <summary>
        /// Control style.
        /// </summary>
        protected string mControlStyle = "";

        /// <summary>
        /// Control css class.
        /// </summary>
        protected string mControlCssClass = "PagerControl";

        /// <summary>
        /// Section padding.
        /// </summary>
        protected int mSectionPadding = 10;

        /// <summary>
        /// Hide on single page.
        /// </summary>
        protected bool mHideOnSinglePage = true;

        /// <summary>
        /// Results style.
        /// </summary>
        protected string mResultsStyle = "padding-bottom:4px;padding-top:4px;font-weight: bold;";

        /// <summary>
        /// Results CSS class.
        /// </summary>
        protected string mResultsClass = "PagerResults";

        /// <summary>
        /// Results format.
        /// </summary>
        protected string mResultsFormat = "";

        /// <summary>
        /// Results location.
        /// </summary>
        protected ResultsLocationTypeEnum mResultsLocation = ResultsLocationTypeEnum.Top;

        /// <summary>
        /// Number area CSS class.
        /// </summary>
        protected string mPagerNumbersClass = "PagerNumberArea";

        /// <summary>
        /// Label style.
        /// </summary>
        protected string mLabelStyle = "FONT-WEIGHT: bold;";

        /// <summary>
        /// Show label.
        /// </summary>
        protected bool mShowLabel = true;

        /// <summary>
        /// Label text.
        /// </summary>
        protected string mLabelText = "";

        /// <summary>
        /// Page numbers style.
        /// </summary>
        protected string mPageNumbersStyle = "";

        /// <summary>
        /// Page number style.
        /// </summary>
        protected string mPageNumberStyle = "";

        /// <summary>
        /// Page numbers display.
        /// </summary>
        protected PageNumbersDisplayTypeEnum mPageNumbersDisplay = PageNumbersDisplayTypeEnum.Numbers;

        /// <summary>
        /// Page numbers separator.
        /// </summary>
        protected string mPageNumbersSeparator = "-";

        /// <summary>
        /// Show page numbers.
        /// </summary>
        protected bool mShowPageNumbers = true;

        /// <summary>
        /// Use slider.
        /// </summary>
        protected bool mUseSlider = true;

        /// <summary>
        /// Slider size.
        /// </summary>
        protected int mSliderSize = 10;


        /// <summary>
        /// Class of unselected page.
        /// </summary>
        protected string mUnselectedClass = "UnselectedPage";

        /// <summary>
        /// Class of selected page.
        /// </summary>
        protected string mSelectedClass = "SelectedPage";

        /// <summary>
        /// Class of selected next.
        /// </summary>
        protected string mSelectedNextClass = "SelectedNext";

        /// <summary>
        /// Class of unselected next.
        /// </summary>
        protected string mUnselectedNextClass = "UnselectedNext";

        /// <summary>
        /// Class of selected previous.
        /// </summary>
        protected string mSelectedPrevClass = "SelectedPrev";

        /// <summary>
        /// Class of unselected previous.
        /// </summary>
        protected string mUnselectedPrevClass = "UnselectedPrev";


        /// <summary>
        /// Back next style.
        /// </summary>
        protected string mBackNextStyle = "";

        /// <summary>
        /// Back next display.
        /// </summary>
        protected BackNextDisplayTypeEnum mBackNextDisplay = BackNextDisplayTypeEnum.HyperLinks;

        /// <summary>
        /// Back next location.
        /// </summary>
        protected BackNextLocationTypeEnum mBackNextLocation = BackNextLocationTypeEnum.Right;

        /// <summary>
        /// Show FirstLast.
        /// </summary>
        protected bool mShowFirstLast = true;


        /// <summary>
        /// BackText.
        /// </summary>
        protected string mBackText = "";

        /// <summary>
        /// NextText.
        /// </summary>
        protected string mNextText = "";

        /// <summary>
        /// FirstText.
        /// </summary>
        protected string mFirstText = "";

        /// <summary>
        /// LastText.
        /// </summary>
        protected string mLastText = "";

        /// <summary>
        /// Back next button style
        /// </summary>
        protected string mBackNextButtonStyle = "";

        /// <summary>
        /// Back next link separator
        /// </summary>
        protected string mBackNextLinkSeparator = "&nbsp;";

        /// <summary>
        /// Befor pager html.
        /// </summary>
        protected string mPagerHTMLBefore = "";

        /// <summary>
        /// After pager html.
        /// </summary>
        protected string mPagerHTMLAfter = "";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Before pager html.
        /// </summary>
        public string PagerHTMLBefore
        {
            get
            {
                return mPagerHTMLBefore;
            }
            set
            {
                mPagerHTMLBefore = value;
            }
        }


        /// <summary>
        /// After pager html.
        /// </summary>
        public string PagerHTMLAfter
        {
            get
            {
                return mPagerHTMLAfter;
            }
            set
            {
                mPagerHTMLAfter = value;
            }
        }


        /// <summary>
        /// Rendered html ???
        /// </summary>
        [Browsable(false)]
        public string RenderedHtml
        {
            get
            {
                return mRenderedHtml;
            }
        }


        /// <summary>
        /// Pager DataSource.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager")]
        public override object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
                ProcessPageIndex();
            }
        }


        /// <summary>
        /// Query parameter name for the page index.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("Page")]
        public string QueryStringKey
        {
            get
            {
                return mQueryStringKey;
            }
            set
            {
                if (Regex.IsMatch(value, "[a-zA-z]"))
                {
                    mQueryStringKey = value;
                }
            }
        }


        /// <summary>
        /// Indicates if QueryString parameters should be ignored.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(false)]
        public bool IgnoreQueryString
        {
            get
            {
                return mIgnoreQueryString;
            }
            set
            {
                mIgnoreQueryString = value;
            }
        }


        /// <summary>
        /// Pager control style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string PagerControlStyle
        {
            get
            {
                return mControlStyle;
            }
            set
            {
                mControlStyle = value;
            }
        }


        /// <summary>
        /// CSS Class of the pager control.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("PagerControl")]
        public string ControlCssClass
        {
            get
            {
                return mControlCssClass;
            }
            set
            {
                mControlCssClass = value;
            }
        }


        /// <summary>
        /// Section padding.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(10)]
        public int SectionPadding
        {
            get
            {
                return mSectionPadding;
            }
            set
            {
                mSectionPadding = value;
            }
        }


        /// <summary>
        /// If true, the pager is hidden if only one page is displayed.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(true)]
        public bool HideOnSinglePage
        {
            get
            {
                return mHideOnSinglePage;
            }
            set
            {
                mHideOnSinglePage = value;
            }
        }


        /// <summary>
        /// Results style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("padding-bottom:4px;padding-top:4px;font-weight: bold;")]
        public string ResultsStyle
        {
            get
            {
                return mResultsStyle;
            }
            set
            {
                mResultsStyle = value;
            }
        }


        /// <summary>
        /// Results CSS class.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("PagerResults")]
        public string ResultsClass
        {
            get
            {
                return mResultsClass;
            }
            set
            {
                mResultsClass = value;
            }
        }


        /// <summary>
        /// Results location.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Bindable(true)]
        [Category("Data Pager"), DefaultValue(ResultsLocationTypeEnum.Top)]
        public ResultsLocationTypeEnum ResultsLocation
        {
            get
            {
                return mResultsLocation;
            }
            set
            {
                if (Enum.IsDefined(typeof(ResultsLocationTypeEnum), value))
                {
                    mResultsLocation = value;
                }
            }
        }


        /// <summary>
        /// Results text format.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Bindable(true)]
        [Category("Data Pager"), DefaultValue("Displaying results {0}-{1} (of {2})")]
        public string ResultsFormat
        {
            get
            {
                return mResultsFormat;
            }
            set
            {
                mResultsFormat = value.Trim();
            }
        }


        /// <summary>
        /// Pager number area CSS class.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("PagerNumberArea")]
        public string PagerNumberAreaClass
        {
            get
            {
                return mPagerNumbersClass;
            }
            set
            {
                mPagerNumbersClass = value.Trim();
            }
        }


        /// <summary>
        /// Label style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("FONT-WEIGHT: bold;")]
        public string LabelStyle
        {
            get
            {
                return mLabelStyle;
            }
            set
            {
                mLabelStyle = value;
            }
        }


        /// <summary>
        /// Show label.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(true)]
        public bool ShowLabel
        {
            get
            {
                return ValidationHelper.GetBoolean(mShowLabel, true);
            }
            set
            {
                mShowLabel = value;
            }
        }


        /// <summary>
        /// Label text.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("Page:")]
        public string LabelText
        {
            get
            {
                return mLabelText;
            }
            set
            {
                mLabelText = value.Trim();
            }
        }


        /// <summary>
        /// Page numbers display mode.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(PageNumbersDisplayTypeEnum.Numbers)]
        public PageNumbersDisplayTypeEnum PageNumbersDisplay
        {
            get
            {
                return mPageNumbersDisplay;
            }
            set
            {
                if (Enum.IsDefined(typeof(PageNumbersDisplayTypeEnum), value))
                {
                    mPageNumbersDisplay = value;
                }
            }
        }


        /// <summary>
        /// Show page numbers.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(true)]
        public bool ShowPageNumbers
        {
            get
            {
                return mShowPageNumbers;
            }
            set
            {
                mShowPageNumbers = value;
            }
        }


        /// <summary>
        /// Class of unselected page.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("UnselectedPage")]
        public string UnselectedClass
        {
            get
            {
                return mUnselectedClass;
            }
            set
            {
                mUnselectedClass = value;
            }
        }


        /// <summary>
        /// Class of selected page.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("SelectedPage")]
        public string SelectedClass
        {
            get
            {
                return mSelectedClass;
            }
            set
            {
                mSelectedClass = value;
            }
        }


        /// <summary>
        /// Class of unselected next.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("UnselectedNext")]
        public string UnselectedNextClass
        {
            get
            {
                return mUnselectedNextClass;
            }
            set
            {
                mUnselectedNextClass = value;
            }
        }


        /// <summary>
        /// Class of selected next.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("SelectedNext")]
        public string SelectedNextClass
        {
            get
            {
                return mSelectedNextClass;
            }
            set
            {
                mSelectedNextClass = value;
            }
        }


        /// <summary>
        /// Class of unselected prev.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("UnselectedPrev")]
        public string UnselectedPrevClass
        {
            get
            {
                return mUnselectedPrevClass;
            }
            set
            {
                mUnselectedPrevClass = value;
            }
        }


        /// <summary>
        /// Class of selected prev.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("SelectedPrev")]
        public string SelectedPrevClass
        {
            get
            {
                return mSelectedPrevClass;
            }
            set
            {
                mSelectedPrevClass = value;
            }
        }


        /// <summary>
        /// Page numbers separator.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("-")]
        public string PageNumbersSeparator
        {
            get
            {
                return mPageNumbersSeparator;
            }
            set
            {
                mPageNumbersSeparator = value.Trim() == string.Empty ? "&nbsp;" : value.Trim();
            }
        }


        /// <summary>
        /// Page numbers style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string PageNumbersStyle
        {
            get
            {
                return mPageNumbersStyle;
            }
            set
            {
                mPageNumbersStyle = value;
            }
        }


        /// <summary>
        /// Page number style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string PageNumberStyle
        {
            get
            {
                return mPageNumberStyle;
            }
            set
            {
                mPageNumberStyle = value;
            }
        }


        /// <summary>
        /// Use slider.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(true)]
        public bool UseSlider
        {
            get
            {
                return mUseSlider;
            }
            set
            {
                mUseSlider = value;
            }
        }


        /// <summary>
        /// Slider size.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(10)]
        public int SliderSize
        {
            get
            {
                return mSliderSize;
            }
            set
            {
                mSliderSize = value;
            }
        }


        /// <summary>
        /// BackNext style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string BackNextStyle
        {
            get
            {
                return mBackNextStyle;
            }
            set
            {
                mBackNextStyle = value;
            }
        }


        /// <summary>
        /// BackNext button style.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string BackNextButtonStyle
        {
            get
            {
                return mBackNextButtonStyle;
            }
            set
            {
                mBackNextButtonStyle = value;
            }
        }


        /// <summary>
        /// BackNext display.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(BackNextDisplayTypeEnum.HyperLinks)]
        public BackNextDisplayTypeEnum BackNextDisplay
        {
            get
            {
                return mBackNextDisplay;
            }
            set
            {
                if (Enum.IsDefined(typeof(BackNextDisplayTypeEnum), value))
                {
                    mBackNextDisplay = value;
                }
            }
        }


        /// <summary>
        /// BackNext location.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(BackNextLocationTypeEnum.Right)]
        public BackNextLocationTypeEnum BackNextLocation
        {
            get
            {
                return mBackNextLocation;
            }
            set
            {
                if (Enum.IsDefined(typeof(BackNextLocationTypeEnum), value))
                {
                    mBackNextLocation = value;
                }
            }
        }


        /// <summary>
        /// BackNext link separator.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("&nbsp;")]
        public string BackNextLinkSeparator
        {
            get
            {
                return mBackNextLinkSeparator;
            }
            set
            {
                mBackNextLinkSeparator = value.Trim();
            }
        }


        /// <summary>
        /// Show first last labels.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(true)]
        public bool ShowFirstLast
        {
            get
            {
                return mShowFirstLast;
            }
            set
            {
                mShowFirstLast = value;
            }
        }


        /// <summary>
        /// Back button/hyperlink text
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string BackText
        {
            get
            {
                return mBackText;
            }
            set
            {
                mBackText = value.Trim();
            }
        }


        /// <summary>
        /// Next button/hyperlink text
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string NextText
        {
            get
            {
                return mNextText;
            }
            set
            {
                mNextText = value.Trim();
            }
        }


        /// <summary>
        /// First text.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string FirstText
        {
            get
            {
                return mFirstText;
            }
            set
            {
                mFirstText = value.Trim();
            }
        }


        /// <summary>
        /// Last text.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue("")]
        public string LastText
        {
            get
            {
                return mLastText;
            }
            set
            {
                mLastText = value.Trim();
            }
        }


        /// <summary>
        /// Current page number.
        /// </summary>
        public int Current
        {
            get
            {
                if (ViewState["Current"] == null)
                {
                    return 1;
                }
                else
                {
                    return Convert.ToInt32(ViewState["Current"]);
                }
            }
            set
            {
                ViewState["Current"] = value;
            }
        }

        #endregion


        #region "Viewstate management"

        /// <summary>
        /// Save ViewState.
        /// </summary>
        protected override object SaveViewState()
        {
            if (Context == null)
            {
                return base.SaveViewState();
            }

            // Save State as a cumulative array of objects.
            object baseState = base.SaveViewState();
            object[] allStates = new object[35];
            allStates[0] = baseState;

            //Data Related
            allStates[1] = mMaxPages;
            allStates[2] = CurrentPage;

            //Behavior Related
            allStates[3] = mPagingMode;
            allStates[4] = mQueryStringKey;

            //Control
            allStates[5] = mControlStyle;
            allStates[6] = mControlCssClass;
            allStates[7] = mSectionPadding;
            allStates[8] = mHideOnSinglePage;
            //allStates[9] = mBindToControl;

            //Results Info
            allStates[10] = mResultsStyle;
            allStates[11] = mResultsFormat;
            allStates[12] = mResultsLocation;

            //Label
            allStates[13] = mLabelStyle;
            allStates[14] = mShowLabel;
            allStates[15] = mLabelText;

            //Page Numbers
            allStates[16] = mPageNumbersStyle;
            allStates[17] = mPageNumbersDisplay;
            allStates[18] = mPageNumbersSeparator;
            allStates[19] = mShowPageNumbers;

            //Back/Next Section
            allStates[20] = mBackNextStyle;
            allStates[21] = mBackNextDisplay;
            allStates[22] = mBackNextLocation;
            allStates[23] = mShowFirstLast;

            allStates[24] = mBackText;
            allStates[25] = mNextText;
            allStates[26] = mFirstText;
            allStates[27] = mLastText;

            //Buttons Version
            allStates[28] = mBackNextButtonStyle;

            //Link version
            allStates[29] = mBackNextLinkSeparator;

            //Additional Items
            allStates[30] = mUseSlider;
            allStates[31] = mSliderSize;

            allStates[32] = mIgnoreQueryString;
            allStates[33] = mRenderedHtml;
            allStates[34] = mPageNumberStyle;

            return allStates;
        }


        /// <summary>
        /// Event is raised after view state load.
        /// </summary>
        public event EventHandler ViewStateLoaded;


        /// <summary>
        /// Load ViewState.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            if (Context == null)
            {
                return;
            }

            if (savedState != null)
            {
                // Load State from the array of objects that was saved at ;
                // SavedViewState.

                object[] allStates = (object[])savedState;

                if (allStates[0] != null)
                {
                    base.LoadViewState(allStates[0]);
                }

                //Data Related
                if (allStates[1] != null)
                {
                    mMaxPages = (int)allStates[1];
                }

                //Behavior Related
                if (allStates[3] != null)
                {
                    mPagingMode = (PagingModeTypeEnum)allStates[3];
                }
                if (allStates[4] != null)
                {
                    mQueryStringKey = (string)allStates[4];
                }

                //Control
                if (allStates[5] != null)
                {
                    mControlStyle = (string)allStates[5];
                }
                if (allStates[6] != null)
                {
                    mControlCssClass = (string)allStates[6];
                }
                if (allStates[7] != null)
                {
                    mSectionPadding = (int)allStates[7];
                }
                if (allStates[8] != null)
                {
                    mHideOnSinglePage = (bool)allStates[8];
                }
                //if (allStates[9] != null) mBindToControl = (string)allStates[9];

                //Results Info
                if (allStates[10] != null)
                {
                    mResultsStyle = (string)allStates[10];
                }
                if (allStates[11] != null)
                {
                    mResultsFormat = (string)allStates[11];
                }
                if (allStates[12] != null)
                {
                    mResultsLocation = (ResultsLocationTypeEnum)allStates[12];
                }

                //Label
                if (allStates[13] != null)
                {
                    mLabelStyle = (string)allStates[13];
                }
                if (allStates[14] != null)
                {
                    mShowLabel = (bool)allStates[14];
                }
                if (allStates[15] != null)
                {
                    mLabelText = (string)allStates[15];
                }

                //Page Numbers
                if (allStates[16] != null)
                {
                    mPageNumbersStyle = (string)allStates[16];
                }
                if (allStates[17] != null)
                {
                    mPageNumbersDisplay = (PageNumbersDisplayTypeEnum)allStates[17];
                }
                if (allStates[18] != null)
                {
                    mPageNumbersSeparator = (string)allStates[18];
                }
                if (allStates[19] != null)
                {
                    mShowPageNumbers = (bool)allStates[19];
                }

                //Back/Next Section
                if (allStates[20] != null)
                {
                    mBackNextStyle = (string)allStates[20];
                }
                if (allStates[21] != null)
                {
                    mBackNextDisplay = (BackNextDisplayTypeEnum)allStates[21];
                }
                if (allStates[22] != null)
                {
                    mBackNextLocation = (BackNextLocationTypeEnum)allStates[22];
                }
                if (allStates[23] != null)
                {
                    mShowFirstLast = (bool)allStates[23];
                }

                if (allStates[24] != null)
                {
                    mBackText = (string)allStates[24];
                }
                if (allStates[25] != null)
                {
                    mNextText = (string)allStates[25];
                }
                if (allStates[26] != null)
                {
                    mFirstText = (string)allStates[26];
                }
                if (allStates[27] != null)
                {
                    mLastText = (string)allStates[27];
                }

                //Buttons Version
                if (allStates[28] != null)
                {
                    mBackNextButtonStyle = (string)allStates[28];
                }

                //Link version
                if (allStates[29] != null)
                {
                    mBackNextLinkSeparator = (string)allStates[29];
                }

                //Additional 
                if (allStates[30] != null)
                {
                    mUseSlider = (bool)allStates[30];
                }
                if (allStates[31] != null)
                {
                    mSliderSize = (int)allStates[31];
                }
                if (allStates[32] != null)
                {
                    mIgnoreQueryString = (bool)allStates[32];
                }
                if (allStates[33] != null)
                {
                    mRenderedHtml = (string)allStates[33];
                }
                if (allStates[34] != null)
                {
                    mPageNumberStyle = (string)allStates[34];
                }

                if (allStates[2] != null)
                {
                    CurrentPage = (int)allStates[2];
                }

                LoadedFromHiddenField = true;

                ViewStateLoaded?.Invoke(this, null);
            }
        }

        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        public DataPager()
        {
            QueryStringKey = "";

            if (Context != null)
            {
                mResultsFormat = HttpUtility.HtmlEncode(ResHelper.GetString("DataPager.ResultText"));
                mLabelText = HttpUtility.HtmlEncode(ResHelper.GetString("DataPager.LabelText"));
                mBackText = HttpUtility.HtmlEncode(ResHelper.GetString("DataPager.Back"));
                mNextText = HttpUtility.HtmlEncode(ResHelper.GetString("DataPager.Next"));
                mFirstText = HttpUtility.HtmlEncode(ResHelper.GetString("DataPager.First"));
                mLastText = HttpUtility.HtmlEncode(ResHelper.GetString("DataPager.Last"));
            }
        }


        #region "Render/PreRender"

        /// <summary>
        /// Make same action like PreRender function.
        /// </summary>
        public void ProcessPageIndex()
        {
            if (Page?.Request.Params[ClientID + "_cpage"] != null && !LoadedFromHiddenField)
            {
                LoadedFromHiddenField = true;
                CurrentPage = ValidationHelper.GetInteger(Page.Request.Params[ClientID + "_cpage"], 1);
            }

            if (Context == null)
            {
                return;
            }

            if (PagingMode == PagingModeTypeEnum.QueryString)
            {
                BindToQueryString();
            }

            //If only 1 page, then hide and exit.
            if ((PageCount <= 1) && HideOnSinglePage)
            {
            }
            else
            {
                BuildControl();
            }
        }


        /// <summary> 
        /// Render this control to the output parameter specified.
        /// </summary>
        /// <param name="output">The HTML writer to write out to</param>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[DataPager: " + ID + "]");
                return;
            }

            if ((PageCount <= 1) && HideOnSinglePage)
            {
            }
            else
            {
                ProcessPageIndex();
                output.Write(mRenderedHtml);
                output.Write("<input type=\"hidden\" value=\"" + CurrentPage + "\" name=\"" + ClientID + "_cpage\" />");
                base.Render(output);
            }
        }

        #endregion


        #region "Build Methods (builds mRenderedHtml string)"

        /// <summary>
        /// Build control.
        /// </summary>
        public virtual void BuildControl()
        {
            StringBuilder html = new StringBuilder(256);

            string backNextNavBack = string.Empty;
            string backNextNavSeparator = string.Empty;
            string backNextNavNext = string.Empty;

            var results = BuildResults(ResultsFormat);

            if (PageCount == 0 || Context == null)
            {
                if (ResultsLocation != ResultsLocationTypeEnum.None)
                {
                    results = string.Format(ResultsFormat, "1", PageSize, (MaxPages * PageSize));
                }
            }
            else
            {
                //Make Results (if needed)
                if (ResultsLocation != ResultsLocationTypeEnum.None)
                {
                    results = string.Format(ResultsFormat, RecordStart, RecordEnd, TotalRecords);
                }
            }

            //Make Back & Next section (if needed)
            if (BackNextLocation != BackNextLocationTypeEnum.None)
            {
                if (BackNextDisplay == BackNextDisplayTypeEnum.Buttons)
                {
                    backNextNavBack = BuildButtonNavBack(CurrentPage);
                    backNextNavSeparator += "&nbsp;&nbsp;";
                    backNextNavNext += BuildButtonNavNext(CurrentPage, PageCount);
                }
                else
                {
                    backNextNavBack = BuildLinkNavBack(CurrentPage);
                    backNextNavSeparator += $" {BackNextLinkSeparator} ";
                    backNextNavNext += BuildLinkNavNext(CurrentPage, PageCount);
                }
            }


            html.Append(PagerHTMLBefore);

            html.Append("<div");
            if (mControlStyle != null && mControlStyle.Trim() != "")
            {
                html.AppendFormat(" style=\"{0}\"", mControlStyle);
            }

            if (mControlCssClass != null && mControlCssClass.Trim() != "")
            {
                html.AppendFormat(" class=\"{0}\"", mControlCssClass);
            }

            html.Append(">");

            // TOP Results
            if (ResultsLocation == ResultsLocationTypeEnum.Top)
            {
                html.Append(BuildResultsArea(results));
            }

            // MIDDLE
            html.Append("<div");
            if (mPagerNumbersClass != null && mPagerNumbersClass.Trim() != "")
            {
                html.AppendFormat(" class=\"{0}\"", mPagerNumbersClass);
            }
            html.Append(">");

            if (mBackNextStyle != null && mBackNextStyle.Trim() != "")
            {
                html.AppendFormat("<span style=\"{0}\">", mBackNextStyle);
            }
            else
            {
                html.Append("<span>");
            }
            html.Append(backNextNavBack);

            if (BackNextLocation == BackNextLocationTypeEnum.Left)
            {
                html.Append(backNextNavSeparator);
            }
            html.Append("</span>");

            //Make Page numbers (if needed)
            if (ShowPageNumbers)
            {
                if (ShowLabel)
                {
                    html.Append("<span style=\"padding-left:5px;\"></span>");
                }
                else
                {
                    html.AppendFormat("<span style=\"padding-left:{0}px;\"></span>", SectionPadding);
                }

                var currentPages = BuildPageNumbers(CurrentPage, PageCount);
                if (mPageNumbersStyle != null && mPageNumbersStyle.Trim() != "")
                {
                    html.AppendFormat("<span style=\"{0};\">", mPageNumbersStyle);
                }
                else
                {
                    html.Append("<span>");
                }
                html.Append(currentPages);
                html.Append("</span>");
            }

            html.AppendFormat("<span style=\"padding-left:{0}px;\"></span>", SectionPadding);

            if (mBackNextStyle != null && mBackNextStyle.Trim() != "")
            {
                html.AppendFormat("<span style=\"{0};\">", mBackNextStyle);
            }
            else
            {
                html.Append("<span>");
            }


            html.Append(backNextNavNext);
            html.Append("</span>");

            html.Append("</div>");

            // Bottom
            if (ResultsLocation == ResultsLocationTypeEnum.Bottom)
            {
                html.Append(BuildResultsArea(results));
            }

            html.Append("</div>");

            html.Append(PagerHTMLAfter);

            mRenderedHtml = html.ToString();
        }


        /// <summary>
        /// Build results.
        /// </summary>
        /// <param name="resultsHTML">HTML with the results</param>
        protected virtual string BuildResultsArea(string resultsHTML)
        {
            StringBuilder mBuildRes = new StringBuilder(resultsHTML.Length + 100);

            mBuildRes.Append("<div");
            if (!String.IsNullOrWhiteSpace(mResultsStyle))
            {
                mBuildRes.AppendFormat(" style=\"{0}\"", mResultsStyle);
            }
            if (!String.IsNullOrWhiteSpace(mResultsClass))
            {
                mBuildRes.AppendFormat(" class=\"{0}\"", mResultsClass);
            }
            mBuildRes.Append(">");

            mBuildRes.Append(resultsHTML);
            mBuildRes.Append("</div>");

            return mBuildRes.ToString();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Bind to query.
        /// </summary>
        public virtual void BindToQueryString()
        {
            if ((!RemoveFromUrl) && Context?.Request.QueryString[QueryStringKey] != null && !IgnoreQueryString)
            {
                if (ValidationHelper.IsInteger(Context.Request.QueryString[QueryStringKey]))
                {
                    int newPage = int.Parse(Context.Request.QueryString.Get(QueryStringKey));
                    ChangePage(newPage);
                }
            }
        }


        /// <summary>
        /// Change page.
        /// </summary>
        public virtual void ChangePage(int newPage)
        {
            CurrentPage = newPage;
        }


        /// <summary>
        /// Build page number links.
        /// </summary>
        protected virtual string BuildPageNumbers(int currentPage, int pageCount)
        {
            StringBuilder pages = new StringBuilder(pageCount * 50);

            string pageNumberStyleTag = "";
            string pageNumberClassTag = "";

            if (!String.IsNullOrWhiteSpace(mUnselectedClass))
            {
                pageNumberClassTag = $" class=\"{mUnselectedClass}\"";
            }

            if (!String.IsNullOrWhiteSpace(mPageNumbersStyle))
            {
                pageNumberStyleTag = $" style=\"{mPageNumberStyle};\"";
            }

            int startPage = 1;

            int endPage = pageCount > MaxPages ? MaxPages : pageCount;

            // Adjust for slider
            if (UseSlider)
            {
                int half = (int)Math.Floor((double)(SliderSize - 1) / 2);
                int numAbove = currentPage + half + ((SliderSize - 1) % 2);
                int numBelow = currentPage - half;

                if (numBelow < 1)
                {
                    numAbove += (1 - numBelow);
                    numBelow = 1;
                }

                if (numAbove > endPage)
                {
                    numBelow -= (numAbove - endPage);

                    if (numBelow < 1)
                    {
                        numBelow = 1;
                    }

                    numAbove = endPage;
                }
                startPage = numBelow;
                endPage = numAbove;
            }

            pages.Append("&nbsp;");
            for (int i = startPage; i <= endPage; i++)
            {
                int recordStart;
                int recordEnd;

                //For Display
                if (Context == null)
                {
                    recordStart = ((i - 1) * PagedData.PageSize) + 1;
                    recordEnd = i * PagedData.PageSize;

                    if (i > startPage)
                    {
                        if (PageNumbersSeparator.Equals("&nbsp;", StringComparison.OrdinalIgnoreCase))
                        {
                            pages.AppendFormat("{0}", PageNumbersSeparator);
                        }
                        else
                        {
                            pages.AppendFormat(" {0} ", PageNumbersSeparator);
                        }
                    }

                    if (i == startPage)
                    {
                        string mSelect = "<strong>" + i + "</strong>";

                        if (!String.IsNullOrWhiteSpace(mSelectedClass))
                        {
                            mSelect = "<span class=\"" + mSelectedClass + "\">" + i + "</span>";
                        }

                        if (PageNumbersDisplay == PageNumbersDisplayTypeEnum.Numbers)
                        {
                            pages.Append(mSelect);
                        }
                        else
                        {
                            pages.Append("[<strong>{0}-{1}</strong>]", recordStart, recordEnd);
                        }
                    }
                    else
                    {
                        if (PageNumbersDisplay == PageNumbersDisplayTypeEnum.Numbers)
                        {
                            pages.AppendFormat("<a href=\"#\">{0}</a>", i);
                        }
                        else
                        {
                            pages.AppendFormat("[<a href=\"#\">{0}-{1}</a>]", recordStart, recordEnd);
                        }
                    }
                }
                else
                {
                    //For Runtime.

                    //Figure out RecordStart
                    recordStart = ((i - 1) * PagedData.PageSize) + 1;

                    //Figure out RecordEnd
                    if (i * PagedData.PageSize > PagedData.DataSourceCount)
                    {
                        recordEnd = PagedData.DataSourceCount;
                    }
                    else if (PagedData.Count == PagedData.PageSize)
                    {
                        recordEnd = (recordStart - 1) + PagedData.PageSize;
                    }
                    else
                    {
                        recordEnd = (recordStart - 1) + PagedData.Count;
                    }

                    if (i > startPage)
                    {
                        if (PageNumbersSeparator.Equals("&nbsp;", StringComparison.OrdinalIgnoreCase))
                        {
                            pages.AppendFormat("{0}", PageNumbersSeparator);
                        }
                        else
                        {
                            pages.AppendFormat(" {0} ", PageNumbersSeparator);
                        }
                    }

                    if (i == currentPage)
                    {
                        string mUnselect = "<strong>" + i + "</strong>";

                        if (!String.IsNullOrWhiteSpace(mSelectedClass))
                        {
                            mUnselect = "<span class=\"" + mSelectedClass + "\">" + i + "</span>";
                        }

                        if (PageNumbersDisplay == PageNumbersDisplayTypeEnum.Numbers)
                        {
                            pages.Append(mUnselect);
                        }
                        else
                        {
                            pages.AppendFormat("[<strong>{0}-{1}</strong>]", recordStart, recordEnd);
                        }
                    }
                    else
                    {
                        //Set up PostBack link.
                        if (PagingMode == PagingModeTypeEnum.PostBack)
                        {
                            if (PageNumbersDisplay == PageNumbersDisplayTypeEnum.Numbers)
                            {
                                pages.AppendFormat("<a id=\"{0}\" name=\"{0}\" href=\"javascript:{1}\"{2} {4}>{3}</a>", UniqueID.Replace("$", "_") + i, ControlsHelper.GetPostBackEventReference(this, i.ToString()), pageNumberStyleTag, i, pageNumberClassTag);
                            }
                            else
                            {
                                pages.AppendFormat("[<a id=\"{0}\" name=\"{0}\" href=\"javascript:{1}\"{2}>{3}-{4}</a>]", UniqueID.Replace("$", "_") + i, ControlsHelper.GetPostBackEventReference(this, i.ToString()), pageNumberStyleTag, recordStart, recordEnd);
                            }
                        }
                        //Set up querystring link.
                        else
                        {
                            if (PageNumbersDisplay == PageNumbersDisplayTypeEnum.Numbers)
                            {
                                pages.AppendFormat("<a href=\"{0}\" {1} {3}>{2}</a>", UpdateQueryStringItem(QueryStringKey, i.ToString()), pageNumberStyleTag, i, pageNumberClassTag);
                            }
                            else
                            {
                                pages.AppendFormat("[<a href=\"{0}\" {1}>{2}-{3}</a>]", UpdateQueryStringItem(QueryStringKey, i.ToString()), pageNumberStyleTag, recordStart, recordEnd);
                            }
                        }
                    }
                }
            }
            pages.Append("&nbsp;");
            return pages.ToString();
        }


        /// <summary>
        /// Build Back/Next link navigation
        /// </summary>
        protected virtual string BuildLinkNavBack(int currentPage)
        {
            StringBuilder mLinkNav = new StringBuilder(128);

            string mUnselectClassTag = "";

            if (!String.IsNullOrWhiteSpace(mUnselectedPrevClass))
            {
                mUnselectClassTag = "class=\"" + mUnselectedPrevClass + "\"";
            }

            string mSelectClassTag = "";

            if (!String.IsNullOrWhiteSpace(mSelectedPrevClass))
            {
                mSelectClassTag = "class=\"" + mSelectedPrevClass + "\"";
            }


            //Next Links
            if (PagedData.IsFirstPage || PagedData.DataSourceCount == 0)
            {
                if (ShowFirstLast)
                {
                    mLinkNav.Append("&nbsp;" + FirstText);
                }

                mLinkNav.AppendFormat("&nbsp; <span {0} >" + BackText + "</span>", mSelectClassTag);
            }
            else
            {
                //Set up PostBack link.
                if (PagingMode == PagingModeTypeEnum.PostBack)
                {
                    if (ShowFirstLast)
                    {
                        mLinkNav.AppendFormat("&nbsp;<a id=\"{0}\" name=\"{0}\" href=\"javascript:{1}\">{2}</a>&nbsp;", UniqueID.Replace("$", "_") + "_bn", ControlsHelper.GetPostBackEventReference(this, "1"), FirstText);
                    }

                    mLinkNav.AppendFormat("&nbsp;<a id=\"{0}\" name=\"{0}\" href=\"javascript:{1}\" {3}>{2}</a>&nbsp;", UniqueID.Replace("$", "_") + "_bn", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(currentPage - 1)), BackText, mUnselectClassTag);
                }
                else
                {
                    if (ShowFirstLast)
                    {
                        mLinkNav.AppendFormat("&nbsp;<a href=\"{0}\">{1}</a>&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(1)), FirstText);
                    }

                    mLinkNav.AppendFormat("&nbsp;<a href=\"{0}\" {2}>{1}</a>&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(currentPage - 1)), BackText, mUnselectClassTag);
                }
            }

            return mLinkNav.ToString();
        }


        /// <summary>
        /// Build link Next navigation.
        /// </summary>
        protected virtual string BuildLinkNavNext(int currentPage, int pageCount)
        {
            StringBuilder mLinkNav = new StringBuilder(128);

            string mUnselectClassTag = "";

            if (!String.IsNullOrWhiteSpace(mUnselectedNextClass))
            {
                mUnselectClassTag = "class=\"" + mUnselectedNextClass + "\"";
            }

            string mSelectClassTag = "";

            if (!String.IsNullOrWhiteSpace(mSelectedNextClass))
            {
                mSelectClassTag = "class=\"" + mSelectedNextClass + "\"";
            }


            if (Context == null)
            {
                mLinkNav.AppendFormat("&nbsp;<a href=\"#\" {1}>{0}</a>&nbsp;", NextText, mUnselectClassTag);

                if (ShowFirstLast)
                {
                    mLinkNav.AppendFormat("&nbsp;<a href=\"#\">{0}</a>&nbsp;", LastText);
                }
            }
            else if (currentPage >= MaxPages || currentPage == PagedData.PageCount)
            {
                mLinkNav.AppendFormat("<span {0}>" + NextText + "</span> ", mSelectClassTag);

                if (ShowFirstLast)
                {
                    mLinkNav.Append("&nbsp;" + LastText);
                }
            }
            else
            {
                //Set up PostBack link.
                if (PagingMode == PagingModeTypeEnum.PostBack)
                {
                    mLinkNav.AppendFormat("&nbsp;<a id=\"{0}\" name=\"{0}\" href=\"javascript:{1}\" {3}>{2}</a>&nbsp;", UniqueID.Replace("$", "_") + "_nn", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(currentPage + 1)), NextText, mUnselectClassTag);

                    if (ShowFirstLast)
                    {
                        mLinkNav.AppendFormat("&nbsp;<a id=\"{0}\" name=\"{0}\" href=\"javascript:{1}\">{2}</a>&nbsp;", UniqueID.Replace("$", "_") + "_nn_first", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(pageCount)), LastText);
                    }
                }
                else
                {
                    mLinkNav.AppendFormat("&nbsp;<a href=\"{0}\" {2}>{1}</a>&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(currentPage + 1)), NextText, mUnselectClassTag);

                    if (ShowFirstLast)
                    {
                        mLinkNav.AppendFormat("&nbsp;<a href=\"{0}\">{1}</a>&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(pageCount)), LastText);
                    }
                }
            }

            return mLinkNav.ToString();
        }


        /// <summary>
        /// Build button Back navigation.
        /// </summary>
        protected virtual string BuildButtonNavBack(int currentPage)
        {
            StringBuilder mButtonNav = new StringBuilder(128);

            //Back Buttons
            if (PagedData.IsFirstPage || PagedData.DataSourceCount == 0)
            {
                if (ShowFirstLast)
                {
                    mButtonNav.AppendFormat("&nbsp;<input type=\"button\" value=\"{0}\" disabled=\"true\" />&nbsp;", FirstText);
                }

                mButtonNav.AppendFormat("&nbsp;<input type=\"button\" value=\"{0}\" disabled=\"true\" />&nbsp;", BackText);
            }
            else
            {
                if (PagingMode == PagingModeTypeEnum.PostBack)
                {
                    if (ShowFirstLast)
                    {
                        mButtonNav.AppendFormat("&nbsp;<input type=\"button\" id=\"{0}\" name=\"{0}\" onclick=\"javascript:{1}\" value=\"{2}\" />&nbsp;", UniqueID.Replace("$", "_") + "_btnb", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(1)), FirstText);
                    }

                    mButtonNav.AppendFormat("&nbsp;<input type=\"button\" id=\"{0}\" name=\"{0}\" onclick=\"javascript:{1}\" value=\"{2}\" />&nbsp;", UniqueID.Replace("$", "_") + "_btnb_last", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(currentPage - 1)), BackText);
                }
                else
                {
                    if (ShowFirstLast)
                    {
                        mButtonNav.AppendFormat("&nbsp;<input type=\"button\" name=\"{0}\" onclick=\"window.location='{0}';\" value=\"{1}\" />&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(1)), FirstText);
                    }

                    mButtonNav.AppendFormat("&nbsp;<input type=\"button\" name=\"{0}\" onclick=\"window.location='{0}';\" value=\"{1}\" />&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(currentPage - 1)), BackText);
                }
            }
            return mButtonNav.ToString();
        }


        /// <summary>
        /// Build button Next navigation.
        /// </summary>
        protected virtual string BuildButtonNavNext(int currentPage, int pageCount)
        {
            StringBuilder mButtonNav = new StringBuilder(128);

            //Next Buttons
            if (currentPage >= MaxPages || currentPage >= PagedData.PageCount || Context == null)
            {
                mButtonNav.AppendFormat("&nbsp;<input type=\"button\" value=\"{0}\" disabled=\"true\" />&nbsp;", NextText);

                if (ShowFirstLast)
                {
                    mButtonNav.AppendFormat("&nbsp;<input type=\"button\" value=\"{0}\" disabled=\"true\" />&nbsp;", LastText);
                }
            }
            else
            {
                if (PagingMode == PagingModeTypeEnum.PostBack)
                {
                    mButtonNav.AppendFormat("&nbsp;<input type=\"button\" id=\"{0}\" name=\"{0}\" onclick=\"javascript:{1}\" value=\"{2}\" />&nbsp;", UniqueID.Replace("$", "_") + "_btnn", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(currentPage + 1)), NextText);

                    if (ShowFirstLast)
                    {
                        mButtonNav.AppendFormat("&nbsp;<input type=\"button\" id=\"{0}\" name=\"{0}\" onclick=\"javascript:{1}\" value=\"{2}\" />&nbsp;", UniqueID.Replace("$", "_") + "_btnn", ControlsHelper.GetPostBackEventReference(this, Convert.ToString(pageCount)), LastText);
                    }
                }
                else
                {
                    mButtonNav.AppendFormat("&nbsp;<input type=\"button\" onclick=\"window.location='{0}';\" value=\"{1}\" />&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(currentPage + 1)), NextText);

                    if (ShowFirstLast)
                    {
                        mButtonNav.AppendFormat("&nbsp;<input type=\"button\" onclick=\"window.location='{0}';\" value=\"{1}\" />&nbsp;", UpdateQueryStringItem(QueryStringKey, Convert.ToString(pageCount)), LastText);
                    }
                }
            }
            return mButtonNav.ToString();
        }


        /// <summary> 
        /// Build results
        /// </summary>
        public virtual string BuildResults(string resultsFormat)
        {
            if ((PageCount == 0) || (Context == null))
            {
                if (ResultsLocation != ResultsLocationTypeEnum.None)
                {
                    return string.Format(resultsFormat, "1", PageSize, (MaxPages * PageSize));
                }
            }
            else if (ResultsLocation != ResultsLocationTypeEnum.None)
            {
                return string.Format(resultsFormat, RecordStart, RecordEnd, TotalRecords);
            }

            return string.Empty;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Keys to be added to querystring.
        /// </summary>
        public string[,] InsertKeys
        {
            get
            {
                return mInsertKeys;
            }
            set
            {
                mInsertKeys = value;
            }
        }


        /// <summary>
        /// Check if insert is enabled.
        /// </summary>
        public bool InsertToUrl
        {
            get
            {
                return mInsert;
            }
            set
            {
                mInsert = value;
            }
        }


        /// <summary>
        /// Keys to remove.
        /// </summary>
        public string[] RemoveKeys
        {
            get
            {
                return mRemoveKeys;
            }
            set
            {
                mRemoveKeys = value;
            }
        }


        /// <summary>
        /// Check if remove is enabled.
        /// </summary>
        public bool RemoveFromUrl
        {
            get
            {
                return mRemove;
            }
            set
            {
                mRemove = value;
            }
        }


        /// <summary>
        /// Updates current URL with new items.
        /// </summary>
        /// <param name="queryStringKey">Key to add</param>
        /// <param name="newQueryStringValue">Key value</param>
        public string UpdateQueryStringItem(string queryStringKey, string newQueryStringValue)
        {
            string mRawUrl = RequestContext.CurrentURL;

            // Remove Keys from url parameters
            if (RemoveFromUrl && (RemoveKeys != null))
            {
                foreach (string key in RemoveKeys)
                {
                    mRawUrl = URLHelper.RemoveParameterFromUrl(mRawUrl, key);
                }
            }

            // Add keys to Url
            if (InsertToUrl && (InsertKeys != null))
            {
                for (int i = InsertKeys.GetLowerBound(0); i <= InsertKeys.GetUpperBound(0); i++)
                {
                    mRawUrl = URLHelper.AddParameterToUrl(mRawUrl, InsertKeys[i, 0], InsertKeys[i, 1]);
                }
            }

            // Ensure that query for first page will be omitted
            if (string.Equals(QueryStringKey, queryStringKey, StringComparison.Ordinal) && string.Equals("1", newQueryStringValue, StringComparison.Ordinal))
            {
                return HTMLHelper.HTMLEncode(URLHelper.RemoveParameterFromUrl(mRawUrl, queryStringKey));
            }
            
            // Do HTML encode
            return HTMLHelper.HTMLEncode(URLHelper.AddParameterToUrl(mRawUrl, queryStringKey, newQueryStringValue));
        }


        /// <summary>
        /// Defines the Click event.
        /// </summary>
        public event EventHandler Click;


        /// <summary>
        /// Invokes delegates registered with the Click event.
        /// </summary>
        public virtual void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }


        /// <summary>
        /// Method of IPostBackEventHandler that raises change events.
        /// </summary>
        public virtual void RaisePostBackEvent(string eventArgument)
        {
            if (ValidationHelper.IsInteger(eventArgument))
            {
                ChangePage(int.Parse(eventArgument));
            }
            OnClick(new EventArgs());

            RaiseOnPageChange();
        }

        #endregion


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (SiteContext.CurrentSite != null)
                {
                    if (SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSControlElement").Trim().Equals("div", StringComparison.OrdinalIgnoreCase))
                    {
                        return HtmlTextWriterTag.Div;
                    }
                    return HtmlTextWriterTag.Span;
                }
                return HtmlTextWriterTag.Span;
            }
        }
    }


    #region "Properties enumerations"

    /// <summary>
    /// Allows you to specify where you'd like the Back and Next Buttons/Links to appear
    /// </summary>
    public enum BackNextLocationTypeEnum
    {
        ///<summary>Right</summary>
        Right,

        ///<summary>Left</summary>
        Left,

        ///<summary>Split</summary>
        Split,

        ///<summary>None</summary>
        None
    }


    /// <summary>
    /// Allows you to switch between Buttons and Text links.
    /// </summary>
    public enum BackNextDisplayTypeEnum
    {
        ///<summary>Buttons</summary>
        Buttons,

        ///<summary>Hyperlinks</summary>
        HyperLinks
    }


    /// <summary>
    /// Allows you to specify where you want the results to be displayed.
    /// </summary>
    public enum PageNumbersDisplayTypeEnum
    {
        ///<summary>Number</summary>
        Numbers,

        ///<summary>Results</summary>
        Results
    }


    /// <summary>
    /// Allows you to specify where you want the results to be displayed.
    /// </summary>
    public enum ResultsLocationTypeEnum
    {
        ///<summary>Top</summary>
        Top,

        ///<summary>Bottom</summary>
        Bottom,

        ///<summary>None</summary>
        None
    }

    #endregion
}