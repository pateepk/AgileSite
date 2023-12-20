using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides handlers for Ecommerce and Membership modules interaction.
    /// </summary>
    internal static class MembershipHandlers
    {
        /// <summary>
        /// Initializes the membership handlers.
        /// </summary>
        public static void Init()
        {
            UserInfo.TYPEINFO.Events.Update.After += UserUpdate_After;
        }

        private static void UserUpdate_After(object sender, ObjectEventArgs e)
        {
            CustomerInfoProvider.SynchronizeCustomers(e.Object as UserInfo);
        }
    }
}
