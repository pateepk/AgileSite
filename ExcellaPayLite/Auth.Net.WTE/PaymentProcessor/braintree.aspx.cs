using Braintree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WTE.Helpers;

namespace PaymentProcessor
{
    /// <summary>
    /// Sample page for braintree invoince payment (this does not actually work)
    /// </summary>
    public partial class braintree : System.Web.UI.Page
    {
        // protected int year = System.DateTime.Now.Year;
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

        private BraintreeGateway Gateway = new BraintreeGateway
        {
            Environment = Braintree.Environment.SANDBOX,
            PublicKey = "b2w6vffttqnqjc74",
            PrivateKey = "8b95b46e8ebcb274fa474115ea299637",
            MerchantId = "wnwfczj2w3ty3j8t"
        };

        public braintree()
        {
            Load += Page_Load;
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            //  SetKeys();
            pnlForm.Visible = true;
            pnlResult.Visible = false;
            ltlErrors.Text = "";

            if (IsPostBack)

            {
                // Send to Braintree
                TransactionRequest transactionRequest = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(Request.Form["x_amount"]),
                    OrderId = Request.Form["x_invoice_num"],

                    CreditCard = new TransactionCreditCardRequest
                    {
                        Number = Request.Form["x_card_num"],
                        CVV = Request.Form["x_card_code"],
                        ExpirationMonth = Request.Form["x_exp_date_MM"],
                        ExpirationYear = Request.Form["x_exp_date_YY"],
                        //  Token = "125"
                    },
                    Customer = new CustomerRequest
                    {
                        // Id = x_cust_id.Value,
                        FirstName = Request.Form["x_first_name"],
                        LastName = Request.Form["x_last_name"],
                        Company = Request.Form["x_company"],
                        // Phone = "312-555-1234",
                        // Fax = "312-555-1235",
                        // Website = "http://www.example.com",
                        Email = Request.Form["x_email"]
                    },
                    BillingAddress = new AddressRequest
                    {
                        FirstName = Request.Form["x_first_name"],
                        LastName = Request.Form["x_last_name"],
                        Company = Request.Form["x_company"],
                        StreetAddress = Request.Form["x_address"],
                        // ExtendedAddress = "",
                        Locality = Request.Form["x_city"],
                        Region = Request.Form["x_state"],
                        PostalCode = Request.Form["x_zip"],
                        CountryCodeAlpha2 = Request.Form["x_country"]
                    },

                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true,
                        // StoreInVaultOnSuccess = true
                    },
                    Channel = "MyRunnerWebsite"
                };
                Result<Transaction> result = Gateway.Transaction.Sale(transactionRequest);

                pnlForm.Visible = false;
                pnlResult.Visible = true;

                //Start of Braintree Logging

                if (result.IsSuccess())
                {
                    Transaction transaction = result.Target;
                    Console.WriteLine("Success!: " + transaction.Id);
                    lblRTransactionId.Text = transaction.Id;

                    UpdateDBwithResult(transaction.ProcessorResponseCode, transaction.Id, transaction.ProcessorResponseText, transaction.Amount, transaction.ProcessorAuthorizationCode, x_invoice_num.Value);
                    //, transaction.Type, transaction.ProcessorAuthorizationCode
                }
                else if (result.Transaction != null)
                {
                    Transaction transaction = result.Transaction;
                    Console.WriteLine("Error processing transaction:");

                    lblRDescription.Text = ("  Status: " + transaction.Status) +
                    ("  Code: " + transaction.ProcessorResponseCode) +
                    ("  Text: " + transaction.ProcessorResponseText);
                }
                else
                {
                    foreach (ValidationError error in result.Errors.DeepAll())
                    {
                        lblRDescription.Text = ("Attribute: " + error.Attribute) +
                        ("  Code: " + error.Code) +
                        ("  Message: " + error.Message);
                    }
                }

