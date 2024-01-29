using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Abstract base controls properties.
    /// </summary>
    public abstract class CMSAbstractControlProperties : CMSAbstractBaseProperties, ICMSControlProperties
    {
        #region "Variables"

        /// <summary>
        /// Tree provider variable.
        /// </summary>
        protected TreeProvider mTreeProvider = null;

        /// <summary>
        /// Data source variable.
        /// </summary>
        protected DataSet mDataSource = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TreeProvider TreeProvider
        {
            get
            {
                if (mTreeProvider == null)
                {
                    mTreeProvider = new TreeProvider
                    {
                        CombineWithDefaultCulture = CombineWithDefaultCulture,
                        FilterOutDuplicates = FilterOutDuplicates,
                        MergeResults = true
                    };
                }
                return mTreeProvider;
            }

            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Property to set and get the class names list (separated by the semicolon).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Property to set and get the classnames list (separated by the semicolon).")]
        public virtual string ClassNames
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ClassNames"], "");
            }
            set
            {
                ViewState["ClassNames"] = value;
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
                if (ViewState["Path"] == null)
                {
                    string path = DocumentContext.CurrentAliasPath;
                    if (!String.IsNullOrEmpty(path))
                    {
                        // Check class nae for menu item type
                        if (TreePathUtils.IsMenuItemType(DocumentContext.CurrentPageInfo.ClassName))
                        {
                            ViewState["Path"] = path.TrimEnd('/') + "/%";
                        }
                        else
                        {
                            ViewState["Path"] = path;
                        }
                    }
                }
                return ValidationHelper.GetString(ViewState["Path"], "/%");
            }
            set
            {
                ViewState["Path"] = value;
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
                string defaultCulture = String.Empty;

                if (DatabaseHelper.IsDatabaseAvailable)
                {
                    defaultCulture = LocalizationContext.PreferredCultureCode;
                }
                return ValidationHelper.GetString(ViewState["CultureCode"], defaultCulture);
            }
            set
            {
                ViewState["CultureCode"] = value;
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
                bool defaultValue = true;
                if (DatabaseHelper.IsDatabaseAvailable)
                {
                    defaultValue = SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSCombineWithDefaultCulture");
                }

                return ValidationHelper.GetBoolean(ViewState["CombineWithDefaultCulture"], defaultValue);
            }
            set
            {
                ViewState["CombineWithDefaultCulture"] = value;
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
                return ValidationHelper.GetBoolean(ViewState["SelectOnlyPublished"], true);
            }
            set
            {
                ViewState["SelectOnlyPublished"] = value;
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
                return ValidationHelper.GetInteger(ViewState["MaxRelativeLevel"], -1);
            }
            set
            {
                ViewState["MaxRelativeLevel"] = value;
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
                return ValidationHelper.GetBoolean(ViewState["CheckPermissions"], false);
            }
            set
            {
                ViewState["CheckPermissions"] = value;
            }
        }


        /// <summary>
        /// Indicates if the duplicated (linked) items should be filtered out from the data.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if the duplicate items should be filtered out from the data.")]
        public virtual bool FilterOutDuplicates
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["FilterOutDuplicates"], false);
            }
            set
            {
                ViewState["FilterOutDuplicates"] = value;
                if (mTreeProvider != null)
                {
                    mTreeProvider.FilterOutDuplicates = value;
                }
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
                return mDataSource;
            }
            set
            {
                mDataSource = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// ControlProperties constructor.
        /// </summary>
        protected CMSAbstractControlProperties()
        {
        }


        /// <summary>
        /// Control properties constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        protected CMSAbstractControlProperties(HtmlTextWriterTag tag)
            : base(tag)
        {
        }


        /// <summary>
        /// Reload control data.
        /// </summary>
        public virtual void ReloadData(bool forceLoad)
        {
            ICMSControlProperties ctrl = ParentControl as ICMSControlProperties;

            if (ctrl != null)
            {
                ctrl.ReloadData(forceLoad);
            }
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            // Add document dependencies
            var resolvedPath = MacroResolver.ResolveCurrentPath(Path, true);
            string newDependencies = String.Join("\n", DocumentDependencyCacheKeysBuilder.GetDependencyCacheKeys(SiteName, ClassNames, resolvedPath));
            if (result != null)
            {
                result += "\n";
            }

            result += newDependencies;

            return result;
        }


        /// <summary>
        /// Appends culture parameter to query. Handles "##ALL##" macro
        /// </summary>
        /// <param name="query">Document query</param>
        protected void HandleCulture(MultiDocumentQuery query)
        {
            // Handle all macro
            if (CultureCode.EqualsCSafe(TreeProvider.ALL_CULTURES, true))
            {
                query.AllCultures();
            }
            else
            {
                query.Culture(CultureCode);
            }
        }

        #endregion
    }
}