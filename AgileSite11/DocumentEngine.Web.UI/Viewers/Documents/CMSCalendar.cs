using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSCalendar class.
    /// </summary>
    [ToolboxData("<{0}:CMSCalendar runat=server></{0}:CMSCalendar>"), Serializable]
    public class CMSCalendar : BasicCalendar, ICMSControlProperties
    {
        #region "Variables"

        /// <summary>
        /// Control properties variable.
        /// </summary>
        protected CMSDataProperties mProperties = new CMSDataProperties();

        /// <summary>
        /// Filter name.
        /// </summary>
        protected string mFilterName = null;

        /// <summary>
        /// Filter control.
        /// </summary>
        protected CMSAbstractBaseFilterControl mFilterControl = null;

        /// <summary>
        /// Indicates whether data should be loaded.
        /// </summary>
        protected bool mDataLoaded = false;

        #endregion


        #region "CMS Control Properties"

        /// <summary>
        /// Stop processing 
        /// </summary>
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
        /// No event transformation name in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("No event transformation name in format application.class.transformation.")]
        public string NoEventTransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["NoEventTransformationName"], "");
            }
            set
            {
                ViewState["NoEventTransformationName"] = value;
            }
        }


        /// <summary>
        /// Transformation name in format application.class.transformation.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Transformation name in format application.class.transformation.")]
        public string TransformationName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["TransformationName"], "");
            }
            set
            {
                ViewState["TransformationName"] = value;
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
        /// Property to set and get the classnames list (separated by the semicolon).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the classnames list (separated by the semicolon).")]
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
        /// Property to set and get the category name for filtering documents.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the category name for filtering documents.")]
        public virtual string CategoryName
        {
            get
            {
                return mProperties.CategoryName;
            }
            set
            {
                mProperties.CategoryName = value;
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
                return mProperties.Path;
            }
            set
            {
                mProperties.Path = value;
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
        /// Property to set and get the WhereCondition.
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
        /// Property to set and get the OrderBy.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Order By expression.")]
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
        /// Property to set and get the MaxRelativeLevel.
        /// </summary>
        [Category("Behavior"), DefaultValue(-1), Description("Maximum relative node level to be returned. Value 1 returns only the node itself. Use -1 for unlimited recurrence.")]
        public virtual int MaxRelativeLevel
        {
            get
            {
                return mProperties.MaxRelativeLevel;
            }
            set
            {
                mProperties.MaxRelativeLevel = value;
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
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Gets or sets the columns to be retrieved from database.")]
        public virtual string SelectedColumns
        {
            get
            {
                return mProperties.SelectedColumns;
            }
            set
            {
                mProperties.SelectedColumns = value;
            }
        }


        /// <summary>
        /// Gets or sets the columns to be retrieved from database.
        /// </summary>    
        public string Columns
        {
            get
            {
                return SelectedColumns;
            }
            set
            {
                SelectedColumns = value;
            }
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Select top N rows.")]
        public int TopN
        {
            get
            {
                return mProperties.TopN;
            }
            set
            {
                mProperties.TopN = value;
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
        public new virtual DataSet DataSource
        {
            get
            {
                return (DataSet)base.DataSource;
            }
            set
            {
                base.DataSource = value;
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
        /// Select nodes with given relationship with given node.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Select nodes with given relationship with given node."), ReadOnly(true)]
        public Guid RelationshipWithNodeGuid
        {
            get
            {
                return ValidationHelper.GetGuid(ViewState["RelationshipWithNodeGuid"], Guid.Empty);
            }
            set
            {
                Guid mGuid = ValidationHelper.GetGuid(value, Guid.Empty);

                if ((mGuid.ToString() == ("11111111-1111-1111-1111-111111111111")) && (DocumentContext.CurrentDocument != null))
                {
                    ViewState["RelationshipWithNodeGuid"] = ValidationHelper.GetGuid(DocumentContext.CurrentDocument.NodeGUID, Guid.Empty);
                }
                else
                {
                    ViewState["RelationshipWithNodeGuid"] = mGuid;
                }
            }
        }


        /// <summary>
        /// Name of the relationship.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Name of the relationship.")]
        public string RelationshipName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["RelationshipName"], "");
            }
            set
            {
                ViewState["RelationshipName"] = value;
            }
        }


        /// <summary>
        /// If true, the returned nodes are on the right side of the relationship.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("If true, the returned nodes are on the right side of the relationship.")]
        public bool RelatedNodeIsOnTheLeftSide
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RelatedNodeIsOnTheLeftSide"], true);
            }
            set
            {
                ViewState["RelatedNodeIsOnTheLeftSide"] = value;
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext
        {
            get
            {
                return mProperties.ControlContext;
            }
            set
            {
                mProperties.ControlContext = value;
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
                    if (!String.IsNullOrEmpty(FilterName))
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

        #endregion //public properties


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSCalendar()
        {
            if (Context == null)
            {
                return;
            }

            mProperties.ParentControl = this;
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (FilterControl != null)
            {
                FilterControl.OnFilterChanged += FilterControl_OnFilterChanged;
            }

            if (!StopProcessing && (Context != null))
            {
                ReloadData(false);
                base.CreateChildControls();
            }
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter html)
        {
            if (!StopProcessing)
            {
                base.Render(html);
            }
        }


        /// <summary>
        /// OnLoad.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (Context == null)
            {
                return;
            }

            if (!StopProcessing)
            {
                base.OnLoad(e);

                // Register edit mode buttons script
                if ((PortalContext.ViewMode != ViewModeEnum.LiveSite) && (PortalContext.ViewMode != ViewModeEnum.EditLive))
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.EDIT_DOCUMENT_SCRIPT_KEY, CMSControlsHelper.EditDocumentScript);
                }

                EnsureChildControls();
            }
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        /// <param name="forceLoad">Indicates force load</param>
        public void ReloadData(bool forceLoad)
        {
            if (!mDataLoaded || forceLoad)
            {
                LoadItemTemplate();
                LoadData(forceLoad);
            }
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public virtual void ClearCache()
        {
            mProperties.ClearCache();
        }


        /// <summary>
        /// OnInit Handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            RaiseOnBeforeInit();
            base.OnInit(e);
        }


        /// <summary>
        /// Loads ItemTemplate according to the current values of properties.
        /// </summary>
        private void LoadItemTemplate()
        {
            // Load event transformation
            if (!string.IsNullOrEmpty(TransformationName))
            {
                ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName);
            }
            // Load no event transformation
            if (!string.IsNullOrEmpty(NoEventTransformationName))
            {
                NoEventsTemplate = TransformationHelper.LoadTransformation(this, NoEventTransformationName);
            }
        }


        /// <summary>
        /// Loads data from the database according to the current values of properties.
        /// </summary>
        /// <param name="forceLoad">Indicates force load</param>
        private void LoadData(bool forceLoad)
        {
            if ((DataSource == null) || forceLoad)
            {
                // Path must be specified
                if (String.IsNullOrEmpty(Path))
                {
                    return;
                }

                // Class names must be specified
                if (String.IsNullOrEmpty(ClassNames))
                {
                    return;
                }

                SetContext();

                // Get the data
                DataSource = GetDataSet();

                DataBind();

                ReleaseContext();
            }
        }


        /// <summary>
        /// Retrieves DataSet.
        /// </summary>
        protected virtual DataSet GetDataSet()
        {
            // Prepare the path
            string path = MacroResolver.ResolveCurrentPath(Path);

            if (!string.IsNullOrEmpty(mProperties.SelectedColumns))
            {
                // Ensure SelectedItemIDColumn
                mProperties.SelectedColumns = SqlHelper.MergeColumns(mProperties.SelectedColumns, SelectedItemIDColumn);
            }
            // Get the data
            return mProperties.GetDataSet(path);
        }


        /// <summary>
        /// Sets the web part context.
        /// </summary>
        public virtual void SetContext()
        {
            mProperties.SetContext();
        }


        /// <summary>
        /// Releases the web part context.
        /// </summary>
        public virtual void ReleaseContext()
        {
            mProperties.ReleaseContext();
        }


        /// <summary>
        /// Data filter control handler.
        /// </summary>
        private void FilterControl_OnFilterChanged()
        {
            FilterControl.InitDataProperties(mProperties);
            ReloadData(true);
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public virtual string GetDefaultCacheDependencies()
        {
            return mProperties.GetDefaultCacheDependencies();
        }


        /// <summary>
        /// Gets the cache dependency for the control.
        /// </summary>
        public virtual CMSCacheDependency GetCacheDependency()
        {
            return mProperties.GetCacheDependency();
        }

        #endregion
    }
}
