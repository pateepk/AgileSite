using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PaymentProcessor
{
    /// <summary>
    /// Handles recurring subscription sign up for the dark pool (Training Pit)
    ///  https://thedarkpools.com/excellalite/DPCRecurringBilling.aspx
    /// </summary>
    public partial class DPCRecurringBilling : DPCRecurringPaymentPage
    {
        #region children controls

        /// <summary>
        /// First Name text box
        /// </summary>
        protected override TextBox FirstNameTextBox
        {
            get
            {
                return txtFirstName;
            }
        }

        /// <summary>
        /// Last name text box
        /// </summary>
        protected override TextBox LastNameTextBox
        {
            get
            {
                return txtLastName;
            }
        }

        /// <summary>
        /// Year drop down
        /// </summary>
        protected override DropDownList YearDropDownList
        {
            get
            {
                return ddlYear;
            }
        }

        /// <summary>
        /// Message label
        /// </summary>
        protected override Label MessageLabel
        {
            get
            {
                return lblMessage;
            }
        }

        /// <summary
        /// The container
        /// </summary>
        protected override HtmlTable FormFieldTable
        {
            get
            {
                return tblFormFields;
            }
        }

        /// <summary>
        /// Credit card number textbox
        /// </summary>
        protected override TextBox CreditCardNumberTextBox
        {
            get
            {
                return txtCCN;
            }
        }

        /// <summary>
        /// The month drop down
        /// </summary>
        protected override DropDownList MonthDropDownList
        {
            get
            {
                return ddlMonth;
            }
        }

        /// <summary>
        /// Security code text box
        /// </summary>
        protected override TextBox SecurityCodeTextBox
        {
            get
            {
                return txtSecurityCode;
            }
        }

        /// <summary>
        /// The address text box
        /// </summary>
        protected override TextBox AddressTextBox
        {
            get
            {
                return txtAddress;
            }
        }

        /// <summary>
        /// City text box
        /// </summary>
        protected override TextBox CityTextBox
        {
            get
            {
                return txtCity;
            }
        }

        /// <summary>
        /// The state text box
        /// </summary>
        protected override TextBox StateTextBox
        {
            get
            {
                return txtState;
            }
        }

        /// <summary>
        /// zip code
        /// </summary>
        protected override TextBox PostalCodeTextBox
        {
            get
            {
                return txtPostalCode;
            }
        }

        /// <summary>
        /// Email address text box
        /// </summary>
        protected override TextBox EmailAddressTextBox
        {
            get
            {
                return txtEmailAddress;
            }
        }

        /// <summary>
        /// Team member drop down list.
        /// </summary>
        protected override DropDownList TeamMemberDropDown
        {
            get
            {
                return ddlTeamMember;
            }
        }

        /// <summary>
        /// The payment option drop down.
        /// </summary>
        protected override DropDownList PaymentOptionDropdown
        {
            get
            {
                return ddlPaymentOption;
            }
        }

        /// <summary>
        /// Agree to payment term and condition check box.
        /// </summary>
        protected override CheckBox AgreeToTermCheckBox
        {
            get
            {
                return chkAgreeToTerm;
            }
        }

        #endregion children controls
    }
}