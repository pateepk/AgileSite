using System;
using System.Web.UI;
using System.Data;

using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.DataEngine;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Products data source server control.
    /// </summary>
    [ToolboxData("<{0}:ProductsDataSource runat=server />"), Serializable]
    public class ProductsDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private ICMSDataProperties mProperties = new CMSDataProperties();

        #endregion


        #region "Properties"

        /// <summary>
        /// Data properties
        /// </summary>
        protected CMSAbstractDataProperties Properties
        {
            get
            {
                return (CMSAbstractDataProperties)mProperties;
            }
        }


        /// <summary>
        /// Indicates if the comments should be retrieved according to document filter settings.
        /// </summary>
        public bool UseDocumentFilter
        {
            get;
            set;
        }


        /// <summary>
        /// Path of the documents to be displayed. /% selects all documents.
        /// </summary>
        public string Path
        {
            get
            {
                return Properties.Path;
            }
            set
            {
                Properties.Path = value;
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
                return Properties.CultureCode;
            }
            set
            {
                Properties.CultureCode = value;
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
                return Properties.CombineWithDefaultCulture;
            }
            set
            {
                Properties.CombineWithDefaultCulture = value;
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
                return Properties.SelectOnlyPublished;
            }
            set
            {
                Properties.SelectOnlyPublished = value;
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
                return Properties.MaxRelativeLevel;
            }
            set
            {
                Properties.MaxRelativeLevel = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Gets or sets the where condition for product documents.
        /// </summary>
        public string DocumentsWhereCondition
        {
            get
            {
                return ValidationHelper.GetString(ViewState["DocumentsWhereCondition"], "");
            }
            set
            {
                ViewState["DocumentsWhereCondition"] = value;
                FilterChanged = true;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductsDataSource()
        {
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
        /// Gets datasource from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (StopProcessing)
            {
                return null;
            }

            // Initialize properties with dependence on filter settings
            SourceFilterControl?.InitDataProperties(this);

            // Create WHERE condition
            var where = new WhereCondition();

            // Document filter should be used
            if (UseDocumentFilter)
            {
                var documentsWhere = new WhereCondition().Where(TreeProvider.GetCompleteWhereCondition(SiteName, Path, CultureCode, CombineWithDefaultCulture, DocumentsWhereCondition, SelectOnlyPublished, MaxRelativeLevel));
                var tree = new TreeProvider();
                var subQuery = tree.SelectNodes()
                                   .All()
                                   .Column("NodeSKUID")
                                   .Where(documentsWhere);

                where.WhereIn("SKUID", subQuery);
            }

            if (!string.IsNullOrEmpty(WhereCondition))
            {
                where.Where(WhereCondition);
            }

            var ds = SKUInfoProvider.GetSKUs()
                             .TopN(TopN)
                             .Columns(SelectedColumns)
                             .Where(where)
                             .OrderBy(OrderBy)
                             .TypedResult;

            // Ensure SiteName column if set to a specific site
            if ((SiteName != TreeProvider.ALL_SITES) && !DataHelper.DataSourceIsEmpty(ds))
            {
                DataTable table = ds.Tables[0];
                DataHelper.EnsureColumn(table, "SiteName", typeof(string));
                DataHelper.ChangeStringValues(table, "SiteName", SiteName, null);
            }

            return ds;
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            if (result != null)
            {
                result += "\n";
            }

            result += "ecommerce.sku|all";

            return result;
        }


        /// <summary>
        /// Gets the default cache item name.
        /// </summary>
        /// <returns></returns>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "productsdatasource", CacheHelper.BaseCacheKey, ClientID, SiteName, Path, TopN, CultureCode, CombineWithDefaultCulture, WhereCondition, OrderBy, UseDocumentFilter, SelectOnlyPublished, MaxRelativeLevel, DocumentsWhereCondition, SelectedColumns };
        }

        #endregion
    }
}