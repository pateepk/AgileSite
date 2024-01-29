using System;
using System.Data;
using System.Web;

using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.WTE.MOBLZ
{
    public partial class WTERedirection : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// Get/Set SiteID
        /// </summary>
        public string SiteID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SiteID"), "");
            }
            set
            {
                SetValue("SiteID", value);
            }
        }

        /// <summary>
        /// Get/Set UserID
        /// </summary>
        public string UserID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UserID"), "");
            }
            set
            {
                SetValue("UserID", value);
            }
        }

        /// <summary>
        /// Get/Set RedirectionType
        /// </summary>
        public string RedirectionType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectionType"), "");
            }
            set
            {
                SetValue("RedirectionType", value);
            }
        }

        /// <summary>
        /// Get/Set RoleID
        /// </summary>
        public string RoleID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RoleID"), "");
            }
            set
            {
                SetValue("RoleID", value);
            }
        }

        /// <summary>
        /// Get/Set PageURL
        /// </summary>
        public string PageURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RoleID"), "");
            }
            set
            {
                SetValue("RoleID", value);
            }
        }

        #endregion "Properties"

        #region "Methods"

        /// <summary>
        /// Content loaded event handler. - Main functionality is in here
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (Visible)
            {
                if (!StopProcessing)
                {
                    string redirectURL = GetRedirectionURL();

                    lblredir.Text = redirectURL;

                    if (!String.IsNullOrWhiteSpace(redirectURL)
                        && CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Edit
                        && CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Design)
                    {
                        redirectURL = ResolveMacros(redirectURL);
                        if ((RequestContext.CurrentURL != redirectURL) && (URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL) != redirectURL))
                        {
                            URLHelper.ResponseRedirect(redirectURL);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reloads the control data.
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();
            SetupControl();
        }

        /// <summary>
        /// Get the redirection URL
        /// </summary>
        /// <returns></returns>
        protected string GetRedirectionURL()
        {
            string ret = String.Empty;
            DataSet ds = null;
            GeneralConnection conn = ConnectionHelper.GetConnection();
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", UserID);
            parameters.Add("@RoleID", RoleID);
            parameters.Add("@RedirectionType", RedirectionType);
            parameters.Add("@SiteID", SiteID);
            parameters.Add("@PageURL", PageURL);


            QueryParameters qp = new QueryParameters("MOBLZ_System_GetRedirectionURL", parameters, QueryTypeEnum.StoredProcedure, false);
            ds = conn.ExecuteQuery(qp);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                DataTable table = DataHelper.GetDataTable(ds);
                if (table != null && table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];
                    if (row != null)
                    {
                        ret = (string)row["RedirectURL"];
                    }
                }
            }

            return ret;
        }

        #region "Utilities"

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetRequestParam(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request != null)
                    {
                        ret = HttpContext.Current.Request.Params[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetSession(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        ret = (string)HttpContext.Current.Session[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void SetSession(string p_key, object p_value)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session[p_key] = p_value;
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion

        #endregion "Methods"
    }
}