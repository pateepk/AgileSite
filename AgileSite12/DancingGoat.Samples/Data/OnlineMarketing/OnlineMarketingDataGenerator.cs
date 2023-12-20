using System;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample contacts with activities data.
    /// </summary>
    internal class OnlineMarketingDataGenerator
    {
        private const int NUMBER_OF_ANONYMOUS_CONTACTS = 5;
        private const string CEO_CONTACT_ROLE = "CEO";
        private const string OWNER_CONTACT_ROLE = "Owner";
        private const string BARISTA_CONTACT_ROLE = "Barista";
        private const string CONTACT_CAMPAIGN = "CafeSamplePromotion";
        private const string CONTACT_US_FORM_CODE_NAME = "ContactUs";
        private const string TRY_FREE_SAMPLE_FORM_CODE_NAME = "TryAFreeSample";
        private const string BUSINESS_CUSTOMER_REGISTATION_FORM_CODE_NAME = "BusinessCustomerRegistration";
        private readonly SiteInfo mSiteInfo;
        private readonly PagesActivityGenerator mPagesActivityGenerator = new PagesActivityGenerator();
        private readonly FormActivityGenerator mFormActivityGenerator = new FormActivityGenerator();

        private readonly TreeNode mPartnershipDocument = DocumentHelper.GetDocuments()
                                                                       .All()
                                                                       .Culture("en-US")
                                                                       .Path("/Partnership")
                                                                       .OnCurrentSite()
                                                                       .TopN(1)
                                                                       .FirstOrDefault();

        private readonly TreeNode mHomeDocument = DocumentHelper.GetDocuments()
                                                                .All()
                                                                .Culture("en-US")
                                                                .Path("/Home")
                                                                .OnCurrentSite()
                                                                .TopN(1)
                                                                .FirstOrDefault();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site data will be generated for</param>
        public OnlineMarketingDataGenerator(SiteInfo site)
        {
            mSiteInfo = site;
        }


        /// <summary>
        /// Performs contacts and activities generation.
        /// </summary>
        public void Generate()
        {
            GenerateAnonymousContacts();
            GenerateContactsWithContactProfileData();
        }


        private void GenerateAnonymousContacts()
        {
            for (int i = 0; i < NUMBER_OF_ANONYMOUS_CONTACTS; i++)
            {
                var contactCreated = DateTime.Now.AddDays(-i)
                                             .AddHours(-i)
                                             .AddMinutes(-i)
                                             .AddSeconds(-i);
                CreateAnonymousContact(contactCreated);
            }
        }


        private void CreateAnonymousContact(DateTime contactCreated)
        {
            var contact = new ContactInfo
            {
                ContactLastName = ContactHelper.ANONYMOUS + contactCreated.ToString(ContactHelper.ANONYMOUS_CONTACT_LASTNAME_DATE_PATTERN),
                ContactMonitored = true,
                ContactCreated = contactCreated
            };

            ContactInfoProvider.SetContactInfo(contact);
            GeneratePageVisitActivity(mHomeDocument, contact);
        }


        private void GenerateContactsWithContactProfileData()
        {
            CreateContactGroup("CoffeeClubMembershipRecipients", "Coffee club membership recipients");
            var airCafeAccount = CreateAccount("Air Cafe");
            var jasperCoffeeAccount = CreateAccount("Jasper Coffee");
            var contactStatusInfo = CreateContactStatus("ProspectiveClient", "Prospective client");
            var prospectiveContactGroup = CreateContactGroup("ProspectiveClients", "Prospective clients");
            var importedContactsContactGroup = CreateContactGroup("ImportedContacts", "Imported contacts");

            var ownerContactRole = CreateContactRole(OWNER_CONTACT_ROLE);
            var ceoContactRole = CreateContactRole(CEO_CONTACT_ROLE);
            var baristaContactRole = CreateContactRole(BARISTA_CONTACT_ROLE);

            var andyUserId = UserInfoProvider.GetUserInfo("Andy").UserID;

            var monicaKing = GenerateMonicaKing(contactStatusInfo.ContactStatusID, andyUserId);
            var dustinEvans = GenerateDustinEvans(contactStatusInfo.ContactStatusID, andyUserId);
            var toddRay = GenerateToddRay(contactStatusInfo.ContactStatusID, andyUserId);
            GenerateStacyStewart();
            GenerateHaroldLarson();

            AssignCustomerToContact(monicaKing, "Monica");
            AssignCustomerToContact(dustinEvans, "Dustin");

            AddContactToContactGroup(monicaKing, prospectiveContactGroup);
            AddContactToContactGroup(dustinEvans, prospectiveContactGroup);
            AddContactToContactGroup(toddRay, prospectiveContactGroup);

            AddContactToContactGroup(monicaKing, importedContactsContactGroup);
            AddContactToContactGroup(dustinEvans, importedContactsContactGroup);
            AddContactToContactGroup(toddRay, importedContactsContactGroup);

            AssignContactToAccount(airCafeAccount.AccountID, toddRay, ownerContactRole);
            AssignContactToAccount(airCafeAccount.AccountID, monicaKing, baristaContactRole);
            AssignContactToAccount(jasperCoffeeAccount.AccountID, dustinEvans, ceoContactRole);
        }


        private void GenerateHaroldLarson()
        {
            var haroldLarson = GenerateContact("Harold", "Larson", "Harold.Larson@localhost.local", "(742)-343-5223");
            haroldLarson.ContactGender = (int)UserGenderEnum.Male;
            haroldLarson.ContactCountryID = CountryInfoProvider.GetCountryInfo("USA").CountryID;
            haroldLarson.ContactCity = "Bedford";
            haroldLarson.ContactBounces = 5;
            haroldLarson.ContactStateID = StateInfoProvider.GetStateInfo("NewHampshire").StateID;
            ContactInfoProvider.SetContactInfo(haroldLarson);

            GeneratePageVisitActivity(mPartnershipDocument, haroldLarson);
            CreateFormSubmission(mPartnershipDocument, TRY_FREE_SAMPLE_FORM_CODE_NAME, haroldLarson);
            CreateFormSubmission(mPartnershipDocument, CONTACT_US_FORM_CODE_NAME, haroldLarson);
        }


        private void GenerateStacyStewart()
        {
            var stacyStewart = GenerateContact("Stacy", "Stewart", "Stacy.Stewart@localhost.local", null);
            stacyStewart.ContactCountryID = CountryInfoProvider.GetCountryInfo("Germany").CountryID;
            stacyStewart.ContactCity = "Berlin";
            stacyStewart.ContactCampaign = CONTACT_CAMPAIGN;
            stacyStewart.ContactNotes = "Contact acquired at CoffeeExpo2015";
            ContactInfoProvider.SetContactInfo(stacyStewart);

            GeneratePageVisitActivity(mHomeDocument, stacyStewart);
            GeneratePageVisitActivity(mPartnershipDocument, stacyStewart);
            CreateFormSubmission(mPartnershipDocument, TRY_FREE_SAMPLE_FORM_CODE_NAME, stacyStewart);
        }


        private ContactInfo GenerateToddRay(int contactStatusID, int contactOwneruserId)
        {
            var toddRay = GenerateContact("Todd", "Ray", "Todd.Ray@localhost.local", "(808)-289-4459");
            toddRay.ContactBirthday = DateTime.Today.AddYears(-42);
            toddRay.ContactGender = (int)UserGenderEnum.Male;
            toddRay.ContactJobTitle = OWNER_CONTACT_ROLE;
            toddRay.ContactStatusID = contactStatusID;
            toddRay.ContactMobilePhone = "+420123456789";
            toddRay.ContactCampaign = CONTACT_CAMPAIGN;
            toddRay.ContactOwnerUserID = contactOwneruserId;

            toddRay.ContactCity = "Brno";
            toddRay.ContactAddress1 = "Benesova 13";
            toddRay.ContactZIP = "612 00";
            toddRay.ContactCompanyName = "Air Cafe";
            toddRay.ContactCountryID = CountryInfoProvider.GetCountryInfo("CzechRepublic").CountryID;
            toddRay.ContactNotes = "Should be involved in every communication with Air Cafe.";
            ContactInfoProvider.SetContactInfo(toddRay);

            GeneratePageVisitActivity(mPartnershipDocument, toddRay);
            CreateFormSubmission(mPartnershipDocument, BUSINESS_CUSTOMER_REGISTATION_FORM_CODE_NAME, toddRay);
            return toddRay;
        }


        private ContactInfo GenerateDustinEvans(int contactStatusID, int contactOwneruserId)
        {
            var dustinEvans = GenerateContact("Dustin", "Evans", "Dustin.Evans@localhost.local", "(808)-139-4639");
            dustinEvans.ContactBirthday = DateTime.Today.AddYears(-40);
            dustinEvans.ContactGender = (int)UserGenderEnum.Male;
            dustinEvans.ContactJobTitle = CEO_CONTACT_ROLE;
            dustinEvans.ContactStatusID = contactStatusID;
            dustinEvans.ContactMobilePhone = "+420123456789";
            dustinEvans.ContactCampaign = CONTACT_CAMPAIGN;
            dustinEvans.ContactOwnerUserID = contactOwneruserId;
            dustinEvans.ContactNotes = "Willing to participate in the partnership program - materials sent";

            dustinEvans.ContactCity = "South Yarra";
            dustinEvans.ContactAddress1 = "163 Commercial Road";
            dustinEvans.ContactZIP = "VIC 3141";
            dustinEvans.ContactCompanyName = "Jasper Coffee";
            dustinEvans.ContactCountryID = CountryInfoProvider.GetCountryInfo("Australia").CountryID;
            dustinEvans.ContactNotes = "Willing to participate in the partnership program - materials sent";
            ContactInfoProvider.SetContactInfo(dustinEvans);

            GeneratePageVisitActivity(mHomeDocument, dustinEvans);
            GenerateInternalSearchActivity(mHomeDocument, dustinEvans, "wholesale");
            GeneratePageVisitActivity(mPartnershipDocument, dustinEvans);
            CreateFormSubmission(mPartnershipDocument, BUSINESS_CUSTOMER_REGISTATION_FORM_CODE_NAME, dustinEvans);
            return dustinEvans;
        }


        private ContactInfo GenerateMonicaKing(int contactStatusID, int contactOwneruserId)
        {
            var monicaKing = GenerateContact("Monica", "King", "monica.king@localhost.local", "(595)-721-1648");
            monicaKing.ContactBirthday = DateTime.Today.AddYears(-35);
            monicaKing.ContactGender = (int)UserGenderEnum.Female;
            monicaKing.ContactJobTitle = BARISTA_CONTACT_ROLE;
            monicaKing.ContactStatusID = contactStatusID;
            monicaKing.ContactMobilePhone = "+420123456789";
            monicaKing.ContactCampaign = CONTACT_CAMPAIGN;
            monicaKing.ContactOwnerUserID = contactOwneruserId;

            monicaKing.ContactCity = "Brno";
            monicaKing.ContactAddress1 = "New Market 187/5";
            monicaKing.ContactZIP = "602 00";
            monicaKing.ContactCompanyName = "Air Cafe";
            monicaKing.ContactCountryID = CountryInfoProvider.GetCountryInfo("CzechRepublic").CountryID;
            monicaKing.ContactNotes = "Should be involved in every communication with Air Cafe.";
            ContactInfoProvider.SetContactInfo(monicaKing);

            GeneratePageVisitActivity(mPartnershipDocument, monicaKing);
            CreateFormSubmission(mPartnershipDocument, TRY_FREE_SAMPLE_FORM_CODE_NAME, monicaKing);
            CreateFormSubmission(mPartnershipDocument, CONTACT_US_FORM_CODE_NAME, monicaKing);
            GeneratePurchaseActivity(20, monicaKing);
            return monicaKing;
        }


        private void AssignContactToAccount(int accountAccountId, ContactInfo monicaKing, ContactRoleInfo contactRoleInfo)
        {
            var accountContact = new AccountContactInfo();
            accountContact.ContactID = monicaKing.ContactID;
            accountContact.AccountID = accountAccountId;
            accountContact.ContactRoleID = contactRoleInfo.ContactRoleID;
            AccountContactInfoProvider.SetAccountContactInfo(accountContact);
        }


        private ContactRoleInfo CreateContactRole(string contactRoleCodeName)
        {
            var contactRoleInfo = ContactRoleInfoProvider.GetContactRoleInfo(contactRoleCodeName);
            if (contactRoleInfo != null)
            {
                ContactRoleInfoProvider.DeleteContactRoleInfo(contactRoleInfo);
            }

            contactRoleInfo = new ContactRoleInfo();
            contactRoleInfo.ContactRoleDescription = contactRoleCodeName;
            contactRoleInfo.ContactRoleDisplayName = contactRoleCodeName;
            contactRoleInfo.ContactRoleName = contactRoleCodeName;
            ContactRoleInfoProvider.SetContactRoleInfo(contactRoleInfo);
            return contactRoleInfo;
        }


        private ContactStatusInfo CreateContactStatus(string contactStatusCodeName, string contactStatusDisplayName)
        {
            var contactStatusInfo = ContactStatusInfoProvider.GetContactStatusInfo(contactStatusCodeName);
            if (contactStatusInfo != null)
            {
                ContactStatusInfoProvider.DeleteContactStatusInfo(contactStatusInfo);
            }

            contactStatusInfo = new ContactStatusInfo();
            contactStatusInfo.ContactStatusDescription = contactStatusDisplayName;
            contactStatusInfo.ContactStatusDisplayName = contactStatusDisplayName;
            contactStatusInfo.ContactStatusName = contactStatusCodeName;
            ContactStatusInfoProvider.SetContactStatusInfo(contactStatusInfo);
            return contactStatusInfo;
        }


        private ContactInfo GenerateContact(string firstName, string lastName, string email, string businessPhone)
        {
            var contact = new ContactInfo
            {
                ContactFirstName = firstName,
                ContactLastName = lastName,
                ContactEmail = email,
                ContactBusinessPhone = businessPhone,
                ContactMonitored = true
            };
            ContactInfoProvider.SetContactInfo(contact);
            return contact;
        }


        private void GenerateInternalSearchActivity(TreeNode homeDocument, ContactInfo contact, string searchKeyword)
        {
            mPagesActivityGenerator.GenerateInternalSearchActivity(searchKeyword, homeDocument, contact.ContactID, mSiteInfo.SiteID);
        }


        private void CreateFormSubmission(ITreeNode document, string formName, ContactInfo contact)
        {
            var form = BizFormInfoProvider.GetBizFormInfo(formName, mSiteInfo.SiteID);
            var classInfo = DataClassInfoProvider.GetDataClassInfo(form.FormClassID);
            var formItem = new BizFormItem(classInfo.ClassName);

            mFormActivityGenerator.GenerateFormSubmitActivity(formItem, document, contact.ContactID, mSiteInfo.SiteID);

            CopyDataFromContactToForm(contact, classInfo, formItem);
            SetFormSpecificData(formName, contact, formItem);
            formItem.Insert();
        }


        private void CopyDataFromContactToForm(ContactInfo contact, DataClassInfo classInfo, BizFormItem formItem)
        {
            var mapInfo = new FormInfo(classInfo.ClassContactMapping);
            var fields = mapInfo.GetFields(true, true);
            foreach (FormFieldInfo ffi in fields)
            {
                formItem.SetValue(ffi.MappedToField, contact.GetStringValue(ffi.Name, string.Empty));
            }
        }


        private void SetFormSpecificData(string formName, ContactInfo contact, BizFormItem formItem)
        {
            if (formName == TRY_FREE_SAMPLE_FORM_CODE_NAME)
            {
                formItem.SetValue("Country", CountryInfoProvider.GetCountryInfo(contact.ContactCountryID).CountryThreeLetterCode);
                var state = StateInfoProvider.GetStateInfo(contact.ContactStateID);
                var stateName = state != null ? state.StateDisplayName : string.Empty;
                formItem.SetValue("State", stateName);
            }
            if (formName == CONTACT_US_FORM_CODE_NAME)
            {
                formItem.SetValue("UserMessage", "Message");
            }
            if (formName == BUSINESS_CUSTOMER_REGISTATION_FORM_CODE_NAME)
            {
                formItem.SetValue("BecomePartner", "Becoming a partner café");
            }
        }


        private void GeneratePageVisitActivity(TreeNode document, ContactInfo contact)
        {
            mPagesActivityGenerator.GeneratePageVisitActivity(document, contact.ContactID, mSiteInfo.SiteID);
        }



        private void GeneratePurchaseActivity(double spent, ContactInfo contact)
        {
            var nameBuilder = new ActivityTitleBuilder();

            var activity = new ActivityInfo
            {
                ActivityTitle = nameBuilder.CreateTitle(PredefinedActivityType.PURCHASE, "$" + spent),
                ActivityValue = spent.ToString(CultureHelper.EnglishCulture),
                ActivityItemID = 0,
                ActivityType = PredefinedActivityType.PURCHASE,
                ActivitySiteID = mSiteInfo.SiteID,
                ActivityContactID = contact.ContactID
            };

            ActivityInfoProvider.SetActivityInfo(activity);
        }


        private void AddContactToContactGroup(ContactInfo contact, ContactGroupInfo contactGroup)
        {
            var contactGroupMembership = new ContactGroupMemberInfo();
            contactGroupMembership.ContactGroupMemberContactGroupID = contactGroup.ContactGroupID;
            contactGroupMembership.ContactGroupMemberType = ContactGroupMemberTypeEnum.Contact;
            contactGroupMembership.ContactGroupMemberRelatedID = contact.ContactID;
            ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(contactGroupMembership);
        }


        private ContactGroupInfo CreateContactGroup(string contactGroupCodeName, string contactGroupName)
        {
            var contactGroup = ContactGroupInfoProvider.GetContactGroupInfo(contactGroupCodeName);
            if (contactGroup != null)
            {
                ContactGroupInfoProvider.DeleteContactGroupInfo(contactGroup);
            }

            contactGroup = new ContactGroupInfo();
            contactGroup.ContactGroupDisplayName = contactGroupName;
            contactGroup.ContactGroupName = contactGroupCodeName;
            contactGroup.ContactGroupEnabled = true;
            ContactGroupInfoProvider.SetContactGroupInfo(contactGroup);
            return contactGroup;
        }


        private AccountInfo CreateAccount(string accountName)
        {
            var account = AccountInfoProvider.GetAccounts().FirstOrDefault(acc => acc.AccountName == accountName);
            if (account != null)
            {
                AccountInfoProvider.DeleteAccountInfo(account);
            }

            account = new AccountInfo();
            account.AccountName = accountName;
            AccountInfoProvider.SetAccountInfo(account);
            return account;
        }


        private void AssignCustomerToContact(ContactInfo contact, string customerFirstName)
        {
            var customerId = new ObjectQuery(PredefinedObjectType.CUSTOMER).WhereEquals("CustomerFirstName", customerFirstName)
                                                                           .Column("CustomerID")
                                                                           .GetScalarResult(0);
            if (customerId != 0)
            {
                var customerRelationship = new ContactMembershipInfo();
                customerRelationship.ContactID = contact.ContactID;
                customerRelationship.MemberType = MemberTypeEnum.EcommerceCustomer;
                customerRelationship.RelatedID = customerId;
                ContactMembershipInfoProvider.SetMembershipInfo(customerRelationship);
            }
        }
    }
}