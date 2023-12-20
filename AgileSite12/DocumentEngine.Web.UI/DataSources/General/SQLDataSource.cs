using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// SQL data source control.
    /// </summary>
    [ToolboxData("<{0}:SQLDataSource runat=server></{0}:SQLDataSource>"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SQLDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private string mDatabaseName;
        private string mServerName;
        private string mUserName;
        private string mPassword;
        private string mLanguage = "English";
        private int mTimeout = 240;
        private SQLServerAuthenticationModeEnum mAuthenticationMode = SQLServerAuthenticationModeEnum.SQLServerAuthentication;
        private string mConnectionString;
        private QueryTypeEnum mQueryType = QueryTypeEnum.SQLQuery;
        private string mQueryText;
        private QueryDataParameters mQueryParameters;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the server authentication mode.
        /// </summary>
        public SQLServerAuthenticationModeEnum AuthenticationMode
        {
            get
            {
                return mAuthenticationMode;
            }
            set
            {
                mAuthenticationMode = value;
            }
        }


        /// <summary>
        /// Gets or sets query text.
        /// </summary>
        public string QueryText
        {
            get
            {
                return mQueryText;
            }
            set
            {
                mQueryText = value;
            }
        }


        /// <summary>
        /// Gets or sets query type. (Standard query or stored procedure.).
        /// </summary>
        public QueryTypeEnum QueryType
        {
            get
            {
                return mQueryType;
            }
            set
            {
                mQueryType = value;
            }
        }


        /// <summary>
        /// Query parameters.
        /// </summary>
        public QueryDataParameters QueryParameters
        {
            get
            {
                return mQueryParameters;
            }
            set
            {
                mQueryParameters = value;
            }
        }


        /// <summary>
        /// Gets or sets complete connection string.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return mConnectionString;
            }
            set
            {
                mConnectionString = value;
            }
        }


        /// <summary>
        /// Gets or sets database name.
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return mDatabaseName;
            }
            set
            {
                mDatabaseName = value;
            }
        }


        /// <summary>
        /// Gets or sets database server name.
        /// </summary>
        public string ServerName
        {
            get
            {
                return mServerName;
            }
            set
            {
                mServerName = value;
            }
        }


        /// <summary>
        /// Gets or sets user name.
        /// </summary>
        public string UserName
        {
            get
            {
                return mUserName;
            }
            set
            {
                mUserName = value;
            }
        }


        /// <summary>
        /// Gets or sets password.
        /// </summary>
        public string Password
        {
            get
            {
                return mPassword;
            }
            set
            {
                mPassword = value;
            }
        }


        /// <summary>
        /// Gets or sets connection language.
        /// </summary>
        public string Language
        {
            get
            {
                return mLanguage;
            }
            set
            {
                mLanguage = value;
            }
        }


        /// <summary>
        /// Gets or sets connection timeout (240 seconds by default).
        /// </summary>
        public int Timeout
        {
            get
            {
                return mTimeout;
            }
            set
            {
                mTimeout = value;
            }
        }

        #endregion


        #region "Methods"
        
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

            DataSet ds = null;

            if (string.IsNullOrEmpty(QueryText))
            {
                throw new Exception("[SQLDataSource.GetDataSourceFromDB]: Missing query text.");
            }

            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            try
            {
                GeneralConnection conn;
                bool useExternalConnection = !string.IsNullOrEmpty(ConnectionString) || !string.IsNullOrEmpty(ServerName);

                // External connection
                if (useExternalConnection)
                {
                    // Complete connection string set
                    if (string.IsNullOrEmpty(ConnectionString))
                    {
                        ConnectionString = ConnectionHelper.BuildConnectionString(AuthenticationMode, ServerName, DatabaseName, UserName, Password, Timeout, Language);
                    }

                    // Get the connection
                    conn = ConnectionHelper.GetConnection(ConnectionString);
                }
                // Application connection
                else
                {
                    conn = ConnectionHelper.GetConnection();
                }

                // Get data
                var query = new QueryParameters(QueryText, QueryParameters, QueryType);

                ds = conn.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SQLDataSource", "GetData", ex, SiteContext.CurrentSiteID);
            }

            return ds;
        }


        /// <summary>
        /// Gets default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "sqldatasource", CacheHelper.BaseCacheKey, ClientID, QueryText, ValidationHelper.GetHashString(ConnectionString, new HashSettings("")), QueryType };
        }

        #endregion
    }
}