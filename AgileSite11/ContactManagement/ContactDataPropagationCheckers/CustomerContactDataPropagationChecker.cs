using CMS.Base;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Checks whether the propagation of cutomer data to Contact object is allowed.
    /// </summary>
    public class CustomerContactDataPropagationChecker : IContactDataPropagationChecker
    {
        /// <summary>
        /// Checks whether the propagation of cutomer data to Contact object is allowed.
        /// </summary>
        public bool IsAllowed()
        {
            return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSCustomerContactDataMergingAllowed"], true);
        }
    }
}
