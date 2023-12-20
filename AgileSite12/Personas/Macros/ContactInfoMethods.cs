using System;
using System.Linq;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Personas;

[assembly: RegisterExtension(typeof(ContactInfoMethods), typeof(ContactInfo))]

namespace CMS.Personas
{
    /// <summary>
    /// Persona methods extending ContactInfo.
    /// </summary>
    internal class ContactInfoMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns a bool representing if the contact is assigned to a specified persona.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true, if contact is assigned to the specified persona, returns false otherwise.", 2)]
        [MacroMethodParam(0, "contact", typeof(ContactInfo), "ContactInfo.")]
        [MacroMethodParam(1, "persona", typeof(object), "Contact's presence in this Persona is checked. Can be PersonaInfo or persona Guid")]
        public static object IsInPersona(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    var contact = (ContactInfo)parameters[0];

                    if (contact == null)
                    {
                        return false;
                    }

                    // This method works with PersonaInfo and persona Guid due to the backwards compatibility
                    // Allowing only PersonaInfo is not possible, because it is not possible to obtain PersonaInfo when SiteContext is not available in macros
                    // (SiteContext is not available when activity triggers are processed for example)

                    // Try to get PersonaInfo from the parameter 
                    PersonaInfo persona = parameters[1] as PersonaInfo;
                    if (persona == null)
                    {
                        // Try to get persona Guid from the parameter 
                        Guid personaGuid = ValidationHelper.GetGuid(parameters[1], Guid.Empty);
                        persona = (PersonaInfo)ProviderHelper.GetInfoByGuid(PersonaInfo.OBJECT_TYPE, personaGuid);
                    }

                    if (persona == null)
                    {
                        return false;
                    }

                    return PersonasFactory.GetPersonaService().IsContactInPersona(contact, persona);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets the persona the contact is assigned to or null if contact is not assigned to any persona.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(PersonaInfo), "Returns the persona the contact is currently assigned to. Returns null if the contact is not assigned to a persona. Always use this method over the " +
                                          "ContactPersona property, as this method is able to correctly take into account the persona selected in the preview mode.", 1)]
        [MacroMethodParam(0, "contact", typeof(ContactInfo), "ContactInfo.")]
        public static object GetPersona(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Get contact
                    var contact = (ContactInfo)parameters[0];

                    if (contact == null)
                    {
                        return null;
                    }
                    
                    return PersonasFactory.GetPersonaService().GetPersonaForContact(contact);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}