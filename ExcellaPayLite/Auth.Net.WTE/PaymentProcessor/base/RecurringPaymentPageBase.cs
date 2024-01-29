using PaymentProcessor.net.authorize.apitest;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using WTE.Helpers;

namespace PaymentProcessor
{
    /// <summary>
    /// Base for The Dark pool recurring payment.
    /// </summary>
    public class RecurringPaymentPageBase : PaymentPageBase
    {
        #region members

        public static net.authorize.apitest.Service _webservice = new net.authorize.apitest.Service();

        #endregion members

        #region properties

        #region settings

        /// <summary>
        /// The currently selected option
        /// </summary>
        protected override string SelectedOption
        {
            get
            {
                return SelectedPaymentOption;
            }
        }

        /// <summary>
        /// The name of the subcription (shown in the merchant dash board)
        /// </summary>
        protected string SubscriptionName
        {
            get
            {
                return GetStringAppSetting(SubscriptionNameKey, SelectedOption, "New subscription TRAININGPT");
            }
        }

        /// <summary>
        /// The name of the subcription (shown on the receipt)
        /// </summary>
        protected string SubscriptionDescription
        {
            get
            {
                return GetStringAppSetting(SubscriptionDescriptionKey, SelectedOption, "Training Pit Membership");
            }
        }

        #endregion settings

        #region children controls

        /// <summary>
        /// First Name text box
        /// </summary>
        protected virtual TextBox FirstNameTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Last name text box
        /// </summary>
        protected virtual TextBox LastNameTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Year drop down
        /// </summary>
        protected virtual DropDownList YearDropDownList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Message label
        /// </summary>
        protected virtual Label MessageLabel
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The container
        /// </summary>
        protected virtual HtmlTable FormFieldTable
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Credit card number textbox
        /// </summary>
        protected virtual TextBox CreditCardNumberTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The month drop down
        /// </summary>
        protected virtual DropDownList MonthDropDownList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Security code text box
        /// </summary>
        protected virtual TextBox SecurityCodeTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The address text box
        /// </summary>
        protected virtual TextBox AddressTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// City text box
        /// </summary>
        protected virtual TextBox CityTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The state text box
        /// </summary>
        protected virtual TextBox StateTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// zip code
        /// </summary>
        protected virtual TextBox PostalCodeTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Email address text box
        /// </summary>
        protected virtual TextBox EmailAddressTextBox
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Team member drop down list.
        /// </summary>
        protected virtual DropDownList TeamMemberDropDown
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The payment option drop down.
        /// </summary>
        protected virtual DropDownList PaymentOptionDropdown
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Agree to payment term and condition check box.
        /// </summary>
        protected virtual CheckBox AgreeToTermCheckBox
        {
            get
            {
                return null;
            }
        }

        #endregion children controls

        #region Billing and payment options

        /// <summary>
        /// The selected value for the the subscription
        /// </summary>
        protected override string SelectedPaymentOption
        {
            get
            {
                return GetControlValueAsString(PaymentOptionDropdown);
                //if (PaymentOptionDropdown != null)
                //{
                //    return PaymentOptionDropdown.SelectedValue;
                //}
                //return String.Empty;
            }
        }

        /// <summary>
        /// billing first name
        /// </summary>
        protected override string BillingFirstName
        {
            get
            {
                return GetControlValueAsString(FirstNameTextBox);
            }
        }

        /// <summary>
        /// Billing last name field
        /// </summary>
        protected override string BillingLastName
        {
            get
            {
                return GetControlValueAsString(LastNameTextBox);
            }
        }

        /// <summary>
        /// Billing street address
        /// </summary>
        protected override string BillingStreetAddress
        {
            get
            {
                return GetControlValueAsString(AddressTextBox);
            }
        }

        /// <summary>
        /// Billing City
        /// </summary>
        protected override string BillingCity
        {
            get
            {
                return GetControlValueAsString(CityTextBox);
            }
        }

        /// <summary>
        /// Billing State
        /// </summary>
        protected override string BillingState
        {
            get
            {
                return GetControlValueAsString(StateTextBox);
            }
        }

        /// <summary>
        /// Billing Postal Code
        /// </summary>
        protected override string BillingPostalCode
        {
            get
            {
                return GetControlValueAsString(PostalCodeTextBox);
            }
        }

