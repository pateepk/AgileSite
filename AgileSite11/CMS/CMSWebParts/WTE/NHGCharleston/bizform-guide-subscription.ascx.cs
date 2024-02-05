using System;
using System.Data;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace NHG_C
{
    public partial class CMSWebParts_BizForms_bizform_guide_subscription_NHGC : CMSAbstractWebPart
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

        #region "Event Handlers"

        /// <summary>
        /// On form after save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

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

            string[,] specialMacros = new String[,]
            { { "first_name", first_name },
              { "last_name", last_name},
              {"address_1", address_1},
              {"address_2", address_2},
              {"city", city},
              {"state", state},
              {"zip_code", zip_code},
              {"country", country},
              {"phone", phone},
              {"email_address", email_address},
              {"builder", builder},
              {"type", type},
              {"price_range", price_range},
              {"purchase_home", purchase_home},
              {"hear_about_us", hear_about_us},
              {"hear_other", hear_other},
              {"comments", comments},
              {"submitteddate", whensubmitted},
              {"debug", ""}};

            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();

            resolver.SetNamedSourceData("first_name", first_name);
            resolver.SetNamedSourceData("last_name", last_name);
            resolver.SetNamedSourceData("address_1", address_1);
            resolver.SetNamedSourceData("address_2", address_2);
            resolver.SetNamedSourceData("city", city);
            resolver.SetNamedSourceData("state", state);
            resolver.SetNamedSourceData("zip_code", zip_code);
            resolver.SetNamedSourceData("country", country);
            resolver.SetNamedSourceData("phone", phone);
            resolver.SetNamedSourceData("email_address", email_address);
            resolver.SetNamedSourceData("builder", builder);
            resolver.SetNamedSourceData("type", type);
            resolver.SetNamedSourceData("price_range", price_range);
            resolver.SetNamedSourceData("purchase_home", purchase_home);
            resolver.SetNamedSourceData("hear_about_us", hear_about_us);
            resolver.SetNamedSourceData("hear_other", hear_other);
            resolver.SetNamedSourceData("comments", comments);
            resolver.SetNamedSourceData("submitteddate", whensubmitted);
            resolver.SetNamedSourceData("debug", "");

            msg.From = "noreply@newhomesguidecharleston.com";
            msg.Subject = "Guide Subscription Request";

            string[] builder_emails = getBuildersEmails(builder);

            if (builder_emails != null)
            {
                foreach (string builder_email in builder_emails)
                {
                    msg.Recipients = builder_email;
                    msg.CcRecipients = "info@newhomesguidecharleston.com";
                    msg.BccRecipients = "louise@newhomesguidecharleston.com";
                    EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, false);
                }
            }
            else
            {
                msg.Recipients = "info@newhomesguidecharleston.com";
                msg.BccRecipients = "louise@newhomesguidecharleston.com";
                EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, resolver, false);
            }
        }

        #endregion "Event Handlers"

        #region "Helpers"

        private string[] getBuildersEmails(string builders)
        {
            string[] builder_emails = null;

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;

            if (!String.IsNullOrEmpty(builders))
            {
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
            }
            return builder_emails;
        }

        #endregion "Helpers"

        #region not compatible with V11

        /*
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

            string[,] specialMacros = new String[,] { { "first_name", first_name }, { "last_name", last_name}, {"address_1", address_1}, {"address_2", address_2}, {"city", city},
                                            {"state", state}, {"zip_code", zip_code}, {"country", country}, {"phone", phone}, {"email_address", email_address}, {"builder", builder},
                                            {"type", type}, {"price_range", price_range}, {"purchase_home", purchase_home}, {"hear_about_us", hear_about_us}, {"hear_other", hear_other},
                                            {"comments", comments}, {"submitteddate", whensubmitted}, {"debug", ""}};

            msg.From = "noreply@newhomesguidecharleston.com";
            msg.Subject = "Guide Subscription Request";

            string[] builder_emails = getBuildersEmails(builder);

            if (builder_emails != null)
            {
                foreach (string builder_email in builder_emails)
                {
                    msg.Recipients = builder_email;
                    msg.CcRecipients = "info@newhomesguidecharleston.com";
                    msg.BccRecipients = "louise@newhomesguidecharleston.com";
                    EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, specialMacros);
                }
            }
            else
            {
                msg.Recipients = "info@newhomesguidecharleston.com";
                msg.BccRecipients = "louise@newhomesguidecharleston.com";
                EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, msg, template, specialMacros);
            }
        }
        */

        #endregion not compatible with V11
    }
}