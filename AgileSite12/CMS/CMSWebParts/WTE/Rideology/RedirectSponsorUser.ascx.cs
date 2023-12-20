using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;
using CMS.DataEngine;
using CMS.SiteProvider;
using System.Net.Mail;
using System.Threading;
using System.Xml;
using Telerik.Web.UI.PdfViewer;

namespace CMSApp.CMSWebParts.WTE.Rideology
{
    public partial class RedirectSponsorUser : CMSAbstractWebPart
    {
        #region "Enums"

        public enum ValidationTypeEnum
        {
            User_To_Ambassador = 0,
            User_To_AmbassadorBrand = 1,
            User_To_Sponsor = 2,
            User_To_EventManager = 3,
            User_To_CarMeet = 4,
            User_To_CarClub = 5,
            User_To_EventProfessional = 6,
            User_To_Charity = 7
        }

        #endregion "Enums"

        #region "Properties"

        /// <summary>
        /// Gets or sets User ID passed from web part in cms.
        /// </summary>
        public string UserID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("UserID"), "");
            }
            set
            {
                this.SetValue("UserID", value);
            }
        }

        public string SponsorGUID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("SponsorGUID"), "");
            }
            set
            {
                this.SetValue("SponsorGUID", value);
            }
        }

        /// <summary>
        /// Gets or sets Sponsor passed from web part in cms.
        /// </summary>
        public string SponsorID
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("SponsorID"), "");
            }
            set
            {
                this.SetValue("SponsorID", value);
            }
        }

        /// <summary>
        /// Gets or sets redirect URL passed from web part in cms.
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectURL"), "");
            }
            set
            {
                this.SetValue("RedirectURL", value);
            }
        }

        /// <summary>
        /// Gets or sets redirect URL passed from web part in cms.
        /// </summary>
        public string ErrorRedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ErrorRedirectURL"), "");
            }
            set
            {
                this.SetValue("ErrorRedirectURL", value);
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

            //check for validity and redirect
            if (CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Edit &&
                CMS.PortalEngine.PortalContext.ViewMode != CMS.PortalEngine.ViewModeEnum.Design)
            {
                RedirectSponsor();
            }
        }

        //protected void CheckUserAndCompanyFromDB()
        //{
        //    try
        //    {
        //        DataSet ds = ConnectionHelper.ExecuteQuery(string.Format("SELECT ISNULL(US.FullName,'') As Fullname, US.UserID AS UserID, ISNULL(US.Username,'') As Username, ISNULL(UserCompany,0) AS CompanyID FROM CMS_User US LEFT OUTER JOIN CMS_UserSettings ST ON US.UserID = ST.UserSettingsUserID WHERE US.Email = '{0}'", EmailAddress), null, QueryTypeEnum.SQLQuery);
        //        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //        {
        //            DataRow row = ds.Tables[0].Rows[0];
        //            CurrentCompanyID = GetSafeInt(row["CompanyID"]);
        //            int.TryParse(row["CompanyID"].ToString(), out CurrentCompanyID);
        //            int.TryParse(row["UserID"].ToString(), out UserID);
        //            CurrentUsername = row["Username"].ToString();
        //            CurrentFullName = row["Fullname"].ToString();
        //        }
        //        else
        //        {
        //            CurrentCompanyID = 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        KenticoLogEvent(ex);
        //        ErrorMessage += ex.Message;
        //    }
        //}

        /// <summary>
        /// Get the redirection URL
        /// </summary>
        /// <returns></returns>
        protected void RedirectSponsor()
        {
            string ret = String.Empty;
            DataSet ds = null;
            GeneralConnection conn = ConnectionHelper.GetConnection();
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SponsorGUID", SponsorGUID);

            QueryParameters qp = new QueryParameters("custom_getSponsorInformationForLogin", parameters, QueryTypeEnum.StoredProcedure, false);
            ds = conn.ExecuteQuery(qp);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                UserID = GetSafeString(row["UserID"]);
                SponsorID = GetSafeString(row["SponsorID"]);
            }

            //if (DataHelper.DataSourceIsEmpty(ds))
            //{
            //    DataTable table = DataHelper.GetDataTable(ds);
            //    if (table != null && table.Rows.Count > 0)
            //    {
            //        DataRow row = table.Rows[0];
            //        if (row != null)
            //        {
            //            if (row["UserID"] != null)
            //            {
            //                UserID = GetSafeString(row["UserID"]);
            //            }

            //            if (row["SponsorID"] != null)
            //            {
            //                SponsorID = GetSafeString(row["SponsorID"]);
            //            }
            //        }
            //    }
            //}

            // login the user if not already log in
            UserInfo userInfo = null;
            userInfo = UserInfoProvider.GetUserInfo(GetSafeInt(UserID));

            bool login = false;
            if (userInfo != null)
            {
                if (MembershipContext.AuthenticatedUser != null && MembershipContext.AuthenticatedUser.UserID == userInfo.UserID)
                {
                    // nothing to do here, we just need to redirect
                    login = true;
                }
                else
                {
                    // we need to log out the current user and log in the user
                    AuthenticationHelper.SignOut(); // sign out the current user...
                                                    // login the user.
                    AuthenticationHelper.AuthenticateUser(userInfo.UserName, false);
                    login = true;
                }
            }
            if (login)
            {
                lbl1.Text = String.Format("{0}:{1}", UserID, SponsorID);
                lbl2.Text = "loggedin";
                lbl3.Text = RedirectURL + SponsorID;
                DoRedirect(RedirectURL + SponsorID, true);
            }
            else
            {
                // need to redirect to the login page.
                // DoRedirect(RedirectURL + SponsorID, true);
                DoRedirect(ErrorRedirectURL);
            }
        }

        /// <summary>
        /// Perform URL redirection
        /// </summary>
        /// <param name="p_url"></param>
        /// <param name="p_useHelper"></param>
        protected void DoRedirect(string p_url, bool p_useHelper = true)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                if (p_useHelper)
                {
                    URLHelper.Redirect(p_url);
                }
                else
                {
                    DoRedirect(p_url);
                }
            }
        }

        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (this.StopProcessing)
            {
                // Do not process
            }
            else
            {
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

        #endregion "Methods"

        #region "Helpers"

        #region "Session helper"

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
        /// Set Session data
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
        /// Clear session data
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

        #endregion "Session helper"

        #region "Properties Helper"

        /// <summary>
        /// Get Bool property values
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public bool GetBoolProperties(string p_key, bool p_default)
        {
            return GetSafeBool(ValidationHelper.GetBoolean(GetValue(p_key), p_default), p_default);
        }

        /// <summary>
        /// Get string properties
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public string GetStringProperty(string p_key, string p_default)
        {
            return GetSafeString(ValidationHelper.GetString(GetValue(p_key), p_default), p_default);
        }

        /// <summary>
        /// Get nullable boolean
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public bool? GetNullableBoolProperties(string p_key, bool? p_default)
        {
            return GetBool(ValidationHelper.GetNullableBoolean(GetValue(p_key), p_default), p_default);
        }

        #endregion "Properties Helper"

        #region "wte data helper"

        #region "get values"

        /// <summary>
        /// Safe cast an object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetString(object p_obj)
        {
            return GetString(p_obj, null);
        }

        /// <summary>
        ///  Get String value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetString(object p_obj, string p_default)
        {
            string ret = null;

            if (p_obj != null)
            {
                if (p_obj is Enum)
                {
                    ret = ((int)(p_obj)).ToString();
                }
                else
                {
                    ret = p_obj.ToString();
                }
            }

            // clean it up
            if (String.IsNullOrWhiteSpace(ret))
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Safe cast an object to nullable bool
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj)
        {
            return GetBool(p_obj, null);
        }

        /// <summary>
        /// Get boolean value with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj, bool? p_default)
        {
            bool? ret = null;
            string val = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(val))
            {
                // the setting could be bool (true/false) or (0/1)
                bool b = false;
                if (bool.TryParse(val, out b))
                {
                    ret = b;
                }
                else
                {
                    // failed, try parsing it as int
                    int num = 0;
                    if (int.TryParse(val, out num))
                    {
                        ret = (num != 0);
                    }
                }
            }
            if (ret == null)
            {
                ret = p_default;
            }
            return ret;
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj)
        {
            return GetInt(p_obj, null);
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj, int? p_default)
        {
            int? ret = null;
            string svalue = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(svalue))
            {
                int num = 0;
                if (int.TryParse(svalue, out num))
                {
                    ret = num;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj)
        {
            return GetDecimal(p_obj, null);
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj, decimal? p_default)
        {
            decimal? ret = null;
            string svalue = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(svalue))
            {
                decimal num = 0;
                if (decimal.TryParse(svalue, out num))
                {
                    ret = num;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Get DateTime? value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj)
        {
            return GetDateTime(p_obj, null);
        }

        /// <summary>
        /// Get date time value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj, DateTime? p_default)
        {
            DateTime? ret = null;
            string val = GetString(p_obj);

            if (!String.IsNullOrWhiteSpace(val))
            {
                // default date is 01/01/1776

                // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
                // however DATETIME2 class does not have this limitation.

                DateTime d = DateTime.Now;
                if (DateTime.TryParse(val, out d))
                {
                    ret = d;
                }
                else
                {
                    ret = null;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            //return null if no date
            return ret;
        }

        #endregion "get values"

        #region "safe values"

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj)
        {
            return GetSafeString(p_obj, String.Empty);
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj, string p_default)
        {
            string output = p_default;
            string value = GetString(p_obj, p_default);
            if (!String.IsNullOrWhiteSpace(value))
            {
                output = value.Trim();
            }
            return output;
        }

        /// <summary>
        /// Get string value or null
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetStringValueOrNull(object p_obj)
        {
            string output = null;
            string value = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(value))
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to boolean
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj)
        {
            return GetSafeBool(p_obj, false);
        }

        /// <summary>
        /// Get safe bool with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj, bool p_default)
        {
            bool ret = p_default;
            bool? value = GetBool(p_obj, p_default);
            if (value.HasValue)
            {
                ret = value.Value;
            }
            return ret;
        }

        /// <summary>
        /// Get decimal or null (if value is false)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBoolValueOrNull(object p_obj)
        {
            bool? ret = null;
            bool value = GetSafeBool(p_obj);
            if (value)
            {
                ret = value;
            }
            return ret;
        }

        /// <summary>
        /// Convert object to int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj)
        {
            return GetSafeInt(p_obj, 0);
        }

        /// <summary>
        /// Get safe int with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj, int p_default)
        {
            int output = p_default;
            int? value = GetInt(p_obj, p_default);
            if (value.HasValue)
            {
                output = value.Value;
            }
            return output;
        }

        /// <summary>
        /// Get int or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns>null if failed to parse or if value is 0</returns>
        public static int? GetIntValueOrNull(object p_obj)
        {
            int? output = null;
            int value = GetSafeInt(p_obj);
            if (value > 0)
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj)
        {
            return GetSafeDecimal(p_obj, 0);
        }

        /// <summary>
        /// Get safe decimal with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj, decimal p_default)
        {
            decimal output = p_default;
            decimal? value = GetDecimal(p_obj, p_default);
            if (value.HasValue)
            {
                output = value.Value;
            }
            return output;
        }

        /// <summary>
        /// Get decimal or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimalValueOrNull(object p_obj)
        {
            decimal? output = null;
            decimal value = GetSafeDecimal(p_obj);
            if (value > 0)
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to date time
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj)
        {
            return GetSafeDateTime(p_obj, DateTime.Now);
        }

        /// <summary>
        /// Get safe date time with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj, DateTime p_default)
        {
            DateTime ret = p_default;
            DateTime? value = GetDateTimeValueOrNull(p_obj);
            if (value.HasValue)
            {
                ret = value.Value;
            }

            // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
            // however DATETIME2 class does not have this limitation.

            // clean up the value so that it is ms sql safe.
            if (ret.Year < 1900)
            {
                ret.AddYears(1900 - ret.Year);
            }

            return ret;
        }

        /// <summary>
        /// Get date time value or null!
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTimeValueOrNull(object p_obj)
        {
            DateTime? ret = GetDateTime(p_obj, null);
            return ret;
        }

        #endregion "safe values"

        #endregion "wte data helper"

        #endregion "Helpers"
    }
}