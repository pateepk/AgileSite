using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PaymentProcessor.net.authorize.apitest;
using System.Data;
using System.Data.SqlClient;
using WTE.Helpers;

namespace PaymentProcessor
{
    public partial class RecurringBilling : System.Web.UI.Page
    {
        private bool repost = false;
        public static net.authorize.apitest.Service _webservice = new net.authorize.apitest.Service();
        private string _userLoginName;
        private string _transactionKey;
        private bool _enableAch;
        private int SiteId
        {
            get
            {
                return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SW_SiteId"]);
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

        protected void Page_Load(object sender, EventArgs e)
        {
            //Before doing anything, see if there is a qs, if there is, then clear it by storing the values in sessionstate
            MoveFromQStoSS("First");
            MoveFromQStoSS("Last");

            string redirectTo = Request.Url.AbsolutePath.ToString();

            if (repost == true) { Response.Redirect(redirectTo); }
            
            //if we make it here, we've got all of our values in sessionstate

            // start by setting the static values
            SetKeys();

            if (Session["First"] != null) { txtFirstName.Text = Session["First"].ToString(); }
            if (Session["Last"] != null) { txtLastName.Text = Session["Last"].ToString(); }

        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            CreateSubscription();
        }

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
                lblMessage.Text += "A subscription with an ID of '" + _subscriptionId.ToString() + "' was successfully created.\n";
                InsertRecord();
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
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
                        WTELogging.LogException(new Exception("Unable to insert processing log", ex));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
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
                lblMessage.Text += "The subscription was successfully updated.<br />";
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
                lblMessage.Text += "The subscription was successfully cancelled.<br />";
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

        private void PopulateSubscription(ARBSubscriptionType sub, bool bForUpdate)
        {
            CreditCardType creditCard = new CreditCardType();

            sub.name = "New subscription";

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

            // Create a subscription that has unlimited monthly payments starting on Jan 1, 2019

            sub.paymentSchedule = new PaymentScheduleType();
            sub.paymentSchedule.startDate = DateTime.Today;
            sub.paymentSchedule.startDateSpecified = true;

            sub.paymentSchedule.totalOccurrences = 9999;
            sub.paymentSchedule.totalOccurrencesSpecified = true;
            sub.paymentSchedule.trialOccurrences = 1;
            sub.paymentSchedule.trialOccurrencesSpecified = true;

            sub.trialAmount = 0;
            sub.trialAmountSpecified = true;
            sub.amount = 29;
            sub.amountSpecified = true;

            if (!bForUpdate)
            { // Interval can't be updated once a subscription is created.
                sub.paymentSchedule.interval = new PaymentScheduleTypeInterval();
                sub.paymentSchedule.interval.length = 1;
                sub.paymentSchedule.interval.unit = ARBSubscriptionUnitEnum.months;
            }
        }

        private MerchantAuthenticationType PopulateMerchantAuthentication()
        {
            MerchantAuthenticationType authentication = new MerchantAuthenticationType();
            authentication.name = _userLoginName;
            authentication.transactionKey = _transactionKey;
            return authentication;
        }

        private void WriteErrors(ANetApiResponseType response)
        {
            lblMessage.Text += "The API request failed with the following errors:<br />";
            for (int i = 0; i < response.messages.Length; i++)
            {
                lblMessage.Text += "[" + response.messages[i].code
                        + "] " + response.messages[i].text + "<br />";
            }
        }

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

        private void MoveFromQStoSS(string name)
        {
            if (!string.IsNullOrEmpty(Request.QueryString[name]))
            {
                Session.Add(name, Request.QueryString[name]);
                repost = true;
            }
        }

    }
}
