using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.PortalEngine.Web.UI;

using CMS.DataEngine;
using CMS.WebAnalytics;
using CMS.DeviceProfiles;

//using BlueKey;

namespace NHG_T
{
    public partial class CMSWebParts_BizForms_bizform_brochure_request_NHGT : CMSAbstractWebPart
    {
        #region app_code classes

        public class SQL
        {
            public static DataSet ExecuteQuery(string sql, QueryDataParameters p)
            {
                QueryParameters qp = new QueryParameters(sql, p, QueryTypeEnum.SQLQuery);

                return ConnectionHelper.ExecuteQuery(qp);
            }
        }

        #endregion app_code classes

        #region "Members"

        protected string requestType = null;
        protected string requestID = null;

        protected string BuilderId;
        protected string NeighborhoodId;

        #endregion "Members"

        #region "Public properties"

        /// <summary>
        /// Gets or sets the form name of BizForm
        /// </summary>
        public string BizFormName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("BizFormName"), "");
            }
            set
            {
                SetValue("BizFormName", value);
            }
        }

        /// <summary>
        /// Gets or sets the alternative form full name (ClassName.AlternativeFormName)
        /// </summary>
        public string AlternativeFormName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AlternativeFormName"), "");
            }
            set
            {
                SetValue("AlternativeFormName", value);
            }
        }

        /// <summary>
        /// Gets or sets the site name
        /// </summary>
        public string SiteName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SiteName"), "");
            }
            set
            {
                SetValue("SiteName", value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether the WebPart use colon behind label
        /// </summary>
        public bool UseColonBehindLabel
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseColonBehindLabel"), true);
            }
            set
            {
                SetValue("UseColonBehindLabel", value);
            }
        }

        /// <summary>
        /// Gets or sets the message which is displayed after validation failed
        /// </summary>
        public string ValidationErrorMessage
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ValidationErrorMessage"), "");
            }
            set
            {
                SetValue("ValidationErrorMessage", value);
            }
        }

        /// <summary>
        /// Override the logic to send email the builders when neighboorhood brochure is requested.
        /// </summary>
        public bool SendBuilderEmailForNeighborhood
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("SendBuilderEmailForNeighborhood"), false);
            }
            set
            {
                SetValue("SendBuilderEmailForNeighborhood", value);
            }
        }

        #endregion "Public properties"

        protected void Page_Load(object sender, EventArgs e)
        {
            requestName.Text = getRequestName();
            lblPhoneNumber.Text = getPhoneNumber();
        }

        /// <summary>
        /// Content loaded event handler
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }

        /// <summary>
        /// Reloads data for partial caching
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();
            SetupControl();
        }

        /// <summary>
        /// Initializes the control properties
        /// </summary>
        protected void SetupControl()
        {
            if (StopProcessing)
            {
                // Do nothing
                viewBiz.StopProcessing = true;
            }
            else
            {
                // Set BizForm properties
                viewBiz.FormName = BizFormName;
                viewBiz.SiteName = SiteName;
                viewBiz.UseColonBehindLabel = UseColonBehindLabel;
                viewBiz.AlternativeFormFullName = AlternativeFormName;
                viewBiz.ValidationErrorMessage = ValidationErrorMessage;

                //     Control ctrl = viewBiz.FindControl("Audience");
                //((Label)ctrl).Text = getRequestName();

                // Set the live site context
                if (viewBiz != null)
                {
                    viewBiz.ControlContext.ContextName = CMS.Base.Web.UI.ControlContext.LIVE_SITE;
                }
            }
        }

        public string getRequestName()
        {
            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            TreeProvider tree = new TreeProvider(ui);

            DataSet ds = null;
            string url = RequestContext.RawURL;
            string neighborhoodID = URLHelper.GetUrlParameter(url, "neighborhood");
            string builderID = URLHelper.GetUrlParameter(url, "builder");
            string lotID = URLHelper.GetUrlParameter(url, "lotID");

            NeighborhoodId = neighborhoodID;
            BuilderId = builderID;

            if (neighborhoodID != null)
            {
                requestType = "neighborhood";
                requestID = neighborhoodID;

                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + neighborhoodID + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    if (lotID != null)
                    {
                        return dr["NeighborhoodName"].ToString() + " - Lot # " + lotID;
                    }
                    else
                    {
                        return dr["NeighborhoodName"].ToString();
                    }
                }
            }
            else if (builderID != null)
            {
                requestType = "builder";
                requestID = builderID;

                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.developers", "DevelopersID = '" + builderID + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    return dr["DeveloperName"].ToString();
                }
            }

            return "";
        }

        public string getPhoneNumber()
        {
            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            string url = RequestContext.RawURL;
            string neighborhoodID = URLHelper.GetUrlParameter(url, "neighborhood");
            string builderID = URLHelper.GetUrlParameter(url, "builder");
            string lotID = URLHelper.GetUrlParameter(url, "lotID");

            if (neighborhoodID != null)
            {
                requestType = "neighborhood";
                requestID = neighborhoodID;

                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + neighborhoodID + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    if (lotID != null)
                    {
                        return dr["NeighborhoodName"].ToString() + " - Lot # " + lotID;
                    }
                    else
                    {
                        return dr["NeighborhoodName"].ToString();
                    }
                }
            }
            else if (builderID != null)
            {
                requestType = "builder";
                requestID = builderID;

                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.developers", "DevelopersID = '" + builderID + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    return dr["DeveloperName"].ToString();
                }
            }
            else
            {
                return CurrentDocument.DocumentName;
            }

            return "";
        }

        protected string[] getRecipient()
        {
            string url = RequestContext.RawURL;

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            TreeProvider tree = new TreeProvider(ui);

            DataSet ds = null;

            List<string> emails_list = new List<string>();

            if (requestType == "neighborhood")
            {
                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + requestID + "'", null);

                DataRow neighborhoodRow = ds.Tables[0].Rows[0];

                if (neighborhoodRow != null)
                {
                    string neighborhoodEmailsField = neighborhoodRow["NeighborhoodEmails"].ToString();

                    string[] neighborhoodemails = neighborhoodEmailsField.Split(';');

                    foreach (string email in neighborhoodemails)
                    {
                        emails_list.Add(email);
                    }

                    DataSet brds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = '" + neighborhoodRow["NeighborhoodDevelopers"].ToString() + "'", null);

                    DataRow builderRow = brds.Tables[0].Rows[0];

                    if (builderRow != null)
                    {
                        string developerEmailsField = builderRow["DeveloperEmails"].ToString();

                        string[] developerEmails = developerEmailsField.Split(';');

                        foreach (string email in developerEmails)
                        {
                            emails_list.Add(email);
                        }
                    }
                }
            }

            if (requestType == "builder")
            {
                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = '" + requestID + "'", null);

                DataRow builderRow = ds.Tables[0].Rows[0];

                if (builderRow != null)
                {
                    string developerEmailsField = builderRow["DeveloperEmails"].ToString();

                    string[] developerEmails = developerEmailsField.Split(';');

                    foreach (string email in developerEmails)
                    {
                        emails_list.Add(email);
                    }
                }
            }

            return emails_list.ToArray();
        }

        /**
        protected void viewBiz_OnBeforeSave()
        {
            viewBiz.DataRow["Audience"] = getRequestName();
            base.viewBiz_OnBeforeSave();
        }
        **/

        protected void viewBiz_OnBeforeSave(object sender, EventArgs e)
        {
            viewBiz.Data.SetValue("Audience", requestName.Text);
        }

        protected void viewBiz_OnAfterSave(object sender, EventArgs e)
        {
            string first_name = ValidationHelper.GetString(viewBiz.Data["FirstName"], "N/A");
            string last_name = ValidationHelper.GetString(viewBiz.Data["LastName"], "N/A");
            string address = ValidationHelper.GetString(viewBiz.Data["Address"], "N/A");
            string city = ValidationHelper.GetString(viewBiz.Data["City"], "N/A");
            string state = ValidationHelper.GetString(viewBiz.Data["State"], "N/A");
            string zip_code = ValidationHelper.GetString(viewBiz.Data["Zip"], "N/A");
            string country = ValidationHelper.GetString(viewBiz.Data["Country"], "N/A");
            string daytime_phone = ValidationHelper.GetString(viewBiz.Data["DaytimePhone"], "N/A");
            string evening_phone = ValidationHelper.GetString(viewBiz.Data["EveningPhone"], "N/A");
            string email_address = ValidationHelper.GetString(viewBiz.Data["EmailAddress"], "N/A");
            string comments = ValidationHelper.GetString(viewBiz.Data["Comments"], "N/A");
            string whensubmitted = ValidationHelper.GetString(DateTime.Now, "N/A");

            string contact_name = ValidationHelper.GetString(getRequestName(), "N/A");
            string url = RequestContext.RawURL;
            string lotID = URLHelper.GetUrlParameter(url, "lotID");
            string additional_info = "N/A";
            if (lotID != null)
            {
                additional_info = "Lot # " + lotID.ToString();
            }

            //viewBiz.SetValue("Audience", getRequestName());

            EmailMessage msg = new EmailMessage();
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate("BrochureRequest", SiteContext.CurrentSiteID);

            //create table for macros (token replacement in email template)
            DataTable dt = new DataTable();
            dt.Columns.Add("first_name");
            dt.Columns.Add("last_name");
            dt.Columns.Add("address");
            dt.Columns.Add("city");
            dt.Columns.Add("state");
            dt.Columns.Add("zip_code");
            dt.Columns.Add("country");
            dt.Columns.Add("daytime_phone");
            dt.Columns.Add("evening_phone");
            dt.Columns.Add("email_address");
            dt.Columns.Add("contact_name");
            dt.Columns.Add("whensubmitted");
            dt.Columns.Add("form_comments");
            dt.Columns.Add("debug");
            dt.Columns.Add("additionalInfo");

            //add data row with values
            DataRow drRow = dt.NewRow();
            drRow["first_name"] = first_name;
            drRow["last_name"] = last_name;
            drRow["address"] = address;
            drRow["city"] = city;
            drRow["state"] = state;
            drRow["zip_code"] = zip_code;
            drRow["country"] = country;
            drRow["daytime_phone"] = daytime_phone;
            drRow["evening_phone"] = evening_phone;
            drRow["email_address"] = email_address;
            drRow["contact_name"] = contact_name;
            drRow["whensubmitted"] = whensubmitted;
            drRow["form_comments"] = comments;
            drRow["debug"] = "";
            drRow["additionalInfo"] = additional_info;

            //create macro resolver object to pass to template
            var resolver = MacroResolver.GetInstance();
            resolver.SetAnonymousSourceData(drRow);

            msg.From = (SiteContext.CurrentSiteName.ToLower().StartsWith("grand") ? "noreply@grandstrandnewhomeguide.com" : "noreply@triadnewhomeguide.com");
            msg.Subject = "Brochure Request";

            string[] emails = getRecipient();

            foreach (string recipient in emails)
            {
                if (recipient != null)
                {
                    msg.Recipients = recipient;
                    msg.CcRecipients = "info@newhomeguides.com";
                    msg.BccRecipients = "day@newhomeguides.com";
                    EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, false);

                    //WTE Custom log
                    LogClickTrack();
                }
            }
        }

        protected string getDomain(string url)
        {
            string retDomain = url;

            try
            {
                retDomain = retDomain.Replace("https://", "");
                retDomain = retDomain.Replace("http://", "");

                Char delimiter = '/';
                retDomain = retDomain.Split(delimiter)[0];
            }
            catch
            {
                //do nothing
            }

            return retDomain;
        }

        protected void LogClickTrack()
        {
            string uSql = string.Empty;
            QueryDataParameters uParameters = new QueryDataParameters();
            string linkType = QueryHelper.GetString("type", "Brochure Request - " + requestType);
            string linkItemID = QueryHelper.GetString("id", requestID);
            string linkNote = QueryHelper.GetString("LinkNote", "Notes: ") + " | type = " + linkType + " | id = " + linkItemID;
            string docID = QueryHelper.GetString("DocumentID", CurrentDocument.DocumentID.ToString());
            string linkTo = QueryHelper.GetString("url", "Brochure Request - " + requestType);
            string SiteID = "0";
            string linkToDomain = getDomain(linkTo);
            string viewURL = "";

            if (System.Web.HttpContext.Current.Request.UrlReferrer != null)
            {
                viewURL = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
            }

            //if (SiteContext.CurrentSiteID != null)
            {
                SiteID = SiteContext.CurrentSiteID.ToString();
            }

            uParameters.Add("@ClickTrackDate", DateTime.Now.ToString());
            uParameters.Add("@UserIPAddress", System.Web.HttpContext.Current.Request.UserHostAddress);
            uParameters.Add("@UserID", CurrentUser.UserID);
            uParameters.Add("@IsAuthenticated", AuthenticationHelper.IsAuthenticated().ToString());
            uParameters.Add("@IsReturningVisitor", AnalyticsContext.IsReturningVisitor.ToString());
            uParameters.Add("@IsNewVisitor", AnalyticsContext.IsNewVisitor.ToString());
            uParameters.Add("@Browser", BrowserHelper.GetBrowser());
            uParameters.Add("@UserAgent", BrowserHelper.GetUserAgent());
            uParameters.Add("@IsCrawler", BrowserHelper.IsCrawler().ToString());

            uParameters.Add("@IsMobile", DeviceContext.CurrentDevice.IsMobile().ToString());
            uParameters.Add("@IsTablet", "false");

            uParameters.Add("@DocumentID", docID);
            uParameters.Add("@LinkTo", linkTo);
            uParameters.Add("@LinkNote", linkNote);
            uParameters.Add("@PageViewURL", viewURL);
            uParameters.Add("@LinkType", linkType);
            uParameters.Add("@LinkItemID", linkItemID);
            uParameters.Add("@LinkToDomain", linkToDomain);
            uParameters.Add("@BuilderID", BuilderId);
            uParameters.Add("@NeighborhoodID", NeighborhoodId);
            uParameters.Add("@SiteID", SiteID);

            uSql = "INSERT INTO WTE_ClickTrack (ClickTrackDate,UserIPAddress,UserID,IsAuthenticated,IsReturningVisitor,IsNewVisitor,Browser,UserAgent,IsCrawler,IsMobile,IsTablet,DocumentID,LinkTo,LinkNote,PageViewURL,LinkType,LinkItemID,LinkToDomain,BuilderID,NeighborhoodID,SiteID) ";
            uSql += " VALUES (@ClickTrackDate,@UserIPAddress,@UserID,@IsAuthenticated,@IsReturningVisitor,@IsNewVisitor,@Browser,@UserAgent,@IsCrawler,@IsMobile,@IsTablet,@DocumentID,@LinkTo,@LinkNote,@PageViewURL,@LinkType,@LinkItemID,@LinkToDomain,@BuilderID,@NeighborhoodID,@SiteID)";

            SQL.ExecuteQuery(uSql, uParameters);
        }
    }
}