using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using WTE.Configuration;
using WTE.Helpers;

namespace PaymentProcessor
{
    /// <summary>
    /// Base for Excellalite payment page
    /// </summary>
    public class PaymentPageBase : System.Web.UI.Page
    {
        #region const

        protected const string SiteIDKey = "SiteID";
        protected const string EmailTemplateIDKey = "EmailTemplate";
        protected const string ConnectionStringKey = "connstring";
        protected const string LogPaymentKey = "LogPayment";
        protected const string SubscriptionNameKey = "SubscriptionName";
        protected const string SubscriptionDescriptionKey = "SubscriptionDescription";
        protected const string RedirectUrlKey = "RedirectUrl";
        protected const string FormActionKey = "formaction";
        protected const string SettingKeyCategoryIDKey = "SettingKeyCategoryID";
        protected const string SettingKeyPrefixKey = "SettingKeyPrefix";
        protected const string EmailFromKey = "EmailFrom";
        protected const string EmailToKey = "EmailTo";
        protected const string SendEmailKey = "SendEmail";
        protected const string DoRedirectKey = "DoRedirect";
        protected const string MaxRetryKey = "MaxRetry";
        protected const string SleepBeforeRetryMSKey = "SleepBeforeRetryMS";

        protected const string AuthNetLoginIDKey = "LoginID";
        protected const string AuthNetTransactionKeyKey = "TransactionKey";
        protected const string AuthNetEnableACHKey = "EnableACH";

        #endregion const

        #region members

        private bool repost = false;

        private string _userLoginName;
        private string _transactionKey;
        private bool _enableAch;
        private long _subscriptionId = 0;

        #endregion members

        #region properties

        /// <summary>
        /// Redirect to the current page after moving QueryString to Session
        /// </summary>
        protected virtual bool Repost
        {
            get
            {
                return repost;
            }
            set
            {
                repost = value;
            }
        }

        #region Gateway properties

        /// <summary>
        /// Login name for the payment gateway
        /// </summary>
        protected virtual string UserLoginName
        {
            get
            {
                return _userLoginName;
            }
            set
            {
                _userLoginName = value;
            }
        }

        /// <summary>
        /// Payment gateway transaction key
        /// </summary>
        protected virtual string TransactionKey
        {
            get
            {
                return _transactionKey;
            }
            set
            {
                _transactionKey = value;
            }
        }

        /// <summary>
        /// ACH enabled?
        /// </summary>
        protected virtual bool EnableACH
        {
            get
            {
                return _enableAch;
            }
            set
            {
                _enableAch = value;
            }
        }

        /// <summary>
        /// The subscription ID
        /// </summary>
        protected virtual long SubscriptionID
        {
            get
            {
                return _subscriptionId;
            }
            set

            {
                _subscriptionId = value;
            }
        }

        #endregion Gateway properties

        #region settings

        /// <summary>
        /// The currently selected option
        /// </summary>
        protected virtual string SelectedOption
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Setting prefix
        /// </summary>
        protected virtual string SiteSettingPrefix
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// The site id
        /// </summary>
        protected int SiteId
        {
            get
            {
                return GetIntAppSetting(SiteIDKey, 3);
            }
        }

        /// <summary>
        /// Agile site's setting key category id
        /// </summary>
        protected int SettingKeyCategoryID
        {
            get
            {
                return GetIntAppSetting(SettingKeyCategoryIDKey, 1104);
            }
        }

        /// <summary>
        /// CMS settings key prefix.
        /// </summary>
        protected string SettingKeyPrefix
        {

            get
            {
                return GetStringAppSetting(SettingKeyPrefixKey, String.Empty);
            }
        }


        /// <summary>
        /// Email template ID
        /// </summary>
        protected int EmailTemplateID
        {
            get
            {
                return GetIntAppSetting(EmailTemplateIDKey, 1180);
            }
        }

        /// <summary>
        /// The connection string
        /// </summary>
        protected string ConnectionString
        {
            get
            {
                return GetStringAppSetting(ConnectionStringKey, "Data Source=10.100.1.109;Initial Catalog=AS11_Agile11;Integrated Security=False;User ID=sa;Password=FrodoFrodo!!;Connect Timeout=180");
            }
        }

        /// <summary>
        /// Log payment
        /// </summary>
        protected bool LogPayment
        {
            get
            {
                return GetBoolAppSetting(LogPaymentKey, true);
            }
        }

        /// <summary>
        /// The redirect url
        /// </summary>
        protected string RedirectURL
        {
            get
            {
                return GetStringAppSetting(RedirectUrlKey, SelectedOption, "/Intranet/chat-room");
            }
        }

