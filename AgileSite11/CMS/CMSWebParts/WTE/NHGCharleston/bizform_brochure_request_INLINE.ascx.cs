using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using System;
using System.Data;

//using BlueKey;

namespace NHG_C
{
    public partial class CMSWebParts_BizForms_bizform_brochure_request_INLINE_NHGC : CMSAbstractWebPart
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

        protected string requestType = null;
        protected string requestID = null;

        protected string BuilderId;
        protected string NeighborhoodId;

        protected void Page_Load(object sender, EventArgs e)
        {
            /*
             THIS INFO IS NOT NEEDED ON PAGE LOAD BECAUSE IT IS NOT DISPLAYED ON THE PAGE
             */
            requestName.Text = getRequestName();
            //lblPhoneNumber.Text = "3213213211"; //getPhoneNumber();

            // throw new Exception("line 110"); //test if this is being hit
            // HIT

            /*
             THIS DATA IS NOT AVAILABLE UNTIL AFTER POST BACK
             */
            int ndClassID = ValidationHelper.GetInteger(viewBiz.Data["NodeClassID"], 0);
            int docNdID = ValidationHelper.GetInteger(viewBiz.Data["DocNodeID"], 0);
            int docFKVal = ValidationHelper.GetInteger(viewBiz.Data["DocFKValue"], 0);
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

        public string getRequestName()
        {
            /*TESTING - SKIPS FUNCTION*/
            // return "Test Request Name";

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;

            //NodeClassID = 2396 = Neighborhood
            //NodeClassID = 2397 = Builder
            //NodeClassID = 2400 = Lot

            int ndClassID = ValidationHelper.GetInteger(viewBiz.Data["NodeClassID"], 0);
            int docNdID = ValidationHelper.GetInteger(viewBiz.Data["DocNodeID"], 0);
            int docFKVal = ValidationHelper.GetInteger(viewBiz.Data["DocFKValue"], 0);
            string hoodNodeID = "0";
            string returnValue = "aa";

            if (ndClassID == 2396 && docFKVal != 0) //Neighborhood
            {
                NeighborhoodId = docFKVal.ToString();
                requestType = "neighborhood";
                requestID = Convert.ToString(docFKVal);

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + docFKVal.ToString() + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    BuilderId = dr["NeighborhoodDevelopers"].ToString();
                    returnValue = dr["NeighborhoodName"].ToString();
                }
            }
            else if (ndClassID == 2397 && docFKVal != 0) //Builder
            {
                BuilderId = docFKVal.ToString();
                requestType = "builder";
                requestID = Convert.ToString(docFKVal);

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.developers", "DevelopersID = '" + docFKVal.ToString() + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    returnValue = dr["DeveloperName"].ToString();
                }
            }
            else if (ndClassID == 2400 && docFKVal != 0) //listing
            {
                requestType = "listing";
                requestID = Convert.ToString(docFKVal);

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Listing", "ListingID = '" + docFKVal.ToString() + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    BuilderId = dr["ListingDeveloper"].ToString();
                    hoodNodeID = dr["NodeParentID"].ToString();
                    returnValue = dr["ListingTitle"].ToString();
                }

