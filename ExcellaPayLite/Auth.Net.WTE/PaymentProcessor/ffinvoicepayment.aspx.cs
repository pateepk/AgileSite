using AuthorizeNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web;
using WTE.Helpers;

namespace PaymentProcessor
{
    public partial class ffinvoicepayment : System.Web.UI.Page
    {
        protected int year = System.DateTime.Now.Year;
        private bool repost = false;
        private bool _enableAch;
        private string _userLoginName;
        private string _transactionKey;

        private int SiteId
        {
            get
            {
                return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["FF_SiteId"]);
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

        public ffinvoicepayment()
        {
            Load += Page_Load;
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            SetKeys();
            pnlForm.Visible = true;
            pnlResult.Visible = false;
            ltlErrors.Text = "";

            if (IsPostBack)
            {
                SendToAuthNet();
            }
            else
            {
                InitForm();
            }
        }

        protected void InitForm()
        {
            //Before doing anything, see if there is a qs, if there is, then clear it by storing the values in sessionstate
            MoveFromQStoSS("amount");
            MoveFromQStoSS("description");
            MoveFromQStoSS("invoice");
            MoveFromQStoSS("CID");

            string redirectTo = Request.Url.AbsolutePath.ToString();

            if (repost == true) { Response.Redirect(redirectTo); }

            //if we make it here, we've got all of our values in sessionstate
            string sequence = Crypto.GenerateSequence().ToString();
            string timeStamp = Crypto.GenerateTimestamp().ToString();
            decimal amount = 1;
            string description = "Sample Transaction";
            string cust_id = "0000";
            string invoice = DateTime.Now.ToString("yyyyMMddHHmmss").ToString();

            if (Session["amount"] != null) { amount = Convert.ToDecimal(Session["amount"]); }
            if (Session["invoice"] != null) { invoice = Session["invoice"].ToString(); }
            if (Session["description"] != null) { description = Session["description"].ToString(); }
            if (Session["CID"] != null) { cust_id = Session["CID"].ToString(); }

            var fingerPrint = Crypto.GenerateFingerprint(_transactionKey, _userLoginName, amount, sequence, timeStamp);

            lblDescription.Text = description;
            lblInvoiceNumber.Text = invoice;

            x_login.Value = _userLoginName;
            x_invoice_num.Value = invoice;
            x_amount.Value = amount.ToString("F");
            x_description.Value = description;
            x_test_request.Value = "false";
            x_fp_sequence.Value = sequence;
            x_fp_timestamp.Value = timeStamp;
            x_fp_hash.Value = fingerPrint;
            x_cust_id.Value = cust_id;//hide ach for certain accounts

            if (_enableAch == false)
            {
                tdPayByBankAccount1.Visible = false;
                tdPayByBankAccount2.Visible = false;
            }
        }

        protected void SendToAuthNet()
        {
            String post_url = _formAction;
            Dictionary<string, string> post_values = new Dictionary<string, string>();

            post_values.Add("x_login", _userLoginName);
            post_values.Add("x_tran_key", _transactionKey);
            post_values.Add("x_delim_data", "TRUE");
            post_values.Add("x_delim_char", "|");
            post_values.Add("x_relay_response", "FALSE");

            post_values.Add("x_amount", Request.Form["x_amount"]);
            post_values.Add("x_invoice_num", Request.Form["x_invoice_num"]);
            post_values.Add("x_fp_sequence", Request.Form["x_fp_sequence"]);
            post_values.Add("x_fp_timestamp", Request.Form["x_fp_timestamp"]);
            post_values.Add("x_fp_hash", Request.Form["x_fp_hash"]);
            post_values.Add("x_cust_id", Request.Form["x_cust_id"]);
            post_values.Add("x_test_request", Request.Form["x_test_request"]);

            post_values.Add("x_method_available", Request.Form["x_method_available"]);
            post_values.Add("x_type", Request.Form["x_type"]);
            post_values.Add("x_method", Request.Form["x_method"]);

            post_values.Add("x_card_num", Request.Form["x_card_num"]);
            post_values.Add("x_exp_date", Request.Form["x_exp_date"]);
            post_values.Add("x_card_code", Request.Form["x_card_code"]);

            post_values.Add("x_bank_name", Request.Form["x_bank_name"]);
            post_values.Add("x_bank_acct_num", Request.Form["x_bank_acct_num"]);
            post_values.Add("x_bank_aba_code", Request.Form["x_bank_aba_code"]);
            post_values.Add("x_bank_acct_name", Request.Form["x_bank_acct_name"]);
            post_values.Add("x_bank_acct_type", Request.Form["x_bank_acct_type"]);

            post_values.Add("x_first_name", Request.Form["x_first_name"]);
            post_values.Add("x_last_name", Request.Form["x_last_name"]);
            post_values.Add("x_company", Request.Form["x_company"]);
            post_values.Add("x_email", Request.Form["x_email"]);
            post_values.Add("x_phone", Request.Form["x_phone"]);
            post_values.Add("x_fax", Request.Form["x_fax"]);
            post_values.Add("x_address", Request.Form["x_address"]);
            post_values.Add("x_state", Request.Form["x_state"]);
            post_values.Add("x_zip", Request.Form["x_zip"]);
            post_values.Add("x_country", Request.Form["x_country"]);

            // Additional fields can be added here as outlined in the AIM integration
            // guide at: http://developer.authorize.net

            String post_string = "";

            foreach (KeyValuePair<string, string> post_value in post_values)
            {
                post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
            }
            post_string = post_string.TrimEnd('&');

            // create an HttpWebRequest object to communicate with Authorize.net
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(post_url);
            objRequest.Method = "POST";
            objRequest.ContentLength = post_string.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";

            // post data is sent as a stream
            StreamWriter myWriter = null;
            myWriter = new StreamWriter(objRequest.GetRequestStream());
            myWriter.Write(post_string);
            myWriter.Close();

            // returned values are returned as a stream, then read into a string
            String post_response;
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }

            // The split character specified here must match the delimiting character specified above
            List<string> ANResponse = new List<string>(post_response.Split('|'));
            UpdateDBwithResult(ANResponse);

            if (GetResultValue(ResponseOrder.ResponseCode, ANResponse) != "1")
            {
                ltlErrors.Text = GetResultValue(ResponseOrder.ResponseReasonText, ANResponse);
            }
            else
            {
                ShowReciept(ANResponse);
            }
        }

