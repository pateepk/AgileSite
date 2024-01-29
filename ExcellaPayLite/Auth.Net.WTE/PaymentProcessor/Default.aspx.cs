using AuthorizeNet;
using System;

namespace PaymentProcessor
{
    /// <summary>
    /// Sample page for ASC Fortifiber payment (this doesn't actually work)
    /// </summary>
    public partial class DefaultPage : System.Web.UI.Page
    {
        #region constructor

        public DefaultPage()
        {
            Load += Page_Load;
        }

        #endregion constructor

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

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // start by setting the static values
            string loginID = _userLoginName;
            string transactionKey = _transactionKey;
            string formAction = _formAction;
            decimal amount = 1;

            string description = "Sample Transaction";
            string label = "Submit Payment";
            string x_cust_id = "0000";
            string testMode = "false";

            // If an amount or description were posted to this page, the defaults are overidden
            if (!string.IsNullOrEmpty(Request.Form["amount"]))
            {
                amount = Convert.ToDecimal(Request.Form["amount"]);
            }
            if (!string.IsNullOrEmpty(Request.Form["description"]))
            {
                description = Request.Form["description"];
            }

            if (!string.IsNullOrEmpty(Request.Form["CID"]))
            {
                x_cust_id = Request.Form["CID"];
            }

            // also check to see if the amount or description were sent using the GET method
            if (!string.IsNullOrEmpty(Request.QueryString["amount"]))
            {
                amount = Convert.ToDecimal(Request.QueryString["amount"]);
            }
            if (!string.IsNullOrEmpty(Request.QueryString["description"]))
            {
                description = Request.QueryString["description"];
            }

            // an invoice is generated using the date and time
            var invoice = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (!string.IsNullOrEmpty(Request.QueryString["invoice"]))
            {
                invoice = Request.QueryString["invoice"];
            }

            string sequence = Crypto.GenerateSequence().ToString();
            string timeStamp = Crypto.GenerateTimestamp().ToString();
            var fingerPrint = Crypto.GenerateFingerprint(transactionKey, loginID, amount, sequence, timeStamp);

            amountSpan.InnerHtml = amount.ToString();
            descriptionSpan.InnerHtml = description;

            x_login.Value = loginID;
            x_amount.Value = amount.ToString();
            x_description.Value = description;
            buttonLabel.Value = label;
            x_test_request.Value = testMode;
            x_invoice_num.Value = invoice;
            x_fp_sequence.Value = sequence;
            x_fp_timestamp.Value = timeStamp;
            x_fp_hash.Value = fingerPrint;
            simForm.Action = formAction;
        }
    }
}