using System;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

[assembly: RegisterCustomClass("OrderAddressEditExtender", typeof(CMS.Ecommerce.Web.UI.OrderAddressEditExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Order address edit extender
    /// </summary>
    public class OrderAddressEditExtender : ControlExtender<UIForm>
    {
        #region "Variables"

        private OrderInfo order;

        #endregion


        #region "Properties"

        /// <summary>
        /// Edited address.
        /// </summary>
        public OrderAddressInfo Address
        {
            get
            {
                return Control.EditedObject as OrderAddressInfo;
            }
        }


        /// <summary>
        /// Child order for order address.
        /// </summary>
        public OrderInfo Order
        {
            get
            {
                if (order == null)
                {
                    if (Address == null)
                    {
                        return null;
                    }

                    // Try to find edited address order (edited address may be shipping or billing or company address)
                    order = OrderInfoProvider.GetOrderInfo(Address.AddressOrderID);
                }

                return order;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.Page.Load += Page_Load;
        }


        private void Page_Load(object sender, EventArgs e)
        {
            // Check if user has permission to edit object on current site
            if (!MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && (Order != null) && (Order.OrderSiteID != SiteContext.CurrentSiteID))
            {
                Control.EditedObject = null;
            }
        }

        #endregion
    }
}