        private void ShowReciept(List<string> r)
        {
            string method;

            if (pmCC.Checked)
            {
                method = "Credit Card - ************" +
                    x_card_num.Value.Substring(x_card_num.Value.Length - 1 - 4, 4);
            }
            else
            {
                method = "Bank Account - ********" +
                    x_card_num.Value.Substring(x_bank_acct_num.Value.Length - 1 - 4, 4);
            }

            pnlForm.Visible = false;
            pnlResult.Visible = true;

            lblRAddress.Text = GetResultValue(ResponseOrder.Address, r);
            lblRAmount.Text = GetResultValue(ResponseOrder.Amount, r);
            lblRAuthCode.Text = GetResultValue(ResponseOrder.AuthorizationCode, r);
            lblRCity.Text = x_city.Value;
            lblRCompany.Text = GetResultValue(ResponseOrder.Company, r);
            lblRCountry.Text = GetResultValue(ResponseOrder.Country, r);
            lblRCustomerId.Text = GetResultValue(ResponseOrder.CustomerID, r);
            lblRDateTime.Text = DateTime.Now.ToString();
            lblRDateTime2.Text = DateTime.Now.ToString();
            //lblRDescription.Text = GetResultValue(ResponseOrder.Description, r);
            lblREmail.Text = GetResultValue(ResponseOrder.EmailAddress, r);
            lblRFirstName.Text = GetResultValue(ResponseOrder.FirstName, r);
            lblRInvoiceNumber.Text = GetResultValue(ResponseOrder.InvoiceNumber, r);
            lblRLastName.Text = GetResultValue(ResponseOrder.LastName, r);
            lblRMerchant.Text = x_description.Value;
            lblRMethod.Text = method;
            lblRMethod2.Text = method;
            lblRPhone.Text = GetResultValue(ResponseOrder.Phone, r);
            lblRState.Text = GetResultValue(ResponseOrder.State, r);
            lblRTransactionId.Text = GetResultValue(ResponseOrder.TransactionID, r);
            lblRZip.Text = GetResultValue(ResponseOrder.ZIPCode, r);
        }

