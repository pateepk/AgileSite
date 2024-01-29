using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System;
using System.Data;
using System.Web.UI.Design;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Bread crumbs control that displays current location within Web site.
    /// </summary>
    [DefaultProperty("Text"), ToolboxData("<{0}:CMSBreadCrumbs runat=server></{0}:CMSBreadCrumbs>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSBreadCrumbs : CMSAbstractMenuProperties
    {
        #region "Variables"

        /// <summary>
        /// Rendered HTML code.
        /// </summary>
        private string mRenderedHTML = String.Empty;

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        #endregion


        #region "CMS Control Properties"

        /// <summary>
        /// Property to set and get the Path.
        /// </summary>
        [Category("Behavior"), DefaultValue("/%"), Description("Path to the menu items that should be displayed in the site map.")]
        public override string Path
        {
            get
            {
                if ((ViewState["Path"] == null) || (ViewState["Path"].ToString() == String.Empty))
                {
                    if (!String.IsNullOrEmpty(DefaultPath))
                    {
                        ViewState["Path"] = DefaultPath;
                        base.Path = DefaultPath;
                    }
                }

                return base.Path;
            }

            set
            {
                base.Path = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// The document value ShowInNavigation is ignored if this property is true.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("The document value HideInNavigation is ignored if this property is true.")]
        public bool IgnoreShowInNavigation
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IgnoreShowInNavigation"], false);
            }
            set
            {
                ViewState["IgnoreShowInNavigation"] = value;
            }
        }


        /// <summary>
        /// Indicates if breadcrumbs is rendered with rtl direction for specific languages.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if breadcrumbs is rendered with rtl direction for specific languages")]
        public bool UseRtlBehaviour
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["UseRtlBehaviour"], true);
            }
            set
            {
                ViewState["UseRtlBehaviour"] = value;
            }
        }


        /// <summary>
        /// Gets or set rendered HTML code.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Get or set rendered HTML code")]
        public string RenderedHTML
        {
            get
            {
                return mRenderedHTML;
            }
            set
            {
                mRenderedHTML = value;
            }
        }


        /// <summary>
        /// Indicates if data should be loaded automatically.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if data will be loaded automaticaly.")]
        public bool LoadDataAutomaticaly
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["LoadDataAutomaticaly"], true);
            }
            set
            {
                ViewState["LoadDataAutomaticaly"] = value;
            }
        }


        /// <summary>
        /// Indicates if the current (last) item should be displayed.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if the current (last) item should be displayed.")]
        public bool ShowCurrentItem
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowCurrentItem"], true);
            }
            set
            {
                ViewState["ShowCurrentItem"] = value;
            }
        }


        /// <summary>
        /// Indicates if the current (last) item should be displayed as a link.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the current (last) item should be displayed as a link.")]
        public bool ShowCurrentItemAsLink
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowCurrentItemAsLink"], false);
            }
            set
            {
                ViewState["ShowCurrentItemAsLink"] = value;
            }
        }


        /// <summary>
        /// Path to the node whose path should be displayed. This value is used in case no Path value is provided and no alias path is provided through friendly URL.
        /// </summary>
        [Category("Behavior"), Description("Path to the node whose path should be displayed. This value is used in case no Path value is provided and no alias path is provided through friendly URL.")]
        public string DefaultPath
        {
            get
            {
                if (((ViewState["DefaultPath"] == null) || (Convert.ToString(ViewState["DefaultPath"]) == String.Empty)) && (Context != null))
                {
                    ViewState["DefaultPath"] = DocumentContext.OriginalAliasPath;
                }
                return Convert.ToString(ViewState["DefaultPath"]);
            }
            set
            {
                ViewState["DefaultPath"] = value;
            }
        }


        /// <summary>
        /// Starting part of the path.
        /// </summary>
        [Category("Behavior"), DefaultValue("/"), Description("Starting part of the path.")]
        public string StartingPath
        {
            get
            {
                if ((ViewState["SelectNodesStartPath"] == null) || (Convert.ToString(ViewState["SelectNodesStartPath"]).Trim() == String.Empty))
                {
                    return "/";
                }
                else
                {
                    if ((Convert.ToString(ViewState["SelectNodesStartPath"])).EndsWithCSafe("/"))
                    {
                        return Convert.ToString(ViewState["SelectNodesStartPath"]);
                    }
                    else
                    {
                        return Convert.ToString(ViewState["SelectNodesStartPath"]) + "/";
                    }
                }
            }
            set
            {
                ViewState["SelectNodesStartPath"] = value;
            }
        }


        /// <summary>
        /// Specifies target frame for all URLs.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("If URL for tab items is set, this property specifies target frame for all URLs.")]
        public string UrlTarget
        {
            get
            {
                return Convert.ToString(ViewState["UrlTarget"]);
            }
            set
            {
                ViewState["UrlTarget"] = value;
            }
        }


        /// <summary>
        /// Character(s) that separate the bread crumbs. You can use HTML code here.
        /// </summary>
        [Category("Appearance"), DefaultValue("&gt;"), Description("Character(s) that separate the bread crumbs. You can use HTML code here.")]
        public string BreadCrumbSeparator
        {
            get
            {
                return ValidationHelper.GetString(ViewState["BreadCrumbSeparator"], "&gt;");
            }
            set
            {
                ViewState["BreadCrumbSeparator"] = value;
            }
        }


        /// <summary>
        /// Character(s) that separate the bread crumbs. You can use HTML code here.
        /// </summary>
        [Category("Appearance"), DefaultValue("&lt;"), Description("RTL character(s) that separate the bread crumbs. You can use HTML code here.")]
        public string BreadCrumbSeparatorRTL
        {
            get
            {
                return ValidationHelper.GetString(ViewState["BreadCrumbSeparatorRTL"], "&lt;");
            }
            set
            {
                ViewState["BreadCrumbSeparatorRTL"] = value;
            }
        }


        /// <summary>
        /// Render the link title attribute?
        /// </summary>
        [Category("Behavior"), Description("Render the title attribute within the menu links.")]
        public bool RenderLinkTitle
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderLinkTitle"], false);
            }
            set
            {
                ViewState["RenderLinkTitle"] = value;
            }
        }


        /// <summary>
        /// Encodes name.
        /// </summary>
        [Category("Behavior"), Description("Encode name of current link.")]
        public bool EncodeName
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EncodeName"], true);
            }
            set
            {
                ViewState["EncodeName"] = value;
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

        #endregion //Public properties


        #region "Methods"

        /// <summary>
        /// Reload data.
        /// </summary>
        /// <param name="forceReload">If is true data will be always loaded</param>
        public override void ReloadData(bool forceReload)
        {
            SetContext();

            // Clear rendered HTML code
            mRenderedHTML = String.Empty;

            // Do not load data if is stop processing set
            if ((StopProcessing) || (Context == null))
            {
                return;
            }

            // Load DataSource
            if ((DataSource == null) || (forceReload))
            {
                if (FilterControl != null)
                {
                    FilterControl.InitDataProperties(this);
                }

                DataSource = GetDataSet();
            }

            if (DataHelper.DataSourceIsEmpty(DataSource))
            {
                return;
            }

            string fullAliasPath = DocumentContext.OriginalAliasPath;
            StartingPath = MacroResolver.ResolveCurrentPath(StartingPath);

            string[] startaliases = StartingPath.TrimEnd('%').TrimEnd('/').Split('/');

            // Prepare the classes
            string classnames = null;
            if (!String.IsNullOrEmpty(ClassNames))
            {
                classnames = ";" + DocumentTypeHelper.GetClassNames(ClassNames).ToLowerCSafe() + ";";
            }

            // Get the list of items
            List<DataRow> infos = new List<DataRow>();
            foreach (DataRow dr in DataSource.Tables[0].Rows)
            {
                bool hideInNavigation = ValidationHelper.GetBoolean(dr["DocumentMenuItemHideInNavigation"], false) & (!IgnoreShowInNavigation);
                if (!hideInNavigation)
                {
                    string aliasPath = ValidationHelper.GetString(dr["NodeAliasPath"], String.Empty);
                    string className = ValidationHelper.GetString(dr["ClassName"], String.Empty);
                    if (CheckPath(aliasPath, startaliases))
                    {
                        // Check classes
                        if ((classnames != null) && !classnames.Contains(";" + className.ToLowerCSafe() + ";"))
                        {
                        }
                        else
                        {
                            if (ShowCurrentItem || (fullAliasPath.ToLowerCSafe() != aliasPath.ToLowerCSafe()))
                            {
                                infos.Add(dr);
                            }
                        }
                    }
                }
            }

            // Need rtl rendering
            bool jrtl = false;

            // Set rtl rendering only if it is enabled
            if (UseRtlBehaviour)
            {
                jrtl = CultureHelper.IsCultureRTL(CultureCode);
            }

            var localResolver = ContextResolver.CreateChild();
            StringBuilder strBuild = new StringBuilder(infos.Count * 100);

            // Create breadcrumbs code
            for (int indx = 0; indx < infos.Count; indx++)
            {
                // Get the data
                DataRow dr = infos[infos.Count - indx - 1];
                if (jrtl)
                {
                    dr = infos[indx];
                }

                // Add current datarow to the resolver
                localResolver.SetAnonymousSourceData(dr);

                string aliasPath = ValidationHelper.GetString(dr["NodeAliasPath"], String.Empty);

                bool currentItem = (aliasPath.ToLowerCSafe() == fullAliasPath.ToLowerCSafe());

                // Prepare the item name. Disable encoding. Encoding depends on "EncodeMenuCaption" property
                localResolver.Settings.EncodeResolvedValues = false;
                string linkName = localResolver.ResolveMacros(TreePathUtils.GetMenuCaption(ValidationHelper.GetString(dr["DocumentMenuCaption"], String.Empty), ValidationHelper.GetString(dr["DocumentName"], String.Empty)));
                localResolver.Settings.EncodeResolvedValues = true;

                if (EncodeName)
                {
                    linkName = HTMLHelper.HTMLEncode(linkName);
                }

                string style = String.Empty;
                string jClass = currentItem ? "CMSBreadCrumbsCurrentItem" : "CMSBreadCrumbsLink";

                // Apply class / style
                if (ApplyMenuDesign)
                {
                    string menuStyle = ValidationHelper.GetString(dr["DocumentMenuStyle"], String.Empty);
                    if (menuStyle != String.Empty)
                    {
                        style = " style=\"" + menuStyle + "\"";
                    }

                    string menuClass = ValidationHelper.GetString(dr["DocumentMenuClass"], String.Empty);
                    if (menuClass != String.Empty)
                    {
                        jClass = menuClass;
                    }
                }

                // Current item
                if ((currentItem) && (ShowCurrentItem) && (!ShowCurrentItemAsLink))
                {
                    strBuild.Append("<span " + style + " class=\"" + jClass + "\">");
                    strBuild.Append(linkName);
                    strBuild.Append("</span>");

                    // RTL Separator
                    if ((jrtl) && (infos.Count > 1))
                    {
                        strBuild.Append(" " + BreadCrumbSeparatorRTL + " ");
                    }
                }
                else
                {
                    // Inactive item
                    if (ValidationHelper.GetBoolean(dr["DocumentMenuItemInactive"], false))
                    {
                        strBuild.Append(@"<a href=""#"" onclick=""return false;""");
                    }
                    else
                    {
                        // Get URL of link
                        var url = DocumentURLProvider.GetNavigationUrl(new DataRowContainer(dr), localResolver);

                        strBuild.Append(@"<a href=""");
                        strBuild.Append(url);
                        strBuild.Append(@"""");
                    }

                    // Set target
                    if (!String.IsNullOrEmpty(UrlTarget))
                    {
                        strBuild.Append(@" target=""");
                        strBuild.Append(UrlTarget);
                        strBuild.Append(@"""");
                    }

                    // Menu item JavaScript
                    string javascript = ValidationHelper.GetString(dr["DocumentMenuJavascript"], String.Empty);
                    if (javascript != String.Empty)
                    {
                        strBuild.Append(" onclick=\"" + localResolver.ResolveMacros(ValidationHelper.GetString(javascript, "")) + "\" ");
                    }

                    // Render link title
                    if (RenderLinkTitle)
                    {
                        strBuild.Append(" title=\"" + HTMLHelper.HTMLEncode(linkName) + "\"");
                    }

                    strBuild.Append(style + " class=\"" + jClass + "\">");
                    strBuild.Append(linkName);
                    strBuild.Append("</a> ");

                    // Add separator
                    if ((currentItem) && (ShowCurrentItem) && (!jrtl))
                    {
                    }
                    else
                    {
                        if (indx != (infos.Count - 1))
                        {
                            string sep = BreadCrumbSeparator + " ";
                            if (jrtl)
                            {
                                sep = BreadCrumbSeparatorRTL + " ";
                            }

                            strBuild.Append(sep);
                        }
                    }
                }
            }

            // Render breadcrumbs
            mRenderedHTML = strBuild.ToString();

            ReleaseContext();
        }


        /// <summary>
        /// Returns true if current alias path could be displayed.
        /// </summary>
        /// <param name="currentPath">Current alias path</param>
        /// <param name="startingPath">String array with start path items</param>
        protected static bool CheckPath(string currentPath, string[] startingPath)
        {
            string[] tmp = currentPath.Split('/');

            // Check differences in paths, if is different ==> Do not display
            // If path is smaller or the same ==> Do not display
            return (startingPath == null) || ((tmp.Length > startingPath.Length) && (currentPath != "/") && (currentPath.Trim() != String.Empty) && (!startingPath.Where((t, i) => (t.ToLowerCSafe() != tmp[i].ToLowerCSafe())).Any()));
        }


        /// <summary>
        /// Renders bread crumbs.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Render only if is not stop processing or if is some HTML code generated
            if (!StopProcessing)
            {
                if (LoadDataAutomaticaly)
                {
                    ReloadData(false);
                }

                // Hide control for zero rows or display zero rows text
                if (DataHelper.DataSourceIsEmpty(DataSource))
                {
                    if (HideControlForZeroRows)
                    {
                        return;
                    }
                    else if (!string.IsNullOrEmpty(ZeroRowsText))
                    {
                        output.Write(ZeroRowsText);
                        return;
                    }
                }

                output.Write(mRenderedHTML);
            }
        }


        /// <summary>
        /// Ensures default data binding 
        /// </summary>
        /// <param name="loadPhase">Indicates whether Init is call from Load event</param>
        protected override void InitControl(bool loadPhase)
        {
            if ((LoadDataAutomaticaly) && (!StopProcessing))
            {
                if (FilterControl != null)
                {
                    FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
                }
                base.InitControl(loadPhase);
            }
        }


        /// <summary>
        /// Gets the cache dependencies for the breadcrumbs control. 
        /// Control depends on the current document and on the parent documents.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Override default navigation dependencies
            var path = GetStartingPath();
            var dependencies = DocumentDependencyCacheKeysBuilder.GetParentPathsDependencyCacheKeys(SiteName, path);

            return String.Join("\n", dependencies);
        }


        /// <summary>
        /// Returns DataRow with specified node.
        /// </summary>
        protected DataSet GetDataSet()
        {
            DataSet ds = null;

            // Try to get data from cache
            using (var cs = new CachedSection<DataSet>(ref ds, CacheMinutes, true, CacheItemName, "breadcrumbs", SiteName, Path, CacheHelper.GetCultureCacheKey(CultureCode), CombineWithDefaultCulture, ClassNames, WhereCondition, SelectOnlyPublished, SelectedColumns))
            {
                if (cs.LoadData)
                {
                    // Get the data
                    ds = GetNodeDataSet();

                    // Save the result to the cache
                    if (cs.Cached)
                    {
                        cs.CacheDependency = GetCacheDependency();
                    }

                    cs.Data = ds;
                }
            }

            return ds;
        }


        /// <summary>
        /// Returns DataSet with selected node.
        /// </summary>
        protected DataSet GetNodeDataSet()
        {
            bool isLivesite = PortalContext.ViewMode == ViewModeEnum.LiveSite;
            var path = GetStartingPath();

            // Get nesting level
            int nestingLevel = path.Split('/').Length - 1;

            // Prepare the where condition
            string where = SqlHelper.AddWhereCondition(WhereCondition, TreePathUtils.GetNodesOnPathWhereCondition(path, true, true).ToString(true));

            var query = DocumentHelper.GetDocuments()
                .PublishedVersion(isLivesite)
                .Types(ClassNames.Trim(';').Split(';'))
                .OnSite(SiteName)
                .Culture(CultureCode)
                .CombineWithDefaultCulture(CombineWithDefaultCulture)
                .Where(where)
                .OrderByDescending("NodeAliasPath")
                .NestingLevel(nestingLevel)
                .Published(isLivesite && SelectOnlyPublished)
                .CheckPermissions(CheckPermissions)
                .Columns(GetColumns());

            // Get coupled data if there is a where condition or order by expression specified
            bool getCoupled = !string.IsNullOrEmpty(WhereCondition) || !string.IsNullOrEmpty(OrderBy);
            if (getCoupled)
            {
                // When columns defined explicitly, do not include all columns to the result
                // Include columns in the inner query to be able to define WHERE condition containing these columns
                query.WithCoupledColumns(query.SelectColumnsList.AnyColumnsDefined ? IncludeCoupledDataEnum.InnerQueryOnly : IncludeCoupledDataEnum.Complete);
            }
            else
            {
                query.WithCoupledColumns(IncludeCoupledDataEnum.None);
            }

            return query.Result;
        }


        private string GetStartingPath()
        {
            var path = MacroResolver.ResolveCurrentPath(Path);
            return TreePathUtils.EnsureSingleNodePath(path);
        }


        /// <summary>
        /// Data filter control handler.
        /// </summary>
        private void FilterControl_OnFilterChanged()
        {
            ReloadData(true);
        }

        #endregion
    }
}