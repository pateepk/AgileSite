using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(ConsentAgreementComponent.IDENTIFIER, typeof(ConsentAgreementComponent), "{$kentico.formbuilder.component.consentagreement.name$}", Description = "{$kentico.formbuilder.component.consentagreement.description$}", IconClass = "icon-cb-check-preview", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a consent agreement form component.
    /// </summary>
    [RequiresFeatures(FeatureEnum.DataProtection)]
    public class ConsentAgreementComponent : FormComponent<ConsentAgreementProperties, Guid?>
    {
        /// <summary>
        /// Represents the <see cref="ConsentAgreementComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.ConsentAgreement";


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public bool ConsentChecked
        {
            get;
            set;
        }


        /// <summary>
        /// Represents whether checkbox should be enabled.
        /// </summary>
        public bool CheckboxEnabled => IsComponentValidForUsage();


        /// <summary>
        /// Gets name of the <see cref="ConsentChecked"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(ConsentChecked);


        /// <summary>
        /// Binds biz form to component.
        /// </summary>
        public override void BindContext(FormComponentContext context)
        {
            base.BindContext(context);

            if (context is BizFormComponentContext bizFormContext)
            {
                bizFormContext.SaveBizFormItem.Before += SetConsentAgreementToItem;
            }
            else
            {
                throw new ArgumentException($"The {nameof(ConsentAgreementComponent)} can be used in context of the biz form only.", nameof(context));
            }
        }


        /// <summary>
        /// Validates whether selected <see cref="ConsentInfo"/> exists.
        /// </summary>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = base.Validate(validationContext).ToList();

            if (Properties.ConsentInfo == null)
            {
                results.Add(new ValidationResult(ResHelper.GetString("kentico.formbuilder.component.consentagreement.properties.noconsentchosen")));
            }

            return results;
        }


        /// <summary>
        /// Saves consent agreement and adds its value to <see cref="SaveBizFormItemEventArgs.FormItem"/> passed in <paramref name="eventArgs"/>.
        /// </summary>
        private void SetConsentAgreementToItem(object sender, SaveBizFormItemEventArgs eventArgs)
        {
            if (IsComponentValidForUsage())
            {
                var service = Service.Resolve<IFormConsentAgreementService>();
                var contact = ContactManagementContext.GetCurrentContact();
                var consentInfo = Properties.ConsentInfo;

                Guid mAgreementGuid;
                if (ConsentChecked)
                {
                    mAgreementGuid = service.Agree(contact, consentInfo, eventArgs.FormItem).ConsentAgreementGuid;
                }
                else
                {
                    mAgreementGuid = service.Revoke(contact, consentInfo, eventArgs.FormItem).ConsentAgreementGuid;
                }

                eventArgs.FormItem.SetValue(Name, mAgreementGuid);
            }
        }


        /// <summary>
        /// Returns null. Value of consent agreement GUID is set to the corresponding <see cref="BizFormItem"/> before the item is saved using the <see cref="BizFormItemHandler"/> event.
        /// </summary>
        public override Guid? GetValue()
        {
            return null;
        }


        /// <summary>
        /// Sets the <see cref="ConsentChecked"/> based on <paramref name="value"/>.
        /// </summary>
        public override void SetValue(Guid? value)
        {
            var agreementGuid = value.HasValue ? value.Value : Guid.Empty;
            if (agreementGuid == Guid.Empty)
            {
                ConsentChecked = false;

                return;
            }

            var consentNotGiven = ConsentAgreementInfoProvider.GetConsentAgreements().WithGuid(agreementGuid).Column("ConsentAgreementRevoked").GetScalarResult(true);
            ConsentChecked = !consentNotGiven;
        }


        private bool IsComponentValidForUsage()
        {
            var isRenderedOnLiveSite = !VirtualContext.IsInitialized;
            var isConsentInfoPresent = Properties.ConsentInfo != null;

            return isRenderedOnLiveSite && isConsentInfoPresent;
        }
    }
}