        /// <summary>
        /// Email To
        /// </summary>
        protected string EmailTo
        {
            get
            {
                return GetStringAppSetting(EmailToKey, SelectedOption, String.Empty);
            }
        }

        /// <summary>
        /// Email From
        /// </summary>
        protected string EmailFrom
        {
            get
            {
                return GetStringAppSetting(EmailFromKey, SelectedOption, String.Empty);
            }
        }

        /// <summary>
        /// send notification email?
        /// </summary>
        protected bool SendEmail
        {
            get
            {
                return GetBoolAppSetting(SendEmailKey, SelectedOption, false);
            }
        }

        /// <summary>
        /// Do redirection after payment is completed?
        /// </summary>
        protected bool DoRedirect
        {
            get
            {
                return GetBoolAppSetting(DoRedirectKey, SelectedOption, true);
            }
        }

        /// <summary>
        /// Maximum number of retries on error
        /// </summary>
        protected int MaxRetry
        {
            get
            {
                return GetIntAppSetting(MaxRetryKey, SelectedOption, 3);
            }
        }

        /// <summary>
        /// Time to wait before retrying (in ms)
        /// </summary>
        protected int SleepBeforeRetryMS
        {
            get
            {
                return GetIntAppSetting(SleepBeforeRetryMSKey, SelectedOption, 1000);
            }
        }

        /// <summary>
        /// from action url from the web.config
        /// </summary>
        protected string _formAction
        {
            get
            {
                return GetStringAppSetting("formaction", String.Empty);
            }
        }

        #endregion settings

        #region Billing and payment options

        /// <summary>
        /// The selected value for the the subscription
        /// </summary>
        protected virtual string SelectedPaymentOption
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// billing first name
        /// </summary>
        protected virtual string BillingFirstName
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Billing last name field
        /// </summary>
        protected virtual string BillingLastName
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Billing street address
        /// </summary>
        protected virtual string BillingStreetAddress
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Billing City
        /// </summary>
        protected virtual string BillingCity
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Billing State
        /// </summary>
        protected virtual string BillingState
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Billing Postal Code
        /// </summary>
        protected virtual string BillingPostalCode
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Billing email
        /// </summary>
        protected virtual string BillingEmailAddress
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// The creditcard number
        /// </summary>
        protected virtual string CreditCardNumber
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Selected month
        /// </summary>
        protected virtual string SelectedCCExpireMonth
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Selected year
        /// </summary>
        protected virtual string SelectedCCExpireYear
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// The Credit card security code
        /// </summary>
        protected virtual string CreditCardSecurityCode
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// The selected team member
        /// </summary>
        protected virtual string SelectedTeamMember
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Is the agree to term check box checked
        /// </summary>
        protected virtual bool AgreedToTerm
        {
            get
            {
                return false;
            }
        }

        #endregion Billing and payment options



        #endregion properties

        #region methods

        #region utils

        #region settings helper

        /// <summary>
        /// Get setting key
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        private string GetSettingKey(string p_key)
        {
            return GetSettingKey(p_key, String.Empty, SiteSettingPrefix);
        }

        /// <summary>
        /// Get setting key
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <returns></returns>
        private string GetSettingKey(string p_key, string p_selectedOption)
        {
            return GetSettingKey(p_key, p_selectedOption, SiteSettingPrefix);
        }

        /// <summary>
        /// Get setting key
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_siteprefix"></param>
        /// <returns></returns>
        private string GetSettingKey(string p_key, string p_selectedOption, string p_siteprefix)
        {
            string ret = p_key;

            if (!String.IsNullOrWhiteSpace(ret))
            {
                if (!String.IsNullOrWhiteSpace(p_siteprefix))
                {
                    ret = p_siteprefix + "_" + ret;
                }

                if (!string.IsNullOrWhiteSpace(p_selectedOption))
                {
                    ret += "_" + p_selectedOption;
                }
            }
            return ret;
        }

        /// <summary>
        /// Get int app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected int GetIntAppSetting(string p_key, int p_default)
        {
            return GetIntAppSetting(p_key, String.Empty, p_default);
        }

        /// <summary>
        /// Get int app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected int GetIntAppSetting(string p_key, string p_selectedOption, int p_default)
        {
            return GetIntAppSetting(p_key, p_selectedOption, SiteSettingPrefix, p_default);
        }

