using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object encapsulating all settings objects to be accessible via macro engine.
    /// </summary>
    /// <remarks>This class is intended for backward compatibility of old setting macro style (Settings.Category.CMSSettingKey)</remarks>
    internal class SettingsCategoryContainer : IDataContainer, IMacroSecurityCheckPermissions
    {
        #region "Variables"

        private bool mKeysAsObjects = true;

        private int mSiteId = 0;

        private SettingsCategoryInfo mSettingsCategoryInfo = null;

        private List<string> mColumnNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the encapsulated settings category info.
        /// </summary>
        public SettingsCategoryInfo SettingsCategoryInfo
        {
            get
            {
                return mSettingsCategoryInfo;
            }
        }


        /// <summary>
        /// Gets the site ID.
        /// </summary>
        public int SiteID
        {
            get
            {
                return mSiteId;
            }
        }


        /// <summary>
        /// Indicates whether settings keys are treated as normal children (InfoObjects) or simple values.
        /// </summary>
        public bool KeysAsObjects
        {
            get
            {
                return mKeysAsObjects;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of SettingsCategoryContainer.
        /// </summary>
        /// <param name="category">Settings category to be encapsulated</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="keysAsObjects">Indicates whether settings keys are treated as normal children (InfoObjects) or simple values</param>
        public SettingsCategoryContainer(SettingsCategoryInfo category, int siteId, bool keysAsObjects)
        {
            mSettingsCategoryInfo = category;
            mSiteId = siteId;
            mKeysAsObjects = keysAsObjects;
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Gets the value of the column, setter is not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object retval = null;
            TryGetValue(columnName, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region "IDataContainer Members"

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                if (mColumnNames == null)
                {
                    var result = new List<string>();

                    if (SettingsCategoryInfo != null)
                    {
                        // Get child categories
                        var dsCategories = SettingsCategoryInfoProvider.GetSettingsCategories()
                            .WhereEquals("CategoryParentID", SettingsCategoryInfo.CategoryID)
                            .Column("CategoryName");

                        if (!DataHelper.DataSourceIsEmpty(dsCategories))
                        {
                            foreach (DataRow dr in dsCategories.Tables[0].Rows)
                            {
                                string name = ValidationHelper.GetString(dr["CategoryName"], "");
                                result.Add(name.Substring(name.LastIndexOfCSafe(".") + 1));
                            }

                            result.Sort();
                        }

                        // Category can contain settings keys as child columns
                        var dsSettings = SettingsKeyInfoProvider.GetSettingsKeys(SettingsCategoryInfo.CategoryID, (KeysAsObjects ? SiteID : 0))
                            .Columns("KeyName")
                            .OrderBy("KeyName");

                        if (!DataHelper.DataSourceIsEmpty(dsSettings))
                        {
                            var settings = new List<string>();

                            foreach (DataRow dr in dsSettings.Tables[0].Rows)
                            {
                                settings.Add(ValidationHelper.GetString(dr["KeyName"], ""));
                            }

                            result.AddRange(settings);
                        }
                    }

                    mColumnNames = result;

                    // Add property which leads to the original InfoObject
                    if (!mColumnNames.Contains("Category"))
                    {
                        mColumnNames.Add("Category");
                    }
                }

                return mColumnNames;
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            switch (columnName.ToLowerCSafe())
            {
                case "category":
                    value = mSettingsCategoryInfo;
                    return true;

                case "cms":
                    // If columnName is CMS, it should return root settings category - CMS.Settings
                    value = new SettingsCategoryContainer(SettingsCategoryInfoProvider.GetRootSettingsCategoryInfo(), SiteID, KeysAsObjects);
                    return true;

                default:

                    // First, try to get the settings key
                    if (KeysAsObjects)
                    {
                        SettingsKeyInfo key = SettingsKeyInfoProvider.GetSettingsKeyInfo(columnName, SiteID);
                        if (key != null)
                        {
                            value = key;
                            return true;
                        }
                    }
                    else
                    {
                        string siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, SiteID);
                        string strValue = SettingsKeyInfoProvider.GetValue(columnName, siteName);
                        if (!String.IsNullOrEmpty(strValue))
                        {
                            value = strValue;
                            return true;
                        }
                    }

                    // Try to get category if the setting key was not found
                    string parent = SettingsCategoryInfo.CategoryName;
                    if (parent == "CMS.Settings")
                    {
                        parent = "CMS";
                    }

                    // Return category if found
                    SettingsCategoryInfo category = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName(parent + "." + columnName);
                    if (category != null)
                    {
                        value = new SettingsCategoryContainer(category, SiteID, KeysAsObjects);
                        return true;
                    }

                    value = null;
                    return false;
            }
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            object value = null;
            TryGetValue(columnName, out value);
            return (value != null);
        }

        #endregion


        #region "Public Macro Methods"

        /// <summary>
        /// Gets an object for which to perform the permissions check.
        /// </summary>
        public object GetObjectToCheck()
        {
            return SettingsCategoryInfo;
        }

        #endregion
    }
}