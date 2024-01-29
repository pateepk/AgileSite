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

namespace PaymentProcessor
{
    public partial class OnTimeBilling : System.Web.UI.Page
    {
        public static net.authorize.apitest.Service _webservice = new net.authorize.apitest.Service();
        private string _userLoginName
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["userLogin"];
            }
        }
        private string _transactionKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["transactionKey"];
            }
        }

        private string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["connstring"];
            }
        }

        private string _formAction
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["formaction"];
            }
        }
        private long _subscriptionId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {

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
                //txtResults.Text += "A subscription with an ID of '" + _subscriptionId.ToString() + "' was successfully created.\n";
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

            // Create a subscription that is 12 monthly payments starting on Jan 1, 2019

            sub.paymentSchedule = new PaymentScheduleType();
            sub.paymentSchedule.startDate = DateTime.Today;
            sub.paymentSchedule.startDateSpecified = true;

            sub.paymentSchedule.totalOccurrences = 1;
            sub.paymentSchedule.totalOccurrencesSpecified = true;

            sub.amount = 124M;
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
    }
}