        /// <summary>
        /// Billing email
        /// </summary>
        protected override string BillingEmailAddress
        {
            get
            {
                return GetControlValueAsString(EmailAddressTextBox);
            }
        }

        /// <summary>
        /// The creditcard number
        /// </summary>
        protected override string CreditCardNumber
        {
            get
            {
                return GetControlValueAsString(CreditCardNumberTextBox);
            }
        }

        /// <summary>
        /// Selected month
        /// </summary>
        protected override string SelectedCCExpireMonth
        {
            get
            {
                return GetControlValueAsString(MonthDropDownList);
            }
        }

        /// <summary>
        /// Selected year
        /// </summary>
        protected override string SelectedCCExpireYear
        {
            get
            {
                return GetControlValueAsString(YearDropDownList);
            }
        }

        /// <summary>
        /// The Credit card security code
        /// </summary>
        protected override string CreditCardSecurityCode
        {
            get
            {
                return GetControlValueAsString(SecurityCodeTextBox);
            }
        }

        /// <summary>
        /// The selected team member
        /// </summary>
        protected override string SelectedTeamMember
        {
            get
            {
                return GetControlValueAsString(TeamMemberDropDown);
            }
        }

        /// <summary>
        /// Is the agree to term check box checked
        /// </summary>
        protected override bool AgreedToTerm
        {
            get
            {
                return GetControlValueAsBool(AgreeToTermCheckBox);
            }
        }

        #endregion

        #endregion properties

        #region page event

        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Before doing anything, see if there is a qs, if there is, then clear it by storing the values in sessionstate
            MoveFromQStoSS("First");
            MoveFromQStoSS("Last");

            string redirectTo = Request.Url.AbsolutePath.ToString();

            if (Repost) { Response.Redirect(redirectTo); }

            //if we make it here, we've got all of our values in sessionstate

            // start by setting the static values
            SetTransactionKeys();

            if (Session["First"] != null) { FirstNameTextBox.Text = Session["First"].ToString(); }
            if (Session["Last"] != null) { LastNameTextBox.Text = Session["Last"].ToString(); }

            if (YearDropDownList != null)
            {
                int Cyear = DateTime.Now.Year;
                for (int i = 0; i < 10; i++)
                {
                    YearDropDownList.Items.Add(new ListItem(Cyear.ToString(), Cyear.ToString()));
                    Cyear++;
                }
            }
        }

        #endregion page event

        #region general events

        /// <summary>
        /// Submit button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRun_Click(object sender, EventArgs e)
        {
            bool redirect = false;
            string genericErrorMessage = "We are unable to process your request at this time, please try again later.";
            if (ValidateFields())
            {
                bool success = false;
                int maxRetry = MaxRetry;
                int retryCount = 0;
                int waittime = SleepBeforeRetryMS;

                while (retryCount < maxRetry && !success)
                {
                    try
                    {
                        success = CreateSubscription();
                        if (success)
                        {
                            bool doRedirect = DoRedirect;
                            if (doRedirect)
                            {
                                redirect = true;
                            }
                        }
                        // there was no crash, so error message
                        success = true;
                    }
                    catch (ThreadAbortException)
                    {
                        // This happens if we redirect the page (on success)
                        // - we can ignore
                        //WTELogging.LogException(new Exception(String.Format("Error: Retrying {0}", retryCount), ex));
                        //MessageLabel.Text = genericErrorMessage;
                        //retryCount = retryCount + 1;
                        //Thread.Sleep(sleepBeforeRetryMS); // sleep for one second before trying.
                    }
                    catch (Exception ex)
                    {
                        WTELogging.LogException(new Exception(String.Format("Error: Retrying {0}", retryCount), ex));
                        MessageLabel.Text = genericErrorMessage;
                        retryCount = retryCount + 1;
                        Thread.Sleep(waittime); // sleep for one second before trying.
                    }
                }
            }

            if (redirect)
            {
                string redirectUrl = RedirectURL;
                //redirect if success
                Response.Redirect(redirectUrl);
            }
        }

        #endregion general events

        #region methods

        #region AUTH.Net

        /// <summary>
        /// Create a recurring payment subscription
        /// </summary>
        /// <returns></returns>
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
                SubscriptionID = response.subscriptionId;
                string message = "A subscription with an ID of '" + SubscriptionID.ToString() + "' was successfully created.\n";
                SetControlValue(MessageLabel, message);
                SetControlVisibility(FormFieldTable, false);
                if (LogPayment)
                {
                    InsertProcessingLog();
                }