                // END OF BrainTree Code
            }
            else
            {
                InitForm();
            }
        }

        private void ShowReciept(Transaction transaction)
        {
            throw new NotImplementedException();
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
            //string sequence = Crypto.GenerateSequence().ToString();
            //string timeStamp = Crypto.GenerateTimestamp().ToString();
            decimal amount = 1.01M;
            string description = "Sample Transaction";
            string cust_id = "0000";
            string invoice = DateTime.Now.ToString("yyyyMMddHHmmss").ToString();

            if (Session["amount"] != null) { amount = Convert.ToDecimal(Session["amount"]); }
            if (Session["invoice"] != null) { invoice = Session["invoice"].ToString(); }
            if (Session["description"] != null) { description = Session["description"].ToString(); }
            if (Session["CID"] != null) { cust_id = Session["CID"].ToString(); }

            var fingerPrint = "test";

            lblDescription.Text = description;
            lblInvoiceNumber.Text = invoice;

            x_login.Value = _userLoginName;
            x_invoice_num.Value = invoice;
            x_amount.Value = amount.ToString("F");
            x_description.Value = description;
            x_test_request.Value = "false";
            //  x_fp_sequence.Value = sequence;
            // x_fp_timestamp.Value = timeStamp;
            x_fp_hash.Value = fingerPrint;
            x_cust_id.Value = cust_id;//hide ach for certain accounts

            if (_enableAch == false)
            {
                tdPayByBankAccount1.Visible = false;
                tdPayByBankAccount2.Visible = false;
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

            // lblRAddress.Text = GetResultValue(ResponseOrder.Address, r);
            //lblRAmount.Text = GetResultValue(ResponseOrder.Amount, r);
            //lblRAuthCode.Text = GetResultValue(ResponseOrder.AuthorizationCode, r);
            //lblRCompany.Text = GetResultValue(ResponseOrder.Company, r);
            //lblRCountry.Text = GetResultValue(ResponseOrder.Country, r);
            //lblRCustomerId.Text = GetResultValue(ResponseOrder.CustomerID, r);
            //lblRDateTime.Text = DateTime.Now.ToString();
            //lblRDateTime2.Text = DateTime.Now.ToString();
            //lblRDescription.Text = GetResultValue(ResponseOrder.Description, r);
            //lblREmail.Text = GetResultValue(ResponseOrder.EmailAddress, r);
            //lblRFirstName.Text = GetResultValue(ResponseOrder.FirstName, r);
            //lblRInvoiceNumber.Text = GetResultValue(ResponseOrder.InvoiceNumber, r);
            //lblRLastName.Text = GetResultValue(ResponseOrder.LastName, r);
            //lblRMerchant.Text = x_description.Value;
            //lblRMethod.Text = method;
            //lblRMethod2.Text = method;
            //lblRPhone.Text = GetResultValue(ResponseOrder.Phone, r);
            //lblRState.Text = GetResultValue(ResponseOrder.State, r);
            // lblRTransactionId.Text = transaction.Id;
            //lblRZip.Text = GetResultValue(ResponseOrder.ZIPCode, r);
        }

        private void UpdateDBwithResult(string ProcessorResponseCode, string TransID, string ProcessorResponseText, decimal? Amount, string ProcessorAuthorizationCode, string InvoiceID)
        // transaction.Type, transaction.ProcessorAuthorizationCode

        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Insert into  [AS8_production_agilesite6].[dbo].[customtable_myrunner_payments] ([x_response_code],[x_auth_code],[x_response_reason_text],[x_amount],[x_trans_id],[x_invoice_num]) values('" + ProcessorResponseCode + "', '" + TransID + "', '" + ProcessorResponseText + "', " + Amount + ", '" + ProcessorAuthorizationCode + "', '" + InvoiceID + "')", conn);
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return;
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

        private void SetKeys()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT KeyName, KeyValue FROM CMS_SettingsKey ​WHERE siteid=21 and keycategoryid=775";
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
                                //_userLoginName = reader[1].ToString(); //Database
                                _userLoginName = "6AbXVr396sNg"; //QA
                            }
                            if (reader[0].ToString() == "transactionKey")
                            {
                                //NOTE: TESTING
                                //_transactionKey = reader[1].ToString(); //Database
                                _transactionKey = "4MB6X777sv43Mz6F"; //QA
                            }
                            if (reader[0].ToString() == "EnableACH")
                            {
                                //_enableAch = Convert.ToBoolean(reader[1].ToString());
                                _enableAch = Convert.ToBoolean(0);
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
    }
}