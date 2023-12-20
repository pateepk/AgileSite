using System;
using System.Web.UI.WebControls;

using CMS;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.Ecommerce.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

[assembly: RegisterCustomClass(nameof(MultiBuyDiscountEditExtender), typeof(MultiBuyDiscountEditExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for MultiBuy Discount new/edit page
    /// </summary>
    public class MultiBuyDiscountEditExtender : ProductCouponEditExtender
    {
        #region "Constants"

        private const string REDIRECTION_ELEMENT = "EditBuyXGetY";
        private const string COUPON_CODE_ELEMENT = "MultiBuyCouponsCodes";

        #endregion


        #region "Variables"

        private HiddenField mUsesCouponsDefaultValue;
        private bool? mUsesCouponsChecked;
        private bool mRedirectAfterNewDiscountCreated;

        #endregion


        #region "Properties"

        /// <summary>
        /// Remembers original value of uses coupon check box.
        /// </summary>
        private HiddenField UsesCouponsDefaultValue
        {
            get
            {
                return mUsesCouponsDefaultValue ?? (mUsesCouponsDefaultValue = new HiddenField { ID = "usesCouponsDefaultValue" });
            }
        }


        /// <summary>
        /// Returns original value of Uses coupons checkbox.
        /// </summary>
        private bool UsesCouponsChecked
        {
            get
            {
                if (mUsesCouponsChecked == null)
                {
                    mUsesCouponsChecked = ValidationHelper.GetBoolean(UsesCouponsDefaultValue.Value, true);
                }

                return mUsesCouponsChecked.Value;
            }
        }


        /// <summary>
        /// Returns current value of uses coupons check box.
        /// </summary>
        private bool UsesCouponsCheckedByUser
        {
            get
            {
                return ValidationHelper.GetBoolean(Control.FieldControls["MultiBuyDiscountUsesCoupons"].Value, false);
            }
        }


        /// <summary>
        /// Indicates whether status of "discount uses coupons" field was changed from disabled to enabled.
        /// </summary>
        private bool RedirectionEnabled
        {
            get
            {
                return (!UsesCouponsChecked && UsesCouponsCheckedByUser);
            }
        }


        /// <summary>
        /// Indicates whether status of "discount uses coupons" field was changed from enabled to disabled.
        /// </summary>
        private bool CouponCodesUnchecked
        {
            get
            {
                return (UsesCouponsChecked && !UsesCouponsCheckedByUser);
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            // Apply the common handlers shared with product coupons
            base.OnInit();

            if (Control != null)
            {
                Control.Page.Load += Page_Load;
                Control.OnBeforeSave += Control_OnBeforeSave;
                Control.OnAfterSave += Control_OnAfterSave;
                Control.OnBeforeRedirect += Control_OnBeforeRedirect;
            }
        }


        /// <summary>
        /// Set up dummy form fields.
        /// </summary>
        private void Page_Load(object sender, EventArgs e)
        {
            if (Discount == null)
            {
                return;
            }

            if (!RequestHelper.IsPostBack())
            {
                Control.FieldControls["GetSpecificOrCheapestUnit"].Value = (Discount.MultiBuyDiscountApplyToSKUID > 0) ? "specificUnit" : "cheapestUnit";
            }

            // Remember if discount uses coupons (remember value stored in the database);
            if (String.IsNullOrEmpty(UsesCouponsDefaultValue.Value))
            {
                // Insert value to the form to remember original checkbox value
                UsesCouponsDefaultValue.Value = ValidationHelper.GetBoolean(Control.Data.GetValue("MultiBuyDiscountUsesCoupons"), false).ToString().ToLowerInvariant();
                var pnlHidden = new Panel
                {
                    ID = "pnlHidden",
                    CssClass = "discountUsesCouponsValue"
                };
                pnlHidden.Controls.Add(UsesCouponsDefaultValue);
                pnlHidden.Attributes["style"] = "display: none";

                Control.Page.Form.Controls.Add(pnlHidden);
            }

            // Register script hiding redirection message
            ScriptHelper.RegisterModule(Control, "CMS.Ecommerce/Discounts");
        }


        /// <summary>
        /// Clears MultiBuyDiscountApplyToSKUID (GET Y product) if discount is configured to get cheapest product from set.
        /// </summary>
        private void Control_OnBeforeSave(object sender, EventArgs e)
        {
            if (Discount == null)
            {
                return;
            }

            if (string.Equals("cheapestunit", Control.FieldControls["GetSpecificOrCheapestUnit"].Text, StringComparison.OrdinalIgnoreCase))
            {
                Discount.SetValue("MultiBuyDiscountApplyToSKUID", null);
            }
        }


        /// <summary>
        /// Sets redirection in case coupon code use has been enabled/disabled.
        /// </summary>
        private void Control_OnAfterSave(object sender, EventArgs e)
        {
            if (Discount == null)
            {
                return;
            }

            // Redirect to coupon codes generation
            if (RedirectionEnabled)
            {
                RedirectIfDiscountIsEdited();
            }
            else if (CouponCodesUnchecked)
            {
                // Update original value
                UsesCouponsDefaultValue.Value = "false";

                // Refresh tabs if "discount uses coupons" field was unchecked and discount don´t have any coupon codes
                if (!Discount.HasCoupons)
                {
                    RedirectIfDiscountIsEdited();
                }
            }
        }


        /// <summary>
        /// Redirects to the Coupons tab after a new discount is created.
        /// The redirection needs to be performed here because OnAfterSave
        /// of this control is executed _before_ OnAfterSave of MultiObjectBinding
        /// control, so a redirect in OnAfterSave skips correct saving of binding
        /// objects.
        /// </summary>
        private void Control_OnBeforeRedirect(object sender, EventArgs e)
        {
            if (mRedirectAfterNewDiscountCreated)
            {
                URLHelper.Redirect(GenerateRedirectionUrl(REDIRECTION_ELEMENT, Discount.MultiBuyDiscountID));
            }
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Redirects to coupon codes generation if discount is edited.
        /// For new discount sets mRedirectAfterNewDiscountCreated variable
        /// to true, what causes the redirection in OnBeforRedirect.
        /// </summary>
        private void RedirectIfDiscountIsEdited()
        {
            string currentElementName = Control.UIContext.UIElement.ElementName;

            switch (currentElementName.ToLowerInvariant())
            {
                case "newbuyxgetydiscount":
                    mRedirectAfterNewDiscountCreated = true;
                    break;

                case "editbuyxgetydiscount":
                    // Parent element needs to be redirected
                    ExecuteParentWindowLocationRedirect(GenerateRedirectionUrl(REDIRECTION_ELEMENT, Discount.MultiBuyDiscountID));
                    break;
            }
        }


        /// <summary>
        /// Ensures correct redirection of parent element.
        /// </summary>
        /// <param name="redirectUrl">Url to redirect to.</param>
        private void ExecuteParentWindowLocationRedirect(string redirectUrl)
        {
            ScriptHelper.RegisterClientScriptBlock(Control.Page, typeof(string), "OrderCouponRedirect", $"parent.window.location='{redirectUrl}';", true);
        }


        /// <summary>
        /// Generate redirection url.
        /// </summary>
        /// <param name="elementName">Element where user will be redirected</param>
        /// <param name="objectId">ID of edited object</param>
        private string GenerateRedirectionUrl(string elementName, int objectId)
        {
            string url = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, elementName, false, objectId);

            if (RedirectionEnabled)
            {
                url = URLHelper.AddParameterToUrl(url, "tabname", COUPON_CODE_ELEMENT);
            }
            
            url = URLHelper.AddParameterToUrl(url, "saved", 1.ToString());

            return url;
        }

        #endregion
    }
}