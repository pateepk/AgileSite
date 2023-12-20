using CMS.Core;
using CMS.Membership;
using CMS.DataEngine;
using CMS.ContactManagement;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Encapsulates extension methods regarding <see cref="CustomerInfo"/>.
    /// </summary>
    public static class CustomerInfoExtensions
    {
        /// <summary>
        /// Updates data of a contact who is linked to the <paramref name="customerInfo"/>.
        /// </summary>
        /// <remarks>If customer does not have linked <see cref="ContactInfo"/> then one is created for him.</remarks>
        /// <param name="customerInfo">Customer whose data are used to update a <see cref="ContactInfo"/> linked to him.</param>
        internal static void UpdateLinkedContact(this CustomerInfo customerInfo)
        {
            var mCurrentContactProvider = Service.Resolve<ICurrentContactProvider>();
            if (mCurrentContactProvider == null)
            {
                return;
            }

            var currentContact = mCurrentContactProvider.GetCurrentContact(MembershipContext.AuthenticatedUser, false);
            if (currentContact == null)
            {
                return;
            }
            mCurrentContactProvider.SetCurrentContact(currentContact);

            Service.Resolve<IContactRelationAssigner>().Assign(MemberTypeEnum.EcommerceCustomer, customerInfo, currentContact);
            ContactInfoProvider.UpdateContactFromExternalData(
                customerInfo,
                DataClassInfoProvider.GetDataClassInfo(CustomerInfo.TYPEINFO.ObjectClassName).ClassContactOverwriteEnabled,
                currentContact.ContactID);
        }
    }
}
