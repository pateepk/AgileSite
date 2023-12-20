using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Search;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSSearchResults class.
    /// </summary>
    [ToolboxData("<{0}:CMSSearchResults runat=server></{0}:CMSSearchResults>")]
    public class CMSSearchResults : WebControl
    {
        #region "Variables"

        /// <summary>
        /// Basic repeater variable.
        /// </summary>
        protected BasicRepeater mRepeater = new BasicRepeater();

        /// <summary>
        /// DataPager variable.
        /// </summary>
        protected DataPager mDataPager = null;

        /// <summary>
        /// Search dialog.
        /// </summary>
        protected CMSSearchDialog mSearchDialog;

        /// <summary>
        /// No result label.
        /// </summary>
        protected Label mNoResultsLabel = null;

        /// <summary>
        /// Query string key for setup data pager query string key.
        /// </summary>
        protected string mQueryStringKey = "PageSearchResults";

        private bool mNoResult = false;
        private bool mRenderIt = false;

        /// <summary>
        /// Control properties variable.
        /// </summary>
        protected CMSControlProperties mProperties = new CMSControlProperties();

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        /// <summary>
        /// Indicates if transformations should be ignored and templates for direct access should be used.
        /// </summary>
        private bool mIgnoreTransformations = false;

        #endregion


        #region "Delegates & events"

        /// <summary>
        /// Search completed delegate.
        /// </summary>
        /// <param name="noResults">Determines whether control has data</param>
        /// <param name="renderIt">Determines whether control is being rendered</param>
        public delegate void SearchCompletedHandler(bool noResults, bool renderIt);

        /// <summary>
        /// Raises when search is completed.
        /// </summary>
        public event SearchCompletedHandler OnSearchCompleted;

        #endregion


        #region "CMS Control Properties"

        /// <summary>
        /// Stop processing 
        ///</summary>
        [Category("Behavior"), DefaultValue(false), Description("Stop processing.")]
        public virtual bool StopProcessing
        {
            get
            {
                return mProperties.StopProcessing;
            }
            set
            {
                mProperties.StopProcessing = value;
            }
        }


        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TreeProvider TreeProvider
        {
            get
            {
                return mProperties.TreeProvider;
            }
            set
            {
                mProperties.TreeProvider = value;
            }
        }


        /// <summary>
        /// Property to set and get the ClassNames list (separated by the semicolon).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the ClassNames list (separated by the semicolon).")]
        public virtual string ClassNames
        {
            get
            {
                return mProperties.ClassNames;
            }
            set
            {
                mProperties.ClassNames = value;
            }
        }


        /// <summary>
        /// Property to set and get the Where condition.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Where condition.")]
        public virtual string WhereCondition
        {
            get
            {
                return mProperties.WhereCondition;
            }
            set
            {
                mProperties.WhereCondition = value;
            }
        }


        /// <summary>
        /// Property to set and get the Order by.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Order by.")]
        public virtual string OrderBy
        {
            get
            {
                return mProperties.OrderBy;
            }
            set
            {
                mProperties.OrderBy = value;
            }
        }


        /// <summary>
        /// Property to set and get the Path.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Path to the menu items that should be displayed in the site map.")]
        public virtual string Path
        {
            get
            {
                return ValidationHelper.GetString(ViewState["Path"], "");
            }
            set
            {
                ViewState["Path"] = value;
            }
        }


        /// <summary>
        /// Property to set and get the SiteName.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the SiteName.")]
        public virtual string SiteName
        {
            get
            {
                return mProperties.SiteName;
            }
            set
            {
                mProperties.SiteName = value;
            }
        }


        /// <summary>
        /// Property to set and get the CultureCode.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Code of the preferred culture (en-us, fr-fr, etc.). If it's not specified, it is read from the CMSPreferredCulture session variable and then from the CMSDefaultCultureCode configuration key.")]
        public virtual string CultureCode
        {
            get
            {
                return mProperties.CultureCode;
            }
            set
            {
                mProperties.CultureCode = value;
            }
        }


        /// <summary>
        /// Property to set and get the CombineWithDefaultCulture flag.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the results should be combined with default language versions in case the translated version is not available.")]
        public virtual bool CombineWithDefaultCulture
        {
            get
            {
                return mProperties.CombineWithDefaultCulture;
            }
            set
            {
                mProperties.CombineWithDefaultCulture = value;
            }
        }


        /// <summary>
        /// Indicates if the duplicated (linked) items should be filtered out from the data.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the duplicated (linked) items should be filtered out from the data.")]
        public virtual bool FilterOutDuplicates
        {
            get
            {
                return mProperties.FilterOutDuplicates;
            }
            set
            {
                mProperties.FilterOutDuplicates = value;
            }
        }


        /// <summary>
        /// Property to set and get the SelectOnlyPublished flag.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if only published documents should be displayed.")]
        public virtual bool SelectOnlyPublished
        {
            get
            {
                return mProperties.SelectOnlyPublished;
            }
            set
            {
                mProperties.SelectOnlyPublished = value;
            }
        }


        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.")]
        public virtual bool CheckPermissions
        {
            get
            {
                return mProperties.CheckPermissions;
            }
            set
            {
                mProperties.CheckPermissions = value;
            }
        }


        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        [Category("Behavior"), DefaultValue(0), Description("Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.")]
        public virtual int CacheMinutes
        {
            get
            {
                return mProperties.CacheMinutes;
            }
            set
            {
                mProperties.CacheMinutes = value;
            }
        }


        /// <summary>
        /// Name of the cache item the control will use.
        /// </summary>
        /// <remarks>
        /// By setting this name dynamically, you can achieve caching based on URL parameter or some other variable - simply put the value of the parameter to the CacheItemName property. If no value is set, the control stores its content to the item named "URL|ControlID".
        /// </remarks>
        [Category("Behavior"), DefaultValue(""), Description("Name of the cache item the control will use.")]
        public virtual string CacheItemName
        {
            get
            {
                return mProperties.CacheItemName;
            }
            set
            {
                mProperties.CacheItemName = value;
            }
        }


        /// <summary>
        /// Cache dependencies, each cache dependency on a new line.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Cache dependencies, each cache dependency on a new line.")]
        public virtual string CacheDependencies
        {
            get
            {
                return mProperties.CacheDependencies;
            }
            set
            {
                mProperties.CacheDependencies = value;
            }
        }


        /// <summary>
        /// Gets or sets a DataSet containing values used to populate the items within the control. This value needn't be set.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataSet DataSource
        {
            get
            {
                return mProperties.DataSource;
            }
            set
            {
                mProperties.DataSource = value;
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return mProperties.ControlTagKey;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if transformations should be ignored and templates for direct access should be used.
        /// </summary>
        public bool IgnoreTransformations
        {
            get
            {
                return Convert.ToBoolean(mIgnoreTransformations);
            }
            set
            {
                mIgnoreTransformations = value;
            }
        }

        /// <summary>
        /// Word(s) to be searched for.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Word(s) to be searched for.")]
        public string SearchExpression
        {
            get
            {
                return Convert.ToString(ViewState["SearchExpression"]);
            }
            set
            {
                ViewState["SearchExpression"] = value;
            }
        }


        /// <summary>
        /// Indicates if all content or only the current section should be searched.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Returns 0 for all content or 1 for the current section."), NotifyParentProperty(true)]
        public SearchModeEnum SearchMode
        {
            get
            {
                if (ViewState["SearchMode"] == null)
                {
                    ViewState["SearchMode"] = SearchScopeEnum.SearchAllContent;
                }
                return ((SearchModeEnum)(ViewState["SearchMode"]));
            }
            set
            {
                ViewState["SearchMode"] = (SearchScopeEnum)value;
            }
        }


        /// <summary>
        /// Search mode - any word, all words or exact phrase.
        /// </summary>
        [Category("Behavior"), DefaultValue(SearchModeEnum.AnyWord), Description("Search mode - any word, all words or exact phrase.")]
        public SearchScopeEnum SearchScope
        {
            get
            {
                if (ViewState["SearchScope"] == null)
                {
                    ViewState["SearchScope"] = EnumHelper.GetDefaultValue<SearchModeEnum>();
                }
                return ((SearchScopeEnum)(ViewState["SearchScope"]));
            }
            set
            {
                ViewState["SearchScope"] = value;
            }
        }


        /// <summary>
        /// Optionally, you can use this property to specify the ID of the source CMSSearchDialog control that provides search parameters.
        /// </summary>
        [Category("Data"), DefaultValue(""), Description("Optionally, you can use this property to specify the ID of the source CMSSearchDialog control that provides search parameters.")]
        public string CMSSearchDialogID
        {
            get
            {
                return Convert.ToString(ViewState["CMSSearchDialogID"]);
            }
            set
            {
                ViewState["CMSSearchDialogID"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue("cms.root.searchresults"), Description("Transformation name in format application.class.transformation.")]
        public string TransformationName
        {
            get
            {
                if (DataHelper.IsEmpty(ViewState["TransformationName"]))
                {
                    ViewState["TransformationName"] = "cms.root.searchresults";
                }
                return Convert.ToString(ViewState["TransformationName"]);
            }
            set
            {
                ViewState["TransformationName"] = value;
            }
        }


        /// <summary>
        /// Label with 'no results' text.
        /// </summary>
        public Label NoResultsLabel
        {
            get
            {
                return mNoResultsLabel;
            }
        }


        /// <summary>
        /// Filter control.
        /// </summary>
        public CMSAbstractBaseFilterControl FilterControl
        {
            get
            {
                if (mFilterControl == null)
                {
                    if (FilterName != null)
                    {
                        mFilterControl = CMSControlsHelper.GetFilter(FilterName) as CMSAbstractBaseFilterControl;
                    }
                }
                return mFilterControl;
            }
            set
            {
                mFilterControl = value;
            }
        }


        /// <summary>
        /// Gets or Set filter name.
        /// </summary>
        public string FilterName
        {
            get
            {
                return mFilterName;
            }
            set
            {
                mFilterName = value;
            }
        }


        /// <summary>
        /// Data pager control.
        /// </summary>
        [Category("Data Pager"), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.Attribute), NotifyParentProperty(true), RefreshProperties(RefreshProperties.Repaint)]
        public DataPager PagerControl
        {
            get
            {
                return mDataPager;
            }
            set
            {
                mDataPager = value;
            }
        }


        /// <summary>
        /// Enables paging on Data control.
        /// </summary>
        [Category("Data Pager"), DefaultValue(true)]
        public bool EnablePaging
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EnablePaging"], true);
            }
            set
            {
                ViewState["EnablePaging"] = value;
            }
        }


        /// <summary>
        /// Query string key used for data pager URL parameter.
        /// </summary>
        public string QueryStringKey
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
        /// Item template for direct access.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate ItemTemplate
        {
            get
            {
                return mRepeater.ItemTemplate;
            }
            set
            {
                mRepeater.ItemTemplate = value;
            }
        }


        /// <summary>
        /// Header template for direct access.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate HeaderTemplate
        {
            get
            {
                return mRepeater.HeaderTemplate;
            }
            set
            {
                mRepeater.HeaderTemplate = value;
            }
        }


        /// <summary>
        /// Footer template for direct access.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Browsable(false)]
        public ITemplate FooterTemplate
        {
            get
            {
                return mRepeater.FooterTemplate;
            }
            set
            {
                mRepeater.FooterTemplate = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Finds the search dialog for the given control.
        /// </summary>
        /// <param name="sender">Control that request to find the search dialog</param>
        /// <param name="dialogId">Search dialog ID</param>
        public static CMSSearchDialog FindSearchDialog(Control sender, string dialogId)
        {
            if (dialogId == null)
            {
                dialogId = "";
            }
            return FindSearchDialog(sender, dialogId, true);
        }


        /// <summary>
        /// Finds the search dialog within the controls structure.
        /// </summary>
        /// <param name="sender">Control that requests to find the search dialog</param>
        /// <param name="dialogId">Search dialog ID</param>
        /// <param name="searchUp">If true, parent controls are searched</param>
        public static CMSSearchDialog FindSearchDialog(Control sender, string dialogId, bool searchUp)
        {
            // If found or no control given, return the original value
            if ((sender == null) || (sender is CMSSearchDialog))
            {
                if (CMSString.Compare(sender.ID, dialogId, true) == 0)
                {
                    return (CMSSearchDialog)sender;
                }
            }
            // Search up or down the tree
            if (searchUp)
            {
                // If no parent found, switch to search down
                if (sender.Parent == null)
                {
                    return FindSearchDialog(sender, dialogId, false);
                }
                else
                {
                    // Search up the tree
                    Control parent = sender.Parent;
                    if ((parent is CMSSearchDialog) && ((CMSString.Compare(parent.ID, dialogId, true) == 0) || (dialogId == "")))
                    {
                        return (CMSSearchDialog)parent;
                    }
                    else
                    {
                        return FindSearchDialog(parent, dialogId, true);
                    }
                }
            }
            else
            {
                // Search down the tree
                foreach (Control child in sender.Controls)
                {
                    // Check if the control is manager
                    if ((child is CMSSearchDialog) && ((CMSString.Compare(child.ID, dialogId, true) == 0) || (dialogId == "")))
                    {
                        return (CMSSearchDialog)child;
                    }
                    else
                    {
                        // Search the inner controls
                        CMSSearchDialog result = FindSearchDialog(child, dialogId, false);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Loads ItemTemplate according to the current values of properties.
        /// </summary>
        private void LoadItemTemplate()
        {
            // Load transformations
            if ((TransformationName != "") && !IgnoreTransformations)
            {
                mRepeater.ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName);
            }
        }


        /// <summary>
        /// Returns the search mode based on the given string.
        /// </summary>
        /// <param name="searchMode">String mode representation</param>
        public SearchModeEnum GetSearchMode(string searchMode)
        {
            return searchMode.ToEnum<SearchModeEnum>();
        }


        /// <summary>
        /// Reload data.
        /// </summary>
        public void ReloadData(bool forceLoad)
        {
            if (!StopProcessing)
            {
                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                if (forceLoad)
                {
                    mSearchDialog = null;
                }

                mDataPager.OnPageChange += mDataPager_OnPageChange;

                LoadItemTemplate();

                // Set variables
                DataSet resultDS;
                string searchExpression;
                string searchPath;
                string searchQuery = null;
                SearchModeEnum searchMode = SearchMode;
                SearchScopeEnum searchScope = SearchScope;

                // Get search text from query
                if (QueryHelper.Contains("searchtext"))
                {
                    searchQuery = QueryHelper.GetString("searchtext", String.Empty);
                }

                // Get search mode frorm query
                if (QueryHelper.Contains("searchmode"))
                {
                    searchMode = QueryHelper.GetString("searchmode", String.Empty).ToEnum<SearchModeEnum>();
                }

                // Get search scope from query
                if (QueryHelper.Contains("searchscope"))
                {
                    switch (QueryHelper.GetString("searchscope", String.Empty))
                    {
                        // Current section
                        case "searchcurrentsection":
                            searchScope = SearchScopeEnum.SearchCurrentSection;
                            break;
                        // All content
                        default:
                            searchScope = SearchScopeEnum.SearchAllContent;
                            break;
                    }
                }

                // Find Search dialog
                if (mSearchDialog == null)
                {
                    mSearchDialog = FindSearchDialog(this, CMSSearchDialogID);
                }

                // Search by values from search dialog
                if (mSearchDialog != null)
                {
                    if (mSearchDialog.SearchExpression == null || mSearchDialog.SearchExpression.Trim() == "")
                    {
                        if (searchQuery != null)
                        {
                            mSearchDialog.SearchExpression = searchQuery;
                        }
                        else
                        {
                            InvokeSearchCompleted();
                            return;
                        }
                    }

                    // Get search mode
                    if (QueryHelper.Contains("searchmode"))
                    {
                        if (!RequestHelper.IsPostBack())
                        {
                            mSearchDialog.SearchMode = searchMode;
                        }
                    }

                    // Get search scope
                    if (QueryHelper.Contains("searchscope"))
                    {
                        if (!RequestHelper.IsPostBack())
                        {
                            mSearchDialog.SearchScope = searchScope;
                        }
                    }

                    searchExpression = mSearchDialog.SearchExpression;
                    var searchModeDialog = mSearchDialog.SearchMode;
                    var searchScopeDialog = mSearchDialog.SearchScope;

                    // Set data pager
                    mDataPager.RemoveFromUrl = false;
                    if ((searchQuery != null && (searchQuery != searchExpression)) || (searchMode != searchModeDialog) || (searchScope != searchScopeDialog))
                    {
                        mDataPager.RemoveFromUrl = true;
                        mDataPager.RemoveKeys = new string[3];
                        mDataPager.RemoveKeys[0] = "searchtext";
                        mDataPager.RemoveKeys[1] = "searchmode";
                        mDataPager.RemoveKeys[2] = "searchscope";
                    }

                    // Set path
                    if (!string.IsNullOrEmpty(Path))
                    {
                        searchPath = MacroResolver.ResolveCurrentPath(Path);
                    }
                    else
                    {
                        searchPath = "/%";

                        if (searchScopeDialog == SearchScopeEnum.SearchCurrentSection)
                        {
                            searchPath = DocumentContext.OriginalAliasPath;
                        }
                    }

                    // Search results
                    resultDS = TreeProvider.Search(SiteName, searchPath, CultureCode, mSearchDialog.SearchExpression, mSearchDialog.SearchMode, true, ClassNames, false, SelectOnlyPublished, WhereCondition, OrderBy, CombineWithDefaultCulture);
                }
                else // search by query search expression
                {
                    if (SearchExpression == null || SearchExpression.Trim() == "")
                    {
                        if (searchQuery != null)
                        {
                            SearchExpression = searchQuery;
                        }
                        else
                        {
                            InvokeSearchCompleted();
                            return;
                        }
                    }

                    // Set path
                    if (!string.IsNullOrEmpty(Path))
                    {
                        searchPath = Path;
                    }
                    else
                    {
                        searchPath = "/%";

                        if (searchScope == SearchScopeEnum.SearchCurrentSection)
                        {
                            searchPath = DocumentContext.OriginalAliasPath;
                        }
                    }

                    searchExpression = SearchExpression;

                    // Search results
                    resultDS = TreeProvider.Search(SiteName, searchPath, CultureCode, SearchExpression + "", searchMode, true, ClassNames, false, SelectOnlyPublished, WhereCondition, OrderBy, CombineWithDefaultCulture);
                }


                // check permissions
                if (CheckPermissions)
                {
                    UserInfo userInfo = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);
                    resultDS = TreeSecurityProvider.FilterDataSetByPermissions(resultDS, NodePermissionsEnum.Read, userInfo);
                }

                mNoResult = DataHelper.DataSourceIsEmpty(resultDS) && (Context != null);

                // When nothing was found
                if (!mNoResult)
                {
                    DataSource = resultDS;

                    mDataPager.mQueryStringKey = mQueryStringKey;
                    mDataPager.Page = Page;

                    // Need databind to orderBy
                    if ((ValidationHelper.GetString(OrderBy, "").Trim() != "") &&
                        (!DataHelper.DataSourceIsEmpty(resultDS)))
                    {
                        resultDS.Tables[0].DefaultView.Sort = OrderBy;
                        mRepeater.DataSource = resultDS.Tables[0].DefaultView;
                        mRepeater.DataBind();
                    }
                    else
                    {
                        mRepeater.DataSource = resultDS;
                        mRepeater.DataBind();
                    }

                    Controls.Clear();

                    if (EnablePaging)
                    {
                        // Add data pager control
                        Controls.Add(mDataPager);

                        mDataPager.DataSource = mRepeater.DataSource;
                        DataSource = resultDS;
                        mRepeater.DataSource = mDataPager.PagedData;

                        if ((forceLoad) && (!DataHelper.DataSourceIsEmpty(resultDS)))
                        {
                            if ((RequestHelper.IsPostBack()) &&
                                (mDataPager.PagingMode == PagingModeTypeEnum.QueryString))
                            {
                                mDataPager.ChangePage(1);
                            }
                        }

                        if (!QueryHelper.Contains("searchtext") || mDataPager.RemoveFromUrl)
                        {
                            // Set data pager properties
                            mDataPager.InsertToUrl = true;
                            mDataPager.InsertKeys = new string[3, 2];
                            mDataPager.InsertKeys[0, 0] = "searchtext";
                            mDataPager.InsertKeys[0, 1] = searchExpression;
                            mDataPager.InsertKeys[1, 0] = "searchmode";
                            mDataPager.InsertKeys[1, 1] = searchMode.ToString();
                            mDataPager.InsertKeys[2, 0] = "searchscope";
                            mDataPager.InsertKeys[2, 1] = searchScope.ToString();
                        }
                    }

                    Controls.Add(mRepeater);

                    mRenderIt = true;
                }
            }
            InvokeSearchCompleted();
        }


        private void InvokeSearchCompleted()
        {
            // Call handler
            if (OnSearchCompleted != null)
            {
                OnSearchCompleted(mNoResult, mRenderIt);
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (Context == null)
            {
                return;
            }

            Page.InitComplete += Page_InitComplete;
        }


        /// <summary>
        /// Init complete event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Page_InitComplete(object sender, EventArgs e)
        {
            // Add filter change event
            if (FilterControl != null)
            {
                FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                ReloadData(true);
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Context == null)
            {
                return;
            }

            ReloadData(false);
        }


        /// <summary>
        /// OnPage change.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void mDataPager_OnPageChange(object sender, EventArgs e)
        {
            mRepeater.DataSource = mDataPager.PagedData;
            mRepeater.DataBind();
        }


        /// <summary>
        /// Renders the results.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                // Show label for visual studio design mode
                output.Write("[ CMSSearchResult : " + ClientID + " ]");
                return;
            }


            if (!StopProcessing)
            {
                if (!string.IsNullOrEmpty(CssClass))
                {
                    output.Write("<div class=\"{0}\">", CssClass);
                }

                if (mNoResult)
                {
                    mNoResultsLabel.RenderControl(output);
                    return;
                }

                if (mRenderIt)
                {
                    // Render DataPager and Repeater
                    if (EnablePaging && PagerControl.PagerPosition == PagingPlaceTypeEnum.Bottom)
                    {
                        mRepeater.RenderControl(output);
                        mDataPager.RenderControl(output);
                    }

                    if (EnablePaging && PagerControl.PagerPosition == PagingPlaceTypeEnum.Top)
                    {
                        mDataPager.RenderControl(output);
                        mRepeater.RenderControl(output);
                    }

                    if (EnablePaging && PagerControl.PagerPosition == PagingPlaceTypeEnum.TopAndBottom)
                    {
                        mDataPager.RenderControl(output);
                        mRepeater.RenderControl(output);
                        mDataPager.RenderControl(output);
                    }

                    if (!EnablePaging)
                    {
                        mRepeater.RenderControl(output);
                    }
                }

                if (!string.IsNullOrEmpty(CssClass))
                {
                    output.Write("</div>");
                }
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSSearchResults()
        {
            if (Context == null)
            {
                return;
            }

            mProperties.ParentControl = this;

            mDataPager = new DataPager();
            mDataPager.ID = "srchDataPager";

            mNoResultsLabel = new Label();
            mNoResultsLabel.Text = ResHelper.GetString("CMSSearchResults.NoDocumentFound");
            mNoResultsLabel.CssClass = "CMSSearchResultsNoResultsLabel";
        }


        /// <summary>
        /// Data filter control handler.
        /// </summary>
        private void FilterControl_OnFilterChanged()
        {
            FilterControl.InitDataProperties(mProperties);
            Path = mProperties.Path;
            ReloadData(true);
        }

        #endregion
    }
}
