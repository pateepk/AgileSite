using System;
using System.Data;
using System.Web;

using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.WTE.MOBLZ
{
    public partial class WTESignupRedirect : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// Specific action ID
        /// </summary>
        public int ActionID
        {
            get
            {
                return ValidationHelper.GetInteger(this.GetValue("ActionID"), 0); // set to none
            }
            set
            {
                this.SetValue("ActionID", value);
            }
        }

        /// <summary>
        /// Process Short code and validation in the backgroup
        /// </summary>
        public bool ProcessCode
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("ProcessCode"), false);
            }
            set
            {
                this.SetValue("ProcessCode", value);
            }
        }

        /// <summary>
        /// Track only - only store any query string in the session data.
        /// </summary>
        public bool TrackOnly
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("TrackOnly"), false);
            }
            set
            {
                this.SetValue("TrackOnly", value);
            }
        }

        /// <summary>
        /// Process redirection - if false - no redirection will be processed.
        /// </summary>
        public bool PerformRedirect
        {
            get
            {
                return ValidationHelper.GetBoolean(this.GetValue("PerformRedirect"), true);
            }
            set
            {
                this.SetValue("PerformRedirect", value);
            }
        }

        /// <summary>
        /// Get/Set Short URL code from query string
        /// </summary>
        public string ShortURLCode
        {
            get
            {
                string ret = GetSession("ShortURLCode");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("sc");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("ShortURLCode"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("ShortURLCode", value);
                SetSession("ShortURLCode", value);
            }
        }

        /// <summary>
        /// Get/Set Verification code from query string
        /// </summary>
        public string VerificationCode
        {
            get
            {
                string ret = GetSession("VerificationCode");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("vc");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("VerificationCode"), "");
                    }
                }

                return ret;
                //return ValidationHelper.GetString(this.GetValue("VerificationCode"), "");
            }
            set
            {
                this.SetValue("VerificationCode", value);
                SetSession("VerificationCode", value);
            }
        }

        /// <summary>
        /// Get/Set base redirect url
        /// </summary>
        public string BaseRedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("BaseRedirectURL"), "");
            }
            set
            {
                this.SetValue("BaseRedirectURL", value);
            }
        }

        /// <summary>
        /// Get set vendor id
        /// </summary>
        public string VendorID
        {
            get
            {
                string ret = GetSession("VendorID");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("vid");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("VendorID"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("VendorID", value);
                SetSession("VendorID", value);
            }
        }

        /// <summary>
        /// Get set park id
        /// </summary>
        public string OfficeParkID
        {
            get
            {
                string ret = GetSession("OfficeParkID");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("parkid");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("OfficeParkID"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("OfficeParkID", value);
                SetSession("OfficeParkID", value);
            }
        }

        /// <summary>
        /// Get/Set Building ID
        /// </summary>
        public string BuildingID
        {
            get
            {
                string ret = GetSession("BuildingID");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("pid");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("BuildingID"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("BuildingID", value);
                SetSession("BuildingID", value);
            }
        }

        /// <summary>
        /// Get/Set Tenant ID
        /// </summary>
        public string TenantID
        {
            get
            {
                string ret = GetSession("TenantID");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("tid");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("TenantID"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("TenantID", value);
                SetSession("TenantID", value);
            }
        }

        /// <summary>
        /// The occupant ID
        /// </summary>
        public string OccupantID
        {
            get
            {
                string ret = GetSession("OccupantID");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("oid");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("OccupantID"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("OccupantID", value);
                SetSession("OccupantID", value);
            }
        }

        /// <summary>
        /// Management Company ID
        /// </summary>
        public string ManagementID
        {
            get
            {
                string ret = GetSession("ManagementID");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    ret = GetRequestParam("pmid");
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = ValidationHelper.GetString(this.GetValue("ManagementID"), "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("ManagementID", value);
                SetSession("ManagementID", value);
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

            if (!IsPostBack)
            {
                string tempShortCode = GetRequestParam("sc");
                string tempVerificationCode = GetRequestParam("vc");

                if (!String.IsNullOrWhiteSpace(tempShortCode))
                {
                    ShortURLCode = tempShortCode;
                }

                if (!String.IsNullOrWhiteSpace(tempVerificationCode))
                {
                    VerificationCode = tempVerificationCode;
                }
            }
        }

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (!IsPostBack)
            {
                SetSessionData();
            }

            if (Visible)
            {
                if (!StopProcessing && !TrackOnly)
                {
                    bool needRedirect = false;
                    string redir = BaseRedirectURL;
                    string requestParam = String.Empty;

                    if (!String.IsNullOrWhiteSpace(redir))
                    {
                        requestParam = AddRequestParam(requestParam, "sc", ShortURLCode);
                        requestParam = AddRequestParam(requestParam, "vc", VerificationCode);
                        requestParam = AddRequestParam(requestParam, "vid", VendorID);
                        requestParam = AddRequestParam(requestParam, "tid", TenantID);
                        requestParam = AddRequestParam(requestParam, "parkid", OfficeParkID);
                        requestParam = AddRequestParam(requestParam, "pid", BuildingID);
                        requestParam = AddRequestParam(requestParam, "pmid", ManagementID);
                        requestParam = AddRequestParam(requestParam, "oid", OccupantID);

                        if (!String.IsNullOrWhiteSpace(requestParam))
                        {
                            redir = BaseRedirectURL + "?" + requestParam;
                        }
                    }

                    lblsc.Text = redir;
                    lblvc.Text = VerificationCode + "|" + ShortURLCode;

                    //needRedirect = !String.IsNullOrWhiteSpace(requestParam);
                    needRedirect = !String.IsNullOrWhiteSpace(redir);

                    UserInfo user = MembershipContext.AuthenticatedUser;
                    //user = MembershipContext.CurrentUserProfile;

                    if (user != null)
                    {
                        lblsc.Text += "|" + user.UserID.ToString() + "|" + user.UserName;
                    }

                    if (user != null && !user.IsPublic())
                    {
                        bool success = false;

                        if (ProcessCode)
                        {
                            success = ProcessShortURL();
                            lblvc.Text += "| ProcessShortURL " + success.ToString();
                            //ClearSessionData();
                        }
                    }
                    else
                    {
                        needRedirect = false;
                    }

                    if (CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Edit
                            && CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Design)
                    {
                        lblsc.Text = String.Empty;
                        lblvc.Text = String.Empty;
                    }

                    if (needRedirect && PerformRedirect)
                    {
                        if (!String.IsNullOrWhiteSpace(BaseRedirectURL)
                            && CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Edit
                            && CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Design)
                        {
                            if ((RequestContext.CurrentURL != redir) && (URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL) != redir))
                            {
                                URLHelper.ResponseRedirect(redir);
                            }
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
        protected bool ProcessShortURL()
        {
            bool ret = false;

            UserInfo user = MembershipContext.AuthenticatedUser;
            bool canProcess = user != null && !user.IsPublic();

            if (canProcess)
            {

                DataSet ds = null;
                GeneralConnection conn = ConnectionHelper.GetConnection();
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ShortUrlCode", ShortURLCode);
                parameters.Add("@VerificationCode", VerificationCode);
                parameters.Add("@ActionID", ActionID);

                //user = MembershipContext.CurrentUserProfile;
                if (user != null)
                {
                    parameters.Add("@CurrentMemberID", user.UserID);
                }

                if (user != null)
                {
                    lblvc.Text += String.Format("|Passing|{0}|{1}|{2}|{3}", ShortURLCode, VerificationCode, user.UserID, ActionID);
                }

                QueryParameters qp = new QueryParameters("MOBLZ_System_ProcessShortURL", parameters, QueryTypeEnum.StoredProcedure, false);
                ds = conn.ExecuteQuery(qp);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    DataTable table = DataHelper.GetDataTable(ds);
                    if (table != null && table.Rows.Count > 0)
                    {
                        DataRow row = table.Rows[0];
                        if (row != null)
                        {
                            ret = ValidationHelper.GetBoolean(row["Success"], false);
                        }
                    }
                }

                if (user != null)
                {
                    int userId = user.UserID;
                    string username = user.UserName;
                    user.Generalized.Invalidate(false);
                    AuthenticationHelper.AuthenticateUser(username, false, false);
                }
            }

            return ret;
        }

        #region utilties

        /// <summary>
        /// Set the session data
        /// </summary>
        protected void SetSessionData()
        {
            string tempShortCode = GetRequestParam("sc");
            string tempVerificationCode = GetRequestParam("vc");
            string tempBuilding = GetRequestParam("pid");
            string tempPark = GetRequestParam("parkid");
            string tempTenant = GetRequestParam("tid");
            string tempVendor = GetRequestParam("vid");
            string tempOccupant = GetRequestParam("oid");
            string tempPM = GetRequestParam("pmid");

            if (!String.IsNullOrWhiteSpace(tempShortCode))
            {
                ShortURLCode = tempShortCode;
            }

            if (!String.IsNullOrWhiteSpace(tempVerificationCode))
            {
                VerificationCode = tempVerificationCode;
            }

            if (!String.IsNullOrWhiteSpace(tempBuilding))
            {
                BuildingID = tempBuilding;
            }

            if (!String.IsNullOrWhiteSpace(tempPark))
            {
                OfficeParkID = tempPark;
            }

            if (!String.IsNullOrWhiteSpace(tempTenant))
            {
                TenantID = tempTenant;
            }

            if (!String.IsNullOrWhiteSpace(tempVendor))
            {
                VendorID = tempVendor;
            }

            if (!String.IsNullOrWhiteSpace(tempOccupant))
            {
                OccupantID = tempOccupant;
            }

            if (!String.IsNullOrWhiteSpace(tempPM))
            {
                ManagementID = tempPM;
            }
        }

        /// <summary>
        /// Clear the SessionData
        /// </summary>
        protected void ClearSessionData()
        {
            ShortURLCode = null;
            VerificationCode = null;
            BuildingID = null;
            OfficeParkID = null;
            TenantID = null;
            VendorID = null;
            OccupantID = null;
            ManagementID = null;
        }

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
        /// Get Session data
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
                if (p_value == null)
                {
                    ClearSession(p_key);
                }
                else
                {
                    if (HttpContext.Current != null)
                    {
                        if (HttpContext.Current.Session != null)
                        {
                            HttpContext.Current.Session[p_key] = p_value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void ClearSession(string p_key)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session.Remove(p_key);
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Add Query param to query string
        /// </summary>
        /// <param name="p_queryString"></param>
        /// <param name="p_key"></param>
        /// <param name="p_param"></param>
        /// <returns></returns>
        protected string AddRequestParam(string p_queryString, string p_key, string p_param)
        {
            string ret = p_queryString;
            if (!String.IsNullOrWhiteSpace(p_param))
            {
                if (!String.IsNullOrWhiteSpace(p_key))
                {
                    if (!String.IsNullOrWhiteSpace(p_queryString))
                    {
                        ret += "&";
                    }
                    ret += String.Format("{0}={1}", p_key, p_param);
                }
            }
            return ret;
        }

        #endregion

        #endregion "Methods"
    }
}