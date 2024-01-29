using PaymentProcessor.net.authorize.apitest;
using System;
using System.Data;
using System.Data.SqlClient;
using WTE.Helpers;

namespace PaymentProcessor
{
    /// <summary>
    /// Handles subscription sign up for The Stockwhisperer (old SW app purchase)
    /// https://thedarkpools.com/excellalite/SWRecurringBilling2.aspx
    /// </summary>
    public partial class SWRecurringBilling2 : System.Web.UI.Page
    {
        #region members

        private bool repost = false;
        public static net.authorize.apitest.Service _webservice = new net.authorize.apitest.Service();
        private string _userLoginName;
        private string _transactionKey;
        private bool _enableAch;
        private string _email = "";

        #endregion members

        #region properties

        private int SiteId
        {
            get
            {
                return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SW_SiteId"]);
            }
        }

        private int EmailTemplateID
        {
            get
            {
                int etid = 0;
                try
                {
                    etid = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SW_EmailTemplateID"]);
                }
                catch
                {
                    etid = 1180; // Membership - Notification - New registration
                }

                return etid;
            }
        }

        private string EmailFrom
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SW_EmailFrom"];
            }
        }

        private string EmailTo
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SW_EmailTo"];
            }
        }

        private string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["connstring"];
            }
        }

        private long _subscriptionId = 0;

        #endregion properties

        #region page events

        /// <summary>
        ///  Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Before doing anything, see if there is a qs, if there is, then clear it by storing the values in sessionstate
            MoveFromQStoSS("First");
            MoveFromQStoSS("Last");
            MoveFromQStoSS("Email");

            string redirectTo = Request.Url.AbsolutePath.ToString();

            if (repost == true) { Response.Redirect(redirectTo); }

            //if we make it here, we've got all of our values in sessionstate

            // start by setting the static values
            SetKeys();

            if (Session["First"] != null) { txtFirstName.Text = Session["First"].ToString(); }
            if (Session["Last"] != null) { txtLastName.Text = Session["Last"].ToString(); }
            if (Session["Email"] != null) { _email = Session["Email"].ToString(); }
        }

        #endregion page events

        #region general events

        protected void btnRun_Click(object sender, EventArgs e)
        {
            if (CreateSubscription())
            {
                //redirect if success

                try
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("Proc_SW_EmailSendToQueue", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@EmailSiteID", SqlDbType.Int).Value = SiteId;
                            if (EmailTemplateID > 0)
                            {
                                cmd.Parameters.Add("@EmailTemplateID", SqlDbType.Int).Value = EmailTemplateID;
                            }
                            if (EmailFrom.Length > 0)
                            {
                                cmd.Parameters.Add("@EmailFrom", SqlDbType.VarChar).Value = EmailFrom;
                            }
                            if (EmailTo.Length > 0)
                            {
                                cmd.Parameters.Add("@EmailTo", SqlDbType.VarChar).Value = EmailTo;
                            }

                            cmd.Parameters.Add("@EmailSubject", SqlDbType.VarChar).Value = "A New App Subscription with an ID of '" + _subscriptionId.ToString() + "' was successfully created.\n"; ;
                            string TokenReplacements = "";
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%UserCustomData%}", "AppUser");
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%firstname%}", txtFirstName.Text.Replace("|", ""));
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%lastname%}", txtLastName.Text.Replace("|", ""));
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%email%}", _email);
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%TrimSitePrefix(username)%}", _userLoginName);
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%HearAboutSW%}", "");
                            TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%HearAboutSWOther%}", "");
                            cmd.Parameters.Add("@TokenReplacements", SqlDbType.VarChar).Value = TokenReplacements;
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch
                {
                }

                Response.Redirect("/Intranet/chat-room");
            }
        }

        #endregion general events

        #region methods

        #region AUTH.Net

        private bool CreateSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBSubscriptionType subscription = new ARBSubscriptionType();
            PopulateSubscription(subscription, false);

            ARBCreateSubscriptionResponseType response;
            response = _webservice.ARBCreateSubscription(authentication, subscription);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                _subscriptionId = response.subscriptionId;
                lblMessage.Text += "A New App Subscription with an ID of '" + _subscriptionId.ToString() + "' was successfully created.\n";
                InsertRecord();
                tblFormFields.Visible = false;
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        private bool UpdateSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBSubscriptionType subscription = new ARBSubscriptionType();
            PopulateSubscription(subscription, true); // Expiration date will be different.

            ARBUpdateSubscriptionResponseType response;
            response = _webservice.ARBUpdateSubscription(authentication, _subscriptionId, subscription);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                lblMessage.Text += "The New App Subscription was successfully updated.<br />"; // CC
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        private bool CancelSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBCancelSubscriptionResponseType response;
            response = _webservice.ARBCancelSubscription(authentication, _subscriptionId);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                lblMessage.Text += "The New App subscription was successfully cancelled.<br />"; // CC
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Get the status of an existing ARB subscription
        /// </summary>
        // ----------------------------------------------------------------------------------------
        private bool GetStatusSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBGetSubscriptionStatusResponseType response;
            response = _webservice.ARBGetSubscriptionStatus(authentication, _subscriptionId);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                lblMessage.Text += "Status Text: " + response.status + "<br />";
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        private MerchantAuthenticationType PopulateMerchantAuthentication()
        {
            //TODO: Testing with hardcoded login and trans key
            MerchantAuthenticationType authentication = new MerchantAuthenticationType();
            authentication.name = _userLoginName; //"6AbXVr396sNg" = test value
            authentication.transactionKey = _transactionKey; // "4MB6X777sv43Mz6F" = test value
            return authentication;
        }

        private void PopulateSubscription(ARBSubscriptionType sub, bool bForUpdate)
        {
            CreditCardType creditCard = new CreditCardType();

            sub.name = "New App Subscription Purchased"; // CC

            creditCard.cardNumber = txtCCN.Text;
            creditCard.expirationDate = ddlYear.SelectedValue + "-" + ddlMonth.SelectedValue;  // required format for API is YYYY-MM
            creditCard.cardCode = txtSecurityCode.Text;
            sub.payment = new PaymentType();
            sub.payment.Item = creditCard;

            sub.billTo = new NameAndAddressType();
            sub.billTo.firstName = txtFirstName.Text;
            sub.billTo.lastName = txtLastName.Text;
            sub.billTo.address = txtAddress.Text;
            sub.billTo.city = txtCity.Text;
            sub.billTo.state = txtState.Text;
            sub.billTo.zip = txtPostalCode.Text;

            sub.paymentSchedule = new PaymentScheduleType();
            sub.paymentSchedule.startDate = DateTime.Today;
            sub.paymentSchedule.startDateSpecified = true;

            sub.paymentSchedule.totalOccurrences = 9999;
            sub.paymentSchedule.totalOccurrencesSpecified = true;
            sub.paymentSchedule.trialOccurrences = 0;               // CC: 1 -> 0.        no trial
            sub.paymentSchedule.trialOccurrencesSpecified = false;  // CC: true -> false. no trial

            sub.trialAmount = 0;              // CC: 9.95M -> 0.      no trail
            sub.trialAmountSpecified = false; // CC: true -> false.    no trial
            sub.amount = 14.99M;              // CC: 69.95M -> 14.99M. directly 14.99
            sub.amountSpecified = true;

            if (!bForUpdate)
            { // Interval can't be updated once a subscription is created.
                sub.paymentSchedule.interval = new PaymentScheduleTypeInterval();
                sub.paymentSchedule.interval.length = 1;
                sub.paymentSchedule.interval.unit = ARBSubscriptionUnitEnum.months;
            }
        }

        private void WriteErrors(ANetApiResponseType response)
        {
            //lblMessage.Text += "The API request failed with the following errors:<br />";
            for (int i = 0; i < response.messages.Length; i++)
            {
                lblMessage.Text += "[" + response.messages[i].code
                        + "] " + response.messages[i].text + "<br />";
            }
        }

        #endregion AUTH.Net

        #region data access

        private void SetKeys()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT KeyName, KeyValue FROM CMS_SettingsKey ​WHERE siteid=" + SiteId + " and keycategoryid=1061";
                        cmd.CommandType = CommandType.Text;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Call Read before accessing data.
                        while (reader.Read())
                        {
                            if (reader[0].ToString() == "loginID")
                            {
                                _userLoginName = reader[1].ToString();
                            }
                            if (reader[0].ToString() == "transactionKey")
                            {
                                _transactionKey = reader[1].ToString();
                            }
                            if (reader[0].ToString() == "EnableACH")
                            {
                                _enableAch = Convert.ToBoolean(reader[1].ToString());
                            }
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        WTELogging.LogException(new Exception("Error Obtaining Transaction Key", ex));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        private void InsertRecord()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "sp_InsertProcessed";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.VarChar, 50)).Value = txtFirstName.Text;
                        cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.VarChar, 50)).Value = txtLastName.Text;
                        cmd.Parameters.Add(new SqlParameter("@SubscriptionId", SqlDbType.Int, 4)).Value = _subscriptionId;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        WTELogging.LogException(new Exception("Error Updating processing log", ex));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        #endregion data access

        #region util

        private void MoveFromQStoSS(string name)
        {
            if (!string.IsNullOrEmpty(Request.QueryString[name]))
            {
                Session.Add(name, Request.QueryString[name]);
                repost = true;
            }
        }

        private string AddingTokenReplacement(string str, string key, string value)
        {
            if (str.Length > 0)
            {
                str += "|";
            }
            str += key + "=" + value;
            return str;
        }

        #endregion util

        #endregion methods
    }
}