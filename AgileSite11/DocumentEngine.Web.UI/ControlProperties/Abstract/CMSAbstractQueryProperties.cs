using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Properties container for the Query based controls.
    /// </summary>
    public abstract class CMSAbstractQueryProperties : CMSAbstractBaseProperties, ICMSQueryProperties
    {
        #region "Variables"

        private string mSelectedValidationType = "int";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether cache minutes must be set manually (Cache minutes value is independent on view mode and cache settings)
        /// </summary>
        public bool ForceCacheMinutes
        {
            get;
            set;
        }


        /// <summary>
        /// Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.
        /// </summary>
        /// <remarks>
        /// This parameter allows you to set up caching of content so that it's not retrieved from the database each time a user requests the page.
        /// </remarks>
        [Category("Behavior"), DefaultValue(-1), Description("Number of minutes the retrieved content is cached for. Zero indicates that the content will not be cached.")]
        public override int CacheMinutes
        {
            get
            {
                if (ForceCacheMinutes)
                {
                    return ValidationHelper.GetInteger(ViewState["CacheMinutes"], 0);
                }

                return base.CacheMinutes;
            }
            set
            {
                if (ForceCacheMinutes)
                {
                    ViewState["CacheMinutes"] = value;
                }
                else
                {
                    base.CacheMinutes = value;
                }
            }
        }


        /// <summary>
        /// Query name in format application.class.query.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Query name in format application.class.query.")]
        public string QueryName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["QueryName"], "");
            }
            set
            {
                ViewState["QueryName"] = value;
            }
        }


        /// <summary>
        /// Query parameters.
        /// </summary>
        /// <remarks>The first dimension contains the name of the parameter in format @param, the second dimension contains its value.</remarks>
        [Description("Query parameters"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public QueryDataParameters QueryParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Select top N rows.")]
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


        /// <summary>
        /// Number of items per page.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Number of items per page.")]
        public int PageSize
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["PageSize"], 0);
            }
            set
            {
                ViewState["PageSize"] = value;
            }
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
            get
            {
                return mSelectedValidationType;
            }
            set
            {
                mSelectedValidationType = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current datasource contains  selected item.
        /// </summary>
        public bool IsSelected
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads data from the database according to the current values of properties.
        /// </summary>
        /// <param name="forceReload">If true, tha data is loaded even when already present</param>
        public DataSet LoadData(bool forceReload)
        {
            int totalRecords = 0;

            return LoadData(forceReload, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Loads data from the database according to the current values of properties.
        /// </summary>
        /// <param name="forceReload">If true, tha data is loaded even when already present</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        public DataSet LoadData(bool forceReload, int offset, int maxRecords, ref int totalRecords)
        {
            if (string.IsNullOrEmpty(QueryName))
            {
                return null;
            }

            // Merge WhereCondition.
            string mergedWhereCondition = WhereCondition;
            if (mergedWhereCondition.IndexOfCSafe("{%") > -1)
            {
                MacroResolver resolver = MacroResolver.GetInstance();
                resolver.SetNamedSourceData(new Dictionary<string, object>
                {
                    { "currentaliaspath", DocumentContext.CurrentAliasPath },
                    { "currentculturecode", LocalizationContext.PreferredCultureCode },
                    { "currentsiteid", SiteContext.CurrentSiteID.ToString() }
                }, isPrioritized: false);

                mergedWhereCondition = resolver.ResolveMacros(mergedWhereCondition);
            }

            // Check if item should be selected
            if (!String.IsNullOrEmpty(SelectedQueryStringKeyName) && !String.IsNullOrEmpty(SelectedDatabaseColumnName))
            {
                string queryValue = QueryHelper.GetString(SelectedQueryStringKeyName, null);
                if (queryValue != null)
                {
                    string column = "[" + SqlHelper.GetSafeQueryString(SelectedDatabaseColumnName, false) + "]";
                    string value = null;

                    // Validate query string input
                    bool validationOk = false;
                    switch (SelectedValidationType.ToLowerCSafe())
                    {
                        case "string":
                            value = SqlHelper.GetSafeQueryString(queryValue, false);
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

            // Try to get data from cache
            string cacheItemName = String.Empty;
            string pagerCacheItemNamePart = (maxRecords > 0) ? "page|" + offset + "|" + maxRecords : null;

            if (String.IsNullOrEmpty(CacheItemName))
            {
                cacheItemName = CacheHelper.BuildCacheItemName(new object[] { "querydatasource", QueryName, SqlHelper.GetParamCacheString(QueryParameters), mergedWhereCondition, OrderBy, SelectTopN, SelectedColumns, pagerCacheItemNamePart });
            }
            else
            {
                // Custom cache item name
                cacheItemName = CacheHelper.BuildCacheItemName(new object[] { CacheItemName, pagerCacheItemNamePart });
            }

            var dataSource = CacheHelper.Cache(cs =>
            {
                // Get the data
                var data = new object[2];
                int recordCount = 0;
                if ((maxRecords > 0) && String.IsNullOrEmpty(OrderBy))
                {
                    throw new Exception("[CMSAbstractQueryProperties.LoadData]: The paged query cannot be executed without the OrderBy property set.");
                }

                data[0] = ConnectionHelper.ExecuteQuery(QueryName, QueryParameters, mergedWhereCondition, OrderBy, TopN, SelectedColumns, offset, maxRecords, ref recordCount);
                data[1] = recordCount;

                return data;
            },
            new CacheSettings(CacheMinutes, cacheItemName)
            {
                BoolCondition = !forceReload,
                CacheDependency = GetCacheDependency()
            }
            );

            totalRecords = (int)dataSource[1];

            return (DataSet)dataSource[0];
        }


        /// <summary>
        /// Clears the cached items.
        /// </summary>
        public void ClearCache()
        {
            // Get the cache item name to clear
            string useCacheItemName = CacheHelper.GetCacheItemName(CacheItemName, "querydatasource", QueryName, SqlHelper.GetParamCacheString(QueryParameters));
            CacheHelper.ClearCache(useCacheItemName);
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            // Add document dependencies
            string newDependencies = QueryInfoProvider.GetQueryCacheDependencies(QueryName);
            if (newDependencies != null)
            {
                if (result != null)
                {
                    result += "\n";
                }

                result += newDependencies;
            }

            return result;
        }

        #endregion
    }
}