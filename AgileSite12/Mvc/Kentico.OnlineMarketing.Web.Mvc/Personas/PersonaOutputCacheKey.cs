using System.Web;

using CMS.ContactManagement;

using Kentico.Web.Mvc;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Class used to generate cache item key for different personas.
    /// </summary>
    internal class PersonaOutputCacheKey : IOutputCacheKey
    {
        public string Name => "KenticoPersona";

        public string GetVaryByCustomString(HttpContextBase context, string custom)
        {
            // Gets the current contact, without creating a new anonymous contact for new visitors
            var existingContact = ContactManagementContext.GetCurrentContact(createAnonymous: false);
            var contactPersonaID = existingContact?.ContactPersonaID;
            return $"{Name}={contactPersonaID}";
        }
    }
}
