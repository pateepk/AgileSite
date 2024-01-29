using System;
using System.Web.UI.WebControls;

using CMS;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

[assembly: RegisterCustomClass("DiscountEditExtender", typeof(CMS.Ecommerce.Web.UI.DiscountEditExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for discount tab
    /// </summary>
    public class DiscountEditExtender : ControlExtender<UIForm>
    {
        private const string ORDER_COUPON_CODE_ELEMENT = "OrderCouponCodes";
        private const string SHIPPING_COUPON_CODE_ELEMENT = "ShippingCouponCodes";

        private HiddenField mUsesCouponsDefaultValue;
        private bool? mUsesCouponsChecked;


        /// <summary>
        /// Returns edited discount info object.
        /// </summary>
        private DiscountInfo Discount
        {
            get
            {
                return Control.EditedObject as DiscountInfo;
            }
        }

        /// <summary>
        /// Remembers original value of uses coupon check box.
        /// </summary>
        private HiddenField UsesCouponsDefaultValue
        {
            get
            {
                if (mUsesCouponsDefaultValue == null)
                {
                    mUsesCouponsDefaultValue = new HiddenField { ID = "usesCouponsDefaultValue" };
                }

                return mUsesCouponsDefaultValue;
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
                return ValidationHelper.GetBoolean(Control.FieldControls["DiscountUsesCoupons"].Value, false);
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


        private bool IsShippingDiscount
        {
            get
            {
                return Discount.DiscountApplyTo == DiscountApplicationEnum.Shipping;
            }
        }


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.Page.Load += Page_Load;
            Control.OnAfterSave += Control_OnAfterSave;
        }


        private void Page_Load(object sender, EventArgs e)
        {
            // Remember if discount uses coupons (remember value stored in the database);
            if (String.IsNullOrEmpty(UsesCouponsDefaultValue.Value))
            {
                // Insert value to the form to remember original checkbox value
                UsesCouponsDefaultValue.Value = ValidationHelper.GetString(Control.Data.GetValue("DiscountUsesCoupons"), "").ToLowerInvariant();
                var pnlHidden = new Panel
                {
                    ID = "pnlHidden",
                    CssClass = "discountUsesCouponsValue"
                };
                pnlHidden.Controls.Add(UsesCouponsDefaultValue);
                pnlHidden.Attributes.Add("style", "display: none");

                Control.Page.Form.Controls.Add(pnlHidden);
            }

            // Register script that ensures showing/hiding redirection message on coupons tab
            ScriptHelper.RegisterModule(Control, "CMS.Ecommerce/Discounts");
        }


        /// <summary>
        /// Ensures redirection to Coupons tab if discount newly uses coupons.
        /// </summary>
        protected void Control_OnAfterSave(object sender, EventArgs e)
        {
            if (Discount == null)
            {
                return;
            }

            // Redirect to coupon codes generation
            if (RedirectionEnabled)
            {
                RedirectAfterSave();
            }
            else if (CouponCodesUnchecked)
            {
                // Update original value
                UsesCouponsDefaultValue.Value = "false";

                // Refresh tabs if "discount uses coupons" field was unchecked and discount don´t have any coupon codes
                if (!Discount.HasCoupons)
                {
                    RedirectAfterSave();
                }
            }
        }


        /// <summary>
        /// Redirects to coupon codes generation tab or refreshes tabs. 
        /// </summary>
        private void RedirectAfterSave()
        {
            var currentElementName = Control.UIContext.UIElement.ElementName.ToLowerInvariant();
            var objectId = Discount.DiscountID;
            var redirectionElement = IsShippingDiscount ? "editshippingdiscount" : "editorderdiscount";
            var redirectUrl = GenerateRedirectionUrl(redirectionElement, objectId, true);

            switch (currentElementName)
            {
                case "newshippingdiscount":
                case "neworderdiscount":
                    URLHelper.Redirect(redirectUrl);
                    break;

                case "generalshipping":
                case "generalorder":
                    // Parent element needs to be redirected
                    ExecuteWindowLocationRedirect(redirectUrl);

                    break;
            }
        }


        /// <summary>
        /// Ensures correct redirection of parent element.
        /// </summary>
        /// <param name="redirectUrl">Url to redirect to.</param>
        private void ExecuteWindowLocationRedirect(string redirectUrl)
        {
            ScriptHelper.RegisterClientScriptBlock(Control.Page, typeof(string), "CouponRedirect", "parent.window.location='" + redirectUrl + "';", true);
        }


        /// <summary>
        /// Generate redirection url.
        /// </summary>
        /// <param name="elementName">Element where user will be redirected</param>
        /// <param name="objectId">ID of edited object</param>
        /// <param name="saved">Show saved info message if true</param>
        /// <returns></returns>
        private string GenerateRedirectionUrl(string elementName, int objectId, bool saved)
        {
            string url = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, elementName, false);

            url = URLHelper.AddParameterToUrl(url, "objectid", objectId.ToString());
            if (RedirectionEnabled)
            {
                url = URLHelper.AddParameterToUrl(url, "tabname", IsShippingDiscount ? SHIPPING_COUPON_CODE_ELEMENT : ORDER_COUPON_CODE_ELEMENT);
            }
            if (saved)
            {
                url = URLHelper.AddParameterToUrl(url, "saved", 1.ToString());
            }

            return url;
        }
    }
}