using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using CMS.DocumentEngine.Web.UI;
using CMS.Base.Web.UI;


namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUSQLDataSourceInternal : SQLDataSource
    {
        #region "Variables"

        private bool mShowSQL = false;
        private string mGroupBy = null;

        #endregion "Variables"

        #region "Properties"

        /// <summary>
        /// Gets or sets complete connection string.
        /// </summary>
        public bool ShowSQL
        {
            get
            {
                return mShowSQL;
            }
            set
            {
                mShowSQL = value;
            }
        }

        /// <summary>
        /// Property to set and get the WhereCondition.
        /// </summary>
        public string GroupBy
        {
            get
            {
                return mGroupBy;
            }
            set
            {
                mGroupBy = value;
                FilterChanged = true;
            }
        }

        #endregion "Properties"

        #region "Methods"

        /// <summary>
        /// Initializes filter handler if set.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (SourceFilterControl != null)
            {
                SourceFilterControl.OnFilterChanged += SourceFilter_OnFilterChanged;
            }

            base.OnInit(e);
        }

        /// <summary>
        /// OnFilterChanged event handler.
        /// </summary>
        private void SourceFilter_OnFilterChanged()
        {
            // Clear old data
            InvalidateLoadedData();

            // Raise change event
            RaiseOnFilterChanged();
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

            // initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            DataSet ds = null;

            if (string.IsNullOrEmpty(QueryText))
            {
                throw new Exception("[SQLDataSource.GetDataSourceFromDB]: Missing query text.");
            }

            string queryText = QueryText;
            if (!string.IsNullOrWhiteSpace(this.WhereCondition))
            {
                queryText += " where " + this.WhereCondition;
            }

            if (!string.IsNullOrWhiteSpace(this.GroupBy))
            {
                queryText += " Group By " + this.GroupBy;
            }

            if (!string.IsNullOrWhiteSpace(this.OrderBy))
            {
                queryText += " Order by " + this.OrderBy;
            }

            if (ShowSQL)
            {
                lblMsg.Text += queryText;
            }
            try
            {
                GeneralConnection conn = null;
                bool useExternalConnection = !string.IsNullOrEmpty(ConnectionString) || !string.IsNullOrEmpty(ServerName);

                // External connection
                if (useExternalConnection)
                {
                    // Complete connection string set
                    if (string.IsNullOrEmpty(ConnectionString))
                    {
                        ConnectionString = GetConnectionString(AuthenticationMode, ServerName, DatabaseName, UserName, Password, Timeout, false, Language, false);
                    }

                    // Get the connection
                    conn = ConnectionHelper.GetConnection(ConnectionString);
                }
                // Application connection
                else
                {
                    conn = ConnectionHelper.GetConnection();
                }

                //conn.AdvancedSettings.CacheMinutes = 0;
                //conn.AdvancedSettings.CacheDependency = null;
                //conn.CacheMinutes = 0;
                conn.KeepOpen = false;

                // Get data
                QueryParameters qp = new QueryParameters(queryText, QueryParameters, QueryType, false);
                ds = conn.ExecuteQuery(qp);

                conn.Close();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SQLDataSource", "GetData", ex, SiteContext.CurrentSiteID);
            }

            return ds;
        }


        /// <summary>
        /// Generates the connection string with the specified parameters.
        /// </summary>
        /// <param name="authenticationMode">Authentication type</param>
        /// <param name="serverName">Server name</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="userName">User name</param>
        /// <param name="password">User password</param>
        /// <param name="timeout">Connection timeout</param>
        /// <param name="htmlEncoded">Html encoded to be able to display it properly on the page</param>
        /// <param name="language">Connection language</param>
        /// <param name="azure">Specifies whether connection string is for Microsoft Azure</param>
        public static string GetConnectionString(SQLServerAuthenticationModeEnum authenticationMode, string serverName, string databaseName, string userName, string password, int timeout, bool htmlEncoded, string language = "English", bool azure = false)
        {
            password = htmlEncoded ? HttpUtility.HtmlEncode(HttpUtility.HtmlEncode(password)) : password;

            string result;
            string tcp = String.Empty;
            string encrypted = String.Empty;

            if (azure && AzureHelper.IsSQLAzureServer(serverName))
            {
                tcp = "tcp:";
                encrypted = ";Encrypt = True;";
            }

            if (authenticationMode == SQLServerAuthenticationModeEnum.SQLServerAuthentication)
            {
                // Connection string for SQL Server authentication
                result = "Persist Security Info=False;database=" + databaseName + ";server=" + tcp + serverName + ";user id=" + userName + ";password=" + password + ";Current Language=English" + encrypted;
            }
            else
            {
                // Connection string for Windows authentication
                result = "Persist Security Info=False;Integrated Security=SSPI;database=" + tcp + databaseName + ";server=" + serverName + ";Current Language=" + language + encrypted;
            }

            // Add timeout parameter
            if (timeout > 0)
            {
                result = result.TrimEnd(';');
                result += ";Connection Timeout=" + timeout + ";";
            }

            return result;
        }

        #endregion "Methods"
    }
}