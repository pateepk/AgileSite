using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.CustomTables;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Custom table data source server control.
    /// </summary>
    [ToolboxData("<{0}:CustomTableDataSource runat=server />"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CustomTableDataSource : CMSBaseDataSource
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the custom table name.
        /// </summary>
        public string CustomTable
        {
            get;
            set;
        }

        
        /// <summary>
        /// Gets or sets the value that indicates that if a page is selected,
        /// the datasource will provide data for the selected page only.
        /// </summary>
        public bool LoadCurrentPageOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets query string key name. Presence of the key in query string indicates, 
        /// that some item should be selected. The item is determined by query string value.        
        /// </summary>
        public string SelectedQueryStringKeyName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets columns name by which the item is selected.
        /// </summary>
        public string SelectedDatabaseColumnName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets validation type for query string value which determines selected item. 
        /// Options are int, guid and string.
        /// </summary>
        public string SelectedValidationType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the where condition of the data source.
        /// Appends where condition for selected items.
        /// </summary>
        public override string WhereCondition
        {
            get
            {
                string mergedWhereCondition = base.WhereCondition;

                // Check if item should be selected
                if (LoadCurrentPageOnly && !String.IsNullOrEmpty(SelectedQueryStringKeyName) && !String.IsNullOrEmpty(SelectedDatabaseColumnName))
                {
                    string queryValue = QueryHelper.GetString(SelectedQueryStringKeyName, null);
                    if (queryValue != null)
                    {
                        string column = "[" + SqlHelper.EscapeQuotes(SelectedDatabaseColumnName) + "]";
                        string value = null;

                        // Validate query string input
                        bool validationOk = false;
                        switch (SelectedValidationType.ToLowerCSafe())
                        {
                            case "string":
                                value = SqlHelper.EscapeQuotes(queryValue);
                                validationOk = true;
                                break;

                            case "guid":
                                if (ValidationHelper.IsGuid(queryValue))
                                {
                                    value = queryValue;
                                    validationOk = true;
                                }
                                break;

                            default:
                                if (ValidationHelper.IsInteger(queryValue))
                                {
                                    value = queryValue;
                                    validationOk = true;
                                }
                                break;
                        }

                        if (validationOk)
                        {
                            // Select item by where condition
                            mergedWhereCondition = SqlHelper.AddWhereCondition(mergedWhereCondition, column + " = N'" + value + "'");

                            // Set flag about selected item
                            IsSelected = true;
                        }
                    }
                }
                return mergedWhereCondition;
            }
            set
            {
                base.WhereCondition = value;
            }
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomTableDataSource()
        {
            SelectedQueryStringKeyName = "ItemID";
            SelectedDatabaseColumnName = "ItemID";
            SelectedValidationType = "int";
            LoadCurrentPageOnly = true;
        }


        /// <summary>
        /// Gets datasource from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (!CustomTableItemProvider.LicenseVersionCheck(RequestContext.CurrentDomain, ObjectActionEnum.Edit))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.CustomTables);
            }

            // initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }
            
            // Get the data
            return CustomTableItemProvider.GetItems(CustomTable, WhereCondition, OrderBy, TopN, SelectedColumns).TypedResult;
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            string className = CustomTable;
            if (!String.IsNullOrEmpty(className))
            {
                // Add custom table items dependencies
                if (result != null)
                {
                    result += "\n";
                }

                result += className.ToLowerCSafe() + "|all";
            }

            return result;
        }


        /// <summary>
        /// Gets cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "customtabledatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, CustomTable};
        }

        #endregion
    }
}