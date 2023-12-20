using System;
using System.Data;
using System.Web;

using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.DataEngine.Query;

namespace CMSApp.CMSWebParts.WTE.Custom
{
    public partial class WTE_Redirection : CMSAbstractWebPart
    {
        #region "Properties"

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
        /// Get/Set MemberID
        /// </summary>
        public string MemberID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("MemberID"), "");
            }
            set
            {
                SetValue("MemberID", value);
            }
        }

        /// <summary>
        /// Member handle/nickanme
        /// </summary>
        public string NickName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NickName"), "");
            }
            set
            {
                SetValue("NickName", value);
            }
        }

        /// <summary>
        /// Get/Set Return URL - the URL to the page to return to, if redirection failed.
        /// </summary>
        public string ReturnURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ReturnURL"), "");
            }
            set
            {
                SetValue("ReturnURL", value);
            }
        }

        /// <summary>
        /// The default rediect URL
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectURL"), "");
            }
            set
            {
                SetValue("RedirectURL", value);
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
        /// Sproc to execute to get redirection URL
        /// </summary>
        public string RedirectSprocName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectSprocName"), "custom_WTE_Redirect");
            }
            set
            {
                SetValue("RedirectSprocName", value);
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

                    lblredir.Text = "<h2><strong>" + redirectURL + "</strong><h2>";

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
            string queryname = RedirectSprocName;
             
            DataSet ds = null;
            GeneralConnection conn = ConnectionHelper.GetConnection();
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", UserID);
            parameters.Add("@MemberID", MemberID);
            parameters.Add("@NickName", NickName);
            parameters.Add("@RedirectionType", RedirectionType);


            QueryParameters qp = new QueryParameters(queryname, parameters, QueryTypeEnum.StoredProcedure, false);
            ds = conn.ExecuteQuery(qp);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                if (row != null)
                {
                    ret = (string)row["RedirectURL"];
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