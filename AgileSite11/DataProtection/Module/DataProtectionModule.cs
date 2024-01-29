using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataProtection;
using System.Collections.Generic;

[assembly: RegisterModule(typeof(DataProtectionModule))]

namespace CMS.DataProtection
{
    /// <summary>
    /// Represents the DataProtection module.
    /// </summary>
    public class DataProtectionModule : Module
    {
        /// <summary>
        /// Name of DataProtection module.
        /// </summary>
        public const string MODULE_NAME = "CMS.DataProtection";


        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataProtectionModule()
            : base(MODULE_NAME)
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ContactManagementEvents.ContactMerge.Execute += MergeConsentAgreements;

            MacroRuleMetadata metadata = new MacroRuleMetadata(
                "CMSContactHasAgreedWithConsent",
                new CMSContactHasAgreedWithConsentTranslator(),
                affectingActivities: new List<string>(0),
                affectingAttributes: new List<string>(0)
            );

            MacroRuleMetadataContainer.RegisterMetadata(metadata);
        }


        private void MergeConsentAgreements(object sender, CMSEventArgs<ContactMergeModel> e)
        {
            var consentAgreementMergeService = Service.Resolve<IConsentAgreementMergeService>();
            consentAgreementMergeService.Merge(e.Parameter.SourceContact.ContactID, e.Parameter.TargetContact.ContactID);
        }
    }
}