                if (SendEmail)
                {
                    SendSubscriptionEmail();
                }
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        /// <summary>
        /// Update a recurring payment subscription
        /// </summary>
        /// <returns></returns>
        private bool UpdateSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBSubscriptionType subscription = new ARBSubscriptionType();
            PopulateSubscription(subscription, true); // Expiration date will be different.

            ARBUpdateSubscriptionResponseType response;
            response = _webservice.ARBUpdateSubscription(authentication, SubscriptionID, subscription);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                SetControlValue(MessageLabel, "The subscription was successfully updated.<br />");
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        /// <summary>
        /// Cancel a recurring paypent subscription
        /// </summary>
        /// <returns></returns>
        private bool CancelSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBCancelSubscriptionResponseType response;
            response = _webservice.ARBCancelSubscription(authentication, SubscriptionID);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                MessageLabel.Text += "The subscription was successfully cancelled.<br />";
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        /// <summary>
        /// Get the status of an existing ARB subscription
        /// </summary>
        /// <returns></returns>
        private bool GetStatusSubscription()
        {
            bool bResult = true;

            MerchantAuthenticationType authentication = PopulateMerchantAuthentication();

            ARBGetSubscriptionStatusResponseType response;
            response = _webservice.ARBGetSubscriptionStatus(authentication, SubscriptionID);

            if (response.resultCode == MessageTypeEnum.Ok)
            {
                MessageLabel.Text += "Status Text: " + response.status + "<br />";
            }
            else
            {
                bResult = false;
                WriteErrors(response);
            }

            return bResult;
        }

        /// <summary>
        /// Create an authentication object
        /// </summary>
        /// <returns></returns>
        private MerchantAuthenticationType PopulateMerchantAuthentication()
        {
            //Testing with hardcoded login and trans key
            MerchantAuthenticationType authentication = new MerchantAuthenticationType();

            SetTransactionKeys(); // for now.

            string overrideLoginName = ConfigurationManager.AppSettings["authnetLogin"];
            string overrideTransactionKey = ConfigurationManager.AppSettings["authnetTransactionKey"];

            if (String.IsNullOrWhiteSpace(overrideLoginName))
            {
                overrideLoginName = UserLoginName;
            };

            if (String.IsNullOrWhiteSpace(overrideTransactionKey))
            {
                overrideTransactionKey = TransactionKey;
            };

            authentication.name = overrideLoginName;
            authentication.transactionKey = overrideTransactionKey;

            //authentication.name = _userLoginName; //"6AbXVr396sNg" = test value
            //authentication.transactionKey = _transactionKey; // "4MB6X777sv43Mz6F" = test value

            //authentication.name = "96B3Rvj2eR";
            //authentication.transactionKey = "693FKej27ruyZ4bw";

            return authentication;
        }

