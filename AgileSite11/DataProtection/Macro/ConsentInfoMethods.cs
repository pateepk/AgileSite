using System;

using CMS;
using CMS.DataProtection;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(ConsentInfoMethods), typeof(ConsentInfo))]

namespace CMS.DataProtection
{
    internal class ConsentInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns localized text of the consent. Texts themselves are stored in the <see cref="ConsentText"/> object.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver.</param>
        /// <param name="parameters">Method parameters.</param>
        [MacroMethod(typeof(ConsentText), "Returns localized short and full text of the consent. Returns the text in the current content culture if not specified.", 0)]
        [MacroMethodParam(1, "cultureCode", typeof(string), "Culture code of the consent text.")]
        public static object GetConsentText(EvaluationContext context, params object[] parameters)
        {
            var consent = parameters[0] as ConsentInfo;
            if (consent == null)
            {
                return null;
            }

            switch (parameters.Length)
            {
                case 1:
                    return consent.GetConsentText(LocalizationContext.CurrentCulture.CultureCode);

                case 2:
                    return consent.GetConsentText(ValidationHelper.GetString(parameters[1], LocalizationContext.CurrentCulture.CultureCode));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