        private string GetSQLCommand(List<string> r)
        {
            string accountNumber;
            if (pmCC.Checked)
            {
                accountNumber = "************" +
                    x_card_num.Value.Substring(x_card_num.Value.Length - 1 - 4, 4);
            }
            else
            {
                accountNumber = "********" +
                    x_card_num.Value.Substring(x_bank_acct_num.Value.Length - 1 - 4, 4);
            }

            string result =
                "INSERT INTO [dbo].[customtable_fortifiber_payments] ([x_response_code],[x_response_reason_code],[x_response_reason_text],[x_auth_code],[x_avs_code]," +
                "[x_trans_id],[x_invoice_num],[x_description],[x_amount],[x_method],[x_type],[x_email],[x_account_number],[x_cvv2_resp_code],[x_cust_id],[ItemCreatedWhen])" +
                " VALUES (" +
                GetResultValue(ResponseOrder.ResponseCode, r) + "," +
                GetResultValue(ResponseOrder.ResponseReasonCode, r) + "," +
                "'" + GetResultValue(ResponseOrder.ResponseReasonText, r) + "'," +
                "'" + GetResultValue(ResponseOrder.AuthorizationCode, r) + "'," +
                "'" + GetResultValue(ResponseOrder.AVSResponse, r) + "'," +
                "'" + GetResultValue(ResponseOrder.TransactionID, r) + "'," +
                "'" + GetResultValue(ResponseOrder.InvoiceNumber, r) + "'," +
                "'" + GetResultValue(ResponseOrder.Description, r) + "'," +
                GetResultValue(ResponseOrder.Amount, r) + "," +
                "'" + GetResultValue(ResponseOrder.Method, r) + "'," +
                "'" + GetResultValue(ResponseOrder.TransactionType, r) + "'," +
                "'" + GetResultValue(ResponseOrder.EmailAddress, r) + "'," +
                "'" + accountNumber + "'," +
                "'" + GetResultValue(ResponseOrder.CardCodeResponse, r) + "'," +
                //"'" + GetResultValue(ResponseOrder.CardholderAuthenticationVerificationResponse, r) + "'," + <-- not seeing this field returned
                "'" + GetResultValue(ResponseOrder.CustomerID, r) + "'," +
                "'" + DateTime.Now.ToShortDateString() + "'" +
                ") ";

            /******************************************
             * Fields in our table, not in response:
            //x_card_type
            //ItemCreatedBy
            //ItemCreatedWhen
            //ItemModifiedBy
            //ItemModifiedWhen
            //ItemOrder
            //ItemGUID
             * CardholderAuthenticationVerificationResponse
            ******************************************/

            return result;
        }

        private void UpdateDBwithResult(List<string> response)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        string sqlCommand = GetSQLCommand(response);

                        cmd.CommandText = sqlCommand;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        WTELogging.LogException(new Exception("Unable to update DB with Result", ex));
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        private string GetResultValue(ResponseOrder ResponseItemName, List<string> response)
        {
            int idx = (int)ResponseItemName - 1;
            string result = response[idx];
            return result;
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
                        //cmd.Parameters.Add(new SqlParameter("@SiteID", SqlDbType.Int)).Value = SiteId;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Call Read before accessing data.
                        while (reader.Read())
                        {
                            if (reader[0].ToString() == "loginID")
                            {
                                //NOTE: TESTING
                                _userLoginName = reader[1].ToString(); //Database
                                //_userLoginName = "6AbXVr396sNg"; //QA
                            }
                            if (reader[0].ToString() == "transactionKey")
                            {
                                //NOTE: TESTING
                                _transactionKey = reader[1].ToString(); //Database
                                //_transactionKey = "4MB6X777sv43Mz6F"; //QA
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

        private enum ResponseOrder
        {
            ResponseCode = 1,
            ResponseSubcode = 2,
            ResponseReasonCode = 3,
            ResponseReasonText = 4,
            AuthorizationCode = 5,
            AVSResponse = 6,
            TransactionID = 7,
            InvoiceNumber = 8,
            Description = 9,
            Amount = 10,
            Method = 11,
            TransactionType = 12,
            CustomerID = 13,
            FirstName = 14,
            LastName = 15,
            Company = 16,
            Address = 17,
            City = 18,
            State = 19,
            ZIPCode = 20,
            Country = 21,
            Phone = 22,
            Fax = 23,
            EmailAddress = 24,
            ShipToFirstName = 25,
            ShipToLastName = 26,
            ShipToCompany = 27,
            ShipToAddress = 28,
            ShipToCity = 29,
            ShipToState = 30,
            ShipToZIPCode = 31,
            ShipToCountry = 32,
            Tax = 33,
            Duty = 34,
            Freight = 35,
            TaxExempt = 36,
            PurchaseOrderNumber = 37,
            MD5Hash = 38,
            CardCodeResponse = 39,
            CardholderAuthenticationVerificationResponse = 40
        }
    }
}