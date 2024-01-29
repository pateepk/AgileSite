using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Braintree;

namespace ssch.tools
{
    public partial class BraintreeSandBox : Form
    {
        Random rnd = new Random();

        public BraintreeSandBox()
        {
            InitializeComponent();
           
        }

        private void btnGenerateRN_Click(object sender, EventArgs e)
        {
            txtOrderID.Text =  rnd.Next(10000000, 100000000).ToString();
        }

        private void BraintreeSandBox_Load(object sender, EventArgs e)
        {
            btnGenerateRN_Click(null, null);
            txtCVV.Text = rnd.Next(100, 999).ToString();
            txtExpMonth.Text = rnd.Next(1, 12).ToString();
            txtExpYear.Text = (DateTime.Now.Year + rnd.Next(1, 5)).ToString();
            CardsFilled();
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void CardsFilled()
        {
            cmbCards.Items.Clear();
            if (chkFailTest.Checked)
            {
                cmbCards.Items.Add(new ComboboxItem() { Text = "4000111111111115 Visa", Value = "4000111111111115" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "5105105105105100 Mastercard", Value = "5105105105105100" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "378734493671000 Amex", Value = "378734493671000" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "6011000990139424 Discover", Value = "6011000990139424" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "3566002020360505 JCB", Value = "3566002020360505" });
            } else
            {
                cmbCards.Items.Add(new ComboboxItem() { Text = "4111111111111111 Visa 1", Value = "4111111111111111" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "378282246310005 Amex 1", Value = "378282246310005" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "371449635398431 Amex 2", Value = "371449635398431" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "6011111111111117 Discover", Value = "6011111111111117" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "3530111333300000 JCB", Value = "3530111333300000" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "6304000000000000 Maestro", Value = "6304000000000000" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "5555555555554444 Mastercard", Value = "5555555555554444" });
                
                cmbCards.Items.Add(new ComboboxItem() { Text = "4005519200000004 Visa 2", Value = "4005519200000004" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4111111111111111 Visa 3", Value = "4111111111111111" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4009348888881881 Visa 4", Value = "4009348888881881" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4012000033330026 Visa 5", Value = "4012000033330026" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4012000077777777 Visa 6", Value = "4012000077777777" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4012888888881881 Visa 7", Value = "4012888888881881" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4217651111111119 Visa 8", Value = "4217651111111119" });
                cmbCards.Items.Add(new ComboboxItem() { Text = "4500600000000061 Visa 9", Value = "4500600000000061" });
            }
            cmbCards.SelectedIndex = 0;
        }

        private void chkFailTest_CheckedChanged(object sender, EventArgs e)
        {
            CardsFilled();
        }

        private void btnSubmitPayment_Click(object sender, EventArgs e)
        {
            btnSubmitPayment.Enabled = false;
            txtResult.Text = "";
            BraintreeGateway Gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                PublicKey = txtPublicKey.Text.Trim(),
                PrivateKey = txtPrivateKey.Text.Trim(),
                MerchantId = txtMerchanID.Text.Trim()
            };

            // Send to Braintree
            TransactionRequest transactionRequest = new TransactionRequest
            {

                Amount = Decimal.Parse(txtAmount.Text),
                OrderId = txtOrderID.Text,
                PaymentMethodToken = txtToken.Text.Trim(),
                Customer = new CustomerRequest
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Company = txtCompany.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                },
                BillingAddress = new AddressRequest
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Company = txtCompany.Text.Trim(),
                    StreetAddress = txtStreetAddress.Text.Trim(),
                    Locality = txtCity.Text.Trim(),
                    Region = txtState.Text.Trim(),
                    PostalCode = txtZipCode.Text.Trim(),
                    CountryCodeAlpha2 = "US"
                },

                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true,

                },
                Channel = "MyRunnerWebsite"
            };
            if (transactionRequest.PaymentMethodToken.Length == 0)
            {
                transactionRequest.CreditCard = new TransactionCreditCardRequest
                {
                    Number = ((ComboboxItem) cmbCards.Items[cmbCards.SelectedIndex]).Value,
                    CVV = txtCVV.Text.Trim(),
                    ExpirationMonth = txtExpMonth.Text.Trim(),
                    ExpirationYear = txtExpYear.Text.Trim()
                };
            }
            Result<Transaction> result = Gateway.Transaction.Sale(transactionRequest);

            //Start of Braintree Logging


            if (result.IsSuccess())
            {
                Transaction transaction = result.Target;
                txtResult.Text = "TransactionID: " + transaction.Id;
            }
            else if (result.Transaction != null)
            {
                Transaction transaction = result.Transaction;
                txtResult.Text = ("  Status: " + transaction.Status) +
                ("  Code: " + transaction.ProcessorResponseCode) +
                ("  Text: " + transaction.ProcessorResponseText);
            }
            else
            {
                foreach (ValidationError error in result.Errors.DeepAll())
                {
                    txtResult.Text = ("Attribute: " + error.Attribute) +
                    ("  Code: " + error.Code) +
                    ("  Message: " + error.Message);
                }
            }

            btnSubmitPayment.Enabled = true;
        }
    }
}
