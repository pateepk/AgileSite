using System;
using System.ComponentModel;
using System.Web.UI;
using System.Data;
using System.Web.UI.Design;

using CMS.DocumentEngine.Web.UI.WS;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Web service data source server control.
    /// </summary>
    [ToolboxData("<{0}:WebServiceDataSource runat=server />"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class WebServiceDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private string mWsUrl;
        private string mWsParameters;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the parameters of the web service.
        /// </summary>
        public string WebServiceParameters
        {
            get
            {
                return mWsParameters;
            }
            set
            {
                mWsParameters = value;
            }
        }


        /// <summary>
        /// Gets or sets the url of the web service.
        /// </summary>
        public string WebServiceUrl
        {
            get
            {
                return mWsUrl;
            }
            set
            {
                mWsUrl = value;
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
            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            DataSet ds = null;

            if (!String.IsNullOrEmpty(WebServiceUrl))
            {
                // Web service object
                WebService webService = new WebService();

                // Web service url 
                webService.Url = URLHelper.GetAbsoluteUrl(mWsUrl);
                try
                {
                    // Get dataset from web service
                    ds = webService.GetDataSet(WebServiceParameters);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("WebServiceDataSource", "GetWebService", ex, SiteContext.CurrentSiteID);
                }
            }

            return ds;
        }

        #endregion
    }
}