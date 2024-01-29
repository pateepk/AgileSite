using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    internal static class DiscountRestrictionHelper
    {
        /// <summary>
        /// Returns <c>true</c> if the user meets discount restrictions.
        /// </summary>
        /// <param name="restriction">Type of restriction</param>
        /// <param name="discountRoles">Role names delimited by ';' character.</param>
        /// <param name="user">User to check</param>
        /// <param name="siteIdentifier">ID</param>
        public static bool CheckCustomerRestriction(DiscountCustomerEnum restriction, string discountRoles, UserInfo user, SiteInfoIdentifier siteIdentifier)
        {
            // Discount can be used by all users
            if (restriction == DiscountCustomerEnum.All)
            {
                return true;
            }

            if (user == null)
            {
                // If user is null, discount meets Registered user condition only when customer is going to be registered after checkout
                if (restriction == DiscountCustomerEnum.RegisteredUsers)
                {
                    return ECommerceHelper.IsCustomerRegisteredAfterCheckout(siteIdentifier);
                }

                // If user is null, discount cannot meet Selected roles condition
                return false;
            }

            // User is not null, he must be registered => apply discount
            if (restriction == DiscountCustomerEnum.RegisteredUsers)
            {
                return true;
            }

            // Apply discount only if the customer/user is in at least one of the roles specified in discountRoles parameter
            if (restriction == DiscountCustomerEnum.SelectedRoles)
            {
                // Trim leading/trailing delimiter chars and ensure that roles are present
                if (discountRoles.Trim(';') != "")
                {
                    // Check the roles
                    return discountRoles
                        .Split(';')
                        .Any(role => CheckRole(role, user, siteIdentifier));
                }
            }

            return false;
        }


        private static bool CheckRole(string roleName, UserInfo user, SiteInfoIdentifier siteIdentifier)
        {
            return (!roleName.Equals(RoleName.NOTAUTHENTICATED, StringComparison.OrdinalIgnoreCase) && user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                   || user.IsInRole(roleName, siteIdentifier);
        }
    }
}