        /// <summary>
        /// Get int app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_siteprefix"></param>
        /// <param name="p_default"></param>
        protected int GetIntAppSetting(string p_key, string p_selectedOption, string p_siteprefix, int p_default)
        {
            string keyFull = GetSettingKey(p_key, p_selectedOption);
            string keyNoOption = GetSettingKey(p_key, String.Empty);
            string keyDefault = p_key;

            int sitevalue = WTEConfiguration.GetIntAppSetting(keyDefault, p_default);
            int nooptionvalue = WTEConfiguration.GetIntAppSetting(keyNoOption, sitevalue);
            int ret = WTEConfiguration.GetIntAppSetting(keyFull, nooptionvalue);

            return ret;
        }

        /// <summary>
        /// Get string app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected string GetStringAppSetting(string p_key, string p_default)
        {
            return GetStringAppSetting(p_key, String.Empty, p_default);
        }

        /// <summary>
        /// Get string app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected string GetStringAppSetting(string p_key, string p_selectedOption, string p_default)
        {
            return GetStringAppSetting(p_key, p_selectedOption, SiteSettingPrefix, p_default);
        }

        /// <summary>
        /// Get string app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_siteprefix"></param>
        /// <param name="p_default"></param>
        protected string GetStringAppSetting(string p_key, string p_selectedOption, string p_siteprefix, string p_default)
        {
            string keyFull = GetSettingKey(p_key, p_selectedOption);
            string keyNoOption = GetSettingKey(p_key, String.Empty);
            string keyDefault = p_key;

            string sitevalue = WTEConfiguration.GetAppSettingString(keyDefault, p_default);
            string nooptionvalue = WTEConfiguration.GetAppSettingString(keyNoOption, sitevalue);
            string ret = WTEConfiguration.GetAppSettingString(keyFull, nooptionvalue);

            return ret;
        }

        /// <summary>
        /// Get Bool app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected bool GetBoolAppSetting(string p_key, bool p_default)
        {
            return GetBoolAppSetting(p_key, String.Empty, p_default);
        }

        /// <summary>
        /// Get Bool app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected bool GetBoolAppSetting(string p_key, string p_selectedOption, bool p_default)
        {
            return GetBoolAppSetting(p_key, p_selectedOption, SiteSettingPrefix, p_default);
        }

        /// <summary>
        /// Get Bool app setting with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_selectedOption"></param>
        /// <param name="p_siteprefix"></param>
        /// <param name="p_default"></param>
        protected bool GetBoolAppSetting(string p_key, string p_selectedOption, string p_siteprefix, bool p_default)
        {
            string keyFull = GetSettingKey(p_key, p_selectedOption);
            string keyNoOption = GetSettingKey(p_key, String.Empty);
            string keyDefault = p_key;

            bool sitevalue = WTEConfiguration.GetBoolAppSetting(keyDefault, p_default);
            bool nooptionvalue = WTEConfiguration.GetBoolAppSetting(keyNoOption, sitevalue);
            bool ret = WTEConfiguration.GetBoolAppSetting(keyFull, nooptionvalue);

            return ret;
        }

        #endregion settings helper

        #region request/session helper

        /// <summary>
        /// Move Query string to Session state
        /// </summary>
        /// <param name="name"></param>
        protected void MoveFromQStoSS(string name)
        {
            if (!string.IsNullOrEmpty(Request.QueryString[name]))
            {
                Session.Add(name, Request.QueryString[name]);
                repost = true;
            }
        }

        #endregion request/session helper

        #region control helper

        /// <summary>
        /// Set control visibility
        /// </summary>
        /// <param name="p_control"></param>
        /// <param name="p_visible"></param>
        protected void SetControlVisibility(Control p_control, bool p_visible)
        {
            if (p_control != null)
            {
                p_control.Visible = p_visible;
            }
        }