        /// <summary>
        /// Create a Recurring subscription object
        /// </summary>
        /// <param name="sub"></param>
        /// <param name="bForUpdate"></param>
        private void PopulateSubscription(ARBSubscriptionType sub, bool bForUpdate)
        {
            CreditCardType creditCard = new CreditCardType();

            //sub.name = "New subscription TRAININGPT";
            sub.name = SubscriptionName;

            creditCard.cardNumber = CreditCardNumber;
            creditCard.expirationDate = SelectedCCExpireYear + "-" + SelectedCCExpireMonth;  // required format for API is YYYY-MM
            creditCard.cardCode = CreditCardSecurityCode;

            sub.payment = new PaymentType();
            sub.payment.Item = creditCard;

            sub.billTo = new NameAndAddressType();
            sub.billTo.firstName = BillingFirstName;
            sub.billTo.lastName = BillingLastName;
            sub.billTo.address = BillingStreetAddress;
            sub.billTo.city = BillingCity;
            sub.billTo.state = BillingState;
            sub.billTo.zip = BillingPostalCode;

            sub.customer = new CustomerType();
            sub.customer.email = BillingEmailAddress;

            sub.paymentSchedule = new PaymentScheduleType();

            string recurrStartDate = ConfigurationManager.AppSettings["recurrStartDate"];
            DateTime startDate = DateTime.Today;

            if (!String.IsNullOrWhiteSpace(recurrStartDate))
            {
                if (!DateTime.TryParse(recurrStartDate, out startDate))
                {
                    startDate = DateTime.Today;
                }
            }

            if (startDate < DateTime.Today)
            {
                // has to start AFTER the submission date.
                startDate = DateTime.Today;
            }

            sub.paymentSchedule.startDate = startDate;
            sub.paymentSchedule.startDateSpecified = true;

            sub.paymentSchedule.totalOccurrences = 9999;
            sub.paymentSchedule.totalOccurrencesSpecified = true;

            // add an order so we can add some notes.
            sub.order = new OrderType();

            string description = SubscriptionDescription;
            string teammember = String.Empty;
            string agreement = String.Empty;

            if (TeamMemberDropDown != null)
            {
                if (TeamMemberDropDown.SelectedValue != "0")
                {
                    teammember = String.Format("Team member: {0}", TeamMemberDropDown.SelectedItem.Text);
                }
            }

            agreement = String.Format("Agreed to Term and Condition: {0}", AgreedToTerm ? "Yes" : "No");

            if (!String.IsNullOrWhiteSpace(teammember))
            {
                if (!String.IsNullOrWhiteSpace(description))
                {
                    description += " : ";
                }
                description += teammember;
            }

            if (!String.IsNullOrWhiteSpace(agreement))
            {
                if (!String.IsNullOrWhiteSpace(description))
                {
                    if (String.IsNullOrWhiteSpace(teammember))
                    {
                        description += " : ";
                    }
                    else
                    {
                        description += ", ";
                    }
                }
                description += agreement;
            }

            sub.order.description = description;

            string selectedOption = SelectedPaymentOption;

            switch (selectedOption)
            {
                case "1": // the training pit monthly subscription (The dark pool)
                    {
                        // $99.97 monthly
                        sub.amount = 99.97M; // WTE: #21562
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        { // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;

                case "2": // the training pit quarterly subscription (The dark pool)
                    {
                        // $269.97 quarterly
                        sub.amount = 269.97M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        { // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 3,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;

                case "3": // The dark pool insight monthly subscription. (the dark pool)
                    {
                        // $37.00 monthly
                        sub.amount = 37.00M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;

                case "4": //Crypto'tunities monthly subscription (old with free trial) - (TSW)
                    {
                        // 1st Month $9.99 Trial, then $69.99 monthly
                        sub.paymentSchedule.trialOccurrences = 1;
                        sub.paymentSchedule.trialOccurrencesSpecified = true;

                        sub.trialAmount = 9.99M;
                        sub.trialAmountSpecified = true;

                        sub.amount = 69.99M;
                        sub.amountSpecified = true;

                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "5": // Remora monthly subscription
                    {
                        // Remora Membership($98 monthly)
                        sub.amount = 98.00M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "6": //Crypto'tunities monthly subscription (new) (TSW)
                    {
                        // Crypto’tunities Membership ($89.99 monthly)
                        sub.amount = 89.99M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;

                case "7": // Dark Pool Interactive App (the dark pool)
                    {
                        sub.amount = 19.99M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "8": // Remora Lite Subscription (the dark pool)
                    {
                        // Remora Lite Subscription $99 monthly
                        sub.amount = 99.00M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "9":  // Remora Lite Subscription (the dark pool) First Month Trial $49.98, then $99 monthly
                    {
                        //First Month Trial $49.98, then $99 monthly

                        sub.paymentSchedule.trialOccurrences = 1;
                        sub.paymentSchedule.trialOccurrencesSpecified = true;

                        sub.trialAmount = 49.98M;
                        sub.trialAmountSpecified = true;

                        sub.amount = 99.00M;
                        sub.amountSpecified = true;

                        //sub.paymentSchedule.totalOccurrences = 1;
                        //sub.paymentSchedule.totalOccurrencesSpecified = true;

                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "10":  // Dark Pool Insights Plus Subscription($67.00 Monthly)
                    {
                        // $67.00 monthly
                        sub.amount = 67.00M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "11": // Dark Pool Interactive App (the dark pool)
                    {
                        sub.amount = 29.99M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;

                case "12": //Crypto'tunities monthly subscription (new - $99.99) (TSW)
                    {
                        // Crypto’tunities Membership ($99.99 monthly)
                        sub.amount = 99.99M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "13": // The dark pool insight monthly subscription. 2024 (start Dec 1) (the dark pool)
                    {
                        // $57.00 monthly
                        sub.amount = 57.00M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "14":  // Dark Pool Insights Plus Subscription 2024 (start Dec 1) (the dark pool)
                    {
                        // $97.00 monthly
                        sub.amount = 97.00M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "15": // Dark Pool Interactive App 2024 (start Dec 1) (the dark pool)
                    {
                        sub.amount = 39.99M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                case "16": // Java Pit Subscription ($299.95 Monthly)
                    {
                        sub.amount = 299.95M;
                        sub.amountSpecified = true;
                        if (!bForUpdate)
                        {
                            // Interval can't be updated once a subscription is created.
                            sub.paymentSchedule.interval = new PaymentScheduleTypeInterval
                            {
                                length = 1,
                                unit = ARBSubscriptionUnitEnum.months
                            };
                        }
                    }
                    break;
                default:
                    // nothing?
                    break;
            }
        }

        /// <summary>
        /// Add error message to the page
        /// </summary>
        /// <param name="response"></param>
        private void WriteErrors(ANetApiResponseType response)
        {
            //MessageLabel.Text += "The API request failed with the following errors:<br />";
            string message = String.Empty;
            for (int i = 0; i < response.messages.Length; i++)
            {
                message += "[" + response.messages[i].code + "] " + response.messages[i].text + "<br />";
            }
            SetControlValue(MessageLabel, message);
        }

        #endregion AUTH.Net

        #region validation

        /// <summary>
        /// Validate fields on the form
        /// </summary>
        /// <returns></returns>
        private bool ValidateFields()
        {
            bool vResult = true;
            string errorMessage = GetControlValueAsString(MessageLabel);

            if (vResult && (String.IsNullOrWhiteSpace(BillingFirstName)))
            {
                errorMessage += "First name is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(BillingLastName)))
            {
                errorMessage += "Last name is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(BillingStreetAddress)))
            {
                errorMessage += "Street address is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(BillingCity)))
            {
                errorMessage += "City is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(BillingState)))
            {
                errorMessage += "State is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(BillingPostalCode)))
            {
                errorMessage += "Postal code is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(BillingEmailAddress) || BillingEmailAddress.IndexOf("@") == -1))
            {
                errorMessage += "Email address is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(CreditCardNumber)))
            {
                errorMessage += "Card number is required.";
                vResult = false;
            }

            if (vResult && (SelectedCCExpireMonth == "0"))
            {
                errorMessage += "Card expiration month is required.";
                vResult = false;
            }

            if (vResult && (SelectedCCExpireYear == "0"))
            {
                errorMessage += "Card expiration year is required.";
                vResult = false;
            }

            if (vResult && (String.IsNullOrWhiteSpace(CreditCardSecurityCode)))
            {
                errorMessage += "Security code required.";
                vResult = false;
            }

            if (vResult && (TeamMemberDropDown != null && SelectedTeamMember == "0"))
            {
                errorMessage += "Team member is required.";
                vResult = false;
            }

            if (vResult && !AgreedToTerm)
            {
                errorMessage += "You must agree to the term and condition and refund policy";
                vResult = false;
            }

            if (!vResult)
            {
                SetControlValue(MessageLabel, errorMessage);
            }

            return vResult;
        }

        #endregion validation

        #region send email

        /// <summary>
        /// Send subcription email
        /// </summary>
        private void SendSubscriptionEmail()
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

                    cmd.Parameters.Add("@EmailSubject", SqlDbType.VarChar).Value = "A New App Subscription with an ID of '" + WTEDataHelper.GetSafeString(SubscriptionID) + "' was successfully created.\n"; ;
                    string TokenReplacements = "";
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%UserCustomData%}", "AppUser");
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%firstname%}", BillingFirstName.Replace("|", ""));
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%lastname%}", BillingLastName.Replace("|", ""));
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%email%}", BillingEmailAddress.Replace("|", ""));
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%TrimSitePrefix(username)%}", UserLoginName); // this should be the user name???
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%HearAboutSW%}", "");
                    TokenReplacements = AddingTokenReplacement(TokenReplacements, "{%HearAboutSWOther%}", "");
                    cmd.Parameters.Add("@TokenReplacements", SqlDbType.VarChar).Value = TokenReplacements;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Add key and token to a string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string AddingTokenReplacement(string str, string key, string value)
        {
            if (str.Length > 0)
            {
                str += "|";
            }
            str += key + "=" + value;
            return str;
        }

        #endregion send email

        #endregion methods
    }
}