using System;
using System.Linq;
using System.Text;

using CMS.ContactManagement;
using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.Personas
{
    /// <summary>
    /// Factory to access classes of the Personas module.
    /// </summary>
    public class PersonasFactory
    {
        /// <summary>
        /// Provides services for persona.
        /// </summary>
        /// <remarks>
        /// This method returns special implementation in the preview mode (<see cref="PagePreviewPersonaService"/>). That special
        /// implementation takes into account persona which is enforced in the page preview mode when user wants to see how page 
        /// looks like for certain persona. This persona is taken into account only if caller is asking for personas for current contact.
        /// </remarks>
        /// <returns>Service for determining membership of contacts in personas</returns>
        public static IPersonaService GetPersonaService()
        {
            IPersonaService originalPersonaService = Service.Resolve<IPersonaService>();

            // If document is in the preview mode, but not through the preview link, return special implementation
            if (PortalContext.ViewMode.IsPreview() && !VirtualContext.IsPreviewLinkInitialized)
            {
                return new PagePreviewPersonaService(originalPersonaService, ContactManagementContext.CurrentContact, GetPreviewPersonaStorage());
            }

            return originalPersonaService;
        }


        /// <summary>
        /// Provides services for reevaluating contact's persona.
        /// </summary>
        /// <remarks>
        /// Reevaluation should be performed after every change of contact's score to ensure the contact belongs to the right persona.
        /// </remarks>
        /// <returns>Service for reevaluation of to which persona the contact belongs</returns>
        public static IContactPersonaEvaluator GetContactPersonaEvaluator()
        {
            return Service.Resolve<IContactPersonaEvaluator>();
        }


        /// <summary>
        /// Handles propagation of changes after persona is updated.
        /// </summary>
        /// <remarks>
        /// Changes are either propagated to underlying score or reevaluation of contact persona is executed.
        /// </remarks>
        /// <returns>Service for propagation the persona changes to underlying score or to contact persona evaluator</returns>
        internal static IPersonaChangesPropagator GetPersonaChangesPropagator()
        {
            return Service.Resolve<IPersonaChangesPropagator>();
        }


        /// <summary>
        /// Gets implementation of storage for saving and retrieving personas which should be displayed in the preview mode.
        /// </summary>
        /// <returns>Preview persona storage</returns>
        public static IPreviewPersonaStorage GetPreviewPersonaStorage()
        {
            var storage = Service.Resolve<IPreviewPersonaStorage>();

            storage.SetCurrentSiteID(SiteContext.CurrentSiteID);

            return storage;
        }


        /// <summary>
        /// Gets implementation of the service for creating persona picture URLs.
        /// </summary>
        /// <returns></returns>
        public static IPersonaPictureUrlCreator GetPersonaPictureUrlCreator()
        {
            return Service.Resolve<IPersonaPictureUrlCreator>();
        }


        /// <summary>
        /// Gets implementation of service for generating img tags of persona pictures.
        /// </summary>
        /// <returns>Persona picture img tag generator</returns>
        public static IPersonaPictureImgTagGenerator GetPersonaPictureImgTagGenerator()
        {
            return Service.Resolve<IPersonaPictureImgTagGenerator>();
        }
    }
}