        /// <summary>
        /// Set the text of a label
        /// </summary>
        /// <param name="p_control"></param>
        /// <param name="p_text"></param>
        protected void SetControlValue(Control p_control, object p_text)
        {
            if (p_control != null)
            {
                if (p_control.GetType() == typeof(Label))
                {
                    ((Label)(p_control)).Text = WTEDataHelper.GetSafeString(p_text);
                }
                else if (p_control.GetType() == typeof(TextBox))
                {
                    ((TextBox)(p_control)).Text = WTEDataHelper.GetSafeString(p_text);
                }
                else if (p_control.GetType() == typeof(Literal))
                {
                    ((Literal)(p_control)).Text = WTEDataHelper.GetSafeString(p_text);
                }
                else if (p_control.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)(p_control)).Checked = WTEDataHelper.GetSafeBool(p_text);
                }
                else
                {
                    // not support at the moment
                }
            }
        }

        /// <summary>
        /// Get control current (selected) value as string
        /// </summary>
        /// <param name="p_control"></param>
        /// <returns></returns>
        protected Object GetControlValue(Control p_control)
        {
            object ret = String.Empty;
            if (p_control != null)
            {
                if (p_control.GetType() == typeof(TextBox))
                {
                    ret = ((TextBox)(p_control)).Text;
                }
                else if (p_control.GetType() == typeof(DropDownList))
                {
                    ret = ((DropDownList)(p_control)).SelectedValue;
                }
                else if (p_control.GetType() == typeof(CheckBox))
                {
                    ret = ((CheckBox)(p_control)).Checked;
                }
                else if (p_control.GetType() == typeof(Label))
                {
                    ret = ((Label)(p_control)).Text;
                }
                else
                {
                    // not support at the moment
                }
            }
            return ret;
        }

        /// <summary>
        /// Get control value as string
        /// </summary>
        /// <param name="p_control"></param>
        /// <returns></returns>
        protected string GetControlValueAsString(Control p_control)
        {
            return WTEDataHelper.GetSafeString(GetControlValue(p_control));
        }

        /// <summary>
        /// Get control value as integer
        /// </summary>
        /// <param name="p_control"></param>
        /// <returns></returns>
        protected int GetControlValueAsInteger(Control p_control)
        {
            return WTEDataHelper.GetSafeInt(GetControlValue(p_control));
        }

        /// <summary>
        /// Get control value as bool
        /// </summary>
        /// <param name="p_control"></param>
        /// <returns></returns>
        protected bool GetControlValueAsBool(Control p_control)
        {
            return WTEDataHelper.GetSafeBool(GetControlValue(p_control));
        }

        #endregion control helper

        #region data access
        
        /// <summary>
        /// Compare 2 string
        /// </summary>
        /// <param name="p_key1"></param>
        /// <param name="p_key2"></param>
        /// <param name="p_ignorecase"></param>
        /// <returns></returns>
        protected bool CompareKey(object p_key1, object p_key2, bool p_ignorecase = true)
        {
            bool ret = false;
            string key1 = WTEDataHelper.GetSafeString(p_key1);
            string key2 = WTEDataHelper.GetSafeString(p_key2);

            if (p_ignorecase)
            {
                key1 = key1.ToLower();
                key2 = key2.ToLower();
            }

            ret = (key1 == key2);

            return ret;
        }

        /// <summary>
        /// Get Auth.Net settings from the database
        /// </summary>
        protected virtual void SetTransactionKeys()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        string loginIDKey = SettingKeyPrefix + AuthNetLoginIDKey;
                        string transactionKeyKey = SettingKeyPrefix + AuthNetTransactionKeyKey;
                        string enableACHKey = SettingKeyPrefix + AuthNetEnableACHKey;

                        cmd.CommandText = "Proc_SW_GetAuthNetTransactionKey";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@SiteID", SqlDbType.Int, 4)).Value = SiteId;
                        cmd.Parameters.Add(new SqlParameter("@KeyCategoryID", SqlDbType.Int, 4)).Value = SettingKeyCategoryID;
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Call Read before accessing data.
                        while (reader.Read())
                        {
                            if (CompareKey(reader[0], loginIDKey))
                            {
                                UserLoginName = WTEDataHelper.GetString(reader[1]);
                            }
                            if (CompareKey(reader[0], transactionKeyKey))
                            {
                                TransactionKey = WTEDataHelper.GetString(reader[1]);
                            }
                            if (CompareKey(reader[0], enableACHKey))
                            {
                                EnableACH = WTEDataHelper.GetSafeBool(reader[1], false);
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

        /// <summary>
        /// Insert a record into the data about the subscription
        /// </summary>
        protected virtual void InsertProcessingLog()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "sp_InsertProcessed";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.VarChar, 500)).Value = BillingFirstName;
                        cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.VarChar, 500)).Value = BillingLastName;
                        cmd.Parameters.Add(new SqlParameter("@SubscriptionId", SqlDbType.Int, 4)).Value = SubscriptionID;
                        cmd.Parameters.Add(new SqlParameter("@ProcessedDate", SqlDbType.DateTime, 4)).Value = DateTime.Now;
                        cmd.Parameters.Add(new SqlParameter("@SelectedOption", SqlDbType.VarChar, 500)).Value = SelectedPaymentOption;
                        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 500)).Value = BillingEmailAddress;
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

        #endregion utils

        #endregion methods
    }
}