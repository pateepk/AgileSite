using System;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Newsletters;
using System.Linq;

[assembly: RegisterExtension(typeof(RecipientMethods), typeof(Recipient))]
namespace CMS.Newsletters
{
    /// <summary>
    /// Macro methods for <see cref="Recipient"/> class.
    /// </summary>
    internal class RecipientMethods : MacroMethodContainer
    {
        /// <summary>
        /// Indicates if the recipient is assigned to a specified persona(s).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates if recipient is assigned to one of the specified persona(s).", 2)]
        [MacroMethodParam(0, "recipient", typeof(Recipient), "Email recipient.")]
        [MacroMethodParam(1, "personaGuids", typeof(object), "Persona GUID(s) separated by semicolon.")]
        public static object IsInPersona(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length == 2)
            {
                var recipient = GetParamValue(parameters, 0, default(Recipient));
                if (recipient == null)
                {
                    return false;
                }

                if (recipient.IsFake)
                {
                    return true;
                }

                var personaGuids = GetParamValue(parameters, 1, default(string));
                if (string.IsNullOrEmpty(personaGuids))
                {
                    return true;
                }

                if (recipient.PersonaID == 0)
                {
                    return false;
                }

                var personaGuid = ProviderHelper.GetInfoById(PredefinedObjectType.PERSONA, recipient.PersonaID).Generalized.ObjectGUID;

                return personaGuids.Split(';').Select(p => ValidationHelper.GetGuid(p, Guid.Empty)).Contains(personaGuid);
            }

            throw new NotSupportedException();
        }
    }
}