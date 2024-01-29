using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Personas
{
    /// <summary>
    /// Stores persona enforced for the preview mode in the session.
    /// </summary>
    internal class PreviewPersonaSessionStorage : IPreviewPersonaStorage
    {
        private int mSiteId;


        /// <summary>
        /// Sets siteID which will be then used when storing and retrieving personas. Personas on each site will be stored independently.
        /// </summary>
        /// <remarks>SiteID isn't set using constructor parameter, because our ObjectFactory does not support constructor injection</remarks>
        /// <param name="siteID">ID of the current site</param>
        public void SetCurrentSiteID(int siteID)
        {
            mSiteId = siteID;
        }


        /// <summary>
        /// Stores persona for the preview mode. When <see cref="IPreviewPersonaStorage.GetPreviewPersona"/> is called later, persona saved using this method is retrieved.
        /// </summary>
        /// <param name="persona">Persona enforced by the user. If null, user wants to see page as someone who is not assigned to any persona</param>
        public void StorePreviewPersona(PersonaInfo persona)
        {
            if (mSiteId == 0)
            {
                throw new InvalidOperationException();
            }

            SessionHelper.SetValue("PreviewPersonaID|" + mSiteId, persona == null ? null : (object)persona.PersonaID);
        }


        /// <summary>
        /// Persona enforced by the user for the preview mode. If null, user wants to see page as someone who is not assigned to any persona.
        /// </summary>
        public PersonaInfo GetPreviewPersona()
        {
            if (mSiteId == 0)
            {
                throw new InvalidOperationException();
            }

            int personaID = SessionHelper.GetValue("PreviewPersonaID|" + mSiteId).ToInteger(0);

            if (personaID == 0)
            {
                return null;
            }

            var persona = PersonaInfoProvider.GetPersonaInfoById(personaID);
            
            return persona;
        }
    }
}