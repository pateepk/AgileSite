using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control for supporting the data paging within the databound controls.
    /// </summary>
    [ToolboxItem(false)]
    public class BasicDataPager : CMSWebControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// DataSource.
        /// </summary>
        protected DataSet mDataSource = null;

        /// <summary>
        /// Paged data.
        /// </summary>
        protected PagedDataSource mPagedData = null;

        /// <summary>
        /// Max. pages.
        /// </summary>
        protected int mMaxPages = 1000;

        /// <summary>
        /// Unique ID.
        /// </summary>
        protected string mUniqueID = null;

        /// <summary>
        /// Paging mode.
        /// </summary>
        protected PagingModeTypeEnum mPagingMode = PagingModeTypeEnum.QueryString;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Unique ID.
        /// </summary>
        public override string UniqueID
        {
            get
            {
                if (mUniqueID == null)
                {
                    mUniqueID = base.UniqueID;
                }
                return mUniqueID;
            }
        }


        /// <summary>
        /// Page count.
        /// </summary>
        [Browsable(false)]
        public virtual int PageCount
        {
            get
            {
                int count;

                if (Context == null)
                {
                    count = MaxPages;
                }
                else
                {
                    count = mPagedData.PageCount;
                    if (count > MaxPages)
                    {
                        count = MaxPages;
                    }
                }

                return count;
            }
        }


        /// <summary>
        /// Index of the first record on current page.
        /// </summary>
        [Browsable(false)]
        public virtual int RecordStart
        {
            get
            {
                if (Context == null)
                {
                    return 0;
                }
                else
                {
                    return ((CurrentPage - 1) * mPagedData.PageSize) + 1;
                }
            }
        }


        /// <summary>
        /// Index of the last record on current page.
        /// </summary>
        [Browsable(false)]
        public virtual int RecordEnd
        {
            get
            {
                if (Context == null)
                {
                    return PageSize;
                }
                else
                {
                    return RecordStart - 1 + mPagedData.Count;
                }
            }
        }


        /// <summary>
        /// Total data source records.
        /// </summary>
        [Browsable(false)]
        public virtual int TotalRecords
        {
            get
            {
                if (Context == null)
                {
                    return MaxPages * PageSize;
                }
                else if (mPagedData.DataSourceCount > MaxPages * PageSize)
                {
                    return MaxPages * PageSize;
                }
                else
                {
                    return mPagedData.DataSourceCount;
                }
            }
        }


        /// <summary>
        /// Pager DataSource.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager")]
        public virtual object DataSource
        {
            get
            {
                return (DataView)mPagedData.DataSource;
            }
            set
            {
                if (value is DataView)
                {
                    mPagedData.DataSource = (DataView)value;
                }
                else if (value is DataSet)
                {
                    mPagedData.DataSource = ((DataSet)value).Tables[0].DefaultView;
                }

                // Raise OnDataSourceChanged event
                RaiseOnDataSourceChanged();
            }
        }


        /// <summary>
        /// Paged data.
        /// </summary>
        [Browsable(false), Category("Data Pager")]
        public virtual PagedDataSource PagedData
        {
            get
            {
                mPagedData.CurrentPageIndex = CurrentPage - 1;
                return mPagedData;
            }
        }


        /// <summary>
        /// Page size.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(10)]
        public virtual int PageSize
        {
            get
            {
                return mPagedData.PageSize;
            }
            set
            {
                mPagedData.PageSize = value;
            }
        }


        /// <summary>
        /// Maximum number of pages to be displayed.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(1000)]
        public virtual int MaxPages
        {
            get
            {
                return mMaxPages;
            }
            set
            {
                mMaxPages = value;
            }
        }


        /// <summary>
        /// Current page number.
        /// </summary>
        public virtual int CurrentPage
        {
            get
            {
                int result = ValidationHelper.GetInteger(ViewState["CurrentPage"], 1);
                if (result < 1)
                {
                    result = 1;
                }
                if (result > PageCount)
                {
                    result = PageCount;
                }
                return result;
            }
            set
            {
                ViewState["CurrentPage"] = value;
            }
        }


        /// <summary>
        /// Pager position.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(PagingPlaceTypeEnum.Bottom)]
        public PagingPlaceTypeEnum PagerPosition
        {
            get
            {
                if (ViewState["PagerPosition"] == null)
                {
                    return PagingPlaceTypeEnum.Bottom;
                }
                else
                {
                    return (PagingPlaceTypeEnum)ViewState["PagerPosition"];
                }
            }
            set
            {
                ViewState["PagerPosition"] = value;
            }
        }


        /// <summary>
        /// Determines if PostBack or QueryString should be used for the paging.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        [Category("Data Pager"), DefaultValue(PagingModeTypeEnum.QueryString)]
        public PagingModeTypeEnum PagingMode
        {
            get
            {
                return mPagingMode;
            }
            set
            {
                if (Enum.IsDefined(typeof(PagingModeTypeEnum), value))
                {
                    mPagingMode = value;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicDataPager()
        {
            // Create new paged datasource
            mPagedData = new PagedDataSource();
            mPagedData.CurrentPageIndex = 0;
            mPagedData.PageSize = 10;
            mPagedData.AllowPaging = true;
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return CMSControlsHelper.GetControlTagKey();
            }
        }


        /// <summary>
        /// Page change event.
        /// </summary>
        public event EventHandler OnPageChange;


        /// <summary>
        /// Raises the OnPageChange event.
        /// </summary>
        public void RaiseOnPageChange()
        {
            if (OnPageChange != null)
            {
                OnPageChange(this, new EventArgs());
            }
        }


        /// <summary>
        /// Data source change event.
        /// </summary>
        public event EventHandler OnDataSourceChanged;


        /// <summary>
        /// Raises the OnDataSourceChanged event.
        /// </summary>
        public void RaiseOnDataSourceChanged()
        {
            if (OnDataSourceChanged != null)
            {
                OnDataSourceChanged(this, new EventArgs());
            }
        }


        /// <summary>
        /// Returns the DataRow for the given item index.
        /// </summary>
        public DataRow GetItemDataRow(int index)
        {
            DataRow dr = null;
            if ((mPagedData.DataSource != null) && (index >= 0))
            {
                DataTable dt = ((DataView)mPagedData.DataSource).Table;
                if (dt.Rows.Count > index)
                {
                    dr = dt.Rows[index];
                }
            }
            return dr;
        }


        /// <summary>
        /// Returns the page of selected item (start with 1)
        /// </summary>
        /// <param name="where">Where condition to find the item</param>
        public int GetItemPage(string where)
        {
            int result = 1;
            if (mPagedData.DataSource != null)
            {
                DataTable dt = ((DataView)mPagedData.DataSource).Table;
                DataRow[] rows = dt.Select(where);

                // Get the row index
                if (rows.Length > 0)
                {
                    int index = 1;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row == rows[0])
                        {
                            break;
                        }
                        index++;
                    }

                    result = (int)Math.Ceiling((Double)index / (Double)PageSize);
                }
            }
            return result;
        }


        /// <summary>
        /// Returns the pager position based on the given string.
        /// </summary>
        /// <param name="position">String position representation</param>
        public PagingPlaceTypeEnum GetPagerPosition(string position)
        {
            if (position == null)
            {
                return PagingPlaceTypeEnum.Bottom;
            }
            else
            {
                switch (position.ToLowerCSafe())
                {
                    case "top":
                        return PagingPlaceTypeEnum.Top;
                    case "topandbottom":
                        return PagingPlaceTypeEnum.TopAndBottom;
                    default:
                        return PagingPlaceTypeEnum.Bottom;
                }
            }
        }


        /// <summary>
        /// Returns the result position based on the given string.
        /// </summary>
        /// <param name="position">String position representation</param>
        public ResultsLocationTypeEnum GetResultPosition(string position)
        {
            if (position == null)
            {
                return ResultsLocationTypeEnum.Top;
            }
            else
            {
                switch (position.ToLowerCSafe())
                {
                    case "bottom":
                        return ResultsLocationTypeEnum.Bottom;
                    case "none":
                        return ResultsLocationTypeEnum.None;
                    default:
                        return ResultsLocationTypeEnum.Top;
                }
            }
        }


        /// <summary>
        /// Returns the paging mode based on the given string.
        /// </summary>
        /// <param name="mode">String mode representation</param>
        public PagingModeTypeEnum GetPagingMode(string mode)
        {
            if (mode == null)
            {
                return PagingModeTypeEnum.QueryString;
            }
            else
            {
                switch (mode.ToLowerCSafe())
                {
                    case "postback":
                        return PagingModeTypeEnum.PostBack;
                    default:
                        return PagingModeTypeEnum.QueryString;
                }
            }
        }


        /// <summary>
        /// Returns back/next type based on the given string.
        /// </summary>
        /// <param name="type">String type representation</param>
        public BackNextLocationTypeEnum GetBackNextLocation(string type)
        {
            switch (type.ToLowerCSafe())
            {
                case "none":
                    return BackNextLocationTypeEnum.None;
                case "split":
                    return BackNextLocationTypeEnum.Split;
                case "left:":
                    return BackNextLocationTypeEnum.Left;
                case "right":
                    return BackNextLocationTypeEnum.Right;
                default:
                    return BackNextLocationTypeEnum.None;
            }
        }


        /// <summary>
        /// Render override.
        /// </summary>
        /// <param name="writer">Writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write(" [ BasicDataPager : " + ID + " ]");
                return;
            }

            base.Render(writer);
        }


        /// <summary>
        /// Returns the pager position based on the given string.
        /// </summary>
        /// <param name="position">String position representation</param>
        public static PagingPlaceTypeEnum StringToPagingPlaceTypeEnum(string position)
        {
            if (position == null)
            {
                return PagingPlaceTypeEnum.Bottom;
            }
            else
            {
                switch (position.ToLowerCSafe())
                {
                    case "top":
                        return PagingPlaceTypeEnum.Top;
                    case "topandbottom":
                        return PagingPlaceTypeEnum.TopAndBottom;
                    default:
                        return PagingPlaceTypeEnum.Bottom;
                }
            }
        }

        #endregion
    }


    #region "Enumerations"

    /// <summary>
    ///	Place where is paging.
    /// </summary>
    public enum PagingPlaceTypeEnum
    {
        ///<summary>Bottom</summary>   
        Bottom,

        ///<summary>Top</summary>
        Top,

        /// <summary>
        /// Top and bottom.
        /// </summary>
        TopAndBottom
    }


    /// <summary>
    ///	Allows you to switch between paging modes.
    /// </summary>
    public enum PagingModeTypeEnum
    {
        ///<summary>PostBack action</summary>
        PostBack,

        ///<summary>QueryString action</summary>
        QueryString
    }

    #endregion
}