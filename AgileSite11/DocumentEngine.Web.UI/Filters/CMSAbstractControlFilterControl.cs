using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Abstract class for filter controls.
    /// </summary>
    public abstract class CMSAbstractControlFilterControl : CMSAbstractBaseFilterControl, ICMSControlProperties
    {
        #region "Variables"

        private TreeProvider mTreeProvider;
        private string mClassNames;
        private string mCategoryName;
        private string mPath;
        private string mCultureCode;
        private bool? mCombineWithDefaultCulture;
        private bool? mSelectOnlyPublished;
        private int? mMaxRelativeLevel;
        private bool? mCheckPermissions;

        #endregion


        #region "Properties"

        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        public virtual TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider;
            }
            set
            {
                mTreeProvider = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Classname value or several values separated by semicolon.
        /// </summary>
        public virtual string ClassNames
        {
            get
            {
                return mClassNames;
            }
            set
            {
                mClassNames = value;
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
        public virtual string Path
        {
            get
            {
                return mPath;
            }
            set
            {
                mPath = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Culture code, such as en-us.
        /// </summary>
        public virtual string CultureCode
        {
            get
            {
                return mCultureCode;
            }
            set
            {
                mCultureCode = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if the documents from the default culture version should be alternatively used.
        /// </summary>
        public virtual bool CombineWithDefaultCulture
        {
            get
            {
                return mCombineWithDefaultCulture.Value;
            }
            set
            {
                mCombineWithDefaultCulture = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if only published documents should be displayed.
        /// </summary>
        public virtual bool SelectOnlyPublished
        {
            get
            {
                return mSelectOnlyPublished.Value;
            }
            set
            {
                mSelectOnlyPublished = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Relative level of child documents that should be selected. -1 selects all child documents.
        /// </summary>
        public virtual int MaxRelativeLevel
        {
            get
            {
                return mMaxRelativeLevel.Value;
            }
            set
            {
                mMaxRelativeLevel = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
        /// </summary>
        public virtual bool CheckPermissions
        {
            get
            {
                return mCheckPermissions.Value;
            }
            set
            {
                mCheckPermissions = value;
                FilterChanged = true;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Initialize data properties from property object.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public override void InitDataProperties(ICMSBaseProperties properties)
        {
            base.InitDataProperties(properties);

            if (properties is ICMSControlProperties)
            {
                ICMSControlProperties controlProperties = (ICMSControlProperties)properties;
                if (Path != null)
                {
                    controlProperties.Path = Path;
                }

                if (ClassNames != null)
                {
                    controlProperties.ClassNames = ClassNames;
                }

                if (mCombineWithDefaultCulture != null)
                {
                    controlProperties.CombineWithDefaultCulture = CombineWithDefaultCulture;
                }

                if (CultureCode != null)
                {
                    controlProperties.CultureCode = CultureCode;
                }

                if (mCheckPermissions != null)
                {
                    controlProperties.CheckPermissions = CheckPermissions;
                }

                if (mMaxRelativeLevel != null)
                {
                    controlProperties.MaxRelativeLevel = MaxRelativeLevel;
                }

                if (mSelectOnlyPublished != null)
                {
                    controlProperties.SelectOnlyPublished = SelectOnlyPublished;
                }
            }
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        /// <param name="forceReload">Indicates force load</param>
        public void ReloadData(bool forceReload)
        {
        }

        #endregion
    }
}