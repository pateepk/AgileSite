using System;
using System.Collections.Generic;

using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.OnlineMarketing;
using CMS.Search;
using CMS.SiteProvider;

[assembly: RegisterModule(typeof(OnlineMarketingModule))]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the On-line Marketing module.
    /// </summary>
    internal class OnlineMarketingModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OnlineMarketingModule()
            : base(new OnlineMarketingModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            RegisterContext<ContactManagementContext>("OnlineMarketingContext");
            RegisterContext<ContactManagementContext>("ContactManagementContext");

            // Init events handlers
            OnlineMarketingHandlers.Init();
            ABHandlers.Init();
            MVTHandlers.Init();

            // Import/Export handlers
            OnlineMarketingImportSpecialActions.Init();

            // Add fields to MacroEngine
            MacroContext.GlobalResolver.AddSourceAlias("Contact", "ContactManagementContext.CurrentContact");

            // Register index type which should be handled by general index
            SearchIndexInfoProvider.GeneralIndexTypeList.Add(SearchHelper.ONLINEFORMINDEX);

            var activityLogService = Service.Resolve<IActivityLogService>();
            activityLogService.RegisterFilter(new DocumentActivityFilter());
            activityLogService.RegisterFilter(new CurrentUserEnabledTrackingFilter());
            activityLogService.RegisterFilter(new ContactProcessingCheckerActivityLogFilter(Service.Resolve<IContactProcessingChecker>()));

            if (ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
            {
                ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.PURCHASE, "{$abtest.conversion.purchase$}", true, "{$abtest.conversion.purchase.valueplaceholder$}"));
                ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.PRODUCT_PURCHASE, "{$abtest.conversion.productpurchase$}", GetSKUSelectorFormControlDefinition("productpurchase"), true, "{$abtest.conversion.productpurchase.valueplaceholder$}"));
                ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.PRODUCT_ADDED_TO_CART, "{$abtest.conversion.productaddedtocart$}", GetSKUSelectorFormControlDefinition("productaddedtocart"), true));
            }

            ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.PAGE_VISIT, "{$abtest.conversion.pagevisit$}", new ABTestFormControlDefinition("ABTestUrlInput", "{$abtest.conversion.pagevisit.formcontrolcaption$}", new Dictionary<string, object> { { "allowempty", false } }), true));
            ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.FORM_SUBMISSION, "{$abtest.conversion.formsubmission$}", new ABTestFormControlDefinition("BizFormSelector", "{$abtest.conversion.formsubmission.formcontrolcaption$}", new Dictionary<string, object> { { "allowempty", false } }), true));
            ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.NEWSLETTER_SUBSCRIPTION, "{$abtest.conversion.newslettersubscription$}", new ABTestFormControlDefinition("NewsletterSelectorSimple", "{$abtest.conversion.newslettersubscription.formcontrolcaption$}", new Dictionary<string, object> { { "EmailFeedSelection", "newsletters" } } ), true));
            ABTestConversionDefinitionRegister.Instance.Register(new ABTestConversionDefinition(ABTestConversionNames.USER_REGISTRATION, "{$abtest.conversion.userregistration$}", true));

            RegisterActivityModifiers();
        }


        private static ABTestFormControlDefinition GetSKUSelectorFormControlDefinition(string displayName)
        {
            return new ABTestFormControlDefinition("SKUSelector", $"{{$abtest.conversion.{displayName}.formcontrolcaption$}}", new Dictionary<string, object> { { "displaynodatamessage", false }, { "displayproductvariants", true } });
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("GetMVTestSiteId", GetMVTestSiteId);
            RegisterCommand("EnsureDefaultCombination", EnsureDefaultCombination);
            RegisterCommand("MoveMVTests", MoveMVTests);

            RegisterCommand("CreateRelation", CreateRelation);
            RegisterCommand("CreateNewContact", CreateNewContact);
            RegisterCommand("ContactIsMonitored", ContactIsMonitored);

            RegisterCommand("RemoveCustomer", RemoveCustomer);

            RegisterCommand("GetCurrentContactId", GetCurrentContactId);
            RegisterCommand("GetExistingContact", GetExistingContact);
            RegisterCommand("GetUserLoginContactId", GetUserLoginContactId);
        }


        /// <summary>
        /// Register classes for modifying on-line marketing related properties in activities.
        /// </summary>
        private static void RegisterActivityModifiers()
        {
            var activityLogService = Service.Resolve<IActivityLogService>();

            activityLogService.RegisterModifier(new ABTestActivityModifier(Service.Resolve<ISiteService>()));
            activityLogService.RegisterModifier(new MVTActivityModifier());
        }


        /// <summary>
        /// Move all related MVTests from one location to another
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object MoveMVTests(object[] parameters)
        {
            String newAlias = ValidationHelper.GetString(parameters[0], String.Empty);
            String oldAlias = ValidationHelper.GetString(parameters[1], String.Empty);
            int siteID = ValidationHelper.GetInteger(parameters[2], 0);

            MVTestInfoProvider.MoveMVTests(newAlias, oldAlias, siteID);

            return null;
        }


        /// <summary>
        /// Ensure that the page template has a default combination created.
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object EnsureDefaultCombination(object[] parameters)
        {
            // Get page info
            int templateId = ValidationHelper.GetInteger(parameters[0], 0);
            MVTCombinationInfoProvider.EnsureTestCombination(templateId);

            return null;
        }


        private static object GetMVTestSiteId(object[] parameters)
        {
            {
                int mvTestId = ValidationHelper.GetInteger(parameters[0], 0);
                MVTestInfo mvTestInfo = MVTestInfoProvider.GetMVTestInfo(mvTestId);
                if (mvTestInfo != null)
                {
                    return mvTestInfo.MVTestSiteID;
                }
            }
            return 0;
        }


        /// <summary>
        /// Removes customer from all contact management objects.
        /// </summary>
        private static object RemoveCustomer(object[] parameters)
        {
            int customerId = (int)parameters[0];
            var parameters1 = new QueryDataParameters();
            parameters1.Add("@ID", customerId);
            ConnectionHelper.ExecuteQuery("om.contact.removecustomer", parameters1);
            return null;
        }


        /// <summary>
        /// Returns TRUE if the given contact is being monitored.
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object ContactIsMonitored(object[] parameters)
        {
            int contactId = ValidationHelper.GetInteger(parameters[0], 0);
            if (contactId > 0)
            {
                ContactInfo ci = ContactInfoProvider.GetContactInfo(contactId);
                return ci.ContactMonitored;
            }
            return false;
        }


        /// <summary>
        /// Creates new contact for related object given in <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object CreateNewContact(object[] parameters)
        {
            string firstName = GetTruncatedName((string)parameters[0]);
            string lastName = GetTruncatedName((string)parameters[1]);
            string emailAddress = (string)parameters[2];
            int relId = ValidationHelper.GetInteger(parameters[3], 0);
            MemberTypeEnum type = (MemberTypeEnum)ValidationHelper.GetInteger(parameters[4], 0);

            ContactInfo contact = new ContactInfo
            {
                ContactFirstName = firstName,
                ContactLastName = lastName,
                ContactEmail = emailAddress,
                ContactMonitored = true
            };

            ContactInfoProvider.SetContactInfo(contact);

            Service.Resolve<IContactRelationAssigner>().Assign(relId, type, contact.ContactID);
            return contact.ContactID;
        }


        /// <summary>
        /// Creates the contact relation
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object CreateRelation(object[] parameters)
        {
            int relatedId = (int)parameters[0];
            MemberTypeEnum memberType = (MemberTypeEnum)(int)parameters[1];

            int contactId = (int)parameters[2];
            Service.Resolve<IContactRelationAssigner>().Assign(relatedId, memberType, contactId);

            return null;
        }


        /// <summary>
        /// Returns current contact ID
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object GetCurrentContactId(object[] parameters)
        {
            ContactInfo cinf = ContactManagementContext.CurrentContact;
            if ((cinf != null) && cinf.ContactMonitored)
            {
                return cinf.ContactID;
            }
            return 0;
        }


        /// <summary>
        /// Returns current contact if such exists.
        /// </summary>
        /// <param name="parameters">Parameters array.</param>
        /// <returns>Current contact</returns>
        private static object GetExistingContact(object[] parameters)
        {
            string siteName = SiteContext.CurrentSiteName;

            if (LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement) &&
                ResourceSiteInfoProvider.IsResourceOnSite("CMS.ContactManagement", siteName)
                && SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableOnlineMarketing"))
            {
                return ContactManagementContext.GetCurrentContact(false);
            }

            return null;
        }


        /// <summary>
        /// Returns contact ID for specified user
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object GetUserLoginContactId(object[] parameters)
        {
            UserInfo ui = (UserInfo)parameters[0];
            ContactInfo ci = ContactManagementContext.UpdateUserLoginContact(ui.UserName);
            if ((ci != null) && ci.ContactMonitored)
            {
                return ci.ContactID;
            }
            return 0;
        }


        /// <summary>
        /// Returns name trimmed to 100 chars. If the string is shorter, returns the same string.
        /// </summary>
        private static string GetTruncatedName(string name)
        {
            return name.Length > 100 ? name.Substring(0, 100) : name;
        }
    }
}