                /////
                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", "NodeID = '" + hoodNodeID + "'");
                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    returnValue += " in " + dr["NeighborhoodName"].ToString();
                }

                /////
                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.developers", "DevelopersID = '" + BuilderId.ToString() + "'");
                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    returnValue += " by " + dr["DeveloperName"].ToString();
                }
            }
            else
            {
                returnValue = CurrentDocument.DocumentName;
            }

            return returnValue;
        }

        protected string[] getRecipient()
        {
            ///*TESTING - SKIPS FUNCTION*/
            //string[] emails2 = new string[2];
            //emails2[0] = "jseeley@wte.net";
            //emails2[1] = "jseeley@wte.net";
            //return emails2;

            string url = RequestContext.RawURL;

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;

            string[] emails = new string[2];

            if (requestType == "neighborhood")
            {
                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + requestID + "'", null);

                DataRow neighborhoodRow = ds.Tables[0].Rows[0];

                if (neighborhoodRow != null)
                {
                    emails[0] = neighborhoodRow["NeighborhoodEmails"].ToString();

                    DataSet brds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = '" + neighborhoodRow["NeighborhoodDevelopers"].ToString() + "'", null);

                    DataRow builderRow = brds.Tables[0].Rows[0];

                    if (builderRow != null)
                    {
                        emails[1] = builderRow["DeveloperEmails"].ToString();
                    }
                }
            }

            if (requestType == "builder")
            {
                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = '" + requestID + "'", null);

                DataRow builderRow = ds.Tables[0].Rows[0];

                if (builderRow != null)
                {
                    emails[0] = builderRow["DeveloperEmails"].ToString();
                    emails[1] = null;
                }
            }

            if (requestType == "listing")
            {
                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Listing", "ListingID = '" + requestID + "'", null);

                DataRow listingRow = ds.Tables[0].Rows[0];

                if (listingRow != null)
                {
                    DataSet brds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = '" + listingRow["ListingDeveloper"].ToString() + "'", null);
                    DataRow builderRow = brds.Tables[0].Rows[0];

                    if (builderRow != null)
                    {
                        emails[0] = builderRow["DeveloperEmails"].ToString();
                    }

                    DataSet nrds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodDevelopers = '" + listingRow["ListingDeveloper"].ToString() + "'", null);
                    DataRow hoodRow = nrds.Tables[0].Rows[0];

                    if (hoodRow != null)
                    {
                        emails[1] = hoodRow["NeighborhoodEmails"].ToString();
                    }
                }
            }

            return emails;
        }

        protected void viewBiz_OnBeforeSave(object sender, EventArgs e)
        {
            viewBiz.Data.SetValue("Audience", requestName.Text);
        }

        protected void viewBiz_OnAfterSave(object sender, EventArgs e)
        {
            string first_name = ValidationHelper.GetString(viewBiz.Data["FirstName"], "N/A");
            string last_name = ValidationHelper.GetString(viewBiz.Data["LastName"], "N/A");
            bool flagSpam = (first_name == last_name);
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
            string is_realtor = ValidationHelper.GetString(viewBiz.Data["Realtor"], "N/A");
            string email_subject = contact_name;

            if (is_realtor.ToLower() == "yes")
            {
                email_subject += " | request from realtor";
            }

            string url = RequestContext.RawURL;
            string lotID = URLHelper.GetUrlParameter(url, "lot");
            string additional_info = "N/A";
            if (lotID != null)
            {
                additional_info = "Lot # " + lotID.ToString();
            }

            EmailMessage msg = new EmailMessage();
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate("BrochureRequest", SiteContext.CurrentSiteID);

            //create table for macros (token replacement in email template)
            DataTable dt = new DataTable();
            dt.Columns.Add("first_name");
            dt.Columns.Add("last_name");
            dt.Columns.Add("addres#");
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
            dt.Columns.Add("email_subject");
            dt.Columns.Add("realtor");

            //add data row with values
            DataRow drRow = dt.NewRow();
            drRow["first_name"] = first_name;
            drRow["last_name"] = last_name;
            drRow["addres#"] = address;
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
            drRow["email_subject"] = email_subject;
            drRow["realtor"] = is_realtor;

            //create macro resolver object to pass to template
            var resolver = MacroResolver.GetInstance();
            resolver.SetAnonymousSourceData(drRow);

            msg.From = "noreply@newhomesguidecharleston.com";
            //msg.Subject = "Brochure Request";

            //string[] emails = new string[] {"jseeley@wte.net"};
            string[] emails = getRecipient();

            if (flagSpam)
            {
                msg.Subject = "Brochure Request -- Possible SPAM message";
                msg.Recipients = "louise@newhomesguidecharleston.com";

                EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, false);
            }
            else
            {
                foreach (string recipient in emails)
                {
                    if (!String.IsNullOrEmpty(recipient))
                    {
                        msg.Recipients = recipient;
                        EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, false);

                        //WTE Custom log
                        LogClickTrack();
                    }
                }
            }
        }
    }
}