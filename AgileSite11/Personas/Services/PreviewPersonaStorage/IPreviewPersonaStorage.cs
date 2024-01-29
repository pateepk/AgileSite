using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.Personas;

[assembly: RegisterImplementation(typeof(IPreviewPersonaStorage), typeof(PreviewPersonaSessionStorage), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas
{
    /// <summary>
    /// Defines contract for the storage for saving and retrieving personas which should be displayed in the preview mode.
    /// </summary>
    public interface IPreviewPersonaStorage
    {
        /// <summary>
        /// Sets siteID which will be then used when storing and retrieving personas. Personas on each site will be stored independently.
        /// </summary>
        /// <remarks>SiteID isn't set using constructor parameter, because our ObjectFactory does not support constructor injection</remarks>
        /// <param name="siteID">ID of the current site</param>
        void SetCurrentSiteID(int siteID);


        /// <summary>
        /// Stores persona for the preview mode. When <see cref="GetPreviewPersona"/> is called later, persona saved using this method is retrieved.
        /// </summary>
        /// <param name="persona">Persona enforced by the user. If null, user wants to see page as someone who is not assigned to any persona</param>
        void StorePreviewPersona(PersonaInfo persona);


        /// <summary>
        /// Persona enforced by the user for the preview mode. If null, user wants to see page as someone who is not assigned to any persona.
        /// </summary>
        PersonaInfo GetPreviewPersona();
    }
}