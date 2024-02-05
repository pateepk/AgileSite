using System;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataProtection;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(ContactInfoMethods), typeof(ContactInfo))]

namespace CMS.DataProtection
{
    internal class ContactInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if the contact has agreed with the specified consent.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact has agreed with the specified consent.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "consentName", typeof(string), "Code name of the consent.")]
        public static object AgreedWithConsent(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return AgreedWithConsentInternal(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact has agreed with the specified consent.
        /// </summary>
        /// <param name="contact">Contact info object.</param>
        /// <param name="consentName">Code name of the consent.</param>
        /// <returns></returns>
        internal static bool AgreedWithConsentInternal(object contact, string consentName)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var consent = ConsentInfoProvider.GetConsentInfo(consentName);
            if (consent == null)
            {
                return false;
            }

            var consentAgreementService = Service.Resolve<IConsentAgreementService>();

            return consentAgreementService.IsAgreed(contactInfo, consent);
        }
    }
}
