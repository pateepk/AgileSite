using System;
using System.Data;

using CMS.EmailEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.MacroEngine;

namespace NHG_T
{
    public partial class CMSWebParts_BizForms_bizform_guide_subscription_NHGT : CMSAbstractWebPart
    {
        #region "Properties"

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

        #endregion "Properties"

        #region "Page Events"

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

        #endregion "Page Events"

        #region "Form Binding"

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

                // Set the live site context
                if (viewBiz != null)
                {
                    viewBiz.ControlContext.ContextName = CMS.Base.Web.UI.ControlContext.LIVE_SITE;
                }
            }
        }

        #endregion "Form Binding"

        #region Event Handlers"

        protected void viewBiz_OnAfterSave(object sender, EventArgs e)
        {
            string first_name = ValidationHelper.GetString(viewBiz.Data["FirstName"], "N/A");
            string last_name = ValidationHelper.GetString(viewBiz.Data["LastName"], "N/A");
            string address_1 = ValidationHelper.GetString(viewBiz.Data["Address1"], "N/A");
            string address_2 = ValidationHelper.GetString(viewBiz.Data["Address2"], "N/A");
            string city = ValidationHelper.GetString(viewBiz.Data["City"], "N/A");
            string state = ValidationHelper.GetString(viewBiz.Data["State"], "N/A");
            string zip_code = ValidationHelper.GetString(viewBiz.Data["ZipCode"], "N/A");
            string country = ValidationHelper.GetString(viewBiz.Data["Country"], "N/A");
            string phone = ValidationHelper.GetString(viewBiz.Data["Phone"], "N/A");
            string email_address = ValidationHelper.GetString(viewBiz.Data["EmailAddress"], "N/A");
            string builder = ValidationHelper.GetString(viewBiz.Data["Builder"], "N/A");
            string type = ValidationHelper.GetString(viewBiz.Data["Type"], "N/A");
            string price_range = ValidationHelper.GetString(viewBiz.Data["PriceRange"], "N/A");
            string purchase_home = ValidationHelper.GetString(viewBiz.Data["PurchaseHome"], "N/A");
            string hear_about_us = ValidationHelper.GetString(viewBiz.Data["HearAboutUs"], "N/A");
            string hear_other = ValidationHelper.GetString(viewBiz.Data["HearOther"], "N/A");
            string comments = ValidationHelper.GetString(viewBiz.Data["Comments"], "N/A");
            string whensubmitted = ValidationHelper.GetString(DateTime.Now, "N/A");

            EmailMessage msg = new EmailMessage();
            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate("GuideSubscription", SiteContext.CurrentSiteID);

            //string[,] specialMacros = new string[,] { { "first_name", first_name }, { "last_name", last_name }, { "address_1", address_1 }, { "address_2", address_2 }, { "city", city }, { "state", state },
            //                                          { "zip_Code", zip_code }, { "country", country }, { "phone", phone }, { "email_address", email_address }, {"builder", builder }, { "type", type },
            //                                          { "price_range", price_range }, { "purchase_home", purchase_home }, {"hear_about_us", hear_about_us }, { "hear_other", hear_other }, { "comments", comments },
            //                                          { "submitteddate", whensubmitted }, { "debug", "" } };

            //create table for macros (token replacement in email template)
            DataTable dt = new DataTable();
            dt.Columns.Add("first_name");
            dt.Columns.Add("last_name");
            dt.Columns.Add("address_1");
            dt.Columns.Add("address_2");
            dt.Columns.Add("city");
            dt.Columns.Add("state");
            dt.Columns.Add("zip_code");
            dt.Columns.Add("country");
            dt.Columns.Add("phone");
            dt.Columns.Add("email_address");
            dt.Columns.Add("builder");
            dt.Columns.Add("type");
            dt.Columns.Add("price_range");
            dt.Columns.Add("purchase_home");
            dt.Columns.Add("hear_about_us");
            dt.Columns.Add("hear_other");
            dt.Columns.Add("comments");
            dt.Columns.Add("submitteddate");
            dt.Columns.Add("debug");

            //add data row with values
            DataRow drRow = dt.NewRow();
            drRow["first_name"] = first_name;
            drRow["last_name"] = last_name;
            drRow["address_1"] = address_1;
            drRow["address_2"] = address_2;
            drRow["city"] = city;
            drRow["state"] = state;
            drRow["zip_code"] = zip_code;
            drRow["country"] = country;
            drRow["phone"] = phone;
            drRow["email_address"] = email_address;
            drRow["builder"] = builder;
            drRow["type"] = type;
            drRow["price_range"] = price_range;
            drRow["purchase_home"] = purchase_home;
            drRow["hear_about_us"] = hear_about_us;
            drRow["hear_other"] = hear_other;
            drRow["comments"] = comments;
            drRow["submitteddate"] = whensubmitted;
            drRow["debug"] = "";

            //create macro resolver object to pass to template
            var resolver = MacroResolver.GetInstance();
            resolver.SetAnonymousSourceData(drRow);

            msg.From = (SiteContext.CurrentSiteName.ToLower().StartsWith("grand") ? "noreply@grandstrandnewhomeguide.com" : "noreply@triadnewhomeguide.com");
            msg.Subject = "Guide Subscription Request";

            foreach (string builder_email in getBuildersEmails(builder))
            {
                msg.Recipients = builder_email;
                msg.CcRecipients = "info@newhomeguides.com";
                msg.BccRecipients = "day@newhomeguides.com";

                EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, true);
            }
        }

        #endregion Event Handlers"

        #region "Helpers"

        private string[] getBuildersEmails(string builders)
        {
            string[] builder_emails = null;

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;

            string[] builder_names = builders.Split('|');

            if (builder_names.Length > 0)
            {
                string where = "1=1 AND (";
                foreach (string name in builder_names)
                {
                    if (name != null)
                    {
                        where += "DeveloperName = '" + name + "' OR ";
                    }
                }
                where = where.Substring(0, where.Length - 4) + ")";

                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", where, "DeveloperName ASC");

                DataTable dt = ds.Tables[0];

                builder_emails = new string[dt.Rows.Count];
                int i = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    builder_emails[i] = dr["DeveloperEmails"].ToString();
                    i++;
                }
            }

            return builder_emails;
        }

        #endregion "Helpers"
    }
}