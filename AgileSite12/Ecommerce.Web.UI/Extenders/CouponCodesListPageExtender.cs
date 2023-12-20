using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("CouponCodesListPageExtender", typeof(CMS.Ecommerce.Web.UI.CouponCodesListPageExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Discount coupon list page extender
    /// </summary>
    public class CouponCodesListPageExtender : PageExtender<CMSPage>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Page.Load += Page_Load;
        }


        private void Page_Load(object sender, EventArgs e)
        {
            // Find parent discount object
            if (Page.EditedObjectParent == null)
            {
                return;
            }

            if (!RequestHelper.IsPostBack() && QueryHelper.GetBoolean("saved", false))
            {
                Page.ShowChangesSaved();
            }

            var discount = (IDiscountInfo)Page.EditedObjectParent;
            var siteName = SiteInfoProvider.GetSiteName(discount.DiscountSiteID);
            var user = MembershipContext.AuthenticatedUser;

            // Check if user is allowed to read discount
            if (!ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_READ, siteName, user))
            {
                Page.EditedObjectParent = null;
            }

            var url = URLHelper.ResolveUrl("~/CMSModules/Ecommerce/Pages/Tools/Discount/Discount_Codes_Generator.aspx");
            url = URLHelper.AddParameterToUrl(url, "discountId", discount.DiscountID.ToString());
            url = URLHelper.AddParameterToUrl(url, "discountObjectType", GetDiscountObjectType(discount));

            // Add action for coupon codes generation
            Page.AddHeaderAction(new HeaderAction
            {
                Text = ResHelper.GetString("com.discount.generatecoupons"),
                RedirectUrl = url,
                Index = 1,
                Enabled = discount.DiscountUsesCoupons && ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_MODIFY, siteName, user),
                ButtonStyle = ButtonStyle.Default
            });
        }


        private static string GetDiscountObjectType(IDiscountInfo discount)
        {
            return ((IInfo)discount).Generalized.TypeInfo.ObjectType;
        }
    }
}