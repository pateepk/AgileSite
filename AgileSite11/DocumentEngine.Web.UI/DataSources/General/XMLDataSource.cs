using System;
using System.ComponentModel;
using System.Net;
using System.Web.UI;
using System.Data;
using System.Xml;
using System.Web.UI.Design;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

using SystemIO = System.IO;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// XML data source server control.
    /// </summary>
    [ToolboxData("<{0}:XMLDataSource runat=server />"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class XMLDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private string mXmlUrl;
        private string mXmlSchemaUrl;
        private string mTableName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the XML URL.
        /// </summary>
        public string XmlUrl
        {
            get
            {
                return mXmlUrl;
            }
            set
            {
                mXmlUrl = value;
            }
        }


        /// <summary>
        /// Gets or sets the XML schema URL.
        /// </summary>
        public string XmlSchemaUrl
        {
            get
            {
                return mXmlSchemaUrl;
            }
            set
            {
                mXmlSchemaUrl = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of dataset table, which will be used as data source.
        /// </summary>
        public string TableName
        {
            get
            {
                return mTableName;
            }
            set
            {
                mTableName = value;
            }
        }

        #endregion


        #region "Methods, events, handlers"

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
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            DataSet ds = null;

            if (!String.IsNullOrEmpty(XmlUrl))
            {
                try
                {
                    // Prepare the data set
                    var dataset = CreateDataSet();

                    // Get XML reader settings
                    var xmlReaderSettings = new XmlReaderSettings()
                    {
                        XmlResolver = new XmlUrlResolver()
                        {
                            Credentials = CredentialCache.DefaultCredentials,
                        },
                    };

                    // Read XML content
                    var feedRequest = (HttpWebRequest)WebRequest.Create(URLHelper.GetAbsoluteUrl(XmlUrl));
                    if ((HttpContext.Current != null) && (HttpContext.Current.Request != null))
                    {
                        // Set the UserAgent (this is needed for example for facebook feeds which return error if the useragent is not specified)
                        feedRequest.UserAgent = HttpContext.Current.Request.UserAgent;
                    }

                    using (var responseStream = feedRequest.GetResponse().GetResponseStream())
                    using (var responseMemoryStream = new SystemIO.MemoryStream())
                    {
                        // Copy to MemoryStream to be able to seek
                        responseStream.CopyTo(responseMemoryStream);
                        responseMemoryStream.Seek(0, SystemIO.SeekOrigin.Begin);

                        try
                        {
                            // Try to load the original XML
                            dataset.ReadXml(responseMemoryStream);
                        }
                        catch (Exception)
                        {
                            // Load the XML with replaced namespace colons
                            responseMemoryStream.Seek(0, SystemIO.SeekOrigin.Begin);
                            using (var originalReader = XmlReader.Create(responseMemoryStream, xmlReaderSettings))
                            using (var transformedStream = new SystemIO.MemoryStream())
                            {
                                // Replace namespace colons
                                using (var transformedWriter = XmlWriter.Create(transformedStream))
                                {
                                    XmlHelper.ReplaceNamespaceColon(originalReader, transformedWriter);
                                    transformedStream.Seek(0, SystemIO.SeekOrigin.Begin);
                                }

                                using (var transformedReader = XmlReader.Create(transformedStream, xmlReaderSettings))
                                {
                                    // Reset the data set
                                    dataset = CreateDataSet();

                                    // Load the transformed XML
                                    dataset.ReadXml(transformedReader);
                                }
                            }
                        }
                    }

                    // Check whether dataset contains data)
                    if (!DataHelper.DataSourceIsEmpty(dataset))
                    {
                        DataView dv = null;

                        // Use specific table from dataset
                        if (!String.IsNullOrEmpty(TableName))
                        {
                            if (dataset.Tables[TableName] != null)
                            {
                                dv = dataset.Tables[TableName].DefaultView;
                            }
                        }
                        // By default use first table
                        else
                        {
                            dv = dataset.Tables[0].DefaultView;
                        }

                        if (dv != null)
                        {
                            // Apply Where Condition on dataview
                            if (!String.IsNullOrEmpty(WhereCondition))
                            {
                                dv.RowFilter = WhereCondition;
                            }

                            // Apply OrderBy on datatable
                            if (!String.IsNullOrEmpty(OrderBy))
                            {
                                DataHelper.SortDataTable(dv.Table, OrderBy);
                            }

                            // Apply TopN on dataset
                            DataSet restrictedDS = new DataSet();
                            restrictedDS.Tables.Add(dv.ToTable());
                            DataHelper.RestrictRows(restrictedDS, TopN);

                            ds = restrictedDS;
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("XMLDataSource", "GetData", ex, SiteContext.CurrentSiteID);
                }
            }

            return ds;
        }


        /// <summary>
        /// Creates the data set for the data source data.
        /// </summary>
        private DataSet CreateDataSet()
        {
            var dataset = new DataSet();
            if (!string.IsNullOrEmpty(XmlSchemaUrl))
            {
                dataset.ReadXmlSchema(URLHelper.GetAbsoluteUrl(XmlSchemaUrl));
            }
            return dataset;
        }


        /// <summary>
        /// Gets default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "xmldatasource", CacheHelper.BaseCacheKey, ClientID, XmlUrl, XmlSchemaUrl, TableName, WhereCondition, TopN, OrderBy };
        }

        #endregion
    }
}