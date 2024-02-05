using System;
using System.Globalization;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;

using AngleSharp.Parser.Html;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactProfileController"/>.
    /// </summary>
    internal class ContactProfileControllerService : IContactProfileControllerService
    {
        private readonly IDateTimeNowService mDateTimeNowService;
        internal ILicenseService mLicenseService;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactProfileControllerService"/>.
        /// </summary>
        /// <param name="dateTimeNowService">Service for obtaining current date time</param>
        public ContactProfileControllerService(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
            mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton();
        }


        /// <summary>
        /// Gets instance of <see cref="ContactProfileViewModel"/> for the given <paramref name="contactID"/>. Returns <c>null</c> if no <see cref="ContactInfo"/> is found for given <paramref name="contactID"/>.
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactProfileViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactProfileViewModel"/> for the given <paramref name="contactID"/>, or <c>null</c> is no <see cref="ContactInfo"/> is found</returns>
        public ContactProfileViewModel GetContactViewModel(int contactID)
        {
            var contact = ContactInfoProvider.GetContactInfo(contactID);
            if (contact == null)
            {
                return null;
            }

            bool isEms = mLicenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement);
            var userRelations = ContactMembershipInfoProvider.GetRelationships(contactID, MemberTypeEnum.CmsUser);
            var customerRelations = ContactMembershipInfoProvider.GetRelationships(contactID, MemberTypeEnum.EcommerceCustomer);

            var contactName = GetContactNameForSimpleProfile(contact);

            var resultViewModel = new ContactProfileViewModel
            {
                ContactID = contactID,
                ContactName = contactName,
                ContactEmail = contact.ContactEmail,
                ContactType = ContactTypeEnum.Simple,
                EditUrl = GetEditUrl(contactID)
            };

            if (isEms)
            {
                resultViewModel.ContactType = ContactTypeEnum.Full;
                resultViewModel.ContactName = GetContactNameForFullProfile(contact);
                resultViewModel.ContactAddress = GetContactAddress(contact);
                resultViewModel.ContactAge = GetAge(contact.ContactBirthday);
                resultViewModel.ContactGender = (UserGenderEnum)contact.ContactGender;
                resultViewModel.ContactNotes = GetContactNotesWithLinksTargetingNewWindow(contact);
                resultViewModel.IsUser = userRelations.Count > 0;
                resultViewModel.IsCustomer = customerRelations.Count > 0;
            }

            return resultViewModel;
        }


        private string GetEditUrl(int contactId)
        {
            var fullUrl = URLHelper.GetAbsoluteUrl("~/CMSModules/ContactManagement/Pages/Tools/Contact/Tab_General.aspx");
            return URLHelper.AddParameterToUrl(fullUrl, "ContactID", contactId.ToString(CultureInfo.InvariantCulture));
        }


        private string GetContactNotesWithLinksTargetingNewWindow(ContactInfo contact)
        {
            string resolvedNotes = HTMLHelper.ResolveUrls(contact.ContactNotes, SystemContext.ApplicationPath);

            var parser = new HtmlParser();
            var doc = parser.Parse(resolvedNotes);

            var anchors = doc.All
                .Where(element => element.LocalName.Equals("a", StringComparison.OrdinalIgnoreCase))
                .Where(attr => (!attr.Attributes["href"]?.Value?.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ?? true) && String.IsNullOrEmpty(attr.Attributes["target"]?.Value));
            
            foreach (var anchor in anchors)
            {
                anchor.SetAttribute("target", "_blank");
            }

            return doc.Body.InnerHtml;
        }


        private static string GetContactNameForSimpleProfile(ContactInfo contact)
        {
            var name = new []
            {
                contact.ContactFirstName,  contact.ContactLastName
            };

            return GetContactName(name);
        }


        private string GetContactNameForFullProfile(ContactInfo contact)
        {
            var name = new[]
            {
                contact.ContactFirstName, contact.ContactMiddleName,  contact.ContactLastName
            };
            return GetContactName(name);
        }


        private static string GetContactName(string[] nameParts)
        {
            return string.Join(" ", nameParts.Where(x => x != string.Empty));
        }


        private int? GetAge(DateTime birthday)
        {
            if (birthday == DateTime.MinValue)
            {
                return null;
            }

            var reference = mDateTimeNowService.GetDateTimeNow();
            int age = reference.Year - birthday.Year;
            if (birthday > reference.AddYears(-age))
            {
                age--;
            }

            return age;
        }


        private string GetContactAddress(ContactInfo contact)
        {
            var address = new []
            {
                contact.ContactCity, GetStateName(contact), GetCountryName(contact)
            };

            return string.Join(", ", address.Where(x => x != string.Empty));
        }


        private string GetStateName(ContactInfo contact)
        {
            var state = string.Empty;
            if (contact.ContactStateID > 0)
            {
                var stateInfo = StateInfoProvider.GetStateInfo(contact.ContactStateID);
                state = stateInfo.StateDisplayName;
            }
            return state;
        }


        private string GetCountryName(ContactInfo contact)
        {
            var country = string.Empty;
            if (contact.ContactCountryID > 0)
            {
                var countryInfo = CountryInfoProvider.GetCountryInfo(contact.ContactCountryID);
                country = countryInfo.CountryDisplayName;
            }
            return country;
        }
    }
}