using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control data source.
    /// </summary>
    public class CMSControlDataSource : CMSBaseDataSource, ICMSControlProperties
    {
        #region "Variables"

        private ICMSControlProperties mProperties = new CMSControlProperties();
        private string mCategoryName = "";

        #endregion


        #region "Properties"

        /// <summary>
        /// Data properties
        /// </summary>
        protected CMSAbstractControlProperties ControlProperties
        {
            get
            {
                return (CMSAbstractControlProperties)mProperties;
            }
        }


        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return ControlProperties.TreeProvider;
            }
            set
            {
                ControlProperties.TreeProvider = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Class name value or several values separated by semicolon.
        /// </summary>
        public string ClassNames
        {
            get
            {
                return ControlProperties.ClassNames;
            }
            set
            {
                ControlProperties.ClassNames = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Category code name for filtering documents.
        /// </summary>
        public string CategoryName
        {
            get
            {
                return mCategoryName;
            }
            set
            {
                mCategoryName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Path of the documents to be displayed. /% selects all documents.
        /// </summary>
        public string Path
        {
            get
            {
                return ControlProperties.Path;
            }
            set
            {
                ControlProperties.Path = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Culture code, such as en-us.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return ControlProperties.CultureCode;
            }
            set
            {
                ControlProperties.CultureCode = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if the documents from the default culture version should be alternatively used.
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get
            {
                return ControlProperties.CombineWithDefaultCulture;
            }
            set
            {
                ControlProperties.CombineWithDefaultCulture = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if only published documents should be displayed.
        /// </summary>
        public bool SelectOnlyPublished
        {
            get
            {
                return ControlProperties.SelectOnlyPublished;
            }
            set
            {
                ControlProperties.SelectOnlyPublished = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Relative level of child documents that should be selected. -1 selects all child documents.
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return ControlProperties.MaxRelativeLevel;
            }
            set
            {
                ControlProperties.MaxRelativeLevel = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return ControlProperties.CheckPermissions;
            }
            set
            {
                ControlProperties.CheckPermissions = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if the duplicated (linked) items should be filtered out from the data.
        /// </summary>        
        public bool FilterOutDuplicates
        {
            get
            {
                return ControlProperties.FilterOutDuplicates;
            }
            set
            {
                ControlProperties.FilterOutDuplicates = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Top N rows to select.
        /// </summary>    
        public int SelectTopN
        {
            get
            {
                return TopN;
            }
            set
            {
                TopN = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public CMSControlDataSource()
        {
            PropagateProperties(mProperties);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Propagates given settings
        /// </summary>
        /// <param name="properties">Settings</param>
        protected void PropagateProperties(ICMSControlProperties properties)
        {
            base.PropagateProperties(properties);
            mProperties = properties;
        }


        /// <summary>
        /// Initialize data properties from property object.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public virtual void InitDataProperties(ICMSControlProperties properties)
        {
            base.InitDataProperties(properties);

            properties.Path = Path;
            properties.ClassNames = ClassNames;
            properties.CombineWithDefaultCulture = CombineWithDefaultCulture;
            properties.CultureCode = CultureCode;
            properties.CheckPermissions = CheckPermissions;
            properties.MaxRelativeLevel = MaxRelativeLevel;
            properties.SelectOnlyPublished = SelectOnlyPublished;
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        /// <param name="forceReload">Indicates force load</param>
        public void ReloadData(bool forceReload)
        {
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            return ControlProperties.GetDefaultCacheDependencies();
        }

        #endregion
    }
}