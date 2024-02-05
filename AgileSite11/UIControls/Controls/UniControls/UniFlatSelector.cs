using System;
using System.ComponentModel;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Uni flat selector base class.
    /// </summary>
    public abstract class UniFlatSelector : FormEngineUserControl
    {
        #region "Events"

        /// <summary>
        /// On selected item delegate.
        /// </summary>    
        public delegate string ItemSelectedEventHandler(string selectedValue);

        /// <summary>
        /// On selected item event handler.
        /// </summary>
        public event ItemSelectedEventHandler OnItemSelected;


        /// <summary>
        /// On search delegate.
        /// </summary>    
        public delegate void ItemSearch();

        /// <summary>
        /// On search event.
        /// </summary>
        public event ItemSearch OnSearch;

        #endregion


        #region "Private variables"

        // Template
        private ITemplate mItemTemplate = null;
        private ITemplate mAlternatingItemTemplate = null;
        private ITemplate mFooterTemplate = null;
        private ITemplate mHeaderTemplate = null;
        private ITemplate mSeparatorTemplate = null;

        // Other    
        private string mSearchLabelResourceString = string.Empty;
        private string mJavaScriptFunction = string.Empty;
        private string mSelectFunction = string.Empty;
        private string mCustomSelectItemFunction = string.Empty;
        private string mCustomDoubleClickFunction = string.Empty;
        private string mCustomCallBackHandlerFunction = string.Empty;
        private bool mUsePostBack = false;
        private bool mRememberSelectedItem = false;
        private int mImageMaxSideSize = 64;
        private string mNotAvailableImageUrl = null;
        private string mNoRecordsMessage = null;
        private string mNoRecordsSearchMessage = null;

        // Object type    
        private string mValueColumn = string.Empty;
        private string mSkipPropertiesDialogColumn = string.Empty;
        private string mSearchColumn = string.Empty;

        private string mOrderBy = String.Empty;
        private string mWhereCondition = String.Empty;
        private int mSelectTopN = 0;
        private string mQueryName = String.Empty;
        private string mSelectedColumns = String.Empty;
        private int mPageSize = 15;
        private UniPagerMode mPagingMode = UniPagerMode.PostBack;
        private string mQueryStringKey = "page";
        private int mGroupSize = 10;
        private bool mDisplayFirstLastAutomatically = false;
        private bool mDisplayPreviousNextAutomatically = true;
        private bool mHidePagerForSinglePage = true;
        private int mMaxPages = 10000;
        private string mSelectedItem = String.Empty;

        // Indicates whether auto focus functionality is enabled
        private bool mUseStartUpFocus = true;

        #endregion


        #region "Virtual repeater properties"

        /// <summary>
        /// Gets or sets the order by clause.
        /// </summary>
        public virtual string OrderBy
        {
            get
            {
                return mOrderBy;
            }
            set
            {
                mOrderBy = value;
            }
        }


        /// <summary>
        /// Gets or sets the where condition.
        /// </summary>
        public virtual string WhereCondition
        {
            get
            {
                return mWhereCondition;
            }
            set
            {
                mWhereCondition = value;
            }
        }


        /// <summary>
        /// Gets or sets the number which indicates how many documents should be displayed.
        /// </summary>
        public virtual int SelectTopN
        {
            get
            {
                return mSelectTopN;
            }
            set
            {
                mSelectTopN = value;
            }
        }


        /// <summary>
        /// Gets or sets the query name.
        /// </summary>
        public virtual string QueryName
        {
            get
            {
                return mQueryName;
            }
            set
            {
                mQueryName = value;
            }
        }


        /// <summary>
        /// Gets or sets the selected columns.
        /// </summary>
        public virtual string SelectedColumns
        {
            get
            {
                return mSelectedColumns;
            }
            set
            {
                mSelectedColumns = value;
            }
        }

        #endregion


        #region "Virtual UniPager properties"

        /// <summary>
        /// Gets or sets page size.
        /// </summary>
        public virtual int PageSize
        {
            get
            {
                return mPageSize;
            }
            set
            {
                mPageSize = value;
            }
        }


        /// <summary>
        /// Gets or sets search option.
        /// </summary>
        public virtual UniPagerMode PagingMode
        {
            get
            {
                return mPagingMode;
            }
            set
            {
                mPagingMode = value;
            }
        }


        /// <summary>
        /// Gets or sets query string key.
        /// </summary>
        public virtual string QueryStringKey
        {
            get
            {
                return mQueryStringKey;
            }
            set
            {
                mQueryStringKey = value;
            }
        }


        /// <summary>
        /// Gets or sets group size.
        /// </summary>
        public virtual int GroupSize
        {
            get
            {
                return mGroupSize;
            }
            set
            {
                mGroupSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether first and last item template are displayed dynamically based on current view.
        /// </summary>
        public virtual bool DisplayFirstLastAutomatically
        {
            get
            {
                return mDisplayFirstLastAutomatically;
            }
            set
            {
                mDisplayFirstLastAutomatically = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether first and last item template are displayed dynamically based on current view.
        /// </summary>
        public virtual bool DisplayPreviousNextAutomatically
        {
            get
            {
                return mDisplayPreviousNextAutomatically;
            }
            set
            {
                mDisplayPreviousNextAutomatically = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether pager should be hidden for single page.
        /// </summary>
        public virtual bool HidePagerForSinglePage
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
        /// Gets or sets the pager max pages.
        /// </summary>
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

        #endregion


        #region "Object type properties"

        /// <summary>
        /// Gets or sets image value column.
        /// </summary>
        public string ValueColumn
        {
            get
            {
                return mValueColumn;
            }
            set
            {
                mValueColumn = value;
            }
        }

        /// <summary>
        /// Gets or sets the column name which contains information whether to skip the insert properties dialog.
        /// </summary>
        public string SkipPropertiesDialogColumn
        {
            get
            {
                return mSkipPropertiesDialogColumn;
            }
            set
            {
                mSkipPropertiesDialogColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets column name for searching.
        /// </summary>
        public string SearchColumn
        {
            get
            {
                return mSearchColumn;
            }
            set
            {
                mSearchColumn = value;
            }
        }

        #endregion


        #region "Template properties"

        /// <summary>
        /// Sets or gets item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate ItemTemplate
        {
            get
            {
                return mItemTemplate;
            }
            set
            {
                mItemTemplate = value;
            }
        }


        /// <summary>
        /// Sets or gets alternating item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate AlternatingItemTemplate
        {
            get
            {
                return mAlternatingItemTemplate;
            }
            set
            {
                mAlternatingItemTemplate = value;
            }
        }


        /// <summary>
        /// Sets or gets header template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate HeaderTemplate
        {
            get
            {
                return mHeaderTemplate;
            }
            set
            {
                mHeaderTemplate = value;
            }
        }


        /// <summary>
        /// Sets or gets footer template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate FooterTemplate
        {
            get
            {
                return mFooterTemplate;
            }
            set
            {
                mFooterTemplate = value;
            }
        }


        /// <summary>
        /// Sets or gets separator template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate SeparatorTemplate
        {
            get
            {
                return mSeparatorTemplate;
            }
            set
            {
                mSeparatorTemplate = value;
            }
        }

        #endregion


        #region "Other properties"

        /// <summary>
        /// Gets or sets the value that indicates whether autofocus functionality is enabled.
        /// </summary>
        public virtual bool UseStartUpFocus
        {
            get
            {
                return mUseStartUpFocus;
            }
            set
            {
                mUseStartUpFocus = value;
            }
        }


        /// <summary>
        /// Gets or sets selected item.
        /// </summary>
        public virtual string SelectedItem
        {
            get
            {
                return mSelectedItem;
            }
            set
            {
                mSelectedItem = value;
            }
        }


        /// <summary>
        /// Gets or sets searched value.
        /// </summary>
        public string SearchText
        {
            get
            {
                return ValidationHelper.GetString(ViewState["SearchText"], "");
            }
            set
            {
                ViewState["SearchText"] = value;
            }
        }


        /// <summary>
        /// Gets or sets resource string for search label.
        /// </summary>
        public string SearchLabelResourceString
        {
            get
            {
                return mSearchLabelResourceString;
            }
            set
            {
                mSearchLabelResourceString = value;
            }
        }


        /// <summary>
        /// Gets or sets name of javascript function which is used for selecting item in flat selector.
        /// </summary>
        public string JavascriptFunction
        {
            get
            {
                return mJavaScriptFunction;
            }
            set
            {
                mJavaScriptFunction = value;
            }
        }


        /// <summary>
        /// Gets or sets name of javascript function which is used for passing selected item out of selector.
        /// </summary>
        public string SelectFunction
        {
            get
            {
                return mSelectFunction;
            }
            set
            {
                mSelectFunction = value;
            }
        }


        /// <summary>
        /// Gets or sets the full code of select item javascript function used for selecting item in flat selector.
        /// </summary>
        public string CustomSelectItemFunction
        {
            get
            {
                return mCustomSelectItemFunction;
            }
            set
            {
                mCustomSelectItemFunction = value;
            }
        }


        /// <summary>
        /// Gets or sets the full code of call back handler javascript function function (called from the selected item).
        /// </summary>
        public string CustomCallBackHandlerFunction
        {
            get
            {
                return mCustomCallBackHandlerFunction;
            }
            set
            {
                mCustomCallBackHandlerFunction = value;
            }
        }


        /// <summary>
        /// Gets or sets the full code of double click javascript function.
        /// </summary>
        public string CustomDoubleClickFunction
        {
            get
            {
                return mCustomDoubleClickFunction;
            }
            set
            {
                mCustomDoubleClickFunction = value;
            }
        }


        /// <summary>
        /// Enables or disables postback mode for item selection.
        /// </summary>
        public bool UsePostback
        {
            get
            {
                return mUsePostBack;
            }
            set
            {
                mUsePostBack = value;
            }
        }


        /// <summary>
        /// If enabled, flat selector remembers selected item trough postbacks.
        /// </summary>
        public bool RememberSelectedItem
        {
            get
            {
                return mRememberSelectedItem;
            }
            set
            {
                mRememberSelectedItem = value;
            }
        }


        /// <summary>
        /// Gets or sets the URL for image which is used if flat item hasn't its own image.
        /// </summary>
        public string NotAvailableImageUrl
        {
            get
            {
                return mNotAvailableImageUrl;
            }
            set
            {
                mNotAvailableImageUrl = value;
            }
        }


        /// <summary>
        /// Gets or sets the value of maxsidesize parameter for flat items image.
        /// </summary>
        public int ImageMaxSideSize
        {
            get
            {
                return mImageMaxSideSize;
            }
            set
            {
                mImageMaxSideSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the resource string for message which is displayed if repeater has no records
        /// Note: Unresolved resource string can be used
        /// </summary>
        /// <seealso cref="NoRecordsSearchMessage"/>
        public string NoRecordsMessage
        {
            get
            {
                return mNoRecordsMessage;
            }
            set
            {
                mNoRecordsMessage = value;
            }
        }


        /// <summary>
        /// Gets or sets the resource string for message which is displayed if repeater has no records 
        /// due to specified search condition
        /// Note: Unresolved resource string can be used
        /// </summary>
        /// <seealso cref="NoRecordsMessage"/>
        public string NoRecordsSearchMessage
        {
            get
            {
                return mNoRecordsSearchMessage;
            }
            set
            {
                mNoRecordsSearchMessage = value;
            }
        }


        /// <summary>
        /// Gets or sets field value. You need to override this method to make the control work properly with the form.
        /// </summary>
        public override object Value
        {
            get
            {
                return SelectedItem;
            }
            set
            {
                SelectedItem = ValidationHelper.GetString(value, "");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises OnSearch event.
        /// </summary>
        protected void RaiseOnSearch()
        {
            // Raise OnSearch event
            if (OnSearch != null)
            {
                OnSearch();
            }
        }


        /// <summary>
        /// Raises OnSearch event.
        /// </summary>
        protected string RaiseOnItemSelected(string selectedValue)
        {
            // Raise OnSearch event
            if (OnItemSelected != null)
            {
                return OnItemSelected(selectedValue);
            }

            return null;
        }


        /// <summary>
        /// Sets pager to first page.
        /// </summary>
        public virtual void ResetPager()
        {
        }


        /// <summary>
        /// Clears search text.
        /// </summary>
        public virtual void ClearSearchText()
        {
        }


        /// <summary>
        /// Clears search condition and resets pager to first page.
        /// </summary>
        public virtual void ResetToDefault()
        {
        }

        #endregion
    }
}