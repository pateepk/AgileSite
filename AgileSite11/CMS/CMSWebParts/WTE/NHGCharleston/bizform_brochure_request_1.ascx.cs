using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using System;
using System.Data;

namespace NHG_C
{
    public partial class CMSWebParts_BizForms_bizform_brochure_request_1_NHGC : CMSAbstractWebPart
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
            }
        }

        public string getRequestName()
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

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + neighborhoodID + "'");

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

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.developers", "DevelopersID = '" + builderID + "'");

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

            return "aa";
        }

        public string getPhoneNumber()
        {
            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            string url = RequestContext.RawURL;
            string neighborhoodID = URLHelper.GetUrlParameter(url, "neighborhood");
            string builderID = URLHelper.GetUrlParameter(url, "builder");
            string lotID = URLHelper.GetUrlParameter(url, "lot");

            if (neighborhoodID != null)
            {
                requestType = "neighborhood";
                requestID = neighborhoodID;

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = '" + neighborhoodID + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    string phone = ValidationHelper.GetString(dr["NeighborhoodPhoneNumber"], string.Empty);
                    if (!String.IsNullOrEmpty(phone))
                    {
                        string cleanphone = phone.Replace("/", "").Replace("-", "").Replace(".", "");
                        return "<a class=\"btn btnAccent1 stroke\" href=\"tel:" + cleanphone + "\">" + phone + "</a>";
                    }
                }
            }
            else if (builderID != null)
            {
                requestType = "builder";
                requestID = builderID;

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.developers", "DevelopersID = '" + builderID + "'");

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    string phone = ValidationHelper.GetString(dr["DeveloperPhone"], string.Empty);
                    if (!String.IsNullOrEmpty(phone))
                    {
                        string cleanphone = phone.Replace("/", "").Replace("-", "").Replace(".", "");
                        return "<a class=\"btn btnAccent1 stroke\"  href=\"tel:" + cleanphone + "\">" + phone + "</a>";
                    }
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

            string url = RequestContext.RawURL;
            string lotID = URLHelper.GetUrlParameter(url, "lot");
            string additional_info = "N/A";
            if (lotID != null)
            {
                additional_info = "Lot # " + lotID.ToString();
            }

            EmailMessage msg = new EmailMessage();
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate("BrochureRequest", SiteContext.CurrentSiteID);

            string[,] specialMacros = new string[,]
            { { "first_name", first_name },
              { "last_name", last_name},
              {"addres#", address},
              {"city", city},
              {"state", state},
              {"zip_code", zip_code},
              {"country", country},
              {"daytime_phone", daytime_phone},
              {"evening_phone", evening_phone},
              {"email_address", email_address},
              {"contact_name", contact_name},
              {"whensubmitted", whensubmitted},
              {"comments", comments},
              {"debug", ""},
              {"additionalInfo", additional_info } };

            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();

            resolver.SetNamedSourceData("first_name", first_name);
            resolver.SetNamedSourceData("last_name", last_name);
            resolver.SetNamedSourceData("addres#", address);
            resolver.SetNamedSourceData("city", city);
            resolver.SetNamedSourceData("state", state);
            resolver.SetNamedSourceData("zip_code", zip_code);
            resolver.SetNamedSourceData("country", country);
            resolver.SetNamedSourceData("daytime_phone", daytime_phone);
            resolver.SetNamedSourceData("evening_phone", evening_phone);
            resolver.SetNamedSourceData("email_address", email_address);
            resolver.SetNamedSourceData("contact_name", contact_name);
            resolver.SetNamedSourceData("whensubmitted", whensubmitted);
            resolver.SetNamedSourceData("comments", comments);
            resolver.SetNamedSourceData("debug", "");
            resolver.SetNamedSourceData("additionalInfo", additional_info);

            msg.From = "noreply@newhomesguidecharleston.com";
            msg.Subject = "Brochure Request";

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
                    if (recipient != null)
                    {
                        msg.Recipients = recipient;
                        msg.CcRecipients = "info@newhomesguidecharleston.com";
                        msg.BccRecipients = "louise@newhomesguidecharleston.com";
                        EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, false);
                    }
                }
            }
        }
    }
}