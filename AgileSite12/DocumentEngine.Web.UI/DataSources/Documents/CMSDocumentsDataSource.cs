using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Documents data source control.
    /// </summary>
    [ToolboxData("<{0}:CMSDocumentsDataSource runat=server></{0}:CMSDocumentsDataSource>"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CMSDocumentsDataSource : CMSControlDataSource, ICMSDataProperties
    {
        #region "Variables"

        internal ICMSDataProperties mProperties = new CMSDataProperties();

        private bool mLoadCurrentPageOnly = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Data properties
        /// </summary>
        internal CMSAbstractDataProperties Properties
        {
            get
            {
                return (CMSAbstractDataProperties)mProperties;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates that if a page is selected,
        /// the datasource will provide data for the selected page only.
        /// </summary>
        public bool LoadCurrentPageOnly
        {
            get
            {
                return mLoadCurrentPageOnly;
            }
            set
            {
                mLoadCurrentPageOnly = value;
            }
        }


        /// <summary>
        /// Transformation name for selected item in format application.class.transformation.
        /// </summary>
        public string SelectedItemTransformationName
        {
            get
            {
                return Properties.SelectedItemTransformationName;
            }
            set
            {
                Properties.SelectedItemTransformationName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// If true, the returned nodes are on the right side of the relationship.
        /// </summary>
        public bool RelatedNodeIsOnTheLeftSide
        {
            get
            {
                return Properties.RelatedNodeIsOnTheLeftSide;
            }
            set
            {
                Properties.RelatedNodeIsOnTheLeftSide = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Name of the relationship.
        /// </summary>
        public string RelationshipName
        {
            get
            {
                return Properties.RelationshipName;
            }
            set
            {
                Properties.RelationshipName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Select nodes with given relationship with given node.
        /// </summary>
        public Guid RelationshipWithNodeGuid
        {
            get
            {
                return Properties.RelationshipWithNodeGuid;
            }
            set
            {
                Properties.RelationshipWithNodeGuid = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize
        {
            get
            {
                return Properties.PageSize;
            }
            set
            {
                Properties.PageSize = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether current data source contains selected item.
        /// </summary>
        public override bool IsSelected
        {
            get
            {
                if (ViewState["IsSelected"] == null)
                {
                    ViewState["IsSelected"] = Properties.IsSelected;
                }
                return ValidationHelper.GetBoolean(ViewState["IsSelected"], false);
            }
            set
            {
                ViewState["IsSelected"] = value;
            }
        }


        /// <summary>
        /// Parent control.
        /// </summary>
        public Control ParentControl
        {
            get
            {
                return Properties.ParentControl;
            }
            set
            {
                Properties.ParentControl = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public CMSDocumentsDataSource()
        {
            LoadPagesIndividually = true;
            PropagateProperties(mProperties);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Propagates given settings
        /// </summary>
        /// <param name="properties">Settings</param>
        protected void PropagateProperties(ICMSDataProperties properties)
        {
            base.PropagateProperties(properties);
            mProperties = properties;
        }


        /// <summary>
        /// Gets data source according to caching properties.
        /// </summary>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <returns>Dataset with data source</returns>
        protected override object GetDataSource(int offset, int maxRecords)
        {
            if (StopProcessing)
            {
                return null;
            }

            if (LoadCurrentPageOnly)
            {
                bool isSelected = Properties.IsSelected;
                if (isSelected)
                {
                    Path = DocumentContext.CurrentAliasPath;
                    ClassNames = Properties.SelectedItemClass;
                    IsSelected = true;
                }
            }

            // Initialize data and return dataset
            InitDataProperties(Properties);

            int totalRecords = 0;

            // Load the data
            Properties.LoadData(ref mDataSource, false, offset, maxRecords, ref totalRecords);
            TotalRecords = totalRecords;

            return mDataSource;
        }


        /// <summary>
        /// Initialize data properties from property object.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public virtual void InitDataProperties(ICMSDataProperties properties)
        {
            base.InitDataProperties(properties);

            properties.RelatedNodeIsOnTheLeftSide = RelatedNodeIsOnTheLeftSide;
            properties.RelationshipName = RelationshipName;
            properties.RelationshipWithNodeGuid = RelationshipWithNodeGuid;
            properties.SelectedItemTransformationName = SelectedItemTransformationName;
            properties.CategoryName = CategoryName;
            properties.FilterOutDuplicates = FilterOutDuplicates;

            // Check if source filter is set
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(Properties);
            }
        }


        /// <summary>
        /// Sets the web part context.
        /// </summary>
        public virtual void SetContext()
        {
            Properties.SetContext();
        }


        /// <summary>
        /// Releases the web part context.
        /// </summary>
        public virtual void ReleaseContext()
        {
            Properties.ReleaseContext();
        }

        #endregion
    }
}