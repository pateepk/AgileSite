using System;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.ContactManagement.Web.UI.Internal;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides method for resolving accounts in contact detail component.
    /// </summary>
    internal class ContactDetailsAccountResolver : IContactDetailsFieldResolver
    {
        private readonly IUILinkProvider mUILinkProvider;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactDetailsAccountResolver"/>
        /// </summary>
        /// <param name="uiLinkProvider">Provides methods for generating links to access single objects within the module. (e.g. single Site)</param>
        public ContactDetailsAccountResolver(IUILinkProvider uiLinkProvider)
        {
            mUILinkProvider = uiLinkProvider;
        }


        /// <summary>
        /// Resolves accounts detail field for given <paramref name="contact"/>. 
        /// </summary>
        /// <param name="contact">Contact the detail field is resolved for</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c></exception>
        /// <returns>Collection of <see cref="ContactDetailsAccountViewModel"/> resolved for given <paramref name="contact"/></returns>
        public object ResolveField(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            var accounts = AccountContactInfoProvider.GetRelationships()
                                                     .Source(s => s
                                                         .LeftJoin<AccountInfo>("AccountID", "AccountID")
                                                         .LeftJoin<ContactRoleInfo>("OM_AccountContact.ContactRoleID", "ContactRoleID"))
                                                     .WhereEquals("ContactID", contact.ContactID)
                                                     .Columns("AccountName", "ContactRoleDisplayName", "OM_Account.AccountID");
            
            if (DataHelper.DataSourceIsEmpty(accounts))
            {
                return null;
            }

            return accounts.Select(dataRow => new ContactDetailsAccountViewModel
            {
                Url = URLHelper.GetAbsoluteUrl(mUILinkProvider.GetSingleObjectLink(ModuleName.CONTACTMANAGEMENT, "EditAccount", new ObjectDetailLinkParameters
                {
                    AllowNavigationToListing = true,
                    ObjectIdentifier = dataRow["AccountID"].ToInteger(0)
                })),
                ContactRole = dataRow["ContactRoleDisplayName"].ToString(""),
                Text = dataRow["AccountName"].ToString("")
            }).ToList();
        }
    }
}