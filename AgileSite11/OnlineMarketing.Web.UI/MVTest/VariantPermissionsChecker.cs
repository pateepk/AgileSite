using System;

using CMS.Membership;
using CMS.UIControls;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Contains method for checking permission for MVT or CP variants.
    /// </summary>
    public static class VariantPermissionsChecker
    {
        /// <summary>
        /// Check permissions for MVT or CP.
        /// </summary>
        /// <param name="variantType">Type of editing object (widget, webpart, zone)</param>
        public static void CheckPermissions(VariantTypeEnum variantType)
        {
            CurrentUserInfo ui = MembershipContext.AuthenticatedUser;

            // Check permissions - design is not required for widgets
            if ((ui == null) || ((variantType != VariantTypeEnum.Widget) && !ui.IsAuthorizedPerResource("CMS.Design", "Design")))
            {
                CMSPage.RedirectToAccessDenied("CMS.Design", "Design");
            }

            // Set the right permission (depends on edited object type)
            String uiPermission = "WebPartZoneProperties.Variant";
            switch (variantType)
            {
                case VariantTypeEnum.WebPart:
                    uiPermission = "WebPartProperties.Variant";
                    break;

                case VariantTypeEnum.Widget:
                    uiPermission = "WidgetProperties.Variant";
                    break;
            }

            // Check UI Permissions
            if (!ui.IsAuthorizedPerUIElement("CMS.Design", uiPermission))
            {
                CMSPage.RedirectToUIElementAccessDenied("CMS.Design", uiPermission);
            }
        }
    }
}
