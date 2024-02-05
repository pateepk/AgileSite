using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Discount customer selection enumeration.
    /// </summary>
    public enum DiscountCustomerEnum
    {
        /// <summary>
        /// All customers
        /// </summary>
        [EnumStringRepresentation("All")]
        [EnumDefaultValue]
        All = 0,


        /// <summary>
        /// Registered customers
        /// </summary>
        [EnumStringRepresentation("RegisteredCustomers")]
        RegisteredUsers = 1,


        /// <summary>
        /// Selected roles
        /// </summary>
        [EnumStringRepresentation("SelectedRoles")]
        SelectedRoles = 2
    }
}
