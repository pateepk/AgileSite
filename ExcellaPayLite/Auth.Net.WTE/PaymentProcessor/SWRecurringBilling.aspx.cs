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
    /// <summary>
    /// Handles subscription sign up for The Stockwhisperer JavaPit
    /// https://thedarkpools.com/excellalite/SWRecurringBilling.aspx
    /// </summary>
    public partial class SWRecurringBilling : System.Web.UI.Page
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

            int Cyear = DateTime.Now.Year;
            for (int i = 0; i < 10; i++)
            {
                ddlYear.Items.Add(new ListItem(Cyear.ToString(), Cyear.ToString()));
                Cyear++;
            }
            

        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            if (Validation() && CreateSubscription())
            {
                //redirect if success
                Response.Redirect("/Intranet/chat-room");
            }
        }

        private bool Validation()
        {
            bool vResult = true;


            if (vResult && (txtFirstName.Text.Length == 0))
            {
                lblMessage.Text += "First name is required.";
                vResult = false;
            }

            if (vResult && (txtLastName.Text.Length == 0))
            {
                lblMessage.Text += "Last name is required.";
                vResult = false;
            }

            if (vResult && (txtAddress.Text.Length == 0))
            {
                lblMessage.Text += "Street address is required.";
                vResult = false;
            }

            if (vResult && (txtCity.Text.Length == 0))
            {
                lblMessage.Text += "City is required.";
                vResult = false;
            }

            if (vResult && (txtState.Text.Length == 0))
            {
                lblMessage.Text += "State is required.";
                vResult = false;
            }

            if (vResult && (txtPostalCode.Text.Length == 0))
            {
                lblMessage.Text += "Postal code is required.";
                vResult = false;
            }

            if (vResult && (txtEmailAddress.Text.Length == 0 || txtEmailAddress.Text.IndexOf("@") == -1))
            {
                lblMessage.Text += "Email address is required.";
                vResult = false;
            }

            if (vResult && (txtCCN.Text.Length == 0))
            {
                lblMessage.Text += "Card number is required.";
                vResult = false;
            }

            if (vResult && (ddlMonth.SelectedValue == "0"))
            {
                lblMessage.Text += "Card expiration month is required.";
                vResult = false;

            }

            if (vResult && (ddlYear.SelectedValue == "0"))
            {
                lblMessage.Text += "Card expiration year is required.";
                vResult = false;


            }

            if (vResult && (txtSecurityCode.Text.Length == 0))
            {
                lblMessage.Text += "Security code required.";
                vResult = false;
            }


            return vResult;
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
                tblFormFields.Visible = false;

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

            sub.customer = new CustomerType();
            sub.customer.email = txtEmailAddress.Text;
            
            sub.paymentSchedule = new PaymentScheduleType();
            sub.paymentSchedule.startDate = DateTime.Today;
            sub.paymentSchedule.startDateSpecified = true;

            sub.paymentSchedule.totalOccurrences = 9999;
            sub.paymentSchedule.totalOccurrencesSpecified = true;
            sub.paymentSchedule.trialOccurrences = 1;
            sub.paymentSchedule.trialOccurrencesSpecified = true;

            sub.trialAmount = 9.95M;
            sub.trialAmountSpecified = true;
            // sub.amount = 99.95M; // WTE: #17348
            sub.amount = 129.95M; // WTE: #18773
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
            //TODO: Testing with hardcoded login and trans key
            MerchantAuthenticationType authentication = new MerchantAuthenticationType();
            authentication.name = _userLoginName; //"6AbXVr396sNg" = test value
            authentication.transactionKey = _transactionKey; // "4MB6X777sv43Mz6F" = test value
            return authentication;
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
